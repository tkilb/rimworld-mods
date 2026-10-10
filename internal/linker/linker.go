package linker

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"rimworld-mods/internal/config"
	"rimworld-mods/internal/manifest"
	"rimworld-mods/internal/order"
)

var protectedDirs = map[string]bool{
	"core":                               true,
	"royalty":                            true,
	"ideology":                           true,
	"biotech":                            true,
	"anomaly":                            true,
	"odyssey":                            true,
	"place mods here.txt":                true,
	"place official expansions here.txt": true,
}

// LinkStatus represents the result of a link/unlink action on a mod.
type LinkStatus string

const (
	StatusCreated    LinkStatus = "created"
	StatusUpdated    LinkStatus = "updated"
	StatusUpToDate   LinkStatus = "up-to-date"
	StatusSkipped    LinkStatus = "skipped"
	StatusUnlinked   LinkStatus = "unlinked"
	StatusError      LinkStatus = "error"
)

// LinkResult holds details of a mod link operation.
type LinkResult struct {
	ModID       string
	SourcePath  string
	DestPath    string
	Status      LinkStatus
	Message     string
}

// Linker manages symlinks between monorepo mods and RimWorld's active Mods directory.
type Linker struct {
	Env      *config.Environment
	Manifest *manifest.Manifest
}

// NewLinker initializes a Linker.
func NewLinker(env *config.Environment, mf *manifest.Manifest) *Linker {
	return &Linker{
		Env:      env,
		Manifest: mf,
	}
}

// IsProtected checks if a mod name matches an official DLC / Core directory or system file.
func IsProtected(name string) bool {
	return protectedDirs[strings.ToLower(strings.TrimSpace(name))]
}

// DiscoverCandidates resolves all enabled vendor and custom mods to be linked.
func (l *Linker) DiscoverCandidates() (map[string]string, error) {
	candidates := make(map[string]string)
	vendorDir := filepath.Join(l.Env.RepoRoot, "mods", "vendor")
	customDir := filepath.Join(l.Env.RepoRoot, "mods", "custom")

	disabled := make(map[string]bool)

	// 1. Process declared mods
	if l.Manifest != nil {
		for modID, entry := range l.Manifest.Mods {
			if !entry.IsEnabled() {
				disabled[modID] = true
				continue
			}

			var candidatePath string
			if entry.Source == "custom" || entry.Source == "local" {
				candidatePath = filepath.Join(customDir, modID)
			} else {
				candidatePath = filepath.Join(vendorDir, modID)
			}

			if fi, err := os.Stat(candidatePath); err == nil && fi.IsDir() {
				candidates[modID] = candidatePath
			}
		}
	}

	// 2. Auto-discover custom mods with About/About.xml in mods/custom/
	if fi, err := os.Stat(customDir); err == nil && fi.IsDir() {
		entries, _ := os.ReadDir(customDir)
		for _, e := range entries {
			if !e.IsDir() || strings.HasPrefix(e.Name(), ".") {
				continue
			}
			customID := e.Name()
			if disabled[customID] {
				continue
			}
			modPath := filepath.Join(customDir, customID)
			if _, err := order.FindAboutXML(modPath); err == nil {
				candidates[customID] = modPath
			}
		}
	}

	return candidates, nil
}

// LinkAll deploys symlinks for all candidate mods into RimWorld's Mods directory,
// purging any existing unmanaged, stale, or orphaned entries to enforce manifests as source of truth.
func (l *Linker) LinkAll(dryRun bool) ([]LinkResult, error) {
	candidates, err := l.DiscoverCandidates()
	if err != nil {
		return nil, err
	}

	modsDir := l.Env.RimWorldModsDir
	if !dryRun {
		if err := os.MkdirAll(modsDir, 0755); err != nil {
			return nil, fmt.Errorf("failed to create RimWorld Mods directory %s: %w", modsDir, err)
		}
	}

	var results []LinkResult

	// 1. Clean slate: remove existing entries in RimWorld Mods that are NOT protected and NOT candidates
	if entries, err := os.ReadDir(modsDir); err == nil {
		for _, e := range entries {
			name := e.Name()
			if IsProtected(name) {
				continue
			}

			destPath := filepath.Join(modsDir, name)

			if _, isCandidate := candidates[name]; !isCandidate {
				if dryRun {
					results = append(results, LinkResult{
						ModID:    name,
						DestPath: destPath,
						Status:   StatusUnlinked,
						Message:  "[DRY-RUN] Would remove unmanaged/stale mod",
					})
				} else {
					if err := os.RemoveAll(destPath); err != nil {
						results = append(results, LinkResult{
							ModID:    name,
							DestPath: destPath,
							Status:   StatusError,
							Message:  fmt.Sprintf("failed to remove stale entry: %v", err),
						})
					} else {
						results = append(results, LinkResult{
							ModID:    name,
							DestPath: destPath,
							Status:   StatusUnlinked,
							Message:  "Removed unmanaged/stale mod",
						})
					}
				}
			}
		}
	}

	// 2. Link all candidate mods
	for modID, srcPath := range candidates {
		res := l.linkSingle(modID, srcPath, dryRun)
		results = append(results, res)
	}

	return results, nil
}

