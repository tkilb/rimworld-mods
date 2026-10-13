using RimWorld;
using Verse;

namespace OrganicConstructs
{
    /// <summary>
    /// Explicitly displays a disabled float menu option when a non-construct colonist
    /// right-clicks a ConstructStasisPod, ensuring absolute clarity that ONLY constructs may use it.
    /// </summary>
    public class FloatMenuOptionProvider_NonConstructStasisPodReject : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            Pawn pawn = context?.FirstSelectedPawn;
            return pawn != null && pawn.Spawned && !ConstructUtility.IsConstruct(pawn);
        }

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            if (clickedThing?.def?.defName == "ConstructStasisPod")
            {
                return new FloatMenuOption("Cannot use: Only biological constructs may use stasis pods.", null);
            }
            return null;
        }
    }
}
