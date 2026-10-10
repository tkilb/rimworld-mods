package updater

import (
	"encoding/json"
	"fmt"
	"net/http"
	"net/url"
	"path/filepath"
	"strings"
	"time"

	"rimworld-mods/internal/config"
	"rimworld-mods/internal/fetcher"
	"rimworld-mods/internal/manifest"
)

// UpdateStatus describes the update outcome for a single mod.
type UpdateStatus struct {
	ModID   string
	Name    string
	Source  string
	Reason  string
	NewInfo string
}

// UpdateSummary summarizes the entire update operation.
type UpdateSummary struct {
	Updated   []UpdateStatus
	Unchanged []UpdateStatus
	Errors    map[string]error
}

// CheckSteamDetails queries the Steam API for workshop item metadata.
func CheckSteamDetails(workshopID string) (int64, string, error) {
	apiURL := "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/"
	form := url.Values{}
	form.Set("itemcount", "1")
	form.Set("publishedfileids[0]", workshopID)

	client := &http.Client{Timeout: 15 * time.Second}
	resp, err := client.PostForm(apiURL, form)
	if err != nil {
		return 0, "", fmt.Errorf("steam API request failed: %w", err)
	}
	defer resp.Body.Close()

	var data struct {
		Response struct {
			ResultCount          int `json:"resultcount"`
			PublishedFileDetails []struct {
				PublishedFileID string `json:"publishedfileid"`
				Result          int    `json:"result"`
				Title           string `json:"title"`
				TimeUpdated     int64  `json:"time_updated"`
			} `json:"publishedfiledetails"`
		} `json:"response"`
	}

	if err := json.NewDecoder(resp.Body).Decode(&data); err != nil {
		return 0, "", fmt.Errorf("failed to decode steam response: %w", err)
	}

	if len(data.Response.PublishedFileDetails) == 0 {
		return 0, "", fmt.Errorf("no details returned for workshop item %s", workshopID)
	}

	detail := data.Response.PublishedFileDetails[0]
	if detail.Result != 1 {
		return 0, "", fmt.Errorf("steam returned non-success result code %d", detail.Result)
	}

	return detail.TimeUpdated, detail.Title, nil
}

