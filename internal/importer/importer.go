package importer

import (
	"bytes"
	"fmt"
	"net/url"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"unicode"

	"gopkg.in/yaml.v3"
	"rimworld-mods/internal/config"
	"rimworld-mods/internal/fetcher"
	"rimworld-mods/internal/manifest"
	"rimworld-mods/internal/order"
)

// SourceInfo holds parsed mod source information.
type SourceInfo struct {
	SourceType string // "steam_workshop", "github_release", "git"
	WorkshopID string
	Repo       string
	Tag        string
	URL        string
}

var (
	workshopIDRegex = regexp.MustCompile(`^[0-9]+$`)
	urlIDParamRegex = regexp.MustCompile(`[?&]id=([0-9]+)`)
	ghReleaseRegex  = regexp.MustCompile(`^github\.com/([^/]+)/([^/]+)/releases(?:/tag/([^/]+)|/latest)?/?$`)
	ghRepoRegex     = regexp.MustCompile(`^github\.com/([^/]+)/([^/]+?)(?:\.git)?/?$`)
)

// ParseSource parses an input string (URL or workshop ID) into a SourceInfo.
func ParseSource(raw string) (*SourceInfo, error) {
	trimmed := strings.TrimSpace(raw)
	if trimmed == "" {
		return nil, fmt.Errorf("empty URL or source provided")
	}

	// 1. Direct numeric Workshop ID (e.g. 3525571078)
	if workshopIDRegex.MatchString(trimmed) {
		return &SourceInfo{
			SourceType: "steam_workshop",
			WorkshopID: trimmed,
		}, nil
	}

	// 2. Steam Workshop URL
	if strings.Contains(trimmed, "steamcommunity.com") {
		matches := urlIDParamRegex.FindStringSubmatch(trimmed)
		if len(matches) > 1 {
			return &SourceInfo{
				SourceType: "steam_workshop",
				WorkshopID: matches[1],
			}, nil
		}
		return nil, fmt.Errorf("could not extract workshop id from Steam URL: %s", trimmed)
	}

	// Clean protocol prefix for GitHub matching
	cleaned := trimmed
	for _, prefix := range []string{"https://", "http://", "git@github.com:"} {
		if strings.HasPrefix(cleaned, prefix) {
			if prefix == "git@github.com:" {
				cleaned = "github.com/" + strings.TrimPrefix(cleaned, prefix)
			} else {
				cleaned = strings.TrimPrefix(cleaned, prefix)
			}
			break
		}
	}

	// 3. GitHub release URL (e.g. github.com/owner/repo/releases/tag/v1.0.0 or releases/latest)
	if m := ghReleaseRegex.FindStringSubmatch(cleaned); len(m) > 0 {
		tag := m[3]
		if tag == "" {
			tag = "latest"
		}
		return &SourceInfo{
			SourceType: "github_release",
			Repo:       fmt.Sprintf("%s/%s", m[1], m[2]),
			Tag:        tag,
		}, nil
	}

	// 4. GitHub repo URL (e.g. github.com/owner/repo or github.com/owner/repo.git)
	if m := ghRepoRegex.FindStringSubmatch(cleaned); len(m) > 0 {
		repoPath := fmt.Sprintf("%s/%s", m[1], m[2])
		// If explicitly ending with .git, treat as git source
		if strings.HasSuffix(trimmed, ".git") {
			return &SourceInfo{
				SourceType: "git",
				URL:        trimmed,
			}, nil
		}
		// Default GitHub repo to github_release latest
		return &SourceInfo{
			SourceType: "github_release",
			Repo:       repoPath,
			Tag:        "latest",
		}, nil
	}

	// 5. Generic Git URL
	if strings.HasSuffix(trimmed, ".git") || strings.HasPrefix(trimmed, "git@") {
		return &SourceInfo{
			SourceType: "git",
			URL:        trimmed,
		}, nil
	}

	// Fallback check: valid HTTP/HTTPS URL that might be a git repo
	if u, err := url.Parse(trimmed); err == nil && (u.Scheme == "http" || u.Scheme == "https") {
		return &SourceInfo{
			SourceType: "git",
			URL:        trimmed,
		}, nil
	}

	return nil, fmt.Errorf("unrecognized mod source URL: %s", trimmed)
}

// genericSlugParts are common mod package suffixes that should be combined with the preceding token.
var genericSlugParts = map[string]bool{
	"core":      true,
	"mod":       true,
	"base":      true,
	"framework": true,
	"lib":       true,
	"library":   true,
	"patch":     true,
	"redux":     true,
}

