# RimWorld Monorepo Specification & Agent Task Roadmap

## 1. Project Overview & Goals

This monorepo serves as a centralized control center for managing a custom RimWorld setup across multiple Linux machines:

- **Arch Linux Desktop**
- **Steam Deck (SteamOS)**

### Core Capabilities

1. **Bypass Workshop Lock-in:** Manage and install community mods independently of the Steam Workshop client for granular control.
2. **Version Pinning & Rollbacks:** Track mod versions explicitly (via lockfile/manifests) with the ability to roll back individual or all mods when issues arise.
3. **Multi-Environment Support:** Single source of truth adaptable to local paths on Arch Linux and remote/local Steam Deck targets (via shell/SSH).
4. **Makefile Task Runner:** Manage all operational tasks and chore scripts (`make sync`, `make update`, `make status`, `make link`, etc.)
5. **Private Mod Scaffolding (Future Phase):** Dedicated workspace structure for authoring personal C# and XML RimWorld mods without publishing to the workshop.

---

## 2. Monorepo Architecture & Structure

```text
.
├── Makefile                     # Top-level task runner
├── config/
│   ├── env.desktop.env          # Arch desktop configuration (paths, SteamCMD config)
│   ├── env.steamdeck.env        # Steam Deck configuration
│   └── config.env.example       # Example template
├── manifests/
│   ├── mods.yaml                # Declared 3rd-party community mods and pinned versions/sources
│   └── mods.lock.yaml           # Resolved versions, commit hashes, or workshop revision IDs
├── scripts/
│   ├── common.sh                # Shared bash utilities (logging, path resolution, OS check)
│   ├── fetch-mods.sh            # Download/fetch 3rd-party mods (SteamCMD / GitHub releases)
│   ├── link-mods.sh             # Idempotent symlinker to RimWorld Mods directory
│   ├── update-mods.sh           # Check for updates, update lockfile
│   └── status.sh                # Inspect active mod versions vs installed RimWorld state
├── mods/
│   ├── vendor/                  # Stored/cached 3rd-party mods (git-ignored or LFS if large)
│   └── custom/                  # (Phase 5) Personal private mods under active development
└── docs/
    ├── steamdeck-setup.md       # SSH setup, path configuration, and remote sync guide
    └── mod-management.md        # User guide for adding, pinning, and updating mods
```

---

## 3. Human-in-the-Loop QA & Chunked Work Protocol

To maintain high quality and reliability across Linux environments:

- **Small, atomic chunks:** Each task produces a single self-contained deliverable.
- **Idempotent CLI scripts:** Bash scripts must use `#!/usr/bin/env bash`, `set -euo pipefail`, and support `--dry-run` where applicable.
- **Zero unconfirmed mutations:** Scripts must preview changes before modifying directories.
- **Human Verification Checkpoints:** After each chunk, execution halts for manual review and validation by the user before proceeding to the next chunk.
- **Manual Commits:** The agent will not perform git commits; all git commits are made manually by the user.

---

## 4. Chunked Agent Task Breakdown

### Phase 1: Foundation, Configuration, & Shared Tooling

- [x] **Task 1.1: Project Directory Structure & Baseline Makefile**
  - **Goal:** Establish standard monorepo folder layout and baseline `Makefile` with help/discovery targets.
  - **Deliverables:**
    - Directory scaffold (`config/`, `manifests/`, `scripts/`, `mods/vendor/`, `mods/custom/`, `docs/`).
    - `Makefile` with `.DEFAULT_GOAL := help` and standard targets (`help`, `check-deps`, `status`).
    - `.gitignore` for vendor cache and temporary download directories.
  - **QA Step:** Run `make help` and verify clean output and proper directory layout.

- [x] **Task 1.2: Environment Configuration & Path Resolver**
  - **Goal:** Support environment-specific path detection for Arch Linux and Steam Deck.
  - **Deliverables:**
    - Self-contained `$MACHINE` path resolver in `scripts/common.sh` (detects `linux-box`, `steam-deck`, etc.).
    - `scripts/common.sh` containing shell helper functions (`log_info`, `log_err`, `check_cmd`).
  - **QA Step:** Source `scripts/common.sh` with a test environment file and verify path validations and error handling.

- [x] **Task 1.3: Dependency Checker Script**
  - **Goal:** Verify required host dependencies on Arch Linux and SteamOS (`steamcmd`, `curl`, `jq` / `yq`, `rsync`, etc.).
  - **Deliverables:**
    - `scripts/check-deps.sh` integrated into `make check-deps`.
    - Clear remediation instructions when a dependency is missing.
  - **QA Step:** Run `make check-deps` on the host machine and confirm all dependency checks pass.

---

### Phase 2: Mod Manifest & Acquisition Engine

- [x] **Task 2.1: Mod Manifest Schema & Lockfile Specification**
  - **Goal:** Define schema for declaring mods and their sources (Steam Workshop ID, GitHub release, or Git repo).
  - **Deliverables:**
    - `manifests/mods.yaml` specification with sample entries supporting:
      - Workshop items (ID, pinned update date/revision if applicable).
      - Direct Git/GitHub release repositories.
    - Initial lockfile format (`manifests/mods.lock.yaml`).
  - **QA Step:** Validate `mods.yaml` structure using `yq` or validation helper.

