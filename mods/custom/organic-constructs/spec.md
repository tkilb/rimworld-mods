# Organic Constructs - Technical Specification

## Overview
**Organic Constructs** (`tyler.organicconstructs`) is a Biotech expansion introducing mass-produced, 100% biological artificial humans (Base and Complex Constructs). Constructed exclusively from organic muscle, vat-grown tissue, and synthetic neural wetware, constructs are immune to EMP and solar flares, operate with emotionless efficiency, and are treated by natural colonists as industrial property.

---

## 1. Tripartite Architecture

Construct design is governed by three distinct, modular layers:
1. **GENOME (Genetics — Synthesizer & Architect):**
   - **Base Genome:** Standard factory baseline construct template. Unedited, zero-deviation endogenes.
   - **Complex Genome:** Custom engineered template incorporating external gene adaptations. Requires an encoded/burned master disc.
2. **NEURAL WIRING (Wetware — Growth Vat):**
   - **Blank Reflexes:** Innate baseline motor reflexes (Shooting 4, Melee 4, Social 2, Intellectual 2, Artistic 0, Others 3; 0 passions).
   - **Mentored Imprint:** 1:1 synaptic doctrine streamed from an encoded `NeuralBlueprintDisk` (scanned from a natural colonist).
3. **BIOWARE (Hardware — Surgery):**
   - Modular biological augment packages (Combat, Industrial, Laborer, Biotech) grafted directly onto the construct's neural bus. Natural humans reject these packages.

---

## 2. Core Mechanics & Subsystems

### 2.1 Genome Architecture: `Building_ConstructGenomeArchitect`
- **Role:** High-tech biometric workstation (800 W) for drafting construct genomes and burning master discs.
- **GeneBank Integration:** Automatically scans connected, powered `GeneBank` facilities within a 16-cell radius to populate available genepacks.
- **Architect UI (`Dialog_ConfigureConstructGenome`):**
  - Left panel: Locked Core Construct Genes (`Gene_ConstructPsychology`, `Instability_Major`, `Gene_MandatorySterility`, `Gene_ConstructHibernation`, `Immunity_SuperStrong`, `Pain_Reduced`, `Robust`, `MeleeDamage_Strong`, `MoveSpeed_Quick`, `WoundHealing_Fast`, `Superclotting`) + selected adaptations.
  - Right panel: Searchable browser of available genepacks.
  - Bottom bar: Live readouts of Complexity, Net Metabolism, and Genome Stability.
- **ROM Burning:** Once burned, the loaded blank `GenomeBlueprintDisk` is permanently locked (`isBurned = true`) with the chosen template name and gene list.

### 2.2 Stability & Volatility Engine: `ConstructStabilityUtility`
- **Formula:** 
  $$\text{Stability} = \text{Clamp}\Big(1.0 - (\text{Complexity} \times 0.015) + \text{MetabolismBonus} - \text{OptimizerPenalties},\ 0.05,\ 1.0\Big)$$
  - Positive metabolism adds $+2\%$ per point; negative metabolism subtracts $-4\%$ per point.
  - Unstable optimizer genes impose direct stability penalties (e.g. `Gene_MitochondrialOverdrive` $-25\%$, `Gene_GenomicCompression` $-20\%$).
- **Rating Bands:**
  - $\ge 90\%$: Industrial Grade (Green)
  - $75\% - 89\%$: Stable (White)
  - $60\% - 74\%$: Unstable (Orange)
  - $< 60\%$: Highly Volatile (Red)
- **Gameplay Consequences:**
  - **Synthesis Botch Roll:** Volatile genomes dramatically increase synthesis failure chance at the synthesizer:
    $$\text{BaseCollapse} = (1.0 - \text{Stability}) \times 0.5$$
    Factored against doctor skill and room cleanliness. On botch, the embryo collapses into 1x `GeneticNutrientPaste`.
  - **Decant Cellular Defects:** Volatile embryos (< 60% stability) decanting from a Growth Vat carry a $15\%$ risk of developing cellular defects (`Construct_Assimilation` coma or `CryptosleepSickness`).

### 2.3 Embryo Synthesis: `Building_ConstructSynthesizer`
- **Role:** Heavy biochemical workstation (600 W) for single-step embryo synthesis and biomass recycling.
- **Direct Synthesis Bills:**
  - `Synthesize Base Construct Embryo (Fresh Organics / Recycled Biomass)`: Synthesizes a parentless construct embryo carrying the standard baseline construct genome. Requires no disc.
  - `Synthesize Complex Construct Embryo (Fresh Organics / Recycled Biomass)`: Synthesizes a parentless construct embryo carrying the loaded master blueprint's genes. Requires a burned `GenomeBlueprintDisk`. Blocked at bill dispatch if no disc is present (`Patch_WorkGiver_DoBill`).
  - `Liquefy Embryo Biomass`: Recycles discarded embryos into `GeneticNutrientPaste`.

