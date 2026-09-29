# Gene Splicer - QA Test Plan & Verification Log

## Test Cases

### TC-GS-01: Bench Construction & Facility Linking
- **Procedure:** Spawn or build `EmbryoSplicingBench`. Place adjacent vanilla `GeneBank` and `GeneProcessor`.
- **Expected:**
  - Bench links to facility with visual connection lines.
  - Linked genepacks are queryable by the bench.

### TC-GS-02: Embryo Loading & Ejection
- **Procedure:**
  1. Have a `HumanEmbryo` on the ground.
  2. Select bench -> Click "Load Embryo" gizmo -> Select embryo from float menu.
  3. Verify a hauler carries and loads the embryo into the bench.
  4. Test right-click float menu on pawn: "Load designated embryo".
  5. Click "Eject Embryo" gizmo.
- **Expected:**
  - Embryo is stored safely in `embryoContainer`.
  - Bench inspect string shows contained embryo and current edits (0/2).
  - On eject, embryo drops to interaction cell unharmed.

### TC-GS-03: Splicing UI & Gene Staging
- **Procedure:**
  1. Load embryo into bench. Click "Edit Genes".
  2. Stage adding a gene from connected Gene Banks.
  3. Attempt to remove a gene that does NOT exist in connected Gene Banks.
  4. Attempt to stage more than 2 total operations.
- **Expected:**
  - Unrepresented genes cannot be removed (template requirement).
  - Adding beyond 2 operations is blocked ("Cap reached (2/2)").
  - Clicking "Commit Order" sets pending order on bench and updates inspect string.

### TC-GS-04: Doctor Splicing Job & Botch Mechanics
- **Procedure:**
  1. With staged order pending, order a doctor to perform gene splicing (or allow auto-workgiver).
  2. Doctor works at bench for duration.
  3. Test outcome on success.
  4. Force failure (e.g. via low skill or filthy room).
- **Expected:**
  - On success: Staged genes added/removed on embryo, `editCount` incremented, positive message displayed.
  - On failure: Embryo destroyed, 1× `GeneticNutrientPaste` spawned, crunch sound plays, negative event letter displayed.

### TC-GS-05: Parental Thoughts
- **Procedure:** Successfully edit an embryo whose biological parents are active colonists.
- **Expected:**
  - Baseline parents receive `ChildGeneticallyAltered` (-8 mood, 10 days).
  - Transhumanist/Body Modder parents receive `ChildBiologicallyUpgraded` (+6 mood, 10 days).

---

## Build Verification
- **Status:** PASS
- **Target:** .NET Framework 4.7.2 (Assemblies/GeneSplicer.dll)
- **Compiler Result:** 0 Errors, 0 Warnings.
