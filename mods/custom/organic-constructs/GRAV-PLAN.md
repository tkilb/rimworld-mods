# GRAV-PLAN.md — Organic Constructs: Odyssey Gravship Integration

## Overview & Lore Premise

In _RimWorld: Odyssey_, mobile gravships redefine colony survival around nomadic exploration, extreme spatial constraints, and high environmental volatility. Integrating **Organic Constructs** (`tyler.organicconstructs`) introduces a classic sci-fi paradigm: a small command cadre of natural human officers operating a mobile vessel crewed by emotionless, synthetic biological labor units.

Constructs eliminate the social friction, romance disputes, and recreation demands of cramped shipboard quarters, while remaining immune to the EMP, solar flare, and ion storm hazards that plague mechanoid-heavy ships. Furthermore, unlike mechanoid gestators, constructs produce zero toxic wastepacks.

This plan details future shipboard miniaturization, dedicated gravship bioware, and mobile life-support loops for when this integration is revisited.

---

## 1. Architectural Pillars

```text
┌─────────────────────────────────────────────────────────────────────────┐
│                           ODYSSEY GRAVSHIP                              │
│                                                                         │
│  [ Officers' Quarters ]      [ Compact Shipboard Vat ]                  │
│   • Natural Humans            • 2x1 Growth Vat with Neuro-Imprinter     │
│   • Command & Strategy        • Field decanting from master discs       │
│                                                                         │
│  [ Bridge & Grav Engine ]    [ Auxiliary Construct Synthesizer ]        │
│   • Nav / Turret Stations     • Space-efficient (2x1) field printer     │
│   • Flight Wetware Bioware    • Consumes chemfuel & recycled biomass    │
│                                                                         │
│  [ Construct Stasis Bunks ]  [ Closed-Loop Slurry Tank ]                │
│   • 48h stasis cycle bay      • Liquefies lost constructs               │
│   • 0 recreation footprint    • Zero toxic wastepacks                   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Proposed Features & Shipboard Components

### 2.1 Space-Efficient Hardware

- **Auxiliary Shipboard Synthesizer (`Building_ShipConstructSynthesizer`):**
  - **Dimensions:** 2x1 footprint (reduced from the standard 3x2 `Building_ConstructSynthesizer`).
  - **Role:** Field replenishment only. Does not author genomes; requires pre-burned `GenomeBlueprintDisk` items prepared at a planetary lab.
  - **Power:** 350 W (optimized for limited grav-engine power grids).
- **Compact Growth Cradle (`Building_CompactGrowthVat`):**
  - Miniaturized gestation cradle designed to snap flush against ship bulkheads.
  - Retains `CompGrowthVatImprinter` for 1:1 doctrine streaming from `NeuralBlueprintDisk`s.

### 2.2 Dedicated Bioware: Gravship Operations Suite

A modular biological augment graft designed specifically for vessel maintenance and hazardous field drops:

- **`Construct_GravOpsPackage` (Gravship Flight & Operations Bioware):**
  - **Target Slot:** Thoracic / Neural Bus.
  - **Effects:**
    - +25% Ship piloting and maneuver calculation speed.
    - +30% Hull repair, construction, and firefighting speed.
    - Immune to decompression / hard vacuum / grav-shift disorientation.
    - Reduced oxygen and nutrition burn during flight maneuvers.
- **`Construct_SalvagePackage` (Deep-Field EVA & Breaching Bioware):**
  - Enhanced kinetic armor and rapid deep-drill mining yield for surface landing sorties.

### 2.3 Shipboard Life Support & Organic Provisioning

- **Zero Toxic Footprint:**
  - Unlike mechanoid gestators and chargers that accumulate toxic wastepacks in valuable cargo holds, organic construct production produces zero toxic waste.
- **Standard Organic Inputs:**
  - Uses the standard fresh-organics synthesis pipeline (`Recipe_SynthesizeBaseEmbryoFresh`: raw meat/protein, crops, neutroamine, medicine). No convoluted liquefaction slurry needed.
- **Hibernation Alignment:**
  - Shipboard stasis bunks allow synchronized construct hibernation ([`Gene_ConstructHibernation`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Genes/Gene_ConstructHibernation.cs)) during long planetary transit hops, eliminating food and life-support consumption while airborne.

---

## 3. Mod Dependency & Integration Strategy

- **Soft Dependency / Conditional Loading:**
  - Implement patches inside `Patches/Odyssey/` guarded by package check:
    ```xml
    <Operation Class="PatchOperationFindMod">
      <mods>
        <li>Ludeon.RimWorld.Odyssey</li>
      </mods>
      <match Class="PatchOperationSequence">
        <!-- Shipboard Defs & Grav Engine linking -->
      </match>
    </Operation>
    ```
- **Load Order:**
  - `tyler.organicconstructs` loads after `Ludeon.RimWorld.Odyssey`.

---

## 4. Tech Tree Restructuring & Progression Pacing

### The "40-Hour Gap" Problem

In standard progression, [`Construct_Foundations`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ResearchProjectDefs/ResearchProjects_Constructs.xml#L5) is an **Ultra-tech** project gated behind `Xenogermination`, which requires `Microelectronics`, `Multi-Analyzer`, and `Fabrication`.
Because constructs age ~3.3x faster than natural humans due to [`Instability_Major`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/spec.md#L82) (0.6x lifespan) and [`Gene_ConstructHibernation`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/spec.md#L81) (0.5x lifespan), starter constructs would develop dementia, carcinoma, and frailty within 5–8 in-game years—dozens of hours before the player unlocks ultra-tech synthesis. They would die off and disappear until the late-game.

### The Solution: Two-Tier Tech Architecture

```text
┌────────────────────────────────────────────────────────┐
│ TIER 1: Field Bioproduction (Industrial - 600 pts)    │
│  Prerequisites: Electricity + Basic Biotech/Medicine  │
│  • Synthesize BASE CONSTRUCTS ONLY (un-spliced)        │
│  • No custom disc authoring, no bioware packages       │
│  • Enables replenishment pipeline early in the run    │
└──────────────────────────┬─────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────┐
│ TIER 2: Master Genome Architecture (Ultra - 1600 pts)  │
│  Prerequisites: Xenogermination + Fabrication          │
│  • ConstructGenomeArchitect & Gene Splicing terminal   │
│  • Complex Constructs, ROM Discs & Bioware Packages    │
│  • Neural Scanners & 1:1 Skill Imprinting              │
└────────────────────────────────────────────────────────┘
```

1. **Tier 1 (Field Bioproduction):**
   - Early-to-mid game accessible (Industrial tech level).
   - Unlocks the ability to synthesize **Base Constructs only** using raw ingredients (meat, herbal medicine, chemfuel, steel) without needing discs or gene splicing.
   - Gives the colony the tools to replace failing constructs _before_ they turn frail.
2. **Tier 2 (Ultra-Tech Splicing & Neural Wetware):**
   - Preserved for late-game: authoring complex custom templates, high-tier bioware grafts, and veteran neural scanning.

---

## 5. Starting Scenario: "The Gravship Vanguard"

### Narrative Hook

_A corporate exploration charter dispatched to the Rimworld to claim mineral stakes with a gravship prototype. The ship survived a catastrophic descent, leaving a single biological officer and two synthetic labor units with limited stores to restore flight capability._

### Scenario Roster

- **1 Natural Human Officer (The Supervisor):**
  - Normal human with full emotions, romance, traits, and passions.
  - Capable of intellectual work, social diplomacy, and leadership.
- **2 Base Constructs (`C-01` and `C-02`):**
  - Age: Biological age 13 (freshly decanted, maximizing working years before rapid cell decay sets in).
  - Baseline Blank Reflexes (Skill 3–4 across the board, 0 passions).
  - Trait: [`Trait_ConstructAsset`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/spec.md#L85) (0 colonist grief upon loss).

### Starting Equipment & Infrastructure

- **Pre-Researched Tech:** Tier 1 _Field Bioproduction_ unlocked.
- **Shipboard Hardware:**
  - 1x Crashed/Damaged Gravship Hull (Grav Engine intact).
  - 1x Damaged/Salvaged Auxiliary Synthesizer (or parts to deploy one).
  - 1x Growth Cradle with built-in Imprinter.
- **Starting Consumables:**
  - 30x Herbal Medicine, 15x Neutroamine (starter biological reagents).
  - 300x Chemfuel, 40x Packaged Survival Meals.

---

## 6. Anti-Cheat & Balance Levers

To prevent early-game construct mass-production from breaking game balance, production is gated by strict logistical bottlenecks rather than artificial tech walls:

1. **Heavy Nutrition & Incubation Sunk Cost:**
   - A construct requires a 20-day incubation pipeline (4 days embryo synthesis + 16 days vat maturation to age 13).
   - The vat consumes 10 nutrition/day (200 nutrition total = ~400 raw food/meat). Feeding 3 crew members _plus_ an active vat in the early game places heavy strain on colony agriculture and hunting.
2. **Strict Production Throughput (1 Vat = Max 3 Constructs / Year):**
   - Unlike robot mods that build a mechanoid in 8 in-game hours, constructs cannot be panic-spammed during raids.
3. **Modest Baseline Capabilities:**
   - Early Base Constructs have flat skills (3–4) and **zero passions**. They excel at hauling, cleaning, basic mining, and basic guard duty, but cannot carry high-end research, masterwork crafting, or delicate surgeries.
4. **Power & Grid Vulnerability:**
   - The growth vat and synthesizer require constant electrical power (600W+). A brownout or power loss during incubation risks embryonic shock and cellular collapse.
5. **The Industrial Replacement Lifecycle (The Intended Fantasy):**
   - When `C-01` hits age 22–25 and begins developing major cellular instability or carcinoma, the colony does not mourn ([`Trait_ConstructAsset`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/spec.md#L85)).
   - The failing unit is decommissioned/euthanized, and the crew synthesizes fresh embryo replacement `C-03` using standard organic supplies (meat, crops, neutroamine, medicine). The player actively manages a practical, industrial turnover cycle.
6. **Irreversible Sunk Cost & Economic Risk (No Liquefaction Safety Net):**
   - Without a liquefaction mechanic to refund biomass, every synthesized construct represents a true, non-refundable material investment.
   - If a synthesis bill fails, a power loss aborts incubation, or a construct dies in combat, the meat, crops, medicine, and neutroamine are permanently forfeit.
   - This prevents infinite recycling and forces the player to maintain active hunting, farming, and trading supply chains to sustain their synthetic crew.

---

## 7. Phased Implementation Roadmap

1. **Phase 1: Tech Tree Splitting [COMPLETED]**
   - Split `ResearchProjects_Constructs.xml` into Tier 1 (Field Bioproduction, Industrial, 600 pts) and Tier 2 (Construct Genome Architecture, Ultra-tech, 1600 pts).
2. **Phase 2: Hardware & Bioware Defs [COMPLETED]**
   - Implemented conditional `Patches/Odyssey/Patch_ShipHardware.xml` (`ShipConstructSynthesizer` 2x1, `CompactGrowthVat` 2x1 with imprinter).
   - Implemented conditional `Patches/Odyssey/Patch_GravBioware.xml` (`Construct_GravOpsPackage`, `Construct_SalvagePackage` with items, craft recipes, and surgical recipes).
3. **Phase 3: Starting Scenario & Preset [COMPLETED]**
   - Created `Defs/ScenarioDefs/Scenario_GravshipVanguard.xml` (The Gravship Vanguard).
   - Configured `Defs/XenotypeDefs/Xenotype_Construct.xml` (`Construct_Base`) and `Defs/PawnKindDefs_Humanlikes/PawnKinds_Construct.xml` (`Construct_Colonist`).
   - Extended `Patch_ConstructNaming.cs` to apply baseline skills and passions wipe upon generation.
4. **Phase 4: Odyssey Compatibility Patches [COMPLETED]**
   - Guarded all Odyssey gravship hardware and bioware in `Patches/Odyssey/` under `<Operation Class="PatchOperationFindMod">` matching `Ludeon.RimWorld.Odyssey`.
   - Updated `<loadAfter>` in `About.xml` and guidelines in `AGENTS.md`.
