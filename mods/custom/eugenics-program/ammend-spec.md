# Amended Feature Specification: Eugenics Program

> [!IMPORTANT]
> **Target Environment: RimWorld 1.6 (Biotech)**
> All C# code, API signatures, and XML Defs must target **RimWorld version 1.6**. Ensure all class references, Harmony patch targets, and recipe methods strictly match the 1.6 `Assembly-CSharp.dll` codebase.

This document serves as the formal specification amendment to clarify, separate, and streamline the two core systems in the **Eugenics Program** mod:

1. **Germline Gene Editing:** Modifying natural and artificial embryos.
2. **Constructs (SecUnits):** Industrial manufacturing and cybernetic enhancement of artificial humans.

---

## 1. System Separation: The Core Distinction

- **Natural Embryos:** Spliced for colony genetic refinement. Biological parents react emotionally to genetic tampering. Retain normal human psychology, passions, and social relationships.
- **Constructs (Artificial Humans):** Synthesized from blank biological matrices. Treated as company/colony property. Emotionally blunted, incapable of romance, generate zero colonist grief on death, possess no natural passions, and interface exclusively with specialized Bionic Augment Packages.

---

## 2. Germline Gene Editing

Germline gene editing applies to any valid human embryo (both natural colonist embryos and blank construct matrices) at the [`EmbryoSplicingBench`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/ThingDefs_Buildings/Buildings_EugenicsBench.xml).

### 2.1 Editing Limits & Splicing Rules

- **Operation Limit:** Maximum of **2 gene edits** per embryo for game balance.
- **Germline Modifications:** All spliced genes are applied directly to the embryo's `geneSet` (inheritable endogenes).

### 2.2 Splicing Failure & Biomass Collapse (Zero Busywork)

- Splicing has an inherent failure chance based on doctor skill and room cleanliness.
- **On Failure (Botched Splicing):** The embryo's cellular structure collapses immediately on the worktable and drops **1× [`GeneticNutrientPaste`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/ThingDefs_Items/Items_Biomass.xml)**.
- **Obsolete Mechanics Removed:** Eliminates hidden congenital defects, manual sequence-screening bills, and separate disposal bills. Splicing failures resolve instantly without micromanagement.

### 2.3 Parental & Ideological Social Reactions

When a natural embryo's genetics are edited, its biological parents receive memory thoughts upon bill completion (tracked via `CompEmbryo.Father` and `CompEmbryo.Mother`):

- **Standard Parents:** **-8 mood debuff for 10 days** (*"Child genetically altered"*)
  - *Hover Description:* *"Someone manipulated my unborn child's DNA in a laboratory. Splicing natural flesh like industrial material is an abhorrent violation of our family."*
- **Body Modder / Transhumanist Parents:** **+6 mood buff for 10 days** (*"Child biologically upgraded"*)
  - *Hover Description:* *"My unborn child was engineered at the splicing bench to transcend weak baseline genetics. True perfection begins before birth."*
- **Constructs:** Have no parents (`Father = null, Mother = null`), producing zero parental mood events.

---

## 3. Constructs (Artificial Humans)

### 3.1 Innate Biology & Murderbot Psychology

Constructs gestated from synthetic matrices possess locked baseline traits and genes reflecting corporate mass-production:

- **`Gene_ConstructPsychology` (+2 Met):** Cold indifference, immune to insults, incapable of romance or marriage, no social recreation need.
- **`Trait_ConstructAsset`:** Property status; natural-born colonists suffer **zero mood penalties** when a construct dies, suffers, or is organ-harvested.
- **Psychic Deafness (`PsychicSensitivity_Deaf` / +2 Met):** Immune to psychic drones, suppression, and psycasts.
- **Universal Sterility (`Gene_MandatorySterility` / +1 Met):** Cannot reproduce or cross-pollinate genetic drift.
- **Very Unattractive (`Beauty_VeryUgly` / +2 Met):** Misshapen, synthetic appearance. Grants a positive metabolic efficiency dividend.
- **Deathrest:** Capable of long-term dormant stasis.
- **Slow Study:** Feature set is pre-programmed; slow at learning new routines organically.
- **Metabolically Efficient (`Construct_MetabolicallyEfficient` / +5 Met Dividend):** Eliminates empathy and fear, converting psychological removal directly into a metabolic surplus. Automatically enforces the **Psychopath** and **Bloodlust** traits in XML (`forcedTraits`).
- **Strict Xenogerm Rejection (Biological DRM):** Constructs cannot be implanted with vanilla or modded Xenogerms (`Recipe_ImplantXenogerm` blocked: *"Synthetic construct genetic architecture is locked by corporate biological DRM"*). All post-gestation upgrades must come via modular Bionic Augment Packages.

---

### 3.2 Blank Matrix Synthesis (Donorless Embryos)

Constructs do not require ovum extraction or sperm donation. Players craft synthetic matrices at the [`EmbryoSplicingBench`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/ThingDefs_Buildings/Buildings_EugenicsBench.xml) via two distinct bills:

1. **Synthesize Blank Embryo (Fresh Organics):**
   - _Ingredients:_ 40 Raw Meat + 40 Raw Plants + 10 Neutroamine + 2 Medicine.
   - _Role:_ Standard mid-game synthesis path.
