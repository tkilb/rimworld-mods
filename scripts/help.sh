#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=scripts/common.sh
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

show_full_guide() {
  cat << 'EOF'
================================================================================
                    RimWorld Monorepo - Task Runner Guide
================================================================================

OVERVIEW & MENTAL MODEL
-----------------------
This monorepo manages your RimWorld mods outside of Steam Workshop, keeping
your setup deterministic, version-controlled, and synchronized across machines
(Arch Linux desktop, laptop, and Steam Deck).

The lifecycle flows through five main steps:
  1. Declare      Edit `manifests/mods.yaml` with mods you want.
  2. Fetch        `make fetch-mods` downloads them to `mods/vendor/`.
  3. Sort & Order `make order-mods` topologically resolves dependencies (Harmony ->
                  Core -> DLCs -> Mods) using `tools/load-order`.
  4. Link         `make link` idempotently symlinks active mods into RimWorld's
                  `Mods/` directory without altering game core files.
  5. Sync         `make sync-config` generates and writes `ModsConfig.xml`.
                  `make sync-deck` sends your mods & config to your Steam Deck.

USAGE
-----
  make <target> [VARIABLE=value]
  ./make -h                    # Show this guide

TARGET REFERENCE
----------------

[ Verification & Diagnostics ]
  make check-deps              Verify host tools (steamcmd, jq, yq, rsync, curl).
  make status                  Display table of declared mods, download status,
                               locked versions, and active symlinks.

[ Mod Management ]
  make fetch-mods              Download all enabled mods in manifests/mods.yaml.
  make update-mods             Check upstream (Steam/GitHub) for mod updates,
                               re-sync manifests/mods.lock.yaml, and download.
  make update-mods-dry-run     Preview upstream updates without altering files.
  make link [MOD=<name>]       Symlink vendor and custom mods into RimWorld's
                               Mods directory and update ModsConfig.xml.
  make link-dry-run            Preview symlinks that will be created.
  make unlink [MOD=<name>]     Remove monorepo symlinks from RimWorld's Mods dir.
  make unlink-dry-run          Preview symlinks that will be removed.
  make rollback MOD=<name> VERSION=<ver>
                               Roll back a mod to a specific version or tag.
  make rollback-dry-run        Preview rollback changes.

[ Load Order & Game Config ]
  make build-load-order        Compile Go topological load-order engine (bin/load-order).
  make order-mods              Compute and print valid topological load order.
  make order-mods-dry-run      Preview load order calculation and cycle detection.
  make sync-config             Generate and deploy ModsConfig.xml to RimWorld.
  make sync-config-dry-run     Preview ModsConfig.xml output without writing to disk.

[ Repository Tidying ]
  make tidy                    Clean up orphaned lockfile entries, stale vendor
                               caches, dangling symlinks, and run 'go mod tidy'.
  make tidy-dry-run            Preview what tidy would prune without deleting anything.

[ Remote Synchronization (Steam Deck) ]
  make import-deck             Cherry-pick Steam Workshop mods from Steam Deck via TUI.
  make import-deck-dry-run     Preview cherry-picked mods without modifying mods.yaml.
  make sync-deck-check         Test SSH connectivity to Steam Deck.
  make sync-deck               Sync mods, deploy symlinks, and update ModsConfig.xml on Deck.
  make sync-deck-dry-run       Preview sync and deployment actions on Steam Deck.

[ Private Mod Development ]
  make scaffold-mod MOD=<name> [TYPE=xml|csharp]
                               Scaffold new custom mod in mods/custom/<name>.
  make scaffold-mod-dry-run    Preview files to be generated.
  make build-mod MOD=<name>    Compile C# assemblies for custom mod (.NET 4.7.2).
  make build-mod-dry-run       Preview build execution.

COMMON WORKFLOWS
----------------
1. Adding a new workshop mod:
   - Add entry under `mods:` in `manifests/mods.yaml`.
   - Run: `make fetch-mods`
   - Run: `make link` (automatically updates ModsConfig.xml)
   - Verify: `make status`

2. Removing a mod:
   - Remove entry from `manifests/mods.yaml`.
   - Run: `make tidy-dry-run` to inspect what will be cleaned up.
   - Run: `make tidy` to clean the lockfile, vendor cache, and symlinks.
   - Run: `make sync-config` to refresh ModsConfig.xml.

3. Syncing to Steam Deck:
   - Check connection: `make sync-deck-check`
   - Dry run preview: `make sync-deck-dry-run`
   - Deploy: `make sync-deck`

================================================================================
EOF
}

# Parse CLI options
while [[ $# -gt 0 ]]; do
  case "$1" in
    -h|--help|help)
      show_full_guide
      exit 0
      ;;
    *)
      show_full_guide
      exit 0
      ;;
  esac
done

show_full_guide
