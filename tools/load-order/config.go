package main

import (
	"encoding/xml"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"strings"
)

// ModsConfigData models RimWorld's ModsConfig.xml file.
type ModsConfigData struct {
	XMLName         xml.Name `xml:"ModsConfigData"`
	Version         string   `xml:"version,omitempty"`
	ActiveMods      []string `xml:"activeMods>li"`
	KnownExpansions []string `xml:"knownExpansions>li"`
}

// GenerateModsConfigXML constructs the ModsConfig.xml byte representation.
func GenerateModsConfigXML(activeMods []string, knownExpansions []string, version string) ([]byte, error) {
	data := ModsConfigData{
		Version:         version,
		ActiveMods:      activeMods,
		KnownExpansions: knownExpansions,
	}

	raw, err := xml.MarshalIndent(data, "", "  ")
	if err != nil {
		return nil, fmt.Errorf("failed to marshal ModsConfigData: %w", err)
	}

	header := []byte("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n")
	return append(header, append(raw, '\n')...), nil
}

// ReadExistingVersion reads the <version> tag from an existing ModsConfig.xml,
// or falls back to reading LastPlayedVersion.txt in the same directory.
func ReadExistingVersion(configPath string) string {
	if configPath == "" {
		return ""
	}

	if data, err := os.ReadFile(configPath); err == nil {
		var cfg ModsConfigData
		if err := xml.Unmarshal(data, &cfg); err == nil && cfg.Version != "" {
			return cfg.Version
		}
	}

	lastPlayed := filepath.Join(filepath.Dir(configPath), "LastPlayedVersion.txt")
	if data, err := os.ReadFile(lastPlayed); err == nil {
		v := strings.TrimSpace(string(data))
		if v != "" {
			return v
		}
	}

	return ""
}

// DeployModsConfig writes the ModsConfig.xml atomically to destPath, creating a backup if it already exists.
func DeployModsConfig(destPath string, content []byte, dryRun bool) (backupPath string, err error) {
	destDir := filepath.Dir(destPath)

	if _, statErr := os.Stat(destPath); statErr == nil {
		backupPath = destPath + ".bak"
	}

	if dryRun {
		return backupPath, nil
	}

	if err := os.MkdirAll(destDir, 0755); err != nil {
		return "", fmt.Errorf("failed to create directory %s: %w", destDir, err)
	}

	// Create backup if target file already exists
	if backupPath != "" {
		if err := copyFile(destPath, backupPath); err != nil {
			return "", fmt.Errorf("failed to backup existing config to %s: %w", backupPath, err)
		}
	}

	// Atomic write using a temporary file in the destination directory
	tmpFile, err := os.CreateTemp(destDir, "ModsConfig.*.xml.tmp")
	if err != nil {
		return backupPath, fmt.Errorf("failed to create temp file in %s: %w", destDir, err)
	}
	tmpName := tmpFile.Name()
	defer os.Remove(tmpName)

	if _, err := tmpFile.Write(content); err != nil {
		tmpFile.Close()
		return backupPath, fmt.Errorf("failed to write content to %s: %w", tmpName, err)
	}
	if err := tmpFile.Close(); err != nil {
		return backupPath, fmt.Errorf("failed to close temp file %s: %w", tmpName, err)
	}

	if err := os.Rename(tmpName, destPath); err != nil {
		return backupPath, fmt.Errorf("failed to rename %s to %s: %w", tmpName, destPath, err)
	}

	return backupPath, nil
}

func copyFile(src, dst string) error {
	in, err := os.Open(src)
	if err != nil {
		return err
	}
	defer in.Close()

	out, err := os.Create(dst)
	if err != nil {
		return err
	}
	defer out.Close()

	if _, err := io.Copy(out, in); err != nil {
		return err
	}
	return out.Sync()
}
