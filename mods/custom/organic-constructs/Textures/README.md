# Organic Constructs — Art & Texture Specification

This document details all visual assets used by **Organic Constructs** (`tyler.organicconstructs`), their game definitions, required pixel dimensions, graphic classes, and art direction guidelines. All runtime `.png` files reside directly in this `Textures/` directory matching the `<texPath>` and `<iconPath>` definitions in XML.

---

## 1. RimWorld Texture Conventions

- **Format:** 32-bit PNG (RGBA with alpha transparency).
- **Scale Standard:** 64 px per in-game map tile (standard) or 128 px per tile (2x high-resolution / recommended for modern RimWorld).
- **Lighting / Shading:** Standard RimWorld top-left light source (~45-degree angle from upper left). Subtle dark outlines (1–2 px `#1A1A1A` or dark tinted border).
- **Graphic Classes:**
  - `Graphic_Single`: A single standalone sprite used regardless of rotation (typical for items, meals, discs, packages).
  - `Graphic_Multi`: 4 directional sprites with suffixes `_north` (facing top), `_east` (facing right), `_south` (facing bottom / camera), `_west` (facing left).

---

## 2. Complete Asset Manifest & Mapping

### A. Buildings (`Textures/Things/Building/`)

| Asset Folder / File | XML Definition | Footprint | Canvas (2x) | Graphic Class | Visual Notes |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `ConstructSynthesizer/ConstructSynthesizer_*.png` | [`ConstructSynthesizer`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Buildings/Buildings_OrganicConstructs.xml#L7-L76) | 3×1 tiles | 384 × 128 px | `Graphic_Multi` (`_north`, `_east`, `_south`, `_west`) | Heavy industrial synthesis vat/bench. Bio-slurry tanks, robotic manipulators, disc drive slot on front console. South face should feature the operator terminal and disc intake tray. |
| `NeuralScanner/NeuralScanner_*.png` | [`NeuralScanner`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Buildings/Buildings_OrganicConstructs.xml#L81-L148) | 1×2 tiles | 192 × 320 px | `Graphic_Multi` (`_north`, `_east`, `_south`, `_west`) | Upright biometric scanning sarcophagus/pod with a viewing glass or neural sensor crown. South face shows open/translucent diagnostic glass; north face shows rear heat syncs and power cabling. |

---

### B. Blueprint Discs & Biological Items (`Textures/Things/Item/`)

| Asset Path | XML Definition | Canvas | Graphic Class | Visual Notes |
| :--- | :--- | :--- | :--- | :--- |
| `Blueprints/GenomeBlueprintDisk.png` | [`GenomeBlueprintDisk`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml#L7-L38) | 128 × 128 px | `Graphic_Single` | Ruggedized magnetic/optical data cartridge housing genetic templates. Cyan/teal bioluminescent accents, DNA helix or matrix motif. |
| `Blueprints/NeuralBlueprintDisk.png` | [`NeuralBlueprintDisk`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml#L40-L75) | 128 × 128 px | `Graphic_Single` | High-density synaptic pattern core. Deep purple/magenta circuitry, brainwave or neural lattice motif. |
| `Biomass/GeneticNutrientPaste.png` | [`GeneticNutrientPaste`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Patches/Patch_Biomass.xml#L10-L36) | 128 × 128 px | `Graphic_Single` | Concentrated embryonic nutrient paste container or pressurized pouch. Murky greenish-amber bio-organic liquid. |
| `Augments/AugmentPackage_Basic.png` | `Construct_Basic*PackageItem` | 128 × 128 px | `Graphic_Single` | Basic Tier bioware package. Sealed sterile cryo-ampoule or synthetic muscle bundle with industrial gray/steel casing. |
| `Augments/AugmentPackage_Intermediate.png` | `Construct_Intermediate*PackageItem` | 128 × 128 px | `Graphic_Single` | Intermediate Tier bioware package. Blue/cobalt medical alloy casing with synthetic neural bridge fibers visible. |
| `Augments/AugmentPackage_Advanced.png` | `Construct_Advanced*PackageItem` | 128 × 128 px | `Graphic_Single` | Advanced Tier bioware package. High-tech gold/amber casing, nanite-infused synthetic muscle tissue. |

---

### C. Gene Icons (`Textures/UI/Icons/Genes/`)

All gene icons are rendered on a square canvas (**128 × 128 px**) with transparent background and crisp silhouette.

| Asset File | XML Definition | Role & Description | Visual Theme |
| :--- | :--- | :--- | :--- |
| `Gene_ConstructPsychology.png` | [`Gene_ConstructPsychology`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/GeneDefs/Genes_Construct.xml#L10-L31) | Suppressed socialization, romance, and empathy; forces Psychopath. | Mask or neutral synthetic skull with emotional circuits crossed out / silenced. Cold teal/cyan. |
| `Gene_MandatorySterility.png` | [`Gene_MandatorySterility`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/GeneDefs/Genes_Construct.xml#L33-L50) | Total reproductive sterility. | Crossed-out gamete/cell or sterile shield emblem. Clean industrial red or gray. |
| `Gene_ConstructHibernation.png` | [`Gene_ConstructHibernation`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/GeneDefs/Genes_Construct.xml#L75-L90) | 30-day operation cycle / 12-hour hibernation stasis. | Stasis pod, freezing crystal, or sleeping neural wave. Ice blue / violet. |
| `Gene_MitochondrialOverdrive.png` | [`Gene_MitochondrialOverdrive`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/GeneDefs/Genes_Optimizer.xml#L10-L28) | +8 Metabolic Efficiency with accelerated cell decay. | Exploding / overclocked mitochondrion or glowing double helix with warning aura. Orange/red. |
| `Gene_GenomicCompression.png` | [`Gene_GenomicCompression`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/GeneDefs/Genes_Optimizer.xml#L30-L48) | -10 Complexity with strain on longevity. | Dense folded DNA helix or chromosome in a molecular clamp. Indigo / purple. |

---

## 3. How to Update & Polish In-Game Art

1. Open or create your custom sprites in your digital painting tool (Krita, Aseprite, Photoshop, Inkscape, etc.).
2. Export flattened **32-bit transparent PNGs** directly into the corresponding subfolder under [`Textures/`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Textures/), overwriting the placeholder templates.
3. Launch or reload RimWorld. The game will automatically load your new art. No XML edits or code compilation required.
