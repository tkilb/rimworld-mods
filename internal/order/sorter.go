package order

import (
	"fmt"
	"sort"
	"strings"

	"rimworld-mods/internal/manifest"
)

// Tier constants for anchor-based ordering.
const (
	TierHarmony  = 0
	TierCore     = 1
	TierDLC      = 2
	TierStandard = 3
	TierTrailing = 4
)

// Well-known RimWorld anchor package IDs (normalized to lower case).
const (
	PkgHarmony  = "brrainz.harmony"
	PkgCore     = "ludeon.rimworld"
	PkgRoyalty  = "ludeon.rimworld.royalty"
	PkgIdeology = "ludeon.rimworld.ideology"
	PkgBiotech  = "ludeon.rimworld.biotech"
	PkgAnomaly  = "ludeon.rimworld.anomaly"
	PkgOdyssey  = "ludeon.rimworld.odyssey"
)

// Canonical DLC ordering.
var CanonicalDLCs = []string{
	PkgRoyalty,
	PkgIdeology,
	PkgBiotech,
	PkgAnomaly,
	PkgOdyssey,
}

// Well-known late/trailing mods.
var KnownTrailingMods = map[string]bool{
	"krkr.rocketman": true,
}

// Well-known base frameworks that should load immediately after DLCs before general content mods.
var KnownFrameworkMods = map[string]int{
	"unlimitedhugs.hugslib":                     1000,
	"smashphil.vehicleframework":                900,
	"oskarpotocki.vanillafactionsexpanded.core": 800,
	"ilyvion.laboratory":                        700,
}

// ModNode represents a mod in the dependency graph.
type ModNode struct {
	ID               string
	PackageID        string
	Name             string
	Source           string // "anchor", "vendor", "custom", "manifest"
	Tier             int
	Priority         int
	Trailing         bool
	LoadAfter        []string
	LoadBefore       []string
	Dependencies     []string
	IncompatibleWith []string
	Dir              string
	HasAboutXML      bool
}

// NormalizedPackageID returns the lower-case package ID for case-insensitive matching.
func (n *ModNode) NormalizedPackageID() string {
	return strings.ToLower(n.PackageID)
}

// ResolveResult holds the ordered list of mods and any diagnostic warnings.
type ResolveResult struct {
	OrderedMods []*ModNode
	Warnings    []string
}

// BuildModNode creates a ModNode from manifest entry, optional About.xml metadata, and directory.
func BuildModNode(id string, entry manifest.ModEntry, meta *ModMetaData, dir string, isCustom bool) *ModNode {
	node := &ModNode{
		ID:        id,
		PackageID: entry.PackageID,
		Name:      entry.Name,
		Source:    entry.Source,
		Priority:  entry.Priority,
		Trailing:  entry.Trailing,
		Dir:       dir,
	}

	if isCustom {
		node.Source = "custom"
	}

	if meta != nil {
		node.HasAboutXML = true
		if meta.PackageID != "" {
			node.PackageID = meta.PackageID
		}
		if meta.Name != "" && node.Name == "" {
			node.Name = meta.Name
		}

		// Combine About.xml ordering rules
		node.LoadAfter = append(node.LoadAfter, meta.LoadAfter...)
		node.LoadAfter = append(node.LoadAfter, meta.ForceLoadAfter...)
		node.LoadBefore = append(node.LoadBefore, meta.LoadBefore...)
		node.LoadBefore = append(node.LoadBefore, meta.ForceLoadBefore...)
		node.IncompatibleWith = append(node.IncompatibleWith, meta.IncompatibleWith...)

		for _, dep := range meta.ModDependencies {
			node.Dependencies = append(node.Dependencies, dep.PackageID)
			// A mod dependency is also an implicit loadAfter
			node.LoadAfter = append(node.LoadAfter, dep.PackageID)
		}
	}

	// Merge manifest overrides
	node.LoadAfter = append(node.LoadAfter, entry.OrderAfter...)
	node.LoadBefore = append(node.LoadBefore, entry.OrderBefore...)

	// Determine Tier
	normPkg := node.NormalizedPackageID()
	switch normPkg {
	case PkgHarmony:
		node.Tier = TierHarmony
	case PkgCore:
		node.Tier = TierCore
	case PkgRoyalty, PkgIdeology, PkgBiotech, PkgAnomaly, PkgOdyssey:
		node.Tier = TierDLC
	default:
		if node.Trailing || KnownTrailingMods[normPkg] {
			node.Tier = TierTrailing
		} else {
			node.Tier = TierStandard
		}
	}

	// Apply default framework priority if priority is not explicitly set
	if node.Priority == 0 {
		if pri, ok := KnownFrameworkMods[normPkg]; ok {
			node.Priority = pri
		}
	}

	return node
}

