# Bug: Pawn invisible when inside neural scanner

**Status:** Passed QA

## Description

Pawn is invisible when entering the neural scanning machine

## Steps to Reproduce (Optional)

1. Load an imprint disc into the neural scanner.
2. Order a colonist to enter the scanner.
3. Observe that the pawn despawns into the scanner but is not drawn lying inside the pod.

## Logs / Errors (Optional)

```text

```

---

## Dev Notes / Fix (Optional)

- **Cause:** `NeuralScanner` inherited from `BuildingBase`, which defaults to `<drawerType>MapMeshOnly</drawerType>`. RimWorld's `DynamicDrawManager` and `Verse.Thing.DynamicDrawPhase` filter out `MapMeshOnly` buildings, meaning `DynamicDrawPhaseAt` was never called by the engine loop and `Occupant.Drawer.renderer.DynamicDrawPhaseAt` was never invoked.
- **Fix:** Added `<drawerType>MapMeshAndRealTime</drawerType>` to the `NeuralScanner` `ThingDef` in `Defs/ThingDefs_Buildings/Buildings_Neural.xml`, registering the building with `DynamicDrawManager` so the occupant renders visibly resting inside the scanning pod.

## Reopen Notes (Optional)
