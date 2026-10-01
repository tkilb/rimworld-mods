# Organic Constructs - Technical Specification

## Overview
**Organic Constructs** (`tyler.organicconstructs`) is a Biotech expansion introducing mass-produced, 100% biological artificial humans (SecUnits / Replicants). Constructed exclusively from organic muscle, vat-grown tissue, and synthetic neural wetware, constructs are immune to EMP and solar flares, operate with emotionless efficiency, and are treated by natural colonists as industrial property.

---

## 1. Core Mechanics

### 1.1 Workstation: `Building_ConstructSynthesizer`
- **Role:** Industrial synthesis bench for mass embryo creation, batch genome imprinting, and biomass recycling.
- **Disc Receptacle:** Equipped with a `discContainer` holding exactly 1 `GenomeBlueprintDisk`. The disc acts as a reusable master template for batch printing.
- **Bills:**
  - `Synthesize Blank Embryo (Fresh Organics)`: Raw meat, plant food, neutroamine, medicine.
  - `Synthesize Blank Embryo (Recycled Biomass)`: Genetic nutrient paste + reduced raw organics.
  - `Batch Imprint Construct Caste`: Imprints the loaded genome blueprint disc onto blank embryos.
  - `Liquefy Embryo Biomass`: Recycles culled or botched embryos into Genetic Nutrient Paste.

### 1.2 Data Storage: Discs
- **`GenomeBlueprintDisk`:** Stores complete genetic caste templates (`CompGenomeBlueprint`). Reusable in synthesizers.
- **`NeuralBlueprintDisk`:** Stores scanned synaptic patterns, proficiencies, and combat doctrines (`CompNeuralBlueprint`).

### 1.3 Scanning: `Building_NeuralScanner`
- Casket-style biometric scanner pod.
- Scans natural human colonists onto a loaded `NeuralBlueprintDisk`.
- **Lossy Encoding:** During the scan, skills are encoded at **50% of the donor's level** (rounded up to nearest integer) and **all passions are neutralized to None**, providing transparent WYSIWYG disc stats.
- **Construct Restriction:** Constructs cannot be scanned (synthetic neural architecture incompatible).
- Donor experiences temporary `Construct_NeuralFatigue` upon completion.

### 1.4 Growth Vat Imprinting: `CompGrowthVatImprinter`
- Attached directly to vanilla `GrowthVat` instances.
- Holds 1 `NeuralBlueprintDisk`.
- **Mentored Decanting:** When decanted with an encoded disc, construct directly inherits the proficiencies encoded on the disc 1:1 with 0 passions.
- **Blank Decanting:** When decanted without a disc, construct awakens with innate baseline reflexes:
  - Shooting 4, Melee 4, Social 2, Intellectual 2, Artistic 0, all other skills 3. Passions: None.

### 1.5 Construct Biology, Traits & Genes
- **`Gene_ConstructPsychology`:** Suppresses romance, marriage, social chit-chat, and loneliness.
- **`Trait_ConstructAsset`:** Designates construct as colony property. Colonists suffer 0 mood debuffs when a construct dies or is lost.
- **`Gene_ConstructHibernation`:** Requires a 12-hour stasis cycle every 30 days. Interrupted stasis causes `Construct_InterruptedStasis`. Neglecting stasis causes emergency comatose shutdown (`Construct_Assimilation`).
- **`Gene_MandatorySterility`:** Complete sterility (+1 Metabolic Efficiency).
- **`Construct_MetabolicallyEfficient`:** +5 Metabolic Efficiency surplus; forces Psychopath and Bloodlust.
- **Physical Architecture:** Uniform genderless machine physiology (`Gender.None`, "it/its" pronouns, `BodyTypeDefOf.Thin`), completely bald and beardless (`Hair_BaldOnly`, `Beard_NoBeardOnly`).
- **Locked Genome:** Harmony patch blocks `Recipe_ImplantXenogerm` on constructs.

### 1.6 Bioware Augments
- Modular bioware augments: Combat, Medical, Industrial, Laborer (Basic, Intermediate, Advanced).
- Surgical installation restricted exclusively to constructs (baseline humans reject the biological bus).
- Induces `Construct_Assimilation` coma (2 hours on success, 4 hours on failure).
- Failure salvage chance: $20\% + (\text{Doctor Medicine Skill} \times 5\%)$.

---

## 2. Technical Architecture & File Map

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
│   │   └── Recipes_ConstructAugments.xml
│   ├── ResearchProjectDefs/
│   │   └── ResearchProjects_Constructs.xml
│   ├── ThingDefs_Buildings/
│   │   └── Buildings_OrganicConstructs.xml # ConstructSynthesizer, NeuralScanner
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
│   │   └── Patch_SocialAndRomance.cs
│   ├── Recipes/
│   │   ├── Recipe_BatchApplyBlueprint.cs
│   │   ├── Recipe_BatchRecycleEmbryo.cs
│   │   ├── Recipe_InstallConstructPackage.cs
│   │   ├── Recipe_RemoveConstructPackage.cs
│   │   └── Recipe_SynthesizeBlankEmbryo.cs
│   ├── Thoughts/
│   │   └── ThoughtWorker_ConstructProperty.cs
│   ├── WorkGivers/
│   │   ├── WorkGiver_HaulToConstructSynthesizer.cs
│   │   ├── WorkGiver_HaulToGrowthVatImprinter.cs
│   │   ├── WorkGiver_HaulToNeuralScanner.cs
│   │   └── WorkGiver_RecycleEmbryo.cs
│   ├── OrganicConstructs.csproj
│   └── OrganicConstructsMod.cs
├── spec.md
└── qa.md
```