- [x] **Task 2.2: Mod Downloader / Fetcher Script**
  - **Goal:** Download community mods without running the Steam desktop client.
  - **Deliverables:**
    - `scripts/fetch-mods.sh` implementing:
      - Anonymous/user SteamCMD download for Steam Workshop items into `mods/vendor/<mod-name>`.
      - GitHub release archive download / extraction fallback.
      - Pinned version caching.
  - **QA Step:** Download a small test community mod via `scripts/fetch-mods.sh` and verify files are extracted cleanly to `mods/vendor/`.

- [x] **Task 2.3: Mod Update & Lockfile Synchronization**
  - **Goal:** Provide a controlled mechanism to update all 3rd-party mods to latest or to a specific target version.
  - **Deliverables:**
    - `scripts/update-mods.sh` (wired to `make update-mods`).
    - Lockfile updater recording exact downloaded versions/timestamps.
  - **QA Step:** Execute `make update-mods --dry-run` and live update, checking that lockfile reflects the downloaded state.

---

### Phase 3: Deployment, Symlinking, & Rollback Engine

- [ ] **Task 3.1: Idempotent Mod Linker Script**
  - **Goal:** Deploy mods to RimWorld's local `Mods` folder using clean symlinks or rsync.
  - **Deliverables:**
    - `scripts/link-mods.sh` (wired to `make link` and `make unlink`).
    - Support for `--dry-run` showing what links will be created or removed.
    - Safety checks avoiding overwriting game core files.
  - **QA Step:** Run `make link --dry-run` and inspect planned symlinks; execute `make link` and confirm links in RimWorld's mod folder.

- [ ] **Task 3.2: Status & Integrity Inspection Tool**
  - **Goal:** Compare manifest declarations, locked versions, cached vendor mods, and deployed symlinks.
  - **Deliverables:**
    - `scripts/status.sh` (wired to `make status`).
    - Output showing: declared mods, downloaded status, active symlink status, and version drift.
  - **QA Step:** Run `make status` under various states (unlinked, linked, missing mod) and verify accurate reporting.

- [ ] **Task 3.3: Version Rollback Workflow**
  - **Goal:** Allow rolling back individual mods or all mods to previous versions using lockfile snapshots or cached versions.
  - **Deliverables:**
    - `scripts/rollback-mod.sh` (wired to `make rollback MOD=<name> VERSION=<ver>`).
    - Documentation in `docs/mod-management.md` explaining the rollback process.
  - **QA Step:** Test rolling back a mod to a cached previous version and verify symlink updates accordingly.

---

### Phase 4: Steam Deck Remote Deployment & Automation

- [ ] **Task 4.1: Remote Sync Script for Steam Deck**
  - **Goal:** Allow syncing mods and lockfiles from the desktop to the Steam Deck via SSH/rsync.
  - **Deliverables:**
    - `scripts/sync-deck.sh` (wired to `make sync-deck`).
    - Support for executing remote `make link` over SSH on the Steam Deck.
  - **QA Step:** Test SSH connectivity and dry-run rsync against Steam Deck or local mock target.

- [ ] **Task 4.2: Comprehensive Documentation & Onboarding Guides**
  - **Goal:** Document daily workflows for mod management, updating, and multi-machine sync.
  - **Deliverables:**
    - `docs/steamdeck-setup.md` (SSH setup, path discovery on SteamOS).
    - `docs/mod-management.md` (Adding mods, updating, rollbacks, troubleshooting).
  - **QA Step:** Review documentation against all implemented Makefile targets.

---

### Phase 5: Personal Mod Development Scaffolding (Future Phase)

- [ ] **Task 5.1: Private Mod Project Template (XML / C#)**
  - **Goal:** Create starter templates and build scripts for private mods in `mods/custom/`.
  - **Deliverables:**
    - Template directory with `About/About.xml`, `Defs/`, and optional `.csproj` for C# patches with RimWorld assembly references.
    - Integration with `make link` so private mods are symlinked alongside vendor mods.
  - **QA Step:** Scaffold a test custom XML mod and verify it loads in RimWorld.

- [ ] **Task 5.2: Embryo Gene Editor Mod Implementation (Biotech Expansion)**
  - **Goal:** Implement the Embryo Gene Editor mod based on the detailed specification at `mods/custom/embryo-gene-editor/spec.md`.
  - **Deliverables:**
    - Optimizer genes with bundled Cellular Instability penalties (`GeneDef`s, research projects, Gene Fabrication mod compatibility).
    - Physical Genome Blueprint Discs (`ThingDef: GenomeBlueprintDisk`, `CompGenomeBlueprint`) and encoding recipes.
    - Custom UI (`Dialog_EditEmbryoGenes`) for adding/removing embryo genes via connected Gene Banks.
    - Job drivers, workgivers, and batch assembly bills for applying blueprints to natural and cloned embryos.
    - Integration testing with *Biotech Cloning Continued* and vanilla `Building_GrowthVat`.
  - **QA Step:** Build C# assembly, deploy via `make link`, and verify embryo genetic modifications, disc burning, and growth vat gestation in-game.

