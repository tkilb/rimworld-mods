# Eugenics Program: Dev-Mode QA Runbook & Verification Guide

This runbook provides complete, step-by-step instructions for testing and verifying all systems in the **Eugenics Program** mod using RimWorld's built-in **Development Mode**.

No gameplay grinding or lengthy research progression is required. Following this guide takes approximately 10–15 minutes.

---

## 1. Pre-Flight: Verify Mod Deployment

Before launching RimWorld, ensure the compiled assembly and defs are symlinked into your game's `Mods` folder:

1. Confirm mod state:
   ```bash
   make status
   ```
2. If `eugenics-program` is not active or linked, deploy it:
   ```bash
   make link
   ```

---

## 2. Launch & Fast Test Colony Setup (Under 60 Seconds)

### Step 1: Enable Development Mode

1. In RimWorld, go to **Options** -> **General** (right-hand column).
2. Check **Development mode**.
3. _Notice the top bar:_ A row of small debug icons will appear at the top-center of the screen:
   - **Open the log (`~`):** Shows console errors/warnings.
   - **Open debug view settings:** Visual overlays and toggles.
   - **Open debug inspector:** Deep inspection of game objects.
   - **Open debug actions menu (gear / play icon):** Spawn items, finish research, and trigger events.
   - **Toggle god mode (face icon or `T`):** Instant 0-cost construction.

### Step 2: Spin Up a Test Colony

- **Method A (Instant Map via Main Menu):** With Dev Mode enabled, the main menu displays a **Quick test** button in the lower area that generates a 3-colonist flat map instantly.
- **Method B (Standard Fast Start):** Click **New Colony** -> **Crashlanded** -> choose any storyteller -> click any tile on the world map -> start with default colonists.

---

## 3. Dev Mode QA "Cheat Sheet"

| Icon / Hotkey                 | Tool                   | What It Does & How to Use It                                                                                                                                                                                         |
| :---------------------------- | :--------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Face Icon** (or `T`)        | **God Mode**           | **Instant 0-cost building.** With God Mode checked, opening the **Architect** menu lets you place any building (even locked structures like the `EmbryoSplicingBench`) immediately with zero cost and no pawn labor. |
| **Gear / Play Icon**          | **Debug Actions Menu** | The main cheat menu. Contains search filters for spawning items, modifying pawns, finishing research, and passing time.                                                                                              |
| **`4` Key**                   | **Super Speed (4x)**   | Dev mode unlocks 4x game speed. Press `4` on the keyboard to fast-forward incubation, scanning, or crafting.                                                                                                         |
| **First Icon (Log)** (or `~`) | **Debug Log**          | Opens the console. Keep an eye out for red messages during mod actions.                                                                                                                                              |

---

## 4. Essential Debug Actions to Cheat In What You Need

Open the **Debug actions menu** (gear icon) and type the following in the top search filter:

### 4.1 Instant Tech Unlocks

- Search: `finish` -> click **Finish all research**.
- _Result:_ Instantly unlocks all vanilla Biotech and Eugenics projects (`Eugenics_ConstructFoundations`, `Eugenics_GeneOptimization`, `Eugenics_NeuralImprinting`, `Eugenics_GeneticDiagnostics`).

### 4.2 Spawning Items & Resources

- Search: `spawn thing` -> click **Spawn thing...**.
- Type any of the following and click on the map to drop:
  - `NeuralBlueprintDisk` (Blank disc for neural scans)
  - `GenomeBlueprintDisk` (Master matrix genome blueprint disc)
  - `HumanEmbryo` (Fresh human embryo for splicing and vat gestation)
  - `VanometricPowerCell` (Instant free wireless power)
  - `Filth_Dirt` / `Filth_Trash` (Used to test room cleanliness penalties)

### 4.3 Boosting Pawn Skills (Doctor / Researcher)

- Search: `set skill` -> click **Set skill...** -> click your pawn -> choose `Medicine` or `Intellectual` -> enter `15` or `20`.
- Search: `max needs` -> click **Fill all needs** to keep test pawns from breaking or sleeping during testing.

### 4.4 Fast-Forwarding Work & Time

- Search: `pass` -> click **Pass 1 hour** or **Pass 1000 ticks** to instantly advance vat gestation or scanning times.

---

## 5. Step-by-Step Test Flows

### Test Flow 1: Neural Scanner & Imprint Doctrines

**Objective:** Verify that `Building_NeuralScanner` accepts a veteran colonist and encodes their synaptic skills onto a `NeuralBlueprintDisk`.

1. **Build Scanner:**
   - In the architect menu under **Biotech**, select **Neural Scanner** and place it (instant with God Mode).
   - Place a **Vanometric power cell** adjacent to power it.
2. **Spawn Disc:**
   - Debug menu -> `Spawn thing...` -> `NeuralBlueprintDisk`. Click near the scanner.
3. **Load Disc:**
   - Click the **Neural Scanner** -> click gizmo **Load Imprint Disc** -> select the blank disc.
   - _Inspection Check:_ Pane shows `Ready for subject (Disc: Neural Imprint Disc)`.
4. **Scan Colonist:**
   - Click gizmo **Select Subject to Scan** -> choose any colonist.
   - Colonist enters the pod. Fast-forward with `4` key or debug `Pass 1 hour`.
5. **Verify Output:**
   - Colonist decants with the `Neural Fatigue` .
   - Disc drops encoded. Select the disc and click **Inspect Imprint Doctrine** to view the recorded skills and passions.

---

### Test Flow 2: Growth Vat Imprinting Engine

**Objective:** Verify that `CompGrowthVatImprinter` attached to vanilla `Building_GrowthVat` streams doctrine skills and passions into a gestating construct pawn.

