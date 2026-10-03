using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace OrganicConstructs
{
    [HarmonyPatch(typeof(Building_GrowthVat), "Tick")]
    public static class Patch_GrowthVat_Tick
    {

        [HarmonyPostfix]
        public static void Postfix(Building_GrowthVat __instance)
        {
            if (!__instance.Spawned) return;

            // Check if vat is operational (powered and has nutrition if required)
            CompPowerTrader power = __instance.GetComp<CompPowerTrader>();
            if (power != null && !power.PowerOn) return;

            // ── Phase 1: 3-Day Embryo Gestation ──────────────────────────────
            // Vanilla takes 9 days (540,000 ticks). 
            // We want construct embryos to complete in 3 days (180,000 ticks).
            // That requires 540,000 / 180,000 = 3 ticks per tick.
            // Vanilla adds 1. We add +2 on every tick.
            if (__instance.selectedEmbryo != null && ConstructUtility.IsConstructEmbryo(__instance.selectedEmbryo))
            {
                Traverse trav = Traverse.Create(__instance);
                int gest = trav.Field("gestationTicks").GetValue<int>();
                if (gest < 540000)
                {
                    trav.Field("gestationTicks").SetValue(gest + 2);
                }
            }

            // ── Phase 2: 12-Day In-Vat Maturation (Age 0 -> 13) ───────────────
            // Target: reach age 13 (46,800,000 biological ticks) in 12 in-game days (720,000 ticks).
            // Rate needed: 46,800,000 / 720,000 = 65 biological ticks per real tick.
            // Vanilla adds 20 biological ticks per real tick.
            // We add an extra 45 biological ticks per tick (20 + 45 = 65).
            Pawn occupant = Traverse.Create(__instance).Field("selectedPawn").GetValue<Pawn>();
            if (occupant != null && ConstructUtility.IsConstruct(occupant))
            {
                long ageBioTicks = occupant.ageTracker.AgeBiologicalTicks;
                const long Age13Ticks = 13L * 3600000L; // 13 biological years

                if (ageBioTicks < Age13Ticks)
                {
                    occupant.ageTracker.AgeBiologicalTicks += 45;

                    // Keep passions wiped at all times during maturation
                    if (Find.TickManager.TicksGame % 500 == 0)
                    {
                        CompGrowthVatImprinter.WipePassions(occupant);
                    }

                    // Milestone check: just reached age 13
                    if (occupant.ageTracker.AgeBiologicalTicks >= Age13Ticks)
                    {
                        occupant.ageTracker.AgeBiologicalTicks = Age13Ticks;
                        CompGrowthVatImprinter.WipePassions(occupant);

                        // Ensure construct physiology is locked
                        ConstructUtility.ApplyConstructPhysiology(occupant);

                        Messages.Message(
                            $"Construct {occupant.LabelShortCap} has reached neural maturity (Age 13) inside the growth vat and is ready to be decanted.",
                            new LookTargets(occupant, __instance),
                            MessageTypeDefOf.PositiveEvent);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Building_GrowthVat), "GetGizmos")]
    public static class Patch_GrowthVat_GetGizmos
    {
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building_GrowthVat __instance)
        {
            Pawn occupant = Traverse.Create(__instance).Field("selectedPawn").GetValue<Pawn>();
            bool isImmatureConstruct = occupant != null &&
                                      ConstructUtility.IsConstruct(occupant) &&
                                      occupant.ageTracker.AgeBiologicalYears < 13;

            foreach (Gizmo g in __result)
            {
                if (isImmatureConstruct && g is Command_Action cmd)
                {
                    string label = cmd.defaultLabel?.ToLower() ?? "";
                    if (label.Contains("eject") || label.Contains("cancel"))
                    {
                        cmd.Disable("Construct neural architecture is immature. Early decanting prior to age 13 causes fatal cellular dissolution.");
                    }
                }
                yield return g;
            }
        }
    }

    // ── Root-level growth moment suppression ────────────────────────────────────
    // Patch 1: Kill growth moment processing at its source for construct pawns.
    // TryChildGrowthMoment populates the newPassionOptions / newTraitOptions /
    // passionGainsCount out-params that BirthdayBiological uses to decide whether
    // to fire a ChoiceLetter_GrowthMoment.  Returning false (with all outs at 0)
    // prevents any growth-moment letter from being created at all.
    [HarmonyPatch(typeof(Pawn_AgeTracker), "TryChildGrowthMoment")]
    public static class Patch_AgeTracker_TryChildGrowthMoment
    {
        [HarmonyPrefix]
        public static bool Prefix(
            Pawn_AgeTracker __instance,
            ref int newPassionOptions,
            ref int newTraitOptions,
            ref int passionGainsCount)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn != null && ConstructUtility.IsConstruct(pawn))
            {
                newPassionOptions = 0;
                newTraitOptions   = 0;
                passionGainsCount = 0;
                CompGrowthVatImprinter.WipePassions(pawn);
                return false; // skip vanilla growth moment machinery entirely
            }
            return true;
        }
    }

    // Patch 2: Backstop — if any letter somehow reaches the window stack, suppress
    // the dialog for constructs.
    [HarmonyPatch(typeof(ChoiceLetter_GrowthMoment), "OpenLetter")]
    public static class Patch_ChoiceLetter_GrowthMoment_OpenLetter
    {
        [HarmonyPrefix]
        public static bool Prefix(ChoiceLetter_GrowthMoment __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn != null && ConstructUtility.IsConstruct(pawn))
            {
                // Silently discard — no dialog for constructs.
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Building_GrowthVat), "GetInspectString")]
    public static class Patch_GrowthVat_GetInspectString
    {
        [HarmonyPostfix]
        public static string Postfix(string __result, Building_GrowthVat __instance)
        {
            Pawn occupant = Traverse.Create(__instance).Field("selectedPawn").GetValue<Pawn>();
            if (occupant != null && ConstructUtility.IsConstruct(occupant))
            {
                int years = occupant.ageTracker.AgeBiologicalYears;
                if (years < 13)
                {
                    __result += $"\nConstruct Incubation: Maturing (Age {years}/13, decanting locked until neural maturity)";
                }
                else
                {
                    __result += $"\nConstruct Incubation: Neural Maturity Reached (Age {years}) - Ready to decant";
                }
            }
            else if (__instance.selectedEmbryo != null && ConstructUtility.IsConstructEmbryo(__instance.selectedEmbryo))
            {
                __result += "\nConstruct Gestation: Accelerated synthesis (3-day embryonic cycle)";
            }
            return __result;
        }
    }
}
