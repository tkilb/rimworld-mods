# Bug Report: [Short Descriptive Title]

**Date:** YYYY-MM-DD  
**Status:** Open / Under Investigation / Resolved  
**Severity:** Critical (Crash/Game-breaking) | Major (Pawn Job Loop / Stuck) | Moderate (Feature Broken) | Minor (Visual/UI/Text)

---

## 1. Environment & Target System

- **Machine:** Arch Linux Desktop / Steam Deck (SteamOS)
- **RimWorld Version:** e.g., 1.5 / 1.6
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
- [ ] **Neural Scanner (`Building_NeuralScanner`):** Colonist neural scanning, `NeuralBlueprintDisk` encoding, fatigue hediff.
- [ ] **Growth Vat Imprinting (`CompGrowthVatImprinter`):** Vat insertion, skill/passion streaming, pawn decanting.
- [ ] **Construct Psychology & Social:** `Gene_ConstructPsychology`, `Trait_ConstructAsset`, romance suppression, death mood suppression.
- [ ] **Persistence & Scribe:** Save/load state corruption, null references upon game load.

---

## 3. Description & Summary

A clear and concise description of what the bug is.

---

## 4. Steps to Reproduce

Provide step-by-step reproduction instructions (refer to [Dev-Mode QA Runbook](../../../../docs/eugenics-qa-runbook.md) for debug actions/spawn names):

1. Turn on Dev Mode and God Mode.
2. Spawn `...` via Debug Actions -> Spawn thing...
3. Perform action `...`
4. Observe error or unexpected state.

---

## 5. Expected Behavior

A clear description of what you expected to happen according to the specification.

---

## 6. Actual Behavior

What actually happened (e.g. pawn dropped job, red error popped in console, UI failed to render, embryo lost genes).

---

## 7. Logs & Stack Trace

Paste the relevant exception stack trace from RimWorld's debug console or `Player.log`:
_(Default Linux path: `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Player.log`)_

```text
[Paste exception or red error lines here]
```

---

## 8. Root Cause Analysis (Dev Notes)

_(Leave blank initially; filled out during triage and debugging)_

- **Suspected File / Method:**
- **Proposed Fix:**
