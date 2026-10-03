#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=scripts/common.sh
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

ACTION="link"
DRY_RUN=false
SKIP_CONFIG=false
TARGET_MOD=""

# Official Core / DLC directory names that must NEVER be overwritten or unlinked
PROTECTED_DIRS=("Core" "Royalty" "Ideology" "Biotech" "Anomaly")

# Parse command-line arguments
while [[ $# -gt 0 ]]; do
  case "$1" in
    --link)
      ACTION="link"
      shift
      ;;
    --unlink)
      ACTION="unlink"
      shift
      ;;
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --skip-config)
      SKIP_CONFIG=true
      shift
      ;;
    --help|-h)
      echo "Usage: $0 [--link|--unlink] [MOD_NAME] [--dry-run] [--skip-config]"
      echo ""
      echo "Options:"
      echo "  --link         Deploy symlinks for enabled mods into RimWorld Mods folder (default)"
      echo "  --unlink       Remove symlinks targeting monorepo mods from RimWorld Mods folder"
      echo "  MOD_NAME       Optional specific mod identifier to link/unlink"
      echo "  --dry-run      Preview link/unlink operations without modifying filesystem"
      echo "  --skip-config  Skip automatic ModsConfig.xml synchronization"
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

VENDOR_DIR="$REPO_ROOT/mods/vendor"
CUSTOM_DIR="$REPO_ROOT/mods/custom"
MANIFEST_FILE="$REPO_ROOT/manifests/mods.yaml"

is_protected() {
  local target_name="$1"
  for prot in "${PROTECTED_DIRS[@]}"; do
    if [[ "${target_name,,}" == "${prot,,}" ]]; then
      return 0
    fi
  done
  return 1
}

# Collect candidates to link/unlink: pairs of (mod_id, source_path)
declare -A CANDIDATE_MODS
declare -A DISABLED_MODS

# 1. Mods declared in manifest (vendor and custom)
if [[ -f "$MANIFEST_FILE" ]]; then
  while IFS='|' read -r mod_id mod_name enabled source; do
    if [[ "$enabled" == "false" ]]; then
      DISABLED_MODS["$mod_id"]=1
    elif [[ "$source" == "custom" || "$source" == "local" ]]; then
      candidate_path="$CUSTOM_DIR/$mod_id"
      if [[ -d "$candidate_path" ]]; then
        CANDIDATE_MODS["$mod_id"]="$candidate_path"
      fi
    else
      candidate_path="$VENDOR_DIR/$mod_id"
      if [[ -d "$candidate_path" ]]; then
        CANDIDATE_MODS["$mod_id"]="$candidate_path"
      fi
    fi
  done < <(python3 - "$MANIFEST_FILE" << 'EOF'
import sys
import yaml

with open(sys.argv[1], 'r') as f:
    data = yaml.safe_load(f) or {}

mods = data.get('mods', {})
for mod_id, mod in mods.items():
    enabled = str(mod.get('enabled', True)).lower()
    name = mod.get('name', mod_id)
    source = mod.get('source', '')
    print(f"{mod_id}|{name}|{enabled}|{source}")
EOF
)
fi

