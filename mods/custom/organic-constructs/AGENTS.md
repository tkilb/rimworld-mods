# AGENTS.md — Organic Constructs (`tyler.organicconstructs`)

## Mod Overview

**Organic Constructs** is a RimWorld Biotech expansion mod introducing mass-produced, 100% biological artificial humans (SecUnits / Replicants).
- **Core Concept:** Unlike mechanoids or androids, constructs are fully biological vat-grown organisms consisting of synthetic muscle, organs, and wetware.
- **Key Distinctions:**
  - **Biological Immunity:** Immune to EMPs, solar flares, and mechanoid-specific hacking.
  - **Colony Property:** Treated as industrial assets; natural colonists suffer 0 mood debuffs when a construct dies or is lost (`Trait_ConstructAsset`).
  - **Suppressed Psychology:** Suppressed romance, marriage, chit-chat, and socialization (`Gene_ConstructPsychology`).
  - **Locked Architecture:** Harmony patch explicitly disables `Recipe_ImplantXenogerm` on constructs.
  - **Maintenance Stasis:** Requires periodic 12-hour hibernation every 30 days (`Gene_ConstructHibernation`).
  - **Bioware Augments:** Exclusive biological augment packages (Combat, Medical, Industrial, Laborer) that induce an assimilation coma upon grafting; rejected by natural humans.

---

## Core Mechanics & Subsystems

1. **Embryo Synthesis & Batch Printing:**
   - [Building_ConstructSynthesizer.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_ConstructSynthesizer.cs): Custom workbench with a single-slot container for a [GenomeBlueprintDisk](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml).
   - [Recipe_SynthesizeBlankEmbryo.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_SynthesizeBlankEmbryo.cs): Crafts blank parentless embryos marked with `isConstruct = true` via [CompEmbryoQuality.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompEmbryoQuality.cs).
   - [Recipe_BatchApplyBlueprint.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_BatchApplyBlueprint.cs): Copies genes from the loaded master blueprint disc onto blank embryos without consuming the disc.
   - [Recipe_BatchRecycleEmbryo.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_BatchRecycleEmbryo.cs): Recycles discarded embryos into Genetic Nutrient Paste.

2. **Neural Scanning & Vat Imprinting:**
   - [Building_NeuralScanner.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_NeuralScanner.cs): Casket building that scans a natural human pawn onto a [NeuralBlueprintDisk](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml). Constructs are blocked from entering. Donor suffers `Construct_NeuralFatigue`.
   - [CompGrowthVatImprinter.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompGrowthVatImprinter.cs): Injected into vanilla `GrowthVat` via [Patch_GrowthVat.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Patches/Patch_GrowthVat.xml) to hold a mentor disc.
   - [Patch_GrowthVatDecant.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_GrowthVatDecant.cs): Intercepts gestation decanting:
     - With mentor disc: 50% of donor skills, all passions zeroed.
     - Without disc: Baseline reflexes (Shooting 4, Melee 4, Social 2, Intellectual 2, Artistic 0, Others 3; 0 passions).

3. **Construct Biology & Social Patches:**
   - [ConstructUtility.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/ConstructUtility.cs): Central checker `IsConstruct(Pawn)` and `IsConstructEmbryo(HumanEmbryo)`.
   - [Patch_RecipeImplantXenogerm.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_RecipeImplantXenogerm.cs): Blocks xenogerm implantation on constructs.
   - [Patch_SocialAndRomance.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_SocialAndRomance.cs): Nullifies romance, marriage, and interaction weights.
   - [Patch_ColonistDeathThoughts.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_ColonistDeathThoughts.cs): Suppresses colonist grief thoughts on construct death.
   - [Gene_ConstructHibernation.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Genes/Gene_ConstructHibernation.cs): Manages 30-day operation cycles and 12-hour stasis hibernation.

4. **Bioware Augments:**
   - [Recipe_InstallConstructPackage.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_InstallConstructPackage.cs): Surgical recipes restricted to constructs; rejects natural humans. Causes `Construct_Assimilation` coma.
   - [Recipe_RemoveConstructPackage.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_RemoveConstructPackage.cs): Augment extraction with doctor-skill-based recovery chance.

---

## Key File Index

