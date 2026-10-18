package importer

import (
	"os"
	"path/filepath"
	"strings"
	"testing"

	"rimworld-mods/internal/manifest"
)

func TestParseSource(t *testing.T) {
	tests := []struct {
		name        string
		input       string
		wantType    string
		wantID      string
		wantRepo    string
		wantTag     string
		wantURL     string
		wantErr     bool
	}{
		{
			name:     "Numeric Workshop ID",
			input:    "3525571078",
			wantType: "steam_workshop",
			wantID:   "3525571078",
		},
		{
			name:     "Steam Community sharedfiles URL",
			input:    "https://steamcommunity.com/sharedfiles/filedetails/?id=3525571078",
			wantType: "steam_workshop",
			wantID:   "3525571078",
		},
		{
			name:     "Steam Community workshop URL with extra query params",
			input:    "https://steamcommunity.com/workshop/filedetails/?id=2009463077&searchtext=harmony",
			wantType: "steam_workshop",
			wantID:   "2009463077",
		},
		{
			name:     "GitHub release tagged URL",
			input:    "https://github.com/UnlimitedHugs/RimworldHugsLib/releases/tag/v11.0.0",
			wantType: "github_release",
			wantRepo: "UnlimitedHugs/RimworldHugsLib",
			wantTag:  "v11.0.0",
		},
		{
			name:     "GitHub release latest URL",
			input:    "https://github.com/UnlimitedHugs/RimworldHugsLib/releases/latest",
			wantType: "github_release",
			wantRepo: "UnlimitedHugs/RimworldHugsLib",
			wantTag:  "latest",
		},
		{
			name:     "GitHub repo URL defaults to github_release latest",
			input:    "https://github.com/UnlimitedHugs/RimworldHugsLib",
			wantType: "github_release",
			wantRepo: "UnlimitedHugs/RimworldHugsLib",
			wantTag:  "latest",
		},
		{
			name:     "GitHub git URL",
			input:    "https://github.com/example/rimworld-mod.git",
			wantType: "git",
			wantURL:  "https://github.com/example/rimworld-mod.git",
		},
		{
			name:     "SSH git URL",
			input:    "git@github.com:example/rimworld-mod.git",
			wantType: "git",
			wantURL:  "git@github.com:example/rimworld-mod.git",
		},
		{
			name:    "Invalid empty input",
			input:   "   ",
			wantErr: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got, err := ParseSource(tt.input)
			if (err != nil) != tt.wantErr {
				t.Fatalf("ParseSource(%q) error = %v, wantErr %v", tt.input, err, tt.wantErr)
			}
			if tt.wantErr {
				return
			}
			if got.SourceType != tt.wantType {
				t.Errorf("SourceType = %v, want %v", got.SourceType, tt.wantType)
			}
			if got.WorkshopID != tt.wantID {
				t.Errorf("WorkshopID = %v, want %v", got.WorkshopID, tt.wantID)
			}
			if got.Repo != tt.wantRepo {
				t.Errorf("Repo = %v, want %v", got.Repo, tt.wantRepo)
			}
			if got.Tag != tt.wantTag {
				t.Errorf("Tag = %v, want %v", got.Tag, tt.wantTag)
			}
			if got.URL != tt.wantURL {
				t.Errorf("URL = %v, want %v", got.URL, tt.wantURL)
			}
		})
	}
}

func TestToKebabCase(t *testing.T) {
	tests := []struct {
		input string
		want  string
	}{
		{"LaunchSitesExpire", "launch-sites-expire"},
		{"VanillaBionicsExpansion", "vanilla-bionics-expansion"},
		{"SimpleSidearms", "simple-sidearms"},
		{"VFECore", "vfe-core"},
		{"VanillaVehiclesExpanded", "vanilla-vehicles-expanded"},
		{"PeteTimesSix", "pete-times-six"},
		{"DubsPaintShop", "dubs-paint-shop"},
		{"GeneRipper", "gene-ripper"},
		{"MinifyEverything", "minify-everything"},
		{"[FSF] Vanilla Bionics", "fsf-vanilla-bionics"},
		{"already-kebab-case", "already-kebab-case"},
		{"", ""},
	}

	for _, tt := range tests {
		t.Run(tt.input, func(t *testing.T) {
			got := ToKebabCase(tt.input)
			if got != tt.want {
				t.Errorf("ToKebabCase(%q) = %q, want %q", tt.input, got, tt.want)
			}
		})
	}
}

