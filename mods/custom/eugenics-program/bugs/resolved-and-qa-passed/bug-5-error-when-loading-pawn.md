# Bug: Neural Scanner Occupant Rendering & Abort Gizmo Texture Exception

**Status:** Partial QA Pass

## Description

Pawn enters the neural scanner and a dev error pops up.
Additional: The pawn disappears instead of laying in the scanner machine (visual bug).

## Steps to Reproduce (Optional)

1. Load an imprint disc into the Neural Scanner.
2. Select a pawn to enter the scanner.
3. Observe red dev console error as the abort gizmo initializes.
4. Observe that the pawn becomes invisible inside the scanner sprite instead of visibly resting in the pod.

## Logs / Errors (Optional)

```text
Could not load Texture2D at 'UI/Commands/Cancel' in any active mod or in base resources.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.ContentFinder`1<UnityEngine.Texture2D>:Get (string,bool)
EugenicsProgram.Building_NeuralScanner/<GetGizmos>d__34:MoveNext ()
System.Collections.Generic.List`1<Verse.Gizmo>:AddEnumerable (System.Collections.Generic.IEnumerable`1<Verse.Gizmo>)
System.Collections.Generic.List`1<Verse.Gizmo>:InsertRange (int,System.Collections.Generic.IEnumerable`1<Verse.Gizmo>)
System.Collections.Generic.List`1<Verse.Gizmo>:AddRange (System.Collections.Generic.IEnumerable`1<Verse.Gizmo>)
Verse.GizmoGridDrawer:DrawGizmoGridFor (System.Collections.Generic.IEnumerable`1<object>,Verse.Gizmo&)
RimWorld.MapGizmoUtility:MapUIOnGUI ()
RimWorld.MapInterface:MapInterfaceOnGUI_BeforeMainTabs ()
RimWorld.UIRoot_Play:UIRootOnGUI ()
Verse.Root:OnGUI ()
```

---

## Dev Notes / Fix (Optional)

- **Cause:**
  1. Texture Exception: In `Building_NeuralScanner.cs`, the "Abort Scan" gizmo used `ContentFinder<Texture2D>.Get("UI/Commands/Cancel", true)`. In vanilla RimWorld, the cancel button texture is located at `UI/Designators/Cancel`.
  2. Missing Occupant Rendering: `Building_NeuralScanner` did not implement `IThingHolderWithDrawnPawn` nor override `DynamicDrawPhaseAt`. When pawns despawn into `innerContainer`, the game engine does not draw them unless the container explicitly implements `IThingHolderWithDrawnPawn` and calls `Occupant.Drawer.renderer.DynamicDrawPhaseAt(...)`.
- **Fix:**
  1. Updated cancel button texture path to `"UI/Designators/Cancel"`.
  2. Implemented `IThingHolderWithDrawnPawn` on `Building_NeuralScanner` with `HeldPawnPosture = PawnPosture.LayingOnGroundFaceUp`, `HeldPawnBodyAngle = Rotation.AsAngle`, and `HeldPawnDrawPos_Y = DrawPos.y + 0.03658537f` (matching vanilla `Building_SubcoreScanner`).
  3. Overrode `DynamicDrawPhaseAt` to invoke `Occupant?.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc, null, neverAimWeapon: true)`, rendering the colonist properly lying inside the pod during scans.
