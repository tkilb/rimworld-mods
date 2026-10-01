# TC-07: Blank Genome Disc Fabrication

- **Procedure:**
  1. Build a `TableFabrication` (Fabrication bench) with power connected.
  2. Research `Construct_Foundations` (or unlock via Dev Mode).
  3. Ensure a colonist with Crafting ≥ 8 is available.
  4. Place ingredients near bench: 15 Plasteel, 2 ComponentIndustrial, 1 ComponentSpacer.
  5. Add bill: `Craft_BlankGenomeDisc` ("craft blank genome blueprint disc").
  6. Order the crafter to prioritize working the bill to completion.
- **Expected:**
  - Bill produces 1x `GenomeBlueprintDisk`.
  - Disc inspect string displays: `Blank Genome Disc (Requires burning at Genome Architect)`.
  - Disc does NOT contain any pre-loaded genes (Complexity: 0, Net Metabolism: 0).
  - Disc gizmo `Record Construct Genome` is available if spawned on ground, but disc cannot yet be used to synthesize Complex Construct embryos.