func TestDeriveSlug(t *testing.T) {
	existing := map[string]bool{
		"launch-sites-expire": true,
	}

	tests := []struct {
		name      string
		packageID string
		want      string
	}{
		{
			name:      "Standard PascalCase packageId",
			packageID: "cass.LaunchSitesExpire",
			// already in existing -> should append -2
			want: "launch-sites-expire-2",
		},
		{
			name:      "Multi-part packageId",
			packageID: "FrozenSnowFox.VanillaBionicsExpansion",
			want:      "vanilla-bionics-expansion",
		},
		{
			name:      "Generic suffix Core combined with preceding part",
			packageID: "OskarPotocki.VanillaFactionsExpanded.Core",
			want:      "vanilla-factions-expanded-core",
		},
		{
			name:      "Generic suffix Framework combined",
			packageID: "SmashPhil.VehicleFramework",
			want:      "vehicle-framework",
		},
		{
			name:      "Single lowercase word",
			packageID: "brrainz.harmony",
			want:      "harmony",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got := DeriveSlug(tt.packageID, existing)
			if got != tt.want {
				t.Errorf("DeriveSlug(%q) = %q, want %q", tt.packageID, got, tt.want)
			}
		})
	}
}

func TestManifestPreservesComments(t *testing.T) {
	sampleYAML := `# Monorepo Mod Manifest
version: 1

# Official DLCs
dlcs:
  royalty: true
  ideology: false

# Community Mods
mods:
  harmony:
    name: "Harmony"
    source: steam_workshop
    workshop_id: "2009463077"
    package_id: "brrainz.harmony"
    enabled: true
`

	tempDir := t.TempDir()
	manifestPath := filepath.Join(tempDir, "mods.yaml")
	if err := os.WriteFile(manifestPath, []byte(sampleYAML), 0644); err != nil {
		t.Fatalf("failed to write test manifest: %v", err)
	}

	enabled := true
	newEntry := manifest.ModEntry{
		Name:        "Launch Sites Expire",
		Source:      "steam_workshop",
		WorkshopID:  "3525571078",
		PackageID:   "cass.LaunchSitesExpire",
		Enabled:     &enabled,
		Description: "A small mod that allows gravship launch sites to expire.",
	}

	if err := SaveManifestWithComments(manifestPath, "launch-sites-expire", newEntry); err != nil {
		t.Fatalf("SaveManifestWithComments failed: %v", err)
	}

	content, err := os.ReadFile(manifestPath)
	if err != nil {
		t.Fatalf("failed to read back manifest: %v", err)
	}
	text := string(content)

	// Verify comments are preserved
	if !strings.Contains(text, "# Monorepo Mod Manifest") {
		t.Errorf("Header comment lost")
	}
	if !strings.Contains(text, "# Official DLCs") {
		t.Errorf("DLC comment lost")
	}
	if !strings.Contains(text, "# Community Mods") {
		t.Errorf("Community mods comment lost")
	}

	// Verify new mod was added cleanly
	if !strings.Contains(text, "launch-sites-expire:") {
		t.Errorf("Expected launch-sites-expire key in YAML")
	}
	if !strings.Contains(text, "workshop_id: \"3525571078\"") && !strings.Contains(text, "workshop_id: 3525571078") {
		t.Errorf("Expected workshop_id in YAML")
	}

	// Verify it still cleanly unmarshals into manifest.Manifest
	mf, err := manifest.LoadManifest(manifestPath)
	if err != nil {
		t.Fatalf("failed to reload manifest into struct: %v", err)
	}
	if len(mf.Mods) != 2 {
		t.Fatalf("expected 2 mods, got %d", len(mf.Mods))
	}
	mod, ok := mf.Mods["launch-sites-expire"]
	if !ok {
		t.Fatalf("launch-sites-expire not found in parsed manifest")
	}
	if mod.PackageID != "cass.LaunchSitesExpire" {
		t.Errorf("expected packageID cass.LaunchSitesExpire, got %s", mod.PackageID)
	}
}