// CreateAnchorNode creates synthetic nodes for Core and official expansions when needed.
func CreateAnchorNode(pkgID string, name string, tier int) *ModNode {
	return &ModNode{
		ID:        pkgID,
		PackageID: pkgID,
		Name:      name,
		Source:    "anchor",
		Tier:      tier,
	}
}

// Sorter coordinates dependency resolution and topological sort.
type Sorter struct {
	nodes           map[string]*ModNode // keyed by normalized package ID
	originalIndices map[string]int
}

// NewSorter initializes a sorter instance.
func NewSorter() *Sorter {
	return &Sorter{
		nodes:           make(map[string]*ModNode),
		originalIndices: make(map[string]int),
	}
}

// AddNode adds a mod node to the resolver.
func (s *Sorter) AddNode(node *ModNode) {
	norm := node.NormalizedPackageID()
	if _, exists := s.originalIndices[norm]; !exists {
		s.originalIndices[norm] = len(s.originalIndices)
	}
	s.nodes[norm] = node
}

// Sort executes Kahn's topological sort and enforces strict anchor tiering.
func (s *Sorter) Sort(availableDLCs []string) (*ResolveResult, error) {
	var warnings []string

	// Ensure Core is present in nodes
	if _, ok := s.nodes[PkgCore]; !ok {
		s.AddNode(CreateAnchorNode(PkgCore, "Core", TierCore))
	}

	// Ensure available DLCs are present in nodes
	for _, dlcPkg := range availableDLCs {
		normDLC := strings.ToLower(dlcPkg)
		if _, ok := s.nodes[normDLC]; !ok {
			name := strings.TrimPrefix(normDLC, "ludeon.rimworld.")
			name = strings.ToUpper(name[:1]) + name[1:]
			s.AddNode(CreateAnchorNode(normDLC, name, TierDLC))
		}
	}

	// 1. Check for missing dependencies and incompatibilities
	for _, node := range s.nodes {
		for _, dep := range node.Dependencies {
			normDep := strings.ToLower(dep)
			if _, exists := s.nodes[normDep]; !exists {
				warnings = append(warnings, fmt.Sprintf(
					"Mod '%s' (%s) requires dependency '%s', which is not installed or enabled",
					node.Name, node.PackageID, dep,
				))
			}
		}

		for _, incomp := range node.IncompatibleWith {
			normIncomp := strings.ToLower(incomp)
			if conflicting, exists := s.nodes[normIncomp]; exists {
				warnings = append(warnings, fmt.Sprintf(
					"Mod '%s' (%s) is incompatible with active mod '%s' (%s)",
					node.Name, node.PackageID, conflicting.Name, conflicting.PackageID,
				))
			}
		}
	}

	// 2. Partition nodes into tiers
	var harmonyNodes []*ModNode
	var coreNodes []*ModNode
	var dlcNodes []*ModNode
	var standardNodes []*ModNode
	var trailingNodes []*ModNode

	for _, node := range s.nodes {
		switch node.Tier {
		case TierHarmony:
			harmonyNodes = append(harmonyNodes, node)
		case TierCore:
			coreNodes = append(coreNodes, node)
		case TierDLC:
			dlcNodes = append(dlcNodes, node)
		case TierTrailing:
			trailingNodes = append(trailingNodes, node)
		default:
			standardNodes = append(standardNodes, node)
		}
	}

	// Sort DLCs by canonical release order
	sort.Slice(dlcNodes, func(i, j int) bool {
		rank := func(pkg string) int {
			for idx, c := range CanonicalDLCs {
				if c == pkg {
					return idx
				}
			}
			return 99
		}
		return rank(dlcNodes[i].NormalizedPackageID()) < rank(dlcNodes[j].NormalizedPackageID())
	})

	// 3. Topologically sort standard mods (Tier 3)
	sortedStandard, err := s.topologicalSortTier(standardNodes)
	if err != nil {
		return nil, err
	}

	// 4. Topologically sort trailing mods (Tier 4)
	sortedTrailing, err := s.topologicalSortTier(trailingNodes)
	if err != nil {
		return nil, err
	}

	// Assemble final ordered slice
	var finalOrder []*ModNode
	finalOrder = append(finalOrder, harmonyNodes...)
	finalOrder = append(finalOrder, coreNodes...)
	finalOrder = append(finalOrder, dlcNodes...)
	finalOrder = append(finalOrder, sortedStandard...)
	finalOrder = append(finalOrder, sortedTrailing...)

	return &ResolveResult{
		OrderedMods: finalOrder,
		Warnings:    warnings,
	}, nil
}

