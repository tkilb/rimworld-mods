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

### Forked Community Mods
When maintaining customized, patched, or fine-tuned versions of open-source 3rd-party mods, house their source tree directly in `mods/forked/<mod-id>` and declare them with `source: forked`:
```yaml
mods:
  turnitonandoffrepowered:
    name: "Turn It On and Off - RePowered"
    source: forked
    upstream_repo: "https://github.com/emipa606/TurnOnOffRePowered"
    upstream_branch: "main"
    upstream_commit: "75d5341"
    package_id: "Mlie.TurnItOnAndOffRePowered"
    enabled: true
```
Forked mods are tracked directly in monorepo git history, compiled with `rimmod dev build <mod-id>`, and symlinked directly via `rimmod apply`.

---

## 2. Importing Mods via URL (`rimmod import`)

You can import mods directly from URLs without manually crafting YAML entries. Supported sources include Steam Workshop URLs (or raw Workshop IDs), GitHub releases, and Git repositories:

```bash
# Preview importing a Steam Workshop mod
rimmod import --dry-run "https://steamcommunity.com/sharedfiles/filedetails/?id=3525571078"

# Import mod, download via steamcmd, inspect About.xml, and add kebab-cased slug to manifests/mods.yaml
rimmod import "https://steamcommunity.com/sharedfiles/filedetails/?id=3525571078"

# Import with a custom slug and immediately apply/sort load order
rimmod import "https://steamcommunity.com/sharedfiles/filedetails/?id=3525571078" --slug launch-sites-expire --apply

# Or import by raw Workshop ID
rimmod import 3525571078

# Import a GitHub mod
rimmod import "https://github.com/UnlimitedHugs/RimworldHugsLib"
```

The importer:
1. Downloads the mod into staging.
2. Locates and inspects `About/About.xml` to discover the exact `packageId`, canonical mod name, and description.
3. Automatically derives a kebab-case slug (e.g. `launch-sites-expire`), avoiding collisions.
4. Moves the files into `mods/vendor/<slug>`.
5. Appends the entry to `manifests/mods.yaml` while preserving all comments and structure.

---

## 3. Inspecting Monorepo & Deployment Status

To view the current status of all declared and custom mods:

```bash
rimmod status
```

This outputs a clear matrix displaying:
- Mod ID and Enabled state
- Source type
- Download/cache presence in `mods/vendor/`
- Resolved/locked version or timestamp
- Active symlink status in RimWorld's `Mods` directory
- Any orphaned symlinks or targets

---

## 4. Deploying & Symlinking Mods

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

## 5. Updating Mods & Lockfile

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

## 6. Mod Load Order & `ModsConfig.xml` Synchronization

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

Sync the monorepo (vendor mods, manifests, lockfile, custom mods) to a remote Steam Deck via SSH/rsync and automatically apply remotely.

### Test SSH Connectivity
```bash
rimmod deck push --check
```

### Preview Sync (Dry-Run)
```bash
rimmod deck push --dry-run
```

### Sync Repository and Automatically Reconcile on Deck
Syncs all files and triggers remote reconciliation in one step:
```bash
rimmod deck push
```

> For complete setup instructions (enabling SSH on SteamOS, key-based auth, storage path detection for internal SSD vs MicroSD), see [Steam Deck Setup Guide](steamdeck-setup.md).

---

## 8. Authoring Private Mods (`mods/custom/`)

Private mods under active development reside in `mods/custom/` and can be scaffolded using the starter templates.

### Scaffolding a New Mod

To scaffold a new mod:

```bash
# Preview scaffolding (dry-run)
rimmod dev scaffold my-mod --type xml --dry-run
rimmod dev scaffold my-mod --type csharp --dry-run

# Scaffold an XML mod (default)
rimmod dev scaffold my-mod

# Scaffold a C# mod with project files and assembly references
rimmod dev scaffold my-mod --type csharp
```

The scaffolding command creates standard RimWorld layouts in `mods/custom/<mod_name>`:
- `About/About.xml`: Valid metadata, `supportedVersions` (1.5, 1.6), and load dependencies.
- `Defs/`: Sample XML definitions.
- `Source/`: (C# mods) SDK-style `.csproj` configured for `net472` with dynamic references to RimWorld's `RimWorldLinux_Data/Managed/` game assemblies.
- `Assemblies/`: (C# mods) Build target directory.

### Building C# Mods

To compile assemblies for a custom C# mod and automatically reconcile into RimWorld:

```bash
# Preview compilation command
rimmod dev build my-mod --dry-run

# Compile assembly to mods/custom/<mod_name>/Assemblies/ and auto-apply
rimmod dev build my-mod
```

*Note:* Compiling C# mods requires the .NET SDK (`sudo pacman -S dotnet-sdk` on Arch Linux).

### Deploying & Testing

Custom mods with a valid `About/About.xml` are automatically recognized by `rimmod apply`, `rimmod status`, and `rimmod order`. To activate them in RimWorld:

```bash
# Reconcile all mods (symlinks and load order)
rimmod apply
```

