# Bug: Constructs in Stasis Rescued to Medical Beds & Lack of Fallback Carry Option

**Status:** Ready for QA

## Description

Whenever constructs enter hibernation stasis in their assigned bed, colony doctors autonomously "rescue" them and carry them to hospital/medical beds. In addition, the game displays a "Colonist needs rescue" alert banner and records a downed tale. Furthermore, if a construct collapses into emergency stasis or shutdown on the floor outside of a bed, there was no direct option for colonists to carry/haul them to their assigned bed to begin stasis.

## Steps to Reproduce (Optional)

1. Have a construct enter stasis in their personal bed while medical beds exist in the colony.
2. Observe a doctor pawn prioritizing a `Rescue` job to drag the construct to a medical bed.
3. Observe the "Colonist needs rescue" alert banner appearing while the construct is resting in stasis.
4. If a construct collapses outside a bed due to emergency stasis, observe the lack of an explicit right-click command to carry them to their personal bed.

---

## Dev Notes / Fix

- **Cause:** 
  - `Construct_InStasis` capped `Consciousness` at 10% (`<setMax>0.1</setMax>`), placing the pawn in the `pawn.Downed == true` state without disabling the downed incident tale (`recordDownedTale`).
  - Base game doctor AI (`WorkGiver_RescueDowned`, `HealthAIUtility.WantsToBeRescued`, `HealthAIUtility.CanRescueNow`) considers any downed colonist resting in a standard non-medical bed as needing to be hospitalized, actively relocating them to an available medical bed.
  - `Alert_ColonistNeedsRescuing.NeedsRescue` flagged any downed colonist regardless of whether they were intentionally hibernating in a bed.
  - No dedicated float menu option existed to allow colonists to pick up a construct collapsed on the floor in emergency shutdown/stasis and bring them directly to their assigned bed.

- **Fix:**
  - Added `<recordDownedTale>false</recordDownedTale>` to `Construct_InStasis` in [Hediffs_Neural.xml](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/HediffDefs/Hediffs_Neural.xml).
  - Added Harmony patches in [Patch_StasisSleep.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs):
    - `HealthAIUtility.WantsToBeRescued`: returns `false` if the construct is in stasis and already in a bed.
    - `HealthAIUtility.CanRescueNow`: returns `false` for unforced autonomous checks if the construct is in stasis and in a bed.
    - `WorkGiver_RescueDowned.HasJobOnThing`: suppresses autonomous rescue jobs targeting stasis constructs in bed.
    - `Alert_ColonistNeedsRescuing.NeedsRescue`: suppresses the alert when the construct is safely in bed, while keeping it active if they collapse on the floor.
    - `WorkGiver_TakeToBed.FindBed`: routes rescued stasis constructs to their assigned non-medical bed instead of hospital beds.
    - `Pawn_JobTracker.Notify_TuckedIntoBed`: updated to handle emergency shutdown constructs, automatically placing them into `Construct_EnterConstructStasis` once tucked into bed.
  - Created [FloatMenuOptionProvider_CarryConstructToBed.cs](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/UI/FloatMenuOptionProvider_CarryConstructToBed.cs) and registered it in `ConstructStasisMenuInitializer`: provides a direct right-click option (`"Carry <Name> to bed (<Bed>)"`) to carry a construct who collapsed on the ground to their assigned personal bed.
