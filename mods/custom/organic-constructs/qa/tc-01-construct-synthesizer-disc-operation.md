# TC-01: Construct Synthesizer & Disc Operation

- **Procedure:**
  1. Build `ConstructSynthesizer` and connect electrical power (600 W).
  2. Spawn or craft a `GenomeBlueprintDisk` containing genetic data.
  3. Load disc into synthesizer via gizmo or right-click hauling.
  4. Ensure a colonist with Intellectual ≥ 6 and Medicine ≥ 4 is assigned to Doctor or Research work type.
  5. Provide synthesis ingredients on map: 40 MeatRaw, 40 PlantFoodRaw, 10 Neutroamine, 2 Medicine.
  6. Bill: Synthesize blank embryo (fresh organics).
  7. Bill: Batch imprint construct caste using loaded blueprint (uses synthesized blank embryo).
- **Expected:**
  - Disc is held in `discContainer` and inspect string shows loaded blueprint info.
  - Blank embryo is generated with no parents and `(construct matrix)` label.
  - Batch imprinting transfers blueprint genes to embryo without consuming the master disc.
