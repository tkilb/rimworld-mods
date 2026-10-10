package manifest

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

// UnmarshalYAML supports both scalar booleans and mapping objects.
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

// IsDLCEnabled checks if a DLC is enabled in the manifest.
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
	Name        string   `yaml:"name,omitempty"`
	Source      string   `yaml:"source,omitempty"`
	WorkshopID  string   `yaml:"workshop_id,omitempty"`
	PackageID   string   `yaml:"package_id,omitempty"`
	Enabled     *bool    `yaml:"enabled,omitempty"`
	Description string   `yaml:"description,omitempty"`
	OrderAfter  []string `yaml:"order_after,omitempty"`
	OrderBefore []string `yaml:"order_before,omitempty"`
	Priority    int      `yaml:"priority,omitempty"`
	Trailing    bool     `yaml:"trailing,omitempty"`
	Repo        string   `yaml:"repo,omitempty"`
	Tag         string   `yaml:"tag,omitempty"`
	Asset       string   `yaml:"asset,omitempty"`
	URL         string   `yaml:"url,omitempty"`
	Branch      string   `yaml:"branch,omitempty"`
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

	if m.Mods == nil {
		m.Mods = make(map[string]ModEntry)
	}

	return &m, nil
}

// SaveManifest writes the manifest back to disk.
func (m *Manifest) Save(path string) error {
	data, err := yaml.Marshal(m)
	if err != nil {
		return err
	}
	return os.WriteFile(path, data, 0644)
}
