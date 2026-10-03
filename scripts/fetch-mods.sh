#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=scripts/common.sh
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

DRY_RUN=false
TARGET_MOD=""

# Parse arguments
while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --help|-h)
      echo "Usage: $0 [MOD_NAME] [--dry-run]"
      echo ""
      echo "Options:"
      echo "  MOD_NAME    Optional specific mod identifier to fetch (e.g. harmony)"
      echo "  --dry-run   Preview fetch operations without downloading or modifying files"
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
VENDOR_DIR="$REPO_ROOT/mods/vendor"

if [[ ! -f "$MANIFEST_FILE" ]]; then
  log_err "Manifest file not found: $MANIFEST_FILE"
  exit 1
fi

mkdir -p "$VENDOR_DIR"

if $DRY_RUN; then
  log_info "[DRY-RUN] Enabled. No network requests or file modifications will be made."
fi

# Helper python script to parse mods manifest and output bash-evaluable lines
python3 - "$MANIFEST_FILE" "$TARGET_MOD" << 'EOF' | while IFS='|' read -r mod_id mod_name mod_source workshop_id gh_repo gh_tag gh_asset_pattern git_url git_branch enabled; do
import sys
import yaml

manifest_file = sys.argv[1]
target_mod = sys.argv[2] if len(sys.argv) > 2 else ""

with open(manifest_file, 'r') as f:
    data = yaml.safe_load(f) or {}

mods = data.get('mods', {})

for mod_id, mod in mods.items():
    if target_mod and mod_id != target_mod:
        continue
    
    enabled = str(mod.get('enabled', True)).lower()
    name = mod.get('name', mod_id)
    source = mod.get('source', '')
    workshop_id = str(mod.get('workshop_id', ''))
    repo = mod.get('repo', '')
    tag = mod.get('tag', '')
    asset_pattern = mod.get('asset_pattern', '')
    git_url = mod.get('url', '')
    git_branch = mod.get('branch', '')

    print(f"{mod_id}|{name}|{source}|{workshop_id}|{repo}|{tag}|{asset_pattern}|{git_url}|{git_branch}|{enabled}")
