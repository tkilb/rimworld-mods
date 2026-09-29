# Eugenics Program: Dev-Mode QA Runbook & Verification Guide

> [!IMPORTANT]
> **Target Environment: RimWorld 1.6 (Biotech)**
> All verification tests use RimWorld's built-in **Development Mode**. Following this runbook takes approximately 10–15 minutes and requires zero gameplay grinding.

---

## 1. Pre-Flight: Verify Mod Deployment

1. Confirm mod state:
   ```bash
   make status
   ```
2. If `eugenics-program` is not active or symlinked into your RimWorld `Mods` directory:
   ```bash
   make link
   ```
3. Verify compilation:
   ```bash
   dotnet build Source/EugenicsProgram.csproj
   ```

---

## 2. Fast Test Colony Setup (Under 60 Seconds)

1. In RimWorld, navigate to **Options** -> **General** -> check **Development mode**.
2. Start an instant testing environment:
   - **Method A:** Click **Quick test** in the main menu to instantly spawn a flat 3-colonist map.
   - **Method B:** **New Colony** -> **Crashlanded** -> choose any tile -> start with default colonists.
3. Dev Mode Controls:
   - **`T`** or **Face Icon:** Toggle **God Mode** (instant 0-cost construction).
   - **`4` Key:** Toggle **4x Super Speed**.
   - **`~`** or **First Icon:** Toggle **Debug Log** (inspect for red console errors).
   - **Gear / Play Icon:** Open **Debug Actions Menu**.

---

## 3. Dev-Mode Cheat Sheet & Spawning Catalog

Open the **Debug actions menu** (gear icon) and type into the search filter:

| Debug Action | Search Query | Usage |
| :--- | :--- | :--- |
| **Finish Research** | `finish` -> **Finish all research** | Unlocks all Eugenics tech and vanilla Biotech infrastructure. |
| **Spawn Items** | `spawn thing` -> **Spawn thing...** | Spawn blueprint discs, embryos, reagents, or augments. |
| **Boost Skill** | `set skill` -> **Set skill...** | Set doctor or researcher `Medicine` or `Intellectual` to 15–20. |
| **Pass Time** | `pass` -> **Pass 1 hour** / **Pass 1000 ticks** | Fast-forward vat gestation, surgery assimilation, and stasis cycles. |
| **Fill Needs** | `max needs` -> **Fill all needs** | Prevent test pawns from breaking or sleeping during QA runs. |

### Spawnable Def Names
- **Blueprint Discs:** `GenomeBlueprintDisk`, `NeuralBlueprintDisk`
- **Embryos & Biomass:** `HumanEmbryo`, `GeneticNutrientPaste`
- **Ingredients:** `Meat_Raw`, `Plant_Raw`, `Neutroamine`, `MedicineIndustrial`
- **Power & Utility:** `VanometricPowerCell`, `Filth_Dirt`
- **Bio-Augment Packages:**
  - *Combat:* `Eugenics_BasicCombatPackageItem`, `Eugenics_IntermediateCombatPackageItem`, `Eugenics_AdvancedCombatPackageItem`
  - *Medical:* `Eugenics_BasicMedicalPackageItem`, `Eugenics_IntermediateMedicalPackageItem`, `Eugenics_AdvancedMedicalPackageItem`
  - *Industrial:* `Eugenics_BasicIndustrialPackageItem`, `Eugenics_IntermediateIndustrialPackageItem`, `Eugenics_AdvancedIndustrialPackageItem`
  - *Laborer:* `Eugenics_BasicLaborerPackageItem`, `Eugenics_IntermediateLaborerPackageItem`, `Eugenics_AdvancedLaborerPackageItem`

---

## 4. Pillar 1: Germline Gene Editing & Social Dynamics

### Test Flow 1: 2-Edit Cap & Splicing Botch Collapse
**Objective:** Verify that embryos enforce a strict 2-gene edit limit and catastrophic failure collapses the embryo into 1× `GeneticNutrientPaste`.

