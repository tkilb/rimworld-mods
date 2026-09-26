# Mod Management Guide

This document describes how to declare, fetch, update, deploy, and roll back RimWorld mods in this monorepo.

---

## 1. Declaring Mods (`manifests/mods.yaml`)

Mods are declared centrally in `manifests/mods.yaml`. Supported sources include:

### Steam Workshop
```yaml
mods:
  harmony:
    name: "Harmony"
    source: steam_workshop
    workshop_id: "2009463077"
    enabled: true
```

### GitHub Releases
```yaml
mods:
  hugslib:
    name: "HugsLib"
    source: github_release
    repo: "UnlimitedHugs/RimworldHugsLib"
    tag: "v11.0.0" # or "master" / "latest"
    enabled: true
```

### Git Repository
```yaml
mods:
  example_mod:
    name: "Example Git Mod"
    source: git
    url: "https://github.com/example/rimworld-mod.git"
    branch: "main"
    enabled: true
```

---

## 2. Inspecting Monorepo & Deployment Status

To view the current status of all declared and custom mods:

```bash
make status
```

This outputs a clear matrix displaying:
- Mod ID and Enabled state
- Source type
- Download/cache presence in `mods/vendor/`
- Resolved/locked version or timestamp
- Active symlink status in RimWorld's `Mods` directory
- Any orphaned symlinks or targets

---

## 3. Deploying & Symlinking Mods

Monorepo mods (`mods/vendor/` and authoring mods in `mods/custom/`) are deployed into RimWorld's active `Mods` directory via idempotent symlinks.

### Preview Deployment (Dry-Run)
```bash
make link --dry-run
```

### Deploy Symlinks
```bash
make link
```

### Remove Monorepo Symlinks
```bash
make unlink --dry-run   # Preview links to remove
make unlink             # Remove symlinks safely
```

> **Safety Guarantee:** The linker guards official DLC/core directories (`Core`, `Royalty`, `Ideology`, `Biotech`, `Anomaly`) and will never overwrite pre-existing regular directories in your `Mods` folder.

---

## 4. Updating Mods & Lockfile

To check upstream sources for updates and re-synchronize `manifests/mods.lock.yaml`:

```bash
# Preview updates without modifying lockfile or downloading
make update-mods ARGS="--dry-run"

# Update all mods to latest resolved upstream version
make update-mods

# Update a single mod
make update-mods ARGS="harmony"
```

---

## 5. Version Rollback Workflow

If an updated mod causes game crashes, save-game incompatibilities, or breaking bugs, you can roll back to a specific previous release, tag, or version:

### Preview Rollback (Dry-Run)
```bash
make rollback --dry-run MOD=hugslib VERSION=v11.0.0
```

### Execute Rollback
```bash
make rollback MOD=hugslib VERSION=v11.0.0
```

### How Rollback Works:
1. **Manifest Pinning:** The mod's tag or version pin in `manifests/mods.yaml` is updated to the targeted version.
2. **Deterministic Fetch:** `scripts/fetch-mods.sh` downloads and extracts the exact targeted archive into `mods/vendor/<mod_id>`.
3. **Lockfile Synchronization:** `manifests/mods.lock.yaml` records the resolved rollback version and timestamp.
4. **Active Deployment:** Since the mod is symlinked, the updated vendor folder is immediately live in RimWorld without needing to recreate symlinks (or verify via `make link`).
