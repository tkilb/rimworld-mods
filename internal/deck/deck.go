package deck

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"

	"rimworld-mods/internal/config"
)

// CheckConnection tests SSH connection to the Steam Deck.
func CheckConnection(env *config.Environment) error {
	deckHost := env.DeckHost
	if deckHost == "" {
		deckHost = "steamdeck"
	}

	cmd := exec.Command("ssh", "-o", "ConnectTimeout=5", "-o", "BatchMode=yes", deckHost, "echo ok")
	cmd.Env = os.Environ()
	out, err := cmd.CombinedOutput()
	if err != nil {
		return fmt.Errorf("cannot reach %s via SSH: %s (%w)", deckHost, string(out), err)
	}
	return nil
}

// Push synchronizes the monorepo manifests, vendor cache, and custom mods to Steam Deck,
// then triggers remote reconciliation via 'rimmod apply'.
func Push(env *config.Environment, dryRun bool, checkOnly bool) error {
	deckHost := env.DeckHost
	if deckHost == "" {
		deckHost = "steamdeck"
	}
	deckRemoteDir := env.DeckRemoteDir
	if deckRemoteDir == "" {
		deckRemoteDir = "~/.local/share/rimworld-mods"
	}

	fmt.Printf("Steam Deck host: %s  (resolved via ~/.ssh/config)\n", deckHost)
	fmt.Printf("Remote directory: %s\n", deckRemoteDir)

	fmt.Printf("Testing SSH connection to %s...\n", deckHost)
	if err := CheckConnection(env); err != nil {
		return err
	}
	fmt.Printf("\033[32m[OK]\033[0m SSH connection OK.\n")

	if checkOnly {
		return nil
	}

	// Ensure bin/rimmod binary is freshly compiled for target architecture before sync
	binPath := filepath.Join(env.RepoRoot, "bin", "rimmod")
	fmt.Println("Compiling rimmod binary before sync...")
	buildCmd := exec.Command("go", "build", "-o", binPath, "./cmd/rimmod")
	buildCmd.Dir = env.RepoRoot
	buildCmd.Env = append(os.Environ(), "GOOS=linux", "GOARCH=amd64")
	if err := buildCmd.Run(); err != nil {
		return fmt.Errorf("failed compiling rimmod for target: %w", err)
	}

	if dryRun {
		fmt.Printf("[DRY-RUN] Would ensure remote directory: %s\n", deckRemoteDir)
	} else {
		mkdirCmd := exec.Command("ssh", deckHost, fmt.Sprintf("mkdir -p %s", deckRemoteDir))
		if err := mkdirCmd.Run(); err != nil {
			return fmt.Errorf("failed to create remote directory %s: %w", deckRemoteDir, err)
		}
	}

	rsyncArgs := []string{
		"-avz",
		"--delete",
		"-e", "ssh",
		"--exclude=.git/",
		"--exclude=.git*",
		"--exclude=config/local.env",
		"--exclude=.cache/",
		"--exclude=tmp/",
		"--exclude=*.swp",
		"--exclude=*~",
		"--exclude=.DS_Store",
	}

	if dryRun {
		rsyncArgs = append(rsyncArgs, "--dry-run")
		fmt.Printf("[DRY-RUN] Previewing rsync to %s:%s...\n", deckHost, deckRemoteDir)
	} else {
		fmt.Printf("Syncing monorepo to %s:%s...\n", deckHost, deckRemoteDir)
	}

	rsyncArgs = append(rsyncArgs, env.RepoRoot+"/", fmt.Sprintf("%s:%s/", deckHost, deckRemoteDir))

	rsyncCmd := exec.Command("rsync", rsyncArgs...)
	rsyncCmd.Stdout = os.Stdout
	rsyncCmd.Stderr = os.Stderr
	if err := rsyncCmd.Run(); err != nil {
		return fmt.Errorf("rsync sync failed: %w", err)
	}

	// Ensure remote config/local.env exists on Deck
	if !dryRun {
		checkEnvCmd := exec.Command("ssh", deckHost, fmt.Sprintf("test -f %s/config/local.env", deckRemoteDir))
		if err := checkEnvCmd.Run(); err != nil {
			fmt.Printf("Initializing remote config/local.env on %s...\n", deckHost)
			cpCmd := exec.Command("ssh", deckHost, fmt.Sprintf("cp %s/config/env.steamdeck.env %s/config/local.env", deckRemoteDir, deckRemoteDir))
			_ = cpCmd.Run()
		}
	}

	// Trigger remote reconciliation on Deck
	if dryRun {
		fmt.Printf("[DRY-RUN] Would run './bin/rimmod apply --dry-run' on %s in %s\n", deckHost, deckRemoteDir)
	} else {
		fmt.Printf("Reconciling remote RimWorld installation on %s...\n", deckHost)
		remoteCmd := exec.Command("ssh", deckHost, fmt.Sprintf("cd %s && ./bin/rimmod apply", deckRemoteDir))
		remoteCmd.Stdout = os.Stdout
		remoteCmd.Stderr = os.Stderr
		if err := remoteCmd.Run(); err != nil {
			return fmt.Errorf("remote rimmod apply failed: %w", err)
		}
		fmt.Printf("\033[32m[OK]\033[0m Steam Deck synchronization complete.\n")
	}

	return nil
}