// topologicalSortTier runs Kahn's algorithm with cycle detection within a specific tier.
func (s *Sorter) topologicalSortTier(tierNodes []*ModNode) ([]*ModNode, error) {
	if len(tierNodes) <= 1 {
		return tierNodes, nil
	}

	tierSet := make(map[string]bool)
	nodeMap := make(map[string]*ModNode)
	for _, n := range tierNodes {
		norm := n.NormalizedPackageID()
		tierSet[norm] = true
		nodeMap[norm] = n
	}

	edges := make(map[string][]string)
	inDegree := make(map[string]int)

	for _, n := range tierNodes {
		norm := n.NormalizedPackageID()
		inDegree[norm] = 0
	}

	for _, n := range tierNodes {
		u := n.NormalizedPackageID()

		for _, dep := range n.LoadAfter {
			v := strings.ToLower(dep)
			if tierSet[v] && v != u {
				if !containsString(edges[v], u) {
					edges[v] = append(edges[v], u)
					inDegree[u]++
				}
			}
		}

		for _, dep := range n.LoadBefore {
			v := strings.ToLower(dep)
			if tierSet[v] && v != u {
				if !containsString(edges[u], v) {
					edges[u] = append(edges[u], v)
					inDegree[v]++
				}
			}
		}
	}

	var queue []string
	for _, n := range tierNodes {
		norm := n.NormalizedPackageID()
		if inDegree[norm] == 0 {
			queue = append(queue, norm)
		}
	}

	sortQueue := func(q []string) {
		sort.Slice(q, func(i, j int) bool {
			ni := nodeMap[q[i]]
			nj := nodeMap[q[j]]
			if ni.Priority != nj.Priority {
				return ni.Priority > nj.Priority
			}
			idxI := s.originalIndices[q[i]]
			idxJ := s.originalIndices[q[j]]
			if idxI != idxJ {
				return idxI < idxJ
			}
			return ni.PackageID < nj.PackageID
		})
	}

	sortQueue(queue)

	var result []*ModNode
	for len(queue) > 0 {
		u := queue[0]
		queue = queue[1:]
		result = append(result, nodeMap[u])

		for _, v := range edges[u] {
			inDegree[v]--
			if inDegree[v] == 0 {
				queue = append(queue, v)
			}
		}
		sortQueue(queue)
	}

	if len(result) < len(tierNodes) {
		cyclePath := s.findCycle(tierSet, edges, inDegree)
		return nil, fmt.Errorf("circular dependency detected in mod load order:\n  %s", cyclePath)
	}

	return result, nil
}

func (s *Sorter) findCycle(tierSet map[string]bool, edges map[string][]string, inDegree map[string]int) string {
	var remaining []string
	for norm := range tierSet {
		if inDegree[norm] > 0 {
			remaining = append(remaining, norm)
		}
	}

	visited := make(map[string]int)
	var path []string
	var cycle []string

	var dfs func(u string) bool
	dfs = func(u string) bool {
		visited[u] = 1
		path = append(path, u)

		for _, v := range edges[u] {
			if inDegree[v] <= 0 {
				continue
			}
			if visited[v] == 1 {
				startIdx := -1
				for i, p := range path {
					if p == v {
						startIdx = i
						break
					}
				}
				if startIdx != -1 {
					cycle = append(cycle, path[startIdx:]...)
					cycle = append(cycle, v)
				}
				return true
			}
			if visited[v] == 0 {
				if dfs(v) {
					return true
				}
			}
		}

		path = path[:len(path)-1]
		visited[u] = 2
		return false
	}

	for _, u := range remaining {
		if visited[u] == 0 {
			if dfs(u) {
				break
			}
		}
	}

	if len(cycle) == 0 {
		return strings.Join(remaining, " <-> ")
	}

	var formatted []string
	for _, c := range cycle {
		if node, ok := s.nodes[c]; ok {
			formatted = append(formatted, fmt.Sprintf("%s (%s)", node.Name, node.PackageID))
		} else {
			formatted = append(formatted, c)
		}
	}

	return strings.Join(formatted, " \n    ↳ ")
}

func containsString(slice []string, val string) bool {
	for _, s := range slice {
		if s == val {
			return true
		}
	}
	return false
}
