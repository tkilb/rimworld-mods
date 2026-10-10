package builder

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"

	"rimworld-mods/internal/config"
)

// BuildMod compiles C# assemblies for a custom mod using dotnet build.
func BuildMod(env *config.Environment, modName string, configuration string, dryRun bool) error {
	if configuration == "" {
		configuration = "Release"
	}

	modDir := filepath.Join(env.RepoRoot, "mods", "custom", modName)
	if fi, err := os.Stat(modDir); err != nil || !fi.IsDir() {
		return fmt.Errorf("custom mod directory not found: %s", modDir)
	}

	// Find .csproj in mod directory or Source/
	var csprojPath string
	matches, _ := filepath.Glob(filepath.Join(modDir, "*.csproj"))
	if len(matches) > 0 {
		csprojPath = matches[0]
	} else {
		matches, _ = filepath.Glob(filepath.Join(modDir, "Source", "*.csproj"))
		if len(matches) > 0 {
			csprojPath = matches[0]
		}
	}

	if csprojPath == "" {
		return fmt.Errorf("no .csproj file found in %s or %s/Source", modDir, modDir)
	}

	args := []string{"build", csprojPath, "-c", configuration}

	if dryRun {
		fmt.Printf("  [DRY-RUN] Would run: dotnet %v\n", args)
		return nil
	}

	cmd := exec.Command("dotnet", args...)
	cmd.Dir = modDir
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	cmd.Env = os.Environ()

	if err := cmd.Run(); err != nil {
		return fmt.Errorf("dotnet build failed: %w", err)
	}

	return nil
}

// ScaffoldMod invokes scripts/scaffold-mod.sh.
func ScaffoldMod(env *config.Environment, name string, modType string, dryRun bool) error {
	scriptPath := filepath.Join(env.RepoRoot, "scripts", "scaffold-mod.sh")
	args := []string{scriptPath, "--name", name}
	if modType != "" {
		args = append(args, "--type", modType)
	}
	if dryRun {
		args = append(args, "--dry-run")
	}

	cmd := exec.Command("bash", args...)
	cmd.Dir = env.RepoRoot
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	cmd.Env = os.Environ()

	return cmd.Run()
}
