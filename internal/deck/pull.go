package deck

import (
	"archive/tar"
	"bufio"
	"compress/gzip"
	"fmt"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"sort"
	"strconv"
	"strings"

	"rimworld-mods/internal/config"
	"rimworld-mods/internal/importer"
	"rimworld-mods/internal/manifest"
	"rimworld-mods/internal/order"
)

var officialPackages = map[string]bool{
	"ludeon.rimworld":          true,
	"ludeon.rimworld.royalty":  true,
	"ludeon.rimworld.ideology": true,
	"ludeon.rimworld.biotech":  true,
	"ludeon.rimworld.anomaly":  true,
	"ludeon.rimworld.odyssey":  true,
}

// DiscoveredMod represents a Steam Workshop mod found on the Steam Deck.
type DiscoveredMod struct {
	Name         string
	PackageID    string
	WorkshopID   string
	Author       string
	Description  string
	ModDir       string
	Dependencies []order.ModDependency
}

// Pull launches the interactive Steam Workshop cherry-pick importer in pure Go.
func Pull(env *config.Environment, dryRun bool, copyFiles bool, showAll bool) error {
	deckHost := env.DeckHost
	if deckHost == "" {
		deckHost = "steamdeck"
	}

	manifestPath := filepath.Join(env.RepoRoot, "manifests", "mods.yaml")
	mf, err := manifest.LoadManifest(manifestPath)
	if err != nil {
		return fmt.Errorf("failed to read %s: %w", manifestPath, err)
	}

	tmpDir, err := os.MkdirTemp("", "rimmod_deck_mods_*")
	if err != nil {
		return fmt.Errorf("failed to create temp dir: %w", err)
	}
	defer os.RemoveAll(tmpDir)

	fmt.Printf("\033[34m[INFO]\033[0m Searching for RimWorld workshop mods on %s...\n", deckHost)

	// Step 1: Discover Workshop content directories on Deck
	workshopDirs, err := discoverWorkshopDirs(deckHost)
	if err != nil {
		return err
	}
	if len(workshopDirs) == 0 {
		return fmt.Errorf("no RimWorld workshop directory found on %s (checked default Steam & SD card paths)", deckHost)
	}
	fmt.Printf("\033[32m[OK]\033[0m Located workshop content at: %s\n", strings.Join(workshopDirs, ", "))

	// Step 2: Stream About.xml metadata over SSH and unpack
	mods, err := streamWorkshopMetadata(deckHost, workshopDirs, tmpDir)
	if err != nil {
		return err
	}
	if len(mods) == 0 {
		fmt.Println("\033[33m[WARN]\033[0m No valid RimWorld workshop mods with About.xml found.")
		return nil
	}
	fmt.Printf("\033[32m[OK]\033[0m Discovered %d workshop mods.\n", len(mods))

	// Step 3: Build dependency graph to identify transitive dependencies
	transitiveSet := make(map[string]bool)
	for _, m := range mods {
		for _, dep := range m.Dependencies {
			depLower := strings.ToLower(dep.PackageID)
			if !officialPackages[depLower] {
				transitiveSet[depLower] = true
			}
		}
	}

	// Step 4: Filter out mods already declared in manifest
	existingPackageIDs := make(map[string]bool)
	existingWorkshopIDs := make(map[string]bool)
	existingSlugs := make(map[string]bool)

	for slug, entry := range mf.Mods {
		existingSlugs[slug] = true
		if entry.PackageID != "" {
			existingPackageIDs[strings.ToLower(entry.PackageID)] = true
		}
		if entry.WorkshopID != "" {
			existingWorkshopIDs[entry.WorkshopID] = true
		}
	}

	var candidates []*DiscoveredMod
	for _, m := range mods {
		pkgLower := strings.ToLower(m.PackageID)
		if existingPackageIDs[pkgLower] || existingWorkshopIDs[m.WorkshopID] {
			continue
		}
		if !showAll && transitiveSet[pkgLower] {
			continue
		}
		candidates = append(candidates, m)
	}

	sort.Slice(candidates, func(i, j int) bool {
		return strings.ToLower(candidates[i].Name) < strings.ToLower(candidates[j].Name)
	})

	if len(candidates) == 0 {
		fmt.Println("\033[33m[INFO]\033[0m All discoverable mods are already declared in manifests/mods.yaml (or are hidden transitive dependencies).")
		if !showAll && len(transitiveSet) > 0 {
			fmt.Printf("  (\033[33m%d\033[0m transitive dependencies are hidden. Use --all to display everything).\n", len(transitiveSet))
		}
		return nil
	}

	// Step 5: Generate previews and launch interactive picker
	previewsDir := filepath.Join(tmpDir, "previews")
	if err := os.MkdirAll(previewsDir, 0755); err != nil {
		return err
	}
	generatePreviewFiles(mods, previewsDir, transitiveSet)

	selectedPkgIDs, err := runInteractivePicker(candidates, previewsDir)
	if err != nil {
		return err
	}
	if len(selectedPkgIDs) == 0 {
		fmt.Println("\033[33m[INFO]\033[0m No mods selected. Operation canceled.")
		return nil
	}

	// Step 6: Resolve transitive closure
	toImport, autoIncluded := resolveTransitiveClosure(selectedPkgIDs, mods)

	fmt.Println("\n\033[1mSelected mods for import:\033[0m")
	for _, pkg := range selectedPkgIDs {
		if m, ok := mods[pkg]; ok {
			fmt.Printf("  • \033[1;32m%s\033[0m (%s)\n", m.Name, m.PackageID)
		}
	}

	if len(autoIncluded) > 0 {
		fmt.Println("\n\033[1mAutomatically included dependencies:\033[0m")
		for _, m := range autoIncluded {
			fmt.Printf("  + \033[1;35m%s\033[0m (%s)\n", m.Name, m.PackageID)
		}
	}

	// Step 7: Update manifests/mods.yaml
	addedSlugs, err := updateManifest(manifestPath, toImport, existingSlugs, existingPackageIDs, existingWorkshopIDs, dryRun)
	if err != nil {
		return err
	}

	// Step 8: Optional file copying from Deck
	if len(addedSlugs) > 0 && copyFiles && !dryRun {
		copyWorkshopFiles(deckHost, toImport, addedSlugs, env.RepoRoot)
	} else if len(addedSlugs) > 0 && !dryRun {
		fmt.Println("\n\033[36mNext steps:\033[0m")
		fmt.Println("  1. Run \033[1mrimmod apply\033[0m to download, lock, link, and configure load order.")
	}

	return nil
}