// CheckGitHubReleaseDetails queries GitHub's API for release metadata.
func CheckGitHubReleaseDetails(repo, tag string) (string, string, string, error) {
	apiURL := fmt.Sprintf("https://api.github.com/repos/%s/releases/latest", repo)
	if tag != "" && tag != "latest" {
		apiURL = fmt.Sprintf("https://api.github.com/repos/%s/releases/tags/%s", repo, tag)
	}

	client := &http.Client{Timeout: 15 * time.Second}
	req, err := http.NewRequest("GET", apiURL, nil)
	if err != nil {
		return "", "", "", err
	}
	req.Header.Set("Accept", "application/vnd.github.v3+json")
	req.Header.Set("User-Agent", "RimWorldMonorepo/1.0")

	resp, err := client.Do(req)
	if err != nil {
		return "", "", "", fmt.Errorf("github request failed: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return "", "", "", fmt.Errorf("github API returned status %d", resp.StatusCode)
	}

	var rel struct {
		TagName     string `json:"tag_name"`
		PublishedAt string `json:"published_at"`
		Assets      []struct {
			Name string `json:"name"`
		} `json:"assets"`
	}

	if err := json.NewDecoder(resp.Body).Decode(&rel); err != nil {
		return "", "", "", err
	}

	assetName := ""
	for _, a := range rel.Assets {
		if strings.HasSuffix(a.Name, ".zip") {
			assetName = a.Name
			break
		}
	}

	return rel.TagName, rel.PublishedAt, assetName, nil
}

// UpdateAll checks upstream sources, synchronizes lockfile, and cleanly fetches updated or invalid mods.
func UpdateAll(env *config.Environment, targetMod string, dryRun bool) (*UpdateSummary, error) {
	manifestPath := filepath.Join(env.RepoRoot, "manifests", "mods.yaml")
	lockPath := filepath.Join(env.RepoRoot, "manifests", "mods.lock.yaml")

	mf, err := manifest.LoadManifest(manifestPath)
	if err != nil {
		return nil, fmt.Errorf("failed to load manifest: %w", err)
	}

	lf, err := manifest.LoadLockfile(lockPath)
	if err != nil {
		return nil, fmt.Errorf("failed to load lockfile: %w", err)
	}

	summary := &UpdateSummary{
		Errors: make(map[string]error),
	}

	lockModified := false

	for modID, entry := range mf.Mods {
		if targetMod != "" && modID != targetMod {
			continue
		}
		if !entry.IsEnabled() {
			continue
		}
		if entry.Source == "custom" || entry.Source == "local" {
			continue
		}

		source := entry.Source
		if source == "" {
			source = "steam_workshop"
		}

		currentLock, hasLock := lf.Mods[modID]
		validCache, cacheReason := fetcher.ValidateCache(env, modID, entry)

		switch source {
		case "steam_workshop":
			fmt.Printf("Checking %s (Workshop ID: %s)...\n", modID, entry.WorkshopID)
			remoteTime, title, err := CheckSteamDetails(entry.WorkshopID)
			if err != nil {
				summary.Errors[modID] = err
				fmt.Printf("  [WARN] Failed to query Steam API: %v\n", err)
				continue
			}

			needsUpdate := false
			updateReason := ""

			if !validCache {
				needsUpdate = true
				updateReason = fmt.Sprintf("invalid cache: %s", cacheReason)
			} else if !hasLock {
				needsUpdate = true
				updateReason = "not locked"
			} else if remoteTime > currentLock.WorkshopTimeUpdated {
				needsUpdate = true
				updateReason = fmt.Sprintf("newer upstream version available (current rev:%d, new rev:%d)",
					currentLock.WorkshopTimeUpdated, remoteTime)
			}

			if needsUpdate {
				newLock := manifest.LockEntry{
					Source:              "steam_workshop",
					WorkshopID:          entry.WorkshopID,
					WorkshopTimeUpdated: remoteTime,
					WorkshopTitle:       title,
					DownloadPath:        fmt.Sprintf("mods/vendor/%s", modID),
				}

				status := UpdateStatus{
					ModID:   modID,
					Name:    title,
					Source:  source,
					Reason:  updateReason,
					NewInfo: fmt.Sprintf("rev:%d", remoteTime),
				}

				if dryRun {
					fmt.Printf("  [DRY-RUN] Would update %s (%s)\n", modID, updateReason)
				} else {
					fmt.Printf("  ==> Updating %s: %s\n", modID, updateReason)
					if err := fetcher.FetchMod(env, modID, entry, false); err != nil {
						summary.Errors[modID] = err
						fmt.Printf("  [ERROR] Failed to fetch %s: %v\n", modID, err)
						continue
					}
					lf.Mods[modID] = newLock
					lockModified = true
				}

				summary.Updated = append(summary.Updated, status)
			} else {
				summary.Unchanged = append(summary.Unchanged, UpdateStatus{
					ModID:  modID,
					Name:   currentLock.WorkshopTitle,
					Source: source,
				})
			}

		case "github_release":
			fmt.Printf("Checking %s (GitHub: %s)...\n", modID, entry.Repo)
			tag, pubAt, assetName, err := CheckGitHubReleaseDetails(entry.Repo, entry.Tag)
			if err != nil {
				summary.Errors[modID] = err
				fmt.Printf("  [WARN] Failed to query GitHub API: %v\n", err)
				continue
			}

			needsUpdate := false
			updateReason := ""

			if !validCache {
				needsUpdate = true
				updateReason = fmt.Sprintf("invalid cache: %s", cacheReason)
			} else if !hasLock || currentLock.ResolvedTag != tag {
				needsUpdate = true
				updateReason = fmt.Sprintf("new release tag: %s", tag)
			}

			if needsUpdate {
				newLock := manifest.LockEntry{
					Source:       "github_release",
					Repo:         entry.Repo,
					ResolvedTag:  tag,
					PublishedAt:  pubAt,
					AssetName:    assetName,
					DownloadPath: fmt.Sprintf("mods/vendor/%s", modID),
				}

				status := UpdateStatus{
					ModID:   modID,
					Source:  source,
					Reason:  updateReason,
					NewInfo: tag,
				}

				if dryRun {
					fmt.Printf("  [DRY-RUN] Would update %s (%s)\n", modID, updateReason)
				} else {
					fmt.Printf("  ==> Updating %s: %s\n", modID, updateReason)
					if err := fetcher.FetchMod(env, modID, entry, false); err != nil {
						summary.Errors[modID] = err
						fmt.Printf("  [ERROR] Failed to fetch %s: %v\n", modID, err)
						continue
					}
					lf.Mods[modID] = newLock
					lockModified = true
				}

				summary.Updated = append(summary.Updated, status)
			} else {
				summary.Unchanged = append(summary.Unchanged, UpdateStatus{
					ModID:  modID,
					Source: source,
				})
			}

		case "git":
			fmt.Printf("Checking %s (Git: %s)...\n", modID, entry.URL)
			if !validCache {
				if dryRun {
					fmt.Printf("  [DRY-RUN] Would clone git repo %s\n", entry.URL)
				} else {
					if err := fetcher.FetchMod(env, modID, entry, false); err != nil {
						summary.Errors[modID] = err
						fmt.Printf("  [ERROR] Failed to fetch %s: %v\n", modID, err)
						continue
					}
				}
				summary.Updated = append(summary.Updated, UpdateStatus{
					ModID:  modID,
					Source: source,
					Reason: cacheReason,
				})
			} else {
				summary.Unchanged = append(summary.Unchanged, UpdateStatus{
					ModID:  modID,
					Source: source,
				})
			}
		}
	}

	if !dryRun && lockModified {
		if err := lf.Save(lockPath, env.Machine); err != nil {
			return summary, fmt.Errorf("failed to save lockfile: %w", err)
		}
		fmt.Printf("\n[OK] Synchronized updated mod versions to %s\n", lockPath)
	}

	return summary, nil
}
