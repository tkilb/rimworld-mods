package config

import (
	"os"
	"path/filepath"
	"testing"
)

func TestLoadEnvironment(t *testing.T) {
	tmpDir := t.TempDir()
	configDir := filepath.Join(tmpDir, "config")
	if err := os.MkdirAll(configDir, 0755); err != nil {
		t.Fatal(err)
	}

	localEnv := `
MACHINE="linux-box"
DECK_HOST="custom-deck"
RIMWORLD_MODS_DIR="/tmp/custom/Mods"
`
	if err := os.WriteFile(filepath.Join(configDir, "local.env"), []byte(localEnv), 0644); err != nil {
		t.Fatal(err)
	}

	env, err := LoadEnvironment(tmpDir)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}

	if env.Machine != "linux-box" {
		t.Errorf("expected Machine 'linux-box', got '%s'", env.Machine)
	}
	if env.DeckHost != "custom-deck" {
		t.Errorf("expected DeckHost 'custom-deck', got '%s'", env.DeckHost)
	}
	if env.RimWorldModsDir != "/tmp/custom/Mods" {
		t.Errorf("expected ModsDir '/tmp/custom/Mods', got '%s'", env.RimWorldModsDir)
	}
}

func TestFindRepoRoot(t *testing.T) {
	tmpDir := t.TempDir()
	manifestsDir := filepath.Join(tmpDir, "manifests")
	if err := os.MkdirAll(manifestsDir, 0755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(manifestsDir, "mods.yaml"), []byte("mods: []\n"), 0644); err != nil {
		t.Fatal(err)
	}

	subDir := filepath.Join(tmpDir, "mods", "custom", "test-mod")
	if err := os.MkdirAll(subDir, 0755); err != nil {
		t.Fatal(err)
	}

	origWd, err := os.Getwd()
	if err != nil {
		t.Fatal(err)
	}
	defer func() { _ = os.Chdir(origWd) }()

	if err := os.Chdir(subDir); err != nil {
		t.Fatal(err)
	}

	root, err := FindRepoRoot()
	if err != nil {
		t.Fatalf("FindRepoRoot failed: %v", err)
	}

	resolvedExpected, _ := filepath.EvalSymlinks(tmpDir)
	resolvedActual, _ := filepath.EvalSymlinks(root)
	if resolvedActual != resolvedExpected {
		t.Errorf("expected repo root %q, got %q", resolvedExpected, resolvedActual)
	}
}
