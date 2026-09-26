#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=scripts/common.sh
source "$SCRIPT_DIR/common.sh"

log_info "Checking dependencies for machine: $MACHINE"
log_info "Target RimWorld Mods directory: $RIMWORLD_MODS_DIR"

missing=0

check_cmd "steamcmd" "steamcmd (AUR/multilib)" || missing=$((missing + 1))
check_cmd "jq" "jq" || missing=$((missing + 1))
check_cmd "yq" "yq" || missing=$((missing + 1))
check_cmd "rsync" "rsync" || missing=$((missing + 1))
check_cmd "curl" "curl" || missing=$((missing + 1))

echo ""
if [[ -d "$RIMWORLD_MODS_DIR" ]]; then
  log_succ "Found RimWorld Mods directory at: $RIMWORLD_MODS_DIR"
else
  log_warn "RimWorld Mods directory not found at: $RIMWORLD_MODS_DIR"
  log_warn "If RimWorld is not installed on this machine or stored in a custom path, override with RIMWORLD_MODS_DIR"
fi

if [[ $missing -eq 0 ]]; then
  log_succ "All dependencies are satisfied!"
  exit 0
else
  log_err "$missing required tool(s) missing."
  exit 1
fi