### 2.4 Data Storage: Blueprint Discs
- **`GenomeBlueprintDisk`:** High-capacity optical disc (`CompGenomeBlueprint`). Fabricated blank at `TableFabrication` (`Craft_BlankGenomeDisc`). Burned into a permanent master template at the `ConstructGenomeArchitect`. Reusable during synthesis.
- **`NeuralBlueprintDisk`:** Synaptic storage medium (`CompNeuralBlueprint`). Scanned from natural colonists at the `NeuralScanner`. Reusable in Growth Vats.

### 2.5 Neural Scanning: `Building_NeuralScanner`
- Casket-style biometric scanner pod.
- Scans natural human colonists onto a loaded `NeuralBlueprintDisk`.
- **Lossy Encoding:** Donor skills are encoded at **50% of the donor's level** (rounded up); all passions neutralized to None.
- **Construct Restriction:** Constructs cannot be scanned (incompatible wetware).
- Donor suffers temporary `Construct_NeuralFatigue`.

### 2.6 Vat Incubation & Neural Imprinting: `CompGrowthVatImprinter`
- Attached to vanilla `GrowthVat` instances via patch. Holds 1 `NeuralBlueprintDisk`.
- **20-Day Continuous Incubation Pipeline (Embryo ➔ Age 13):**
  - **Phase 1 (Days 0–4):** Embryonic synthesis takes 4 in-game days.
  - **Phase 2 (Day 4 Form Emergence):** The construct's physical body emerges directly inside the vat fluid as the occupant (`selectedPawn`). Never dropped to the floor as an infant.
  - **Phase 3 (Days 4–20 Visual Maturation):** Rapid in-vat maturation ($\sim 49\times$) ages the construct from Age 0 to Age 13. Players visually see the construct grow through its developmental stages behind the glass.
  - **Mandatory Ejection Lock:** Ejection is strictly disabled prior to Age 13 (*immature neural architecture*).
  - **Growth Milestone Suppression:** Growth Moments (ages 7, 10, 13) are completely suppressed. Constructs never roll random traits or gain passions.
- **Mentored Decanting:** Decanted construct inherits proficiencies encoded on the disc 1:1.
- **Blank Decanting:** Without a disc, construct awakens with baseline reflexes (Shooting 4, Melee 4, Social 2, Intellectual 2, Artistic 0, Others 3; 0 passions).
- **Optional Adult Aging:** Reaching Age 13 unlocks decanting. Constructs left in the vat can continue maturing to **Age 18** for full adult body size ($1.0$).

### 2.7 Construct Biology, Traits & Genes
- **Net Zero Foundation (`0 Met`):** Synthetic metabolic surpluses (`Instability_Major` $+4$, `Gene_ConstructHibernation` $+9$, `Gene_MandatorySterility` $+1$, plus behavioral/utility modifiers $+6$ = $+20\text{ Met}$) precisely balance the enhanced physical chassis ($-20\text{ Met}$), establishing a clean $100\%$ baseline hunger rate.
- **`Gene_ConstructHibernation`:** "Flash in the pan" biology. Requires a 48-hour stasis cycle every 30 days and halves biological lifespan (`LifespanFactor` 0.5, +9 Met). Tracked via native `Need_ConstructStasis` meter and selection gizmo.
- **`Instability_Major`:** Major cell instability (+4 Met, 0.6x lifespan factor, 5x cancer rate).
- **`Gene_MandatorySterility`:** Complete sterility (+1 Met).
- **Physical Chassis Suite:** `Immunity_SuperStrong` (-2 Met), `WoundHealing_Fast` (-2 Met), `Robust` (-2 Met), `Pain_Reduced` (-1 Met), `MeleeDamage_Strong` (-1 Met), `MoveSpeed_Quick` (-1 Met), `Superclotting` (-1 Met).
- **`Trait_ConstructAsset`:** Colony property. Colonists suffer 0 mood debuffs when a construct dies or is lost.
- **Uniform Machine Physiology:** `Gender.None`, "it/its" pronouns, `BodyTypeDefOf.Thin`, completely bald and beardless (`Hair_BaldOnly`, `Beard_NoBeardOnly`).
- **Locked Architecture:** Harmony patch blocks `Recipe_ImplantXenogerm` on constructs.

### 2.8 Bioware Augments
- Modular packages: Combat, Biotech, Industrial, Laborer (Basic, Intermediate, Advanced).
- Surgical installation restricted exclusively to constructs (natural humans reject the bus).
- Induces `Construct_Assimilation` coma. Doctor medicine skill determines recovery/salvage chance.

