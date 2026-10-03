#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# If invoked with -h or --help or no args, show the user guide
if [[ $# -eq 0 ]] || [[ "${1:-}" == "-h" ]] || [[ "${1:-}" == "--help" ]]; then
  exec bash "$SCRIPT_DIR/scripts/help.sh" "$@"
fi

# If GNU make is installed, forward directly
if command -v make >/dev/null 2>&1; then
  exec make "$@"
fi

# Fallback dispatch when GNU make is not installed (e.g. SteamOS / Steam Deck)
case "${1:-}" in
  link)               exec bash "$SCRIPT_DIR/scripts/link-mods.sh" --link "${@:2}" ;;
  link-dry-run)       exec bash "$SCRIPT_DIR/scripts/link-mods.sh" --link --dry-run "${@:2}" ;;
  unlink)             exec bash "$SCRIPT_DIR/scripts/link-mods.sh" --unlink "${@:2}" ;;
  unlink-dry-run)     exec bash "$SCRIPT_DIR/scripts/link-mods.sh" --unlink --dry-run "${@:2}" ;;
  status)             exec bash "$SCRIPT_DIR/scripts/status.sh" "${@:2}" ;;
  tidy)               exec bash "$SCRIPT_DIR/scripts/tidy.sh" "${@:2}" ;;
  tidy-dry-run)       exec bash "$SCRIPT_DIR/scripts/tidy.sh" --dry-run "${@:2}" ;;
  order-mods)         exec bash "$SCRIPT_DIR/scripts/order-mods.sh" "${@:2}" ;;
  order-mods-dry-run) exec bash "$SCRIPT_DIR/scripts/order-mods.sh" --dry-run "${@:2}" ;;
  sync-config)        exec bash "$SCRIPT_DIR/scripts/order-mods.sh" --write-config "${@:2}" ;;
  sync-config-dry-run) exec bash "$SCRIPT_DIR/scripts/order-mods.sh" --write-config --dry-run "${@:2}" ;;
  check-deps)         exec bash "$SCRIPT_DIR/scripts/check-deps.sh" "${@:2}" ;;
  fetch-mods)         exec bash "$SCRIPT_DIR/scripts/fetch-mods.sh" "${@:2}" ;;
  update-mods)        exec bash "$SCRIPT_DIR/scripts/update-mods.sh" "${@:2}" ;;
  update-mods-dry-run) exec bash "$SCRIPT_DIR/scripts/update-mods.sh" --dry-run "${@:2}" ;;
  *)
    echo "Error: GNU make is not installed on this system, and target '$1' has no direct fallback." >&2
    exit 1
    ;;
esac
