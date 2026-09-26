#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=scripts/common.sh
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

MOD_NAME=""
TARGET_VERSION=""
DRY_RUN=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --help|-h)
      echo "Usage: $0 [MOD_NAME] [TARGET_VERSION] [--dry-run]"
      echo "   or: make rollback MOD=<name> VERSION=<ver>"
      echo ""
      echo "Options:"
      echo "  MOD_NAME        Mod identifier to rollback (e.g. hugslib)"
      echo "  TARGET_VERSION  Target release tag, git commit hash, or version string"
      echo "  --dry-run       Preview rollback changes without modifying manifests or filesystem"
      exit 0
      ;;
    *)
      if [[ -z "$MOD_NAME" ]]; then
        MOD_NAME="$1"
      elif [[ -z "$TARGET_VERSION" ]]; then
        TARGET_VERSION="$1"
      else
        log_err "Unknown argument: $1"
        exit 1
      fi
      shift
      ;;
  esac
done

if [[ -z "$MOD_NAME" || -z "$TARGET_VERSION" ]]; then
  log_err "Missing required arguments."
  echo "Usage: make rollback MOD=<name> VERSION=<version>"
  exit 1
fi

MANIFEST_FILE="$REPO_ROOT/manifests/mods.yaml"
LOCK_FILE="$REPO_ROOT/manifests/mods.lock.yaml"
VENDOR_DIR="$REPO_ROOT/mods/vendor"

if [[ ! -f "$MANIFEST_FILE" ]]; then
  log_err "Manifest file not found: $MANIFEST_FILE"
  exit 1
fi

log_info "Initiating rollback for mod '$MOD_NAME' to target version '$TARGET_VERSION'..."

if $DRY_RUN; then
  log_info "[DRY-RUN] Enabled. No files will be modified."
fi

# Python helper to inspect mod source and update manifest and lockfile
MOD_SOURCE=$(python3 - "$MANIFEST_FILE" "$MOD_NAME" << 'EOF'
import sys
import yaml

manifest_file = sys.argv[1]
mod_id = sys.argv[2]

with open(manifest_file, 'r') as f:
    data = yaml.safe_load(f) or {}

mods = data.get('mods', {})
if mod_id not in mods:
    sys.exit(2)

print(mods[mod_id].get('source', ''))
EOF
) || {
  rc=$?
  if [[ $rc -eq 2 ]]; then
    log_err "Mod '$MOD_NAME' not found in $MANIFEST_FILE"
  else
    log_err "Failed reading manifest for '$MOD_NAME'"
  fi
  exit 1
}

log_info "Mod source type: $MOD_SOURCE"

if $DRY_RUN; then
  echo "  [DRY-RUN] Would update manifests/mods.yaml tag/version pin for $MOD_NAME to $TARGET_VERSION"
  echo "  [DRY-RUN] Would re-fetch $MOD_NAME at $TARGET_VERSION into $VENDOR_DIR/$MOD_NAME"
  echo "  [DRY-RUN] Would update lockfile manifests/mods.lock.yaml"
  log_succ "Dry-run rollback completed successfully."
  exit 0
fi

# Update manifests/mods.yaml
python3 - "$MANIFEST_FILE" "$MOD_NAME" "$TARGET_VERSION" "$MOD_SOURCE" << 'EOF'
import sys
import yaml

manifest_file, mod_id, version, source = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]

with open(manifest_file, 'r') as f:
    data = yaml.safe_load(f) or {}

if 'mods' in data and mod_id in data['mods']:
    mod = data['mods'][mod_id]
    if source == 'github_release':
        mod['tag'] = version
    elif source == 'git':
        mod['branch'] = version
    elif source == 'steam_workshop':
        mod['pinned_version'] = version

with open(manifest_file, 'w') as f:
    yaml.dump(data, f, sort_keys=False)

EOF
log_succ "Updated $MANIFEST_FILE pin for $MOD_NAME."

# Re-fetch mod using fetch-mods.sh
log_info "Fetching rolled-back version for $MOD_NAME..."
bash "$SCRIPT_DIR/fetch-mods.sh" "$MOD_NAME"

# Update lockfile entry
python3 - "$LOCK_FILE" "$MOD_NAME" "$TARGET_VERSION" "$MOD_SOURCE" << 'EOF'
import sys
import os
from datetime import datetime, timezone
import yaml

lock_file, mod_id, version, source = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]

lock_data = {}
if os.path.exists(lock_file):
    with open(lock_file, 'r') as f:
        lock_data = yaml.safe_load(f) or {}

if 'mods' not in lock_data:
    lock_data['mods'] = {}

entry = lock_data['mods'].get(mod_id, {})
entry['source'] = source
entry['download_path'] = f"mods/vendor/{mod_id}"

if source == 'github_release':
    entry['resolved_tag'] = version
elif source == 'git':
    entry['resolved_commit'] = version
elif source == 'steam_workshop':
    entry['resolved_version'] = version

lock_data['mods'][mod_id] = entry
lock_data['generated_at'] = datetime.now(timezone.utc).isoformat()

with open(lock_file, 'w') as f:
    yaml.dump(lock_data, f, sort_keys=False)

EOF
log_succ "Updated $LOCK_FILE for $MOD_NAME."

log_succ "Rollback of mod '$MOD_NAME' to '$TARGET_VERSION' completed."
