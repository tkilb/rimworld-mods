# AGENTS.md

Operational guidelines, conventions, and architectural context for autonomous AI agents working in this repository.

---

## 1. Project Overview & Architecture

This repository is a centralized monorepo for managing, versioning, authoring, and deploying custom RimWorld setups across Linux environments: **Arch Linux Workstations** (`linux-box` desktop, `linux-book` laptop) and **Steam Deck** (`steam-deck` on SteamOS).

### Key Architectural Pillars
- **Unified Operational CLI (`rimmod`):** A native Go CLI (`cmd/rimmod`, `internal/`) that orchestrates all mod lifecycle operations, eliminating fragile shell script glue.
- **Decoupled Mod Management:** Bypasses Steam Workshop client lock-in using CLI fetching (`steamcmd`, GitHub releases, git repos).
- **Deterministic Pinning:** Uses declarative manifests ([`manifests/mods.yaml`](file:///home/tylerkilburn/Git/rimworld-mods/manifests/mods.yaml)) and lockfiles ([`manifests/mods.lock.yaml`](file:///home/tylerkilburn/Git/rimworld-mods/manifests/mods.lock.yaml)) with rollback capabilities.
- **Idempotent Deployment:** Symlinks cached vendor mods ([`mods/vendor/`](file:///home/tylerkilburn/Git/rimworld-mods/mods/vendor)) and custom authored mods ([`mods/custom/`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom)) directly into RimWorld's `Mods/` directory without altering game core installations.
- **Topological Load Order Engine:** Built directly into `internal/order` and executed by `rimmod order` / `rimmod apply`. Parses `About.xml` rules (`<loadAfter>`, `<loadBefore>`, etc.) to generate and deploy well-formed `ModsConfig.xml` files with strict tiering (`Harmony` -> `Core` -> `DLCs` -> `Standard Mods` -> `Trailing Mods`).
- **Remote Synchronization:** Synchronizes repository manifests, lockfiles, and active mod lists over SSH/rsync to Steam Deck targets via `rimmod deck push`.

---

## 2. Directory Layout

```text
.
├── Makefile                     # Build and install runner for rimmod CLI
├── bin/                         # Compiled binaries (e.g., bin/rimmod)
├── cmd/rimmod/                  # Cobra CLI application commands
├── config/                      # Machine-specific environment overrides
│   ├── config.env.example       # Template environment variables
│   ├── env.desktop.env          # Arch Linux desktop defaults (linux-box)
│   ├── env.laptop.env           # Arch Linux laptop defaults (linux-book)
│   ├── env.steamdeck.env        # Steam Deck defaults (steam-deck)
│   └── local.env                # Local host-specific active config (required, git-ignored)
├── docs/                        # Deep-dive guides and documentation
│   ├── mod-management.md        # Detailed guide on declaring, updating, and rolling back mods
│   └── steamdeck-setup.md       # Steam Deck SSH configuration and paths
├── internal/                    # Core Go packages (config, fetcher, linker, order, tidy, updater)
├── manifests/
│   ├── mods.yaml                # Declared 3rd-party community mods and pinned sources
│   └── mods.lock.yaml           # Resolved versions, asset tags, commit hashes, timestamps
├── mods/
│   ├── vendor/                  # Downloaded 3rd-party mods (git-ignored)
│   ├── forked/                  # Forked & fine-tuned 3rd-party mods under monorepo git control
│   └── custom/                  # Personal private mods under active development
├── templates/                   # Starter templates for custom mod scaffolding
└── tools/                       # Ancillary tooling (load-order)
```

---

## 3. Autonomous Execution & Safety Boundaries

### Authorized Autonomous Operations
As defined in [`GEMINI.md`](file:///home/tylerkilburn/Git/rimworld-mods/GEMINI.md), agents are authorized to autonomously execute local CLI tasks without asking for confirmation:
- Compilation & Builds: `dotnet build`, `make build`, `make install`, `rimmod dev build <mod> --dry-run`.
- Mod Management & Inspection: `rimmod status`, `rimmod apply --dry-run`, `rimmod update --dry-run`, `rimmod tidy --dry-run`, `rimmod order`, `rimmod deck push --check`, `rimmod deck push --dry-run`.
- Local Read/Inspection/Search: `fd`, `rg`, `ls`, `cat`, Go test runs (`go test ./...`).
- Scoped Editing: Use [`replace_file_content`](file:///home/tylerkilburn/Git/rimworld-mods) and [`write_to_file`](file:///home/tylerkilburn/Git/rimworld-mods) for precise modifications.

### Operations Requiring Confirmation or User Hands-Off
- **Never Commit Code:** Do not run `git commit` or suggest committing automatically; the user manages git commits manually.
- **No Remote Network Mutations:** Do not run mutating remote operations (e.g., `git push`, external cloud API state mutations) without explicit user authorization.
- **Steam Deck Sync Confirmation:** Modifying files on the remote Steam Deck (`rimmod deck push`) mutates remote target state. Run dry-runs (`rimmod deck push --dry-run`) first or confirm before running mutating syncs.
- **No DLL Crawling / Decompilation:** Never crawl, disassemble, inspect IL tokens of, or reflect into compiled binary DLLs (e.g. `Assembly-CSharp.dll`, engine assemblies). All modding decisions, logic, and debugging must be derived from mod source code, game XML definitions, documentation, and observable mod state. If internal game engine behavior is uncertain, state the hypothesis directly and ask or test via normal mod logic rather than launching disassembly scripts.

---

## 4. Development & Tooling Standards

### Go Tooling (`rimmod`)
- Adhere to idiomatic Go: explicit error handling, table-driven unit tests, and clean package separation in `internal/`.
- Maintain build output path to [`bin/rimmod`](file:///home/tylerkilburn/Git/rimworld-mods/bin/rimmod) and installation to `~/bin/rimmod` via `make install`.
- Dry-run flags: Every mutating command must provide `--dry-run` (`-n`) preview capabilities.

### RimWorld Custom Mod Development
- **XML-Driven Configuration:** Custom genes, numerical balance values, skill gates, time intervals, and tuning factors must be defined in XML (standard Def fields, `DefModExtension`s, or custom `Def` classes) rather than hardcoded in C# logic.
- **Target Framework & Game Version:** C# mod projects target `RimWorld 1.6` (`.NET Framework 4.7.2`) using RimWorld's public assemblies (`Assembly-CSharp.dll`) and `0Harmony.dll`.

### Documentation Maintenance
- Maintain documentation integrity: root [`README.md`](file:///home/tylerkilburn/Git/rimworld-mods/README.md) is strictly for high-level overview and quickstart commands.
- Deep-dive guides, operational manuals, and architecture notes belong in [`docs/`](file:///home/tylerkilburn/Git/rimworld-mods/docs).