func discoverWorkshopDirs(deckHost string) ([]string, error) {
	discoveryCmd := `for d in "$HOME/.local/share/Steam/steamapps/workshop/content/294100" /run/media/*/steamapps/workshop/content/294100; do if [ -d "$d" ]; then echo "$d"; fi; done; find /run/media -maxdepth 5 -type d -path "*/steamapps/workshop/content/294100" 2>/dev/null || true`
	cmd := exec.Command("ssh", "-o", "BatchMode=yes", "-o", "ConnectTimeout=10", deckHost, discoveryCmd)
	out, err := cmd.CombinedOutput()
	if err != nil {
		return nil, fmt.Errorf("failed to query workshop directories on %s: %s (%w)", deckHost, string(out), err)
	}

	var dirs []string
	seen := make(map[string]bool)
	scanner := bufio.NewScanner(strings.NewReader(string(out)))
	for scanner.Scan() {
		line := strings.TrimSpace(scanner.Text())
		if line != "" && !seen[line] {
			seen[line] = true
			dirs = append(dirs, line)
		}
	}
	return dirs, nil
}

func streamWorkshopMetadata(deckHost string, dirs []string, localTmpDir string) (map[string]*DiscoveredMod, error) {
	mods := make(map[string]*DiscoveredMod)

	for _, remoteDir := range dirs {
		fmt.Printf("\033[34m[INFO]\033[0m Fetching mod metadata from %s...\n", remoteDir)
		tarCmd := fmt.Sprintf("find %s -maxdepth 4 -iname 'About.xml' -print0 2>/dev/null | tar -czf - --null -T -", remoteDir)

		sshCmd := exec.Command("ssh", "-o", "BatchMode=yes", deckHost, tarCmd)
		stdout, err := sshCmd.StdoutPipe()
		if err != nil {
			return nil, fmt.Errorf("failed to open stdout pipe: %w", err)
		}

		if err := sshCmd.Start(); err != nil {
			return nil, fmt.Errorf("failed to start remote tar command: %w", err)
		}

		gzReader, err := gzip.NewReader(stdout)
		if err == nil {
			tarReader := tar.NewReader(gzReader)
			for {
				header, err := tarReader.Next()
				if err == io.EOF {
					break
				}
				if err != nil {
					break
				}
				if header.Typeflag != tar.TypeReg {
					continue
				}

				targetPath := filepath.Join(localTmpDir, header.Name)
				_ = os.MkdirAll(filepath.Dir(targetPath), 0755)

				outFile, err := os.OpenFile(targetPath, os.O_CREATE|os.O_WRONLY|os.O_TRUNC, header.FileInfo().Mode())
				if err != nil {
					continue
				}
				_, _ = io.Copy(outFile, tarReader)
				outFile.Close()
			}
			gzReader.Close()
		}
		_ = sshCmd.Wait()

		// Walk extracted directory and find all About.xml files
		_ = filepath.Walk(localTmpDir, func(path string, info os.FileInfo, err error) error {
			if err != nil || info == nil || info.IsDir() {
				return nil
			}
			if !strings.EqualFold(filepath.Base(path), "About.xml") {
				return nil
			}

			meta, err := order.ParseAboutXML(path)
			if err != nil || meta == nil || meta.PackageID == "" {
				return nil
			}

			parts := strings.Split(filepath.ToSlash(path), "/")
			var wid string
			for i, p := range parts {
				if p == "294100" && i+1 < len(parts) {
					if _, err := strconv.Atoi(parts[i+1]); err == nil {
						wid = parts[i+1]
						break
					}
				}
			}
			if wid == "" {
				for i := len(parts) - 1; i >= 0; i-- {
					if _, err := strconv.Atoi(parts[i]); err == nil && parts[i] != "294100" {
						wid = parts[i]
						break
					}
				}
			}

			if wid != "" {
				pkgLower := strings.ToLower(meta.PackageID)
				if _, ok := mods[pkgLower]; !ok {
					mods[pkgLower] = &DiscoveredMod{
						Name:         meta.Name,
						PackageID:    meta.PackageID,
						WorkshopID:   wid,
						Author:       meta.Author,
						Description:  meta.Description,
						ModDir:       fmt.Sprintf("%s/%s", remoteDir, wid),
						Dependencies: meta.ModDependencies,
					}
				}
			}
			return nil
		})
	}

	return mods, nil
}

