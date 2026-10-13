# PLAN_REMOVE_DEPRECATED_1.md — Hard Removal of Deprecated Genetic Nutrient Paste

## Context & Background

During early development, the mod retained `GeneticNutrientPaste` as a biomass item. When that mechanic was superseded, existing save games still held serialized references to `GeneticNutrientPaste` inside:
* Colony stockpile zones (`Zone_Stockpile`)
* Storage buildings and shelf settings (`Building_Storage` / `StorageGroup`)
* Outfit policies (`ApparelPolicy`)
* Map item instances or pawn inventories

To prevent startup/load crashes and missing Def errors (`Could not load reference to Verse.ThingDef named GeneticNutrientPaste`), a two-stage deprecation process was implemented:

1. **Stage 1 (Current State — Soft Deprecation & Auto-Migration):**
   * [`Defs/ThingDefs_Items/Items_Deprecated.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Deprecated.xml): Defines a placeholder `GeneticNutrientPaste` with `<tradeability>None</tradeability>`, `<destroyOnDrop>true</destroyOnDrop>`, and empty `<thingCategories />`.
   * [`Source/Comps/GameComponent_ConstructMigration.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/GameComponent_ConstructMigration.cs): On `LoadedGame()`, automatically converts physical `GeneticNutrientPaste` stacks into vanilla `MealNutrientPaste`, and purges `GeneticNutrientPaste` from all storage, zone, and outfit filters. When the player saves their game, the resulting `.rws` save file is 100% clean of all `GeneticNutrientPaste` references.

2. **Stage 2 (This Plan — Hard Removal):**
   * Completely purge the deprecated placeholder Def and migration GameComponent once players have had sufficient version cycles to load and resave their games.

---

## Prerequisites Before Execution

Before executing this plan, ensure:
- [ ] At least one major release cycle or playthrough has occurred where saves have been loaded and resaved under Stage 1.
- [ ] No active gameplay systems in `mods/custom/organic-constructs` reference `GeneticNutrientPaste` (already verified clean).

---

## Execution Steps for Agent

### Step 1: Delete Deprecated XML Def
Remove the placeholder Def file:
```bash
rm mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Deprecated.xml
```

### Step 2: Delete Migration GameComponent
Remove the migration component C# source file:
```bash
rm mods/custom/organic-constructs/Source/Comps/GameComponent_ConstructMigration.cs
```

### Step 3: Verify No Lingering References
Run a repository search to ensure no leftover references exist in the mod:
```bash
rg "GeneticNutrientPaste|GameComponent_ConstructMigration" mods/custom/organic-constructs/
```
*Expected result:* 0 matches (or only matches within this plan file prior to its deletion).

### Step 4: Compile & Build Verification
Compile the C# assembly using dotnet build:
```bash
dotnet build mods/custom/organic-constructs/Source/OrganicConstructs.csproj
```
*Standard:* Must compile cleanly with **0 Warnings** and **0 Errors**.

### Step 5: Clean Up This Plan File
Once verified, delete this plan file:
```bash
rm mods/custom/organic-constructs/PLAN_REMOVE_DEPRECATED_1.md
```
