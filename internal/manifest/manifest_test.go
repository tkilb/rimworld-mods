package manifest

import (
	"os"
	"path/filepath"
	"testing"
)

func TestLoadManifest(t *testing.T) {
	yamlContent := `
version: 1
dlcs:
  royalty: true
  ideology: false
  biotech:
    enabled: true
mods:
  harmony:
    name: Harmony
    source: steam_workshop
    workshop_id: "2009463077"
    package_id: brrainz.harmony
  test_mod:
    name: Test Mod
    enabled: false
`
	tmpDir := t.TempDir()
	manifestFile := filepath.Join(tmpDir, "mods.yaml")
	if err := os.WriteFile(manifestFile, []byte(yamlContent), 0644); err != nil {
		t.Fatal(err)
	}

	m, err := LoadManifest(manifestFile)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}

	if m.Version != 1 {
		t.Errorf("expected version 1, got %d", m.Version)
	}
	if !m.IsDLCEnabled("royalty") {
		t.Errorf("expected royalty enabled")
	}
	if m.IsDLCEnabled("ideology") {
		t.Errorf("expected ideology disabled")
	}
	if !m.IsDLCEnabled("biotech") {
		t.Errorf("expected biotech enabled")
	}
	if !m.IsDLCEnabled("anomaly") {
		t.Errorf("expected anomaly enabled by default")
	}

	h, ok := m.Mods["harmony"]
	if !ok {
		t.Fatalf("expected harmony in mods")
	}
	if !h.IsEnabled() {
		t.Errorf("expected harmony to be enabled")
	}

	tm, ok := m.Mods["test_mod"]
	if !ok {
		t.Fatalf("expected test_mod in mods")
	}
	if tm.IsEnabled() {
		t.Errorf("expected test_mod to be disabled")
	}
}

func TestLockfileRoundTrip(t *testing.T) {
	tmpDir := t.TempDir()
	lockPath := filepath.Join(tmpDir, "mods.lock.yaml")

	lf := &Lockfile{
		Version: 1,
		Mods: map[string]LockEntry{
			"harmony": {
				Source:       "steam_workshop",
				WorkshopID:   "2009463077",
				DownloadPath: "mods/vendor/harmony",
			},
		},
	}

	if err := lf.Save(lockPath, "linux-box"); err != nil {
		t.Fatalf("failed to save lockfile: %v", err)
	}

	loaded, err := LoadLockfile(lockPath)
	if err != nil {
		t.Fatalf("failed to load lockfile: %v", err)
	}

	if loaded.Machine != "linux-box" {
		t.Errorf("expected machine linux-box, got %s", loaded.Machine)
	}
	if loaded.Mods["harmony"].WorkshopID != "2009463077" {
		t.Errorf("expected workshop ID 2009463077, got %s", loaded.Mods["harmony"].WorkshopID)
	}
}
