# Migration Plan: Eugenics Program Refinement

> [!IMPORTANT]
> **Target Environment: RimWorld 1.6 (Biotech)**
> All migration steps, C# classes, and XML configurations must be implemented and compiled against **RimWorld 1.6**. When writing Harmony patches and referencing game types (`Assembly-CSharp.dll`), ensure compatibility with 1.6 internal method signatures.

This document outlines the step-by-step, chunked technical execution plan to transition the **Eugenics Program** codebase from its current implementation to the target architecture defined in [`spec.md`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/spec.md).

---

## 1. State Delta Summary

| Dimension | Current State ([`current-state-spec.md`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/current-state-spec.md)) | Target State ([`spec.md`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/spec.md)) | Delta / Action Required |
| :--- | :--- | :--- | :--- |
| **System Boundary** | Blurry overlap between normal embryos and constructs. | Strictly separated: Pillar 1 (Germline Editing) vs Pillar 2 (Constructs). | Architectural restructuring and clear labeling in UI and comps. |
| **Splicing Failure** | Splicing causes hidden defects requiring manual screening & culling bills. | Splicing failure instantly collapses embryo into **1× Genetic Nutrient Paste**. | Remove screening/defect system; implement instant collapse on failure. |
| **Edit Limits** | Unlimited gene edits per embryo. | Capped at **2 gene edits** per embryo. | Add `editCount` tracking and cap check in UI / recipe. |
| **Parental Reactions** | None. | -8 mood debuff (10 days) for parents; +6 mood buff for Body Modders. | Add custom `ThoughtDef`s and parent trigger in recipe completion. |
| **Construct Bootstrapping** | Requires natural colonist ovum extraction / fertilization. | Splicing Bench bills synthesize donorless blank construct matrices. | Add 2 synthesis recipes (Fresh Organics vs Recycled Biomass). |
| **Data Disc Storage** | Discs cannot be stored in vanilla Gene Banks. | Gene Banks natively store and refrigerate both disc types. | Harmony patch on `CompGenepackContainer`. |
| **Construct Bio-Augments** | None. Constructs have static traits/genes. | 12 Bio-Augment Packages across 4 categories (Combat, Med, Ind, Lab). | Add 12 items, hediffs, surgeries, assimilation coma, and salvage roll. |
| **Neural Transfer** | 100% of donor skills; passes donor passions. | 50% of donor skills; forces **0 passions** (`Passion.None`). | Adjust skill multiplier; wipe passions in `CompGrowthVatImprinter`. |
| **Scanner Safeguard** | Anyone can be scanned in Neural Scanner. | Constructs are blocked from being scanned. | Target validation check in `Building_NeuralScanner`. |
| **Decanting Baseline** | Vanilla random baby generation. | Flat innate baseline neural reflexes (4/4/2/2/0/3). | Vat decanting patch / skill floor initialization. |

---

## 2. Model Routing & Execution Tiers

To optimize efficiency and token usage across implementations, tasks are tagged with recommended model tiers:

| Tier | Typical Scope | Examples |
| :--- | :--- | :--- |
| **Flash Low** | Pure XML definitions, boilerplate defs, file deletions, documentation | Removing legacy bills, writing basic item/thought defs, docs |
| **Flash Med** | Standard isolated C# components, properties, UI adjustments, simple workers | `editCount` tracking, inspect strings, vanilla component getters |
| **Flash High** | Complex multi-system C# workers, dynamic calculation loops, custom surgery pipelines | Custom surgery workers with salvage rolls, pawn skill initialization |
| **Pro / Sonnet** | High-risk Harmony patches modifying core RimWorld Biotech systems | `CompGenepackContainer` storage acceptance, `Recipe_ImplantXenogerm` DRM hook |

---

## 3. Chunked Implementation Phases

### Phase 1: Codebase Simplification & Deprecation
**Goal:** Strip out the high-micromanagement screening and defect machinery to establish a clean base.