func generatePreviewFiles(mods map[string]*DiscoveredMod, previewsDir string, transitiveSet map[string]bool) {
	for pkgLower, m := range mods {
		previewFile := filepath.Join(previewsDir, pkgLower+".txt")
		var sb strings.Builder
		sb.WriteString(fmt.Sprintf("\033[1;36m%s\033[0m\n", m.Name))
		sb.WriteString(strings.Repeat("─", 50) + "\n")
		sb.WriteString(fmt.Sprintf("\033[1mPackage ID:\033[0m   %s\n", m.PackageID))
		sb.WriteString(fmt.Sprintf("\033[1mWorkshop ID:\033[0m  %s\n", m.WorkshopID))
		if m.Author != "" {
			sb.WriteString(fmt.Sprintf("\033[1mAuthor:\033[0m       %s\n", m.Author))
		}
		sb.WriteString(fmt.Sprintf("\033[1mSteam URL:\033[0m    https://steamcommunity.com/sharedfiles/filedetails/?id=%s\n", m.WorkshopID))

		if len(m.Dependencies) > 0 {
			sb.WriteString("\n\033[1;33mDependencies:\033[0m\n")
			for _, d := range m.Dependencies {
				dPkg := d.PackageID
				dName := d.DisplayName
				if dName == "" {
					dName = dPkg
				}
				dLower := strings.ToLower(dPkg)
				var status string
				if officialPackages[dLower] {
					status = "\033[32m[Official DLC/Core]\033[0m"
				} else if _, onDeck := mods[dLower]; onDeck {
					status = "\033[32m[Available on Deck - will auto-import]\033[0m"
				} else {
					status = "\033[31m[External/Missing from Deck]\033[0m"
				}
				sb.WriteString(fmt.Sprintf("  • %s (%s) %s\n", dName, dPkg, status))
			}
		} else {
			sb.WriteString("\n\033[1;32mNo dependencies required\033[0m\n")
		}

		if transitiveSet[pkgLower] {
			sb.WriteString("\n\033[35m[Note: This is a transitive dependency of another mod]\033[0m\n")
		}

		desc := strings.TrimSpace(m.Description)
		if desc != "" {
			sb.WriteString("\n\033[1mDescription:\033[0m\n")
			if len(desc) > 1000 {
				desc = desc[:1000] + "..."
			}
			sb.WriteString(desc + "\n")
		}

		_ = os.WriteFile(previewFile, []byte(sb.String()), 0644)
	}
}

