# Bug: Cannot Select Pawn In Neural Scanner

**Status:** Ready for QA

## Description

When a pawn is in a neural scanner, the user should be able to select within similar to how a child pawn can be selected from the vat object

## Steps to Reproduce (Optional)

1. Load an imprint disc into the Neural Scanner.
2. Have a colonist enter the Neural Scanner.
3. Observe inability to select the occupant via machine gizmo or by clicking the machine.

## Logs / Errors (Optional)

```text

```

---

## Dev Notes / Fix (Optional)

- **Cause:** `NeuralScanner` `ThingDef` lacked `<containedPawnsSelectable>true</containedPawnsSelectable>` (which allows in-world clicking/cycling of contained pawns like in `Building_GrowthVat`), and `Building_NeuralScanner.GetGizmos()` only offered an "Abort Scan" gizmo without yielding `Building_Casket.SelectContainedItemGizmo(this, Occupant)`.
- **Fix:** Added `<containedPawnsSelectable>true</containedPawnsSelectable>` to `NeuralScanner` in `Defs/ThingDefs_Buildings/Buildings_Neural.xml`, and yielded `Building_Casket.SelectContainedItemGizmo(this, Occupant)` in `Building_NeuralScanner.GetGizmos()`.

## Reopen Notes (Optional)
