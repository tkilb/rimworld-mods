# Bug: Unable to order pawn to haul disc to scanning machine

**Status:** Passed QA

## Description

Unable to order pawn to haul disk to scanning machine

## Steps to Reproduce (Optional)

1. Click on pawn, click on empty device to order to insert into machine. UI says no machine available, but the actually is.

## Logs / Errors (Optional)

```text

```

---

## Dev Notes / Fix (Optional)

- **Cause:**
  1. `CompNeuralBlueprint` and `CompGenomeBlueprint` did not implement `CompFloatMenuOptions(Pawn selPawn)`, meaning right-clicking on a disc item on the ground never provided any action to haul or insert it into a machine.
  2. `Building_NeuralScanner.GetFloatMenuOptions` and `Building_EmbryoSplicingBench.GetFloatMenuOptions` only offered hauling if `targetDisc` was already assigned via the building's gizmo. When unassigned, right-clicking the machine only displayed a disabled message rather than allowing the player to order loading an available disc from the colony.
  3. `JobDriver_HaulDiscToContainer.CanAcceptDisc` strictly required `scanner.targetDisc != null`, which caused immediate job failure if initiated without a pre-existing building target assignment.
  4. `Defs/WorkGiverDefs/WorkGivers_Eugenics.xml` declared `<fixedBillGiverDefs>` on non-bill workgivers (`HaulToNeuralScanner`, `HaulToEmbryoSplicingBench`), causing workgiver validation anomalies.
- **Fix:**
  1. Implemented `CompFloatMenuOptions` on `CompNeuralBlueprint` (allowing direct right-click insertion of blank discs into reachable `NeuralScanner`s, or encoded discs into `GrowthVat`s) and on `CompGenomeBlueprint` (allowing direct right-click insertion of blueprint discs into `EmbryoSplicingBench`es).
  2. Enhanced `GetFloatMenuOptions` on `Building_NeuralScanner` and `Building_EmbryoSplicingBench` so right-clicking an empty machine searches for available, reachable discs on the map and provides a direct "Load [disc] into [machine]" order.
  3. Updated `JobDriver_HaulDiscToContainer.CanAcceptDisc` so hauling is valid whenever the container is empty/accepting (`targetDisc == null || targetDisc == disc`), and ensured `UpdateTargetDisc` sets `targetDisc = carried`.
  4. Removed `<fixedBillGiverDefs>` from hauling workgiver definitions in `WorkGivers_Eugenics.xml`.

## Reopen Notes (Optional)
