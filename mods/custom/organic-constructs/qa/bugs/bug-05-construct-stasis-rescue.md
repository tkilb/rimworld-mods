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

- **Root Architecture Resolution (Approach 1 - Native Deathrest Pipeline):**
  - Rather than fighting vanilla sleep mechanics with piecemeal patches (which caused 06:00 dawn timetable interrupts, doctor rescue loops, and posture flip-flopping), construct hibernation stasis was refactored to hook directly into Biotech's native `Deathresting` engine pipeline:
  - **Pawn.Deathresting Hook ([`Patch_StasisSleep.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs)):** Added a postfix patch to `Pawn.Deathresting` returning `true` during active stasis or `Construct_EnterConstructStasis`. This natively delegates:
    - Locking sleeper head orientation strictly to South (`Rot4.South`) without sideways rotation.
    - Drawing bed blankets over the sleeper body.
    - Suppressing disturbed sleep thoughts.
    - Freezing bleeding and hunger/food need decay.
    - Suppressing the drafting gizmo.
    - Skipping doctor rescue AI when already in an assigned bed.
  - **Downed & Comatose Rest ([`Hediffs_Neural.xml`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Defs/HediffDefs/Hediffs_Neural.xml)):** Added `recordDownedTale: false` and `Consciousness: setMax 0.1` to `Construct_InStasis`. Like Biotech's `Deathrest`, this ensures `ThinkNode_ConditionalMustKeepLyingDown` permanently satisfies without evaluating timetables or falling through to `JobGiver_Work` at dawn.
  - **Toils & Weapons ([`Gene_ConstructHibernation.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Genes/Gene_ConstructHibernation.cs) & [`Patch_StasisSleep.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs)):**
    - Passed `deathrest: true` to `Toils_LayDown.LayDown`.
    - Added `Patch_Pawn_DropAndForbidEverything` to prevent constructs from dropping their equipped weapons upon entering stasis.
    - Cleaned up obsolete workaround patches (`Patch_PawnUtility_GetPosture`, `Patch_PawnRenderer_LayingFacing`, `Patch_RestUtility_ShouldWakeUp`, `Patch_RestUtility_CanFallAsleep`, etc.) that were conflicting with vanilla bed and posture logic.
  - **Engine Rescue Resolution ([`Patch_StasisSleep.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs)):**
    - Deep disassembly of RimWorld's `Alert_ColonistNeedsRescuing.NeedsRescue` and `HealthAIUtility.WantsToBeRescued` confirmed that both systems check `!pawn.InBed()`.
    - Because `Consciousness: setMax 0.1` marks the pawn as downed, vanilla `RestUtility.CurrentBed` and `RestUtility.InBed` failed to recognize the sleeper as in bed whenever `pawn.CurJobDef` was briefly interrupted or transitioning, causing doctors to target the sleeper for rescue and firing the critical alert.
    - Added comprehensive patches:
      - `Patch_RestUtility_CurrentBed` & `Patch_RestUtility_CurrentBed_Slot`: Accurately resolves the underlying `Building_Bed` at the pawn's cell whenever the pawn is in stasis, regardless of job state.
      - `Patch_RestUtility_InBed`: Returns `true` when a construct in stasis occupies any bed or sleeping spot.
      - `Patch_HealthAIUtility_WantsToBeRescued`: Returns `false` while in stasis in a bed, preventing rescue jobs. Returns `true` if downed in the open field so colonists can rescue them to bed.
      - `Patch_Alert_ColonistNeedsRescuing_NeedsRescue`: Suppresses the "Colonist needs rescue" alert banner when the construct is safely resting in a bed.
      - `Patch_WorkGiver_RescueDowned_HasJobOnThing`: Blocks doctors from issuing rescue jobs on constructs safely in bed.
  - Created [`FloatMenuOptionProvider_CarryConstructToBed.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/UI/FloatMenuOptionProvider_CarryConstructToBed.cs) and registered it in `ConstructStasisMenuInitializer`: provides a direct right-click option (`"Carry <Name> to bed (<Bed>)"`) to carry a construct who collapsed on the ground into their assigned personal bed.
  - **Rest Need Dynamic Regeneration & Lock ([`Patch_StasisSleep.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs)):**
    - Resolved the stasis exhaustion bug where the rest meter drained to 0% during 48-hour hibernation:
      - `Patch_Need_Rest_NeedInterval`: Dynamically regenerates rest need while in stasis in a bed at the bed's native rest rate until 100%, and blocks rest decay so the pawn never falls into Tired or Exhausted states during the remainder of the 48-hour cycle.
      - `Patch_Need_Rest_Resting`: Ensures the Resting getter returns `true` while in stasis in a bed, rendering the active recovery indicator (green arrow pointing up) on the Rest need UI.
  - **Weapon Drop Prevention on Entering Stasis ([`Patch_StasisSleep.cs`](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/Source/Patches/Patch_StasisSleep.cs)):**
    - Deep engine analysis revealed why pawns "sometimes" dropped equipped primary weapons upon entering stasis:
      - When `Construct_InStasis` is applied with `Consciousness: setMax 0.1`, Manipulation capacity scales with consciousness down to <= 10%.
      - RimWorld's `Pawn_HealthTracker.CheckForStateChange` checks `!capacities.CapableOf(PawnCapacityDefOf.Manipulation)`. If a pawn has any minor scratch, scar, or float rounding placing Manipulation `< 0.10`, `CheckForStateChange` directly calls `pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out _, pawn.PositionHeld)`, bypassing `Pawn.DropAndForbidEverything`.
      - Added Harmony patches:
        - `Patch_Pawn_EquipmentTracker_TryDropEquipment`: Intercepts and blocks weapon drops during `Construct_EnterConstructStasis` or active stasis while preserving drops on pawn death (`pawn.Dead`), active player orders (`JobDefOf.DropEquipment`), or stripping.
        - `Patch_Pawn_EquipmentTracker_DropAllEquipment`: Safeguard against mass equipment drops during stasis transitions.
        - `Patch_Pawn_Strip`: Tracks active `Pawn.Strip` calls so intentional stripping of constructs by colonists or enemies remains fully functional.
        - Updated `Patch_Pawn_DropAndForbidEverything` to ensure dead pawns drop their belongings normally.

