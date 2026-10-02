# TC-01: Construct Synthesizer & Disc Operation

- **Procedure:**
  1. Build `ConstructSynthesizer` and connect electrical power (600 W).
  2. Spawn or craft a `GenomeBlueprintDisk`. Ensure it is burned with genetic data (either configure via `ConstructGenomeArchitect`, or with Dev Mode active use gizmo `DEV: Burn Default Template`).
  3. Load disc into synthesizer via gizmo or right-click hauling (or burn while loaded using `DEV: Burn Default Template to Disc`).
  4. Ensure a colonist with Intellectual ≥ 6 and Medicine ≥ 4 is assigned to Doctor or Research work type.
  5. Provide synthesis ingredients on map: 40 MeatRaw, 40 PlantFoodRaw, 10 Neutroamine, 2 Medicine.
  6. Bill: Synthesize base construct embryo (fresh organics).
  7. Bill: Synthesize complex construct embryo (fresh organics).

- **Expected:**
  - Disc is held in `discContainer` and inspect string shows loaded blueprint info.
  - Base construct embryo is generated with no parents, locked construct baseline endogenes, and `construct embryo` label.
  - Complex construct embryo is generated directly imprinted with loaded blueprint genes without consuming the master disc.
  - Attempting to prioritize complex construct embryo synthesis when no disc is loaded is blocked with clear reason message.
