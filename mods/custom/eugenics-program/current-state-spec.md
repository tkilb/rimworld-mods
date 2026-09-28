# Current State Specification: Eugenics Program

> [!IMPORTANT]
> **Target Environment: RimWorld 1.6 (Biotech)**
> The existing codebase targets **RimWorld 1.6** (with 1.5 backward support). References and assembly compilations are built against RimWorld 1.6 `Assembly-CSharp.dll`.

This document captures the exact operational state, data structures, and mechanics of the **Eugenics Program** mod as implemented in the codebase prior to the new directional amendment.

---

## 1. Overview of Existing System

The current mod is focused on mass-producing vat-bred soldiers and laborers ("Constructs") by combining endogene editing at an Embryo Splicing Bench with skill injection via Growth Vats.

### Existing Architecture & Core Assets
* **Facilities & Workbenches:**
  * [`Building_EmbryoSplicingBench`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Buildings/Building_EmbryoSplicingBench.cs): Dedicated worktable for batch embryo splicing, screening, and biomass recycling.
  * [`Building_NeuralScanner`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Buildings/Building_NeuralScanner.cs): Subcore-scanner style facility where colonist mentors sit to burn their skills and passions into a neural disc.
* **Items & Containers:**
  * [`GenomeBlueprintDisk`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/ThingDefs_Items/Items_Blueprints.xml): Holds a list of `GeneDef`s acting as a biological caste master template.
  * [`NeuralBlueprintDisk`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/ThingDefs_Items/Items_Blueprints.xml): Holds a dictionary of `SkillDef -> int` (target levels) and `SkillDef -> Passion`.
  * [`GeneticNutrientPaste`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/ThingDefs_Items/Items_Biomass.xml): High-nutrition slurry (0.9 nutrition) yielded from recycling culled embryos.
* **Component Tracking on `HumanEmbryo`:**
  * [`CompEmbryoQuality`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Comps/CompEmbryoQuality.cs): Attached to vanilla `HumanEmbryo`. Tracks `qualityScore`, `isScreened`, and a list of `hiddenDefects`.
* **Growth Vat Imprinting:**
  * [`CompGrowthVatImprinter`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Source/Comps/CompGrowthVatImprinter.cs): Attached to vanilla `Building_GrowthVat`. Holds a single `NeuralBlueprintDisk` and streams skill XP and passions to the vat occupant over time.

---

## 2. Implemented Recipes & Workflows

1. **Blueprint Splicing (`Recipe_BatchApplyBlueprint`):**
   * Uses 1 `HumanEmbryo`. Copies genes from the bench's loaded master disc onto the embryo's `geneSet`.
   * Has a chance to introduce hidden congenital defects (e.g. `BadBack`, `Frail`, `Asthma`, `Carcinoma`) depending on doctor skill and room cleanliness.
2. **Prenatal Screening (`Recipe_BatchScreenEmbryo`):**
   * Medical bill that scans an embryo to reveal whether it has hidden defects (`isScreened = true`).
3. **Biomass Liquefaction (`Recipe_BatchRecycleEmbryo`):**
   * Medical/Labor bill that destroys 1 `HumanEmbryo` to generate **3× `GeneticNutrientPaste`**.
4. **Neural Scanning (`JobDriver_ScanNeuralProfile`):**
   * Colonist mentor enters `Building_NeuralScanner` to encode their current skills and passions onto a blank `NeuralBlueprintDisk`.

---

## 3. Construct Traits & Genetics

* **`Gene_ConstructPsychology`:** Suppresses social recreation, romance, and grief over construct deaths.
* **`Construct_MetabolicallyEfficient`:** Grants +5 `biostatMet` and forces `Psychopath` and `Bloodlust` traits.
* **`Gene_MandatorySterility`:** Forces sterility and grants +1 `biostatMet`.
* **`Gene_MitochondrialOverdrive` / `Gene_GenomicCompression`:** High-yield metabolic and complexity optimizers with cellular instability penalties.
* **`Trait_ConstructAsset`:** Flags the pawn as property; colonist death thoughts are patched out via `Patch_ColonistDeathThoughts`.

---

## 4. Architectural Limitations & Pain Points (The "Why We Are Amending")

1. **Blurred Identity:** Natural embryos and synthetic constructs were treated under the same umbrella. There was no clean distinction between a colonist baby being gene-edited versus an artificial corporate unit being manufactured.
2. **Excessive Micromanagement:** Splicing defects required a multi-step loop: splice embryo $\rightarrow$ run prenatal screening bill $\rightarrow$ inspect defect $\rightarrow$ manually queue liquefy embryo bill.
3. **No Synthetic Embryo Bootstrapping:** Embryos had to originate from natural colonist ovum extraction / fertilization. Players could not synthesize donorless blank construct matrices.
4. **Data Disc Inconvenience:** Blueprint and Neural discs could not be stored in vanilla Gene Banks.
5. **No Modular Upgrades:** Once gestated, constructs had no modular system for runtime hardware/skill reconfiguration (bionic augment packages).
6. **Construct Passions & Skill Spillover:** Constructs retained natural passions if scanned mentors had them, contradicting the unfeeling Murderbot concept.
