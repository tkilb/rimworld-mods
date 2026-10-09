---
name: rimworld-mod-manager
description: >-
  Manage, cherry-pick, order, and synchronize RimWorld mods across Arch Linux workstations and Steam Deck.
  Use when importing Steam Workshop mods from Steam Deck, resolving dependency graphs and load orders,
  generating ModsConfig.xml, or deploying symlinks.
---

# RimWorld Mod Manager Skill

## Overview

This repository acts as a centralized monorepo for managing, versioning, authoring, and deploying custom RimWorld setups across Linux machines (desktop, laptop, and Steam Deck) without Steam client lock-in.

## Key Capabilities & Procedures

### 1. Cherry-Picking Workshop Mods from Steam Deck
To bring Steam Workshop mods installed on the Steam Deck into this repository:
```bash
# Preview cherry-picking via interactive TUI
make import-deck-dry-run

# Run interactive TUI to cherry-pick mods
make import-deck

# Copy workshop files directly over SSH (skipping SteamCMD re-download)
make import-deck COPY=1
```
- **TUI Features:** Powered by `fzf -m` (toggle selection with `Space`, confirm with `Enter`) with a rich side-preview pane detailing the mod title, package ID, workshop ID, Steam URL, dependencies, and description.
- **Transitive Dependency Suppression:** Libraries and framework mods depended upon by other installed mods (e.g. *Harmony*, *ilyvion's Laboratory*) are hidden from the primary selection list to reduce clutter.
- **Auto-Inclusion:** Selecting a top-level mod automatically resolves its dependency DAG and includes required transitive dependencies into `manifests/mods.yaml`.
- **Pass `--show-all`:** Use `./scripts/import-deck.sh --show-all` if you need to view and select individual leaf libraries directly.

### 2. Mod Lifecycle & Synchronization Workflow
Once mods are declared in `manifests/mods.yaml`:
1. **Fetch & Cache:**
   ```bash
   make fetch-mods
   ```
   Downloads mods via SteamCMD / GitHub releases into `mods/vendor/` and pins commits/versions in `manifests/mods.lock.yaml`.
2. **Order Load Order:**
   ```bash
   make order-mods-dry-run
   make order-mods
   ```
   Uses the Go topological sorting engine (`bin/load-order`) to parse `About.xml` rules (`<loadAfter>`, `<loadBefore>`, etc.) and generate a valid load order.
3. **Deploy Symlinks & Config:**
   ```bash
   make link
   make sync-config
   ```
   Idempotently symlinks mods from `mods/vendor/` and `mods/custom/` into RimWorld's `Mods/` directory and writes `ModsConfig.xml`.
4. **Deploy to Steam Deck:**
   ```bash
   make sync-deck-check
   make sync-deck-dry-run
   make sync-deck
   ```
   Synchronizes the repository manifests, vendor cache, and load orders to the remote Steam Deck over SSH/rsync.
