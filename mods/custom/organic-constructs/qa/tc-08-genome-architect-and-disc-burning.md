# TC-08: Genome Architect & Disc Burning

- **Procedure:**
  1. Build a `ConstructGenomeArchitect` and connect electrical power (800 W).
  2. Build 1-2 powered `GeneBank`s within 16 cells containing diverse `Genepack`s (e.g., `FirefoamSpray`, `Superfast`, `Delicate`).
  3. Ensure a blank `GenomeBlueprintDisk` is available on the map.
  4. Select the `ConstructGenomeArchitect` and verify inspect string displays `Connected Gene Banks: X`.
  5. Click gizmo `Load Blank Disc` and select a colonist to haul the blank disc.
  6. Once loaded, click gizmo `Configure Genome`.
  7. In the `Dialog_ConfigureConstructGenome` window:
     - Verify Core Construct Genes (`Gene_ConstructPsychology`, `Construct_MetabolicallyEfficient`, `Gene_MandatorySterility`, `Gene_ConstructHibernation`) are locked in the left panel.
     - Search and select 2-3 adaptation genes from the right panel.
     - Observe live updates to Complexity, Net Metabolism, and Genome Stability readout.
     - Enter a custom template name (e.g., "Assault Specialist").
     - Click `Burn Master Disc`.
- **Expected:**
  - `Configure Genome` gizmo is disabled if the machine is unpowered or if loaded disc is already burned.
  - The genome configuration window opens smoothly without UI clipping or errors.
  - Clicking `Burn Master Disc` plays a confirmation sound and prints a positive notification banner.
  - The disc container now shows the disc as burned: `Loaded Disc: Genome blueprint disc (Burned: Assault Specialist)`.
  - Disc is permanently ROM-locked (`isBurned = true`); `Configure Genome` is no longer available on this disc.
  - Ejecting the disc reveals inspect string displaying the custom template name and full encoded gene list.
