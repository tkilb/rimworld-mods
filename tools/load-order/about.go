package main

import (
	"encoding/xml"
	"errors"
	"io/fs"
	"os"
	"path/filepath"
	"strings"
)

// ModMetaData models the About.xml metadata format for RimWorld mods.
type ModMetaData struct {
	XMLName          xml.Name        `xml:"ModMetaData"`
	Name             string          `xml:"name"`
	Author           string          `xml:"author"`
	PackageID        string          `xml:"packageId"`
	LoadAfter        []string        `xml:"loadAfter>li"`
	LoadBefore       []string        `xml:"loadBefore>li"`
	ForceLoadAfter   []string        `xml:"forceLoadAfter>li"`
	ForceLoadBefore  []string        `xml:"forceLoadBefore>li"`
	IncompatibleWith []string        `xml:"incompatibleWith>li"`
	ModDependencies  []ModDependency `xml:"modDependencies>li"`
}

// ModDependency models a dependency listed in <modDependencies>.
type ModDependency struct {
	PackageID   string `xml:"packageId"`
	DisplayName string `xml:"displayName"`
}

// ParseAboutXML reads and parses an About.xml file from the given file path.
func ParseAboutXML(path string) (*ModMetaData, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil, err
	}

	var meta ModMetaData
	if err := xml.Unmarshal(data, &meta); err != nil {
		return nil, err
	}

	// Trim whitespace across string fields
	meta.Name = strings.TrimSpace(meta.Name)
	meta.Author = strings.TrimSpace(meta.Author)
	meta.PackageID = strings.TrimSpace(meta.PackageID)

	cleanList := func(list []string) []string {
		var out []string
		for _, item := range list {
			trimmed := strings.TrimSpace(item)
			if trimmed != "" {
				out = append(out, trimmed)
			}
		}
		return out
	}

	meta.LoadAfter = cleanList(meta.LoadAfter)
	meta.LoadBefore = cleanList(meta.LoadBefore)
	meta.ForceLoadAfter = cleanList(meta.ForceLoadAfter)
	meta.ForceLoadBefore = cleanList(meta.ForceLoadBefore)
	meta.IncompatibleWith = cleanList(meta.IncompatibleWith)

	var cleanDeps []ModDependency
	for _, dep := range meta.ModDependencies {
		pid := strings.TrimSpace(dep.PackageID)
		if pid != "" {
			cleanDeps = append(cleanDeps, ModDependency{
				PackageID:   pid,
				DisplayName: strings.TrimSpace(dep.DisplayName),
			})
		}
	}
	meta.ModDependencies = cleanDeps

	return &meta, nil
}

// FindAboutXML locates the About.xml file within a given mod directory.
func FindAboutXML(modDir string) (string, error) {
	candidates := []string{
		filepath.Join(modDir, "About", "About.xml"),
		filepath.Join(modDir, "about", "about.xml"),
		filepath.Join(modDir, "About", "about.xml"),
	}

	for _, c := range candidates {
		if fi, err := os.Stat(c); err == nil && !fi.IsDir() {
			return c, nil
		}
	}

	var foundPath string
	err := filepath.WalkDir(modDir, func(path string, d fs.DirEntry, err error) error {
		if err != nil {
			return nil
		}
		rel, relErr := filepath.Rel(modDir, path)
		if relErr == nil {
			depth := len(strings.Split(rel, string(filepath.Separator)))
			if depth > 4 {
				if d.IsDir() {
					return fs.SkipDir
				}
				return nil
			}
		}

		if !d.IsDir() && strings.EqualFold(d.Name(), "About.xml") {
			foundPath = path
			return fs.SkipAll
		}
		return nil
	})

	if err != nil && !errors.Is(err, fs.SkipAll) {
		return "", err
	}

	if foundPath != "" {
		return foundPath, nil
	}

	return "", os.ErrNotExist
}