// UnlinkAll removes all symlinks in RimWorld Mods that point into the monorepo.
func (l *Linker) UnlinkAll(dryRun bool) ([]LinkResult, error) {
	modsDir := l.Env.RimWorldModsDir
	entries, err := os.ReadDir(modsDir)
	if err != nil {
		if os.IsNotExist(err) {
			return nil, nil
		}
		return nil, err
	}

	repoVendor := filepath.Join(l.Env.RepoRoot, "mods", "vendor")
	repoCustom := filepath.Join(l.Env.RepoRoot, "mods", "custom")

	var results []LinkResult

	for _, e := range entries {
		name := e.Name()
		if IsProtected(name) {
			continue
		}

		destPath := filepath.Join(modsDir, name)
		fi, err := os.Lstat(destPath)
		if err != nil || (fi.Mode()&os.ModeSymlink) == 0 {
			continue
		}

		target, err := os.Readlink(destPath)
		if err != nil {
			continue
		}

		var absTarget string
		if filepath.IsAbs(target) {
			absTarget = filepath.Clean(target)
		} else {
			absTarget = filepath.Clean(filepath.Join(modsDir, target))
		}

		repoVendorClean := filepath.Clean(repoVendor)
		repoCustomClean := filepath.Clean(repoCustom)

		// Only remove if it targets our repository vendor or custom dir
		if strings.HasPrefix(absTarget, repoVendorClean) || strings.HasPrefix(absTarget, repoCustomClean) {
			if dryRun {
				results = append(results, LinkResult{
					ModID:      name,
					SourcePath: absTarget,
					DestPath:   destPath,
					Status:     StatusUnlinked,
					Message:    "[DRY-RUN] Would remove symlink",
				})
			} else {
				if err := os.Remove(destPath); err != nil {
					results = append(results, LinkResult{
						ModID:      name,
						DestPath:   destPath,
						Status:     StatusError,
						Message:    fmt.Sprintf("failed to remove: %v", err),
					})
				} else {
					results = append(results, LinkResult{
						ModID:      name,
						SourcePath: absTarget,
						DestPath:   destPath,
						Status:     StatusUnlinked,
						Message:    "Removed symlink",
					})
				}
			}
		}
	}

	return results, nil
}

func (l *Linker) linkSingle(modID, srcPath string, dryRun bool) LinkResult {
	if IsProtected(modID) {
		return LinkResult{
			ModID:   modID,
			Status:  StatusSkipped,
			Message: "Protected core/DLC directory",
		}
	}

	destPath := filepath.Join(l.Env.RimWorldModsDir, modID)

	// Check existing file/link at destination
	fi, err := os.Lstat(destPath)
	if err == nil {
		if (fi.Mode() & os.ModeSymlink) != 0 {
			target, err := os.Readlink(destPath)
			if err == nil {
				var absTarget string
				if filepath.IsAbs(target) {
					absTarget = filepath.Clean(target)
				} else {
					absTarget = filepath.Clean(filepath.Join(filepath.Dir(destPath), target))
				}
				absSrc, _ := filepath.Abs(srcPath)
				if absTarget == filepath.Clean(absSrc) {
					return LinkResult{
						ModID:      modID,
						SourcePath: srcPath,
						DestPath:   destPath,
						Status:     StatusUpToDate,
						Message:    "Symlink already up-to-date",
					}
				}
			}
			// Symlink exists but points elsewhere: remove it to re-link
			if !dryRun {
				_ = os.Remove(destPath)
			}
		} else {
			// Destination exists and is a real directory or file: remove to enforce symlink source of truth
			if dryRun {
				return LinkResult{
					ModID:      modID,
					SourcePath: srcPath,
					DestPath:   destPath,
					Status:     StatusUpdated,
					Message:    "[DRY-RUN] Would replace non-symlink with repository symlink",
				}
			}
			if err := os.RemoveAll(destPath); err != nil {
				return LinkResult{
					ModID:      modID,
					SourcePath: srcPath,
					DestPath:   destPath,
					Status:     StatusError,
					Message:    fmt.Sprintf("failed to remove non-symlink directory: %v", err),
				}
			}
		}
	}

	if dryRun {
		return LinkResult{
			ModID:      modID,
			SourcePath: srcPath,
			DestPath:   destPath,
			Status:     StatusCreated,
			Message:    "[DRY-RUN] Would create symlink",
		}
	}

	absSrc, err := filepath.Abs(srcPath)
	if err != nil {
		absSrc = srcPath
	}

	if err := os.Symlink(absSrc, destPath); err != nil {
		return LinkResult{
			ModID:      modID,
			SourcePath: absSrc,
			DestPath:   destPath,
			Status:     StatusError,
			Message:    err.Error(),
		}
	}

	return LinkResult{
		ModID:      modID,
		SourcePath: absSrc,
		DestPath:   destPath,
		Status:     StatusCreated,
		Message:    "Linked successfully",
	}
}
