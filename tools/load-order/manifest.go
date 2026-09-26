package main

import (
	"os"

	"gopkg.in/yaml.v3"
)

// Manifest represents the structure of manifests/mods.yaml.
type Manifest struct {
	Version int                 `yaml:"version"`
	Mods    map[string]ModEntry `yaml:"mods"`
}

// ModEntry represents an individual mod declared in mods.yaml.
type ModEntry struct {
	Name        string   `yaml:"name"`
	Source      string   `yaml:"source"`
	WorkshopID  string   `yaml:"workshop_id"`
	PackageID   string   `yaml:"package_id"`
	Enabled     *bool    `yaml:"enabled"`
	Description string   `yaml:"description"`
	OrderAfter  []string `yaml:"order_after"`
	OrderBefore []string `yaml:"order_before"`
	Priority    int      `yaml:"priority"`
	Trailing    bool     `yaml:"trailing"`
}

// IsEnabled returns true if Enabled is nil or true.
func (m *ModEntry) IsEnabled() bool {
	if m.Enabled == nil {
		return true
	}
	return *m.Enabled
}

// LoadManifest reads and unmarshals the manifest at the given path.
func LoadManifest(path string) (*Manifest, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil, err
	}

	var m Manifest
	if err := yaml.Unmarshal(data, &m); err != nil {
		return nil, err
	}

	return &m, nil
}
