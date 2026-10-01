# TC-09: Complex Embryo Synthesis & Stability

- **Procedure:**
  1. Build a powered `ConstructSynthesizer` (600 W).
  2. Setup Scenario A (Missing Disc):
     - Without loading any disc, add bill: `Recipe_SynthesizeComplexEmbryoFresh` ("synthesize complex construct embryo (fresh organics)").
     - Order a doctor/researcher pawn to prioritize working the bill.
  3. Setup Scenario B (Stable Complex Genome):
     - Load a burned `GenomeBlueprintDisk` with high stability (≥ 80%) into the synthesizer.
     - Provide synthesis ingredients (40 MeatRaw, 40 PlantFoodRaw, 10 Neutroamine, 2 Medicine).
     - Order pawn to synthesize complex embryo.
  4. Setup Scenario C (Highly Volatile Genome & Collapse):
     - Dev Mode spawn or burn a `GenomeBlueprintDisk` with high negative metabolism / high complexity (Stability < 50%).
     - Load volatile disc into synthesizer and synthesize complex embryo.
     - Observe synthesis completion or collapse into `GeneticNutrientPaste`.
  5. Setup Scenario D (Growth Vat Decant Defects):
     - Insert a complex construct embryo generated from a volatile template (< 60% stability) into a `GrowthVat`.
     - Accelerate gestation using Dev Mode and trigger decant (`FinishGestation`).
- **Expected:**
  - **Scenario A:** Prioritization is rejected immediately with float menu message: `No genome blueprint disc loaded in synthesizer.` (via `Patch_WorkGiver_DoBill`).
  - **Scenario B:** Bill completes successfully. Spawns `HumanEmbryo` marked `isConstruct = true` carrying blueprint's genes, locked core construct genes, bald/beardless traits, and inspect string showing origin blueprint.
  - **Scenario C:** With low stability, botch chance triggers based on stability, doctor skill, and room cleanliness. On botch, synthesis fails cleanly, producing 1x `GeneticNutrientPaste` and a negative notification message (`Synthesis botched! Genetic instability caused embryo structure to collapse`).
  - **Scenario D:** Volatile embryos decanting from Growth Vat roll a 15% chance of incurring `Construct_Assimilation` or `CryptosleepSickness` with an alert message regarding cellular defects.
