# Organic Constructs - QA Test Plan & Verification Log

## Test Cases

### TC-OC-01: Construct Synthesizer & Disc Operation
- **Procedure:**
  1. Build `ConstructSynthesizer`.
  2. Spawn or craft a `GenomeBlueprintDisk`.
  3. Load disc into synthesizer via gizmo or right-click hauling.
  4. Bill: Synthesize blank embryo (fresh organics).
  5. Bill: Batch imprint construct caste using loaded blueprint.
- **Expected:**
  - Disc is held in `discContainer` and inspect string shows loaded blueprint info.
  - Blank embryo is generated with no parents and `(construct matrix)` label.
  - Batch imprinting transfers blueprint genes to embryo without consuming the master disc.

### TC-OC-02: Neural Scanner Pod
- **Procedure:**
  1. Build `NeuralScanner` and supply power.
  2. Insert a blank `NeuralBlueprintDisk`.
  3. Select a natural colonist and order them to enter the scanner.
  4. Attempt to order a construct to enter the scanner.
- **Expected:**
  - Colonist is scanned over 15,000 ticks. Upon completion, disc is encoded and ejected, and donor gains `Construct_NeuralFatigue`.
  - Constructs are explicitly blocked from entering the scanner with an informative rejection message.

### TC-OC-03: Growth Vat Neuro-Imprinting
- **Procedure:**
  1. Insert an encoded `NeuralBlueprintDisk` into a vanilla Growth Vat equipped with `CompGrowthVatImprinter`.
  2. Gestate and decant a construct embryo.
  3. Gestate and decant a construct embryo without an imprinter disc.
- **Expected:**
  - With mentor disc: Decants with 50% of mentor's skills and all passions forced to None.
  - Without disc: Decants with baseline reflexes (Shooting 4, Melee 4, Social 2, Intellectual 2, Artistic 0, Others 3; Passions: None).

### TC-OC-04: Construct Biology & Locked Genome
- **Procedure:**
  1. Attempt to perform "Implant Xenogerm" surgery on an active construct.
  2. Test romance attempts involving a construct.
  3. Kill a construct pawn in colony view.
- **Expected:**
  - Xenogerm surgery is disabled ("Cannot implant xenogerm: Synthetic construct genetic architecture is permanently locked").
  - Romance attempts have 0 selection weight.
  - Colonists suffer no "Colonist died" or "Colonist lost" thoughts.

### TC-OC-05: Construct Stasis & Maintenance
- **Procedure:**
  1. Have construct operate past warning ticks or trigger "Enter Stasis" gizmo.
  2. Test clean wake after 12 hours.
  3. Manually interrupt stasis before 12 hours.
- **Expected:**
  - Clean wake: Operating ticks reset, rest maxed, positive message.
  - Interrupted stasis: Gains `Construct_InterruptedStasis` (consciousness/moving penalties).

### TC-OC-06: Bioware Augment Grafting
- **Procedure:**
  1. Attempt to graft a bioware augment to a baseline colonist.
  2. Graft bioware augment to a construct.
- **Expected:**
  - Baseline human rejected ("Incompatible biological neural bus").
  - Construct accepts surgery; enters assimilation coma (`Construct_Assimilation`).

---

## Build Verification
- **Status:** PASS
- **Target:** .NET Framework 4.7.2 (Assemblies/OrganicConstructs.dll)
- **Compiler Result:** 0 Errors, 0 Warnings.
