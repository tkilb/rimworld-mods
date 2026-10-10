package fetcher

import (
	"archive/zip"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"time"

	"rimworld-mods/internal/config"
	"rimworld-mods/internal/manifest"
)

// FetchResult represents status of fetching a mod.
type FetchResult struct {
	ModID   string
	Status  string
	Message string
}

// ValidateCache checks whether a cached vendor mod directory exists, is non-empty,
// contains an About/About.xml, and matches its declared source type.
func ValidateCache(env *config.Environment, modID string, entry manifest.ModEntry) (bool, string) {
	destDir := filepath.Join(env.RepoRoot, "mods", "vendor", modID)
	fi, err := os.Stat(destDir)
	if err != nil || !fi.IsDir() {
		return false, "directory missing"
	}

	entries, err := os.ReadDir(destDir)
	if err != nil || len(entries) == 0 {
		return false, "directory empty"
	}

	// Source-specific integrity check
	source := entry.Source
	if source == "" {
		source = "steam_workshop"
	}

	if source == "steam_workshop" {
		// A steam workshop mod must NOT be a raw git repository clone
		if gitFi, err := os.Stat(filepath.Join(destDir, ".git")); err == nil && gitFi.IsDir() {
			return false, "source mismatch (found .git repo in steam_workshop cache)"
		}
	} else if source == "git" {
		if gitFi, err := os.Stat(filepath.Join(destDir, ".git")); err != nil || !gitFi.IsDir() {
			return false, "source mismatch (expected git repository)"
		}
	}

	// About/About.xml presence check
	aboutPath := filepath.Join(destDir, "About", "About.xml")
	if _, err := os.Stat(aboutPath); err != nil {
		return false, "missing About/About.xml"
	}

	return true, "valid"
}

// FindMissingMods returns a list of mod IDs declared in manifest but missing or invalid in vendor directory.
func FindMissingMods(env *config.Environment, mf *manifest.Manifest) []string {
	var missing []string

	for modID, entry := range mf.Mods {
		if !entry.IsEnabled() {
			continue
		}
		if entry.Source == "custom" || entry.Source == "local" {
			continue
		}

		valid, _ := ValidateCache(env, modID, entry)
		if !valid {
			missing = append(missing, modID)
		}
	}

	return missing
}

// FetchMod downloads a single mod natively in Go based on its declared source.
func FetchMod(env *config.Environment, modID string, entry manifest.ModEntry, dryRun bool) error {
	source := entry.Source
	if source == "" {
		source = "steam_workshop"
	}

	destDir := filepath.Join(env.RepoRoot, "mods", "vendor", modID)

	switch source {
	case "steam_workshop":
		if entry.WorkshopID == "" {
			return fmt.Errorf("mod '%s' has source steam_workshop but missing workshop_id", modID)
		}
		return FetchSteamWorkshop(env, modID, entry.WorkshopID, destDir, dryRun)

	case "github_release":
		if entry.Repo == "" {
			return fmt.Errorf("mod '%s' has source github_release but missing repo", modID)
		}
		return FetchGitHubRelease(modID, entry.Repo, entry.Tag, entry.Asset, destDir, dryRun)

	case "git":
		if entry.URL == "" {
			return fmt.Errorf("mod '%s' has source git but missing url", modID)
		}
		return FetchGitRepo(modID, entry.URL, entry.Branch, destDir, dryRun)

	case "custom", "local":
		return nil

	default:
		return fmt.Errorf("unknown source type '%s' for mod '%s'", source, modID)
	}
}

