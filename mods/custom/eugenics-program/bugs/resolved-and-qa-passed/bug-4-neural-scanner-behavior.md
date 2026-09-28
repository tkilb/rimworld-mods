# Bug: Neural Scanner Disc Map Transfer & Instant Teleportation

Note to AI: User will default to `medium` model, advise when they should switch to a `high` model

**Status:** Partial QA Pass

## Description

Selecting "Load Imprint Disc" (or "Load Blueprint Disc") and picking a disc causes improper transfer behavior. The disc currently teleports directly into the scanner instead of requiring a colonist to physically haul and deliver it.

## Steps to Reproduce (Optional)

1. Build `NeuralScanner` (or `EmbryoSplicingBench`).
2. Spawn `NeuralBlueprintDisk` (or `GenomeBlueprintDisk`) on the ground.
3. Select scanner, click "Load Imprint Disc", and select the spawned disc.
4. Observe that the disc teleports directly into the scanner rather than dispatching a pawn hauling job.

## Logs / Errors (Optional)

```text
Can't transfer items to or from Maps directly. They must be spawned or despawned manually. Use TryAdd(item.SplitOff(count))
```

---

## Dev Notes / Fix (Optional)

- **Cause:** Direct container transfer into `discContainer` was used without dispatching physical pawn hauling and reservation jobs.
- **Fix:** Route delivery through dedicated job drivers (`JobDriver_HaulDiscToContainer`) and workgivers (`WorkGiver_HaulTo...`) so pawns reserve, carry, and deposit the disc.

## Reopen Notes (Optional)

### First reopen - resolved

Disk teleports to the scanner; a pawn should need to deliver the object instead.

### Second reopen - resolved

error is shown when ordering "load imprint disk" from the machine UI options:

```text
Krystal started 10 jobs in one tick. newJob=Eugenics_HaulDiscToContainer (Job_519) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841 jobGiver=RimWorld.JobGiver_Work jobList=(Eugenics_HaulDiscToContainer (Job_509) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841) (Eugenics_HaulDiscToContainer (Job_510) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841) (Eugenics_HaulDiscToContainer (Job_511) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841) (Eugenics_HaulDiscToContainer (Job_512) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841) (Eugenics_HaulDiscToContainer (Job_513) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841) (Eugenics_HaulDiscToContainer (Job_514) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841) (Eugenics_HaulDiscToContainer (Job_515) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841) (Eugenics_HaulDiscToContainer (Job_516) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841) (Eugenics_HaulDiscToContainer (Job_517) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841) (Eugenics_HaulDiscToContainer (Job_518) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841) (Eugenics_HaulDiscToContainer (Job_519) A = Thing_NeuralBlueprintDisk18819 B = Thing_NeuralScanner18841)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.AI.JobUtility:TryStartErrorRecoverJob (Verse.Pawn,string,System.Exception,Verse.AI.JobDriver)
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.JobDriver:EndJobWith (Verse.AI.JobCondition)
Verse.AI.JobDriver:CheckCurrentToilEndOrFail ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:Notify_PatherArrived ()
Verse.AI.Pawn_PathFollower:PatherArrived ()
Verse.AI.Pawn_PathFollower:StartPath (Verse.LocalTargetInfo,Verse.AI.PathEndMode)
Verse.AI.Toils_Goto/<>c__DisplayClass1_0:<GotoThing>b__0 ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.JobDriver:EndJobWith (Verse.AI.JobCondition)
Verse.AI.JobDriver:CheckCurrentToilEndOrFail ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:Notify_PatherArrived ()
Verse.AI.Pawn_PathFollower:PatherArrived ()
Verse.AI.Pawn_PathFollower:StartPath (Verse.LocalTargetInfo,Verse.AI.PathEndMode)
Verse.AI.Toils_Goto/<>c__DisplayClass1_0:<GotoThing>b__0 ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.JobDriver:EndJobWith (Verse.AI.JobCondition)
Verse.AI.JobDriver:CheckCurrentToilEndOrFail ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:Notify_PatherArrived ()
Verse.AI.Pawn_PathFollower:PatherArrived ()
Verse.AI.Pawn_PathFollower:StartPath (Verse.LocalTargetInfo,Verse.AI.PathEndMode)
Verse.AI.Toils_Goto/<>c__DisplayClass1_0:<GotoThing>b__0 ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.JobDriver:EndJobWith (Verse.AI.JobCondition)
Verse.AI.JobDriver:CheckCurrentToilEndOrFail ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:Notify_PatherArrived ()
Verse.AI.Pawn_PathFollower:PatherArrived ()
Verse.AI.Pawn_PathFollower:StartPath (Verse.LocalTargetInfo,Verse.AI.PathEndMode)
Verse.AI.Toils_Goto/<>c__DisplayClass1_0:<GotoThing>b__0 ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.JobDriver:EndJobWith (Verse.AI.JobCondition)
Verse.AI.JobDriver:CheckCurrentToilEndOrFail ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:Notify_PatherArrived ()
Verse.AI.Pawn_PathFollower:PatherArrived ()
Verse.AI.Pawn_PathFollower:StartPath (Verse.LocalTargetInfo,Verse.AI.PathEndMode)
Verse.AI.Toils_Goto/<>c__DisplayClass1_0:<GotoThing>b__0 ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.JobDriver:EndJobWith (Verse.AI.JobCondition)
Verse.AI.JobDriver:CheckCurrentToilEndOrFail ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:Notify_PatherArrived ()
Verse.AI.Pawn_PathFollower:PatherArrived ()
Verse.AI.Pawn_PathFollower:StartPath (Verse.LocalTargetInfo,Verse.AI.PathEndMode)
Verse.AI.Toils_Goto/<>c__DisplayClass1_0:<GotoThing>b__0 ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.JobDriver:EndJobWith (Verse.AI.JobCondition)
Verse.AI.JobDriver:CheckCurrentToilEndOrFail ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:Notify_PatherArrived ()
Verse.AI.Pawn_PathFollower:PatherArrived ()
Verse.AI.Pawn_PathFollower:StartPath (Verse.LocalTargetInfo,Verse.AI.PathEndMode)
Verse.AI.Toils_Goto/<>c__DisplayClass1_0:<GotoThing>b__0 ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.JobDriver:EndJobWith (Verse.AI.JobCondition)
Verse.AI.JobDriver:CheckCurrentToilEndOrFail ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:Notify_PatherArrived ()
Verse.AI.Pawn_PathFollower:PatherArrived ()
Verse.AI.Pawn_PathFollower:StartPath (Verse.LocalTargetInfo,Verse.AI.PathEndMode)
Verse.AI.Toils_Goto/<>c__DisplayClass1_0:<GotoThing>b__0 ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.JobDriver:EndJobWith (Verse.AI.JobCondition)
Verse.AI.JobDriver:CheckCurrentToilEndOrFail ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:Notify_PatherArrived ()
Verse.AI.Pawn_PathFollower:PatherArrived ()
Verse.AI.Pawn_PathFollower:StartPath (Verse.LocalTargetInfo,Verse.AI.PathEndMode)
Verse.AI.Toils_Goto/<>c__DisplayClass1_0:<GotoThing>b__0 ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.JobDriver:EndJobWith (Verse.AI.JobCondition)
Verse.AI.JobDriver:CheckCurrentToilEndOrFail ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.JobDriver:Notify_PatherArrived ()
Verse.AI.Pawn_PathFollower:PatherArrived ()
Verse.AI.Pawn_PathFollower:StartPath (Verse.LocalTargetInfo,Verse.AI.PathEndMode)
Verse.AI.Toils_Goto/<>c__DisplayClass1_0:<GotoThing>b__0 ()
Verse.AI.JobDriver:TryActuallyStartNextToil ()
Verse.AI.JobDriver:ReadyForNextToil ()
Verse.AI.Pawn_JobTracker:StartJob (Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
Verse.AI.Pawn_JobTracker:EndCurrentJob (Verse.AI.JobCondition,bool,bool)
Verse.AI.Pawn_JobTracker:JobTrackerTickInterval (int)
Verse.Pawn:TickInterval (int)
Verse.Thing:DoTick ()
Verse.TickList:Tick ()
Verse.TickManager:DoSingleTick ()
Verse.TickManager:TickManagerUpdate ()
Verse.Game:UpdatePlay ()
Verse.Root_Play:Update ()

```

#### Resolution:

- **Root Cause:**
  - `JobDriver_HaulDiscToContainer` declared `FailOnDespawnedOrNull(DiscInd)` at the job driver level. When `Toils_Haul.StartCarryThing` picked up the item, it despawned from the map into `carryTracker`, causing the job to fail immediately on the next toil.
  - Concurrently, `Tick()` on the container buildings cleared `targetDisc` whenever `!targetDisc.Spawned`, failing `CanAcceptDisc` checks while carried.
  - When stackCount > 1, `StartCarryThing` split off a new instance, leaving `targetDisc` mismatched.
  - When the job failed and dropped the disc, `JobGiver_Work` reissued the job 10 times in one tick.
- **Fix:**
  - In `JobDriver_HaulDiscToContainer`, changed job-level fail condition to `FailOnDestroyedOrNull(DiscInd)` and scoped `FailOnDespawnedNullOrForbidden(DiscInd)` strictly to the approach toil (`GotoThing`).
  - Added a finish action on `StartCarryThing` to update `Dest`'s `targetDisc` to the carried entity, and updated `CanAcceptDisc` to accept carried items.
  - Updated `Tick()` across `Building_NeuralScanner`, `Building_EmbryoSplicingBench`, and `CompGrowthVatImprinter` so `targetDisc` is preserved while carried by a pawn (`targetDisc.holdingOwner?.Owner is Pawn_CarryTracker || targetDisc.holdingOwner?.Owner is Pawn`).
  - Updated `TryAcceptDisc`/`TryLoadDisc` to reliably clear `targetDisc = null` upon any successful deposit.
