package main

import (
	"fmt"

	"github.com/spf13/cobra"
	"rimworld-mods/internal/config"
	"rimworld-mods/internal/updater"
)

func newUpdateCmd(env *config.Environment) *cobra.Command {
	var dryRun bool
	var targetMod string

	cmd := &cobra.Command{
		Use:   "update",
		Short: "Check for upstream mod updates and synchronize lockfile",
		Long: `Queries Steam Workshop and GitHub for newer releases or commits,
updates manifests/mods.lock.yaml, and re-applies changes to RimWorld.`,
		RunE: func(cmd *cobra.Command, args []string) error {
			if dryRun {
				fmt.Printf("%s[DRY-RUN]%s Checking for upstream mod updates...\n\n", colorCyan, colorReset)
			} else {
				fmt.Printf("%sChecking for upstream mod updates...%s\n\n", colorBold, colorReset)
			}

			summary, err := updater.UpdateAll(env, targetMod, dryRun)
			if err != nil {
				return fmt.Errorf("mod update check failed: %w", err)
			}

			fmt.Printf("\n%sUpdate Summary:%s %d updated, %d unchanged",
				colorBold, colorReset, len(summary.Updated), len(summary.Unchanged))
			if len(summary.Errors) > 0 {
				fmt.Printf(", %s%d errors%s\n", colorRed, len(summary.Errors), colorReset)
			} else {
				fmt.Println()
			}

			if !dryRun && len(summary.Updated) > 0 {
				fmt.Printf("\n%s==> Running apply to reconcile updated mods...%s\n", colorBold, colorReset)
				applyCmd := newApplyCmd(env)
				applyCmd.SetArgs([]string{})
				return applyCmd.Execute()
			}

			return nil
		},
	}

	cmd.Flags().BoolVarP(&dryRun, "dry-run", "n", false, "Preview updates without modifying lockfile or downloading")
	cmd.Flags().StringVarP(&targetMod, "mod", "m", "", "Update specific mod identifier only")
	return cmd
}
