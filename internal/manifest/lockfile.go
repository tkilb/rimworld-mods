package manifest

import (
	"os"
	"time"

	"gopkg.in/yaml.v3"
)

// Lockfile represents the structure of manifests/mods.lock.yaml.
type Lockfile struct {
	Version     int                  `yaml:"version"`
	GeneratedAt string               `yaml:"generated_at"`
	Machine     string               `yaml:"machine"`
	Mods        map[string]LockEntry `yaml:"mods"`
}

// LockEntry records locked state for a single downloaded mod.
type LockEntry struct {
	Source              string `yaml:"source"`
	WorkshopID          string `yaml:"workshop_id,omitempty"`
	WorkshopTimeUpdated int64  `yaml:"workshop_time_updated,omitempty"`
	WorkshopTitle       string `yaml:"workshop_title,omitempty"`
	DownloadPath        string `yaml:"download_path"`
	Repo                string `yaml:"repo,omitempty"`
	ResolvedTag         string `yaml:"resolved_tag,omitempty"`
	PublishedAt         string `yaml:"published_at,omitempty"`
	AssetName           string `yaml:"asset_name,omitempty"`
	GitRepo             string `yaml:"git_repo,omitempty"`
	GitRef              string `yaml:"git_ref,omitempty"`
	GitCommit           string `yaml:"git_commit,omitempty"`
}

// LoadLockfile reads and parses mods.lock.yaml.
func LoadLockfile(path string) (*Lockfile, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		if os.IsNotExist(err) {
			return &Lockfile{
				Version: 1,
				Mods:    make(map[string]LockEntry),
			}, nil
		}
		return nil, err
	}

	var lf Lockfile
	if err := yaml.Unmarshal(data, &lf); err != nil {
		return nil, err
	}

	if lf.Mods == nil {
		lf.Mods = make(map[string]LockEntry)
	}

	return &lf, nil
}

// Save writes the lockfile to disk with an updated timestamp.
func (lf *Lockfile) Save(path string, machine string) error {
	lf.GeneratedAt = time.Now().UTC().Format(time.RFC3339)
	if machine != "" {
		lf.Machine = machine
	}
	if lf.Version == 0 {
		lf.Version = 1
	}

	data, err := yaml.Marshal(lf)
	if err != nil {
		return err
	}

	return os.WriteFile(path, data, 0644)
}
