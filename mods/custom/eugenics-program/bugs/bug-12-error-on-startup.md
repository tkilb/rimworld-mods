# Bug: Error on Startup (Uninitialized DefOf / PregnancyUtility TypeInitializationException)

**Status:** Ready for QA

## Description

Get this error when starting game

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

Tried to use an uninitialized DefOf of type PawnRelationDefOf. DefOfs are initialized right after all defs all loaded. Uninitialized DefOfs will return only nulls. (hint: don't use DefOfs as default field values in Defs, try to resolve them in ResolveReferences() instead)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Warning (string)
RimWorld.DefOfHelper:EnsureInitializedInCtor (System.Type)
RimWorld.PawnRelationDefOf:.cctor ()
RimWorld.PregnancyUtility:.cctor ()
System.RuntimeMethodHandle:GetFunctionPointer ()
MonoMod.Core.Platforms.Runtimes.MonoRuntime:Compile (System.Reflection.MethodBase)
MonoMod.Core.Platforms.PlatformTriple:Compile (System.Reflection.MethodBase)
MonoMod.Core.Platforms.PlatformTripleDetourFactory/Detour:CreateDetour ()
MonoMod.Core.Platforms.PlatformTripleDetourFactory/DetourBase:Apply ()
MonoMod.Core.Platforms.PlatformTripleDetourFactory:CreateDetour (MonoMod.Core.CreateDetourRequest)
MonoMod.Core.DetourFactory:CreateDetour (MonoMod.Core.IDetourFactory,System.Reflection.MethodBase,System.Reflection.MethodBase,bool)
HarmonyLib.PatchTools:DetourMethod (System.Reflection.MethodBase,System.Reflection.MethodBase)
HarmonyLib.PatchFunctions:UpdateWrapper (System.Reflection.MethodBase,HarmonyLib.PatchInfo)
HarmonyLib.PatchClassProcessor:ProcessPatchJob (HarmonyLib.PatchJobs`1/Job<System.Reflection.MethodInfo>)
HarmonyLib.PatchClassProcessor:PatchWithAttributes (System.Reflection.MethodBase&,bool)
HarmonyLib.PatchClassProcessor:Patch ()
HarmonyLib.Harmony:<PatchAll>b__10_1 (System.Type)
HarmonyLib.CollectionExtensions:Do<System.Type> (System.Collections.Generic.IEnumerable`1<System.Type>,System.Action`1<System.Type>)
HarmonyLib.CollectionExtensions:DoIf<System.Type> (System.Collections.Generic.IEnumerable`1<System.Type>,System.Func`2<System.Type, bool>,System.Action`1<System.Type>)
HarmonyLib.Harmony:PatchAll (System.Reflection.Assembly)
HarmonyLib.Harmony:PatchAll ()
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

Error while instantiating a mod of type EugenicsProgram.EugenicsProgramMod: System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. ---> HarmonyLib.HarmonyException: Patching exception in method static Verse.Thing RimWorld.PregnancyUtility::ApplyBirthOutcome(RimWorld.RitualOutcomePossibility outcome, System.Single quality, RimWorld.Precept_Ritual ritual, System.Collections.Generic.List`1<Verse.GeneDef> genes, Verse.Pawn geneticMother, Verse.Thing birtherThing, Verse.Pawn father, Verse.Pawn doctor, RimWorld.LordJob_Ritual lordJobRitual, RimWorld.RitualRoleAssignments assignments, System.Boolean preventLetter) ---> System.TypeInitializationException: The type initializer for 'RimWorld.PregnancyUtility' threw an exception. ---> System.ArgumentNullException: Value cannot be null.
Parameter name: key
[Ref 348091A5]
 [0x00006] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
  at System.Collections.Generic.Dictionary`2[TKey,TValue].TryInsert (TKey key, TValue value, System.Collections.Generic.InsertionBehavior behavior) [0x00008] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
  at System.Collections.Generic.Dictionary`2[TKey,TValue].Add (TKey key, TValue value) [0x00000] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
RimWorld.PregnancyUtility..cctor()
   --- End of inner exception stack trace ---
[Ref 8EEC114]
  at HarmonyLib.PatchFunctions.UpdateWrapper (System.Reflection.MethodBase original, HarmonyLib.PatchInfo patchInfo) [0x000c2] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.PatchClassProcessor.ProcessPatchJob (HarmonyLib.PatchJobs`1+Job[T] job) [0x000f8] in <e53399289d9b419d83f9f5b02c5cf609>:0
   --- End of inner exception stack trace ---
[Ref 5C6B0DFF]
  at HarmonyLib.PatchClassProcessor.ReportException (System.Exception exception, System.Reflection.MethodBase original) [0x0013c] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.PatchClassProcessor.Patch () [0x00096] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.Harmony.<PatchAll>b__10_1 (System.Type type) [0x00007] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.CollectionExtensions.Do[T] (System.Collections.Generic.IEnumerable`1[T] sequence, System.Action`1[T] action) [0x00014] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.CollectionExtensions.DoIf[T] (System.Collections.Generic.IEnumerable`1[T] sequence, System.Func`2[T,TResult] condition, System.Action`1[T] action) [0x00007] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.Harmony.PatchAll (System.Reflection.Assembly assembly) [0x00006] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.Harmony.PatchAll () [0x0001d] in <e53399289d9b419d83f9f5b02c5cf609>:0
EugenicsProgram.EugenicsProgramMod..ctor(ModContentPack content)
(wrapper managed-to-native) System.Reflection.RuntimeConstructorInfo.InternalInvoke(System.Reflection.RuntimeConstructorInfo,object,object[],System.Exception&)
  at System.Reflection.RuntimeConstructorInfo.InternalInvoke (System.Object obj, System.Object[] parameters, System.Boolean wrapExceptions) [0x00005] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
   --- End of inner exception stack trace ---
[Ref 927DEC1]
  at System.Reflection.RuntimeConstructorInfo.InternalInvoke (System.Object obj, System.Object[] parameters, System.Boolean wrapExceptions) [0x0001a] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
  at System.Reflection.RuntimeConstructorInfo.DoInvoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00086] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
  at System.Reflection.RuntimeConstructorInfo.Invoke (System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00000] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
  at System.RuntimeType.CreateInstanceImpl (System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Object[] args, System.Globalization.CultureInfo culture, System.Object[] activationAttributes, System.Threading.StackCrawlMark& stackMark) [0x0022b] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
  at System.Activator.CreateInstance (System.Type type, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Object[] args, System.Globalization.CultureInfo culture, System.Object[] activationAttributes) [0x0009c] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
  at System.Activator.CreateInstance (System.Type type, System.Object[] args) [0x00000] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
  at Verse.LoadedModManager.CreateModClasses () [0x00085] in <b4d967f0d45a413fbb0223eefee4f2ae>:0
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
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

Error in static constructor of Dark.Cloning.LoadHarmony: System.TypeInitializationException: The type initializer for 'Dark.Cloning.LoadHarmony' threw an exception. ---> HarmonyLib.HarmonyException: Patching exception in method static Verse.Thing RimWorld.PregnancyUtility::ApplyBirthOutcome(RimWorld.RitualOutcomePossibility outcome, System.Single quality, RimWorld.Precept_Ritual ritual, System.Collections.Generic.List`1<Verse.GeneDef> genes, Verse.Pawn geneticMother, Verse.Thing birtherThing, Verse.Pawn father, Verse.Pawn doctor, RimWorld.LordJob_Ritual lordJobRitual, RimWorld.RitualRoleAssignments assignments, System.Boolean preventLetter) ---> System.TypeInitializationException: The type initializer for 'RimWorld.PregnancyUtility' threw an exception. ---> System.ArgumentNullException: Value cannot be null.
Parameter name: key
[Ref 348091A5] Duplicate stacktrace, see ref for original
   --- End of inner exception stack trace ---
[Ref 8EEC114] Duplicate stacktrace, see ref for original
   --- End of inner exception stack trace ---
[Ref EAB76322]
  at HarmonyLib.PatchClassProcessor.ReportException (System.Exception exception, System.Reflection.MethodBase original) [0x0013c] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.PatchClassProcessor.Patch () [0x00096] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.Harmony.<PatchAll>b__10_1 (System.Type type) [0x00007] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.CollectionExtensions.Do[T] (System.Collections.Generic.IEnumerable`1[T] sequence, System.Action`1[T] action) [0x00014] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.CollectionExtensions.DoIf[T] (System.Collections.Generic.IEnumerable`1[T] sequence, System.Func`2[T,TResult] condition, System.Action`1[T] action) [0x00007] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.Harmony.PatchAll (System.Reflection.Assembly assembly) [0x00006] in <e53399289d9b419d83f9f5b02c5cf609>:0
  at HarmonyLib.Harmony.PatchAll () [0x0001d] in <e53399289d9b419d83f9f5b02c5cf609>:0
Dark.Cloning.LoadHarmony..cctor()
   --- End of inner exception stack trace ---
[Ref 6E91678D]
(wrapper managed-to-native) System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(intptr)
  at System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor (System.RuntimeTypeHandle type) [0x0002a] in <e3b07672ffbd43c1838e1ebbe94cbdf5>:0
  at Verse.StaticConstructorOnStartupUtility.CallAll () [0x00025] in <b4d967f0d45a413fbb0223eefee4f2ae>:0
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
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

- **Cause:** `EugenicsProgramMod` was calling `harmony.PatchAll()` inside its mod constructor (`Mod..ctor`). Mod constructors are instantiated early during `Verse.LoadedModManager:CreateModClasses()`, before XML defs and `DefOf`s are initialized. When Harmony processed and JIT-compiled `PregnancyUtility.ApplyBirthOutcome`, Mono triggered `RimWorld.PregnancyUtility`'s static constructor (`.cctor()`). Because `PawnRelationDefOf` was still uninitialized and null at that stage, `PregnancyUtility..cctor()` threw an `ArgumentNullException` when attempting to register a null key into an internal dictionary, resulting in a fatal `TypeInitializationException` during mod class instantiation.
- **Fix:** Moved `harmony.PatchAll()` out of the `EugenicsProgramMod` constructor into a static class decorated with `[StaticConstructorOnStartup]` (`EugenicsProgramModInit`). In RimWorld, static constructors marked with `[StaticConstructorOnStartup]` are run by `Verse.StaticConstructorOnStartupUtility:CallAll()` after all defs and `DefOf`s are loaded and bound.

## Reopen Notes (Optional)
