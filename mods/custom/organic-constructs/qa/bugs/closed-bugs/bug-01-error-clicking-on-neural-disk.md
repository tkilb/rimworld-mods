# Bug: [Short Title]

**Status:** Passed QA

## Description

Error when clicking on neural disk

## Steps to Reproduce (Optional)

1.

## Logs / Errors (Optional)

```text
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

[Organic Constructs] Mod loaded and Harmony patches initialized.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
OrganicConstructs.OrganicConstructsModInit:.cctor ()
System.Runtime.CompilerServices.RuntimeHelpers:RunClassConstructor (System.RuntimeTypeHandle)
Verse.StaticConstructorOnStartupUtility:CallAll ()
Verse.PlayDataLoader/<>c:<DoPlayLoad>b__4_4 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

Initializing new game with mods:
  - brrainz.harmony
  - Ludeon.RimWorld
  - Ludeon.RimWorld.Biotech
  - ilyvion.laboratory
  - Mlie.RecycleThis
  - zal.cloning
  - ninagoblin.enhancedvatlearning
  - AmCh.Eragon.HCGeneFabrication
  - lowli.genebankplus
  - erdelf.MinifyEverything
  - ilyvion.colonymanagerredux
  - Zaf.InjectGene
  - Defi.GeneRipper
  - tyler.organicconstructs (incompatible version)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
Verse.Game:InitNewGame ()
Verse.Root_Play/<>c:<Start>b__1_2 ()
Verse.LongEventHandler:RunEventFromAnotherThread (System.Action)
Verse.LongEventHandler/<>c:<UpdateCurrentAsynchronousEvent>b__28_0 ()
System.Threading.ThreadHelper:ThreadStart_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object)
System.Threading.ThreadHelper:ThreadStart ()

Could not load Texture2D at 'UI/Commands/Copy' in any active mod or in base resources.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.ContentFinder`1<UnityEngine.Texture2D>:Get (string,bool)
OrganicConstructs.CompGenomeBlueprint/<CompGetGizmosExtra>d__14:MoveNext ()
Verse.ThingWithComps/<GetGizmos>d__42:MoveNext ()
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

- **Cause:** When selecting a blueprint disc, `CompGenomeBlueprint.CompGetGizmosExtra()` attempted to load an icon via `ContentFinder<Texture2D>.Get("UI/Commands/Copy", true)`. In vanilla RimWorld, no texture exists at `"UI/Commands/Copy"`, causing `ContentFinder` to log a red error when `reportFailure` is true. Additionally, `Gene_ConstructHibernation.cs` was attempting to load `"UI/Designators/Zzz"` with `reportFailure: true`, which also does not exist.
- **Fix:** In `CompGenomeBlueprint.cs`, replaced the missing path with `TexButton.Copy ?? parent?.def?.uiIcon`. In `Gene_ConstructHibernation.cs`, replaced `"UI/Designators/Zzz"` with `def?.Icon ?? ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Sleeping", false)`.

## Reopen Notes (Optional)
