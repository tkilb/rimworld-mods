# Bug: Duplicate GeneCategoryDef displayPriorityInXenotype Config Error

**Status:** Passed QA

## Description

What happened and what went wrong?
Error when loading the game (no crash)

## Steps to Reproduce (Optional)

1. Open the game

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

Config error in Hemogen: Multiple gene categories share the same displayPriorityInXenotype. This can cause display order issues.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.DefDatabase`1<Verse.GeneCategoryDef>:ErrorCheckAllDefs ()
System.Reflection.RuntimeMethodInfo:Invoke (object,System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo)
System.Reflection.MethodBase:Invoke (object,object[])
Verse.GenGeneric:InvokeStaticMethodOnGenericType (System.Type,System.Type,string)
Verse.PlayDataLoader/<>c:<DoPlayLoad>b__4_1 (System.Type)
System.Threading.Tasks.Parallel/<>c__DisplayClass33_0`2<System.Type, object>:<ForEachWorker>b__0 (int)
System.Threading.Tasks.Parallel/<>c__DisplayClass19_0`1<object>:<ForWorker>b__1 (System.Threading.Tasks.RangeWorker&,int,bool&)
System.Threading.Tasks.TaskReplicator/Replica`1<System.Threading.Tasks.RangeWorker>:ExecuteAction (bool&)
System.Threading.Tasks.TaskReplicator/Replica:Execute ()
System.Threading.Tasks.TaskReplicator/Replica/<>c:<.ctor>b__4_0 (object)
System.Threading.Tasks.Task:InnerInvoke ()
System.Threading.Tasks.Task:Execute ()
System.Threading.Tasks.Task:ExecutionContextCallback (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.Tasks.Task:ExecuteWithThreadLocal (System.Threading.Tasks.Task&)
System.Threading.Tasks.Task:ExecuteEntry (bool)
System.Threading.Tasks.Task:System.Threading.IThreadPoolWorkItem.ExecuteWorkItem ()
System.Threading.ThreadPoolWorkQueue:Dispatch ()
System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()

Config error in Healing: Multiple gene categories share the same displayPriorityInXenotype. This can cause display order issues.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.DefDatabase`1<Verse.GeneCategoryDef>:ErrorCheckAllDefs ()
System.Reflection.RuntimeMethodInfo:Invoke (object,System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo)
System.Reflection.MethodBase:Invoke (object,object[])
Verse.GenGeneric:InvokeStaticMethodOnGenericType (System.Type,System.Type,string)
Verse.PlayDataLoader/<>c:<DoPlayLoad>b__4_1 (System.Type)
System.Threading.Tasks.Parallel/<>c__DisplayClass33_0`2<System.Type, object>:<ForEachWorker>b__0 (int)
System.Threading.Tasks.Parallel/<>c__DisplayClass19_0`1<object>:<ForWorker>b__1 (System.Threading.Tasks.RangeWorker&,int,bool&)
System.Threading.Tasks.TaskReplicator/Replica`1<System.Threading.Tasks.RangeWorker>:ExecuteAction (bool&)
System.Threading.Tasks.TaskReplicator/Replica:Execute ()
System.Threading.Tasks.TaskReplicator/Replica/<>c:<.ctor>b__4_0 (object)
System.Threading.Tasks.Task:InnerInvoke ()
System.Threading.Tasks.Task:Execute ()
System.Threading.Tasks.Task:ExecutionContextCallback (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.Tasks.Task:ExecuteWithThreadLocal (System.Threading.Tasks.Task&)
System.Threading.Tasks.Task:ExecuteEntry (bool)
System.Threading.Tasks.Task:System.Threading.IThreadPoolWorkItem.ExecuteWorkItem ()
System.Threading.ThreadPoolWorkQueue:Dispatch ()
System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()

Config error in Eugenics_Construct: Multiple gene categories share the same displayPriorityInXenotype. This can cause display order issues.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.DefDatabase`1<Verse.GeneCategoryDef>:ErrorCheckAllDefs ()
System.Reflection.RuntimeMethodInfo:Invoke (object,System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo)
System.Reflection.MethodBase:Invoke (object,object[])
Verse.GenGeneric:InvokeStaticMethodOnGenericType (System.Type,System.Type,string)
Verse.PlayDataLoader/<>c:<DoPlayLoad>b__4_1 (System.Type)
System.Threading.Tasks.Parallel/<>c__DisplayClass33_0`2<System.Type, object>:<ForEachWorker>b__0 (int)
System.Threading.Tasks.Parallel/<>c__DisplayClass19_0`1<object>:<ForWorker>b__1 (System.Threading.Tasks.RangeWorker&,int,bool&)
System.Threading.Tasks.TaskReplicator/Replica`1<System.Threading.Tasks.RangeWorker>:ExecuteAction (bool&)
System.Threading.Tasks.TaskReplicator/Replica:Execute ()
System.Threading.Tasks.TaskReplicator/Replica/<>c:<.ctor>b__4_0 (object)
System.Threading.Tasks.Task:InnerInvoke ()
System.Threading.Tasks.Task:Execute ()
System.Threading.Tasks.Task:ExecutionContextCallback (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.Tasks.Task:ExecuteWithThreadLocal (System.Threading.Tasks.Task&)
System.Threading.Tasks.Task:ExecuteEntry (bool)
System.Threading.Tasks.Task:System.Threading.IThreadPoolWorkItem.ExecuteWorkItem ()
System.Threading.ThreadPoolWorkQueue:Dispatch ()
System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()

Config error in Eugenics_Optimizer: Multiple gene categories share the same displayPriorityInXenotype. This can cause display order issues.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.DefDatabase`1<Verse.GeneCategoryDef>:ErrorCheckAllDefs ()
System.Reflection.RuntimeMethodInfo:Invoke (object,System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo)
System.Reflection.MethodBase:Invoke (object,object[])
Verse.GenGeneric:InvokeStaticMethodOnGenericType (System.Type,System.Type,string)
Verse.PlayDataLoader/<>c:<DoPlayLoad>b__4_1 (System.Type)
System.Threading.Tasks.Parallel/<>c__DisplayClass33_0`2<System.Type, object>:<ForEachWorker>b__0 (int)
System.Threading.Tasks.Parallel/<>c__DisplayClass19_0`1<object>:<ForWorker>b__1 (System.Threading.Tasks.RangeWorker&,int,bool&)
System.Threading.Tasks.TaskReplicator/Replica`1<System.Threading.Tasks.RangeWorker>:ExecuteAction (bool&)
System.Threading.Tasks.TaskReplicator/Replica:Execute ()
System.Threading.Tasks.TaskReplicator/Replica/<>c:<.ctor>b__4_0 (object)
System.Threading.Tasks.Task:InnerInvoke ()
System.Threading.Tasks.Task:Execute ()
System.Threading.Tasks.Task:ExecutionContextCallback (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.Tasks.Task:ExecuteWithThreadLocal (System.Threading.Tasks.Task&)
System.Threading.Tasks.Task:ExecuteEntry (bool)
System.Threading.Tasks.Task:System.Threading.IThreadPoolWorkItem.ExecuteWorkItem ()
System.Threading.ThreadPoolWorkQueue:Dispatch ()
System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()

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

```

---

## Dev Notes / Fix (Optional)

- **Cause:** Duplicate `displayPriorityInXenotype` definitions between custom gene categories and vanilla categories (`Eugenics_Construct` at `500` conflicted with `Healing` at `500`; `Eugenics_Optimizer` at `510` conflicted with `Hemogen` at `510`), triggering config errors during `Verse.DefDatabase<GeneCategoryDef>.ErrorCheckAllDefs()`.
- **Fix:** Assigned non-conflicting high-tier priorities (`650` for `Eugenics_Optimizer`, `640` for `Eugenics_Construct`) in `Genes_Optimizer.xml` and `Genes_Construct.xml` to sit immediately below Archite (`1000`) and above standard abilities (`520`).
