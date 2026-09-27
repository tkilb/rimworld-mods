# Eugenics Program: Dev-Mode QA Runbook & Verification Guide

This runbook provides complete, step-by-step instructions for testing and verifying all systems in the **Eugenics Program** mod using RimWorld's built-in **Development Mode**.

You do **not** need to play through hours of colony progression or research to verify features. Following this guide takes approximately 10–15 minutes.

---

## 1. Enabling RimWorld Development Mode

1. Launch RimWorld and load any existing test save or start a new **Crashlanded** game.
2. Press **Esc** and click **Options**.
3. In the right-hand column, check **Development mode**.
4. You will immediately see a row of small icons at the top-center of the screen:
   - **Open the log:** Shows console errors/warnings.
   - **Open debug actions menu (gear / play icon):** Used to spawn items, finish research, and trigger events.
   - **Open debug view settings:** Visual overlays and toggles.
   - **Toggle god mode (face icon):** When active, buildings are constructed instantly at zero cost without pawns.

---

## 2. Quick Prep: Instant Research & God Mode

1. Turn on **God Mode** by clicking the face icon in the top debug bar (a checkmark appears).
2. Open the **Debug actions menu** (gear icon).
3. In the search filter box at the top, type `finish` and click **Finish all research**.
   - *Result:* All tech tree projects, including `Eugenics_ConstructFoundations`, `Eugenics_GeneOptimization`, `Eugenics_NeuralImprinting`, and `Eugenics_GeneticDiagnostics`, are unlocked.

---

## 3. Test Flow 1: Neural Scanner & Imprint Doctrines

### Objective
Verify that `Building_NeuralScanner` accepts a veteran colonist and encodes their synaptic skills onto a `NeuralBlueprintDisk`.

1. **Build Scanner:**
   - In the architect menu under **Biotech**, select **Neural Scanner** and place it.
   - Place a **Vanometric power cell** (or solar/generator) adjacent to power it.
2. **Spawn Disc:**
   - Open **Debug actions menu** -> search `spawn thing` -> select **Spawn thing...**.
   - Type `NeuralBlueprintDisk` and click anywhere near the scanner to drop it.
3. **Load Disc:**
   - Click the **Neural Scanner**.
   - Click the gizmo **Load Imprint Disc** -> select the spawned blank disc.
   - *Inspection Check:* The inspect pane updates to `Ready for subject (Disc: Neural Imprint Disc)`.
4. **Scan Colonist:**
   - Click the gizmo **Select Subject to Scan** -> choose any colonist.
   - The colonist enters the scanner pod.
   - Fast-forward game speed (or click **Pass 1 hour / Pass ticks** in debug menu).
5. **Verify Output:**
   - When scan completes, colonist decants with the `Neural Fatigue` hediff.
   - The disc drops out encoded. Click the disc and click **Inspect Imprint Doctrine** to view the recorded skills and passions.

---

## 4. Test Flow 2: Growth Vat Imprinting Engine

### Objective
Verify that `CompGrowthVatImprinter` attached to vanilla `Building_GrowthVat` streams doctrine skills and passions into a gestating construct pawn.

1. **Build Growth Vat:**
   - Architect menu -> **Biotech** -> place **Growth vat** (instant with God Mode) and connect power.
2. **Load Imprint Disc:**
   - Click the growth vat.
   - Observe the new **Load Neural Imprint Disc** gizmo added by `CompGrowthVatImprinter`.
   - Click the gizmo and load the encoded disc from Test Flow 1.
3. **Insert Embryo:**
   - Spawn a `HumanEmbryo` via debug menu: `Spawn thing...` -> `HumanEmbryo`.
   - Right-click vat with doctor to insert embryo, or debug-force gestation.
4. **Accelerate Gestation:**
   - Select the vat and use debug actions (or wait a few vat ticks).
5. **Verify Decanted Pawn:**
   - When the pawn emerges, check the **Bio** tab:
     - Verify passions and skill levels match or exceed the doctrine disc's values.
     - Verify trait `Construct Asset` or situational thought `Construct: Cold Efficiency` if construct genes were present.

---

## 5. Test Flow 3: Dedicated `EmbryoSplicingBench` & Batch Automation

### Objective
Verify that the `EmbryoSplicingBench` functions as a native work table with bills, holds a reusable master genome blueprint disc, and executes batch splicing.

