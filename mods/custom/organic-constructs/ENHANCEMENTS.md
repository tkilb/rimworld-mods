# Organic Constructs — Planned Enhancements & Roadmap

This document outlines proposed mechanics, technical architectures, and design decisions for future feature additions to **Organic Constructs** (`tyler.organicconstructs`).

---

## Phased Implementation Plan

### Phase 1: Nomenclature & IP Normalization
* **Goal:** Establish a distinct, original in-universe identity and remove all external IP references (`SecUnit`, `Replicant`).
* **Official Taxonomy:**
  * **Constructs:** The universal classification for vat-grown biological humans.
  * **Base Constructs:** The factory-standard, unedited biological genome.
  * **Complex Constructs:** Custom variants engineered with spliced genes and higher genetic complexity.
* **Scope:**
  * Update [`About.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/About/About.xml) and [`Buildings_OrganicConstructs.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Buildings/Buildings_OrganicConstructs.xml).
  * Update default template label in [`CompGenomeBlueprint.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Comps/CompGenomeBlueprint.cs) from `"SecUnit Baseline Caste"` to `"Base Construct Template"`.
  * Align [`spec.md`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/spec.md), [`AGENTS.md`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/AGENTS.md), and [GLOSSARY.md](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/GLOSSARY.md).

### Phase 2: Factory Baseline Architecture (Zero-Disc Workflow)
* **Goal:** Streamline the early/mid-game synthesis loop by eliminating starter disc clutter.
* **Scope:**
  * Update [`Building_ConstructSynthesizer.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Buildings/Building_ConstructSynthesizer.cs) so that with no disc loaded, it natively produces and imprints **Base Constructs**.
  * Transition physical [`GenomeBlueprintDisk`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml) items to crafted blanks dedicated exclusively to Complex Constructs.

### Phase 3: Dedicated Terminal (`ConstructGenomeArchitect`) & Gene Splicing UI
* **Goal:** Add late-game research and infrastructure to author custom Complex Construct genomes from linked vanilla `GeneBank`s.
* **Scope:**
  * Create `Building_ConstructGenomeArchitect` linking to nearby `GeneBank` facilities.
  * Implement interactive `Dialog_ConfigureConstructGenome` with locked 4 Core Construct Genes, cosmetic defaulting (bald/thin), and bank gene selection.
  * Implement burn-once (ROM) disc authoring.

### Phase 4: Genome Stability Rating & Factory Volatility (Model C)
* **Goal:** Provide transparent, deterministic risk assessment when engineering Complex Constructs.
* **Scope:**
  * Terminal calculates and displays live Genome Stability Rating (Complexity, Metabolism, Optimizer genes).
  * Factory batch synthesis enforces cellular collapse (Genetic Nutrient Paste) and defect chances for highly volatile templates.

---

## Technical Specifications by System

## 2. Template Architecture & Disc Lifecycle

### 2.1 Built-in Factory Baseline (No Disc Required)
* **Decision:** The `ConstructSynthesizer` possesses an internal default firmware containing the factory-standard **Base Construct Genome**.
* **Zero Inventory Hassle:** Players do not need to craft, store, or manage a physical disc for basic construct production. If no disc is loaded into the synthesizer, it automatically prints the standard baseline model.
* **Consistency:** Directly parallels the Growth Vat imprinter design (empty vat = baseline reflexes; loaded disc = custom doctrine).

