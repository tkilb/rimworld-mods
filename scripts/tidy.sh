#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=scripts/common.sh
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

DRY_RUN=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --help|-h)
      echo "Usage: $0 [--dry-run]"
      echo ""
      echo "Tidies the monorepo, analogous to 'go mod tidy':"
      echo "  1. Prunes orphaned entries from manifests/mods.lock.yaml not declared in mods.yaml"
      echo "  2. Prunes unused cached downloads from mods/vendor/"
      echo "  3. Prunes broken or orphaned monorepo symlinks from RimWorld's Mods directory"
      echo "  4. Runs 'go mod tidy' on tools/load-order"
      echo ""
      echo "Options:"
      echo "  --dry-run  Preview cleanup actions without modifying files"
      echo "  -h, --help Show this help message"
      exit 0
      ;;
    *)
      log_err "Unknown argument: $1"
      exit 1
      ;;
  esac
done

MANIFEST_FILE="$REPO_ROOT/manifests/mods.yaml"
LOCK_FILE="$REPO_ROOT/manifests/mods.lock.yaml"
VENDOR_DIR="$REPO_ROOT/mods/vendor"
CUSTOM_DIR="$REPO_ROOT/mods/custom"

if [[ ! -f "$MANIFEST_FILE" ]]; then
  log_err "Manifest file not found: $MANIFEST_FILE"
  exit 1
fi

log_info "Running monorepo tidy (dry-run: $DRY_RUN)..."

# 1. Prune lockfile entries not declared in manifests/mods.yaml
python3 - "$MANIFEST_FILE" "$LOCK_FILE" "$DRY_RUN" << 'EOF'
import sys
import os
from datetime import datetime, timezone
import yaml

manifest_file = sys.argv[1]
lock_file = sys.argv[2]
dry_run = sys.argv[3].lower() == "true"

if not os.path.exists(lock_file):
    print("No lockfile found. Skipping lockfile pruning.")
    sys.exit(0)

with open(manifest_file, 'r') as f:
    manifest_data = yaml.safe_load(f) or {}

with open(lock_file, 'r') as f:
    lock_data = yaml.safe_load(f) or {}

manifest_mods = manifest_data.get('mods', {})
locked_mods = lock_data.get('mods', {})

orphans = [m for m in locked_mods.keys() if m not in manifest_mods]

if orphans:
    print(f"\033[34m[INFO]\033[0m Found {len(orphans)} orphaned lockfile entry/entries: {', '.join(orphans)}")
    for mod_id in orphans:
        if dry_run:
            print(f"  [DRY-RUN] Would remove '{mod_id}' from manifests/mods.lock.yaml")
        else:
            del locked_mods[mod_id]
            print(f"  \033[32m[OK]\033[0m Removed '{mod_id}' from lockfile")
    
    if not dry_run:
        lock_data['mods'] = locked_mods
        lock_data['generated_at'] = datetime.now(timezone.utc).isoformat()
        with open(lock_file, 'w') as f:
            yaml.dump(lock_data, f, sort_keys=False)
        print("\033[32m[OK]\033[0m Updated manifests/mods.lock.yaml")
else:
    print("\033[32m[OK]\033[0m Lockfile is already clean (no orphaned mod entries).")
EOF

# 2. Prune orphaned vendor downloads in mods/vendor/
if [[ -d "$VENDOR_DIR" ]]; then
  python3 - "$MANIFEST_FILE" "$VENDOR_DIR" "$DRY_RUN" << 'EOF'
import sys
import os
import shutil
import yaml

manifest_file = sys.argv[1]
vendor_dir = sys.argv[2]
dry_run = sys.argv[3].lower() == "true"

with open(manifest_file, 'r') as f:
    manifest_data = yaml.safe_load(f) or {}

manifest_mods = manifest_data.get('mods', {})

orphaned_dirs = []
for entry in sorted(os.listdir(vendor_dir)):
    if entry.startswith('.') or entry == '.gitkeep':
        continue
    dir_path = os.path.join(vendor_dir, entry)
    if os.path.isdir(dir_path) and entry not in manifest_mods:
        orphaned_dirs.append((entry, dir_path))

if orphaned_dirs:
    print(f"\033[34m[INFO]\033[0m Found {len(orphaned_dirs)} unused vendor cache folder(s): {', '.join([o[0] for o in orphaned_dirs])}")
    for mod_id, dir_path in orphaned_dirs:
        if dry_run:
            print(f"  [DRY-RUN] Would delete unused directory: {dir_path}")
        else:
            shutil.rmtree(dir_path)
            print(f"  \033[32m[OK]\033[0m Deleted unused directory: {dir_path}")
else:
    print("\033[32m[OK]\033[0m Vendor cache is clean (no orphaned directories).")
EOF
fi

# 3. Prune broken or orphaned symlinks in RimWorld Mods directory
PROTECTED_DIRS=("Core" "Royalty" "Ideology" "Biotech" "Anomaly")
is_protected() {
  local target_name="$1"
  for prot in "${PROTECTED_DIRS[@]}"; do
    if [[ "${target_name,,}" == "${prot,,}" ]]; then
      return 0
    fi
  done
  return 1
}

if [[ -d "$RIMWORLD_MODS_DIR" ]]; then
  log_info "Checking RimWorld Mods directory for dangling or orphaned symlinks: $RIMWORLD_MODS_DIR"
  vendor_canon="$(readlink -f "$VENDOR_DIR" || true)"
  custom_canon="$(readlink -f "$CUSTOM_DIR" || true)"

  for item in "$RIMWORLD_MODS_DIR"/*; do
    [[ -L "$item" ]] || continue
    item_name="$(basename "$item")"

    if is_protected "$item_name"; then
      continue
    fi

    link_target="$(readlink -f "$item" || true)"

    # Check if this symlink points to this monorepo
    if [[ -z "$link_target" || ! -e "$item" ]]; then
      # Broken / dangling symlink
      if $DRY_RUN; then
        echo "  [DRY-RUN] Would remove broken symlink: $item"
      else
        rm "$item"
        log_succ "Removed broken symlink: $item"
      fi
    elif [[ "$link_target" == "$vendor_canon"* || "$link_target" == "$custom_canon"* ]]; then
      # Symlink points into repo, verify if mod is still declared or custom
      is_valid=false
      if [[ -d "$CUSTOM_DIR/$item_name" ]]; then
        is_valid=true
      fi
      if python3 -c "import sys, yaml; d=yaml.safe_load(open('$MANIFEST_FILE')); sys.exit(0 if '$item_name' in d.get('mods', {}) else 1)" 2>/dev/null; then
        is_valid=true
      fi

      if [[ "$is_valid" != "true" ]]; then
        if $DRY_RUN; then
          echo "  [DRY-RUN] Would remove orphaned symlink: $item -> $link_target"
        else
          rm "$item"
          log_succ "Removed orphaned symlink: $item"
        fi
      fi
    fi
  done
fi

# 4. Tidy Go module in tools/load-order
if [[ -d "$REPO_ROOT/tools/load-order" ]] && command -v go >/dev/null 2>&1; then
  log_info "Tidying Go load-order module..."
  if $DRY_RUN; then
    echo "  [DRY-RUN] Would run: (cd tools/load-order && go mod tidy)"
  else
    (cd "$REPO_ROOT/tools/load-order" && go mod tidy)
    log_succ "tools/load-order Go dependencies tidied."
  fi
fi

log_succ "Tidy complete."
