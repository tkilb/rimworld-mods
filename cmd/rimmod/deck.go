package main

import (
	"fmt"

	"github.com/spf13/cobra"
	"rimworld-mods/internal/config"
	"rimworld-mods/internal/deck"
)

func newDeckCmd(env *config.Environment) *cobra.Command {
	cmd := &cobra.Command{
		Use:   "deck",
		Short: "Steam Deck synchronization and Workshop cherry-pick operations",
		Long: `Manage Steam Deck synchronization:
  rimmod deck push  - Rsync monorepo state to Steam Deck and apply remotely
  rimmod deck pull  - Cherry-pick Steam Workshop mods subscribed on Deck into PC manifest`,
	}

	// rimmod deck push
	var pushDryRun bool
	var pushCheck bool
	pushCmd := &cobra.Command{
		Use:   "push",
		Short: "Rsync monorepo state to Steam Deck and deploy remote symlinks & config",
		RunE: func(cmd *cobra.Command, args []string) error {
			if pushCheck {
				fmt.Printf("Testing SSH connection to Steam Deck (%s)...\n", env.DeckHost)
				if err := deck.CheckConnection(env); err != nil {
					return err
				}
				fmt.Printf("%s[OK] Steam Deck SSH connection successful.%s\n", colorGreen, colorReset)
				return nil
			}

			if pushDryRun {
				fmt.Printf("%s[DRY-RUN]%s Previewing sync to Steam Deck (%s)...\n", colorCyan, colorReset, env.DeckHost)
			} else {
				fmt.Printf("%sSyncing monorepo state to Steam Deck (%s)...%s\n", colorBold, env.DeckHost, colorReset)
			}

			return deck.Push(env, pushDryRun, false)
		},
	}
	pushCmd.Flags().BoolVarP(&pushDryRun, "dry-run", "n", false, "Preview rsync transfer and remote actions")
	pushCmd.Flags().BoolVarP(&pushCheck, "check", "c", false, "Test SSH connection only and exit")

	// rimmod deck pull
	var pullDryRun bool
	var pullCopyFiles bool
	var pullShowAll bool
	pullCmd := &cobra.Command{
		Use:   "pull",
		Short: "Interactive TUI to cherry-pick Steam Workshop mods from Steam Deck",
		RunE: func(cmd *cobra.Command, args []string) error {
			return deck.Pull(env, pullDryRun, pullCopyFiles, pullShowAll)
		},
	}
	pullCmd.Flags().BoolVarP(&pullDryRun, "dry-run", "n", false, "Preview without modifying manifests/mods.yaml")
	pullCmd.Flags().BoolVar(&pullCopyFiles, "copy", false, "Copy mod files directly from Deck instead of using SteamCMD")
	pullCmd.Flags().BoolVar(&pullShowAll, "all", false, "Show all mods in the picker, including dependencies")

	cmd.AddCommand(pushCmd, pullCmd)
	return cmd
}
