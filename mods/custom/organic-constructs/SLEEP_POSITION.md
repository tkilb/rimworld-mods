# Construct Stasis Pod Sleep Positioning & Rendering Guide

This document records the architecture, math, coordinate systems, and Harmony patches governing pawn alignment inside the **Construct Stasis Pod** (`ConstructStasisPod`) in **RimWorld 1.6**. Future agents and developers should reference this guide whenever modifying pod assets, rendering logic, or sleep postures.

---

## 1. Overview & Mechanical Constraints

The `ConstructStasisPod` is a 1x1 vertical hibernation bed introduced for biological constructs (`tyler.organicconstructs`).
- **Hidden Body:** The pod definition uses `<bed_showSleeperBody>false</bed_showSleeperBody>` in [`Buildings_OrganicConstructs.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/ThingDefs_Buildings/Buildings_OrganicConstructs.xml). Only the sleeper's head, facial features, hair, and headwear are rendered.
- **Facing Direction:** Constructs lying down are locked to `Rot4.South` via [`Patch_PawnRenderer_LayingFacing`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs) to ensure they always rest on their back facing outward toward the glass viewing window.
- **Gravship Compatibility:** The pod stays strictly locked to its 1x1 cell footprint with zero building graphic offsets so it snaps cleanly against interior ship bulkheads without clipping hull conduits or walls during flight.

---

## 2. RimWorld 1.6 Coordinate System & Asset Orientation

### World Coordinates
- **X-axis (Horizontal):** `+X` is **East (Right)**, `-X` is **West (Left)**.
- **Z-axis (Vertical Map):** `+Z` is **North (Up)**, `-Z` is **South (Down)**.
- *(Note: Unity's Y-axis is draw altitude/render layer ordering, not screen height).*

### 4-Way Symmetrical Aperture
The pod sprite is 4-way symmetrical. In each rotation, the glass viewing window sits at a fixed distance from the center of the 1x1 tile:
| Pod Rotation | Window Location | Translation Required |
| :--- | :--- | :--- |
| **North** (`Rot4.North`) | Bottom (South) | `Z -= offset` (downward) |
| **South** (`Rot4.South`) | Top (North) | `Z += offset` (upward) |
| **East** (`Rot4.East`) | Left (West) | `X -= offset` (leftward) |
| **West** (`Rot4.West`) | Right (East) | `X += offset` (rightward) |

### Asset Filename Mapping
Vanilla RimWorld rotates clockwise with the **'E'** key: **North (0) → East (1) → South (2) → West (3)**.
The asset files in [`Textures/Things/Building/ConstructStasisPod/`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Textures/Things/Building/ConstructStasisPod) must strictly match this rotation:
- `ConstructStasisPod_north.png`: Latch at 12 o'clock, window at 6 o'clock.
- `ConstructStasisPod_east.png`: Latch at 3 o'clock, window at 9 o'clock.
- `ConstructStasisPod_south.png`: Latch at 6 o'clock, window at 12 o'clock.
- `ConstructStasisPod_west.png`: Latch at 9 o'clock, window at 3 o'clock.

*(Note: If East/West are inverted, 'E' rotation will appear counter-clockwise).*

---

## 3. Rendering Pipeline & Harmony Patches

All stasis sleep rendering logic resides in [`Source/Patches/Patch_StasisSleep.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs).

### A. Zeroed Baseline Normalization
Standard RimWorld neck/head offsets (`BaseHeadOffsetAt`) add standing neck heights (e.g. `0.79f`) that vary across adult body types (Male, Female, Hulk, Thin, Fat).

[`Patch_PawnRenderer_BaseHeadOffsetAt_StasisPod`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs) forces `BaseHeadOffsetAt` to return `Vector3.zero` while inside a `Building_ConstructStasisPod`. This locks all pawns to the exact tile center `(0, 0)` before applying aperture offsets, eliminating body type variance.

### B. Matrix Translation (`PawnRenderTree.ParallelPreDraw`)
In RimWorld 1.6, pawns are rendered via the `PawnRenderTree`. [`Patch_PawnRenderTree_ParallelPreDraw_StasisPod`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs) intercepts `ref PawnDrawParms parms` and shifts the root matrix translation columns:
- `parms.matrix.m03`: Horizontal X translation.
- `parms.matrix.m23`: Vertical Z translation.

*(Note: Portrait and UI cache renders check `parms.Portrait` / `PawnRenderFlags.Portrait` / `PawnRenderFlags.Cache` and are strictly ignored so colonist bar portraits retain standard framing).*

### C. Dialed-In Offset Values

```csharp
public const float PodWindowOffset = 0.20f;

float scale = pawn.ageTracker?.CurLifeStage?.bodySizeFactor ?? 1f;
float offset = (scale < 0.95f) ? 0.12f : PodWindowOffset;
```

- **Adults (`scale >= 0.95`):** `0.20f`
  - Perfectly aligns adult construct heads inside the window frame across all 4 rotations.
- **Teens / Juveniles (`scale < 0.95`):** `0.12f`
  - Because juvenile head sprites are smaller (~85% mesh scale), a smaller offset of `0.12f` keeps the head centered within the curved bezel without pushing it against the outer frame.

---

## 4. Gravship Launch & Despawn Safeguard

During gravship launches, Odyssey de-spawns pawns via `Pawn.DeSpawn()`, which cleans up jobs through `Toils_LayDown.FinalizeLayingJob -> ApplyBedThoughts -> TryGainMemory`.

[`Patch_MemoryThoughtHandler_TryGainMemory`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs) checks `if (newThought.MoodOffset() < 0f)` to suppress negative thoughts in stasis. Because vanilla RimWorld does not assign `newThought.pawn` until after prefixes run, the patch explicitly guarantees:
```csharp
if (newThought.pawn == null)
{
    newThought.pawn = __instance.pawn;
}
```
This prevents a `NullReferenceException` in `ThoughtUtility.ThoughtNullified` during gravship takeoff.