1. **Setup Bench:**
   - Under Architect -> **Biotech**, place an **Embryo Splicing Bench** with God Mode (`T`) and attach power.
   - Spawn a `GenomeBlueprintDisk` and click gizmo **Load Blueprint Disc** on the bench.
2. **Edit Count Cap Check:**
   - Spawn a `HumanEmbryo`.
   - Inspect the embryo: verify the label shows `Genetic Edits: 0/2`.
   - Perform 1st gene edit via the bench bill or "Edit Genes" dialog -> verify `Genetic Edits: 1/2`.
   - Perform 2nd gene edit -> verify `Genetic Edits: 2/2`.
   - Attempt a 3rd edit:
     - In the bench bills, the bill reports the embryo as ineligible.
     - In the inspection gizmo, "Edit Genes" is disabled with the warning: *"Maximum gene edit limit reached (2/2 edits)."*
3. **Botch Splicing (Paste Collapse):**
   - Spawn filth (`Filth_Dirt`) around the bench to create a negative room cleanliness score.
   - Select a low-skill pawn (Medicine 0–2).
   - Order the pawn to splice an embryo.
   - *Result Check:* On a botch failure roll:
     - The embryo is immediately destroyed on the table.
     - Exactly **1× `GeneticNutrientPaste`** drops at the workstation.
     - Notification fires: *"Splicing failure: Embryonic cellular matrix collapsed into Genetic Nutrient Paste."*

### Test Flow 2: Parental Emotional Reactions
**Objective:** Verify biological parents react emotionally when a natural embryo's genetics are edited.

1. **Natural Embryo Preparation:**
   - Take or spawn an embryo that has living biological parents in the colony (`Father` and `Mother` set).
   - In Dev Mode, grant one parent the `BodyModder` (or `Transhumanist`) trait.
   - Leave the other parent with baseline traits.
2. **Execute Gene Edit:**
   - Have a doctor complete a gene edit on the embryo at the `EmbryoSplicingBench`.
3. **Verify Mood Thoughts:**
   - Inspect the baseline parent's **Needs** tab:
     - Verify thought: **`Child genetically altered`** (-8 mood, 10-day duration).
   - Inspect the Transhumanist parent's **Needs** tab:
     - Verify thought: **`Child biologically upgraded`** (+6 mood, 10-day duration).
4. **Construct Independence:**
   - Splice a synthetic construct matrix embryo (`Father = null, Mother = null`).
   - Verify that **zero** parental thoughts are generated in the colony.

---

## 5. Pillar 2: Synthetic Constructs (SecUnits)

### Test Flow 3: Blank Construct Matrix Synthesis (No Donor Needed)
**Objective:** Verify standalone construct manufacturing without biological donors or third-party cloning mods.

1. **Fresh Organics Bill:**
   - At `EmbryoSplicingBench` -> **Bills** -> add `Synthesize Blank Embryo (Fresh Organics)`.
   - Spawn ingredients nearby: 40 Raw Meat, 40 Raw Plants, 10 Neutroamine, 2 Medicine.
   - Have a doctor complete the bill.
   - *Result Check:* Spawns a `HumanEmbryo`. Inspect pane shows:
     - `Origin: Synthetic Construct Matrix`
     - Label contains `(construct matrix)`
     - Biological parents show `None` (`Father = null, Mother = null`)
     - Endogenes include: `Gene_ConstructPsychology`, `Construct_MetabolicallyEfficient`, `Gene_MandatorySterility`, `Gene_ConstructHibernation`, `PsychicAbility_Deaf`, `Beauty_VeryUgly`, `LowSleep`, `Learning_Slow`, `RobustDigestion`, `StrongStomach`.
