#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=scripts/common.sh
source "$SCRIPT_DIR/common.sh"

DRY_RUN=false
SHOW_ALL=false
COPY_FILES=false
LOCAL_DIR=""
DECK_HOST_OVERRIDE=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --show-all|--all)
      SHOW_ALL=true
      shift
      ;;
    --copy-files|--copy)
      COPY_FILES=true
      shift
      ;;
    --host|--deck-host)
      DECK_HOST_OVERRIDE="$2"
      shift 2
      ;;
    --local-dir)
      LOCAL_DIR="$2"
      shift 2
      ;;
    --help|-h)
      echo "Usage: $0 [OPTIONS]"
      echo ""
      echo "Cherry-pick Steam Workshop mods from your Steam Deck into manifests/mods.yaml via interactive TUI."
      echo ""
      echo "Options:"
      echo "  --dry-run      Preview manifest changes without modifying manifests/mods.yaml"
      echo "  --show-all     Show all mods in the picker, including transitive dependencies"
      echo "  --copy-files   Rsync mod files directly from Steam Deck into mods/vendor/ (bypasses SteamCMD)"
      echo "  --host <host>  Override Steam Deck SSH host (default: \$DECK_HOST or 'steamdeck')"
      echo "  --local-dir <dir> Scan a local workshop directory instead of remote Steam Deck"
      echo "  --help, -h     Show this help message"
      exit 0
      ;;
    *)
      log_err "Unknown argument: $1"
      exit 1
      ;;
  esac
done

# Verify required host dependencies
check_cmd fzf "fzf"
check_cmd python3 "python"
if [[ -z "$LOCAL_DIR" ]]; then
  check_cmd ssh "openssh"
fi

TARGET_HOST="${DECK_HOST_OVERRIDE:-${DECK_HOST:-steamdeck}}"

PYTHON_ARGS=(
  python3
  "$SCRIPT_DIR/import-deck.py"
  "--repo-root" "$REPO_ROOT"
  "--deck-host" "$TARGET_HOST"
)

if $DRY_RUN; then
  PYTHON_ARGS+=(--dry-run)
fi

if $SHOW_ALL; then
  PYTHON_ARGS+=(--show-all)
fi

if $COPY_FILES; then
  PYTHON_ARGS+=(--copy-files)
fi

if [[ -n "$LOCAL_DIR" ]]; then
  PYTHON_ARGS+=(--local-dir "$LOCAL_DIR")
fi

"${PYTHON_ARGS[@]}"