// ToKebabCase converts a PascalCase or camelCase string into kebab-case.
func ToKebabCase(s string) string {
	if s == "" {
		return ""
	}

	var buf bytes.Buffer
	runes := []rune(s)
	n := len(runes)

	for i := 0; i < n; i++ {
		r := runes[i]
		if !unicode.IsLetter(r) && !unicode.IsDigit(r) {
			if buf.Len() > 0 && buf.Bytes()[buf.Len()-1] != '-' {
				buf.WriteByte('-')
			}
			continue
		}

		if unicode.IsUpper(r) {
			// Prepend hyphen if:
			// 1. Previous character was lowercase or digit: e.g. "fooBar" -> "foo-bar"
			// 2. Next character is lowercase and previous was uppercase: e.g. "VFECore" -> "vfe-core"
			prevIsLowerOrDigit := i > 0 && (unicode.IsLower(runes[i-1]) || unicode.IsDigit(runes[i-1]))
			nextIsLower := i+1 < n && unicode.IsLower(runes[i+1])
			prevIsUpper := i > 0 && unicode.IsUpper(runes[i-1])

			if prevIsLowerOrDigit || (prevIsUpper && nextIsLower) {
				if buf.Len() > 0 && buf.Bytes()[buf.Len()-1] != '-' {
					buf.WriteByte('-')
				}
			}
			buf.WriteRune(unicode.ToLower(r))
		} else {
			buf.WriteRune(r)
		}
	}

	res := strings.Trim(buf.String(), "-")
	return res
}

// DeriveSlug generates a collision-free kebab-case slug from a mod's packageId.
func DeriveSlug(packageID string, existingSlugs map[string]bool) string {
	parts := strings.Split(packageID, ".")
	base := parts[len(parts)-1]

	// If the last part is a generic name (e.g. Core, Framework) and we have preceding context, combine them
	if (genericSlugParts[strings.ToLower(base)] || len(base) < 3) && len(parts) > 1 {
		base = parts[len(parts)-2] + base
	}

	slug := ToKebabCase(base)
	if slug == "" || len(slug) < 3 {
		slug = ToKebabCase(packageID)
	}
	if slug == "" {
		slug = "mod"
	}

	candidate := slug
	counter := 2
	for existingSlugs != nil && existingSlugs[candidate] {
		candidate = fmt.Sprintf("%s-%d", slug, counter)
		counter++
	}

	return candidate
}

// ImportResult contains information about an imported mod.
type ImportResult struct {
	Slug        string
	Name        string
	PackageID   string
	Description string
	Source      string
	WorkshopID  string
	Repo        string
	Tag         string
	URL         string
	VendorDir   string
}

// FetchAndStage downloads the mod into a temporary staging directory and inspects its About.xml.
func FetchAndStage(env *config.Environment, src *SourceInfo, dryRun bool) (*order.ModMetaData, string, func(), error) {
	tempDir, err := os.MkdirTemp("", "rimmod-import-*")
	if err != nil {
		return nil, "", nil, fmt.Errorf("failed to create staging directory: %w", err)
	}
	cleanup := func() { _ = os.RemoveAll(tempDir) }

	if dryRun {
		return &order.ModMetaData{
			Name:      "DryRun Mod",
			PackageID: "dryrun.preview",
		}, tempDir, cleanup, nil
	}

	switch src.SourceType {
	case "steam_workshop":
		if err := fetcher.FetchSteamWorkshop(env, "staging", src.WorkshopID, tempDir, false); err != nil {
			cleanup()
			return nil, "", nil, fmt.Errorf("failed to download Steam Workshop item %s: %w", src.WorkshopID, err)
		}

	case "github_release":
		if err := fetcher.FetchGitHubRelease("staging", src.Repo, src.Tag, "", tempDir, false); err != nil {
			cleanup()
			return nil, "", nil, fmt.Errorf("failed to download GitHub release for %s: %w", src.Repo, err)
		}

	case "git":
		if err := fetcher.FetchGitRepo("staging", src.URL, "", tempDir, false); err != nil {
			cleanup()
			return nil, "", nil, fmt.Errorf("failed to clone git repository %s: %w", src.URL, err)
		}

	default:
		cleanup()
		return nil, "", nil, fmt.Errorf("unsupported source type: %s", src.SourceType)
	}

	aboutPath, err := order.FindAboutXML(tempDir)
	if err != nil {
		cleanup()
		return nil, "", nil, fmt.Errorf("could not find About.xml in downloaded mod: %w", err)
	}

	meta, err := order.ParseAboutXML(aboutPath)
	if err != nil {
		cleanup()
		return nil, "", nil, fmt.Errorf("failed to parse About.xml: %w", err)
	}

	if meta.PackageID == "" {
		meta.PackageID = filepath.Base(tempDir)
	}
	if meta.Name == "" {
		meta.Name = meta.PackageID
	}

	return meta, tempDir, cleanup, nil
}