# 2. Auto-discover custom authoring mods in mods/custom/ (must contain About/About.xml)
if [[ -d "$CUSTOM_DIR" ]]; then
  for custom_path in "$CUSTOM_DIR"/*; do
    if [[ -d "$custom_path" && -f "$custom_path/About/About.xml" ]]; then
      custom_id="$(basename "$custom_path")"
      if [[ -n "${DISABLED_MODS[$custom_id]:-}" ]]; then
        continue
      fi
      CANDIDATE_MODS["$custom_id"]="$custom_path"
    fi
  done
fi

if $DRY_RUN; then
  log_info "[DRY-RUN] Enabled. No changes will be applied to $RIMWORLD_MODS_DIR"
fi

if [[ "$ACTION" == "link" ]]; then
  log_info "Target RimWorld Mods directory: $RIMWORLD_MODS_DIR"

  if [[ ! -d "$RIMWORLD_MODS_DIR" ]]; then
    if $DRY_RUN; then
      log_info "[DRY-RUN] Would create directory: $RIMWORLD_MODS_DIR"
    else
      log_info "Creating RimWorld Mods directory: $RIMWORLD_MODS_DIR"
      mkdir -p "$RIMWORLD_MODS_DIR"
    fi
  fi

  linked_count=0
  skipped_count=0

  for mod_id in "${!CANDIDATE_MODS[@]}"; do
    if [[ -n "$TARGET_MOD" && "$mod_id" != "$TARGET_MOD" ]]; then
      continue
    fi

    source_path="${CANDIDATE_MODS[$mod_id]}"
    dest_path="$RIMWORLD_MODS_DIR/$mod_id"

    if is_protected "$mod_id"; then
      log_err "Skipping protected mod identifier: $mod_id"
      skipped_count=$((skipped_count + 1))
      continue
    fi

    # Check existing destination
    if [[ -e "$dest_path" || -L "$dest_path" ]]; then
      if [[ -L "$dest_path" ]]; then
        current_target="$(readlink -f "$dest_path" || true)"
        expected_target="$(readlink -f "$source_path" || true)"
        if [[ "$current_target" == "$expected_target" ]]; then
          log_info "Symlink already up-to-date: $dest_path -> $source_path"
          continue
        else
          log_warn "Symlink exists pointing to $current_target; updating to $source_path"
        fi
      else
        log_err "Destination exists and is NOT a symlink: $dest_path. Skipping to prevent data loss."
        skipped_count=$((skipped_count + 1))
        continue
      fi
    fi

    if $DRY_RUN; then
      echo "  [DRY-RUN] Would symlink: $dest_path -> $source_path"
      linked_count=$((linked_count + 1))
    else
      ln -sfn "$source_path" "$dest_path"
      log_succ "Linked: $mod_id ($dest_path -> $source_path)"
      linked_count=$((linked_count + 1))
    fi
  done

  # Remove symlinks for disabled mods if present
  for dis_id in "${!DISABLED_MODS[@]}"; do
    dis_link="$RIMWORLD_MODS_DIR/$dis_id"
    if [[ -L "$dis_link" ]]; then
      if $DRY_RUN; then
        echo "  [DRY-RUN] Would remove symlink for disabled mod: $dis_link"
      else
        rm "$dis_link"
        log_info "Removed symlink for disabled mod: $dis_id"
      fi
    fi
  done

  log_succ "Link completed: $linked_count mod(s) processed, $skipped_count skipped."
 
  if [[ "$SKIP_CONFIG" != "true" ]]; then
    log_info "Synchronizing active load order and ModsConfig.xml..."
    SYNC_ARGS=("--write-config")
    if $DRY_RUN; then
      SYNC_ARGS+=("--dry-run")
    fi
    "$SCRIPT_DIR/order-mods.sh" "${SYNC_ARGS[@]}"
  fi

elif [[ "$ACTION" == "unlink" ]]; then
  log_info "Checking RimWorld Mods directory for links to remove: $RIMWORLD_MODS_DIR"

  if [[ ! -d "$RIMWORLD_MODS_DIR" ]]; then
    log_warn "RimWorld Mods directory does not exist: $RIMWORLD_MODS_DIR"
    exit 0
  fi

  unlinked_count=0

  # Scan symlinks in RIMWORLD_MODS_DIR
  for item in "$RIMWORLD_MODS_DIR"/*; do
    [[ -L "$item" ]] || continue
    item_name="$(basename "$item")"

    if is_protected "$item_name"; then
      log_warn "Encountered protected item as symlink, ignoring: $item_name"
      continue
    fi

    if [[ -n "$TARGET_MOD" && "$item_name" != "$TARGET_MOD" ]]; then
      continue
    fi

    link_target="$(readlink -f "$item" || true)"
    vendor_canon="$(readlink -f "$VENDOR_DIR" || true)"
    custom_canon="$(readlink -f "$CUSTOM_DIR" || true)"

    # Only remove symlinks pointing into this repository's vendor or custom mod directories
    if [[ "$link_target" == "$vendor_canon"* || "$link_target" == "$custom_canon"* ]]; then
      if $DRY_RUN; then
        echo "  [DRY-RUN] Would remove symlink: $item -> $link_target"
        unlinked_count=$((unlinked_count + 1))
      else
        rm "$item"
        log_succ "Removed symlink: $item"
        unlinked_count=$((unlinked_count + 1))
      fi
    fi
  done

  log_succ "Unlink completed: $unlinked_count symlink(s) removed."
fi
