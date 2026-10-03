#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=scripts/common.sh
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

MANIFEST_FILE="$REPO_ROOT/manifests/mods.yaml"
LOCK_FILE="$REPO_ROOT/manifests/mods.lock.yaml"
VENDOR_DIR="$REPO_ROOT/mods/vendor"
CUSTOM_DIR="$REPO_ROOT/mods/custom"

echo -e "\033[1mRimWorld Monorepo - Mod Status & Integrity Report\033[0m"
echo -e "Machine Target:        \033[36m$MACHINE\033[0m"
echo -e "RimWorld Mods Path:    \033[36m$RIMWORLD_MODS_DIR\033[0m"
echo "================================================================================"

python3 - "$MANIFEST_FILE" "$LOCK_FILE" "$VENDOR_DIR" "$CUSTOM_DIR" "$RIMWORLD_MODS_DIR" << 'EOF'
import sys
import os
import yaml
from pathlib import Path

manifest_file = sys.argv[1]
lock_file = sys.argv[2]
vendor_dir = sys.argv[3]
custom_dir = sys.argv[4]
rimworld_mods_dir = sys.argv[5]

manifest_data = {}
if os.path.exists(manifest_file):
    with open(manifest_file, 'r') as f:
        manifest_data = yaml.safe_load(f) or {}

lock_data = {}
if os.path.exists(lock_file):
    with open(lock_file, 'r') as f:
        lock_data = yaml.safe_load(f) or {}

manifest_mods = manifest_data.get('mods', {})
locked_mods = lock_data.get('mods', {})

# Format column headers
header_fmt = "{:<20} {:<8} {:<15} {:<12} {:<15} {:<15}"
print(header_fmt.format("MOD ID", "ENABLED", "SOURCE", "DOWNLOADED", "VERSION/TAG", "LINK STATUS"))
print("-" * 88)

all_mod_ids = list(manifest_mods.keys())

# Helper to format column with ANSI color without skewing table widths
def colorize(text, width, color_code):
    padded = f"{text:<{width}}"
    if color_code:
        return f"\033[{color_code}m{padded}\033[0m"
    return padded

for mod_id in all_mod_ids:
    m = manifest_mods[mod_id]
    source = m.get('source', 'unknown')
    if source in ('custom', 'local'):
        continue
    enabled = m.get('enabled', True)
    
    # Check downloaded cache
    cached_path = os.path.join(vendor_dir, mod_id)
    is_downloaded = os.path.isdir(cached_path) and any(os.scandir(cached_path))
    dl_colored = colorize("Yes" if is_downloaded else "Missing", 12, "32" if is_downloaded else "31")
    
    # Version / Lock info
    lock_entry = locked_mods.get(mod_id, {})
    version_info = "unlocked"
    if lock_entry:
        if 'resolved_tag' in lock_entry:
            version_info = lock_entry['resolved_tag']
        elif 'resolved_version' in lock_entry:
            version_info = lock_entry['resolved_version']
        elif 'workshop_time_updated' in lock_entry:
            version_info = f"rev:{lock_entry['workshop_time_updated']}"
        elif 'resolved_commit' in lock_entry:
            version_info = lock_entry['resolved_commit'][:7]

    # Symlink status
    link_path = os.path.join(rimworld_mods_dir, mod_id)
    if os.path.islink(link_path):
        target = os.path.realpath(link_path)
        expected = os.path.realpath(cached_path)
        if target == expected:
            link_colored = colorize("Linked", 15, "32")
        else:
            link_colored = colorize("Drifted Target", 15, "33")
    elif os.path.exists(link_path):
        link_colored = colorize("Direct Dir", 15, "33")
    else:
        link_colored = colorize("Unlinked", 15, "90")

    enabled_colored = colorize("yes" if enabled else "no", 8, "" if enabled else "90")
    print(f"{mod_id:<20} {enabled_colored} {source:<15} {dl_colored} {version_info:<15} {link_colored}")

# Custom mods authoring section
custom_mods = []
if os.path.isdir(custom_dir):
    for entry in sorted(os.listdir(custom_dir)):
        p = os.path.join(custom_dir, entry)
        if os.path.isdir(p) and not entry.startswith('.'):
            custom_mods.append(entry)

if custom_mods:
    print("\n\033[1mCustom Authoring Mods (mods/custom/):\033[0m")
    custom_fmt = "{:<20} {:<8} {:<15} {:<15}"
    print(custom_fmt.format("MOD ID", "ENABLED", "WORKSPACE", "LINK STATUS"))
    print("-" * 65)
    for c_id in custom_mods:
        c_path = os.path.join(custom_dir, c_id)
        c_meta = manifest_mods.get(c_id, {})
        enabled = c_meta.get('enabled', True)
        enabled_colored = colorize("yes" if enabled else "no", 8, "" if enabled else "90")
        link_path = os.path.join(rimworld_mods_dir, c_id)
        if os.path.islink(link_path):
            target = os.path.realpath(link_path)
            expected = os.path.realpath(c_path)
            if target == expected:
                l_colored = colorize("Linked", 15, "32")
            else:
                l_colored = colorize("Drifted Target", 15, "33")
        elif os.path.exists(link_path):
            l_colored = colorize("Direct Dir", 15, "33")
        else:
            l_colored = colorize("Unlinked", 15, "90")
        print(f"{c_id:<20} {enabled_colored} {'local (custom)':<15} {l_colored}")

# Check for orphan monorepo links in RimWorld mods directory
if os.path.isdir(rimworld_mods_dir):
    orphans = []
    vendor_canon = os.path.realpath(vendor_dir)
    custom_canon = os.path.realpath(custom_dir)
    for entry in os.listdir(rimworld_mods_dir):
        fp = os.path.join(rimworld_mods_dir, entry)
        if os.path.islink(fp):
            tgt = os.path.realpath(fp)
            if tgt.startswith(vendor_canon) or tgt.startswith(custom_canon):
                if entry not in manifest_mods and entry not in custom_mods:
                    orphans.append(entry)
    if orphans:
        print(f"\n\033[33m[WARN]\033[0m Orphaned monorepo symlinks found in RimWorld Mods dir: {', '.join(orphans)}")

print("")
EOF
