# TC-03: Growth Vat Neuro-Imprinting

- **Procedure:**
  1. Build or spawn a standard vanilla Growth Vat (`GrowthVat` via **Architect > Biotech** or Dev mode **Spawn Thing**). Note: `CompGrowthVatImprinter` is a component automatically patched onto all vanilla Growth Vats.
  2. With the Growth Vat selected, click the **Load Neural Disc** gizmo to order a pawn to load an encoded `NeuralBlueprintDisk` (or test with no disc loaded).
  3. Load a construct embryo (`HumanEmbryo` with `CompEmbryoQuality.isConstruct = true`) into the Growth Vat.
  4. Observe embryo phase (completes in 4 days).
  5. Observe construct form emergence directly inside the vat as occupant (never drops as baby).
  6. Attempt to eject before Age 13.
  7. Allow the construct to incubate to Age 13 (reaches neural maturity on Day 20).
  8. Verify no Growth Moment dialogs or letters appear at ages 7, 10, or 13.
  9. Eject at Age 13 and inspect skills, traits, and passions.
  10. In a separate test run, select a Growth Vat incubating an immature construct (or embryo) and click the **Reclaim Biomass** gizmo. Confirm the prompt.
- **Expected:**
  - Embryo phase accelerates to 4 days.
  - At Day 4, the construct body stabilizes inside the vat fluid as occupant without dropping to the ground.
  - The construct visually grows inside the vat across Days 4–20 from age 0 to 13.
  - The "Eject" command is disabled prior to Age 13 (*"Construct neural architecture is immature. Early decanting prior to age 13 causes fatal cellular dissolution."*).
  - No Growth Moment letters or modals appear (no random traits gained; passions strictly locked to `None`).
  - At Day 20 (Age 13), inspect string displays `Neural Maturity Reached (Age 13) - Ready to decant` and positive notification fires.
  - Decanted construct can immediately perform Doctoring, Construction, Smithing, Research, Mining, and combat.
  - Decanted construct inherits mentor skills (or baseline reflexes if no disc) with zero passions.
  - The **Reclaim Biomass** gizmo terminates gestation with confirmation, dissolving the construct and spawning a stack of `GeneticNutrientPaste` returning $\sim 75\%$ of all nutrition invested to date.
