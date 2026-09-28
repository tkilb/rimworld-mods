# Bug Report: Startup XML Schema, Cross-Reference, and Texture Errors

**Date:** 2026-9-27
**Status:** Passed QA
**Severity:** Critical

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

- [x] Initial Mod Loading
- [ ] **Gene Splicing UI (`Dialog_EditEmbryoGenes`):** Inspect gizmo, gene addition/removal, template matching, disc burning.
- [ ] **Batch Workbench (`EmbryoSplicingBench`):** Bill configuration, disc loading/ejection, `Recipe_BatchApplyBlueprint`.
- [ ] **Embryo Comps & Diagnostics:** `CompEmbryoQuality`, defect generation, `Recipe_BatchScreenEmbryo`, `Recipe_BatchRecycleEmbryo`.
- [ ] **Neural Scanner (`Building_NeuralScanner`):** Colonist neural scanning, `NeuralBlueprintDisk` encoding, fatigue hediff.
- [ ] **Growth Vat Imprinting (`CompGrowthVatImprinter`):** Vat insertion, skill/passion streaming, pawn decanting.
- [ ] **Construct Psychology & Social:** `Gene_ConstructPsychology`, `Trait_ConstructAsset`, romance suppression, death mood suppression.
- [ ] **Persistence & Scribe:** Save/load state corruption, null references upon game load.

---

## 3. Description & Summary

Error Message at the menu screen, game loads without crash, but provides these errors

## 4. Steps to Reproduce

1. Error at launch

---

## 5. Expected Behavior

N/A

---

## 6. Actual Behavior

N/A

---

## 7. Logs & Stack Trace

Paste the relevant exception stack trace from RimWorld's debug console or `Player.log`:
_(Default Linux path: `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Player.log`)_

```
Command line arguments: -disable-compute-shaders
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
Verse.Root:CheckGlobalInit ()
Verse.Root:Start ()
Verse.Root_Entry:Start ()

RimWorld 1.6.4871 rev600
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
RimWorld.VersionControl:LogVersionNumber ()
Verse.Root:CheckGlobalInit ()
Verse.Root:Start ()
Verse.Root_Entry:Start ()

[Eugenics Program] Mod loaded and Harmony patches initialized.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
EugenicsProgram.EugenicsProgramMod:.ctor (Verse.ModContentPack)
System.Reflection.RuntimeConstructorInfo:InternalInvoke (object,object[],bool)
System.Reflection.RuntimeConstructorInfo:DoInvoke (object,System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo)
System.Reflection.RuntimeConstructorInfo:Invoke (System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo)
System.RuntimeType:CreateInstanceImpl (System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo,object[],System.Threading.StackCrawlMark&)
System.Activator:CreateInstance (System.Type,System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo,object[])
System.Activator:CreateInstance (System.Type,object[])
Verse.LoadedModManager:CreateModClasses ()
Verse.LoadedModManager:LoadAllActiveMods (bool)
Verse.PlayDataLoader:DoPlayLoad ()
Verse.PlayDataLoader:LoadAllPlayData (bool)
Verse.Root/<>c:<Start>b__10_1 ()
Verse.LongEventHandler:RunEventFromAnotherThread (System.Action)
Verse.LongEventHandler/<>c:<UpdateCurrentAsynchronousEvent>b__28_0 ()
System.Threading.ThreadHelper:ThreadStart_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object)
System.Threading.ThreadHelper:ThreadStart ()

Could not resolve cross-reference to Verse.ResearchProjectDef named Xenogenetics (wanter=prerequisites)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.DirectXmlCrossRefLoader:TryResolveDef<Verse.ResearchProjectDef> (string,Verse.FailMode,object)
Verse.DirectXmlCrossRefLoader/WantedRefForList`1<Verse.ResearchProjectDef>:TryResolve (Verse.FailMode)
Verse.DirectXmlCrossRefLoader/<>c__DisplayClass16_0:<ResolveAllWantedCrossReferences>b__0 (Verse.DirectXmlCrossRefLoader/WantedRef)
Verse.GenThreading/<>c__DisplayClass7_1`1<Verse.DirectXmlCrossRefLoader/WantedRef>:<ParallelForEach>b__0 (object)
System.Threading.QueueUserWorkItemCallback:WaitCallback_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.QueueUserWorkItemCallback:System.Threading.IThreadPoolWorkItem.ExecuteWorkItem ()
System.Threading.ThreadPoolWorkQueue:Dispatch ()
System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()

Mod Test Csharp did not load any content. Following load folders were used:
  - /mnt/gaming/SteamLibrary/steamapps/common/RimWorld/Mods/test-csharp
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.LoadedModManager/<>c:<LoadModContent>b__13_1 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

Mod Test Mod did not load any content. Following load folders were used:
  - /mnt/gaming/SteamLibrary/steamapps/common/RimWorld/Mods/test-mod
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.LoadedModManager/<>c:<LoadModContent>b__13_1 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

[BiotechCloning] Patching ...
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
Dark.Cloning.LoadHarmony:.cctor ()
System.Runtime.CompilerServices.RuntimeHelpers:RunClassConstructor (System.RuntimeTypeHandle)
Verse.StaticConstructorOnStartupUtility:CallAll ()
Verse.PlayDataLoader/<>c:<DoPlayLoad>b__4_4 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

[RecycleThis]: Added 3 things to destroy-benches, 3 things to scrap-benches and 1 things to smelt-benches
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
RecycleThis.RecycleThisUtility:.cctor ()
System.Runtime.CompilerServices.RuntimeHelpers:RunClassConstructor (System.RuntimeTypeHandle)
RecycleThis.RecycleThis:.cctor ()
System.Runtime.CompilerServices.RuntimeHelpers:RunClassConstructor (System.RuntimeTypeHandle)
Verse.StaticConstructorOnStartupUtility:CallAll ()
Verse.PlayDataLoader/<>c:<DoPlayLoad>b__4_4 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

```

---

## 8. Root Cause Analysis (Dev Notes)

- **Suspected File / Method:**
  - `Defs/ResearchProjectDefs/ResearchProjects_Eugenics.xml`: Used `Xenogenetics` (which is the in-game display `<label>`), whereas the actual RimWorld Biotech `ResearchProjectDef` `<defName>` is `Xenogermination`.

- **Proposed Fix:**
  - Changed `<prerequisites><li>Xenogenetics</li></prerequisites>` to `<prerequisites><li>Xenogermination</li></prerequisites>` in `Defs/ResearchProjectDefs/ResearchProjects_Eugenics.xml`.
