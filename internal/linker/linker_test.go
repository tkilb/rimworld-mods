package linker

import (
	"os"
	"path/filepath"
	"testing"

	"rimworld-mods/internal/config"
	"rimworld-mods/internal/manifest"
)

func TestLinkerWorkflow(t *testing.T) {
	tmpDir := t.TempDir()
	repoRoot := filepath.Join(tmpDir, "repo")
	modsDir := filepath.Join(tmpDir, "RimWorldMods")

	vendorDir := filepath.Join(repoRoot, "mods", "vendor")
	customDir := filepath.Join(repoRoot, "mods", "custom")
	if err := os.MkdirAll(vendorDir, 0755); err != nil {
		t.Fatal(err)
	}
	if err := os.MkdirAll(customDir, 0755); err != nil {
		t.Fatal(err)
	}

	// Create a dummy mod in vendor
	modAPath := filepath.Join(vendorDir, "modA")
	if err := os.MkdirAll(modAPath, 0755); err != nil {
		t.Fatal(err)
	}

	// Create a dummy custom mod
	modBPath := filepath.Join(customDir, "modB", "About")
	if err := os.MkdirAll(modBPath, 0755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(modBPath, "About.xml"), []byte("<ModMetaData><name>B</name></ModMetaData>"), 0644); err != nil {
		t.Fatal(err)
	}

	env := &config.Environment{
		RepoRoot:        repoRoot,
		RimWorldModsDir: modsDir,
	}

	mf := &manifest.Manifest{
		Mods: map[string]manifest.ModEntry{
			"modA": {Name: "Mod A", Source: "steam_workshop"},
			"modB": {Name: "Mod B", Source: "custom"},
		},
	}

	l := NewLinker(env, mf)

	// 1. Dry run
	results, err := l.LinkAll(true)
	if err != nil {
		t.Fatalf("dry run LinkAll failed: %v", err)
	}
	if len(results) != 2 {
		t.Errorf("expected 2 results, got %d", len(results))
	}
	if _, err := os.Stat(filepath.Join(modsDir, "modA")); !os.IsNotExist(err) {
		t.Errorf("dry run created symlink!")
	}

	// 2. Real link
	results, err = l.LinkAll(false)
	if err != nil {
		t.Fatalf("real LinkAll failed: %v", err)
	}
	if len(results) != 2 {
		t.Errorf("expected 2 results, got %d", len(results))
	}

	// Verify symlinks exist
	for _, id := range []string{"modA", "modB"} {
		dest := filepath.Join(modsDir, id)
		fi, err := os.Lstat(dest)
		if err != nil || (fi.Mode()&os.ModeSymlink) == 0 {
			t.Errorf("expected symlink at %s", dest)
		}
	}

	// 3. Re-link (should be UpToDate)
	results, err = l.LinkAll(false)
	if err != nil {
		t.Fatalf("re-link failed: %v", err)
	}
	for _, res := range results {
		if res.Status != StatusUpToDate {
			t.Errorf("expected status up-to-date, got %s for %s", res.Status, res.ModID)
		}
	}

	// 4. UnlinkAll
	unlinked, err := l.UnlinkAll(false)
	if err != nil {
		t.Fatalf("UnlinkAll failed: %v", err)
	}
	if len(unlinked) != 2 {
		t.Errorf("expected 2 unlinked mods, got %d", len(unlinked))
	}

	for _, id := range []string{"modA", "modB"} {
		dest := filepath.Join(modsDir, id)
		if _, err := os.Lstat(dest); !os.IsNotExist(err) {
			t.Errorf("symlink still exists at %s", dest)
		}
	}
}
