# GLOSSARY.md — Organic Constructs Lore & Terminology

This document catalogs all in-universe lore names, technical terms, Def names, and concepts introduced across **Organic Constructs** (`tyler.organicconstructs`). Terms flagged for intellectual property (IP) sensitivity or potential renaming are specifically highlighted.

---

## The Tripartite Construct Paradigm

Every construct in the colony is defined by three distinct, layered systems:

```
┌──────────────────────────────────────────────────────────────────┐
│ 1. GENOME (Genetics — Synthesizer)                               │
│    The biological cellular blueprint & genetic foundation.       │
│    • Base Genome: Standard factory unedited biological template. │
│    • Complex Genome: Engineered template with custom genes.      │
├──────────────────────────────────────────────────────────────────┤
│ 2. NEURAL WIRING (Wetware — Growth Vat)                          │
│    The cognitive programming, reflexes, and skill proficiencies. │
│    • Blank Wiring: Innate baseline reflexes (3-4 skill baseline).│
│    • Mentored Wiring: 1:1 doctrine streamed from a Neural Disc.  │
├──────────────────────────────────────────────────────────────────┤
│ 3. BIOWARE (Hardware — Surgery)                                  │
│    Occupational specialization and physical performance suites.  │
│    • Modular packages grafted onto the Biological Neural Bus.    │
│    • Combat, Industrial, Laborer, and Medical packages.          │
└──────────────────────────────────────────────────────────────────┘
```

---

## 1. Core Beings & Classification
| Lore Term | In-Game Def / Class | Current Context & Description | Status |
| :--- | :--- | :--- | :--- |
| **Construct** / **Organic Construct** | Central mod identity (`ConstructUtility.IsConstruct`, `Trait_ConstructAsset`, etc.) | Universal classification for 100% biological vat-grown artificial humans composed of synthetic muscle, organs, and neural wetware. Immune to EMP/solar flares. | ✅ Official Core Term |
| **Base Construct** | Factory default in Synthesizer | Standardized industrial model produced with the unedited **Base Genome**. Bald, beardless, genderless, and sterile. | ✅ Official Term (Phase 1 Roadmap) |
| **Complex Construct** | Spliced custom templates | Advanced model engineered with a **Complex Genome** (custom genes spliced at the Genome Architect terminal). | ✅ Official Term (Phase 1 Roadmap) |
| **Construct Genome** | Embryo genetic template | The foundational biological and metabolic template of the construct (Base vs. Complex). Replaces legacy "Caste" terminology. | ✅ Official Core Term |
| **Neural Wiring** | Vat decanting & neural streaming | The cognitive synaptic pathways determining construct skills (Innate baseline vs. Mentored imprint). | ✅ Official Core Term |
| **Bioware** | Surgical package grafts | The physical modular organs/tissues providing task specialization (Industrial, Combat, Laborer, Medical). | ✅ Official Core Term |
| **Construct Asset** | `Trait_ConstructAsset` | Biological trait designating the construct as colony industrial property. Prevents natural colonist grief/debuffs on death. | ✅ Official Core Term |
| **Construct Embryo** | `CompEmbryoQuality.isConstruct = true` on `HumanEmbryo` | Parentless, vat-synthesized embryonic human prepared for genome imprinting and gestation. Labeled in-game as `construct embryo`. | ✅ Official Core Term |
| **Construct Caste** | Legacy blueprint labeling | Old term for genetic templates. Scheduled for complete deprecation in favor of "Genome" / "Template". | ⚠️ Deprecated (Replaced by "Genome") |
| **SecUnit** | `defaultTemplateLabel`, descriptions | Legacy shorthand for security-oriented synthetic humans. | ⚠️ Deprecated (Scheduled for removal in Phase 1) |
| **Replicant** | Mod descriptions | Legacy evocative reference for biological artificial humans. | ⚠️ Deprecated (Scheduled for removal in Phase 1) |

---

## 2. Genetics & Biological Traits

