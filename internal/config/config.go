package config

import (
	"bufio"
	"os"
	"path/filepath"
	"strings"
)

// Environment holds the resolved paths and settings for the local machine.
type Environment struct {
	Machine          string
	RepoRoot         string
	RimWorldModsDir  string
	RimWorldConfigDir string
	DeckHost         string
	DeckRemoteDir    string
	SteamCMDBin      string
}

// FindRepoRoot searches for the rimworld-mods repository root.
// It checks RIMWORLD_MODS_REPO, walks upward from the current directory looking for
// manifests/mods.yaml, and falls back to known standard repository paths.
func FindRepoRoot() (string, error) {
	if env := os.Getenv("RIMWORLD_MODS_REPO"); env != "" {
		if fi, err := os.Stat(filepath.Join(env, "manifests", "mods.yaml")); err == nil && !fi.IsDir() {
			return env, nil
		}
	}

	cwd, err := os.Getwd()
	if err == nil {
		dir := cwd
		for {
			if fi, err := os.Stat(filepath.Join(dir, "manifests", "mods.yaml")); err == nil && !fi.IsDir() {
				return dir, nil
			}
			parent := filepath.Dir(dir)
			if parent == dir {
				break
			}
			dir = parent
		}
	}

	if home, err := os.UserHomeDir(); err == nil {
		candidates := []string{
			filepath.Join(home, "Git", "rimworld-mods"),
			filepath.Join(home, "src", "rimworld-mods"),
			filepath.Join(home, ".local", "share", "rimworld-mods"),
		}
		for _, c := range candidates {
			if fi, err := os.Stat(filepath.Join(c, "manifests", "mods.yaml")); err == nil && !fi.IsDir() {
				return c, nil
			}
		}
	}

	if cwd != "" {
		return cwd, nil
	}
	return ".", nil
}

// LoadEnvironment resolves the active configuration by reading environment variables,
// config/local.env, and system defaults based on machine type.
func LoadEnvironment(repoRoot string) (*Environment, error) {
	if repoRoot == "" {
		var err error
		repoRoot, err = FindRepoRoot()
		if err != nil {
			return nil, err
		}
	}

	envVars := make(map[string]string)

	// 1. Try reading ~/.profile for MACHINE if not already in env
	if os.Getenv("MACHINE") == "" {
		home, err := os.UserHomeDir()
		if err == nil {
			profilePath := filepath.Join(home, ".profile")
			readSimpleEnvFile(profilePath, envVars)
		}
	}

	// 2. Read config/local.env if present
	localEnvPath := filepath.Join(repoRoot, "config", "local.env")
	readSimpleEnvFile(localEnvPath, envVars)

	// Helper to resolve with priority: process environment > local.env > fallback
	getVal := func(key, fallback string) string {
		if val := os.Getenv(key); val != "" {
			return val
		}
		if val, ok := envVars[key]; ok && val != "" {
			return val
		}
		return fallback
	}

	machine := getVal("MACHINE", "unknown")
	home, _ := os.UserHomeDir()

	env := &Environment{
		Machine:       machine,
		RepoRoot:      repoRoot,
		DeckHost:      getVal("DECK_HOST", "steamdeck"),
		DeckRemoteDir: getVal("DECK_REMOTE_DIR", "~/.local/share/rimworld-mods"),
		SteamCMDBin:   getVal("STEAMCMD_BIN", "steamcmd"),
	}

	// Machine-specific path fallbacks
	switch machine {
	case "linux-box":
		env.RimWorldModsDir = getVal("RIMWORLD_MODS_DIR", "/mnt/gaming/SteamLibrary/steamapps/common/RimWorld/Mods")
		env.RimWorldConfigDir = getVal("RIMWORLD_CONFIG_DIR", filepath.Join(home, ".config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config"))
	case "linux-book":
		env.RimWorldModsDir = getVal("RIMWORLD_MODS_DIR", filepath.Join(home, ".local/share/Steam/steamapps/common/RimWorld/Mods"))
		env.RimWorldConfigDir = getVal("RIMWORLD_CONFIG_DIR", filepath.Join(home, ".config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config"))
	case "steam-deck":
		// Detect MicroSD or fallback to internal SSD
		modsDir := getVal("RIMWORLD_MODS_DIR", "")
		if modsDir == "" {
			sdMods := findSteamDeckSDMods()
			if sdMods != "" {
				modsDir = sdMods
			} else {
				modsDir = filepath.Join(home, ".local/share/Steam/steamapps/common/RimWorld/Mods")
			}
		}
		env.RimWorldModsDir = modsDir

		// Config directory: check Proton compatdata first, then native
		cfgDir := getVal("RIMWORLD_CONFIG_DIR", "")
		if cfgDir == "" {
			protonCfg := filepath.Join(home, ".local/share/Steam/steamapps/compatdata/294100/pfx/drive_c/users/steamuser/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Config")
			nativeCfg := filepath.Join(home, ".config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config")
			if fi, err := os.Stat(protonCfg); err == nil && fi.IsDir() {
				cfgDir = protonCfg
			} else if fi, err := os.Stat(nativeCfg); err == nil && fi.IsDir() {
				cfgDir = nativeCfg
			} else {
				cfgDir = protonCfg
			}
		}
		env.RimWorldConfigDir = cfgDir
	default:
		// Generic Linux default
		env.RimWorldModsDir = getVal("RIMWORLD_MODS_DIR", filepath.Join(home, ".local/share/Steam/steamapps/common/RimWorld/Mods"))
		env.RimWorldConfigDir = getVal("RIMWORLD_CONFIG_DIR", filepath.Join(home, ".config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config"))
	}

	return env, nil
}

func readSimpleEnvFile(path string, target map[string]string) {
	file, err := os.Open(path)
	if err != nil {
		return
	}
	defer file.Close()

	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		line := strings.TrimSpace(scanner.Text())
		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}
		parts := strings.SplitN(line, "=", 2)
		if len(parts) == 2 {
			k := strings.TrimSpace(parts[0])
			v := strings.TrimSpace(parts[1])
			v = strings.Trim(v, `"'`)
			if _, exists := target[k]; !exists {
				target[k] = v
			}
		}
	}
}

func findSteamDeckSDMods() string {
	matches, err := filepath.Glob("/run/media/*/*/steamapps/common/RimWorld/Mods")
	if err == nil && len(matches) > 0 {
		return matches[0]
	}
	return ""
}