### 2.2 Advanced Discs for Complex Constructs
* **Item:** Physical [`GenomeBlueprintDisk`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml#L10) items are used **exclusively for custom Complex Constructs**.
* **Burn-Once / ROM Architecture:** Once encoded at the terminal, the disc is permanently burned to that genetic template. Players craft multiple blank discs at the Fabrication Bench to build a physical library of specialized genome templates (e.g. *Heavy Laborer Genome Disc*, *Vanguard Combat Genome Disc*, *Field Medic Genome Disc*).
* **Workflow:** Insert blank disc into the Genome Architect terminal $\rightarrow$ compile genes from linked `GeneBank` units $\rightarrow$ insert finished master disc into `ConstructSynthesizer` for batch production.

---

## 3. Genome Stability Rating & Volatility Mechanics

### 3.1 Transparent Genome Stability Rating (Model C)
* **Design Philosophy:** 100% transparent and deterministic WYSIWYG feedback at the design terminal before committing resources.
* **Terminal Stability Meter:** The Genome Architect calculates a real-time **Genome Stability Index** (%) based on:
  * **Genetic Complexity:** Moderate complexity maintains high stability; extreme complexity taxes cellular integrity.
  * **Net Metabolic Efficiency:** Deficits (negative metabolism) strain the synthetic organism's metabolic upkeep.
  * **Optimizer Genes:** Aggressive overclocking genes (such as *Mitochondrial Overdrive* or *Genomic Compression*) impose direct stability trade-offs.
* **Pre-Burn Preview:** Before finalizing the master disc, the UI explicitly displays:
  * Overall Genome Stability rating (e.g., *98% Industrial Grade* vs. *62% Experimental / Volatile*).
  * Base embryo splicing collapse chance.
  * Potential defect risks upon decanting.

### 3.2 Factory Batch Production Consequences
* When the master disc is loaded into the `ConstructSynthesizer`:
  * **Cellular Collapse:** Splicing failure rate is directly governed by the disc's Stability Rating, modified by the operator's medical/intellectual skill and room cleanliness. Botched embryos dissolve into Genetic Nutrient Paste.
  * **Decant Defects (Volatile Genomes):** If players intentionally push an experimental template into deep volatility (<70% stability), decanted constructs have a chance to manifest cellular defects (e.g., increased cancer rate, shorter lifespan, or sensory decay).

---

## 4. Genome Architect Terminal (`ConstructGenomeArchitect`) & UI Specification

### 4.1 Immutable Core Construct Genome
Every authored genome template strictly includes the **4 Core Construct Genes**, locked permanently into the template:
1. **`Gene_ConstructPsychology`:** Suppresses romance, marriage, social chit-chat, and loneliness.
2. **`Construct_MetabolicallyEfficient`:** Excision of empathy provides +5 Metabolic Efficiency surplus; forces Psychopath and Bloodlust.
3. **`Gene_MandatorySterility`:** Complete sterility to protect proprietary genetic investments (+1 Metabolic Efficiency).
4. **`Gene_ConstructHibernation`:** 30-day operation cycle requiring a 12-hour stasis hibernation.

### 4.2 Spliced Adaptations from Linked Gene Banks
* The player builds upon the Core Genome by selecting available genes from nearby linked vanilla `GeneBank` facilities.
* Metabolic efficiency from the core genes (+6 total net) gives the player an immediate metabolic budget to afford powerful adaptations (e.g., robust, great crafting, quick sleeper, dark vision).

### 4.3 Aesthetic Defaulting & Cosmetic Overrides
* Unless the player deliberately selects an aesthetic gene from their gene banks, the template automatically burns:
  * `Hair_BaldOnly`
  * `Beard_NoBeardOnly`
  * `BodyTypeDefOf.Thin`
* If cosmetic hair or body genes are selected, the terminal overrides the defaults accordingly, allowing specialized infiltrators or distinct construct silhouettes.

### 4.4 Terminal Interface (`Dialog_ConfigureConstructGenome`)
* **Left Panel:** Immutable Core Genome genes + currently selected spliced genes.
* **Right Panel:** Available genepacks present in all connected `GeneBank` units.
* **Bottom Panel:**
  * Real-time **Complexity** & **Net Metabolism** readouts.
  * Live **Genome Stability Rating** meter (with expected factory failure rates and defect warnings).
  * Custom Template Name input (e.g., *"Titan Heavy Genome"*, *"Vanguard Genome"*).
  * **"Burn Master Disc"** action button (requires loaded blank disc in the terminal).