* **Tasks:**
  1. `[Flash Low]` Deprecate / remove `Recipe_BatchScreenEmbryo.cs`, `JobDriver_ScreenEmbryo.cs`, and `WorkGiver_ScreenEmbryo.cs`.
  2. `[Flash Low]` Remove prenatal screening and culling recipe defs from [`Defs/RecipeDefs/Recipes_EugenicsBatch.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/RecipeDefs/Recipes_EugenicsBatch.xml).
  3. `[Flash Med]` Strip hidden defect list and screening flags from [`Source/Comps/CompEmbryoQuality.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Comps/CompEmbryoQuality.cs) (ensure clean `ExposeData` cleanup).
  4. `[Flash Low]` Adjust [`Defs/RecipeDefs/Recipes_EugenicsBatch.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/RecipeDefs/Recipes_EugenicsBatch.xml) biomass recycling yield from **3× to 1× `GeneticNutrientPaste`**.
* **Verification:** Project compiles cleanly (`dotnet build`); Splicing Bench no longer displays the prenatal screening bill.

---

### Phase 2: Germline Editing Overhaul & Social Dynamics
**Goal:** Implement the 2-edit limit, instant paste collapse on failure, and parental mood thoughts.

* **Tasks:**
  1. `[Flash Med]` Update `CompEmbryoQuality` to track `editCount` (int, default 0, saved via `ExposeData`).
  2. `[Flash Med]` Enforce `editCount < 2` check in [`Source/UI/Dialog_EditEmbryoGenes.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/UI/Dialog_EditEmbryoGenes.cs) and [`Recipe_BatchApplyBlueprint.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Recipes/Recipe_BatchApplyBlueprint.cs).
  3. `[Flash Med]` Implement failure roll in `Recipe_BatchApplyBlueprint`: On botch, destroy embryo and spawn **1× `GeneticNutrientPaste`** with notification message.
  4. `[Flash Low]` Create `Defs/ThoughtDefs/Thoughts_EugenicsParental.xml`:
     * `ChildGeneticallyAltered`: -8 mood, 10-day duration.
     * `ChildBiologicallyUpgraded`: +6 mood, 10-day duration (triggered if parent has `BodyModder` / `Transhumanist` trait).
  5. `[Flash Med]` In recipe completion, resolve `CompEmbryo.Father` and `CompEmbryo.Mother`. If alive and valid, apply corresponding thought.
* **Verification:** Splicing a natural embryo applies parent thoughts; attempting a 3rd edit is blocked; a failed edit immediately drops 1 paste.

---

### Phase 3: Blank Construct Matrix Synthesis
**Goal:** Enable standalone construct manufacturing without biological donors or third-party cloning mods (*Biotech Cloning Continued* / `zal.cloning`). The mod is completely self-contained.

* **Tasks:**
  1. `[Flash Low]` Add flag `public bool isConstruct` to `CompEmbryoQuality`.
  2. `[Flash Low]` Add `Recipe_SynthesizeBlankEmbryoFresh` and `Recipe_SynthesizeBlankEmbryoRecycled` to [`Defs/RecipeDefs/Recipes_EugenicsBatch.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/RecipeDefs/Recipes_EugenicsBatch.xml):
     * *Fresh:* 40 Meat, 40 Plants, 10 Neutroamine, 2 Medicine.
     * *Recycled:* 1 Genetic Nutrient Paste, 10 Meat, 10 Plants, 5 Neutroamine, 2 Medicine.
  3. `[Flash Med]` Implement `Recipe_SynthesizeBlankEmbryo.cs` worker: Instantiates `HumanEmbryo`, sets `Father = null`, `Mother = null`, marks `isConstruct = true`, and populates the construct baseline endogene set (`Gene_ConstructPsychology`, `Construct_MetabolicallyEfficient`, `Gene_MandatorySterility`, `RobustDigestion`, `StrongStomach`, `PsychicAbility_Deaf`, `Beauty_VeryUgly`, `Gene_ConstructHibernation`, `LowSleep`, `Learning_Slow`).
  4. `[Flash Low]` Update inspect string in `CompEmbryoQuality` to display: *"Origin: Synthetic Construct Matrix"*.
* **Verification:** Both bills appear on `EmbryoSplicingBench`, consume respective ingredients, and produce donorless construct embryos.

---

### Phase 4: Gene Bank Storage Integration
**Goal:** Allow vanilla Gene Banks to store and preserve `GenomeBlueprintDisk` and `NeuralBlueprintDisk`.

* **Tasks:**
  1. `[Pro / Sonnet]` Create Harmony patch targeting `RimWorld.CompGenepackContainer`:
     * Postfix on `CanStore(Thing thing)`: Returns `true` if `thing.def == EugenicsDefOf.GenomeBlueprintDisk` or `EugenicsDefOf.NeuralBlueprintDisk`.
     * Patch `Accepts(Thing thing)` / storage filters to allow disc items without causing hauling loops or breaking vanilla container serialization.
  2. `[Flash Med]` Ensure discs stored in Gene Banks benefit from refrigeration / shelf categorization.
