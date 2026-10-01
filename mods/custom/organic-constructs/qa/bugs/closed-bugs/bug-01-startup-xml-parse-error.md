# Bug: Startup XML parse error in Items_Blueprints.xml and unresolved TableFabrication

**Status:** Passed QA

## Description

At game startup, RimWorld threw an `XmlException: An error occurred while parsing EntityName. Line 7, position 121` loading `Items_Blueprints.xml`, followed by cross-reference failures for `TableFabrication`, `GenomeBlueprintDisk`, and `NeuralBlueprintDisk`.

## Steps to Reproduce (Optional)

1. Launch RimWorld with the mod enabled.
2. Observe error logs during def loading.

## Logs / Errors (Optional)

```text
Exception reading Items_Blueprints.xml as XML: System.Xml.XmlException: An error occurred while parsing EntityName. Line 7, position 121.
[Ref 4D5843EB]
/mnt/gaming/SteamLibrary/steamapps/common/RimWorld/Mods/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml: unknown parse failure
Could not resolve cross-reference to Verse.ThingDef named TableFabrication (wanter=recipeUsers)
Could not resolve cross-reference: No Verse.ThingDef named GenomeBlueprintDisk found to give to Verse.ThingDefCountClass (1x null)
Failed to find Verse.ThingDef named NeuralBlueprintDisk. There are 2022 defs of this type loaded.
Failed to find Verse.ThingDef named GenomeBlueprintDisk. There are 2022 defs of this type loaded.
```

---

## Dev Notes / Fix (Optional)

- **Cause:**
  1. In [Items_Blueprints.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml), lines 7 and 40 contained unescaped ampersands (`Base & Complex Constructs` and `Base & Complex Construct gestation`). XML parser expects `&amp;`, causing `Items_Blueprints.xml` to fail to parse. Consequently, `GenomeBlueprintDisk` and `NeuralBlueprintDisk` defs were never registered.
  2. In [Recipes_Crafting.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_Crafting.xml), line 13 specified `TableFabrication` under `<recipeUsers>`. In vanilla RimWorld, the fabrication bench defName is `FabricationBench`.
  3. The downstream errors (`Could not resolve cross-reference: No Verse.ThingDef named GenomeBlueprintDisk...`) were cascading failures caused by `Items_Blueprints.xml` failing to parse.
- **Fix:**
  - Replaced `&` with `&amp;` in both item descriptions in [Items_Blueprints.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml).
  - Updated `<recipeUsers><li>TableFabrication</li></recipeUsers>` to `<recipeUsers><li>FabricationBench</li></recipeUsers>` in [Recipes_Crafting.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_Crafting.xml).
  - Verified with python XML validation across all repository XML files (all valid).
  - Verified `dotnet build Source/OrganicConstructs.csproj` (0 errors, 0 warnings).
