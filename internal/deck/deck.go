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
	cmd := exec.Command("ssh", "-o", "ConnectTimeout=5", "-o", "BatchMode=yes", env.DeckHost, "echo ok")
	cmd.Env = os.Environ()
	out, err := cmd.CombinedOutput()
	if err != nil {
		return fmt.Errorf("cannot reach %s via SSH: %s (%w)", env.DeckHost, string(out), err)
	}
	return nil
}

// Push runs the Steam Deck synchronization script (sync-deck.sh).
func Push(env *config.Environment, dryRun bool, checkOnly bool) error {
	scriptPath := filepath.Join(env.RepoRoot, "scripts", "sync-deck.sh")
	args := []string{scriptPath}
	if dryRun {
		args = append(args, "--dry-run")
	}
	if checkOnly {
		args = append(args, "--check")
	}

	cmd := exec.Command("bash", args...)
	cmd.Dir = env.RepoRoot
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	cmd.Env = os.Environ()

	if err := cmd.Run(); err != nil {
		return fmt.Errorf("deck push failed: %w", err)
	}
	return nil
}

// Pull runs the Steam Deck cherry-pick importer (import-deck.sh).
func Pull(env *config.Environment, dryRun bool, copyFiles bool, showAll bool) error {
	scriptPath := filepath.Join(env.RepoRoot, "scripts", "import-deck.sh")
	args := []string{scriptPath}
	if dryRun {
		args = append(args, "--dry-run")
	}
	if copyFiles {
		args = append(args, "--copy-files")
	}
	if showAll {
		args = append(args, "--show-all")
	}

	cmd := exec.Command("bash", args...)
	cmd.Dir = env.RepoRoot
	cmd.Stdin = os.Stdin
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	cmd.Env = os.Environ()

	if err := cmd.Run(); err != nil {
		return fmt.Errorf("deck pull failed: %w", err)
	}
	return nil
}
