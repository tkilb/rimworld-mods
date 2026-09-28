# Bug Report: Neural Scanner Architect Menu Icon Overflow & Passability Config Error

**Status:** Passed QA
**Severity:** Minor

---

## 1. Environment & Target System

- **Machine:** Arch Linux Desktop / Steam Deck (SteamOS)
- **RimWorld Version:** 1.6
- **Active DLCs:** Royalty [ ] | Ideology [ ] | Biotech [x] | Anomaly [ ]
- **Relevant Active Mods:**
  - `brrainz.harmony`
  - `zal.cloning` (Biotech Cloning Continued)
  - `ninagoblin.enhancedvatlearning`
  - `Zaf.InjectGene`
  - `AmCh.Eragon.HCGeneFabrication`
  - `Defi.GeneRipper`
  - Other:

---

## 2. Affected Subsystem

Check the component(s) involved in the issue:

- [ ] **Gene Splicing UI (`Dialog_EditEmbryoGenes`):** Inspect gizmo, gene addition/removal, template matching, disc burning.
- [ ] **Batch Workbench (`EmbryoSplicingBench`):** Bill configuration, disc loading/ejection, `Recipe_BatchApplyBlueprint`.
- [ ] **Embryo Comps & Diagnostics:** `CompEmbryoQuality`, defect generation, `Recipe_BatchScreenEmbryo`, `Recipe_BatchRecycleEmbryo`.
- [x] **Neural Scanner (`Building_NeuralScanner`):** Colonist neural scanning, `NeuralBlueprintDisk` encoding, fatigue hediff.
- [ ] **Growth Vat Imprinting (`CompGrowthVatImprinter`):** Vat insertion, skill/passion streaming, pawn decanting.
- [ ] **Construct Psychology & Social:** `Gene_ConstructPsychology`, `Trait_ConstructAsset`, romance suppression, death mood suppression.
- [ ] **Persistence & Scribe:** Save/load state corruption, null references upon game load.

---

## 3. Description & Summary

The scanner icon overflows the button slot in the Biotech Architect menu because vanilla's `SubcoreSoftscanner` texture was rendered across an elongated footprint without a dedicated `<uiIconScale>`. Additionally, startup logs showed `Config error in NeuralScanner: impassable, player-buildable building that can be shot/seen over`.

# REOPEN notes:

Icon is still too tall shrink by 25%

---

## 4. Steps to Reproduce

1. Open the Architect menu -> Biotech category.
2. Observe the Neural Scanner button icon overflowing the cell bounds.

---

## 5. Expected Behavior

The building icon should fit neatly within the architect category button without overflow or distortion.

---

## 6. Actual Behavior

Elongated building sprite overflowed the button bounds in the UI.

---

## 7. Logs & Stack Trace

```text
Config error in NeuralScanner: impassable, player-buildable building that can be shot/seen over.
```

---

## 8. Root Cause Analysis (Dev Notes)

- **Suspected File / Method:**
  - `Defs/ThingDefs_Buildings/Buildings_Neural.xml`
- **Root Cause:**
  Missing `<uiIconScale>` in `ThingDef` caused default full-bounds rendering of the 1.5x2.5 drawSize graphic in the square menu slot. Also, `passability` was set to `Impassable` with `fillPercent 0.85` instead of `PassThroughOnly` / `0.5`.
- **Fix:**
  - Initial fix added `<uiIconScale>0.75</uiIconScale>`.
  - In response to reopen QA note ("shrink by 25%"), reduced `<uiIconScale>` from `0.75` to `0.56` (`0.75 * 0.75`).
  - Maintained `passability` at `PassThroughOnly` and `fillPercent` at `0.5` matching vanilla scanner standards.
