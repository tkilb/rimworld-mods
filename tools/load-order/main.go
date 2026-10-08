package main

import (
	"flag"
	"fmt"
	"os"
	"path/filepath"
	"strings"
)

// ANSI color codes
const (
	colorReset  = "\033[0m"
	colorBold   = "\033[1m"
	colorRed    = "\033[31m"
	colorGreen  = "\033[32m"
	colorYellow = "\033[33m"
	colorCyan   = "\033[36m"
	colorGray   = "\033[90m"
)

func main() {
	manifestPath := flag.String("manifest", "manifests/mods.yaml", "Path to manifests/mods.yaml")
	vendorDir := flag.String("vendor-dir", "mods/vendor", "Path to mods/vendor directory")
	customDir := flag.String("custom-dir", "mods/custom", "Path to mods/custom directory")
	rimworldDir := flag.String("rimworld-dir", "", "Path to RimWorld game directory (to detect Data/ DLCs)")
	outputXML := flag.String("output-xml", "", "Path to write generated ModsConfig.xml")
	existingConfig := flag.String("existing-config", "", "Path to existing ModsConfig.xml to preserve version tag (defaults to output-xml)")
	dryRun := flag.Bool("dry-run", false, "Preview computed load order without modifying filesystem")
	flag.Parse()

	if *dryRun {
		fmt.Printf("%s[DRY-RUN]%s Resolving RimWorld mod load order...\n\n", colorCyan, colorReset)
	}

	mf, err := LoadManifest(*manifestPath)
	if err != nil {
		fmt.Fprintf(os.Stderr, "%s[ERROR]%s Failed to read manifest %s: %v\n", colorRed, colorReset, *manifestPath, err)
		os.Exit(1)
	}

	s := NewSorter()
	addedPackageIDs := make(map[string]bool)

	for modID, entry := range mf.Mods {
		if !entry.IsEnabled() {
			continue
		}
		if entry.Source == "custom" || entry.Source == "local" {
			continue
		}

		modDirPath := filepath.Join(*vendorDir, modID)
		var meta *ModMetaData

		if fi, err := os.Stat(modDirPath); err == nil && fi.IsDir() {
			aboutPath, err := FindAboutXML(modDirPath)
			if err == nil {
				parsedMeta, err := ParseAboutXML(aboutPath)
				if err == nil {
					meta = parsedMeta
				}
			}
		}

		node := BuildModNode(modID, entry, meta, modDirPath, false)
		s.AddNode(node)
		addedPackageIDs[node.NormalizedPackageID()] = true
	}

	if fi, err := os.Stat(*customDir); err == nil && fi.IsDir() {
		entries, _ := os.ReadDir(*customDir)
		for _, e := range entries {
			if !e.IsDir() || strings.HasPrefix(e.Name(), ".") {
				continue
			}
			customModID := e.Name()

			// Check if custom mod is declared in manifest and disabled
			if entry, ok := mf.Mods[customModID]; ok && !entry.IsEnabled() {
				continue
			}

			customModDir := filepath.Join(*customDir, customModID)

			var meta *ModMetaData
			aboutPath, err := FindAboutXML(customModDir)
			if err == nil {
				parsedMeta, err := ParseAboutXML(aboutPath)
				if err == nil {
					meta = parsedMeta
				}
			}

			var customPkgID string
			var customName string
			if meta != nil && meta.PackageID != "" {
				customPkgID = meta.PackageID
				customName = meta.Name
			} else {
				customPkgID = "custom." + customModID
				customName = customModID
			}

			if addedPackageIDs[strings.ToLower(customPkgID)] {
				continue
			}

			var entry ModEntry
			if mEntry, ok := mf.Mods[customModID]; ok {
				entry = mEntry
				if entry.Name == "" {
					entry.Name = customName
				}
				if entry.PackageID == "" {
					entry.PackageID = customPkgID
				}
			} else {
				entry = ModEntry{
					Name:      customName,
					PackageID: customPkgID,
				}
			}

			node := BuildModNode(customModID, entry, meta, customModDir, true)
			s.AddNode(node)
			addedPackageIDs[node.NormalizedPackageID()] = true
		}
	}

	var candidateDLCs []string
	resolvedRimWorldDir := *rimworldDir

	if resolvedRimWorldDir == "" {
		if modsEnv := os.Getenv("RIMWORLD_MODS_DIR"); modsEnv != "" {
			resolvedRimWorldDir = filepath.Dir(modsEnv)
		}
	}

	if resolvedRimWorldDir != "" {
		dataDir := filepath.Join(resolvedRimWorldDir, "Data")
		if fi, err := os.Stat(dataDir); err == nil && fi.IsDir() {
			for _, dlcPkg := range CanonicalDLCs {
				dlcSubdir := strings.TrimPrefix(dlcPkg, "ludeon.rimworld.")
				dlcSubdir = strings.ToUpper(dlcSubdir[:1]) + dlcSubdir[1:]
				dlcPath := filepath.Join(dataDir, dlcSubdir)
				if fi, err := os.Stat(dlcPath); err == nil && fi.IsDir() {
					candidateDLCs = append(candidateDLCs, dlcPkg)
				}
			}
		}
	}

	if len(candidateDLCs) == 0 {
		candidateDLCs = append(candidateDLCs, CanonicalDLCs...)
	}

	var activeDLCs []string
	for _, dlcPkg := range candidateDLCs {
		if mf.IsDLCEnabled(dlcPkg) {
			activeDLCs = append(activeDLCs, dlcPkg)
		}
	}

	// Also check any extra DLCs explicitly enabled in manifest (e.g. future or custom expansions)
	for dlcName, setting := range mf.DLCs {
		if !setting.Enabled {
			continue
		}
		dlcPkg := strings.ToLower(dlcName)
		if !strings.HasPrefix(dlcPkg, "ludeon.rimworld.") {
			dlcPkg = "ludeon.rimworld." + dlcPkg
		}
		alreadyActive := false
		for _, existing := range activeDLCs {
			if strings.EqualFold(existing, dlcPkg) {
				alreadyActive = true
				break
			}
		}
		if !alreadyActive {
			activeDLCs = append(activeDLCs, dlcPkg)
		}
		alreadyCandidate := false
		for _, existing := range candidateDLCs {
			if strings.EqualFold(existing, dlcPkg) {
				alreadyCandidate = true
				break
			}
		}
		if !alreadyCandidate {
			candidateDLCs = append(candidateDLCs, dlcPkg)
		}
	}

	result, err := s.Sort(activeDLCs)
	if err != nil {
		fmt.Fprintf(os.Stderr, "%s[ERROR]%s %v\n", colorRed, colorReset, err)
		os.Exit(1)
	}

	tierName := func(tier int) string {
		switch tier {
		case TierHarmony:
			return "Harmony"
		case TierCore:
			return "Core"
		case TierDLC:
			return "DLC"
		case TierTrailing:
			return "Trailing"
		default:
			return "Standard"
		}
	}

	fmt.Printf("%s%-4s %-10s %-32s %-30s %-10s %-10s%s\n",
		colorBold, "#", "TIER", "PACKAGE ID", "MOD NAME", "SOURCE", "METADATA", colorReset)
	fmt.Println(strings.Repeat("-", 102))

	for idx, m := range result.OrderedMods {
		metaStatus := "manifest"
		if m.HasAboutXML {
			metaStatus = "About.xml"
		} else if m.Source == "anchor" {
			metaStatus = "official"
		}

		displayName := m.Name
		if len(displayName) > 28 {
			displayName = displayName[:25] + "..."
		}

		pkgID := m.PackageID
		if len(pkgID) > 30 {
			pkgID = pkgID[:27] + "..."
		}

		color := ""
		switch m.Tier {
		case TierHarmony:
			color = colorGreen
		case TierCore:
			color = colorCyan
		case TierDLC:
			color = colorYellow
		case TierTrailing:
			color = colorGray
		}

		fmt.Printf("%-4d %s%-10s%s %-32s %-30s %-10s %-10s\n",
			idx+1, color, tierName(m.Tier), colorReset, pkgID, displayName, m.Source, metaStatus)
	}

	fmt.Println(strings.Repeat("-", 102))
	fmt.Printf("%s[OK]%s Successfully resolved load order for %d active mods/expansions.\n",
		colorGreen, colorReset, len(result.OrderedMods))

	if len(result.Warnings) > 0 {
		fmt.Println()
		for _, w := range result.Warnings {
			fmt.Printf("%s[WARN]%s %s\n", colorYellow, colorReset, w)
		}
	}

	if *outputXML != "" {
		var activeMods []string
		for _, m := range result.OrderedMods {
			activeMods = append(activeMods, strings.ToLower(m.PackageID))
		}

		var knownExpansions []string
		for _, dlc := range candidateDLCs {
			knownExpansions = append(knownExpansions, strings.ToLower(dlc))
		}

		configPath := *outputXML
		readPath := *existingConfig
		if readPath == "" {
			readPath = configPath
		}
		version := ReadExistingVersion(readPath)

		xmlBytes, err := GenerateModsConfigXML(activeMods, knownExpansions, version)
		if err != nil {
			fmt.Fprintf(os.Stderr, "%s[ERROR]%s Failed to generate ModsConfig.xml: %v\n", colorRed, colorReset, err)
			os.Exit(1)
		}

		if *dryRun {
			fmt.Printf("\n%s[DRY-RUN]%s Would write ModsConfig.xml to: %s\n", colorCyan, colorReset, configPath)
			if fi, err := os.Stat(configPath); err == nil && !fi.IsDir() {
				fmt.Printf("%s[DRY-RUN]%s Would backup existing config: %s -> %s.bak\n", colorCyan, colorReset, configPath, configPath)
			}
		} else {
			backupPath, err := DeployModsConfig(configPath, xmlBytes, false)
			if err != nil {
				fmt.Fprintf(os.Stderr, "%s[ERROR]%s Failed to deploy ModsConfig.xml: %v\n", colorRed, colorReset, err)
				os.Exit(1)
			}
			if backupPath != "" {
				fmt.Printf("%s[OK]%s Backed up previous configuration to: %s\n", colorGreen, colorReset, backupPath)
			}
			fmt.Printf("%s[OK]%s Deployed active ModsConfig.xml to: %s\n", colorGreen, colorReset, configPath)
		}
	}
}
