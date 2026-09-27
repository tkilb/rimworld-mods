#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=scripts/common.sh
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

BIN_PATH="$REPO_ROOT/bin/load-order"
DRY_RUN=false
WRITE_CONFIG=false
OUTPUT_XML=""
EXTRA_ARGS=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --write-config|-w)
      WRITE_CONFIG=true
      shift
      ;;
    --output|-o)
      OUTPUT_XML="$2"
      WRITE_CONFIG=true
      shift 2
      ;;
    --help|-h)
      echo "Usage: $0 [--dry-run] [--write-config] [--output PATH]"
      echo ""
      echo "Options:"
      echo "  --dry-run          Preview the computed mod load order without modifying any files"
      echo "  --write-config, -w Generate and deploy ModsConfig.xml to RimWorld config directory"
      echo "  --output, -o PATH  Write ModsConfig.xml to a specific output path"
      echo "  --help, -h         Display this help message"
      exit 0
      ;;
    *)
      EXTRA_ARGS+=("$1")
      shift
      ;;
  esac
done

# Build binary if not present or if sources are newer
needs_build=false
if [[ ! -x "$BIN_PATH" ]]; then
  needs_build=true
else
  for src in "$REPO_ROOT/tools/load-order"/*.go; do
    if [[ "$src" -nt "$BIN_PATH" ]]; then
      needs_build=true
      break
    fi
  done
fi

if $needs_build; then
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

if $WRITE_CONFIG; then
  if [[ -z "$OUTPUT_XML" ]]; then
    OUTPUT_XML="${RIMWORLD_CONFIG_FILE:-}"
    if [[ -z "$OUTPUT_XML" && -n "${RIMWORLD_CONFIG_DIR:-}" ]]; then
      OUTPUT_XML="$RIMWORLD_CONFIG_DIR/ModsConfig.xml"
    fi
  fi
  if [[ -n "$OUTPUT_XML" ]]; then
    RUN_ARGS+=("-output-xml" "$OUTPUT_XML")
  else
    log_err "No destination config path specified and RIMWORLD_CONFIG_FILE is unset."
    exit 1
  fi
fi

if $DRY_RUN; then
  RUN_ARGS+=("-dry-run")
fi

if [[ ${#EXTRA_ARGS[@]} -gt 0 ]]; then
  RUN_ARGS+=("${EXTRA_ARGS[@]}")
fi

exec "$BIN_PATH" "${RUN_ARGS[@]}"
