# Bug: No tick progress on neural pawn scanning

**Status:** Passed QA

## Description

When the scanner is powered, no tick progress happens, or if it does happen, the UI stays at 0% scanned for many game hours

## Steps to Reproduce (Optional)

1. Build and power a neural scanner.
2. Load an imprint disc and send a colonist to enter the scanner.
3. Observe that scan progress remains frozen at 0% indefinitely because `Tick()` is never called.

## Logs / Errors (Optional)

```text

```

---

## Dev Notes / Fix (Optional)

- **Cause:**
  1. `NeuralScanner` inherited from `BuildingBase`, which defaults to `<tickerType>Never</tickerType>`. Because `tickerType` was `Never`, RimWorld's tick manager never invoked `Building_NeuralScanner.Tick()`, causing `ticksScanning` to never increment.
  2. In addition, `scanTicks`, `powerConsumptionScanning`, and `powerConsumptionIdle` were hardcoded in C# rather than configurable via XML `DefModExtension` as required by the architecture specification.
- **Fix:**
  1. Added `<tickerType>Normal</tickerType>` to `NeuralScanner` `ThingDef` in `Defs/ThingDefs_Buildings/Buildings_Neural.xml`, enabling the engine tick loop for the scanner.
  2. Created `NeuralScannerExtension : DefModExtension` in `Source/Buildings/NeuralScannerExtension.cs` exposing `scanTicks`, `powerConsumptionScanning`, and `powerConsumptionIdle` in XML.
  3. Overrode `SpawnSetup` in `Building_NeuralScanner.cs` to initialize scan duration and power balances from the `NeuralScannerExtension` mod extension on spawn.

## Reopen Notes (Optional)
