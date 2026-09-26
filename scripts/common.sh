#!/usr/bin/env bash
set -euo pipefail

# Source ~/.profile if MACHINE is not already set
if [[ -z "${MACHINE:-}" && -f "$HOME/.profile" ]]; then
  # shellcheck source=/dev/null
  source "$HOME/.profile"
fi

MACHINE="${MACHINE:-unknown}"

# Logging helpers
log_info()  { echo -e "\033[34m[INFO]\033[0m $*"; }
log_warn()  { echo -e "\033[33m[WARN]\033[0m $*"; }
log_err()   { echo -e "\033[31m[ERROR]\033[0m $*" >&2; }
log_succ()  { echo -e "\033[32m[OK]\033[0m $*"; }

# Global constants
export RIMWORLD_APP_ID="294100"
export STEAMCMD_BIN="${STEAMCMD_BIN:-steamcmd}"

# Machine-specific paths
case "$MACHINE" in
  "linux-box")
    export RIMWORLD_MODS_DIR="${RIMWORLD_MODS_DIR:-/mnt/gaming/SteamLibrary/steamapps/common/RimWorld/Mods}"
    export DECK_HOST="${DECK_HOST:-steamdeck}"
    export DECK_REMOTE_DIR="${DECK_REMOTE_DIR:-~/.local/share/rimworld-mods}"
    ;;
  "steam-deck")
    export RIMWORLD_MODS_DIR="${RIMWORLD_MODS_DIR:-$HOME/.local/share/Steam/steamapps/common/RimWorld/Mods}"
    ;;
  *)
    # Default fallback
    export RIMWORLD_MODS_DIR="${RIMWORLD_MODS_DIR:-$HOME/.local/share/Steam/steamapps/common/RimWorld/Mods}"
    ;;
esac

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export REPO_ROOT

# Require config/local.env to be present on all machines
if [[ ! -f "$REPO_ROOT/config/local.env" ]]; then
  echo "" >&2
  log_err "======================================================================"
  log_err "MISSING REQUIRED CONFIGURATION: config/local.env"
  log_err "----------------------------------------------------------------------"
  log_err "This monorepo requires a machine-specific 'config/local.env' file."
  log_err ""
  log_err "To fix this, create it from the provided template:"
  log_err "  cp config/config.env.example config/local.env"
  log_err ""
  log_err "Then review and adjust paths/settings for this machine."
  log_err "======================================================================"
  echo "" >&2
  exit 1
fi

# shellcheck source=/dev/null
source "$REPO_ROOT/config/local.env"

# Dependency checker helper
check_cmd() {
  local cmd="$1"
  local pkg_hint="${2:-$1}"
  if command -v "$cmd" >/dev/null 2>&1; then
    log_succ "$cmd is installed"
    return 0
  else
    log_err "$cmd is missing (install with: sudo pacman -S $pkg_hint)"
    return 1
  fi
}