// FetchSteamWorkshop invokes steamcmd to download an item and cleanly replaces the vendor directory.
func FetchSteamWorkshop(env *config.Environment, modID, workshopID, destDir string, dryRun bool) error {
	steamcmd := env.SteamCMDBin
	if steamcmd == "" {
		steamcmd = "steamcmd"
	}
	if _, err := exec.LookPath(steamcmd); err != nil {
		return fmt.Errorf("steamcmd executable not found in PATH: %w", err)
	}

	if dryRun {
		fmt.Printf("  [DRY-RUN] Would run steamcmd to download workshop item %s into %s\n", workshopID, destDir)
		return nil
	}

	tempDir, err := os.MkdirTemp("", "rimmod-steamcmd-*")
	if err != nil {
		return fmt.Errorf("failed to create temp directory: %w", err)
	}
	defer func() { _ = os.RemoveAll(tempDir) }()

	fmt.Printf("  ==> Downloading Steam Workshop item %s via steamcmd...\n", workshopID)
	cmd := exec.Command(steamcmd,
		"+force_install_dir", tempDir,
		"+login", "anonymous",
		"+workshop_download_item", "294100", workshopID, "validate",
		"+quit",
	)
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr

	if err := cmd.Run(); err != nil {
		return fmt.Errorf("steamcmd execution failed for workshop ID %s: %w", workshopID, err)
	}

	downloadDir := filepath.Join(tempDir, "steamapps", "workshop", "content", "294100", workshopID)
	if fi, err := os.Stat(downloadDir); err != nil || !fi.IsDir() {
		return fmt.Errorf("downloaded files not found at expected path: %s", downloadDir)
	}

	// Cleanly replace destination directory
	if err := os.RemoveAll(destDir); err != nil {
		return fmt.Errorf("failed to remove old cache at %s: %w", destDir, err)
	}
	if err := os.MkdirAll(filepath.Dir(destDir), 0755); err != nil {
		return fmt.Errorf("failed to create parent dir: %w", err)
	}

	if err := CopyDir(downloadDir, destDir); err != nil {
		return fmt.Errorf("failed to copy downloaded files to %s: %w", destDir, err)
	}

	// Some workshop authors inadvertently upload their local .git folder to Steam Workshop; prune it
	_ = os.RemoveAll(filepath.Join(destDir, ".git"))

	return nil
}

// FetchGitHubRelease downloads and extracts a GitHub release asset zip.
func FetchGitHubRelease(modID, repo, tag, assetName, destDir string, dryRun bool) error {
	apiURL := fmt.Sprintf("https://api.github.com/repos/%s/releases/latest", repo)
	if tag != "" && tag != "latest" {
		apiURL = fmt.Sprintf("https://api.github.com/repos/%s/releases/tags/%s", repo, tag)
	}

	client := &http.Client{Timeout: 30 * time.Second}
	req, err := http.NewRequest("GET", apiURL, nil)
	if err != nil {
		return err
	}
	req.Header.Set("Accept", "application/vnd.github.v3+json")
	req.Header.Set("User-Agent", "RimWorldMonorepo/1.0")

	resp, err := client.Do(req)
	if err != nil {
		return fmt.Errorf("failed to query GitHub API for %s: %w", repo, err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return fmt.Errorf("GitHub API returned status %d for %s", resp.StatusCode, repo)
	}

	var releaseData struct {
		TagName string `json:"tag_name"`
		Assets  []struct {
			Name               string `json:"name"`
			BrowserDownloadURL string `json:"browser_download_url"`
		} `json:"assets"`
		ZipballURL string `json:"zipball_url"`
	}

	if err := json.NewDecoder(resp.Body).Decode(&releaseData); err != nil {
		return fmt.Errorf("failed to decode GitHub release json: %w", err)
	}

	downloadURL := ""
	for _, asset := range releaseData.Assets {
		if assetName != "" && asset.Name == assetName {
			downloadURL = asset.BrowserDownloadURL
			break
		}
		if assetName == "" && strings.HasSuffix(asset.Name, ".zip") {
			downloadURL = asset.BrowserDownloadURL
			break
		}
	}
	if downloadURL == "" {
		downloadURL = releaseData.ZipballURL
	}
	if downloadURL == "" {
		return fmt.Errorf("no download asset found for GitHub release %s", repo)
	}

	if dryRun {
		fmt.Printf("  [DRY-RUN] Would download %s into %s\n", downloadURL, destDir)
		return nil
	}

	fmt.Printf("  ==> Downloading GitHub release asset from %s...\n", downloadURL)
	tempFile, err := os.CreateTemp("", "rimmod-gh-*.zip")
	if err != nil {
		return err
	}
	defer func() {
		_ = tempFile.Close()
		_ = os.Remove(tempFile.Name())
	}()

	dlResp, err := http.Get(downloadURL)
	if err != nil {
		return fmt.Errorf("failed to download release asset: %w", err)
	}
	defer dlResp.Body.Close()

	if _, err := io.Copy(tempFile, dlResp.Body); err != nil {
		return fmt.Errorf("failed to write zip file: %w", err)
	}
	_ = tempFile.Close()

	if err := os.RemoveAll(destDir); err != nil {
		return err
	}
	if err := os.MkdirAll(destDir, 0755); err != nil {
		return err
	}

	return UnzipToDir(tempFile.Name(), destDir)
}

// FetchGitRepo clones or pulls a git repository.
func FetchGitRepo(modID, url, branch, destDir string, dryRun bool) error {
	if branch == "" {
		branch = "main"
	}

	if dryRun {
		fmt.Printf("  [DRY-RUN] Would clone/pull git repo %s (%s) to %s\n", url, branch, destDir)
		return nil
	}

	if _, err := os.Stat(filepath.Join(destDir, ".git")); err == nil {
		fmt.Printf("  ==> Pulling latest git updates for %s...\n", modID)
		cmd := exec.Command("git", "-C", destDir, "pull", "--ff-only")
		cmd.Stdout = os.Stdout
		cmd.Stderr = os.Stderr
		return cmd.Run()
	}

	_ = os.RemoveAll(destDir)
	_ = os.MkdirAll(filepath.Dir(destDir), 0755)

	fmt.Printf("  ==> Cloning git repository for %s (%s)...\n", modID, branch)
	cmd := exec.Command("git", "clone", "--depth", "1", "-b", branch, url, destDir)
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	return cmd.Run()
}

// CopyDir recursively copies directory contents from src to dst.
func CopyDir(src, dst string) error {
	return filepath.Walk(src, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		relPath, err := filepath.Rel(src, path)
		if err != nil {
			return err
		}

		targetPath := filepath.Join(dst, relPath)

		if info.IsDir() {
			return os.MkdirAll(targetPath, info.Mode())
		}

		if (info.Mode() & os.ModeSymlink) != 0 {
			linkTarget, err := os.Readlink(path)
			if err != nil {
				return err
			}
			return os.Symlink(linkTarget, targetPath)
		}

		return copyFile(path, targetPath, info.Mode())
	})
}

