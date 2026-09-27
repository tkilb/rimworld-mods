#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=scripts/common.sh
source "$SCRIPT_DIR/common.sh"

MOD_NAME=""
CONFIG="Release"
DRY_RUN=false

show_help() {
  echo "Usage: $0 [OPTIONS] [MOD_NAME]"
  echo ""
  echo "Build C# assemblies for a custom RimWorld mod."
  echo ""
  echo "Options:"
  echo "  --mod, -m NAME       Name of mod in mods/custom/ (e.g. my-mod)"
  echo "  --configuration, -c  Build configuration (Debug or Release, default: Release)"
  echo "  --dry-run            Preview build command without compiling"
  echo "  --help, -h           Display this help message"
  exit 0
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --mod|-m)
      MOD_NAME="$2"
      shift 2
      ;;
    --configuration|-c)
      CONFIG="$2"
      shift 2
      ;;
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --help|-h)
      show_help
      ;;
    *)
      if [[ -z "$MOD_NAME" ]]; then
        MOD_NAME="$1"
        shift
      else
        log_err "Unknown argument: $1"
        exit 1
      fi
      ;;
  esac
done

if [[ -z "$MOD_NAME" ]]; then
  log_err "Mod name is required. Usage: $0 <MOD_NAME> or $0 --mod <MOD_NAME>"
  exit 1
fi

MOD_DIR="$REPO_ROOT/mods/custom/$MOD_NAME"
if [[ ! -d "$MOD_DIR" ]]; then
  log_err "Custom mod directory does not exist: $MOD_DIR"
  exit 1
fi

# Locate .csproj file in Source/ or root of mod
CSPROJ_FILE=""
if compgen -G "$MOD_DIR/Source/*.csproj" > /dev/null; then
  CSPROJ_FILE="$(ls "$MOD_DIR/Source"/*.csproj | head -n 1)"
elif compgen -G "$MOD_DIR/*.csproj" > /dev/null; then
  CSPROJ_FILE="$(ls "$MOD_DIR"/*.csproj | head -n 1)"
fi

if [[ -z "$CSPROJ_FILE" || ! -f "$CSPROJ_FILE" ]]; then
  log_err "No .csproj project file found in $MOD_DIR/Source/ or $MOD_DIR/."
  log_info "This mod appears to be XML-only or missing a C# project file."
  exit 1
fi

log_info "Target C# project: $CSPROJ_FILE"

# Check for dotnet SDK
if ! command -v dotnet >/dev/null 2>&1; then
  if $DRY_RUN; then
    log_warn "The 'dotnet' CLI tool is not installed (install with: sudo pacman -S dotnet-sdk)."
  else
    log_err "The 'dotnet' CLI tool is required to compile C# mods but was not found in PATH."
    echo ""
    echo "To install the .NET SDK on Arch Linux:"
    echo "  sudo pacman -S dotnet-sdk"
    echo ""
    exit 1
  fi
fi

# Resolve managed assemblies directory from RimWorld install
MANAGED_DIR=""
if [[ -n "${RIMWORLD_MODS_DIR:-}" ]]; then
  CANDIDATE_MANAGED="$(dirname "$RIMWORLD_MODS_DIR")/RimWorldLinux_Data/Managed"
  if [[ -d "$CANDIDATE_MANAGED" ]]; then
    MANAGED_DIR="$CANDIDATE_MANAGED"
  fi
fi

BUILD_ARGS=("build" "$CSPROJ_FILE" "-c" "$CONFIG")
if [[ -n "$MANAGED_DIR" ]]; then
  BUILD_ARGS+=("-p:RIMWORLD_MANAGED_DIR=$MANAGED_DIR")
  log_info "Using RimWorld Managed assembly dir: $MANAGED_DIR"
fi

if $DRY_RUN; then
  echo "  [DRY-RUN] Would execute: dotnet ${BUILD_ARGS[*]}"
  log_succ "[DRY-RUN] Preview complete."
  exit 0
fi

log_info "Compiling $MOD_NAME ($CONFIG)..."
dotnet "${BUILD_ARGS[@]}"
log_succ "Build succeeded for $MOD_NAME"
