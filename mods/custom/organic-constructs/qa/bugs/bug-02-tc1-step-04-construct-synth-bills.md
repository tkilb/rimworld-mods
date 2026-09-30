# Bug: Bills do not show on the construct synthesizer

**Status:** Ready for QA

## Description

Bills do not show on the construct synthesizer

## Steps to Reproduce (Optional)

1. Follow TC-01, Step 4 & 5

## Logs / Errors (Optional)

```text

```

---

## Dev Notes / Fix (Optional)

- **Cause:**
  1. `ConstructSynthesizer` in `Buildings_OrganicConstructs.xml` was initially missing `<inspectorTabs><li>ITab_Bills</li></inspectorTabs>`.
  2. In `Recipes_ConstructBatch.xml`, all four workbench recipes (`Construct_BatchApplyBlueprint`, `Recipe_SynthesizeBlankEmbryoFresh`, `Recipe_SynthesizeBlankEmbryoRecycled`, and `Construct_BatchRecycleEmbryo`) specified `<researchPrerequisite>Construct_Foundations</researchPrerequisite>`. In RimWorld's `ITab_Bills`, `<FillTab>g__OptionsMaker` checks `recipe.AvailableNow`. When `Construct_Foundations` is unresearched (e.g. during TC-01 testing where the synthesizer is spawned/built), `recipe.AvailableNow` evaluated to `false`, causing all 4 recipes to be filtered out of the "+ Add bill" dropdown. The building itself is already gated by `Construct_Foundations`, making the recipe-level prerequisite redundant and problematic.
  3. `Recipe_BatchApplyBlueprint` had `AvailableOnNow` checking `bench.HasLoadedBlueprint`. This prevented the blueprint splicing bill from ever showing up in "+ Add bill" unless a disc was already physically inside the synthesizer receptacle.
- **Fix:**
  - Added `<inspectorTabs><li>ITab_Bills</li></inspectorTabs>`, `<surfaceType>Item</surfaceType>`, and `<building><spawnedConceptLearnOpportunity>BillsTab</spawnedConceptLearnOpportunity></building>` to `ConstructSynthesizer` in `Defs/ThingDefs_Buildings/Buildings_OrganicConstructs.xml`.
  - Removed redundant `<researchPrerequisite>Construct_Foundations</researchPrerequisite>` from all four recipes in `Defs/RecipeDefs/Recipes_ConstructBatch.xml`.
  - Updated `Recipe_BatchApplyBlueprint.AvailableOnNow` and `AvailableReport` to allow the bill to be queued on `Building_ConstructSynthesizer` at any time, with a safety message and embryo drop guard in `Notify_IterationCompleted` if completed without a loaded disc.

## Reopen Notes (Optional)

Bills button exists, but no bill options show

### Expected:

4. Bill: Synthesize blank embryo (fresh organics).
5. Bill: Batch imprint construct caste using loaded blueprint.
