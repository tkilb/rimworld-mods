# RimWorld Monorepo

Centralized control center for managing custom RimWorld setups across Linux machines (**Arch Linux Desktop**, **Laptop**, and **Steam Deck**).

This monorepo allows you to:
- Bypass Steam Workshop client lock-in and manage community mods independently via the native `rimmod` CLI.
- Explicitly pin and lock mod versions (`manifests/mods.yaml` & `manifests/mods.lock.yaml`) with deterministic rollback capabilities.
- Safely deploy mods via idempotent symlinking into RimWorld's `Mods` directory without overwriting core game files.
- Author and test personal custom mods in `mods/custom/` alongside 3rd-party vendor mods.
- Reconcile local and remote (Steam Deck) setups in a single command.

---

## Quickstart & Daily Usage

All daily operations are performed directly through the `rimmod` CLI.

```bash
# Verify monorepo health, active mods, symlinks, and target environment
rimmod status

# Reconcile local RimWorld installation (fetch missing, link, sort, deploy ModsConfig.xml)
rimmod apply

# Preview reconciliation without modifying files
rimmod apply --dry-run

# Check upstream sources for updates, sync lockfile, and re-apply
rimmod update
rimmod update --mod <mod_id>

# Prune unreferenced vendor downloads, lockfile entries, and dead symlinks
rimmod tidy

# Inspect computed topological mod load order and dependencies
rimmod order

# Compile a custom C# mod and automatically re-apply
rimmod dev build <mod_name>

# Scaffold a new starter template for an XML or C# mod
rimmod dev scaffold <mod_name> --type csharp

# Synchronize manifests, mods, and load order to Steam Deck over SSH/rsync
rimmod deck push --check       # Test SSH connectivity
rimmod deck push --dry-run     # Preview remote transfer and reconciliation
rimmod deck push               # Sync and reconcile Steam Deck

# Interactive TUI to cherry-pick Steam Workshop subscriptions from Steam Deck
rimmod deck pull
```

---

## Installation & Build

Build and install `rimmod` into `~/bin/` (or custom `$BINDIR`):

```bash
# Compile and install to ~/bin/rimmod
make install

# Run test suite
make test
```

---

## Directory Overview

```text
.
├── Makefile                     # Build & installation runner for rimmod CLI
├── bin/                         # Compiled binaries (rimmod)
├── cmd/rimmod/                  # Cobra CLI entrypoint and commands
├── config/                      # Host-specific environment configuration
├── docs/                        # Detailed guides and deep-dive documentation
├── internal/                    # Core Go packages (linker, fetcher, order, updater, etc.)
├── manifests/
│   ├── mods.yaml                # Declared 3rd-party mods and desired versions/sources
│   └── mods.lock.yaml           # Resolved versions, asset tags, and timestamps
├── mods/
│   ├── vendor/                  # Cached 3rd-party downloaded mods (git-ignored)
│   └── custom/                  # Personal private mods under authoring
├── templates/                   # Starter templates for custom mod scaffolding
└── tools/                       # Ancillary tooling (load-order)
```

---

## Detailed Documentation (`/docs`)

- [**Mod Management Guide**](docs/mod-management.md): In-depth guide for declaring new mods, source schemas (Workshop, GitHub releases, Git), updating lockfiles, and lifecycle workflows.
- [**Steam Deck Setup**](docs/steamdeck-setup.md): Remote synchronization, SSH setup, storage path detection, and running `rimmod` on SteamOS.