1. **Build Growth Vat:**
   - Architect menu -> **Biotech** -> place **Growth vat** (instant with God Mode) and connect power.
2. **Load Imprint Disc:**
   - Click the growth vat.
   - Verify the custom gizmo **Load Neural Imprint Disc** provided by `CompGrowthVatImprinter` is present.
   - Click the gizmo and load the encoded disc from Test Flow 1.
3. **Insert Embryo:**
   - Debug menu: `Spawn thing...` -> `HumanEmbryo`.
   - Right-click vat with a doctor to insert embryo, or debug-force gestation.
4. **Accelerate Gestation:**
   - Use debug actions (`Pass 1 hour` / `Pass ticks`) or fast-forward at 4x speed.
5. **Verify Decanted Pawn:**
   - When the pawn emerges, check the **Bio** tab:
     - Verify passions and skill levels match or exceed the doctrine disc's values.
     - Verify trait `Construct Asset` or situational thought `Construct: Cold Efficiency` if construct genes were present.

---

### Test Flow 3: Dedicated `EmbryoSplicingBench` & Batch Automation

**Objective:** Verify that `EmbryoSplicingBench` functions as a native work table with bills, holds a reusable master genome blueprint disc, and executes batch splicing.

1. **Bench Placement & Disc Loading:**
   - Architect menu -> **Biotech** -> place **Embryo Splicing Bench** (3x2 multi-tile bench) and connect power.
   - Debug menu: `Spawn thing...` -> `GenomeBlueprintDisk`.
   - Click the bench -> notice inspect pane: `No Genome Blueprint Disc loaded (Required for batch genome imprinting)`.
   - Click gizmo **Load Blueprint Disc** -> select your spawned disc.
   - _Verification:_ Inspect pane updates to: `Loaded Blueprint: [Caste Name] ([N] genes, Cpx: [X], Met: [Y])`.
2. **Batch Splicing Bill:**
   - Click the bench -> **Bills** tab -> **Add bill**.
   - Verify all 3 batch recipes appear:
     - `Splice embryo with loaded blueprint disc`
     - `Screen embryo genetics`
     - `Liquefy embryo biomass`
   - Add bill: `Splice embryo with loaded blueprint disc` and set count to `3`.
   - Debug menu: `Spawn thing...` -> 3x `HumanEmbryo` nearby.
   - Assign/prioritize a researcher pawn to work at the bench.
3. **Verify Batch Execution:**
   - Colonist carries embryo to the bench and performs splicing work.
   - _Result Check:_
     - Embryo updates with blueprint endogenes in-place.
     - Notification appears: `Batch splicing complete: Embryo spliced with '...'`.
     - The master `GenomeBlueprintDisk` **remains inside the bench container** and is NOT consumed or dropped.
     - Colonist immediately moves on to the next embryo in the batch.

---

### Test Flow 4: Prenatal Diagnostics & Biomass Recycling

**Objective:** Verify that room cleanliness penalties attach defects to `CompEmbryoQuality` and screening/recycling bills process them.

1. **Dirty Room Defect Test:**
   - Debug menu: `Spawn filth...` -> `Filth_Dirt` around the bench.
   - Run the `Splice embryo with loaded blueprint disc` bill on an embryo in the dirty room.
   - Splicing rolls the cleanliness penalty and attaches a defect to `CompEmbryoQuality`.
2. **Screening Bill:**
   - Bench -> **Bills** tab -> add `Screen embryo genetics`.
   - Assign a doctor pawn to screen the embryo.
   - _Result Check:_ Notification appears:
     `Genomic screening complete: Defects identified (...). Recommended for biomass liquefaction.`
   - Select the embryo -> verify **Defect Report** gizmo appears detailing the cellular diagnosis.
3. **Biomass Recycling Bill:**
   - Bench -> **Bills** tab -> add `Liquefy embryo biomass`.
   - Doctor hauls defective embryo to the bench and liquefies it.
   - _Result Check:_
     - Embryo is consumed/destroyed.
     - 3x `GeneticNutrientPaste` meals spawn.
     - Notification: `Embryo biomass liquefied into Genetic Nutrient Paste.`

---

### Test Flow 5: Native Blank Matrix Synthesis (No Cloning Mods Needed)

1. Go to `EmbryoSplicingBench` -> **Bills** -> Add `Synthesize Blank Embryo (Fresh Organics)`.
2. Haul 40 Meat, 40 Plants, 10 Neutroamine, and 2 Medicine.
3. Doctor crafts the bill.
4. _Result Check:_ A `HumanEmbryo` spawns with `Father = null` and `Mother = null`. Inspect string confirms: *"Origin: Synthetic Construct Matrix"*.
5. Third-party cloning mods (`zal.cloning`) are **not required**.

---

## 6. Summary Checklist for QA Sign-Off

- [ ] Dev mode opens cleanly; all Eugenics research projects unlock via debug action.
- [ ] `NeuralScanner` accepts colonists, records skills, causes neural fatigue, and encodes `NeuralBlueprintDisk`.
- [ ] `CompGrowthVatImprinter` loads imprint disc, streams skills/passions during vat acceleration.
- [ ] `EmbryoSplicingBench` builds cleanly, connects to `GeneBank` facilities, and holds a loaded master disc.
- [ ] `Eugenics_BatchApplyBlueprint` bill processes multiple embryos sequentially without consuming the master disc.
- [ ] Spliced embryos receive blueprint endogenes in-place while preserving parents and cloning tags.
- [ ] `Eugenics_BatchScreenEmbryo` detects defects and updates inspect status.
- [ ] `Eugenics_BatchRecycleEmbryo` yields `GeneticNutrientPaste`.
- [ ] No red error messages in the debug log (`~`).
