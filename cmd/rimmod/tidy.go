package main

import (
	"fmt"

	"github.com/spf13/cobra"
	"rimworld-mods/internal/config"
	"rimworld-mods/internal/tidy"
)

func newTidyCmd(env *config.Environment) *cobra.Command {
	var dryRun bool

	cmd := &cobra.Command{
		Use:   "tidy",
		Short: "Prune unreferenced lock entries, vendor cache, and dead symlinks",
		Long: `Cleans up monorepo state analogous to 'go mod tidy':
  1. Prunes entries from manifests/mods.lock.yaml no longer declared in mods.yaml
  2. Prunes unused cached downloads from mods/vendor/
  3. Prunes broken or orphaned symlinks from RimWorld's active Mods directory
  4. Runs 'go mod tidy' on the root repository toolchain`,
		RunE: func(cmd *cobra.Command, args []string) error {
			if dryRun {
				fmt.Printf("%s[DRY-RUN]%s Previewing monorepo tidy cleanup...\n\n", colorCyan, colorReset)
			} else {
				fmt.Printf("%sRunning monorepo tidy...%s\n\n", colorBold, colorReset)
			}

			report, err := tidy.RunTidy(env, dryRun)
			if err != nil {
				return err
			}

			if len(report.PrunedLockEntries) > 0 {
				fmt.Printf("%sPruned lockfile entries:%s %d\n", colorYellow, colorReset, len(report.PrunedLockEntries))
				for _, e := range report.PrunedLockEntries {
					fmt.Printf("  - %s\n", e)
				}
			} else {
				fmt.Println("No orphaned lockfile entries found.")
			}

			if len(report.PrunedVendorDirs) > 0 {
				fmt.Printf("\n%sPruned vendor directories:%s %d\n", colorYellow, colorReset, len(report.PrunedVendorDirs))
				for _, d := range report.PrunedVendorDirs {
					fmt.Printf("  - %s\n", d)
				}
			} else {
				fmt.Println("No orphaned vendor directories found.")
			}

			if len(report.PrunedSymlinks) > 0 {
				fmt.Printf("\n%sPruned dead/orphaned symlinks:%s %d\n", colorYellow, colorReset, len(report.PrunedSymlinks))
				for _, s := range report.PrunedSymlinks {
					fmt.Printf("  - %s\n", s)
				}
			} else {
				fmt.Println("No orphaned symlinks found.")
			}

			if report.GoModTidied {
				fmt.Printf("\n%s[OK] Go module dependencies verified & tidied.%s\n", colorGreen, colorReset)
			}

			fmt.Printf("\n%s[OK] Tidy complete.%s\n", colorGreen, colorReset)
			return nil
		},
	}

	cmd.Flags().BoolVarP(&dryRun, "dry-run", "n", false, "Preview cleanup actions without modifying files")
	return cmd
}
