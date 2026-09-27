SHELL := /usr/bin/env bash
.DEFAULT_GOAL := help

.PHONY: help check-deps status fetch-mods update-mods update-mods-dry-run \
        link link-dry-run unlink unlink-dry-run rollback rollback-dry-run \
        build-load-order build-order-mods order-mods order-mods-dry-run \
        sync-config sync-config-dry-run \
        sync-deck sync-deck-dry-run sync-deck-check sync-deck-link \
        sync-deck-config sync-deck-config-dry-run \
        scaffold-mod scaffold-mod-dry-run build-mod build-mod-dry-run

##@ General
help: ## Display this help message
	@echo "RimWorld Monorepo Task Runner"
	@echo "=============================="
	@awk 'BEGIN {FS = ":.*##"; printf "\nUsage:\n  make \033[36m<target>\033[0m\n"} /^[a-zA-Z_-]+:.*?##/ { printf "  \033[36m%-22s\033[0m %s\n", $$1, $$2 } /^##@/ { printf "\n\033[1m%s\033[0m\n", substr($$0, 5) } ' $(MAKEFILE_LIST)

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

link: ## Deploy idempotent symlinks into RimWorld Mods directory
	+@bash ./scripts/link-mods.sh --link $(MAKE_DRY_RUN)

link-dry-run: ## Preview symlink deployment without modifying filesystem
	@bash ./scripts/link-mods.sh --link --dry-run

unlink: ## Remove monorepo symlinks from RimWorld Mods directory
	+@bash ./scripts/link-mods.sh --unlink $(MAKE_DRY_RUN)

unlink-dry-run: ## Preview removal of monorepo symlinks
	@bash ./scripts/link-mods.sh --unlink --dry-run

rollback: ## Rollback a mod to a previous version (Usage: make rollback MOD=<name> VERSION=<ver>)
	+@bash ./scripts/rollback-mod.sh $(MOD) $(VERSION) $(MAKE_DRY_RUN)

rollback-dry-run: ## Preview rollback of a mod (Usage: make rollback-dry-run MOD=<name> VERSION=<ver>)
	@bash ./scripts/rollback-mod.sh $(MOD) $(VERSION) --dry-run

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
sync-deck: ## Sync mods and lockfile to Steam Deck via SSH/rsync
	+@bash ./scripts/sync-deck.sh $(MAKE_DRY_RUN)

sync-deck-dry-run: ## Preview rsync transfer to Steam Deck
	@bash ./scripts/sync-deck.sh --dry-run

sync-deck-check: ## Test SSH connectivity to Steam Deck
	@bash ./scripts/sync-deck.sh --check

sync-deck-link: ## Sync to Steam Deck and deploy symlinks remotely
	+@bash ./scripts/sync-deck.sh --link $(MAKE_DRY_RUN)

sync-deck-config: ## Sync to Steam Deck and deploy ModsConfig.xml remotely
	+@bash ./scripts/sync-deck.sh --config $(MAKE_DRY_RUN)

sync-deck-config-dry-run: ## Preview remote ModsConfig.xml deployment on Steam Deck
	@bash ./scripts/sync-deck.sh --config --dry-run

##@ Private Mod Development
scaffold-mod: ## Scaffold a private mod (Usage: make scaffold-mod MOD=<name> [TYPE=xml|csharp])
	+@bash ./scripts/scaffold-mod.sh $(if $(MOD),--name $(MOD),) $(if $(TYPE),--type $(TYPE),) $(MAKE_DRY_RUN)

scaffold-mod-dry-run: ## Preview scaffolding a private mod (Usage: make scaffold-mod-dry-run MOD=<name> [TYPE=xml|csharp])
	@bash ./scripts/scaffold-mod.sh $(if $(MOD),--name $(MOD),) $(if $(TYPE),--type $(TYPE),) --dry-run

build-mod: ## Compile C# assemblies for a custom mod (Usage: make build-mod MOD=<name>)
	+@bash ./scripts/build-mod.sh $(if $(MOD),--mod $(MOD),) $(MAKE_DRY_RUN)

build-mod-dry-run: ## Preview compiling C# assemblies (Usage: make build-mod-dry-run MOD=<name>)
	@bash ./scripts/build-mod.sh $(if $(MOD),--mod $(MOD),) --dry-run
