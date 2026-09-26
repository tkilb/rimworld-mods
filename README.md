# RimWorld Monorepo

Centralized control center for managing custom RimWorld setups across Linux machines (**Arch Linux Desktop** and **Steam Deck**).

This monorepo allows you to:
- Bypass Steam Workshop client lock-in and manage community mods independently via CLI.
- Explicitly pin and lock mod versions (`manifests/mods.yaml` & `manifests/mods.lock.yaml`) with deterministic rollback capabilities.
- Safely deploy mods via idempotent symlinking into RimWorld's `Mods` directory without overwriting core game files.
- Author and test personal custom mods in `mods/custom/` alongside 3rd-party vendor mods.

---

## Quickstart & Daily Usage

All common operations are orchestrated through `make`. Run `make help` at any time to list available targets.

### 1. Check System Dependencies
Verify tools (`steamcmd`, `jq`, `yq`, `rsync`, `curl`):
```bash
make check-deps
```

### 2. Inspect Mod & Deployment Status
Check declared mods, cached downloads, locked versions, and active symlinks:
```bash
make status
```

### 3. Fetch Declared Mods
Download community mods declared in `manifests/mods.yaml`:
```bash
# Preview what would be fetched
bash ./scripts/fetch-mods.sh --dry-run

# Fetch all enabled mods
make fetch-mods

# Fetch a single mod
bash ./scripts/fetch-mods.sh hugslib
```

### 4. Deploy Mods to RimWorld (Symlinking)
Deploy vendor and custom mods into RimWorld's active `Mods` folder:
```bash
# Preview links before applying
make link-dry-run

# Deploy symlinks
make link

# Remove monorepo symlinks
make unlink-dry-run
make unlink
```

### 5. Update Mods & Synchronize Lockfile
Check upstream sources for updates and re-sync `manifests/mods.lock.yaml`:
```bash
# Preview updates without modifying files
make update-mods-dry-run

# Update and re-fetch all mods
make update-mods
```

### 6. Roll Back a Mod Version
Roll back an individual mod to a specific version or tag:
```bash
# Preview rollback
make rollback-dry-run MOD=hugslib VERSION=v11.0.0

# Execute rollback
make rollback MOD=hugslib VERSION=v11.0.0
```

### 7. Sync to Steam Deck
Transfer mods and lockfile to your Steam Deck and optionally deploy symlinks remotely:
```bash
make sync-deck-check          # Test SSH connectivity
make sync-deck-dry-run        # Preview rsync transfer
make sync-deck                # Sync files to Deck
make sync-deck-link           # Sync and immediately run 'make link' on the Deck
```

---

## Directory Overview

```text
.
├── Makefile                     # Top-level task runner
├── config/                      # Host-specific environment configuration
├── manifests/
│   ├── mods.yaml                # Declared 3rd-party mods and desired versions/sources
│   └── mods.lock.yaml           # Resolved versions, asset tags, and timestamps
├── mods/
│   ├── vendor/                  # Cached 3rd-party downloaded mods (git-ignored)
│   └── custom/                  # Personal private mods under authoring
├── scripts/                     # Idempotent operational bash & python automation scripts
└── docs/                        # Detailed guides and deep-dive documentation
```

---

## Detailed Documentation (`/docs`)

For detailed step-by-step guides, refer to the documentation in [`docs/`](docs/):

- [**Mod Management Guide**](docs/mod-management.md): In-depth guide for declaring new mods, source schemas (Workshop, GitHub releases, Git), updating lockfiles, rollback mechanics, and troubleshooting.
- [**Steam Deck Setup**](docs/steamdeck-setup.md): Remote synchronization, SSH setup, storage path detection, and running monorepo tools natively on SteamOS.
