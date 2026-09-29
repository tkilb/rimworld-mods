using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight))]
    public static class Patch_RomanceAttempt
    {
        public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (Gene_ConstructPsychology.IsConstruct(initiator) || Gene_ConstructPsychology.IsConstruct(recipient))
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.RandomSelectionWeight))]
    public static class Patch_MarriageProposal
    {
        public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (Gene_ConstructPsychology.IsConstruct(initiator) || Gene_ConstructPsychology.IsConstruct(recipient))
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_DeepTalk), nameof(InteractionWorker_DeepTalk.RandomSelectionWeight))]
    public static class Patch_DeepTalk
    {
        public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (Gene_ConstructPsychology.IsConstruct(initiator) || Gene_ConstructPsychology.IsConstruct(recipient))
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_Chitchat), nameof(InteractionWorker_Chitchat.RandomSelectionWeight))]
    public static class Patch_Chitchat
    {
        public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (Gene_ConstructPsychology.IsConstruct(initiator) || Gene_ConstructPsychology.IsConstruct(recipient))
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }
}
