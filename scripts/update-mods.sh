#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=scripts/common.sh
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

DRY_RUN=false
TARGET_MOD=""
FETCH_AFTER_UPDATE=true

# Parse arguments
while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --no-fetch)
      FETCH_AFTER_UPDATE=false
      shift
      ;;
    --help|-h)
      echo "Usage: $0 [MOD_NAME] [--dry-run] [--no-fetch]"
      echo ""
      echo "Options:"
      echo "  MOD_NAME    Optional specific mod identifier to update (e.g. harmony)"
      echo "  --dry-run   Check for updates without downloading or updating lockfile"
      echo "  --no-fetch  Update lockfile metadata without fetching files"
      exit 0
      ;;
    *)
      if [[ -z "$TARGET_MOD" ]]; then
        TARGET_MOD="$1"
      else
        log_err "Unknown argument: $1"
        exit 1
      fi
      shift
      ;;
  esac
done

MANIFEST_FILE="$REPO_ROOT/manifests/mods.yaml"
LOCK_FILE="$REPO_ROOT/manifests/mods.lock.yaml"

if [[ ! -f "$MANIFEST_FILE" ]]; then
  log_err "Manifest file not found: $MANIFEST_FILE"
  exit 1
fi

if $DRY_RUN; then
  log_info "[DRY-RUN] Update check mode enabled."
fi

# Python script to check versions and update lockfile
python3 - "$MANIFEST_FILE" "$LOCK_FILE" "$TARGET_MOD" "$DRY_RUN" "$MACHINE" << 'EOF'
import sys
import os
import json
import urllib.request
import urllib.parse
from datetime import datetime, timezone
import yaml

manifest_file = sys.argv[1]
lock_file = sys.argv[2]
target_mod = sys.argv[3] if len(sys.argv) > 3 else ""
dry_run = sys.argv[4].lower() == "true"
machine = sys.argv[5] if len(sys.argv) > 5 else "unknown"

with open(manifest_file, 'r') as f:
    manifest_data = yaml.safe_load(f) or {}

lock_data = {}
if os.path.exists(lock_file):
    with open(lock_file, 'r') as f:
        lock_data = yaml.safe_load(f) or {}

if 'mods' not in lock_data:
    lock_data['mods'] = {}

manifest_mods = manifest_data.get('mods', {})
updated_mods_count = 0

for mod_id, mod in manifest_mods.items():
    if target_mod and mod_id != target_mod:
        continue

    if not mod.get('enabled', True):
        print(f"\033[34m[INFO]\033[0m Skipping disabled mod: {mod_id}")
        continue

    source = mod.get('source', '')
    current_lock = lock_data['mods'].get(mod_id, {})
    new_entry = {
        'source': source,
        'download_path': f"mods/vendor/{mod_id}"
    }

    print(f"\033[34m[INFO]\033[0m Checking mod: {mod.get('name', mod_id)} ({mod_id}) [{source}]")

    if source == 'steam_workshop':
        workshop_id = str(mod.get('workshop_id', ''))
        new_entry['workshop_id'] = workshop_id
        
        # Query Steam Remote Storage API
        try:
            url = "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/"
            post_data = urllib.parse.urlencode({
                'itemcount': '1',
                'publishedfileids[0]': workshop_id
            }).encode('utf-8')
            
            req = urllib.request.Request(url, data=post_data, headers={'User-Agent': 'RimWorldMonorepo/1.0'})
            with urllib.request.urlopen(req, timeout=10) as resp:
                res = json.loads(resp.read().decode('utf-8'))
                details = res.get('response', {}).get('publishedfiledetails', [{}])[0]
                time_updated = details.get('time_updated', 0)
                title = details.get('title', '')
                if time_updated:
                    new_entry['workshop_time_updated'] = time_updated
                    new_entry['workshop_title'] = title
                    print(f"  Workshop item '{title}' last updated: {datetime.fromtimestamp(time_updated, tz=timezone.utc).isoformat()}")
        except Exception as e:
            print(f"\033[33m[WARN]\033[0m Could not query Steam API for {mod_id}: {e}")
            if 'workshop_time_updated' in current_lock:
                new_entry['workshop_time_updated'] = current_lock['workshop_time_updated']

    elif source == 'github_release':
        repo = mod.get('repo', '')
        tag = mod.get('tag', '')
        new_entry['repo'] = repo
        
        api_url = f"https://api.github.com/repos/{repo}/releases/tags/{tag}" if (tag and tag != 'latest') else f"https://api.github.com/repos/{repo}/releases/latest"
        try:
            req = urllib.request.Request(api_url, headers={
                'Accept': 'application/vnd.github.v3+json',
                'User-Agent': 'RimWorldMonorepo/1.0'
            })
            with urllib.request.urlopen(req, timeout=10) as resp:
                rel = json.loads(resp.read().decode('utf-8'))
                resolved_tag = rel.get('tag_name', tag)
                published_at = rel.get('published_at', '')
                new_entry['resolved_tag'] = resolved_tag
                new_entry['published_at'] = published_at
                
                # Check for zip assets
                for asset in rel.get('assets', []):
                    if asset.get('name', '').endswith('.zip'):
                        new_entry['asset_name'] = asset.get('name')
                        break
                print(f"  GitHub release resolved: {resolved_tag} (published: {published_at})")
        except Exception as e:
            print(f"\033[33m[WARN]\033[0m Could not query GitHub API for {repo}: {e}")
            if 'resolved_tag' in current_lock:
                new_entry['resolved_tag'] = current_lock['resolved_tag']

    elif source == 'git':
        url = mod.get('url', '')
        branch = mod.get('branch', 'main')
        new_entry['url'] = url
        new_entry['branch'] = branch
        # Store placeholder or branch info
        if 'resolved_commit' in current_lock:
            new_entry['resolved_commit'] = current_lock['resolved_commit']

    if dry_run:
        print(f"  [DRY-RUN] Proposed lock entry: {new_entry}")
    else:
        lock_data['mods'][mod_id] = new_entry
        updated_mods_count += 1

if not dry_run:
    lock_data['version'] = 1
    lock_data['generated_at'] = datetime.now(timezone.utc).isoformat()
    lock_data['machine'] = machine
    with open(lock_file, 'w') as f:
        yaml.dump(lock_data, f, sort_keys=False)
    print(f"\033[32m[OK]\033[0m Synchronized {updated_mods_count} mod entries to {lock_file}")

EOF

if $DRY_RUN; then
  log_succ "Dry-run update check completed."
  exit 0
fi

if $FETCH_AFTER_UPDATE; then
  log_info "Fetching updated mods..."
  if [[ -n "$TARGET_MOD" ]]; then
    bash "$SCRIPT_DIR/fetch-mods.sh" "$TARGET_MOD"
  else
    bash "$SCRIPT_DIR/fetch-mods.sh"
  fi
fi

log_succ "Update and lockfile synchronization complete."
