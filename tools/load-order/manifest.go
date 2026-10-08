package main

import (
	"os"
	"strings"

	"gopkg.in/yaml.v3"
)

// Manifest represents the structure of manifests/mods.yaml.
type Manifest struct {
	Version int                   `yaml:"version"`
	DLCs    map[string]DLCSetting `yaml:"dlcs"`
	Mods    map[string]ModEntry   `yaml:"mods"`
}

// DLCSetting represents configuration for an official expansion.
type DLCSetting struct {
	Enabled bool
}

// UnmarshalYAML supports both scalar booleans (e.g. `ideology: false`)
// and mapping objects (e.g. `ideology: { enabled: false }`).
func (d *DLCSetting) UnmarshalYAML(value *yaml.Node) error {
	if value.Kind == yaml.ScalarNode {
		var b bool
		if err := value.Decode(&b); err != nil {
			return err
		}
		d.Enabled = b
		return nil
	}
	if value.Kind == yaml.MappingNode {
		var m struct {
			Enabled *bool `yaml:"enabled"`
		}
		if err := value.Decode(&m); err != nil {
			return err
		}
		if m.Enabled != nil {
			d.Enabled = *m.Enabled
		} else {
			d.Enabled = true
		}
		return nil
	}
	return nil
}

// IsDLCEnabled checks if a DLC (by package ID or short name) is enabled in the manifest.
// If the DLC is not specified in the manifest, it defaults to true.
func (m *Manifest) IsDLCEnabled(dlc string) bool {
	if m.DLCs == nil {
		return true
	}
	norm := strings.ToLower(dlc)
	short := strings.TrimPrefix(norm, "ludeon.rimworld.")

	if setting, ok := m.DLCs[short]; ok {
		return setting.Enabled
	}
	if setting, ok := m.DLCs[norm]; ok {
		return setting.Enabled
	}
	return true
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
