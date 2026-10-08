# Makefile & Automation Task Runner Guide

This document details how the monorepo's task runner (`Makefile` / `./make`) coordinates mod acquisition, topological load order generation, symlinking, cleanup, and remote synchronization.

---

## 1. Mental Model & Architecture

This repository replaces the Steam Workshop client with deterministic, CLI-driven configuration:

```text
[ manifests/mods.yaml ]          <-- Single source of truth for declared mods
          │
          ▼
   ( make fetch-mods )           <-- Downloads via steamcmd/github into mods/vendor/
          │
          ▼
[ manifests/mods.lock.yaml ]     <-- Pinned versions/update timestamps
          │
          ▼
   ( make order-mods )           <-- Topologically sorts dependencies via tools/load-order
          │
          ▼
   ( make link )                 <-- Idempotently symlinks mods into RimWorld/Mods/
          │
          ▼
   ( make sync-config )          <-- Writes resolved ModsConfig.xml into RimWorld config
          │
          ▼
   ( make tidy )                 <-- Prunes undeclared locks, vendor caches, dangling links
```

---

## 2. Invocation & Help

To view the CLI guide in your terminal at any time:
```bash
make help
# or
./make -h
# or
bash ./scripts/help.sh -h
```

---

## 3. Command Reference

### General & Diagnostics
- `make check-deps`: Verifies required system tools (`steamcmd`, `jq`, `yq`, `rsync`, `curl`) and RimWorld paths.
- `make status`: Generates a formatted report showing declared mods, download status, locked versions, and symlink targets.

### Mod Acquisition & Updates
- `make fetch-mods`: Downloads all enabled mods defined in `manifests/mods.yaml` to `mods/vendor/`.
- `make update-mods`: Queries Steam Workshop and GitHub APIs for newer versions, updates `manifests/mods.lock.yaml`, and fetches updated files.
- `make update-mods-dry-run`: Previews available upstream updates without modifying local files.

### Symlink Deployment & Game Configuration
- `make link [MOD=<name>]`: Creates idempotent symlinks in RimWorld's `Mods/` directory pointing to `mods/vendor/<name>` and `mods/custom/<name>`. Also regenerates `ModsConfig.xml`.
- `make link-dry-run`: Previews symlink operations.
- `make unlink [MOD=<name>]`: Safely removes symlinks targeting monorepo mods from RimWorld's `Mods/` directory without touching official core directories (`Core`, `Royalty`, `Ideology`, `Biotech`, `Anomaly`, `Odyssey`).
- `make unlink-dry-run`: Previews symlink removals.
- `make rollback MOD=<name> VERSION=<ver>`: Reverts a mod to an earlier cached or pinned version.
- `make rollback-dry-run MOD=<name> VERSION=<ver>`: Previews rollback changes.

### Mod Load Order Engine
- `make build-load-order`: Compiles the standalone Go topological sort engine in `tools/load-order/` to `bin/load-order`.
- `make order-mods`: Computes dependency-ordered load list (`Harmony` -> `Core` -> `DLCs` -> `Standard Mods` -> `Trailing Mods`) using Tarjan's/Kahn's DAG resolution.
- `make order-mods-dry-run`: Runs cycle detection and prints calculated order.
- `make sync-config`: Writes the active load order directly to RimWorld's native `ModsConfig.xml` (backing up previous version).
- `make sync-config-dry-run`: Displays generated XML without writing to disk.

### Repository Maintenance (`make tidy`)
Inspired by Go's `go mod tidy`:
- `make tidy`:
  1. Prunes orphaned mod entries from `manifests/mods.lock.yaml` that are no longer declared in `manifests/mods.yaml`.
  2. Prunes unused cached download directories from `mods/vendor/`.
  3. Prunes broken or orphaned symlinks from RimWorld's active `Mods/` directory.
  4. Runs `go mod tidy` in `tools/load-order/`.
- `make tidy-dry-run`: Previews all cleanup actions without deleting files. Also triggered via `make -n tidy`.

### Remote Synchronization (Steam Deck)
- `make sync-deck-check`: Verifies SSH reachability to the Steam Deck.
- `make sync-deck`: Synchronizes repo manifests and cached vendor mods over rsync.
- `make sync-deck-dry-run`: Previews files to transfer.
- `make sync-deck-link`: Syncs files and triggers `make link` remotely over SSH on the Steam Deck.
- `make sync-deck-config`: Syncs files and deploys `ModsConfig.xml` remotely on the Steam Deck.

### Custom Mod Development
- `make scaffold-mod MOD=<name> [TYPE=xml|csharp]`: Generates a new mod workspace under `mods/custom/<name>`.
- `make build-mod MOD=<name>`: Compiles C# assemblies targeting `.NET Framework 4.7.2` using Mono / `dotnet build`.