| Lore Term | In-Game Def / Class | Function & Meaning |
| **Construct Psychology** | `Gene_ConstructPsychology` | Engineered neural architecture suppressing romance, marriage, recreation decay, and social chit-chat; forces Psychopath trait. |
| **Construct Hibernation** | `Gene_ConstructHibernation` | Mandatory 48-hour defragmentation stasis cycle required every 30 operating days; halves biological lifespan (+3 Met). |
| **Mandatory Sterility** | `Gene_MandatorySterility` | Complete biological sterility (+1 Met) protecting proprietary colony genetic investments. |
| **Major Cell Instability** | `Instability_Major` | Rapid cell degradation (+4 Met, 0.6x lifespan, 5x cancer rate) reflecting the disposable nature of synthetic tissue. |
| **Mitochondrial Overdrive** | `Gene_MitochondrialOverdrive` | Extreme metabolic overclocking (+8 Met) at the expense of halved lifespan and 5.0x cancer incidence. |
| **Genomic Compression** | `Gene_GenomicCompression` | Artificially compresses DNA (-10 Complexity) to allow extreme gene stacking, reducing natural immunity and lifespan. |

---

## 3. Workstations & Facilities

| Lore Term | In-Game Def / Class | Role & Function |
| :--- | :--- | :--- |
| **Construct Synthesizer** | `Building_ConstructSynthesizer` | Industrial synthesis bench for mass embryo synthesis, blueprint imprinting, and biomass recycling. |
| **Neural Scanner** | `Building_NeuralScanner` | Biometric casket pod that lossy-scans a natural colonist's proficiencies onto a neural blueprint disc. |
| **Growth Vat Imprinter** | `CompGrowthVatImprinter` | Component patched onto vanilla `GrowthVat` instances to hold a neural blueprint disc and stream skills during gestation. |
| **Genome Architect** / **Genome Encoder** | `ConstructGenomeArchitect` *(Planned)* | Dedicated high-tech design terminal linked to `GeneBank`s for authoring custom master genome discs. |

---

## 4. Blueprints & Storage Media

| Lore Term | In-Game Def / Class | Meaning & Role |
| :--- | :--- | :--- |
| **Genome Blueprint Disc** | `GenomeBlueprintDisk` (`CompGenomeBlueprint`) | Optical/wetware master storage disc encoding an entire genetic template for batch synthesis. |
| **Neural Blueprint Disc** | `NeuralBlueprintDisk` (`CompNeuralBlueprint`) | Storage medium encoding scanned human skills and combat doctrines. |
| **Imprint Doctrine** | `CompNeuralBlueprint.doctrineTitle` | The formal operational title of a scanned skill set (e.g., *"John's Combat Doctrine"*, *"Standard Defense Protocol"*). |

---

## 5. Bioware Augments & Biomass

| Lore Term | In-Game Def / Class | Description |
| :--- | :--- | :--- |
| **Biological Neural Bus** / **Biological Bus** | Recipe requirement description | The proprietary biological wetware interface in constructs allowing seamless surgical graft integration; rejected by natural humans. |
| **Combat Bioware Package** | `Construct_CombatPackage_*` | Bioware organ/tissue graft suite boosting moving, manipulation, armor, and combat reflexes. |
| **Medical Bioware Package** | `Construct_MedicalPackage_*` | Bioware suite enhancing medical tend quality, surgery success, and immunity gain speed. |
| **Industrial Bioware Package** | `Construct_IndustrialPackage_*` | Heavy-duty bioware suite boosting mining yield, construction speed, and toxic resistance. |
| **Laborer Bioware Package** | `Construct_LaborerPackage_*` | Endurance bioware suite boosting work speed, movement, and general hauling capacity. |
| **Genetic Nutrient Paste** | `GeneticNutrientPaste` | High-purity recycled biomass slurry recovered from discarded or collapsed embryos, used to synthesize fresh matrices. |

---

## 6. Physiological & Medical Conditions

| Lore Term | In-Game Def / Class | Description |
| :--- | :--- | :--- |
| **Assimilation Coma** | `Construct_Assimilation` | Severe comatose state induced while synthetic biological packages graft into the neural bus, or upon emergency hibernation failure. |
| **Neural Fatigue** | `Construct_NeuralFatigue` | Brain drain and disorientation suffered by natural human donors after undergoing a neural scan. |
| **Interrupted Stasis** | `Construct_InterruptedStasis` | Neurological hangover and motor impairment caused by prematurely interrupting a 48-hour hibernation cycle. |

---

## 7. Terms Flagged for Renaming & Discussion

### A. "SecUnit" & "Replicant"
* **The Problem:** 
  * *"SecUnit"* is proprietary to Martha Wells' *The Murderbot Diaries*.
  * *"Replicant"* is proprietary to Philip K. Dick's *Do Androids Dream of Electric Sheep?* / *Blade Runner*.
* **Goal:** Replace all instances of SecUnit and Replicant in code defaults, labels, and text descriptions with unique, original in-universe terminology that establishes a distinct identity for this mod.
