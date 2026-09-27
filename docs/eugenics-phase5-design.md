# Eugenics Program - Phase 5 Batch Automation & Workbench Architecture Design

## 1. Context & Problem Statement

In **Sub-Tasks 6.2.1 through 6.2.4**, the core gene splicing, blueprint discs, neural imprinting, and embryo quality mechanics were implemented. Splicing individual embryos currently relies on an inspect gizmo (`CompEmbryoQuality` -> `Dialog_EditEmbryoGenes`).

For **Task 6.2.5 (Batch Automation & Ecosystem Validation)**, the objective is to allow automated, mass-scale construct production without requiring manual player interaction for every single embryo.

Vanilla RimWorld's `Building_GeneAssembler` inherits from `Building` (not `Building_WorkTable`) and natively utilizes a custom popup dialog (`Dialog_CreateXenotype`) rather than standard workbench bills (`ITab_Bills` / `BillStack`). Injecting bills into vanilla `Building_GeneAssembler` via Harmony poses high mod-conflict risks with other active gene-editing mods (`InjectGene`, `HCGeneFabrication`, `GeneRipper`, `Biotech Cloning`).

---

## 2. Approved Architectural Solution: Dedicated `EmbryoSplicingBench`

Instead of invasively patching `Building_GeneAssembler`, we introduce a dedicated laboratory workbench that visually clones the vanilla Gene Assembler.

### Key Architectural Advantages:
1. **Zero Mod Conflicts:** Completely isolated from third-party mods that hook or replace vanilla `Building_GeneAssembler` logic.
2. **Native Workbench Bills Architecture:** Inherits directly from `RimWorld.Building_WorkTable`, immediately granting:
   - Full native **Bills tab** (`ITab_Bills`).
   - Standard order configurations: "Do X times", "Do until you have X", ingredient search radius, worker skill restrictions, pause/resume thresholds.
   - Standard `WorkGiver_DoBill` execution (colonists automatically haul embryos and blueprints according to colony priorities).
3. **Facility Linking:** Links to vanilla `GeneBank` and `GeneProcessor` facilities via `CompProperties_AffectedByFacilities` (identical to vanilla assembler).
4. **Visual & Aesthetic Parity:** Uses vanilla `Things/Building/Production/GeneAssembler/GeneAssembler` graphics (3x2 multi-tile footprint).

---

## 3. Detailed Component Specification

### 3.1 Workbench Def (`EmbryoSplicingBench`)
- **File:** `Defs/ThingDefs_Buildings/Buildings_EugenicsBench.xml`
- **DefName:** `EmbryoSplicingBench`
- **Class:** `RimWorld.Building_WorkTable`
- **Parent:** `BenchBase`
- **Size:** (3, 2)
- **Power:** 600W operational / 50W idle (`CompPowerTrader`)
- **Linkable Facilities:**
  - `GeneBank`
  - `GeneProcessor`
  - `GBE_GeneBankLarge` (MayRequire `Farxmai2.GeneBanksExapnded`)
  - `GBE_GeneBankHuge` (MayRequire `Farxmai2.GeneBanksExapnded`)
- **Research Prerequisite:** `Eugenics_ConstructFoundations`
- **Work Type:** `Research` or `Doctor` (Medical / Intellectual skill)

### 3.2 Recipe Definitions & Workers

#### Recipe 1: `Eugenics_BatchApplyBlueprint` (Batch Genome Imprinting)
- **Label:** "Apply genome blueprint disc to embryo"
- **Job String:** "Splicing embryo with genome blueprint."
- **Work Amount:** 2,500 ticks (~40 seconds base work)
- **Skill Requirements:** Intellectual >= 6 or Medical >= 6
- **Ingredients:**
  - 1x `HumanEmbryo` (consumed)
  - 1x `GenomeBlueprintDisk` (**PRESERVED / NOT CONSUMED** - acts as a reusable master matrix)
