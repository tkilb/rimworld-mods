# TC-03: Growth Vat Neuro-Imprinting

- **Procedure:**
  1. Build or spawn a standard vanilla Growth Vat (`GrowthVat` via **Architect > Biotech** or Dev mode **Spawn Thing**). Note: `CompGrowthVatImprinter` is a component automatically patched onto all vanilla Growth Vats, not a separate building or item.
  2. With the Growth Vat selected, verify the inspect pane displays `Neural Imprinter: None (Empty...)` and click the **Load Neural Disc** gizmo to order a pawn to load an encoded `NeuralBlueprintDisk`.
  3. Gestate and decant a construct embryo in the Growth Vat.
  4. Gestate and decant a construct embryo in a Growth Vat without a neural blueprint disc loaded.
- **Expected:**
  - Growth Vat inspect pane shows the loaded disc doctrine title once loaded.
  - With mentor disc: Decants with skills directly matching the encoded disc (50% of mentor's skills rounded up, zero passions).
  - Without disc: Decants with baseline reflexes (Shooting 4, Melee 4, Social 2, Intellectual 2, Artistic 0, Others 3; Passions: None). Note: Verify skills when aged to 10+ (or adult) so vanilla Biotech childhood work restrictions (such as Construction unlocking at age 10) do not mask them as incapable/0.
