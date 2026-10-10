package main

import (
	"fmt"
	"os"

	"github.com/spf13/cobra"
	"rimworld-mods/internal/config"
)

func main() {
	env, err := config.LoadEnvironment("")
	if err != nil {
		fmt.Fprintf(os.Stderr, "Error initializing environment: %v\n", err)
		os.Exit(1)
	}

	rootCmd := &cobra.Command{
		Use:   "rimmod",
		Short: "RimWorld Mod Manager CLI",
		Long: `rimmod is the unified operational CLI for managing, sorting, linking, and
synchronizing RimWorld mods across Arch Linux workstations and Steam Deck.`,
		// If no arguments or subcommands are provided, display help and exit cleanly
		Run: func(cmd *cobra.Command, args []string) {
			_ = cmd.Help()
		},
	}

	// Register subcommands
	rootCmd.AddCommand(
		newApplyCmd(env),
		newTidyCmd(env),
		newUpdateCmd(env),
		newOrderCmd(env),
		newDeckCmd(env),
		newDevCmd(env),
		newStatusCmd(env),
	)

	// Ensure Cobra's default help command is available as 'rimmod help [subcmd]'
	rootCmd.SetHelpCommand(&cobra.Command{
		Use:   "help [command]",
		Short: "Help about any command",
		Long:  `Help provides help for any command in the application. Simply type 'rimmod help [path to command]' for full details.`,
		Run: func(c *cobra.Command, args []string) {
			cmd, _, e := c.Root().Find(args)
			if cmd == nil || e != nil {
				c.Printf("Unknown help topic %#q\n", args)
				_ = c.Root().Help()
			} else {
				cmd.InitDefaultHelpFlag()
				_ = cmd.Help()
			}
		},
	})

	if err := rootCmd.Execute(); err != nil {
		os.Exit(1)
	}
}
