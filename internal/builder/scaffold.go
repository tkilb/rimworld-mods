package builder

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"

	"rimworld-mods/internal/config"
)

var validNameRegex = regexp.MustCompile(`^[a-zA-Z0-9_-]+$`)

// ScaffoldMod generates a starter template for a custom XML or C# mod natively in Go.
func ScaffoldMod(env *config.Environment, name string, modType string, dryRun bool) error {
	if name == "" {
		return fmt.Errorf("mod name is required")
	}
	if !validNameRegex.MatchString(name) {
		return fmt.Errorf("invalid mod name '%s': only letters, numbers, hyphens, and underscores are allowed", name)
	}

	if modType == "" {
		modType = "xml"
	}
	if modType != "xml" && modType != "csharp" {
		return fmt.Errorf("invalid mod type '%s': must be either 'xml' or 'csharp'", modType)
	}

	templateDir := filepath.Join(env.RepoRoot, "templates", "custom-mod", modType)
	if fi, err := os.Stat(templateDir); err != nil || !fi.IsDir() {
		return fmt.Errorf("template directory not found: %s", templateDir)
	}

	targetDir := filepath.Join(env.RepoRoot, "mods", "custom", name)
	if _, err := os.Stat(targetDir); err == nil {
		return fmt.Errorf("target mod directory already exists: %s", targetDir)
	}

	author := getAuthor()
	description := fmt.Sprintf("A custom RimWorld mod (%s).", modType)
	projectName := toPascalCase(name)
	displayName := toTitleCase(name)
	cleanModID := toCleanID(name)
	cleanAuthor := toCleanID(author)
	packageID := fmt.Sprintf("%s.%s", cleanAuthor, cleanModID)

	fmt.Printf("Scaffolding custom mod:\n")
	fmt.Printf("  Target Path:   %s\n", targetDir)
	fmt.Printf("  Mod Type:      %s\n", modType)
	fmt.Printf("  Display Name:  %s\n", displayName)
	fmt.Printf("  Project Name:  %s\n", projectName)
	fmt.Printf("  Package ID:    %s\n", packageID)
	fmt.Printf("  Author:        %s\n\n", author)

	replacements := map[string]string{
		"{{MOD_NAME}}":        displayName,
		"{{PACKAGE_ID}}":       packageID,
		"{{AUTHOR}}":           author,
		"{{DESCRIPTION}}":      description,
		"{{PROJECT_NAME}}":     projectName,
		"{{MOD_NAME_PASCAL}}":  projectName,
		"{{MOD_NAME_LOWER}}":   name,
	}

	err := filepath.Walk(templateDir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if info.IsDir() {
			return nil
		}

		relPath, err := filepath.Rel(templateDir, path)
		if err != nil {
			return err
		}

		destRel := relPath
		destRel = strings.ReplaceAll(destRel, "Project.csproj", projectName+".csproj")
		destRel = strings.ReplaceAll(destRel, "ModMain.cs", projectName+"Mod.cs")

		destPath := filepath.Join(targetDir, destRel)

		if dryRun {
			fmt.Printf("  [DRY-RUN] Would create: mods/custom/%s/%s\n", name, destRel)
			return nil
		}

		data, err := os.ReadFile(path)
		if err != nil {
			return fmt.Errorf("failed reading template file %s: %w", path, err)
		}

		content := string(data)
		for placeholder, val := range replacements {
			content = strings.ReplaceAll(content, placeholder, val)
		}

		if err := os.MkdirAll(filepath.Dir(destPath), 0755); err != nil {
			return fmt.Errorf("failed creating directory %s: %w", filepath.Dir(destPath), err)
		}

		if err := os.WriteFile(destPath, []byte(content), info.Mode()); err != nil {
			return fmt.Errorf("failed writing file %s: %w", destPath, err)
		}

		fmt.Printf("  + Created: mods/custom/%s/%s\n", name, destRel)
		return nil
	})

	if err != nil {
		return err
	}

	if modType == "csharp" {
		assembliesDir := filepath.Join(targetDir, "Assemblies")
		if dryRun {
			fmt.Printf("  [DRY-RUN] Would create directory: mods/custom/%s/Assemblies\n", name)
		} else {
			if err := os.MkdirAll(assembliesDir, 0755); err != nil {
				return err
			}
		}
	}

	if dryRun {
		fmt.Printf("\n[DRY-RUN] Dry run complete. No files written.\n")
	} else {
		fmt.Printf("\nSuccessfully scaffolded '%s' at %s\n", name, targetDir)
	}

	return nil
}

func getAuthor() string {
	cmd := exec.Command("git", "config", "user.name")
	if out, err := cmd.Output(); err == nil && len(strings.TrimSpace(string(out))) > 0 {
		return strings.TrimSpace(string(out))
	}
	if user := os.Getenv("USER"); user != "" {
		return user
	}
	return "author"
}

func toPascalCase(s string) string {
	parts := regexp.MustCompile(`[-_ ]+`).Split(s, -1)
	var sb strings.Builder
	for _, p := range parts {
		if len(p) > 0 {
			sb.WriteString(strings.ToUpper(p[:1]) + strings.ToLower(p[1:]))
		}
	}
	return sb.String()
}

func toTitleCase(s string) string {
	parts := regexp.MustCompile(`[-_ ]+`).Split(s, -1)
	var words []string
	for _, p := range parts {
		if len(p) > 0 {
			words = append(words, strings.ToUpper(p[:1])+strings.ToLower(p[1:]))
		}
	}
	return strings.Join(words, " ")
}

func toCleanID(s string) string {
	cleaned := regexp.MustCompile(`[^a-zA-Z0-9]`).ReplaceAllString(s, "")
	return strings.ToLower(cleaned)
}
