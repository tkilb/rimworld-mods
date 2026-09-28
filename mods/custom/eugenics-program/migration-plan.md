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
| **Augment Cyberware** | None. Constructs have static traits/genes. | 12 Bionic Augment Packages across 4 categories (Combat, Med, Ind, Lab). | Add 12 items, hediffs, surgeries, reboot coma, and salvage roll. |
| **Neural Transfer** | 100% of donor skills; passes donor passions. | 50% of donor skills; forces **0 passions** (`Passion.None`). | Adjust skill multiplier; wipe passions in `CompGrowthVatImprinter`. |
| **Scanner Safeguard** | Anyone can be scanned in Neural Scanner. | Constructs are blocked from being scanned. | Target validation check in `Building_NeuralScanner`. |
| **Decanting Baseline** | Vanilla random baby generation. | Flat factory default skills (4/4/2/2/0/3). | Vat decanting patch / skill floor initialization. |

---

## 2. Chunked Implementation Phases

### Phase 1: Codebase Simplification & Deprecation
**Goal:** Strip out the high-micromanagement screening and defect machinery to establish a clean base.

* **Tasks:**
  1. Deprecate / remove `Recipe_BatchScreenEmbryo.cs`, `JobDriver_ScreenEmbryo.cs`, and `WorkGiver_ScreenEmbryo.cs`.
  2. Remove prenatal screening and culling recipe defs from [`Defs/RecipeDefs/Recipes_EugenicsBatch.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/RecipeDefs/Recipes_EugenicsBatch.xml).
  3. Strip hidden defect list and screening flags from [`Source/Comps/CompEmbryoQuality.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Comps/CompEmbryoQuality.cs).
  4. Adjust [`Defs/RecipeDefs/Recipes_EugenicsBatch.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/RecipeDefs/Recipes_EugenicsBatch.xml) biomass recycling yield from **3× to 1× `GeneticNutrientPaste`**.
* **Verification:** Project compiles cleanly (`dotnet build`); Splicing Bench no longer displays the prenatal screening bill.

---

### Phase 2: Germline Editing Overhaul & Social Dynamics
**Goal:** Implement the 2-edit limit, instant paste collapse on failure, and parental mood thoughts.

* **Tasks:**
  1. Update `CompEmbryoQuality` to track `editCount` (int, default 0).
  2. Enforce `editCount < 2` check in [`Source/UI/Dialog_EditEmbryoGenes.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/UI/Dialog_EditEmbryoGenes.cs) and [`Recipe_BatchApplyBlueprint.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Recipes/Recipe_BatchApplyBlueprint.cs).
  3. Implement failure roll in `Recipe_BatchApplyBlueprint`: On botch, destroy embryo and spawn **1× `GeneticNutrientPaste`** with notification message.
  4. Create `Defs/ThoughtDefs/Thoughts_EugenicsParental.xml`:
     * `ChildGeneticallyAltered`: -8 mood, 10-day duration.
     * `ChildBiologicallyUpgraded`: +6 mood, 10-day duration (triggered if parent has `BodyModder` / `Transhumanist` trait).
  5. In recipe completion, resolve `CompEmbryo.Father` and `CompEmbryo.Mother`. If alive and valid, apply corresponding thought.
* **Verification:** Splicing a natural embryo applies parent thoughts; attempting a 3rd edit is blocked; a failed edit immediately drops 1 paste.

---

### Phase 3: Blank Construct Matrix Synthesis
**Goal:** Enable standalone construct manufacturing without biological donors or third-party cloning mods (*Biotech Cloning Continued* / `zal.cloning`). The mod is completely self-contained.

* **Tasks:**
  1. Add flag `public bool isConstruct` to `CompEmbryoQuality`.
  2. Add `Recipe_SynthesizeBlankEmbryoFresh` and `Recipe_SynthesizeBlankEmbryoRecycled` to [`Defs/RecipeDefs/Recipes_EugenicsBatch.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/RecipeDefs/Recipes_EugenicsBatch.xml):
     * *Fresh:* 40 Meat, 40 Plants, 10 Neutroamine, 2 Medicine.
     * *Recycled:* 1 Genetic Nutrient Paste, 10 Meat, 10 Plants, 5 Neutroamine, 2 Medicine.
  3. Implement `Recipe_SynthesizeBlankEmbryo.cs` worker: Instantiates `HumanEmbryo`, sets `Father = null`, `Mother = null`, and marks `isConstruct = true`.
  4. Update inspect string in `CompEmbryoQuality` to display: *"Origin: Synthetic Construct Matrix"*.
