package main

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"github.com/spf13/cobra"
	"rimworld-mods/internal/config"
	"rimworld-mods/internal/manifest"
	"rimworld-mods/internal/order"
)

func newOrderCmd(env *config.Environment) *cobra.Command {
	var dryRun bool
	var writeConfig bool

	cmd := &cobra.Command{
		Use:   "order",
		Short: "Inspect computed mod load order and DAG rules",
		Long: `Parses About.xml metadata and manifests/mods.yaml rules, computes
the topological load order (Harmony -> Core -> DLCs -> Mods -> Trailing),
and optionally writes ModsConfig.xml.`,
		RunE: func(cmd *cobra.Command, args []string) error {
			manifestPath := filepath.Join(env.RepoRoot, "manifests", "mods.yaml")
			mf, err := manifest.LoadManifest(manifestPath)
			if err != nil {
				return fmt.Errorf("failed to read manifest %s: %w", manifestPath, err)
			}

			sorter := order.NewSorter()
			vendorDir := filepath.Join(env.RepoRoot, "mods", "vendor")
			customDir := filepath.Join(env.RepoRoot, "mods", "custom")

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

			fmt.Printf("%sComputed RimWorld Mod Load Order (%d mods):%s\n\n", colorBold, len(res.OrderedMods), colorReset)
			fmt.Printf(" %-4s %-32s %-36s %s\n", "#", "MOD IDENTIFIER", "PACKAGE ID", "TIER")
			fmt.Println(" " + strings.Repeat("-", 85))

			for idx, node := range res.OrderedMods {
				tierLabel := "Standard"
				switch node.Tier {
				case order.TierHarmony:
					tierLabel = "Harmony (Anchor)"
				case order.TierCore:
					tierLabel = "Core (Anchor)"
				case order.TierDLC:
					tierLabel = "DLC (Expansion)"
				case order.TierTrailing:
					tierLabel = "Trailing (Late)"
				}
				name := node.Name
				if len(name) > 30 {
					name = name[:27] + "..."
				}
				fmt.Printf(" %-4d %-32s %-36s %s\n", idx+1, name, node.PackageID, tierLabel)
			}

			if len(res.Warnings) > 0 {
				fmt.Printf("\n%sWarnings:%s\n", colorYellow, colorReset)
				for _, w := range res.Warnings {
					fmt.Printf("  ! %s\n", w)
				}
			}

			if writeConfig {
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
					return err
				}

				backup, err := order.DeployModsConfig(configPath, xmlBytes, dryRun)
				if err != nil {
					return err
				}

				if dryRun {
					fmt.Printf("\n%s[DRY-RUN]%s Would write ModsConfig.xml to: %s\n", colorCyan, colorReset, configPath)
				} else {
					if backup != "" {
						fmt.Printf("\nCreated backup: %s\n", backup)
					}
					fmt.Printf("\n%s[OK] ModsConfig.xml updated:%s %s\n", colorGreen, colorReset, configPath)
				}
			}

			return nil
		},
	}

	cmd.Flags().BoolVarP(&dryRun, "dry-run", "n", false, "Preview without modifying filesystem")
	cmd.Flags().BoolVarP(&writeConfig, "write-config", "w", false, "Write computed load order to ModsConfig.xml")
	return cmd
}