- **Worker Class:** `Recipe_BatchApplyBlueprint : RecipeWorker`
  - Overrides `ConsumeIngredient`: If ingredient is `GenomeBlueprintDisk`, it is **not** destroyed or consumed; only the `HumanEmbryo` is consumed.
  - Splicing Execution: Extracts the encoded `List<GeneDef>` from `CompGenomeBlueprint`.
  - Spawns output `HumanEmbryo` with:
    - Endogenes overwritten to match the blueprint disc.
    - Attached `CompEmbryoQuality` inheriting defect tracking and screening status.
    - If room cleanliness is dirty, rolls for complication defect introduction based on `CompProperties_EmbryoQuality`.

#### Recipe 2: `Eugenics_BatchScreenEmbryo` (Systematic Genomic Screening)
- **Label:** "Screen embryo genetics"
- **Job String:** "Screening embryo genetics."
- **Work Amount:** 1,200 ticks
- **Skill Requirements:** Medical >= 8
- **Ingredients:**
  - 1x `HumanEmbryo` (unscreened)
- **Worker Class:** `Recipe_BatchScreenEmbryo : RecipeWorker`
  - Calls `CompEmbryoQuality.PerformScreening(doctor)`.
  - Embryo is preserved and updated to `isScreened = true`.

#### Recipe 3: `Eugenics_BatchRecycleEmbryo` (Biomass Liquefaction)
- **Label:** "Liquefy culled embryo into genetic nutrient paste"
- **Job String:** "Recycling embryo biomass."
- **Work Amount:** 800 ticks
- **Ingredients:**
  - 1x `HumanEmbryo` (filtered by bill: defective or designated)
- **Output:** 25x `GeneticNutrientPaste`
- **Worker Class:** Standard recipe or `Recipe_RecycleEmbryo` destroying embryo and producing paste.

---

## 4. Ecosystem & Third-Party Mod Compatibility

### Biotech Cloning (Continued) (`zal.cloning`)
- **Integration Mechanism:** `zal.cloning` patches vanilla `HumanEmbryo` with `Dark.Cloning.CompProperties_CloneEmbryo`. Cloned embryos extracted via `CloneExtractor` are standard `HumanEmbryo` instances.
- **Compatibility:** Our `Patch_EmbryoComp.xml` attaches `CompEmbryoQuality` to `HumanEmbryo`. Therefore, all cloned embryos natively support screening, splicing, recycling, and vat imprinting.
- **Verification Requirement:** Ensure batch recipes filter `HumanEmbryo` broadly so both natural and cloned embryos can be processed interchangeably.

### Vanilla `Building_GrowthVat`
- **Integration Mechanism:** `Patch_GrowthVat.xml` attaches `CompGrowthVatImprinter` to vanilla `GrowthVat`.
- **Occupant Streaming:** Streams skills and passions into whatever `Pawn` is gestating or maturing inside the vat.

---

## 5. Next Agent Action Checklist (Runbook for Implementation)

When picking up Task 6.2.5 in a new session:

1. [ ] **Create C# Recipe Workers (`Source/Recipes/`):**
   - Implement `Recipe_BatchApplyBlueprint.cs` (custom ingredient handling to preserve `GenomeBlueprintDisk`).
   - Implement `Recipe_BatchScreenEmbryo.cs` (screening logic on work table).
2. [ ] **Create Workbench XML (`Defs/ThingDefs_Buildings/Buildings_EugenicsBench.xml`):**
   - Define `EmbryoSplicingBench` inheriting `BenchBase`.
3. [ ] **Create Recipe XML (`Defs/RecipeDefs/Recipes_EugenicsBatch.xml`):**
   - Define `Eugenics_BatchApplyBlueprint`, `Eugenics_BatchScreenEmbryo`, and `Eugenics_BatchRecycleEmbryo`.
4. [ ] **Compile Assembly & Verify Build:**
   - Run `make build-mod MOD=eugenics-program`.
5. [ ] **Generate Dev-Mode QA Runbook (`docs/eugenics-qa-runbook.md`):**
   - Provide step-by-step debug actions menu instructions for rapid testing.
6. [ ] **Update Documentation & Checklists:**
   - Check off Task 6.2.5 in `requirements.md` and `mods/custom/eugenics-program/spec.md`.
