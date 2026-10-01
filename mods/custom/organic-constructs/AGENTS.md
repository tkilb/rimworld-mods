# AGENTS.md — Organic Constructs (`tyler.organicconstructs`)

## Mod Overview

**Organic Constructs** is a RimWorld Biotech expansion mod introducing mass-produced, 100% biological artificial humans (Base and Complex Constructs).
- **Core Concept:** Fully biological vat-grown organisms consisting of synthetic muscle, organs, and wetware.
- **Key Distinctions:**
  - **Biological Immunity:** Immune to EMPs, solar flares, and mechanoid-specific hacking.
  - **Colony Property:** Treated as industrial assets; natural colonists suffer 0 mood debuffs when a construct dies or is lost (`Trait_ConstructAsset`).
  - **Suppressed Psychology:** Suppressed romance, marriage, chit-chat, and socialization (`Gene_ConstructPsychology`).
  - **Locked Architecture:** Harmony patch explicitly disables `Recipe_ImplantXenogerm` on constructs.
  - **Maintenance Stasis:** Requires periodic 12-hour hibernation every 30 days (`Gene_ConstructHibernation`).
  - **Bioware Augments:** Exclusive biological augment packages (Combat, Medical, Industrial, Laborer) that induce an assimilation coma upon grafting; rejected by natural humans.

---

## Architecture & Subsystems

1. **Genome Architecture & Stability Engine:**
   - [Building_ConstructGenomeArchitect.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_ConstructGenomeArchitect.cs): Workstation that scans nearby powered `GeneBank`s within 16 cells and opens the configuration window.
   - [Dialog_ConfigureConstructGenome.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/UI/Dialog_ConfigureConstructGenome.cs): UI for selecting adaptations alongside locked Core Genes, calculating stability, and burning blank discs.
   - [ConstructStabilityUtility.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/ConstructStabilityUtility.cs): Calculates template stability from complexity, metabolism, and optimizer genes.
   - [CompGenomeBlueprint.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompGenomeBlueprint.cs): Tracks disc state (`IsBlank`, `isBurned`, `templateLabel`, `genes`). Burned discs are ROM-locked.

2. **Embryo Synthesis:**
   - [Building_ConstructSynthesizer.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_ConstructSynthesizer.cs): Synthesis bench with a single-slot container for a master `GenomeBlueprintDisk`.
   - [Recipe_SynthesizeBaseEmbryo.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_SynthesizeBaseEmbryo.cs): 1-step direct synthesis of standard Base Construct embryos. Requires no disc.
   - [Recipe_SynthesizeComplexEmbryo.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_SynthesizeComplexEmbryo.cs): 1-step direct synthesis of Complex Construct embryos carrying the loaded disc's genome. Factored by stability collapse roll (yielding `GeneticNutrientPaste` on botch).
   - [Patch_WorkGiver_DoBill.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_WorkGiver_DoBill.cs): Blocks dispatching Complex Embryo bills if no disc is loaded.
   - [Recipe_BatchRecycleEmbryo.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_BatchRecycleEmbryo.cs): Recycles discarded embryos into Genetic Nutrient Paste.

3. **Neural Scanning & Vat Imprinting:**
   - [Building_NeuralScanner.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_NeuralScanner.cs): Casket scanning natural humans onto a [NeuralBlueprintDisk](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml) (50% skills, 0 passions, lossy). Constructs cannot enter. Donor suffers `Construct_NeuralFatigue`.
   - [CompGrowthVatImprinter.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompGrowthVatImprinter.cs): Injected into vanilla `GrowthVat` to hold a mentor disc.
   - [Patch_GrowthVatDecant.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_GrowthVatDecant.cs): Decanting applies disc skills 1:1, or baseline reflexes (3-4 skills) if blank. Rolls 15% defect chance on volatile templates (<60% stability).

