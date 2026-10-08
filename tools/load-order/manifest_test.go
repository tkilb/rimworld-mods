package main

import (
	"os"
	"path/filepath"
	"testing"
)

func TestManifestDLCs(t *testing.T) {
	yamlContent := `version: 1
dlcs:
  royalty: true
  ideology: false
  biotech:
    enabled: true
  anomaly:
    enabled: false
  odyssey: false

mods:
  harmony:
    name: "Harmony"
    package_id: "brrainz.harmony"
    enabled: true
`
	tmpDir := t.TempDir()
	manifestPath := filepath.Join(tmpDir, "mods.yaml")
	if err := os.WriteFile(manifestPath, []byte(yamlContent), 0644); err != nil {
		t.Fatalf("failed to write test manifest: %v", err)
	}

	mf, err := LoadManifest(manifestPath)
	if err != nil {
		t.Fatalf("LoadManifest failed: %v", err)
	}

	tests := []struct {
		dlc         string
		wantEnabled bool
	}{
		{"royalty", true},
		{"ludeon.rimworld.royalty", true},
		{"ideology", false},
		{"ludeon.rimworld.ideology", false},
		{"biotech", true},
		{"ludeon.rimworld.biotech", true},
		{"anomaly", false},
		{"ludeon.rimworld.anomaly", false},
		{"odyssey", false},
		{"ludeon.rimworld.odyssey", false},
		{"unknown_future_dlc", true}, // default to true if unspecified
	}

	for _, tt := range tests {
		got := mf.IsDLCEnabled(tt.dlc)
		if got != tt.wantEnabled {
			t.Errorf("IsDLCEnabled(%q) = %v, want %v", tt.dlc, got, tt.wantEnabled)
		}
	}
}

func TestManifestWithoutDLCs(t *testing.T) {
	yamlContent := `version: 1
mods:
  harmony:
    name: "Harmony"
    package_id: "brrainz.harmony"
    enabled: true
`
	tmpDir := t.TempDir()
	manifestPath := filepath.Join(tmpDir, "mods.yaml")
	if err := os.WriteFile(manifestPath, []byte(yamlContent), 0644); err != nil {
		t.Fatalf("failed to write test manifest: %v", err)
	}

	mf, err := LoadManifest(manifestPath)
	if err != nil {
		t.Fatalf("LoadManifest failed: %v", err)
	}

	if !mf.IsDLCEnabled("ideology") {
		t.Errorf("expected ideology to default to enabled when dlcs: is omitted")
	}
	if !mf.IsDLCEnabled("ludeon.rimworld.biotech") {
		t.Errorf("expected biotech to default to enabled when dlcs: is omitted")
	}
}