* **Verification:** Both bills appear on `EmbryoSplicingBench`, consume respective ingredients, and produce donorless construct embryos.

---

### Phase 4: Gene Bank Storage Integration
**Goal:** Allow vanilla Gene Banks to store and preserve `GenomeBlueprintDisk` and `NeuralBlueprintDisk`.

* **Tasks:**
  1. Create Harmony patch targeting `RimWorld.CompGenepackContainer`:
     * Postfix on `CanStore(Thing thing)`: Returns `true` if `thing.def == EugenicsDefOf.GenomeBlueprintDisk` or `EugenicsDefOf.NeuralBlueprintDisk`.
     * Patch `Accepts(Thing thing)` / storage filters to allow disc items.
  2. Ensure discs stored in Gene Banks benefit from refrigeration / shelf categorization.
* **Verification:** Haulers can load both disc types into vanilla Gene Banks; discs show up inside Gene Bank contents inspection.

---

### Phase 5: Bionic Augment Packages (Construct Cyberware)
**Goal:** Create the modular construct cyberware hardware packages and surgical operation pipelines.

* **Tasks:**
  1. Create `Defs/HediffDefs/Hediffs_ConstructAugments.xml`:
     * 12 distinct implant hediffs with skill offsets (+2/+4/+6 combat/labor, +3/+5/+7 medical/industrial).
     * `Hediff_ConstructRebooting`: Anesthetic/dormancy state (2h on success, 4h on failure).
  2. Create `Defs/ThingDefs_Items/Items_ConstructAugments.xml`:
     * 12 physical package items craftable at Machining/Fabrication tables.
  3. Create `Defs/RecipeDefs/Recipes_ConstructAugments.xml` and C# `Recipe_InstallConstructPackage`:
     * Restrict installation to pawns with `Trait_ConstructAsset` or `Gene_ConstructPsychology`.
     * Check doctor Medical skill requirements (4, 7, 10).
     * Apply `Hediff_ConstructRebooting` (5,000 ticks on success, 10,000 ticks on failure).
     * Implement failure salvage check: $20\% + (\text{Doctor Medical Skill} \times 5\%)$ to drop the item on the floor.
  4. Create extraction surgery recipes for removing packages.
  5. Implement Harmony patch on `Recipe_ImplantXenogerm.AvailableReport` / `ApplyOnPawn` to block constructs from receiving xenogerms (*"Cannot implant xenogerm: Synthetic construct genetic architecture is locked by corporate biological DRM"*).
* **Verification:** Surgery is rejected on natural humans; successfully installs on constructs with 2h reboot; failure triggers salvage check and 4h reboot; attempting to implant a xenogerm into a construct is blocked.

---

### Phase 6: Neural Imprinting & Skills Refinement
**Goal:** Refine vat maturation to adhere to Murderbot specifications.

* **Tasks:**
  1. In [`Source/Comps/CompGrowthVatImprinter.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Comps/CompGrowthVatImprinter.cs):
     * Set skill transfer multiplier to **0.5 (50%)**.
     * Explicitly wipe passions: `record.passion = Passion.None` for constructs.
     * Ensure disc is **never consumed** upon vat decanting.
  2. Implement baseline factory skill initialization on construct decanting if no neural disc was used:
     * Shooting: 4, Melee: 4, Social: 2, Intellectual: 2, Artistic: 0, others: 3.
  3. In [`Source/Buildings/Building_NeuralScanner.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Buildings/Building_NeuralScanner.cs) & job drivers:
     * Add validation check rejecting pawns with `Trait_ConstructAsset` or `Gene_ConstructPsychology` from entering the scanner.
* **Verification:** Construct gestated with mentor disc gains half skills and 0 passions; construct gestated blank decants with factory base skills; constructs cannot be scanned in Neural Scanner.

---

### Phase 7: Verification, QA Runbook & Polish
**Goal:** End-to-end verification in dev mode and updating user runbooks.

* **Tasks:**
  1. Update `docs/eugenics-qa-runbook.md` with dev-mode spawn codes and test scenarios for both pillars.
  2. Update `About/Description.xml` and user documentation.
  3. Run full compile and inspect log outputs for zero warnings.
