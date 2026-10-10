package fetcher

import (
	"os"
	"path/filepath"
	"testing"

	"rimworld-mods/internal/config"
	"rimworld-mods/internal/manifest"
)

func TestValidateCache(t *testing.T) {
	tmpDir := t.TempDir()
	env := &config.Environment{RepoRoot: tmpDir}

	tests := []struct {
		name      string
		modID     string
		entry     manifest.ModEntry
		setup     func(dest string)
		wantValid bool
		wantSub   string
	}{
		{
			name:      "missing directory",
			modID:     "missing-mod",
			entry:     manifest.ModEntry{Source: "steam_workshop"},
			setup:     func(dest string) {},
			wantValid: false,
			wantSub:   "directory missing",
		},
		{
			name:  "empty directory",
			modID: "empty-mod",
			entry: manifest.ModEntry{Source: "steam_workshop"},
			setup: func(dest string) {
				_ = os.MkdirAll(dest, 0755)
			},
			wantValid: false,
			wantSub:   "directory empty",
		},
		{
			name:  "git repo in steam_workshop mod",
			modID: "git-in-workshop",
			entry: manifest.ModEntry{Source: "steam_workshop"},
			setup: func(dest string) {
				_ = os.MkdirAll(filepath.Join(dest, ".git"), 0755)
				_ = os.MkdirAll(filepath.Join(dest, "About"), 0755)
				_ = os.WriteFile(filepath.Join(dest, "About", "About.xml"), []byte("<ModMetaData/>"), 0644)
			},
			wantValid: false,
			wantSub:   "source mismatch",
		},
		{
			name:  "missing About.xml",
			modID: "no-about",
			entry: manifest.ModEntry{Source: "steam_workshop"},
			setup: func(dest string) {
				_ = os.MkdirAll(filepath.Join(dest, "Defs"), 0755)
				_ = os.WriteFile(filepath.Join(dest, "Defs", "test.xml"), []byte("<Defs/>"), 0644)
			},
			wantValid: false,
			wantSub:   "missing About/About.xml",
		},
		{
			name:  "valid steam workshop mod",
			modID: "valid-workshop",
			entry: manifest.ModEntry{Source: "steam_workshop"},
			setup: func(dest string) {
				_ = os.MkdirAll(filepath.Join(dest, "About"), 0755)
				_ = os.WriteFile(filepath.Join(dest, "About", "About.xml"), []byte("<ModMetaData/>"), 0644)
			},
			wantValid: true,
			wantSub:   "valid",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			dest := filepath.Join(tmpDir, "mods", "vendor", tt.modID)
			tt.setup(dest)

			valid, reason := ValidateCache(env, tt.modID, tt.entry)
			if valid != tt.wantValid {
				t.Errorf("ValidateCache() valid = %v, want %v (reason: %s)", valid, tt.wantValid, reason)
			}
			if tt.wantSub != "" && reason != tt.wantSub && len(reason) < len(tt.wantSub) {
				t.Errorf("ValidateCache() reason = %q, want substring %q", reason, tt.wantSub)
			}
		})
	}
}
