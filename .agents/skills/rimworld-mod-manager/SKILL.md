---
name: rimworld-mod-manager
description: >-
  Manage, cherry-pick, order, and synchronize RimWorld mods across Arch Linux workstations and Steam Deck.
  Use when importing Steam Workshop mods from Steam Deck, resolving dependency graphs and load orders,
  generating ModsConfig.xml, or deploying symlinks.
---

# RimWorld Mod Manager Skill

## Overview

This repository acts as a centralized monorepo for managing, versioning, authoring, and deploying custom RimWorld setups across Linux machines (desktop, laptop, and Steam Deck) without Steam client lock-in, powered by the unified `rimmod` CLI.

## Key Capabilities & Procedures

### 1. Inspecting Monorepo Status
```bash
rimmod status
```
Displays machine target, RimWorld installation and config paths, declared community mods, official DLC expansions, custom mods, active symlinks, and cache health.

### 2. Cherry-Picking Workshop Mods from Steam Deck
To bring Steam Workshop mods installed on the Steam Deck into this repository:
```bash
# Preview cherry-picking via interactive TUI
rimmod deck pull --dry-run

# Run interactive TUI to cherry-pick mods
rimmod deck pull

# Copy workshop files directly over SSH (skipping SteamCMD re-download)
rimmod deck pull --copy
```
- **TUI Features:** Powered by `fzf -m` (toggle selection with `Space`, confirm with `Enter`) with a rich side-preview pane detailing the mod title, package ID, workshop ID, Steam URL, dependencies, and description.
- **Transitive Dependency Suppression:** Libraries and framework mods depended upon by other installed mods (e.g. *Harmony*, *ilyvion's Laboratory*) are hidden from the primary selection list to reduce clutter.
- **Auto-Inclusion:** Selecting a top-level mod automatically resolves its dependency DAG and includes required transitive dependencies into `manifests/mods.yaml`.
- **Pass `--all`:** Use `rimmod deck pull --all` if you need to view and select individual leaf libraries directly.

### 3. Mod Lifecycle & Local Reconciliation
Once mods are declared in `manifests/mods.yaml`:
```bash
# Full local reconciliation (offline if cache exists):
# 1. Fetches missing mods
# 2. Reconciles symlinks in RimWorld Mods/ directory
# 3. Topologically sorts load order
# 4. Deploys generated ModsConfig.xml
rimmod apply

# Preview reconciliation without modifying files:
rimmod apply --dry-run

# Check upstream sources for updates, sync lockfile, and re-apply:
rimmod update
rimmod update --mod <mod_id>

# Prune unreferenced vendor downloads, lockfile entries, and dead symlinks:
rimmod tidy
```

### 4. Deploy to Steam Deck
```bash
# Test SSH connectivity:
rimmod deck push --check

# Preview remote rsync and reconciliation:
rimmod deck push --dry-run

# Sync repository manifests, vendor cache, and custom mods, then reconcile remotely:
rimmod deck push
```

### 5. Private Mod Development
```bash
# Compile custom C# mod and auto-apply:
rimmod dev build <mod_name>

# Scaffold a new custom mod template (XML or C#):
rimmod dev scaffold <mod_name> --type csharp
```
