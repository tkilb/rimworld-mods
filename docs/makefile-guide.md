# RimWorld CLI & Task Runner Guide

This document details how the unified `rimmod` CLI coordinates mod acquisition, topological load order generation, symlinking, cleanup, and remote synchronization.

---

## 1. Mental Model & Architecture

This repository replaces the Steam Workshop client with deterministic, CLI-driven configuration:

```text
[ manifests/mods.yaml ]          <-- Single source of truth for declared mods
          │
          ▼
   ( rimmod apply )              <-- Downloads missing mods into mods/vendor/
          │
          ▼
[ manifests/mods.lock.yaml ]     <-- Pinned versions/update timestamps
          │
          ▼
   ( rimmod order )              <-- Topologically sorts dependencies via internal/order
          │
          ▼
   ( rimmod apply )              <-- Idempotently symlinks mods into RimWorld/Mods/
          │
          ▼
   ( rimmod apply )              <-- Writes resolved ModsConfig.xml into RimWorld config
          │
          ▼
   ( rimmod tidy )               <-- Prunes undeclared locks, vendor caches, dangling links
```

---

## 2. Invocation & Help

To view CLI help at any time:
```bash
rimmod --help
# or for a specific command
rimmod apply --help
```

---

## 3. Command Reference

### General & Diagnostics
- `rimmod status`: Generates a formatted report showing declared mods, download status, locked versions, active symlinks, and Deck status.

### Mod Acquisition, Deployment & Updates
- `rimmod apply`: Reconciles local RimWorld installation to declared state (fetches missing, links mods, sorts load order, updates `ModsConfig.xml`).
- `rimmod apply --dry-run`: Previews all reconciliation operations without modifying files.
- `rimmod update`: Queries Steam Workshop and GitHub APIs for newer versions, updates `manifests/mods.lock.yaml`, and re-applies changes.
- `rimmod update --mod <id>`: Updates a specific mod identifier.
- `rimmod update --dry-run`: Previews available upstream updates without modifying local files.

### Mod Load Order Engine
- `rimmod order`: Computes dependency-ordered load list (`Harmony` -> `Core` -> `DLCs` -> `Standard Mods` -> `Trailing Mods`) using topological DAG resolution and displays any missing rules or warnings.

### Repository Maintenance (`rimmod tidy`)
- `rimmod tidy`:
  1. Prunes orphaned mod entries from `manifests/mods.lock.yaml` that are no longer declared in `manifests/mods.yaml`.
  2. Prunes unused cached download directories from `mods/vendor/`.
  3. Prunes broken or orphaned symlinks from RimWorld's active `Mods/` directory.
  4. Runs `go mod tidy`.
- `rimmod tidy --dry-run`: Previews all cleanup actions without deleting files.

### Remote Synchronization (Steam Deck)
- `rimmod deck push --check`: Verifies SSH reachability to the Steam Deck.
- `rimmod deck push --dry-run`: Previews files to transfer via rsync and remote reconciliation.
- `rimmod deck push`: Synchronizes repo manifests and cached vendor mods over rsync, and runs `rimmod apply` remotely.
- `rimmod deck pull`: Launches interactive TUI to cherry-pick Steam Workshop subscriptions from the Steam Deck into `manifests/mods.yaml`.

### Custom Mod Development
- `rimmod dev scaffold <name> [--type xml|csharp]`: Generates a new mod workspace under `mods/custom/<name>`.
- `rimmod dev build <name>`: Compiles C# assemblies using `dotnet build` and re-applies to RimWorld.

---

## 4. Makefile Developer Targets

The root `Makefile` is reserved for compiling and installing the `rimmod` CLI binary:

```bash
make build              # Compile bin/rimmod
make test               # Run Go unit test suite
make install            # Install bin/rimmod into ~/bin/rimmod
make uninstall          # Remove installed binary
```
