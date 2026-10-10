package main

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"github.com/spf13/cobra"
	"rimworld-mods/internal/config"
	"rimworld-mods/internal/fetcher"
	"rimworld-mods/internal/linker"
	"rimworld-mods/internal/manifest"
	"rimworld-mods/internal/order"
)

// ANSI colors
const (
	colorReset  = "\033[0m"
	colorBold   = "\033[1m"
	colorRed    = "\033[31m"
	colorGreen  = "\033[32m"
	colorYellow = "\033[33m"
	colorCyan   = "\033[36m"
	colorGray   = "\033[90m"
)

func newApplyCmd(env *config.Environment) *cobra.Command {
	var dryRun bool

	cmd := &cobra.Command{
		Use:   "apply",
		Short: "Reconcile local RimWorld installation to declared state",
		Long: `Reconciles your local RimWorld installation against manifests/mods.yaml:
  1. Fetches any declared mods missing from cache (offline if all exist)
  2. Deploys symlinks into RimWorld Mods directory
  3. Topologically sorts active mods from About.xml rules
  4. Deploys generated ModsConfig.xml directly to RimWorld config directory`,
		RunE: func(cmd *cobra.Command, args []string) error {
			if dryRun {
				fmt.Printf("%s[DRY-RUN]%s Previewing full local reconciliation...\n\n", colorCyan, colorReset)
			}

			manifestPath := filepath.Join(env.RepoRoot, "manifests", "mods.yaml")
			mf, err := manifest.LoadManifest(manifestPath)
			if err != nil {
				return fmt.Errorf("failed to read manifest %s: %w", manifestPath, err)
			}

			// Step 1: Ensure missing mods exist (Offline-by-default)
			fmt.Printf("%s==> Step 1: Checking mod cache...%s\n", colorBold, colorReset)
			missing := fetcher.FindMissingMods(env, mf)
			if len(missing) > 0 {
				fmt.Printf("Found %d missing mod(s): %s\n", len(missing), strings.Join(missing, ", "))
				for _, m := range missing {
					if dryRun {
						fmt.Printf("  %s[DRY-RUN]%s Would fetch missing mod: %s\n", colorCyan, colorReset, m)
					} else {
						fmt.Printf("  Fetching missing mod: %s...\n", m)
						if err := fetcher.FetchMod(env, m, mf.Mods[m], false); err != nil {
							return fmt.Errorf("failed fetching %s: %w", m, err)
						}
					}
				}
			} else {
				fmt.Printf("  %sAll declared mods are present in cache.%s\n", colorGreen, colorReset)
			}

			// Step 2: Deploy symlinks
			fmt.Printf("\n%s==> Step 2: Reconciling mod symlinks...%s\n", colorBold, colorReset)
			l := linker.NewLinker(env, mf)
			linkResults, err := l.LinkAll(dryRun)
			if err != nil {
				return fmt.Errorf("symlink reconciliation failed: %w", err)
			}

			for _, res := range linkResults {
				switch res.Status {
				case linker.StatusCreated:
					fmt.Printf("  %s+ Linked:%s %s\n", colorGreen, colorReset, res.ModID)
				case linker.StatusUpdated:
					fmt.Printf("  %s~ Updated:%s %s\n", colorYellow, colorReset, res.ModID)
				case linker.StatusUpToDate:
					fmt.Printf("  %s= Up-to-date:%s %s\n", colorGray, colorReset, res.ModID)
				case linker.StatusUnlinked:
					fmt.Printf("  %s- Removed (unmanaged):%s %s\n", colorYellow, colorReset, res.ModID)
				case linker.StatusSkipped:
					fmt.Printf("  %s! Skipped:%s %s (%s)\n", colorYellow, colorReset, res.ModID, res.Message)
				case linker.StatusError:
					fmt.Printf("  %s✖ Error:%s %s (%s)\n", colorRed, colorReset, res.ModID, res.Message)
				}
			}

			// Step 3: Topologically sort mods
			fmt.Printf("\n%s==> Step 3: Computing topological mod load order...%s\n", colorBold, colorReset)
			sorter := order.NewSorter()
			vendorDir := filepath.Join(env.RepoRoot, "mods", "vendor")
			customDir := filepath.Join(env.RepoRoot, "mods", "custom")

			// Add declared vendor mods
			for modID, entry := range mf.Mods {
				if !entry.IsEnabled() || entry.Source == "custom" || entry.Source == "local" {
					continue
				}
				modDirPath := filepath.Join(vendorDir, modID)
				var meta *order.ModMetaData
				if fi, err := os.Stat(modDirPath); err == nil && fi.IsDir() {
					aboutPath, err := order.FindAboutXML(modDirPath)
					if err == nil {
						if parsed, err := order.ParseAboutXML(aboutPath); err == nil {
							meta = parsed
						}
					}
				}
				node := order.BuildModNode(modID, entry, meta, modDirPath, false)
				sorter.AddNode(node)
			}

			// Auto-discover custom mods
			if fi, err := os.Stat(customDir); err == nil && fi.IsDir() {
				entries, _ := os.ReadDir(customDir)
				for _, e := range entries {
					if !e.IsDir() || strings.HasPrefix(e.Name(), ".") {
						continue
					}
					customID := e.Name()
					if entry, ok := mf.Mods[customID]; ok && !entry.IsEnabled() {
						continue
					}
					cDir := filepath.Join(customDir, customID)
					var meta *order.ModMetaData
					if aboutPath, err := order.FindAboutXML(cDir); err == nil {
						if parsed, err := order.ParseAboutXML(aboutPath); err == nil {
							meta = parsed
						}
					}
					var entry manifest.ModEntry
					if existing, ok := mf.Mods[customID]; ok {
						entry = existing
					}
					node := order.BuildModNode(customID, entry, meta, cDir, true)
					sorter.AddNode(node)
				}
			}

			// Filter available DLCs
			var activeDLCs []string
			for _, dlcPkg := range order.CanonicalDLCs {
				if mf.IsDLCEnabled(dlcPkg) {
					activeDLCs = append(activeDLCs, dlcPkg)
				}
			}

			res, err := sorter.Sort(activeDLCs)
			if err != nil {
				return fmt.Errorf("mod load order resolution failed: %w", err)
			}

			for _, warn := range res.Warnings {
				fmt.Printf("  %s[WARN]%s %s\n", colorYellow, colorReset, warn)
			}

			// Step 4: Write ModsConfig.xml
			fmt.Printf("\n%s==> Step 4: Updating RimWorld ModsConfig.xml...%s\n", colorBold, colorReset)
			configPath := filepath.Join(env.RimWorldConfigDir, "ModsConfig.xml")
			existingVersion := order.ReadExistingVersion(configPath)
			if existingVersion == "" {
				existingVersion = "1.5.4297 rev513"
			}

			var activePkgs []string
			for _, node := range res.OrderedMods {
				activePkgs = append(activePkgs, node.NormalizedPackageID())
			}

			xmlBytes, err := order.GenerateModsConfigXML(activePkgs, activeDLCs, existingVersion)
			if err != nil {
				return fmt.Errorf("failed generating ModsConfig.xml: %w", err)
			}

			backup, err := order.DeployModsConfig(configPath, xmlBytes, dryRun)
			if err != nil {
				return fmt.Errorf("failed writing ModsConfig.xml: %w", err)
			}

			if dryRun {
				fmt.Printf("  %s[DRY-RUN]%s Would write %d active mods to: %s\n", colorCyan, colorReset, len(activePkgs), configPath)
			} else {
				if backup != "" {
					fmt.Printf("  Created backup: %s\n", backup)
				}
				fmt.Printf("  %sSuccessfully deployed %d active mods to:%s %s\n", colorGreen, len(activePkgs), colorReset, configPath)
			}

			fmt.Printf("\n%s[OK] Local reconciliation complete.%s\n", colorGreen, colorReset)
			return nil
		},
	}

	cmd.Flags().BoolVarP(&dryRun, "dry-run", "n", false, "Preview actions without modifying files")
	return cmd
}