2. **Recycled Biomass Bill:**
   - Add bill: `Synthesize Blank Embryo (Recycled Biomass)`.
   - Requires: 1× `GeneticNutrientPaste`, 10 Raw Meat, 10 Raw Plants, 5 Neutroamine, 2 Medicine.
   - Verify completion yields the identical blank construct matrix embryo.

### Test Flow 4: Growth Vat Gestation & Innate Neural Reflexes
**Objective:** Verify construct decanting behavior both with and without neural imprint discs.

1. **Scenario A: Decanting Blank (No Neural Disc):**
   - Place a vanilla **Growth vat** and power it.
   - Insert a synthetic construct matrix embryo into the vat.
   - Ensure the vat's neural imprinter is empty (no disc loaded).
   - Fast-forward gestation to birth (`Pass 1 hour` or 4x speed).
   - *Result Check:* When the construct decants:
     - Check **Bio** tab:
       - **Shooting:** 4
       - **Melee:** 4
       - **Social:** 2
       - **Intellectual:** 2
       - **Artistic:** 0
       - **All other skills:** 3
       - **Passions:** All skills forced to `Passion.None` (no passions).
2. **Scenario B: Gestation with Neural Blueprint Disc:**
   - Scan a high-skill veteran colonist in the `Building_NeuralScanner` to generate a `NeuralBlueprintDisk` (e.g. Shooting 16, Melee 14).
   - Click the Growth Vat -> gizmo **Load Neural Imprint Disc** -> insert the disc.
   - Gestate another construct matrix embryo to decanting.
   - *Result Check:*
     - The decanted construct receives **50% of the donor skills** (e.g. Shooting 8, Melee 7).
     - All passions are forced to `Passion.None`.
     - The `NeuralBlueprintDisk` **remains safely seated in the vat** and is NOT consumed or destroyed.

### Test Flow 5: Neural Scanner Safeguards
**Objective:** Verify constructs are blocked from acting as templates in the `Building_NeuralScanner`.

1. Select a decanted construct pawn.
2. Select the `Building_NeuralScanner`:
   - Click gizmo **Select Subject to Scan**:
     - Hovering over or clicking the construct is rejected: *"Constructs cannot be scanned: Synthetic neural architecture incompatible with scanner."*
3. Right-click the scanner with the construct selected:
   - Float menu entry is disabled with the incompatibility message.

### Test Flow 6: Gene Bank Native Disc Storage
**Objective:** Verify vanilla Gene Banks accept and refrigerate blueprint discs.

1. Place a vanilla `Building_GeneBank` and connect power.
2. Spawn a `GenomeBlueprintDisk` and a `NeuralBlueprintDisk` on the floor.
3. Order a hauling pawn to haul both discs to the Gene Bank:
   - Verify haulers pick up the discs and load them into the Gene Bank container.
   - Select the Gene Bank: verify both discs appear inside container inspection.
   - Verify stored discs do not consume vanilla genepack capacity slots.
   - Verify discs receive refrigeration benefits (degradation stopped).

### Test Flow 7: Bio-Augment Surgeries & Failure Salvage
**Objective:** Verify construct-exclusive modular bioware grafting, medical skill gates, and surgical salvage mechanics.

1. **Human Incompatibility Check:**
   - Select a natural baseline colonist -> **Health** -> **Operations** -> **Add bill**.
   - Attempt to add any `Install ... Package` surgery:
     - Operation is unavailable / rejected: *"Cannot graft: Incompatible biological neural bus (Requires Construct)."*
2. **Construct Grafting & Assimilation:**
   - Select a construct pawn -> **Health** -> **Operations** -> Add `Install Basic Combat Package`.
   - Requires: Doctor Medical 4+, 1× `Eugenics_BasicCombatPackageItem`.
   - Have a doctor perform the surgery.
   - *On Success:*
     - Hediff `Basic Combat Package` attached (+2 Shooting, +2 Melee).
     - Construct enters dormant **Neural Assimilation** state (`Hediff_ConstructAssimilation`) for **2 in-game hours** (5,000 ticks).
