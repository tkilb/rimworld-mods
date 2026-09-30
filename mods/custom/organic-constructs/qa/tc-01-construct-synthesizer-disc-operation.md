# TC-01: Construct Synthesizer & Disc Operation

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