// AddModToManifestNode updates the YAML AST node tree for manifests/mods.yaml, preserving existing comments and ordering.
func AddModToManifestNode(root *yaml.Node, slug string, entry manifest.ModEntry) error {
	if root == nil || len(root.Content) == 0 {
		return fmt.Errorf("invalid or empty YAML root node")
	}

	doc := root.Content[0]
	if doc.Kind != yaml.MappingNode {
		return fmt.Errorf("expected root document mapping node, got kind %d", doc.Kind)
	}

	// Locate "mods" key node in document mapping
	var modsNode *yaml.Node
	for i := 0; i < len(doc.Content); i += 2 {
		if doc.Content[i].Value == "mods" {
			modsNode = doc.Content[i+1]
			break
		}
	}

	if modsNode == nil {
		// Create mods mapping node if not found
		modsKey := &yaml.Node{Kind: yaml.ScalarNode, Tag: "!!str", Value: "mods"}
		modsNode = &yaml.Node{Kind: yaml.MappingNode, Tag: "!!map"}
		doc.Content = append(doc.Content, modsKey, modsNode)
	}

	// Build the mapping node for the mod entry
	newModMap := &yaml.Node{Kind: yaml.MappingNode, Tag: "!!map"}

	addScalar := func(key, val string) {
		if val == "" {
			return
		}
		newModMap.Content = append(newModMap.Content,
			&yaml.Node{Kind: yaml.ScalarNode, Tag: "!!str", Value: key},
			&yaml.Node{Kind: yaml.ScalarNode, Tag: "!!str", Value: val},
		)
	}

	addBool := func(key string, val bool) {
		valStr := "false"
		if val {
			valStr = "true"
		}
		newModMap.Content = append(newModMap.Content,
			&yaml.Node{Kind: yaml.ScalarNode, Tag: "!!str", Value: key},
			&yaml.Node{Kind: yaml.ScalarNode, Tag: "!!bool", Value: valStr},
		)
	}

	addScalar("name", entry.Name)
	addScalar("source", entry.Source)
	if entry.WorkshopID != "" {
		addScalar("workshop_id", entry.WorkshopID)
	}
	if entry.Repo != "" {
		addScalar("repo", entry.Repo)
	}
	if entry.Tag != "" {
		addScalar("tag", entry.Tag)
	}
	if entry.URL != "" {
		addScalar("url", entry.URL)
	}
	addScalar("package_id", entry.PackageID)
	addBool("enabled", entry.IsEnabled())
	if entry.Description != "" {
		addScalar("description", entry.Description)
	}

	// Check if slug already exists in modsNode
	for i := 0; i < len(modsNode.Content); i += 2 {
		if modsNode.Content[i].Value == slug {
			// Update existing value node
			modsNode.Content[i+1] = newModMap
			return nil
		}
	}

	// Append new key and value
	keyNode := &yaml.Node{Kind: yaml.ScalarNode, Tag: "!!str", Value: slug}
	modsNode.Content = append(modsNode.Content, keyNode, newModMap)
	return nil
}

// SaveManifestWithComments reads manifestPath, updates the entry for slug, and writes back preserving comments.
func SaveManifestWithComments(manifestPath, slug string, entry manifest.ModEntry) error {
	data, err := os.ReadFile(manifestPath)
	if err != nil {
		return err
	}

	var root yaml.Node
	if err := yaml.Unmarshal(data, &root); err != nil {
		return err
	}

	if err := AddModToManifestNode(&root, slug, entry); err != nil {
		return err
	}

	var buf bytes.Buffer
	enc := yaml.NewEncoder(&buf)
	enc.SetIndent(2)
	if err := enc.Encode(&root); err != nil {
		return err
	}
	_ = enc.Close()

	return os.WriteFile(manifestPath, buf.Bytes(), 0644)
}
