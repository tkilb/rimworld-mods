package order

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestGenerateModsConfigXML(t *testing.T) {
	activeMods := []string{
		"brrainz.harmony",
		"ludeon.rimworld",
		"ludeon.rimworld.biotech",
		"zal.cloning",
	}
	knownExpansions := []string{
		"ludeon.rimworld.biotech",
	}
	version := "1.6.4871 rev600"

	xmlBytes, err := GenerateModsConfigXML(activeMods, knownExpansions, version)
	if err != nil {
		t.Fatalf("GenerateModsConfigXML failed: %v", err)
	}

	content := string(xmlBytes)

	if !strings.HasPrefix(content, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n") {
		t.Errorf("expected XML header, got:\n%s", content)
	}

	expectedSubstrings := []string{
		"<ModsConfigData>",
		"<version>1.6.4871 rev600</version>",
		"<activeMods>",
		"<li>brrainz.harmony</li>",
		"<li>ludeon.rimworld</li>",
		"<li>ludeon.rimworld.biotech</li>",
		"<li>zal.cloning</li>",
		"</activeMods>",
		"<knownExpansions>",
		"<li>ludeon.rimworld.biotech</li>",
		"</knownExpansions>",
		"</ModsConfigData>",
	}

	for _, sub := range expectedSubstrings {
		if !strings.Contains(content, sub) {
			t.Errorf("generated XML missing expected substring %q:\n%s", sub, content)
		}
	}
}

func TestReadExistingVersion(t *testing.T) {
	tmpDir := t.TempDir()
	configPath := filepath.Join(tmpDir, "ModsConfig.xml")
	lastPlayedPath := filepath.Join(tmpDir, "LastPlayedVersion.txt")

	// Initially neither exists
	if v := ReadExistingVersion(configPath); v != "" {
		t.Errorf("expected empty version, got %q", v)
	}

	// Create LastPlayedVersion.txt
	if err := os.WriteFile(lastPlayedPath, []byte("1.6.4871\n"), 0644); err != nil {
		t.Fatal(err)
	}
	if v := ReadExistingVersion(configPath); v != "1.6.4871" {
		t.Errorf("expected fallback version from LastPlayedVersion.txt '1.6.4871', got %q", v)
	}

	// Create ModsConfig.xml with explicit version (should take precedence)
	existingXML := `<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData>
  <version>1.6.4871 rev600</version>
  <activeMods>
    <li>ludeon.rimworld</li>
  </activeMods>
</ModsConfigData>`
	if err := os.WriteFile(configPath, []byte(existingXML), 0644); err != nil {
		t.Fatal(err)
	}

	if v := ReadExistingVersion(configPath); v != "1.6.4871 rev600" {
		t.Errorf("expected version from ModsConfig.xml '1.6.4871 rev600', got %q", v)
	}
}

func TestDeployModsConfig(t *testing.T) {
	tmpDir := t.TempDir()
	configPath := filepath.Join(tmpDir, "Config", "ModsConfig.xml")

	testContent := []byte("<ModsConfigData><version>1.0</version></ModsConfigData>\n")

	// 1. Dry run on non-existent file
	bak, err := DeployModsConfig(configPath, testContent, true)
	if err != nil {
		t.Fatalf("dry run DeployModsConfig error: %v", err)
	}
	if bak != "" {
		t.Errorf("expected no backup path on non-existent file, got %s", bak)
	}
	if _, err := os.Stat(configPath); !os.IsNotExist(err) {
		t.Errorf("dry-run should not create destination file %s", configPath)
	}

	// 2. Real deploy on non-existent file
	bak, err = DeployModsConfig(configPath, testContent, false)
	if err != nil {
		t.Fatalf("deploy error: %v", err)
	}
	if bak != "" {
		t.Errorf("expected no backup path on new file, got %s", bak)
	}
	data, err := os.ReadFile(configPath)
	if err != nil {
		t.Fatalf("failed to read created file: %v", err)
	}
	if string(data) != string(testContent) {
		t.Errorf("file content mismatch: got %q, want %q", string(data), string(testContent))
	}

	// 3. Dry run with existing file (should indicate backup would be made)
	newContent := []byte("<ModsConfigData><version>2.0</version></ModsConfigData>\n")
	bak, err = DeployModsConfig(configPath, newContent, true)
	if err != nil {
		t.Fatalf("dry run on existing file error: %v", err)
	}
	if bak != configPath+".bak" {
		t.Errorf("expected dry-run backup path %s, got %s", configPath+".bak", bak)
	}
	// Verify content unchanged by dry-run
	data, _ = os.ReadFile(configPath)
	if string(data) != string(testContent) {
		t.Errorf("dry run modified file!")
	}

	// 4. Real deploy with existing file
	bak, err = DeployModsConfig(configPath, newContent, false)
	if err != nil {
		t.Fatalf("deploy over existing file error: %v", err)
	}
	if bak != configPath+".bak" {
		t.Errorf("expected backup path %s, got %s", configPath+".bak", bak)
	}
	// Verify backup content is testContent
	bakData, err := os.ReadFile(bak)
	if err != nil {
		t.Fatalf("failed to read backup file %s: %v", bak, err)
	}
	if string(bakData) != string(testContent) {
		t.Errorf("backup content mismatch: got %q, want %q", string(bakData), string(testContent))
	}
	// Verify new content is deployed
	newData, err := os.ReadFile(configPath)
	if err != nil {
		t.Fatalf("failed to read updated file: %v", err)
	}
	if string(newData) != string(newContent) {
		t.Errorf("updated content mismatch: got %q, want %q", string(newData), string(newContent))
	}
}
