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

### Deploying / Linking Mods

#### Link All Mods
```bash
make link-dry-run   # Preview symlink creation
make link           # Deploy symlinks and sync ModsConfig.xml
```

#### Link a Specific Mod
Deploy or update the symlink for a single mod (matches the manifest identifier or custom mod folder name, e.g. `harmony`, `my-mod`):
```bash
# Preview linking a specific mod
make link-dry-run MOD=harmony

# Link a specific mod and synchronize load order
make link MOD=harmony

# Call script directly (optional --skip-config avoids re-generating ModsConfig.xml)
bash ./scripts/link-mods.sh --link harmony
bash ./scripts/link-mods.sh --link harmony --skip-config
```

### Removing / Unlinking Mods

#### Unlink All Mods
```bash
make unlink-dry-run   # Preview links to remove
make unlink           # Remove all monorepo symlinks safely
```

#### Unlink a Specific Mod
Remove the symlink for a single mod without affecting other linked mods:
```bash
# Preview unlinking a specific mod
make unlink-dry-run MOD=harmony

# Unlink a specific mod
make unlink MOD=harmony

# Alternatively, call the script directly
bash ./scripts/link-mods.sh --unlink harmony
```

> **Safety Guarantee:** The linker guards official DLC/core directories (`Core`, `Royalty`, `Ideology`, `Biotech`, `Anomaly`) and will never overwrite pre-existing regular directories in your `Mods` folder.

---

## 4. Updating Mods & Lockfile

To check upstream sources for updates and re-synchronize `manifests/mods.lock.yaml`:

```bash
# Preview updates without modifying lockfile or downloading
make update-mods-dry-run

# Update all mods to latest resolved upstream version
make update-mods

# Update a single mod (call script directly)
bash ./scripts/update-mods.sh harmony
```

---

## 5. Version Rollback Workflow

If an updated mod causes game crashes, save-game incompatibilities, or breaking bugs, you can roll back to a specific previous release, tag, or version:

### Preview Rollback (Dry-Run)
```bash
make rollback-dry-run MOD=hugslib VERSION=v11.0.0
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

---

## 6. Mod Load Order & `ModsConfig.xml` Synchronization

The monorepo includes an automated topological load order resolver and `ModsConfig.xml` generator inspired by RimSort and RimPy. It resolves dependencies and ordering constraints from mod `About.xml` metadata, manifest priority/after/before rules, and strict canonical tiers:
1. **Harmony** (`brrainz.harmony`)
2. **Core** (`ludeon.rimworld`)
3. **Official DLCs** (`royalty`, `ideology`, `biotech`, `anomaly`)
4. **Standard Mods** (topologically sorted via Kahn's DAG algorithm)
5. **Trailing Mods** (e.g. `RocketMan`)

### Preview Computed Load Order
```bash
make order-mods-dry-run
```

### Preview `ModsConfig.xml` Generation
```bash
make sync-config-dry-run
```

### Generate & Deploy `ModsConfig.xml`
```bash
make sync-config
```

> **Automated Integration:** `make link` automatically triggers `sync-config`, deploying both the symlinks and the updated `ModsConfig.xml` in a single command. If you only want to symlink without touching `ModsConfig.xml`, pass `--skip-config` directly to `scripts/link-mods.sh`.
>
> **Automatic Backup:** When `sync-config` runs, any existing `ModsConfig.xml` at the target path is automatically backed up to `ModsConfig.xml.bak` before atomic replacement, and the game version tag (e.g., `<version>1.6.4871 rev600</version>`) is preserved.

---

## 7. Remote Deployment to Steam Deck

Sync the monorepo (vendor mods, manifests, lockfile, scripts) to a remote Steam Deck via SSH/rsync.

### Test SSH Connectivity
```bash
make sync-deck-check
```

### Preview Sync (Dry-Run)
```bash
make sync-deck-dry-run
```

### Sync Repository to Steam Deck
```bash
make sync-deck
```

### Sync and Automatically Deploy Symlinks on Deck
Sync files and immediately run `make link` on the Steam Deck in one step:
```bash
make sync-deck-link
```

> For complete setup instructions (enabling SSH on SteamOS, key-based auth, storage path detection for internal SSD vs MicroSD), see [Steam Deck Setup Guide](steamdeck-setup.md).

---

## 8. Authoring Private Mods (`mods/custom/`)

Private mods under active development reside in `mods/custom/` and can be scaffolded using the starter templates.

### Scaffolding a New Mod

To scaffold a new mod:

```bash
# Preview scaffolding (dry-run)
make scaffold-mod-dry-run MOD=my-mod TYPE=xml
make scaffold-mod-dry-run MOD=my-mod TYPE=csharp

# Scaffold an XML mod (default)
make scaffold-mod MOD=my-mod

# Scaffold a C# mod with project files and assembly references
make scaffold-mod MOD=my-mod TYPE=csharp
```

The scaffolding script creates standard RimWorld layouts in `mods/custom/<mod_name>`:
- `About/About.xml`: Valid metadata, `supportedVersions` (1.5, 1.6), and load dependencies.
- `Defs/`: Sample XML definitions.
- `Source/`: (C# mods) SDK-style `.csproj` configured for `net472` with dynamic references to RimWorld's `RimWorldLinux_Data/Managed/` game assemblies.
- `Assemblies/`: (C# mods) Build target directory.

### Building C# Mods

To compile assemblies for a custom C# mod:

```bash
# Preview compilation command
make build-mod-dry-run MOD=my-mod

# Compile assembly to mods/custom/<mod_name>/Assemblies/
make build-mod MOD=my-mod
```

*Note:* Compiling C# mods requires the .NET SDK (`sudo pacman -S dotnet-sdk` on Arch Linux).

### Deploying & Testing

Custom mods with a valid `About/About.xml` are automatically recognized by `make link`, `make status`, and `make order-mods`. To activate them in RimWorld:

```bash
# Link all mods
make link

# Or link only this specific custom mod
make link MOD=my-mod
```

