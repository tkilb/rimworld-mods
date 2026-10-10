package order

import (
	"strings"
	"testing"

	"rimworld-mods/internal/manifest"
)

func TestTopologicalSorter(t *testing.T) {
	tests := []struct {
		name          string
		nodes         []*ModNode
		availableDLCs []string
		wantOrder     []string
		wantErr       bool
		errContains   string
		wantWarnings  int
	}{
		{
			name: "Strict anchor tiers: Harmony -> Core -> DLCs -> Mods",
			nodes: []*ModNode{
				BuildModNode("cloning", manifest.ModEntry{PackageID: "zal.cloning", Name: "Cloning"}, nil, "", false),
				BuildModNode("harmony", manifest.ModEntry{PackageID: "brrainz.harmony", Name: "Harmony"}, nil, "", false),
			},
			availableDLCs: []string{"ludeon.rimworld.biotech", "ludeon.rimworld.royalty"},
			wantOrder: []string{
				"brrainz.harmony",
				"ludeon.rimworld",
				"ludeon.rimworld.royalty",
				"ludeon.rimworld.biotech",
				"zal.cloning",
			},
			wantErr: false,
		},
		{
			name: "All canonical DLCs including Odyssey sorted correctly",
			nodes: []*ModNode{
				BuildModNode("cloning", manifest.ModEntry{PackageID: "zal.cloning", Name: "Cloning"}, nil, "", false),
			},
			availableDLCs: []string{"ludeon.rimworld.odyssey", "ludeon.rimworld.biotech", "ludeon.rimworld.royalty", "ludeon.rimworld.anomaly", "ludeon.rimworld.ideology"},
			wantOrder: []string{
				"ludeon.rimworld",
				"ludeon.rimworld.royalty",
				"ludeon.rimworld.ideology",
				"ludeon.rimworld.biotech",
				"ludeon.rimworld.anomaly",
				"ludeon.rimworld.odyssey",
				"zal.cloning",
			},
			wantErr: false,
		},
		{
			name: "Standard dependencies with LoadAfter and LoadBefore",
			nodes: []*ModNode{
				BuildModNode("modA", manifest.ModEntry{PackageID: "author.modA", Name: "Mod A"}, &ModMetaData{
					LoadBefore: []string{"author.modB"},
				}, "", false),
				BuildModNode("modB", manifest.ModEntry{PackageID: "author.modB", Name: "Mod B"}, nil, "", false),
				BuildModNode("modC", manifest.ModEntry{PackageID: "author.modC", Name: "Mod C"}, &ModMetaData{
					LoadAfter: []string{"author.modB"},
				}, "", false),
			},
			availableDLCs: nil,
			wantOrder: []string{
				"ludeon.rimworld",
				"author.modA",
				"author.modB",
				"author.modC",
			},
			wantErr: false,
		},
		{
			name: "Manifest overrides (OrderAfter and Priority)",
			nodes: []*ModNode{
				BuildModNode("mod1", manifest.ModEntry{
					PackageID:  "test.mod1",
					Name:       "Mod 1",
					OrderAfter: []string{"test.mod2"},
				}, nil, "", false),
				BuildModNode("mod2", manifest.ModEntry{
					PackageID: "test.mod2",
					Name:      "Mod 2",
				}, nil, "", false),
				BuildModNode("mod3", manifest.ModEntry{
					PackageID: "test.mod3",
					Name:      "Mod 3",
					Priority:  100,
				}, nil, "", false),
			},
			availableDLCs: nil,
			wantOrder: []string{
				"ludeon.rimworld",
				"test.mod3",
				"test.mod2",
				"test.mod1",
			},
			wantErr: false,
		},
		{
			name: "Trailing mods placed after standard mods",
			nodes: []*ModNode{
				BuildModNode("rocketman", manifest.ModEntry{
					PackageID: "krkr.rocketman",
					Name:      "RocketMan",
				}, nil, "", false),
				BuildModNode("somemod", manifest.ModEntry{
					PackageID: "user.somemod",
					Name:      "Some Mod",
				}, nil, "", false),
			},
			availableDLCs: nil,
			wantOrder: []string{
				"ludeon.rimworld",
				"user.somemod",
				"krkr.rocketman",
			},
			wantErr: false,
		},
		{
			name: "Circular dependency detection",
			nodes: []*ModNode{
				BuildModNode("cycleA", manifest.ModEntry{
					PackageID:  "cycle.modA",
					Name:       "Cycle A",
					OrderAfter: []string{"cycle.modB"},
				}, nil, "", false),
				BuildModNode("cycleB", manifest.ModEntry{
					PackageID:  "cycle.modB",
					Name:       "Cycle B",
					OrderAfter: []string{"cycle.modA"},
				}, nil, "", false),
			},
			availableDLCs: nil,
			wantErr:       true,
			errContains:   "circular dependency detected",
		},
		{
			name: "Missing dependencies and incompatibility warnings",
			nodes: []*ModNode{
				BuildModNode("modWithDeps", manifest.ModEntry{
					PackageID: "test.withdeps",
					Name:      "With Deps",
				}, &ModMetaData{
					ModDependencies: []ModDependency{
						{PackageID: "nonexistent.dependency", DisplayName: "Missing Lib"},
					},
					IncompatibleWith: []string{"test.rival"},
				}, "", false),
				BuildModNode("rival", manifest.ModEntry{
					PackageID: "test.rival",
					Name:      "Rival Mod",
				}, nil, "", false),
			},
			availableDLCs: nil,
			wantOrder: []string{
				"ludeon.rimworld",
				"test.withdeps",
				"test.rival",
			},
			wantErr:      false,
			wantWarnings: 2,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			s := NewSorter()
			for _, n := range tt.nodes {
				s.AddNode(n)
			}

			res, err := s.Sort(tt.availableDLCs)
			if (err != nil) != tt.wantErr {
				t.Fatalf("Sort() error = %v, wantErr %v", err, tt.wantErr)
			}

			if tt.wantErr {
				if tt.errContains != "" && !strings.Contains(err.Error(), tt.errContains) {
					t.Errorf("expected error containing %q, got %q", tt.errContains, err.Error())
				}
				return
			}

			if tt.wantWarnings > 0 && len(res.Warnings) != tt.wantWarnings {
				t.Errorf("warnings count = %d, want %d (warnings: %v)", len(res.Warnings), tt.wantWarnings, res.Warnings)
			}

			var gotOrder []string
			for _, m := range res.OrderedMods {
				gotOrder = append(gotOrder, strings.ToLower(m.PackageID))
			}

			if len(gotOrder) != len(tt.wantOrder) {
				t.Fatalf("got order len %d (%v), want %d (%v)", len(gotOrder), gotOrder, len(tt.wantOrder), tt.wantOrder)
			}

			for i := range gotOrder {
				if gotOrder[i] != strings.ToLower(tt.wantOrder[i]) {
					t.Errorf("at index %d: got %s, want %s\nFull order: %v", i, gotOrder[i], tt.wantOrder[i], gotOrder)
				}
			}
		})
	}
}
