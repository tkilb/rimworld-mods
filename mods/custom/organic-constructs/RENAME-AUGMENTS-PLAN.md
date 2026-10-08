# RENAME-AUGMENTS-PLAN.md — Transition from "Bioware" to "Neural Augments"

## 1. Objective & Motivation

Standardize all terminology across **Organic Constructs** (`tyler.organicconstructs`) from the legacy term **"Bioware"** to **"Neural Augments"** (singular: *Neural Augment*, plural: *Neural Augments*).

### Why the Change?
* **Lore Consistency:** Constructs are defined by their biological neural bus and wetware interfaces. "Neural Augment" clearly communicates that these packages are cognitive and neuromuscular hardware grafted onto the nervous system.
* **Clarity in the Tripartite Framework:**
  1. **GENOME** (Genetics — Embryo & Splicing)
  2. **NEURAL WIRING** (Wetware — Growth Vat Imprint)
  3. **NEURAL AUGMENTS** (Hardware — Surgical Bus Packages) *(replaces "Bioware")*
* **Avoid Ambiguity:** "Bioware" frequently conflates with vanilla bionics or prosthetics. "Neural Augments" emphasizes that they are modular doctrine packages exclusive to the construct's neural bus.

---

## 2. Naming Order & Hierarchy: `[TYPE] [LEVEL]`

### Why Type-First (`[TYPE] [LEVEL]`)?
In vanilla RimWorld, workbench bill lists, surgical operation menus, and stockpile/storage filter trees sort entries **alphabetically**:
* **Old `[LEVEL] [TYPE]` Problem:**
  * Bills were scattered alphabetically: *"Fabricate Advanced Combat"*, *"Fabricate Basic Combat"*, and *"Fabricate Intermediate Combat"* appeared under **A**, **B**, and **I** in the bill menu.
* **New `[TYPE] Neural Aug. [I/II/III]` Solution (Steam Deck Optimized):**
  * Retains the core **"Neural"** lore identity without generic bionic ambiguity.
  * Uses the sharp, concise sci-fi abbreviation **"Aug."** to stay under ~20 characters, preventing string truncation on 800p Steam Deck screens.
  * Roman numerals (`I`, `II`, `III`) provide immediate visual tier recognition and clean alphabetical sorting:
    * `Combat Neural Aug. I`
    * `Combat Neural Aug. II`
    * `Combat Neural Aug. III`
  * Matches the player's mental model: *"I want a Combat augment. What tier can I afford / craft right now?"*

---

## 3. Terminology Mapping Matrix

