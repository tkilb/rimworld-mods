#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=scripts/common.sh
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

BIN_PATH="$REPO_ROOT/bin/load-order"
DRY_RUN=false
EXTRA_ARGS=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --help|-h)
      echo "Usage: $0 [--dry-run]"
      echo ""
      echo "Options:"
      echo "  --dry-run   Preview the computed mod load order without modifying any files"
      echo "  --help, -h  Display this help message"
      exit 0
      ;;
    *)
      EXTRA_ARGS+=("$1")
      shift
      ;;
  esac
done

# Build binary if not already present
if [[ ! -x "$BIN_PATH" ]]; then
  log_info "Building load-order Go binary..."
  mkdir -p "$REPO_ROOT/bin"
  go -C "$REPO_ROOT/tools/load-order" build -o "$BIN_PATH" .
fi

RUN_ARGS=(
  "-manifest" "$REPO_ROOT/manifests/mods.yaml"
  "-vendor-dir" "$REPO_ROOT/mods/vendor"
  "-custom-dir" "$REPO_ROOT/mods/custom"
)

if [[ -n "${RIMWORLD_MODS_DIR:-}" ]]; then
  # RimWorld install directory is the parent of the Mods directory
  RIMWORLD_DIR="$(dirname "$RIMWORLD_MODS_DIR")"
  if [[ -d "$RIMWORLD_DIR" ]]; then
    RUN_ARGS+=("-rimworld-dir" "$RIMWORLD_DIR")
  fi
fi

if $DRY_RUN; then
  RUN_ARGS+=("-dry-run")
fi

if [[ ${#EXTRA_ARGS[@]} -gt 0 ]]; then
  RUN_ARGS+=("${EXTRA_ARGS[@]}")
fi

exec "$BIN_PATH" "${RUN_ARGS[@]}"
