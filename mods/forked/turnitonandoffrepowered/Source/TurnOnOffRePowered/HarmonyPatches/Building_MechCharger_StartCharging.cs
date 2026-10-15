using HarmonyLib;
using RimWorld;

namespace TurnOnOffRePowered.HarmonyPatches;

[HarmonyPatch(typeof(Building_MechCharger), "StartCharging")]
public static class Building_MechCharger_StartCharging
{
    public static void Postfix(Building_MechCharger __instance)
    {
        TurnItOnUtility.AddBuildingUsed(__instance);
    }
}