3. **Failure & Salvage Check:**
   - Select an Intermediate or Advanced package surgery with a low-skill doctor (causing surgical botch).
   - *On Failure:*
     - Construct enters prolonged shock assimilation for **4 in-game hours** (10,000 ticks).
     - Rolls salvage chance: $20\% + (\text{Doctor Medical Skill} \times 5\%)$.
     - If salvage succeeds: Package drops cleanly on the floor undamaged.
4. **Package Extraction:**
   - In construct **Health** -> **Operations**, add `Remove [Package Name]`.
   - Doctor completes operation: hediff is removed and the package item drops safely.
5. **Strict Xenogerm Rejection:**
   - Create or spawn a Xenogerm.
   - Try to administer `Implant xenogerm` to a construct.
   - *Result Check:* Operation is blocked: *"Cannot implant xenogerm: Synthetic construct genetic architecture is permanently locked."*

### Test Flow 8: Construct Stasis Hibernation (`Gene_ConstructHibernation`)
**Objective:** Verify construct dormancy cycles, early wakeup disruption penalties, and the 30-day operating limit.

1. **Enter Stasis:**
   - Select a construct pawn -> click gizmo **Enter Stasis**.
   - Construct marches to a bed (or lies down on the spot) and enters dormant stasis.
2. **Interrupted Stasis Penalty:**
   - Order the construct to draft or wake before 12 hours elapse.
   - *Result Check:* Construct awakens with **`Interrupted Stasis`** (`Hediff_InterruptedStasis`: -20% Consciousness, -10% Moving, -10% Manipulation, nausea for 3 days).
3. **Clean Wake Cycle:**
   - Put construct into stasis and fast-forward 12+ hours (`Pass 1 hour` x12).
   - Wake the construct.
   - *Result Check:* No penalty applied; Rest need is instantly restored to **100%**; 30-day operational maintenance timer is reset.
4. **Maintenance Warning & Emergency Shutdown:**
   - In Dev Mode, advance construct game ticks toward 30 in-game days (1,800,000 ticks).
   - At Day 27 (75,000 ticks remaining): Verify maintenance warning letter triggers.
   - At Day 30: Verify construct triggers emergency comatose shutdown.

---

## 6. Summary Checklist for QA Sign-Off

- [ ] `dotnet build Source/EugenicsProgram.csproj` finishes with **0 warnings and 0 errors**.
- [ ] Dev mode opens cleanly; all Eugenics research projects unlock via debug action.
- [ ] Embryos enforce the strict **2-gene edit cap** in UI and bench bills.
- [ ] Splicing failures immediately drop **1× `GeneticNutrientPaste`** without defect micromanagement.
- [ ] Splicing a natural embryo triggers `-8` mood on baseline parents and `+6` mood on Transhumanists.
- [ ] `EmbryoSplicingBench` synthesizes blank construct matrices from Fresh Organics and Recycled Biomass.
- [ ] Decanted blank constructs receive innate baseline neural reflexes (4/4/2/2/0/3) and 0 passions.
- [ ] Constructs incubated with a neural disc receive 50% donor skills, 0 passions, and disc is not consumed.
- [ ] `NeuralScanner` rejects constructs from being scanned as templates.
- [ ] Vanilla `Building_GeneBank` stores, refrigerates, and preserves both blueprint disc types.
- [ ] All 12 Bio-Augments install exclusively on constructs with medical skill gates and assimilation sleep.
- [ ] Surgical failures roll salvage formula $20\% + (\text{Doctor Med} \times 5\%)$ to recover package items.
- [ ] Constructs reject `Recipe_ImplantXenogerm` with permanent genetic lock notice.
- [ ] Construct stasis hibernation enforces 12h cycle, penalties on early disruption, and 30-day operating limit.
- [ ] No red error messages in the debug log (`~`).