4. **Bioware Augments:**
   - [Recipe_InstallConstructPackage.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_InstallConstructPackage.cs): Surgical recipes restricted to constructs; causes `Construct_Assimilation` coma. Rejects natural humans.
   - [Recipe_RemoveConstructPackage.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_RemoveConstructPackage.cs): Augment extraction with doctor-skill recovery chance.

---

## Key File Index

### C# Source Files (`Source/`)
| Path | Purpose |
| :--- | :--- |
| [ConstructUtility.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/ConstructUtility.cs) | **Primary entry point** for checking construct identity (`IsConstruct`, `IsConstructEmbryo`). |
| [ConstructStabilityUtility.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/ConstructStabilityUtility.cs) | Calculates genome stability and ratings based on cpx/metabolism. |
| [OrganicConstructsMod.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/OrganicConstructsMod.cs) | Mod initialization and Harmony patch registration (`tyler.organicconstructs`). |
| [DefOfs/ConstructDefOf.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/DefOfs/ConstructDefOf.cs) | Core DefOf references (genes, buildings, jobs, hediffs). |
| [DefOfs/ConstructAugmentDefOf.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/DefOfs/ConstructAugmentDefOf.cs) | Bioware augment hediffs and item DefOfs. |
| [DefOfs/NeuralImprintDefOf.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/DefOfs/NeuralImprintDefOf.cs) | Neural blueprint and scanning DefOfs. |
| [Buildings/Building_ConstructGenomeArchitect.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_ConstructGenomeArchitect.cs) | Workstation for configuring and burning blank genome blueprint discs. |
| [Buildings/Building_ConstructSynthesizer.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_ConstructSynthesizer.cs) | Synthesis workstation holding master genome disc container. |
| [Buildings/Building_NeuralScanner.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_NeuralScanner.cs) | Biometric casket scanning human proficiencies onto neural discs. |
| [Comps/CompGenomeBlueprint.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompGenomeBlueprint.cs) | Comp on genome discs storing ROM state and encoded `GeneDef`s. |
| [Comps/CompNeuralBlueprint.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompNeuralBlueprint.cs) | Comp on neural discs storing skill levels and mentor details. |
| [Comps/CompGrowthVatImprinter.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompGrowthVatImprinter.cs) | Comp on Growth Vats handling single-slot neural disc storage. |
| [Comps/CompEmbryoQuality.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompEmbryoQuality.cs) | Comp attached to human embryos tracking construct status. |
| [UI/Dialog_ConfigureConstructGenome.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/UI/Dialog_ConfigureConstructGenome.cs) | IMGUI genome design window for burning blank master discs. |
| [Recipes/Recipe_SynthesizeBaseEmbryo.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_SynthesizeBaseEmbryo.cs) | Direct 1-step synthesis of standard Base Construct embryos. |
| [Recipes/Recipe_SynthesizeComplexEmbryo.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_SynthesizeComplexEmbryo.cs) | Direct 1-step synthesis of Complex Construct embryos with stability roll. |
| [Patches/Patch_WorkGiver_DoBill.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_WorkGiver_DoBill.cs) | Harmony postfix blocking Complex Embryo bills if no disc is loaded. |
| [Patches/Patch_GrowthVatDecant.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_GrowthVatDecant.cs) | Harmony patch applying skills and volatile decant defects. |
| [Patches/Patch_RecipeImplantXenogerm.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_RecipeImplantXenogerm.cs) | Harmony postfix blocking xenogerm surgery on constructs. |
| [Patches/Patch_SocialAndRomance.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_SocialAndRomance.cs) | Harmony patches eliminating construct romance and interaction weights. |
| [Patches/Patch_ColonistDeathThoughts.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_ColonistDeathThoughts.cs) | Suppresses colonist grief memories when a construct dies. |
| [Genes/Gene_ConstructHibernation.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Genes/Gene_ConstructHibernation.cs) | Ticking gene tracking 30-day operation cycles and stasis jobs. |
| [Genes/Gene_ConstructPsychology.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Genes/Gene_ConstructPsychology.cs) | Suppressed socialization and uniform construct physiology. |