### C# Source Files (`Source/`)
| Path | Purpose |
| :--- | :--- |
| [ConstructUtility.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/ConstructUtility.cs) | **Primary entry point** for checking construct identity (`IsConstruct`, `IsConstructEmbryo`). |
| [OrganicConstructsMod.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/OrganicConstructsMod.cs) | Mod initialization and Harmony patch registration (`tyler.organicconstructs`). |
| [DefOfs/ConstructDefOf.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/DefOfs/ConstructDefOf.cs) | Core DefOf references (genes, buildings, jobs, hediffs). |
| [DefOfs/ConstructAugmentDefOf.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/DefOfs/ConstructAugmentDefOf.cs) | DefOf references for bioware augment hediffs and items. |
| [DefOfs/NeuralImprintDefOf.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/DefOfs/NeuralImprintDefOf.cs) | DefOf references for neural blueprint items and scanning jobs. |
| [Buildings/Building_ConstructSynthesizer.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_ConstructSynthesizer.cs) | Workstation holding genome blueprint disc container and managing synthesis bills. |
| [Buildings/Building_NeuralScanner.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_NeuralScanner.cs) | Biometric casket scanning pawn proficiencies onto neural discs. |
| [Comps/CompGenomeBlueprint.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompGenomeBlueprint.cs) | Comp on genome discs storing serializable list of `GeneDef`s. |
| [Comps/CompNeuralBlueprint.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompNeuralBlueprint.cs) | Comp on neural discs storing skill levels and mentor details. |
| [Comps/CompGrowthVatImprinter.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompGrowthVatImprinter.cs) | Comp on Growth Vats handling single-slot neural disc storage. |
| [Comps/CompEmbryoQuality.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompEmbryoQuality.cs) | Comp attached to human embryos tracking construct matrix status. |
| [Patches/Patch_GrowthVatDecant.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_GrowthVatDecant.cs) | Harmony patch on `Building_GrowthVat.FinishGestation` applying skills/passions. |
| [Patches/Patch_RecipeImplantXenogerm.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_RecipeImplantXenogerm.cs) | Harmony postfix blocking xenogerm surgery on constructs. |
| [Patches/Patch_SocialAndRomance.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_SocialAndRomance.cs) | Harmony patches eliminating construct romance and interaction weights. |
| [Patches/Patch_ColonistDeathThoughts.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_ColonistDeathThoughts.cs) | Suppresses colonist grief memories when a construct dies. |
| [Genes/Gene_ConstructHibernation.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Genes/Gene_ConstructHibernation.cs) | Ticking gene tracking operation hours, warning stages, and stasis jobs. |
| [Genes/Gene_ConstructPsychology.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Genes/Gene_ConstructPsychology.cs) | Construct psychology gene logic. |

### XML Definitions (`Defs/` & `Patches/`)
| Path | Purpose |
| :--- | :--- |
| [Buildings_OrganicConstructs.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Buildings/Buildings_OrganicConstructs.xml) | `ConstructSynthesizer` and `NeuralScanner` building Defs. |
| [Items_Blueprints.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml) | `GenomeBlueprintDisk` and `NeuralBlueprintDisk` item Defs. |
| [Items_ConstructAugments.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_ConstructAugments.xml) | Bioware augment item Defs (Basic, Intermediate, Advanced packages). |
| [Items_Biomass.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Biomass.xml) | `GeneticNutrientPaste` item Def. |
| [Genes_Construct.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/GeneDefs/Genes_Construct.xml) | `Gene_ConstructPsychology`, `Gene_ConstructHibernation`, `Gene_MandatorySterility`. |
| [Traits_Construct.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/TraitDefs/Traits_Construct.xml) | `Trait_ConstructAsset` Def. |
| [Hediffs_ConstructAugments.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/HediffDefs/Hediffs_ConstructAugments.xml) | Augment hediffs and `Construct_Assimilation` coma. |
| [Hediffs_Neural.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/HediffDefs/Hediffs_Neural.xml) | `Construct_NeuralFatigue` and `Construct_InterruptedStasis` hediffs. |
| [Recipes_ConstructBatch.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_ConstructBatch.xml) | Synthesizer bill recipes (synthesize blank, batch imprint, liquefy). |
| [Recipes_ConstructAugments.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_ConstructAugments.xml) | Surgical recipes to install/remove bioware packages. |
| [Patch_GrowthVat.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Patches/Patch_GrowthVat.xml) | Patches vanilla `GrowthVat` to attach `CompGrowthVatImprinter`. |
| [Patch_EmbryoConstructComp.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Patches/Patch_EmbryoConstructComp.xml) | Patches vanilla `HumanEmbryo` to attach `CompEmbryoQuality`. |

### Specifications & QA
| Path | Purpose |
| :--- | :--- |
| [spec.md](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/spec.md) | Full technical specification and system mechanics. |
| [qa/qa.md](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/qa/qa.md) | QA test case index and build verification log. |
| [qa/AGENTS.md](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/qa/AGENTS.md) | QA authoring guidelines and test case formatting schema. |
| [qa/bugs/AGENTS.md](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/qa/bugs/AGENTS.md) | Bug fixing workflow, triaging rules, and resolution procedures. |

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