func runInteractivePicker(candidates []*DiscoveredMod, previewsDir string) ([]string, error) {
	fzfPath, err := exec.LookPath("fzf")
	if err == nil {
		var lines []string
		for _, m := range candidates {
			lines = append(lines, fmt.Sprintf("%-45s │ %-35s │ [Workshop: %s]", m.Name, m.PackageID, m.WorkshopID))
		}
		inputText := strings.Join(lines, "\n")

		previewCmd := fmt.Sprintf(`sh -c 'p=$(echo {2} | tr -d "[:space:]" | tr "[:upper:]" "[:lower:]"); cat "%s/$p.txt" 2>/dev/null || echo "No preview"'`, previewsDir)

		fzfArgs := []string{
			"-m",
			"--ansi",
			"--bind=space:toggle+down",
			"--header=Select mods to import [Space: toggle selection | Enter: confirm | Esc: cancel]",
			"--delimiter=│",
			fmt.Sprintf("--preview=%s", previewCmd),
			"--preview-window=right:50%:wrap",
			"--prompt=Cherry-pick mods > ",
		}

		cmd := exec.Command(fzfPath, fzfArgs...)
		cmd.Stdin = strings.NewReader(inputText)
		cmd.Stderr = os.Stderr
		out, err := cmd.Output()
		if err != nil {
			return nil, nil // User canceled
		}

		var selected []string
		scanner := bufio.NewScanner(strings.NewReader(string(out)))
		for scanner.Scan() {
			line := scanner.Text()
			parts := strings.Split(line, "│")
			if len(parts) >= 2 {
				pkg := strings.ToLower(strings.TrimSpace(parts[1]))
				if pkg != "" {
					selected = append(selected, pkg)
				}
			}
		}
		return selected, nil
	}

	// Fallback prompt when fzf is not installed
	fmt.Println("\nAvailable mods on Steam Deck:")
	for i, m := range candidates {
		fmt.Printf("  [%2d] %s (%s) [Workshop: %s]\n", i+1, m.Name, m.PackageID, m.WorkshopID)
	}
	fmt.Print("\nEnter numbers of mods to import (comma-separated, e.g. 1,3,5) or Enter to cancel: ")

	scanner := bufio.NewScanner(os.Stdin)
	if !scanner.Scan() {
		return nil, nil
	}
	input := strings.TrimSpace(scanner.Text())
	if input == "" {
		return nil, nil
	}

	var selected []string
	for _, tok := range strings.Split(input, ",") {
		tok = strings.TrimSpace(tok)
		if idx, err := strconv.Atoi(tok); err == nil && idx >= 1 && idx <= len(candidates) {
			selected = append(selected, strings.ToLower(candidates[idx-1].PackageID))
		}
	}
	return selected, nil
}