func copyFile(src, dst string, mode os.FileMode) error {
	in, err := os.Open(src)
	if err != nil {
		return err
	}
	defer in.Close()

	if err := os.MkdirAll(filepath.Dir(dst), 0755); err != nil {
		return err
	}

	out, err := os.OpenFile(dst, os.O_CREATE|os.O_WRONLY|os.O_TRUNC, mode)
	if err != nil {
		return err
	}
	defer out.Close()

	_, err = io.Copy(out, in)
	return err
}

// UnzipToDir extracts a zip archive to the target directory.
func UnzipToDir(zipPath, dst string) error {
	r, err := zip.OpenReader(zipPath)
	if err != nil {
		return err
	}
	defer r.Close()

	// Detect if all files share a common single root folder inside zip
	prefix := ""
	if len(r.File) > 0 {
		first := strings.Split(filepath.ToSlash(r.File[0].Name), "/")[0]
		allMatch := true
		for _, f := range r.File {
			slash := filepath.ToSlash(f.Name)
			if !strings.HasPrefix(slash, first+"/") && slash != first {
				allMatch = false
				break
			}
		}
		if allMatch {
			prefix = first + "/"
		}
	}

	for _, f := range r.File {
		cleanName := filepath.ToSlash(f.Name)
		if prefix != "" && strings.HasPrefix(cleanName, prefix) {
			cleanName = strings.TrimPrefix(cleanName, prefix)
		}
		if cleanName == "" {
			continue
		}

		target := filepath.Join(dst, filepath.FromSlash(cleanName))

		if f.FileInfo().IsDir() {
			if err := os.MkdirAll(target, f.Mode()); err != nil {
				return err
			}
			continue
		}

		if err := os.MkdirAll(filepath.Dir(target), 0755); err != nil {
			return err
		}

		rc, err := f.Open()
		if err != nil {
			return err
		}

		outFile, err := os.OpenFile(target, os.O_WRONLY|os.O_CREATE|os.O_TRUNC, f.Mode())
		if err != nil {
			rc.Close()
			return err
		}

		_, err = io.Copy(outFile, rc)
		outFile.Close()
		rc.Close()
		if err != nil {
			return err
		}
	}

	return nil
}