EOF
  if [[ "$enabled" != "true" ]]; then
    log_info "Skipping disabled mod: $mod_name ($mod_id)"
    continue
  fi

  mod_dest="$VENDOR_DIR/$mod_id"

  log_info "Processing mod: $mod_name ($mod_id) [source: $mod_source]"

  case "$mod_source" in
    steam_workshop)
      if [[ -z "$workshop_id" ]]; then
        log_err "Mod $mod_id is missing workshop_id"
        continue
      fi

      log_info "Steam Workshop ID: $workshop_id -> Destination: $mod_dest"

      if $DRY_RUN; then
        echo "  [DRY-RUN] Would run: $STEAMCMD_BIN +login anonymous +workshop_download_item $RIMWORLD_APP_ID $workshop_id +quit"
        echo "  [DRY-RUN] Would copy downloaded files to: $mod_dest"
      else
        if ! command -v "$STEAMCMD_BIN" >/dev/null 2>&1; then
          log_err "Cannot fetch Steam Workshop item: '$STEAMCMD_BIN' not found."
          continue
        fi

        TEMP_STEAM_DIR=$(mktemp -d)
        log_info "Running steamcmd to download workshop item $workshop_id..."
        "$STEAMCMD_BIN" +force_install_dir "$TEMP_STEAM_DIR" +login anonymous +workshop_download_item "$RIMWORLD_APP_ID" "$workshop_id" validate +quit

        WORKSHOP_CONTENT_DIR="$TEMP_STEAM_DIR/steamapps/workshop/content/$RIMWORLD_APP_ID/$workshop_id"
        if [[ -d "$WORKSHOP_CONTENT_DIR" ]]; then
          mkdir -p "$mod_dest"
          rsync -a --delete "$WORKSHOP_CONTENT_DIR/" "$mod_dest/"
          log_succ "Successfully installed $mod_name into $mod_dest"
        else
          log_err "Failed to locate downloaded workshop files at $WORKSHOP_CONTENT_DIR"
        fi
        rm -rf "$TEMP_STEAM_DIR"
      fi
      ;;

    github_release)
      if [[ -z "$gh_repo" ]]; then
        log_err "Mod $mod_id is missing repo"
        continue
      fi

      log_info "GitHub Release: $gh_repo (tag: ${gh_tag:-latest}) -> Destination: $mod_dest"

      if $DRY_RUN; then
        echo "  [DRY-RUN] Would fetch GitHub release from $gh_repo (tag: ${gh_tag:-latest})"
        echo "  [DRY-RUN] Would extract release archive to: $mod_dest"
      else
        TEMP_DL_DIR=$(mktemp -d)
        ZIP_FILE="$TEMP_DL_DIR/mod.zip"
        DOWNLOAD_URL=""

        if [[ -n "$gh_tag" && "$gh_tag" != "latest" ]]; then
          API_URL="https://api.github.com/repos/$gh_repo/releases/tags/$gh_tag"
        else
          API_URL="https://api.github.com/repos/$gh_repo/releases/latest"
        fi

        log_info "Querying GitHub release metadata from $API_URL..."
        RELEASE_JSON="$TEMP_DL_DIR/release.json"
        
        if curl -sSL -f -H "Accept: application/vnd.github.v3+json" "$API_URL" -o "$RELEASE_JSON" 2>/dev/null; then
          DOWNLOAD_URL=$(jq -r '.assets[]? | select(.name | endswith(".zip")) | .browser_download_url' "$RELEASE_JSON" | head -n 1)
          if [[ -z "$DOWNLOAD_URL" || "$DOWNLOAD_URL" == "null" ]]; then
            DOWNLOAD_URL=$(jq -r '.zipball_url // empty' "$RELEASE_JSON")
          fi
        fi

        CANDIDATES=()
        if [[ -n "$DOWNLOAD_URL" && "$DOWNLOAD_URL" != "null" ]]; then
          CANDIDATES+=("$DOWNLOAD_URL")
        fi
        if [[ -n "$gh_tag" && "$gh_tag" != "latest" ]]; then
          CANDIDATES+=("https://github.com/$gh_repo/archive/refs/tags/$gh_tag.zip")
          CANDIDATES+=("https://github.com/$gh_repo/archive/refs/tags/v$gh_tag.zip")
          CANDIDATES+=("https://github.com/$gh_repo/archive/refs/heads/$gh_tag.zip")
        fi
        CANDIDATES+=("https://github.com/$gh_repo/archive/refs/heads/master.zip")
        CANDIDATES+=("https://github.com/$gh_repo/archive/refs/heads/main.zip")

        DOWNLOADED=false
        for candidate_url in "${CANDIDATES[@]}"; do
          log_info "Attempting download from: $candidate_url"
          if curl -sSL -f "$candidate_url" -o "$ZIP_FILE" 2>/dev/null; then
            DOWNLOADED=true
            log_succ "Downloaded archive from $candidate_url"
            break
          fi
        done

        if ! $DOWNLOADED; then
          log_err "Failed to download release/archive for $gh_repo after trying all candidates"
          rm -rf "$TEMP_DL_DIR"
          continue
        fi

        EXTRACT_DIR="$TEMP_DL_DIR/extracted"
        mkdir -p "$EXTRACT_DIR" "$mod_dest"
        unzip -q -o "$ZIP_FILE" -d "$EXTRACT_DIR"

        # If extracted content has a single top-level directory containing About/ or Assemblies/ or Defs/, flatten it
        SUBDIRS=("$EXTRACT_DIR"/*)
        if [[ ${#SUBDIRS[@]} -eq 1 && -d "${SUBDIRS[0]}" ]]; then
          rsync -a --delete "${SUBDIRS[0]}/" "$mod_dest/"
        else
          rsync -a --delete "$EXTRACT_DIR/" "$mod_dest/"
        fi

        log_succ "Successfully extracted $mod_name into $mod_dest"
        rm -rf "$TEMP_DL_DIR"
      fi
      ;;

    git)
      if [[ -z "$git_url" ]]; then
        log_err "Mod $mod_id is missing git url"
        continue
      fi

      log_info "Git repo: $git_url (branch: ${git_branch:-default}) -> Destination: $mod_dest"

      if $DRY_RUN; then
        echo "  [DRY-RUN] Would clone $git_url into $mod_dest"
      else
        if [[ -d "$mod_dest/.git" ]]; then
          log_info "Updating existing git repo at $mod_dest..."
          git -C "$mod_dest" fetch --all
          if [[ -n "$git_branch" ]]; then
            git -C "$mod_dest" checkout "$git_branch"
            git -C "$mod_dest" pull origin "$git_branch"
          else
            git -C "$mod_dest" pull
          fi
        else
          mkdir -p "$mod_dest"
          BRANCH_ARG=()
          if [[ -n "$git_branch" ]]; then
            BRANCH_ARG=(-b "$git_branch")
          fi
          git clone --depth 1 "${BRANCH_ARG[@]}" "$git_url" "$mod_dest"
        fi
        log_succ "Successfully cloned/updated $mod_name in $mod_dest"
      fi
      ;;

    custom|local)
      log_info "Skipping local custom mod: $mod_id"
      continue
      ;;

    *)
      log_warn "Unknown mod source '$mod_source' for $mod_id"
      ;;
  esac
done

log_succ "Mod fetch processing complete."
