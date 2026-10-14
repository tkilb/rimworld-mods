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
rimmod apply --dry-run   # Preview local reconciliation
rimmod apply             # Deploy symlinks, sort dependencies, and sync ModsConfig.xml
```

### Removing / Unlinking Mods

To disable or remove a mod, set `enabled: false` (or delete its entry) in `manifests/mods.yaml`, then run:
```bash
rimmod apply
```
`rimmod` automatically detects unmanaged or disabled mods, removes their symlinks, and recalculates `ModsConfig.xml`.

To prune unreferenced cached vendor folders or lockfile entries:
```bash
rimmod tidy --dry-run    # Preview pruning
rimmod tidy              # Remove dead cache and unreferenced entries
```

---

## 4. Updating Mods & Lockfile

To check upstream sources for updates and re-synchronize `manifests/mods.lock.yaml`:

```bash
# Preview updates without modifying lockfile or downloading
rimmod update --dry-run

# Update all mods to latest resolved upstream version and re-apply
rimmod update

# Update a single mod
rimmod update --mod <mod_id>
```

---

## 5. Mod Load Order & `ModsConfig.xml` Synchronization

The monorepo includes an automated topological load order resolver and `ModsConfig.xml` generator. It resolves dependencies and ordering constraints from mod `About.xml` metadata, manifest priority/after/before rules, and strict canonical tiers:
1. **Harmony** (`brrainz.harmony`)
2. **Core** (`ludeon.rimworld`)
3. **Official DLCs** (`royalty`, `ideology`, `biotech`, `anomaly`, `odyssey`)
4. **Standard Mods** (topologically sorted via Kahn's DAG algorithm)
5. **Trailing Mods** (e.g. `RocketMan`)

### Inspect Computed Load Order
```bash
rimmod order
```

### Reconcile & Deploy `ModsConfig.xml`
```bash
rimmod apply --dry-run   # Preview
rimmod apply             # Execute
```

> **Automatic Backup:** When `rimmod apply` runs, any existing `ModsConfig.xml` at the target path is automatically backed up to `ModsConfig.xml.bak` before atomic replacement, and the game version tag (e.g., `<version>1.6.4871 rev600</version>`) is preserved.

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

