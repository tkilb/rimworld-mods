# Gene Splicer - Technical Specification

## Overview
**Gene Splicer** (`tyler.genesplicer`) is a vanilla-adjacent Biotech expansion dedicated to natural human embryo germline genetic engineering. It allows colonies to refine, add, and remove inheritable endogenes in natural human embryos prior to growth vat gestation or pregnancy implantation.

---

## 1. Core Mechanics

### 1.1 Building: `Building_EmbryoSplicingBench`
- **Role:** Dedicated genetic workstation for modifying human embryos.
- **Embryo Storage:** Contains a dedicated `ThingOwner embryoContainer` holding exactly 1 `HumanEmbryo`.
- **Facility Linking:** Integrates with vanilla `GeneBank`, `GeneProcessor`, and supported modded gene banks via `CompAffectedByFacilities`. Genepacks stored in powered connected facilities supply the molecular blueprints.
- **Interactions:**
  - **Bench Gizmo ("Load Embryo"):** Displays reachable eligible embryos across the colony. Selecting an embryo designates it for loading.
  - **Bench Gizmo ("Eject Embryo"):** Safely deposits the loaded embryo onto the interaction cell and clears any pending staged orders.
  - **Bench Gizmo ("Edit Genes"):** Opens the custom IMGUI console (`Dialog_EditEmbryoGenes`).
  - **Bench Gizmo ("Cancel Splice Order"):** Cancels a staged modification plan.
  - **Pawn Context Menus:** Right-clicking the bench with a pawn provides quick loading or prioritizes pending doctor splicing jobs. Right-clicking an embryo provides "Carry to Splicing Bench".

### 1.2 Console: `Dialog_EditEmbryoGenes`
- **Current Genes Tab:** Inspects the embryo's current endogenes. Genes can be staged for removal *only* if that exact `GeneDef` is stored in a linked Gene Bank (acting as an extraction template).
- **Add From Gene Banks Tab:** Lists all genes currently present in linked Gene Banks that the embryo does not yet possess. Searchable by name.
- **Genetic Edit Cap:** Strictly enforces a maximum of 2 genetic operations per embryo (tracked via `CompEmbryoQuality.editCount`).
- **Order Staging (Approach B):** Changes are staged and reviewed (+Cpx, +/-Met, total edits) and committed to the bench as an active medical order rather than applying instantly.

### 1.3 Execution & Botch Calculation (`JobDriver_SpliceEmbryo`)
- Once committed, the bench becomes a medical target. A colonist assigned to **Doctoring** claims the job and operates at the bench.
- **Botch Calculation:**
  $$\text{Botch Chance} = \text{Clamp}(0.20 - (\text{Medicine} \times 0.012) - ((\text{Manipulation} - 1.0) \times 0.10) - ((\text{Sight} - 1.0) \times 0.05) - (\text{Cleanliness} \times 0.05), 0.01, 0.50)$$
- **Outcomes:**
  - **Catastrophic Botch:** Cellular structure collapses. Embryo is destroyed and spawns 1× `GeneticNutrientPaste` on the interaction cell.
  - **Success:** Staged additions/removals are applied to the embryo's `GeneSet`. `editCount` is incremented. Parental mood thoughts are triggered.

### 1.4 Parental Social Dynamics (`GeneSplicerParentalUtility`)
When a natural embryo is modified, its biological parents receive memories:
- **Baseline Parents:** Gain `ChildGeneticallyAltered` (-8 mood, 10 days, stacks up to 3).
- **Transhumanist / Body Modder Parents:** Gain `ChildBiologicallyUpgraded` (+6 mood, 10 days, stacks up to 3).

---

## 2. Technical Architecture & File Map

```
mods/custom/gene-splicer/
├── About/
│   └── About.xml                       # PackageId: tyler.genesplicer
├── Assemblies/
│   └── GeneSplicer.dll
├── Defs/
│   ├── JobDefs/
│   │   └── Jobs_GeneSplicer.xml        # LoadEmbryoToBench, SpliceEmbryo
│   ├── ThingDefs_Buildings/
│   │   └── Buildings_GeneSplicer.xml   # EmbryoSplicingBench
│   ├── ThingDefs_Items/
│   │   └── Items_Biomass.xml           # GeneticNutrientPaste
│   ├── ThoughtDefs/
│   │   └── Thoughts_GeneSplicerParental.xml # ChildGeneticallyAltered, ChildBiologicallyUpgraded
│   └── WorkGiverDefs/
│       └── WorkGivers_GeneSplicer.xml  # WorkGiver_LoadEmbryoToBench, WorkGiver_SpliceEmbryo
├── Patches/
│   └── Patch_EmbryoComp.xml            # Injects CompProperties_EmbryoQuality to HumanEmbryo
├── Source/
│   ├── Buildings/
│   │   └── Building_EmbryoSplicingBench.cs
│   ├── Comps/
│   │   ├── CompEmbryoQuality.cs
│   │   └── CompProperties_EmbryoQuality.cs
│   ├── DefOfs/
│   │   └── GeneSplicerDefOf.cs
│   ├── Jobs/
│   │   ├── JobDriver_LoadEmbryoToBench.cs
│   │   └── JobDriver_SpliceEmbryo.cs
│   ├── UI/
│   │   └── Dialog_EditEmbryoGenes.cs
│   ├── Utilities/
│   │   └── GeneSplicerParentalUtility.cs
│   ├── WorkGivers/
│   │   ├── WorkGiver_LoadEmbryoToBench.cs
│   │   └── WorkGiver_SpliceEmbryo.cs
│   └── GeneSplicer.csproj
├── spec.md
└── qa.md
```
