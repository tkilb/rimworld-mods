package main

import (
	"fmt"

	"github.com/spf13/cobra"
	"rimworld-mods/internal/builder"
	"rimworld-mods/internal/config"
)

func newDevCmd(env *config.Environment) *cobra.Command {
	cmd := &cobra.Command{
		Use:   "dev",
		Short: "Private mod authoring and development workflows",
		Long: `Author and iterate on custom RimWorld mods:
  rimmod dev build <mod>    - Compile C# assemblies via dotnet build and re-apply
  rimmod dev scaffold <mod> - Generate starter template for new XML or C# mod`,
	}

	// rimmod dev build <mod>
	var buildDryRun bool
	var configuration string
	var skipApply bool

	buildCmd := &cobra.Command{
		Use:   "build [mod]",
		Short: "Compile C# assemblies for custom mod and automatically re-apply",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			modName := args[0]
			if buildDryRun {
				fmt.Printf("%s[DRY-RUN]%s Compiling custom mod '%s'...\n", colorCyan, colorReset, modName)
			} else {
				fmt.Printf("%sCompiling custom mod '%s'...%s\n", colorBold, modName, colorReset)
			}

			if err := builder.BuildMod(env, modName, configuration, buildDryRun); err != nil {
				return err
			}

			fmt.Printf("%s[OK] Build completed successfully.%s\n", colorGreen, colorReset)

			if !skipApply && !buildDryRun {
				fmt.Printf("\n%s==> Automatically reconciling local RimWorld installation...%s\n", colorBold, colorReset)
				applyCmd := newApplyCmd(env)
				applyCmd.SetArgs([]string{})
				return applyCmd.Execute()
			}

			return nil
		},
	}
	buildCmd.Flags().BoolVarP(&buildDryRun, "dry-run", "n", false, "Preview build command without compiling")
	buildCmd.Flags().StringVarP(&configuration, "config", "c", "Release", "Build configuration (Debug or Release)")
	buildCmd.Flags().BoolVar(&skipApply, "no-apply", false, "Skip automatic link and order reconciliation after build")

	// rimmod dev scaffold <name>
	var scaffoldType string
	var scaffoldDryRun bool
	scaffoldCmd := &cobra.Command{
		Use:   "scaffold <name>",
		Short: "Scaffold a new custom mod starter template",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			name := args[0]
			return builder.ScaffoldMod(env, name, scaffoldType, scaffoldDryRun)
		},
	}
	scaffoldCmd.Flags().StringVarP(&scaffoldType, "type", "t", "xml", "Mod type: 'xml' or 'csharp'")
	scaffoldCmd.Flags().BoolVarP(&scaffoldDryRun, "dry-run", "n", false, "Preview scaffolding without creating files")

	cmd.AddCommand(buildCmd, scaffoldCmd)
	return cmd
}
