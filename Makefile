SHELL := /usr/bin/env bash
.DEFAULT_GOAL := help

.PHONY: help check-deps status fetch-mods update-mods update-mods-dry-run \
        link link-dry-run unlink unlink-dry-run rollback rollback-dry-run \
        tidy tidy-dry-run \
        build-load-order build-order-mods order-mods order-mods-dry-run \
        sync-config sync-config-dry-run \
        import-deck import-deck-dry-run import-deck-mods import-deck-mods-dry-run \
        sync-deck sync-deck-dry-run sync-deck-check \
        scaffold-mod scaffold-mod-dry-run build-mod build-mod-dry-run

##@ General
help: ## Display user guide and command reference
	@bash ./scripts/help.sh

##@ Verification & Diagnostics
check-deps: ## Verify system dependencies (steamcmd, jq, yq, rsync, etc.)
	@bash ./scripts/check-deps.sh

status: ## Inspect active mod versions, downloaded cache, and symlinks
	@bash ./scripts/status.sh

##@ Mod Management
fetch-mods: ## Download/fetch declared mods from Steam Workshop and Git
	@bash ./scripts/fetch-mods.sh

update-mods: ## Update 3rd-party mods and synchronize lockfile
	@bash ./scripts/update-mods.sh

update-mods-dry-run: ## Preview 3rd-party mod updates without modifying files
	@bash ./scripts/update-mods.sh --dry-run


# Detect if make was invoked with -n / --dry-run
MAKE_DRY_RUN := $(if $(findstring n,$(firstword -$(MAKEFLAGS))),--dry-run,)

link: ## Deploy idempotent symlinks into RimWorld Mods directory (Usage: make link [MOD=<name>])
	+@bash ./scripts/link-mods.sh --link $(if $(MOD),$(MOD),) $(MAKE_DRY_RUN)

link-dry-run: ## Preview symlink deployment without modifying filesystem (Usage: make link-dry-run [MOD=<name>])
	@bash ./scripts/link-mods.sh --link $(if $(MOD),$(MOD),) --dry-run

unlink: ## Remove monorepo symlinks from RimWorld Mods directory (Usage: make unlink [MOD=<name>])
	+@bash ./scripts/link-mods.sh --unlink $(if $(MOD),$(MOD),) $(MAKE_DRY_RUN)

unlink-dry-run: ## Preview removal of monorepo symlinks (Usage: make unlink-dry-run [MOD=<name>])
	@bash ./scripts/link-mods.sh --unlink $(if $(MOD),$(MOD),) --dry-run

rollback: ## Rollback a mod to a previous version (Usage: make rollback MOD=<name> VERSION=<ver>)
	+@bash ./scripts/rollback-mod.sh $(MOD) $(VERSION) $(MAKE_DRY_RUN)

rollback-dry-run: ## Preview rollback of a mod (Usage: make rollback-dry-run MOD=<name> VERSION=<ver>)
	@bash ./scripts/rollback-mod.sh $(MOD) $(VERSION) --dry-run

##@ Repository Maintenance
tidy: ## Prune unreferenced lock entries, vendor cache, symlinks, and run go mod tidy
	+@bash ./scripts/tidy.sh $(MAKE_DRY_RUN)

tidy-dry-run: ## Preview cleanup actions without deleting files
	@bash ./scripts/tidy.sh --dry-run

##@ Mod Load Order
build-load-order: ## Compile the Go load order resolver into bin/load-order
	@mkdir -p bin
	@go -C tools/load-order build -o ../../bin/load-order .

build-order-mods: build-load-order ## Alias for build-load-order

order-mods: ## Resolve and sort active mods into valid load order
	+@bash ./scripts/order-mods.sh $(MAKE_DRY_RUN)

order-mods-dry-run: ## Preview computed mod load order without modifying files
	@bash ./scripts/order-mods.sh --dry-run

sync-config: ## Generate and deploy ModsConfig.xml to RimWorld config directory
	+@bash ./scripts/order-mods.sh --write-config $(MAKE_DRY_RUN)

sync-config-dry-run: ## Preview ModsConfig.xml generation without modifying filesystem
	@bash ./scripts/order-mods.sh --write-config --dry-run

##@ Remote Synchronization
import-deck: ## Interactive TUI to cherry-pick Steam Workshop mods from Steam Deck
	@bash ./scripts/import-deck.sh $(MAKE_DRY_RUN) $(if $(COPY),--copy-files,)

import-deck-dry-run: ## Preview cherry-picking Steam Workshop mods without writing to mods.yaml
	@bash ./scripts/import-deck.sh --dry-run

import-deck-mods: import-deck ## Alias for import-deck
import-deck-mods-dry-run: import-deck-dry-run ## Alias for import-deck-dry-run

sync-deck: ## Sync mods, deploy symlinks, and update ModsConfig.xml on Steam Deck
	+@bash ./scripts/sync-deck.sh $(MAKE_DRY_RUN)

sync-deck-dry-run: ## Preview sync, symlink, and ModsConfig.xml deployment on Steam Deck
	@bash ./scripts/sync-deck.sh --dry-run

sync-deck-check: ## Test SSH connectivity to Steam Deck
	@bash ./scripts/sync-deck.sh --check

##@ Private Mod Development
scaffold-mod: ## Scaffold a private mod (Usage: make scaffold-mod MOD=<name> [TYPE=xml|csharp])
	+@bash ./scripts/scaffold-mod.sh $(if $(MOD),--name $(MOD),) $(if $(TYPE),--type $(TYPE),) $(MAKE_DRY_RUN)

scaffold-mod-dry-run: ## Preview scaffolding a private mod (Usage: make scaffold-mod-dry-run MOD=<name> [TYPE=xml|csharp])
	@bash ./scripts/scaffold-mod.sh $(if $(MOD),--name $(MOD),) $(if $(TYPE),--type $(TYPE),) --dry-run

build-mod: ## Compile C# assemblies for a custom mod (Usage: make build-mod MOD=<name>)
	+@bash ./scripts/build-mod.sh $(if $(MOD),--mod $(MOD),) $(MAKE_DRY_RUN)

build-mod-dry-run: ## Preview compiling C# assemblies (Usage: make build-mod-dry-run MOD=<name>)
	@bash ./scripts/build-mod.sh $(if $(MOD),--mod $(MOD),) --dry-run