| Current Legacy Term | New Standard Term (`[TYPE] Neural Aug. [TIER]`) | In-Game Context |
| :--- | :--- | :--- |
| `Bioware` / `Bioware Package` | `Neural Augment` / `Neural Augment Package` | General taxonomy & category |
| `basic combat package` (Item) | `Combat Neural Aug. I` | Item label in stockpile/shelf |
| `intermediate combat package` (Item) | `Combat Neural Aug. II` | Item label in stockpile/shelf |
| `advanced combat package` (Item) | `Combat Neural Aug. III` | Item label in stockpile/shelf |
| `basic biotech package` (Item) | `Biotech Neural Aug. I` | Item label in stockpile/shelf |
| `intermediate biotech package` (Item) | `Biotech Neural Aug. II` | Item label in stockpile/shelf |
| `advanced biotech package` (Item) | `Biotech Neural Aug. III` | Item label in stockpile/shelf |
| `basic industrial package` (Item) | `Industrial Neural Aug. I` | Item label in stockpile/shelf |
| `intermediate industrial package` (Item) | `Industrial Neural Aug. II` | Item label in stockpile/shelf |
| `advanced industrial package` (Item) | `Industrial Neural Aug. III` | Item label in stockpile/shelf |
| `basic laborer package` (Item) | `Laborer Neural Aug. I` | Item label in stockpile/shelf |
| `intermediate laborer package` (Item) | `Laborer Neural Aug. II` | Item label in stockpile/shelf |
| `advanced laborer package` (Item) | `Laborer Neural Aug. III` | Item label in stockpile/shelf |
| `fabricate basic combat augment` | `fabricate Combat Neural Aug. I` | Work bill label (Synthesizer/Bench) |
| `fabricate advanced combat augment` | `fabricate Combat Neural Aug. III` | Work bill label (Synthesizer/Bench) |
| `install basic combat augment` | `install Combat Neural Aug. I` | Medical surgery bill |
| `install advanced combat augment` | `install Combat Neural Aug. III` | Medical surgery bill |
| `remove basic combat augment` | `remove Combat Neural Aug. I` | Medical surgery bill |
| `Basic Bioware` (Research) | `Basic Neural Augments` | Research project label |
| `Intermediate Bioware` (Research) | `Intermediate Neural Augments` | Research project label |
| `Advanced Bioware` (Research) | `Advanced Neural Augments` | Research project label |
| `Construct_BiowareBasic` (defName) | `Construct_NeuralAugmentsBasic` | XML ResearchProjectDef defName |
| `Construct_BiowareIntermediate` | `Construct_NeuralAugmentsIntermediate` | XML ResearchProjectDef defName |
| `Construct_BiowareAdvanced` | `Construct_NeuralAugmentsAdvanced` | XML ResearchProjectDef defName |
| `BiowareSkillUtility` | `NeuralAugmentSkillUtility` | C# static utility class |
| `AllBiowareHediffNames` | `AllNeuralAugmentHediffNames` | C# hediff hashset |
| `IsBiowareAugment()` | `IsNeuralAugment()` | C# helper method |
| `Installing bioware augment.` | `Installing neural augment.` | Surgeon job string |
| `Removing bioware augment.` | `Removing neural augment.` | Surgeon job string |
| `salvage basic bioware augment` | `salvage Neural Aug. I` | Synthesizer bill label |

---

## 4. Comprehensive File Modification Inventory

### 3.1 C# Source Code
* **Rename File:**
  * `Source/BiowareSkillUtility.cs` ➔ [`Source/NeuralAugmentSkillUtility.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source)
  * Update class name to `NeuralAugmentSkillUtility`.
  * Update methods: `IsBiowareAugment` ➔ `IsNeuralAugment`, `AllBiowareHediffNames` ➔ `AllNeuralAugmentHediffNames`.
* **[`Source/Patches/Patch_SkillRecord_Aptitude.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_SkillRecord_Aptitude.cs):**
  * Update reference: `BiowareSkillUtility.GetSkillBonus(...)` ➔ `NeuralAugmentSkillUtility.GetSkillBonus(...)`.
* **[`Source/Recipes/Recipe_InstallConstructPackage.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Recipes/Recipe_InstallConstructPackage.cs):**
  * Update variables: `existingBiowares` ➔ `existingAugments`.
  * Update check: `BiowareSkillUtility.IsBiowareAugment(...)` ➔ `NeuralAugmentSkillUtility.IsNeuralAugment(...)`.
  * Update player-facing messages:
    * `"Already has this bioware package installed."` ➔ `"Already has this neural augment installed."`
    * `"Extracted previous bioware package..."` ➔ `"Extracted previous neural augment package..."`
    * `"Surgery failed, but the bioware augment was salvaged."` ➔ `"Surgery failed, but the neural augment was salvaged."`
    * `"Surgery failed and the bioware augment was ruined."` ➔ `"Surgery failed and the neural augment was ruined."`
* **[`Source/DefOfs/ConstructAugmentDefOf.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/DefOfs/ConstructAugmentDefOf.cs):**
  * Review any def references or XML comments.
* **[`OrganicConstructs.csproj`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/OrganicConstructs.csproj):**
  * Verify compilation integrity and ensure old `.cs` references are cleaned up.

---

### 3.2 XML Defs

