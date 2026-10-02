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
| `NeuralScanner/NeuralScanner_*.png` | [`NeuralScanner`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Buildings/Buildings_OrganicConstructs.xml#L68-L135) | 2×2 tiles | 256 × 256 px | `Graphic_Multi` (`_north`, `_east`, `_south`, `_west`) | Biometric neural scanning station with pawn scanning bed on the right and synaptic console/disc drive on the left. South face features overhead synaptic scanning hood, diagnostic terminal, and motorized imprint disc bay. |

---

### B. Blueprint Discs & Biological Items (`Textures/Things/Item/`)

| Asset Path | XML Definition | Canvas | Graphic Class | Visual Notes |
| :--- | :--- | :--- | :--- | :--- |
| `Blueprints/GenomeBlueprintDisk.png` | [`GenomeBlueprintDisk`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml#L7-L38) | 64 × 64 px | `Graphic_Single` | Ruggedized magnetic/optical data cartridge housing genetic templates. Purple luminescent genepack motif. |
| `Blueprints/NeuralBlueprintDisk_Empty.png` | [`NeuralBlueprintDisk`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml#L38-L70) | 64 × 64 px | `Graphic_Single` | Unwritten neural imprint disc. Dark cryogenic capsule with dormant standby sensor core. |
| `Blueprints/NeuralBlueprintDisk_Encoded.png` | [`NeuralBlueprintDisk`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Items/Items_Blueprints.xml#L38-L70) | 64 × 64 px | `Graphic_Single` | Encoded neural imprint disc. Cryogenic capsule with active glowing amber synaptic tree. |
| `Biomass/GeneticNutrientPaste.png` | [`GeneticNutrientPaste`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Patches/Patch_Biomass.xml#L10-L36) | 128 × 128 px | `Graphic_Single` | Clinical white lab puck canister with magnetic vat coupling port, status diode, and clear perspex observation window showing bubbling greenish-amber slurry. |
| `Augments/Augment_Combat_Basic.png` | `Construct_BasicCombatPackageItem` | 128 × 128 px | `Graphic_Single` | Rugged industrial steel capsule with single crimson synthetic muscle bundle and reflex reticle. |
| `Augments/Augment_Combat_Intermediate.png` | `Construct_IntermediateCombatPackageItem` | 128 × 128 px | `Graphic_Single` | Cobalt medical alloy capsule with dual-branch reflex bridge and optical arc. |
| `Augments/Augment_Combat_Advanced.png` | `Construct_AdvancedCombatPackageItem` | 128 × 128 px | `Graphic_Single` | High-tech gold & carbon capsule, pressurized red cooling conduits, and triad nanite-muscle targeting cluster. |
| `Augments/Augment_Medical_Basic.png` | `Construct_BasicMedicalPackageItem` | 128 × 128 px | `Graphic_Single` | Rugged industrial steel capsule with cyan capillary neural lattice and medical cross core. |
| `Augments/Augment_Medical_Intermediate.png` | `Construct_IntermediateMedicalPackageItem` | 128 × 128 px | `Graphic_Single` | Cobalt medical alloy capsule with dual helical DNA capillaries and twin bio-filter nodes. |
| `Augments/Augment_Medical_Advanced.png` | `Construct_AdvancedMedicalPackageItem` | 128 × 128 px | `Graphic_Single` | High-tech gold & carbon capsule, pressurized cyan cooling conduits, multi-tier neural bridge, and bio-stabilizer halo. |
| `Augments/Augment_Industrial_Basic.png` | `Construct_BasicIndustrialPackageItem` | 128 × 128 px | `Graphic_Single` | Rugged industrial steel capsule with amber heavy-duty synthetic tendon and hex anchor glyph. |
| `Augments/Augment_Industrial_Intermediate.png` | `Construct_IntermediateIndustrialPackageItem` | 128 × 128 px | `Graphic_Single` | Cobalt medical alloy capsule with dual hydraulic bio-pistons and interlocking torque coils. |
| `Augments/Augment_Industrial_Advanced.png` | `Construct_AdvancedIndustrialPackageItem` | 128 × 128 px | `Graphic_Single` | High-tech gold & carbon capsule, pressurized amber cooling conduits, and triple titanium-woven tendons. |
| `Augments/Augment_Laborer_Basic.png` | `Construct_BasicLaborerPackageItem` | 128 × 128 px | `Graphic_Single` | Rugged industrial steel capsule with emerald branching tactile motor ganglion and peripheral tendrils. |
| `Augments/Augment_Laborer_Intermediate.png` | `Construct_IntermediateLaborerPackageItem` | 128 × 128 px | `Graphic_Single` | Cobalt medical alloy capsule with intertwined dexterous bio-cords and twin motor ganglia. |
| `Augments/Augment_Laborer_Advanced.png` | `Construct_AdvancedLaborerPackageItem` | 128 × 128 px | `Graphic_Single` | High-tech gold & carbon capsule, pressurized emerald cooling conduits, and multi-strand neural plexus. |

---

### C. Gene Icons (`Textures/UI/Icons/Genes/`)

All gene icons are rendered on a square canvas (**128 × 128 px**) with transparent background and crisp silhouette.

| Asset File | XML Definition | Role & Description | Visual Theme |
| :--- | :--- | :--- | :--- |
| `Gene_ConstructPsychology.png` | [`Gene_ConstructPsychology`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/GeneDefs/Genes_Construct.xml#L10-L31) | Suppressed socialization, romance, and empathy; forces Psychopath. | Vanilla Kind Instinct teddy bear negated with an aligned, crisp red cancellation 'X' with dark stroke. |
| `Gene_MandatorySterility.png` | [`Gene_MandatorySterility`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/GeneDefs/Genes_Construct.xml#L33-L50) | Total reproductive sterility. | Vanilla Biotech `Gene_Sterile` icon: sterile cell emblem with cancellation line. |
| `Gene_ConstructHibernation.png` | [`Gene_ConstructHibernation`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/GeneDefs/Genes_Construct.xml#L75-L90) | 30-day operation cycle / 48-hour hibernation stasis. | Stasis calibration ring with violet crescent moon, frost crystal, and defragmentation vitals wave. |

---

## 3. How to Update & Polish In-Game Art

1. Open or create your custom sprites in your digital painting tool (Krita, Aseprite, Photoshop, Inkscape, etc.).
2. Export flattened **32-bit transparent PNGs** directly into the corresponding subfolder under [`Textures/`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Textures/), overwriting the placeholder templates.
3. Launch or reload RimWorld. The game will automatically load your new art. No XML edits or code compilation required.
