# Bug: Duplicate bill options on Construct Synthesizer

**Status:** Ready for QA

## Description

When clicking "+ Add bill" on the Construct Synthesizer, each bill option appeared twice in the dropdown menu.

## Steps to Reproduce (Optional)

1. Build or spawn a `ConstructSynthesizer`.
2. Select it and click the **Bills** tab.
3. Click **+ Add bill**.
4. Observe duplicate recipe entries.

## Logs / Errors (Optional)

```text
Two duplicate sets of recipes listed in the Bills dropdown.
```

---

## Dev Notes / Fix (Optional)

- **Cause:**
  In [Buildings_OrganicConstructs.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Buildings/Buildings_OrganicConstructs.xml), `ConstructSynthesizer` declared `<recipes>` with the 5 synthesizer recipes. Simultaneously, all 5 recipes in [Recipes_ConstructBatch.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_ConstructBatch.xml) declared `<recipeUsers><li>ConstructSynthesizer</li></recipeUsers>`. During def reference resolution, RimWorld adds recipes from both sources without deduplication, resulting in each bill option appearing twice in `ThingDef.AllRecipes`.
- **Fix:**
  - Removed the redundant `<recipes>` block from `ConstructSynthesizer` in [Buildings_OrganicConstructs.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Buildings/Buildings_OrganicConstructs.xml), relying solely on standard `<recipeUsers>` registration from [Recipes_ConstructBatch.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_ConstructBatch.xml).
  - Verified XML syntax and build compilation (`dotnet build Source/OrganicConstructs.csproj` passed with 0 errors and 0 warnings).
