SHELL := /usr/bin/env bash
.DEFAULT_GOAL := help

.PHONY: help check-deps status fetch-mods update-mods link unlink sync-deck rollback

##@ General
help: ## Display this help message
	@echo "RimWorld Monorepo Task Runner"
	@echo "=============================="
	@awk 'BEGIN {FS = ":.*##"; printf "\nUsage:\n  make \033[36m<target>\033[0m\n"} /^[a-zA-Z_-]+:.*?##/ { printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2 } /^##@/ { printf "\n\033[1m%s\033[0m\n", substr($$0, 5) } ' $(MAKEFILE_LIST)

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

link: ## Create idempotent symlinks to RimWorld Mods directory
	@bash ./scripts/link-mods.sh --link

unlink: ## Remove symlinks from RimWorld Mods directory
	@bash ./scripts/link-mods.sh --unlink

rollback: ## Rollback a mod to a previous version (Usage: make rollback MOD=<name> VERSION=<ver>)
	@bash ./scripts/rollback-mod.sh $(MOD) $(VERSION)

##@ Remote Synchronization
sync-deck: ## Sync mods and lockfile to Steam Deck target via SSH/rsync
	@bash ./scripts/sync-deck.sh