* **[`Defs/ResearchProjectDefs/ResearchProjects_Constructs.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ResearchProjectDefs/ResearchProjects_Constructs.xml):**
  * Rename research defNames and labels:
    * `Construct_BiowareBasic` ➔ `Construct_NeuralAugmentsBasic` (`<label>basic neural augments</label>`)
    * `Construct_BiowareIntermediate` ➔ `Construct_NeuralAugmentsIntermediate` (`<label>intermediate neural augments</label>`)
    * `Construct_BiowareAdvanced` ➔ `Construct_NeuralAugmentsAdvanced` (`<label>advanced neural augments</label>`)
  * Update prerequisite pointers in downstream research nodes.
* **[`Defs/RecipeDefs/Recipes_CraftingAugments.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_CraftingAugments.xml):**
  * Update `<researchPrerequisite>` references from `Construct_Bioware*` to `Construct_NeuralAugments*`.
  * Update recipe labels and descriptions (e.g., `salvage basic bioware augment` ➔ `salvage basic neural augment`).
* **[`Defs/RecipeDefs/Recipes_ConstructAugments.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/RecipeDefs/Recipes_ConstructAugments.xml):**
  * Update `<jobString>`:
    * `Installing bioware augment.` ➔ `Installing neural augment.`
    * `Removing bioware augment.` ➔ `Removing neural augment.`
  * Update surgery recipe descriptions:
    * Replace *"Surgically graft a [tier] [specialization] bioware augment..."* with *"Surgically graft a [tier] [specialization] neural augment..."*.
* **[`Defs/HediffDefs/Hediffs_ConstructAugments.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/HediffDefs/Hediffs_ConstructAugments.xml):**
  * Update `<description>` fields for Combat, Biotech, Industrial, and Laborer hediffs to reference *neural augment* instead of *bioware package*.
* **[`Defs/ThingDefs_Items/Items_ConstructAugments.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_ConstructAugments.xml):**
  * Update item labels and descriptions from *bioware package* to *neural augment package*.

---

### 4.3 Documentation & Architecture Guides

* **[`spec.md`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/spec.md):**
  * Section 1 (Tripartite Architecture): Rename `3. BIOWARE (Hardware — Surgery)` ➔ `3. NEURAL AUGMENTS (Hardware — Surgery)`.
  * Section 2.8: Rename `2.8 Bioware Augments` ➔ `2.8 Neural Augments`.
* **[`GLOSSARY.md`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/GLOSSARY.md):**
  * Update Tripartite paradigm diagram.
  * Section 5: Rename to `5. Neural Augments & Biomass`.
  * Add entry marking `Bioware` as officially deprecated in favor of `Neural Augment`.
* **[`ENHANCEMENTS.md`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/ENHANCEMENTS.md):**
  * Align roadmap items with the Neural Augment nomenclature.
* **[`GRAV-PLAN.md`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/GRAV-PLAN.md):**
  * Update Section 2.2 references (*Gravship Operations Neural Augments*).

---

### 4.4 QA Test Cases

* **[`qa/qa.md`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/qa/qa.md):**
  * Update test case index links.
* **Rename QA Files:**
  * `qa/tc-06-bioware-augment-grafting.md` ➔ `qa/tc-06-neural-augment-grafting.md`
  * `qa/tc-10-bioware-augment-salvage-and-storage.md` ➔ `qa/tc-10-neural-augment-salvage-and-storage.md`
  * Update internal text and assertions within both test cases.

---

## 5. Execution Plan & Rollout Checklist

1. [ ] **Step 1: C# Refactoring**
   * Rename `BiowareSkillUtility.cs` to `NeuralAugmentSkillUtility.cs`.
   * Refactor class, method, and variable identifiers.
   * Update `Patch_SkillRecord_Aptitude.cs` and `Recipe_InstallConstructPackage.cs`.
   * Compile and verify with `dotnet build Source/OrganicConstructs.csproj`.
2. [ ] **Step 2: XML Defs Updates**
   * Update `ResearchProjects_Constructs.xml`.
   * Update `Recipes_CraftingAugments.xml` and `Recipes_ConstructAugments.xml`.
   * Update `Hediffs_ConstructAugments.xml` and `Items_ConstructAugments.xml`.
3. [ ] **Step 3: Documentation & QA Alignment**
   * Update `spec.md`, `GLOSSARY.md`, `ENHANCEMENTS.md`, and `GRAV-PLAN.md`.
   * Rename and update QA test case files.
4. [ ] **Step 4: Final Verification**
   * Run full build: `make build-mod MOD=organic-constructs`.
   * Validate mod status: `make status`.
