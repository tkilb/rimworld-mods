# AGENTS.md

Operational guidelines, conventions, and architectural context for autonomous AI agents working in this repository.

---

## 1. Project Overview & Architecture

This repository is a centralized monorepo for managing, versioning, authoring, and deploying custom RimWorld setups across Linux environments: **Arch Linux Workstations** (`linux-box` desktop, `linux-book` laptop) and **Steam Deck** (`steam-deck` on SteamOS).

### Key Architectural Pillars
- **Decoupled Mod Management:** Bypasses Steam Workshop client lock-in using CLI fetching (`steamcmd`, GitHub releases, git repos).
- **Deterministic Pinning:** Uses declarative manifests ([`manifests/mods.yaml`](file:///home/tylerkilburn/Git/rimworld-mods/manifests/mods.yaml)) and lockfiles ([`manifests/mods.lock.yaml`](file:///home/tylerkilburn/Git/rimworld-mods/manifests/mods.lock.yaml)) with rollback capabilities.
- **Idempotent Deployment:** Symlinks cached vendor mods ([`mods/vendor/`](file:///home/tylerkilburn/Git/rimworld-mods/mods/vendor)) and custom authored mods ([`mods/custom/`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom)) directly into RimWorld's `Mods/` directory without altering game core installations.
- **Topological Load Order Engine:** Built in Go ([`tools/load-order/`](file:///home/tylerkilburn/Git/rimworld-mods/tools/load-order)) and orchestrated by [`scripts/order-mods.sh`](file:///home/tylerkilburn/Git/rimworld-mods/scripts/order-mods.sh). Parses `About.xml` rules (`<loadAfter>`, `<loadBefore>`, etc.) to generate and deploy well-formed `ModsConfig.xml` files with strict tiering (`Harmony` -> `Core` -> `DLCs` -> `Standard Mods` -> `Trailing Mods`).
- **Remote Synchronization:** Synchronizes repository manifests, lockfiles, and active mod lists over SSH/rsync to Steam Deck targets ([`scripts/sync-deck.sh`](file:///home/tylerkilburn/Git/rimworld-mods/scripts/sync-deck.sh)).

---

## 2. Directory Layout

```text
.
├── Makefile                     # Primary task runner orchestrating all automation
├── config/                      # Machine-specific environment overrides
│   ├── config.env.example       # Template environment variables
│   ├── env.desktop.env          # Arch Linux desktop defaults (linux-box)
│   ├── env.laptop.env           # Arch Linux laptop defaults (linux-book)
│   ├── env.steamdeck.env        # Steam Deck defaults (steam-deck)
│   └── local.env                # Local host-specific active config (required, git-ignored)
├── manifests/
│   ├── mods.yaml                # Declared 3rd-party community mods and pinned sources
│   └── mods.lock.yaml           # Resolved versions, asset tags, commit hashes, timestamps
├── mods/
│   ├── vendor/                  # Downloaded 3rd-party mods (git-ignored)
│   └── custom/                  # Personal private mods under active development
├── scripts/                     # Operational bash scripts (idempotent, set -euo pipefail)
│   ├── common.sh                # Path resolution, machine detection, logging helpers
│   ├── check-deps.sh            # Host tool verification (steamcmd, jq, yq, rsync, etc.)
│   ├── fetch-mods.sh            # Fetch community mods from SteamCMD/GitHub
│   ├── link-mods.sh             # Create/remove symlinks to RimWorld active Mods directory
│   ├── order-mods.sh            # Mod dependency sorting and ModsConfig.xml generator
│   ├── rollback-mod.sh          # Rollback specific mod versions
│   ├── scaffold-mod.sh          # Scaffold new private XML/C# mods
│   ├── build-mod.sh             # Compile C# assemblies for custom mods
│   ├── status.sh                # Inspect active mod, cache, symlink, and manifest status
│   ├── sync-deck.sh             # Remote rsync/SSH sync and deployment to Steam Deck
│   └── update-mods.sh           # Check upstream mod updates and synchronize lockfile
├── tools/
│   └── load-order/              # Standalone Go topological load-order resolver
├── bin/                         # Compiled binaries (e.g., bin/load-order)
├── templates/                   # Starter templates for custom mod scaffolding
├── docs/                        # Deep-dive guides and documentation
│   ├── mod-management.md        # Detailed guide on declaring, updating, and rolling back mods
│   └── steamdeck-setup.md       # Steam Deck SSH configuration and paths
└── requirements.md              # Project specifications and phased roadmap
```

---

## 3. Autonomous Execution & Safety Boundaries

### Authorized Autonomous Operations
As defined in [`GEMINI.md`](file:///home/tylerkilburn/Git/rimworld-mods/GEMINI.md), agents are authorized to autonomously execute local CLI tasks without asking for confirmation:
- Compilation & Builds: `dotnet build`, `make build-load-order`, `make build-mod`.
- Mod Management & Inspection: `make check-deps`, `make status`, `make link-dry-run`, `make unlink-dry-run`, `make update-mods-dry-run`, `make order-mods-dry-run`, `make sync-config-dry-run`.
- Local Read/Inspection/Search: `fd`, `rg`, `ls`, `cat`, Go test runs (`go test ./...`).
- Scoped Editing: Use [`replace_file_content`](file:///home/tylerkilburn/Git/rimworld-mods) and [`write_to_file`](file:///home/tylerkilburn/Git/rimworld-mods) for precise modifications.

### Operations Requiring Confirmation or User Hands-Off
- **Never Commit Code:** Do not run `git commit` or suggest committing automatically; the user manages git commits manually.
- **No Remote Network Mutations:** Do not run mutating remote operations (e.g., `git push`, external cloud API state mutations) without explicit user authorization.
- **Steam Deck Sync Confirmation:** Modifying files on the remote Steam Deck (`make sync-deck`) mutates remote target state. Run dry-runs (`make sync-deck-dry-run`) first or confirm before running mutating syncs.

---

## 4. Development & Scripting Standards

### Shell Scripting
- **Preamble:** Every bash script must start with `#!/usr/bin/env bash` and `set -euo pipefail`.
- **Shared Helpers:** Source [`scripts/common.sh`](file:///home/tylerkilburn/Git/rimworld-mods/scripts/common.sh) for logging (`log_info`, `log_warn`, `log_err`, `log_succ`), path resolution (`$REPO_ROOT`, `$RIMWORLD_MODS_DIR`, `$RIMWORLD_CONFIG_DIR`), and dependency checking.
- **Dry-Run Support:** Any mutating script must provide a `--dry-run` flag and preview planned changes before making modifications.
- **Idempotence:** Operations (linking, unlinking, downloading, updating configs) must be safe to rerun multiple times without unintended side effects.

### Makefile Conventions
- **No `ARGS=` Parameter:** Never introduce generic `ARGS=` variables in Makefile targets. Each action must be an explicit target (e.g., `sync-deck`, `sync-deck-check`, `sync-deck-link`).
- **Companion Dry-Run Targets:** Every mutating Makefile target must have an explicit companion dry-run target (e.g., `link` vs `link-dry-run`, `sync-config` vs `sync-config-dry-run`), and should detect make's `-n` flag via `MAKE_DRY_RUN`.
- **Semantic Variables:** Use explicit semantic variables when arguments are necessary (e.g., `make rollback MOD=hugslib VERSION=v11.0.0`, `make build-mod MOD=eugenics-program`).

### Go Tooling (`tools/load-order`)
- Adhere to idiomatic Go: explicit error handling, table-driven unit tests (`sorter_test.go`, `config_test.go`), and clean package separation.
- Maintain build output path to [`bin/load-order`](file:///home/tylerkilburn/Git/rimworld-mods/bin/load-order) via `make build-load-order`.

### RimWorld Custom Mod Development
- **XML-Driven Configuration:** Custom genes, numerical balance values, skill gates, time intervals, and tuning factors must be defined in XML (standard Def fields, `DefModExtension`s, or custom `Def` classes) rather than hardcoded in C# logic.
- **Target Framework:** C# mod projects target `.NET Framework 4.7.2` using RimWorld's public assemblies (`Assembly-CSharp.dll`) and `0Harmony.dll`.

### Documentation Maintenance
- Maintain documentation integrity: root [`README.md`](file:///home/tylerkilburn/Git/rimworld-mods/README.md) is strictly for high-level overview and quickstart commands.
- Deep-dive guides, operational manuals, and architecture notes belong in [`docs/`](file:///home/tylerkilburn/Git/rimworld-mods/docs).