### XML Definitions (`Defs/` & `Patches/`)
| Path | Purpose |
| :--- | :--- |
| [Buildings_OrganicConstructs.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Buildings/Buildings_OrganicConstructs.xml) | `ConstructSynthesizer` and `NeuralScanner` building Defs. |
| [Buildings_GenomeArchitect.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Buildings/Buildings_GenomeArchitect.xml) | `ConstructGenomeArchitect` building Def. |
| [Items_Blueprints.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml) | `GenomeBlueprintDisk` and `NeuralBlueprintDisk` item Defs. |
| [Items_ConstructAugments.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_ConstructAugments.xml) | Bioware augment item Defs. |
| [Items_Biomass.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Biomass.xml) | `GeneticNutrientPaste` item Def. |
| [Genes_Construct.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/GeneDefs/Genes_Construct.xml) | Core construct genes (`Gene_ConstructPsychology`, `Gene_ConstructHibernation`, etc.). |
| [Traits_Construct.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/TraitDefs/Traits_Construct.xml) | `Trait_ConstructAsset` Def. |
| [Hediffs_ConstructAugments.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/HediffDefs/Hediffs_ConstructAugments.xml) | Augment hediffs and `Construct_Assimilation` coma. |
| [Hediffs_Neural.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/HediffDefs/Hediffs_Neural.xml) | `Construct_NeuralFatigue` and `Construct_InterruptedStasis` hediffs. |
| [Recipes_ConstructBatch.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_ConstructBatch.xml) | Synthesizer bill recipes (Base Fresh/Recycled, Complex Fresh/Recycled, Liquefy). |
| [Recipes_Crafting.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_Crafting.xml) | `Craft_BlankGenomeDisc` recipe at Fabrication Table. |
| [Recipes_ConstructAugments.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_ConstructAugments.xml) | Surgical recipes to install/remove bioware packages. |
| [Patch_GrowthVat.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Patches/Patch_GrowthVat.xml) | Patches vanilla `GrowthVat` to attach `CompGrowthVatImprinter`. |
| [Patch_EmbryoConstructComp.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Patches/Patch_EmbryoConstructComp.xml) | Patches vanilla `HumanEmbryo` to attach `CompEmbryoQuality`. |

---

## Bug Fixing Workflow ("fix bugs")

When instructed to **"fix bugs"**, follow [qa/bugs/AGENTS.md](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/qa/bugs/AGENTS.md):
1. Check open `bug-*.md` reports directly in `qa/bugs/` (strictly ignore `closed-bugs/` and `BUG_TEMPLATE.md`).
2. Address bugs sequentially one at a time.
3. Diagnose and fix using scoped tools (`replace_file_content`, `write_to_file`).
4. Verify fixes with `dotnet build Source/OrganicConstructs.csproj` (0 errors, 0 warnings).
5. Mark `**Status:** Ready for QA` and complete the fix notes in the bug report.

---

## Build & Verification

```bash
dotnet build Source/OrganicConstructs.csproj
```
- **Target Framework:** `.NET Framework 4.7.2`
- **Output:** `Assemblies/OrganicConstructs.dll`
- **Standard:** 0 Errors, 0 Warnings required.

---

## Development Guidelines for Agents

1. **Construct Detection:** Always use [`ConstructUtility.IsConstruct(pawn)`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/ConstructUtility.cs#L8-L40) or [`ConstructUtility.IsConstructEmbryo(embryo)`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/ConstructUtility.cs#L42-L65) rather than directly inspecting trait or gene lists across multiple files.
2. **Tunable Numbers:** Expose gameplay balance numbers in XML via `CompProperties` or `DefModExtension`; avoid hardcoding game values in C#.
3. **Scoped Tools:** Prefer `replace_file_content` and `write_to_file` over shell commands for inspecting and editing code.
4. **Git Hygiene:** Never run `git commit` or suggest committing changes; commits are handled manually by the user.
