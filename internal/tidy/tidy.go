package tidy

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"strings"

	"rimworld-mods/internal/config"
	"rimworld-mods/internal/linker"
	"rimworld-mods/internal/manifest"
)

// TidyReport records actions performed by tidy.
type TidyReport struct {
	PrunedLockEntries []string
	PrunedVendorDirs  []string
	PrunedSymlinks    []string
	GoModTidied       bool
}

// RunTidy cleans up orphaned lock records, vendor directories, dead symlinks, and runs go mod tidy.
func RunTidy(env *config.Environment, dryRun bool) (*TidyReport, error) {
	manifestPath := filepath.Join(env.RepoRoot, "manifests", "mods.yaml")
	lockPath := filepath.Join(env.RepoRoot, "manifests", "mods.lock.yaml")
	vendorDir := filepath.Join(env.RepoRoot, "mods", "vendor")

	mf, err := manifest.LoadManifest(manifestPath)
	if err != nil {
		return nil, fmt.Errorf("failed to load manifest: %w", err)
	}

	report := &TidyReport{}

	// 1. Prune orphaned lockfile entries
	lf, err := manifest.LoadLockfile(lockPath)
	if err == nil && lf != nil {
		var lockModified bool
		for modID := range lf.Mods {
			if _, exists := mf.Mods[modID]; !exists {
				report.PrunedLockEntries = append(report.PrunedLockEntries, modID)
				if !dryRun {
					delete(lf.Mods, modID)
					lockModified = true
				}
			}
		}
		if lockModified && !dryRun {
			if err := lf.Save(lockPath, env.Machine); err != nil {
				return nil, fmt.Errorf("failed to save pruned lockfile: %w", err)
			}
		}
	}

	// 2. Prune unused vendor cache folders
	if fi, err := os.Stat(vendorDir); err == nil && fi.IsDir() {
		entries, _ := os.ReadDir(vendorDir)
		for _, e := range entries {
			if !e.IsDir() || strings.HasPrefix(e.Name(), ".") {
				continue
			}
			modID := e.Name()
			if _, exists := mf.Mods[modID]; !exists {
				report.PrunedVendorDirs = append(report.PrunedVendorDirs, modID)
				if !dryRun {
					_ = os.RemoveAll(filepath.Join(vendorDir, modID))
				}
			}
		}
	}

	// 3. Prune broken or orphaned symlinks in RimWorld Mods folder
	if fi, err := os.Stat(env.RimWorldModsDir); err == nil && fi.IsDir() {
		entries, _ := os.ReadDir(env.RimWorldModsDir)
		repoVendorClean := filepath.Clean(filepath.Join(env.RepoRoot, "mods", "vendor"))
		repoCustomClean := filepath.Clean(filepath.Join(env.RepoRoot, "mods", "custom"))

		for _, e := range entries {
			name := e.Name()
			if linker.IsProtected(name) {
				continue
			}
			destPath := filepath.Join(env.RimWorldModsDir, name)
			lfi, err := os.Lstat(destPath)
			if err != nil || (lfi.Mode()&os.ModeSymlink) == 0 {
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
				absTarget = filepath.Clean(filepath.Join(env.RimWorldModsDir, target))
			}

			// Check if target points to our repo
			isRepoSymlink := strings.HasPrefix(absTarget, repoVendorClean) || strings.HasPrefix(absTarget, repoCustomClean)
			if !isRepoSymlink {
				continue
			}

			// If target doesn't exist anymore, or mod is no longer declared
			_, targetErr := os.Stat(absTarget)
			isOrphaned := targetErr != nil
			if !isOrphaned {
				_, declared := mf.Mods[name]
				if !declared {
					// Check if it's an auto-discovered custom mod
					if !strings.HasPrefix(absTarget, repoCustomClean) {
						isOrphaned = true
					}
				}
			}

			if isOrphaned {
				report.PrunedSymlinks = append(report.PrunedSymlinks, name)
				if !dryRun {
					_ = os.Remove(destPath)
				}
			}
		}
	}

	// 4. Run `go mod tidy` on root repo
	if !dryRun {
		cmd := exec.Command("go", "mod", "tidy")
		cmd.Dir = env.RepoRoot
		_ = cmd.Run()
		report.GoModTidied = true
	} else {
		report.GoModTidied = true
	}

	return report, nil
}