func resolveTransitiveClosure(selectedPkgs []string, mods map[string]*DiscoveredMod) (map[string]*DiscoveredMod, []*DiscoveredMod) {
	toImport := make(map[string]*DiscoveredMod)
	var autoIncluded []*DiscoveredMod

	selectedSet := make(map[string]bool)
	queue := append([]string{}, selectedPkgs...)
	visited := make(map[string]bool)

	for _, p := range selectedPkgs {
		selectedSet[p] = true
	}

	for len(queue) > 0 {
		pkg := queue[0]
		queue = queue[1:]

		if visited[pkg] {
			continue
		}
		visited[pkg] = true

		m, ok := mods[pkg]
		if !ok {
			continue
		}

		toImport[pkg] = m
		if !selectedSet[pkg] {
			autoIncluded = append(autoIncluded, m)
		}

		for _, dep := range m.Dependencies {
			depLower := strings.ToLower(dep.PackageID)
			if officialPackages[depLower] {
				continue
			}
			if _, exists := mods[depLower]; exists && !visited[depLower] {
				queue = append(queue, depLower)
			}
		}
	}

	return toImport, autoIncluded
}

func updateManifest(manifestPath string, toImport map[string]*DiscoveredMod, existingSlugs, existingPackageIDs, existingWorkshopIDs map[string]bool, dryRun bool) (map[string]string, error) {
	pkgToSlug := make(map[string]string)
	addedSlugs := make(map[string]manifest.ModEntry)

	for pkgLower, m := range toImport {
		if existingPackageIDs[pkgLower] || existingWorkshopIDs[m.WorkshopID] {
			continue
		}

		slug := importer.DeriveSlug(m.PackageID, existingSlugs)
		existingSlugs[slug] = true

		desc := strings.ReplaceAll(m.Description, "\n", " ")
		desc = strings.TrimSpace(desc)
		if len(desc) > 300 {
			desc = desc[:300]
		}

		t := true
		entry := manifest.ModEntry{
			Name:        m.Name,
			Source:      "steam_workshop",
			WorkshopID:  m.WorkshopID,
			PackageID:   m.PackageID,
			Enabled:     &t,
			Description: desc,
		}

		addedSlugs[slug] = entry
		pkgToSlug[pkgLower] = slug
	}

	if len(addedSlugs) == 0 {
		fmt.Println("\033[33m[WARN]\033[0m All selected mods (and dependencies) are already present in manifests/mods.yaml.")
		return nil, nil
	}

	fmt.Println("\n\033[1;32mPlanned additions to manifests/mods.yaml:\033[0m")
	for slug, entry := range addedSlugs {
		fmt.Printf("  + \033[1;36m%s\033[0m: %q (ID: %s, Package: %s)\n", slug, entry.Name, entry.WorkshopID, entry.PackageID)
	}

	if dryRun {
		fmt.Println("\n\033[34m[DRY-RUN]\033[0m Dry-run enabled. mods.yaml was not modified.")
		return pkgToSlug, nil
	}

	for slug, entry := range addedSlugs {
		if err := importer.SaveManifestWithComments(manifestPath, slug, entry); err != nil {
			return nil, fmt.Errorf("failed adding %s to %s: %w", slug, manifestPath, err)
		}
	}

	fmt.Printf("\033[32m[OK]\033[0m Successfully updated %s with %d mod(s)!\n", manifestPath, len(addedSlugs))
	return pkgToSlug, nil
}

func copyWorkshopFiles(deckHost string, mods map[string]*DiscoveredMod, pkgToSlug map[string]string, repoRoot string) {
	vendorDir := filepath.Join(repoRoot, "mods", "vendor")
	for pkgLower, m := range mods {
		slug, ok := pkgToSlug[pkgLower]
		if !ok || m.ModDir == "" {
			continue
		}
		destDir := filepath.Join(vendorDir, slug)
		fmt.Printf("\033[34m[INFO]\033[0m Syncing files for %s -> %s...\n", m.Name, destDir)
		_ = os.MkdirAll(destDir, 0755)

		cmd := exec.Command("rsync", "-avz", "--delete", fmt.Sprintf("%s:%s/", deckHost, m.ModDir), fmt.Sprintf("%s/", destDir))
		if err := cmd.Run(); err != nil {
			fmt.Printf("\033[31m[ERROR]\033[0m Failed copying %s: %v\n", slug, err)
		} else {
			fmt.Printf("\033[32m[OK]\033[0m Synced %s successfully.\n", slug)
		}
	}
}