* **Verification:** Haulers can load both disc types into vanilla Gene Banks; discs show up inside Gene Bank contents inspection.

---

### Phase 5: Bio-Augment Packages (Construct Bioware)
**Goal:** Create the modular construct bio-augment packages and surgical grafting pipelines.

* **Tasks:**
  1. `[Flash Med]` Create `Defs/HediffDefs/Hediffs_ConstructAugments.xml`:
     * 12 distinct bio-implant hediffs with skill offsets (+2/+4/+6 combat/labor, +3/+5/+7 medical/industrial); pure bio-wetware (zero EMP vulnerability comps).
     * `Hediff_ConstructAssimilation`: Neural assimilation state (2h on success, 4h on failure).
  2. `[Flash Low]` Create `Defs/ThingDefs_Items/Items_ConstructAugments.xml`:
     * 12 physical bio-package items craftable at Machining/Fabrication tables.
  3. `[Flash High]` Create `Defs/RecipeDefs/Recipes_ConstructAugments.xml` and C# `Recipe_InstallConstructPackage`:
     * Restrict grafting to pawns with `Trait_ConstructAsset` or `Gene_ConstructPsychology`.
     * Check doctor Medical skill requirements (4, 7, 10).
     * Apply `Hediff_ConstructAssimilation` (5,000 ticks on success, 10,000 ticks on failure).
     * Implement failure salvage check: $20\% + (\text{Doctor Medical Skill} \times 5\%)$ to drop the item on the floor.
  4. `[Flash Low]` Create extraction surgery recipes for removing packages.
  5. `[Pro / Sonnet]` Implement Harmony patch on `Recipe_ImplantXenogerm.AvailableReport` / `ApplyOnPawn` to block constructs from receiving xenogerms (*"Cannot implant xenogerm: Synthetic construct genetic architecture is permanently locked"*).
  6. `[Flash High]` Implement `Gene_ConstructHibernation` and `Hediff_InterruptedStasis`:
     * Add "Enter Stasis" gizmo/order directing construct to bed or ground.
     * Enforce 12-hour minimum cycle; interrupting early applies `Hediff_InterruptedStasis` (-20% consciousness, -10% moving, -10% manipulation, nausea for 3 days).
     * Clean wake at 12+ hours resets Rest need to 100% and resets the 30-day operating timer.
     * Track operational age (1,800,000 ticks / 30 days): Fire maintenance warning letter at Day 27 (75,000 ticks remaining); trigger forced comatose emergency shutdown at Day 30.
* **Verification:** Surgery is rejected on natural humans; successfully installs on constructs with 2h assimilation; failure triggers salvage check and 4h assimilation coma; attempting to implant a xenogerm into a construct is blocked; stasis hibernation enforces 12h cycle, grants 100% rest on clean exit, and fires maintenance warnings/emergency shutdown at the 30-day limit.

---

### Phase 6: Neural Imprinting & Skills Refinement
**Goal:** Refine vat maturation to adhere to Murderbot specifications.

* **Tasks:**
  1. `[Flash Med]` In [`Source/Comps/CompGrowthVatImprinter.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Comps/CompGrowthVatImprinter.cs):
     * Set skill transfer multiplier to **0.5 (50%)**.
     * Explicitly wipe passions: `record.passion = Passion.None` for constructs.
     * Ensure disc is **never consumed** upon vat decanting.
  2. `[Flash High]` Implement innate baseline neural reflex initialization on construct decanting if no neural disc was used:
     * Shooting: 4, Melee: 4, Social: 2, Intellectual: 2, Artistic: 0, others: 3.
  3. `[Flash Med]` In [`Source/Buildings/Building_NeuralScanner.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Buildings/Building_NeuralScanner.cs) & job drivers:
     * Add validation check rejecting pawns with `Trait_ConstructAsset` or `Gene_ConstructPsychology` from entering the scanner.
* **Verification:** Construct gestated with mentor disc gains half skills and 0 passions; construct gestated blank decants with innate baseline neural reflexes; constructs cannot be scanned in Neural Scanner.

---

### Phase 7: Verification, QA Runbook & Polish
**Goal:** End-to-end verification in dev mode and updating user runbooks.

* **Tasks:**
  1. `[Flash Low]` Update `docs/eugenics-qa-runbook.md` with dev-mode spawn codes and test scenarios for both pillars.
  2. `[Flash Low]` Update `About/Description.xml` and user documentation.
  3. `[Flash Low]` Run full compile and inspect log outputs for zero warnings.