2. **Synthesize Blank Embryo (Recycled Biomass):**
   - _Ingredients:_ 1× [`GeneticNutrientPaste`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/ThingDefs_Items/Items_Biomass.xml) + 10 Raw Meat + 10 Raw Plants + 5 Neutroamine + 2 Medicine.
   - _Role:_ High-efficiency recycling loop using biomass recovered from botched splicings or culled units.

_Generated embryo is a standard `HumanEmbryo` tagged with `IsConstruct = true` and `Father = null, Mother = null`, fully compatible with vanilla Growth Vats._

---

### 3.3 Gene Bank Compatibility for Data Discs

Both data disc types can be stored and refrigerated directly inside vanilla `Building_GeneBank` facilities alongside standard genepacks via a Harmony patch on `CompGenepackContainer`:

- **`GenomeBlueprintDisk`:** Contains full biological endogene caste templates.
- **`NeuralBlueprintDisk`:** Contains neural scanner skill engrams.

---

### 3.4 Bionic Augment Packages (Construct-Exclusive Cyberware)

Constructs utilize modular hardware packages installed and uninstalled via standard medical surgery bills.

#### Restrictions & Rules

- **Target Restriction:** **Constructs only.** Standard humans reject the proprietary neural bus and cannot install these packages. The surgical recipe checks for `Trait_ConstructAsset` or `Gene_ConstructPsychology`; operations on baseline humans are blocked with the message: *"Cannot install: Incompatible proprietary neural bus (Requires Construct)."*
- **Surgical Skill Requirements:**
  - **Basic Tiers:** Medical skill 4+
  - **Intermediate Tiers:** Medical skill 7+
  - **Advanced Tiers:** Medical skill 10+
- **Reboot State (`ConstructRebooting` Hediff):**
  - **Successful Install:** Construct enters a dormant reboot state for **2 in-game hours** (5,000 ticks).
  - **Failed Install:** Construct suffers minor surgical lacerations and a forced safety reboot for **4 in-game hours** (10,000 ticks).
- **Hardware Salvage on Surgery Failure:**
  - To prevent immediate destruction of expensive modules, an installation failure rolls for component salvage:
    $$\text{Salvage Chance} = 20\% + (\text{Doctor's Medical Skill} \times 5\%)$$
  - _Salvaged:_ The augment package ejects safely onto the floor undamaged.
  - _Unsalvaged:_ The module burns out and is destroyed.

#### Package Catalog & Skill Offsets

| Package Name                        | Tier         | Skill Offsets                     | Work / Tech Level     |
| :---------------------------------- | :----------- | :-------------------------------- | :-------------------- |
| **Basic Combat Package**            | Basic        | +2 Shooting, +2 Melee             | Machining             |
| **Intermediate Combat Package**     | Intermediate | +4 Shooting, +4 Melee             | Microelectronics      |
| **Advanced Combat Package**         | Advanced     | +6 Shooting, +6 Melee             | Fabrication / Bionics |
| **Basic Medical Package**           | Basic        | +3 Medical                        | Machining             |
| **Intermediate Medical Package**    | Intermediate | +5 Medical                        | Microelectronics      |
| **Advanced Medical Package**        | Advanced     | +7 Medical                        | Fabrication / Bionics |
| **Basic Industrial Package**        | Basic        | +3 Construction, +3 Mining        | Machining             |
| **Intermediate Industrial Package** | Intermediate | +5 Construction, +5 Mining        | Microelectronics      |
| **Advanced Industrial Package**     | Advanced     | +7 Construction, +7 Mining        | Fabrication / Bionics |
| **Basic Laborer Package**           | Basic        | +2 Cooking, +2 Plants, +2 Animals | Machining             |
| **Intermediate Laborer Package**    | Intermediate | +4 Cooking, +4 Plants, +4 Animals | Microelectronics      |
| **Advanced Laborer Package**        | Advanced     | +6 Cooking, +6 Plants, +6 Animals | Fabrication / Bionics |

---

### 3.5 Construct Skills & Neural Imprinting

#### Maturation with Neural Discs

- Constructs (not humans) incubated in a Growth Vat can optionally have a single [`NeuralBlueprintDisk`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/ThingDefs_Items/Items_Blueprints.xml) loaded into the vat.
- **Disc Reusability:** Discs are **reusable master media and are never consumed** upon decanting. The disc remains securely seated in the vat's imprinter for subsequent construct gestations until manually ejected.
- **Loading Window:** Discs can be loaded into the vat at any point before or during gestation/maturation. If loaded, skill engrams stream into the developing brain; if decanted without a disc, default firmware skills apply.
- **Skill Transfer Multiplier:** Constructs receive **50% of the scanned donor's skill levels**.
- **Passions Disabled:** Constructs **never possess passions**. All injected and generated passions are forced to `Passion.None` (emotions and intrinsic motivation are manufactured away).
- **No Stacking:** Constructs cannot receive multiple neural disc imprints.

#### Scanner Restrictions

- Constructs **may not** act as templates in the [`Building_NeuralScanner`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/Defs/ThingDefs_Buildings/Buildings_Neural.xml). The scanner rejects any pawn possessing `Trait_ConstructAsset` or `Gene_ConstructPsychology`.

#### Baseline Decanting (No Neural Data)

Constructs decanted from a vat without any neural blueprint disc initialized possess a flat factory firmware skill distribution:

- **Shooting:** 4
- **Melee:** 4
- **Social:** 2
- **Intellectual:** 2
- **Artistic:** 0
- **All Other Skills:** 3