### 5.1 Bench Placement & Disc Loading
1. In architect menu -> **Biotech**, select **Embryo Splicing Bench** (3x2 multi-tile visual clone of Gene Assembler) and place it.
2. Connect power.
3. Spawn a `GenomeBlueprintDisk` via `Spawn thing...` -> `GenomeBlueprintDisk`.
4. In the disc's inspect gizmos, click **Inspect Blueprint** to check its encoded caste genes.
5. Click the **Embryo Splicing Bench**:
   - Observe the inspect pane shows: `No Genome Blueprint Disc loaded (Required for batch genome imprinting)`.
   - Click the gizmo **Load Blueprint Disc** -> select your spawned disc.
   - *Verification:* The inspect pane now shows:
     `Loaded Blueprint: [Caste Name] ([N] genes, Cpx: [X], Met: [Y])`.

### 5.2 Batch Splicing Bill
1. Click the bench -> click the **Bills** tab.
2. Click **Add bill**:
   - Verify all 3 batch recipes appear:
     - `Splice embryo with loaded blueprint disc`
     - `Screen embryo genetics`
     - `Liquefy embryo biomass`
3. Add the bill: `Splice embryo with loaded blueprint disc`.
   - Set to "Do 3 times".
4. Spawn 3x `HumanEmbryo` nearby via `Spawn thing...` -> `HumanEmbryo`.
5. Draft an Intellectual/Researcher pawn, undraft them near the bench, and prioritize working at the bench (or let work priorities handle it).
6. **Verify Batch Splicing:**
   - Colonist carries embryo to the bench and performs splicing work.
   - *Result Check:*
     - The embryo is updated with the blueprint's endogenes.
     - A notification appears: `Batch splicing complete: Embryo spliced with '...'`.
     - The master `GenomeBlueprintDisk` **remains inside the bench container** and is NOT consumed or dropped.
     - Colonist immediately moves on to the next embryo in the batch.

---

## 6. Test Flow 4: Prenatal Diagnostics & Biomass Recycling

### Objective
Verify that low room cleanliness introduces defects and that screening reveals them for recycling.

1. **Dirty Room Test:**
   - Move or build an `EmbryoSplicingBench` in a dirty room (or spawn animal filth via `Spawn filth...` -> `Filth_Dirt`).
   - Run the `Splice embryo with loaded blueprint disc` bill on an embryo.
   - Splicing in a dirty lab rolls the room cleanliness penalty and attaches a defect to `CompEmbryoQuality`.
2. **Screening Bill:**
   - On the bench, add bill: `Screen embryo genetics`.
   - Assign a Doctor pawn.
   - Doctor performs genomic screening.
   - *Result Check:* An alert or message pops up:
     `Genomic screening complete: Defects identified (...). Recommended for biomass liquefaction.`
   - Select the embryo -> verify **Defect Report** gizmo appears detailing the cellular diagnosis.
3. **Biomass Recycling Bill:**
   - Add bill: `Liquefy embryo biomass`.
   - Doctor hauls defective embryo to bench and liquefies it.
   - *Result Check:*
     - Embryo is destroyed.
     - 3x `GeneticNutrientPaste` meals spawn.
     - Notification: `Embryo biomass liquefied into Genetic Nutrient Paste.`

---

## 7. Test Flow 5: Ecosystem & Third-Party Mod Compatibility

### Biotech Cloning Continued (`zal.cloning`)
- If `Biotech Cloning Continued` is in your active mod list:
  1. Extract a clone embryo using `CloneExtractor`.
  2. Verify that `CompEmbryoQuality` is automatically attached to the cloned embryo.
  3. Load the cloned embryo into `EmbryoSplicingBench` batch bills; confirm it can be spliced, screened, or recycled identically to vanilla embryos.

---

## 8. Summary Checklist for QA Sign-Off

- [ ] Dev mode opens cleanly; all Eugenics research projects unlock via debug action.
- [ ] `NeuralScanner` accepts colonists, records skills, causes neural fatigue, and encodes `NeuralBlueprintDisk`.
- [ ] `CompGrowthVatImprinter` loads imprint disc, streams skills/passions during vat acceleration.
- [ ] `EmbryoSplicingBench` builds cleanly, connects to `GeneBank` facilities, and holds a loaded master disc.
- [ ] `Eugenics_BatchApplyBlueprint` bill processes multiple embryos sequentially without consuming the master disc.
- [ ] Spliced embryos receive blueprint endogenes in-place while preserving parents and cloning tags.
- [ ] `Eugenics_BatchScreenEmbryo` detects defects and updates inspect status.
- [ ] `Eugenics_BatchRecycleEmbryo` yields `GeneticNutrientPaste`.
- [ ] No red error messages in the debug log.
