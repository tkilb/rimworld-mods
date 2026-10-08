# TC-10: Bioware Augment Salvage & Storage Category

- **Procedure:**
  1. Inspect storage settings on standard shelves and stockpiles:
     - Verify `organic constructs` (`Construct_Augments`) appears directly under `Manufactured`.
     - Confirm that bioware augment packages and blueprint discs can be stored on shelves enabled for Manufactured items.
  2. Spawn or craft 1x Basic Bioware Augment (e.g. Basic Biotech Package).
  3. At a Construct Synthesizer, Fabrication Bench, or Machining Table, queue `salvage basic bioware augment`.
     - Assign a crafter to complete the bill.
     - Verify **1x ComponentIndustrial** and **7x Plasteel** are produced upon completion (all neutroamine/medicine/organics consumed).
  4. Spawn or craft 1x Intermediate Bioware Augment (e.g. Intermediate Biotech Package).
  5. Queue `salvage intermediate bioware augment`.
     - Verify **1x ComponentSpacer** (Advanced Component) and **15x Plasteel** are produced.
  6. Spawn or craft 1x Advanced Bioware Augment (e.g. Advanced Biotech Package).
  7. Queue `salvage advanced bioware augment`.
     - Verify **2x ComponentSpacer** (Advanced Components) and **25x Plasteel** are produced.
- **Expected:**
  - Storage filters correctly categorize all 12 bioware augment packages under `Manufactured > bioware augments`.
  - Shelves accept and store bioware augment packages.
  - Salvaging successfully reclaims 100% of components and 50% of plasteel across all tiers, while consuming organic substrate and chemicals.
