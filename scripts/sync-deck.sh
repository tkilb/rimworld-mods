#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=scripts/common.sh
source "$SCRIPT_DIR/common.sh"

DRY_RUN=false
TRIGGER_REMOTE_LINK=true
TRIGGER_REMOTE_CONFIG=false
CHECK_CONN_ONLY=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --link|--remote-link)
      TRIGGER_REMOTE_LINK=true
      shift
      ;;
    --no-link|--skip-link|--files-only)
      TRIGGER_REMOTE_LINK=false
      shift
      ;;
    --config|--remote-config)
      TRIGGER_REMOTE_CONFIG=true
      TRIGGER_REMOTE_LINK=false
      shift
      ;;
    --check)
      CHECK_CONN_ONLY=true
      shift
      ;;
    --help|-h)
      echo "Usage: $0 [--dry-run] [--no-link] [--config] [--check]"
      echo ""
      echo "Options:"
      echo "  --dry-run    Preview rsync and remote link operations without transferring files"
      echo "  --no-link    Sync files only without updating remote symlinks or ModsConfig.xml"
      echo "  --config     Run 'order-mods.sh --write-config' only on Steam Deck without re-linking"
      echo "  --check      Test SSH connection to Steam Deck and exit"
      echo "  --help, -h   Show this help message"
      echo ""
      echo "Connection is resolved entirely via ~/.ssh/config (Host: \$DECK_HOST)."
      echo "Override DECK_HOST or DECK_REMOTE_DIR in config/local.env if needed."
      exit 0
      ;;
    *)
      log_err "Unknown argument: $1"
      exit 1
      ;;
  esac
done

DECK_HOST="${DECK_HOST:-steamdeck}"
DECK_REMOTE_DIR="${DECK_REMOTE_DIR:-~/.local/share/rimworld-mods}"

log_info "Steam Deck host: $DECK_HOST  (resolved via ~/.ssh/config)"
log_info "Remote directory: $DECK_REMOTE_DIR"

# Test SSH connectivity
log_info "Testing SSH connection..."
if ! ssh -o ConnectTimeout=5 -o BatchMode=yes "$DECK_HOST" "echo ok" >/dev/null 2>&1; then
  log_err "Cannot reach $DECK_HOST via SSH."
  log_err "Check that the Deck is online and sshd is running:"
  log_err "  ssh $DECK_HOST systemctl status sshd"
  exit 1
fi
log_succ "SSH connection OK."

if $CHECK_CONN_ONLY; then
  exit 0
fi

# Ensure load-order binary is built before syncing (SteamOS doesn't have Go installed)
if [[ ! -f "$REPO_ROOT/bin/load-order" ]] && command -v go >/dev/null 2>&1; then
  log_info "Compiling load-order binary before sync..."
  (cd "$REPO_ROOT/tools/load-order" && go build -o ../../bin/load-order .)
fi

# Ensure remote base directory exists
if $DRY_RUN; then
  log_info "[DRY-RUN] Would ensure remote directory: $DECK_REMOTE_DIR"
else
  ssh "$DECK_HOST" "mkdir -p $DECK_REMOTE_DIR"
fi

# Rsync — exclude local-only and non-essential files
RSYNC_EXCLUDES=(
  "--exclude=.git/"
  "--exclude=.git*"
  "--exclude=config/local.env"
  "--exclude=.cache/"
  "--exclude=tmp/"
  "--exclude=*.swp"
  "--exclude=*~"
  "--exclude=.DS_Store"
)

RSYNC_CMD=(
  rsync
  -avz
  --delete
  -e ssh
  "${RSYNC_EXCLUDES[@]}"
)

if $DRY_RUN; then
  RSYNC_CMD+=(--dry-run)
  log_info "[DRY-RUN] Previewing rsync to ${DECK_HOST}:${DECK_REMOTE_DIR}..."
else
  log_info "Syncing to ${DECK_HOST}:${DECK_REMOTE_DIR}..."
fi

"${RSYNC_CMD[@]}" "$REPO_ROOT/" "${DECK_HOST}:${DECK_REMOTE_DIR}/"

# Ensure config/local.env exists on remote Deck
if ! ssh "$DECK_HOST" "test -f $DECK_REMOTE_DIR/config/local.env" 2>/dev/null; then
  if $DRY_RUN; then
    log_info "[DRY-RUN] Would initialize remote config/local.env from config/env.steamdeck.env"
  else
    log_info "Initializing remote config/local.env on $DECK_HOST..."
    ssh "$DECK_HOST" "cp $DECK_REMOTE_DIR/config/env.steamdeck.env $DECK_REMOTE_DIR/config/local.env"
  fi
fi

if $DRY_RUN; then
  log_succ "[DRY-RUN] Preview complete. No files were transferred."
else
  log_succ "Sync complete."
fi

# Trigger remote link and config generation (unless --no-link was passed)
if $TRIGGER_REMOTE_LINK; then
  if $DRY_RUN; then
    log_info "[DRY-RUN] Would deploy mod symlinks and update ModsConfig.xml on $DECK_HOST"
    ssh "$DECK_HOST" "cd $DECK_REMOTE_DIR && bash ./scripts/link-mods.sh --link --dry-run"
  else
    log_info "Deploying mod symlinks and updating ModsConfig.xml on $DECK_HOST..."
    ssh "$DECK_HOST" "cd $DECK_REMOTE_DIR && bash ./scripts/link-mods.sh --link"
    log_succ "Remote link and ModsConfig.xml update complete."
  fi
fi

# Optional: trigger remote sync-config (use bash directly)
if $TRIGGER_REMOTE_CONFIG; then
  if $DRY_RUN; then
    log_info "[DRY-RUN] Would run 'order-mods.sh --write-config --dry-run' on $DECK_HOST in $DECK_REMOTE_DIR"
    ssh "$DECK_HOST" "cd $DECK_REMOTE_DIR && bash ./scripts/order-mods.sh --write-config --dry-run"
  else
    log_info "Generating ModsConfig.xml on $DECK_HOST..."
    ssh "$DECK_HOST" "cd $DECK_REMOTE_DIR && bash ./scripts/order-mods.sh --write-config"
    log_succ "Remote sync-config complete."
  fi
fi