---

## 3. Technical Architecture & File Map

```
mods/custom/organic-constructs/
├── About/
│   └── About.xml                       # PackageId: tyler.organicconstructs
├── Assemblies/
│   └── OrganicConstructs.dll
├── Defs/
│   ├── GeneDefs/
│   │   ├── Genes_Construct.xml
│   │   └── Genes_Optimizer.xml
│   ├── HediffDefs/
│   │   ├── Hediffs_ConstructAugments.xml
│   │   └── Hediffs_Neural.xml
│   ├── JobDefs/
│   │   ├── Jobs_Constructs.xml
│   │   └── Jobs_Neural.xml
│   ├── RecipeDefs/
│   │   ├── Recipes_ConstructBatch.xml
│   │   ├── Recipes_ConstructAugments.xml
│   │   └── Recipes_Crafting.xml        # Craft_BlankGenomeDisc
│   ├── ResearchProjectDefs/
│   │   └── ResearchProjects_Constructs.xml
│   ├── ThingDefs_Buildings/
│   │   ├── Buildings_OrganicConstructs.xml # ConstructSynthesizer, NeuralScanner
│   │   └── Buildings_GenomeArchitect.xml   # ConstructGenomeArchitect
│   ├── ThingDefs_Items/
│   │   ├── Items_Biomass.xml
│   │   ├── Items_Blueprints.xml
│   │   └── Items_ConstructAugments.xml
│   ├── ThoughtDefs/
│   │   └── Thoughts_Construct.xml
│   ├── TraitDefs/
│   │   └── Traits_Construct.xml
│   └── WorkGiverDefs/
│       └── WorkGivers_Constructs.xml
├── Patches/
│   ├── Patch_GrowthVat.xml
│   └── Patch_EmbryoConstructComp.xml
├── Source/
│   ├── Buildings/
│   │   ├── Building_ConstructSynthesizer.cs
│   │   ├── Building_ConstructGenomeArchitect.cs
│   │   ├── Building_NeuralScanner.cs
│   │   └── NeuralScannerExtension.cs
│   ├── Comps/
│   │   ├── CompEmbryoQuality.cs
│   │   ├── CompGenomeBlueprint.cs
│   │   ├── CompGrowthVatImprinter.cs
│   │   ├── CompNeuralBlueprint.cs
│   │   ├── CompProperties_EmbryoQuality.cs
│   │   ├── CompProperties_GenomeBlueprint.cs
│   │   ├── CompProperties_GrowthVatImprinter.cs
│   │   └── CompProperties_NeuralBlueprint.cs
│   ├── DefOfs/
│   │   ├── ConstructAugmentDefOf.cs
│   │   ├── ConstructDefOf.cs
│   │   └── NeuralImprintDefOf.cs
│   ├── Genes/
│   │   ├── Gene_ConstructHibernation.cs
│   │   ├── Gene_ConstructPsychology.cs
│   │   └── GeneExtension_ConstructPsychology.cs
│   ├── Jobs/
│   │   ├── JobDriver_HaulDiscToContainer.cs
│   │   ├── JobDriver_RecycleEmbryo.cs
│   │   └── JobDriver_ScanNeuralProfile.cs
│   ├── Patches/
│   │   ├── Patch_ColonistDeathThoughts.cs
│   │   ├── Patch_GrowthVatDecant.cs
│   │   ├── Patch_RecipeImplantXenogerm.cs
│   │   ├── Patch_SocialAndRomance.cs
│   │   └── Patch_WorkGiver_DoBill.cs
│   ├── Recipes/
│   │   ├── Recipe_SynthesizeBaseEmbryo.cs
│   │   ├── Recipe_SynthesizeComplexEmbryo.cs
│   │   ├── Recipe_BatchRecycleEmbryo.cs
│   │   ├── Recipe_InstallConstructPackage.cs
│   │   └── Recipe_RemoveConstructPackage.cs
│   ├── UI/
│   │   └── Dialog_ConfigureConstructGenome.cs
│   ├── Thoughts/
│   │   └── ThoughtWorker_ConstructProperty.cs
│   ├── WorkGivers/
│   │   ├── WorkGiver_HaulToConstructSynthesizer.cs
│   │   ├── WorkGiver_HaulToGrowthVatImprinter.cs
│   │   ├── WorkGiver_HaulToNeuralScanner.cs
│   │   └── WorkGiver_RecycleEmbryo.cs
│   ├── ConstructStabilityUtility.cs
│   ├── ConstructUtility.cs
│   ├── OrganicConstructs.csproj
│   └── OrganicConstructsMod.cs
├── spec.md
├── GLOSSARY.md
├── ENHANCEMENTS.md
└── qa/
    ├── qa.md
    └── AGENTS.md
```
