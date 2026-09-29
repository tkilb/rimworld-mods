using RimWorld;
using Verse;

namespace OrganicConstructs
{
    public class Gene_ConstructPsychology : Gene
    {
        private const int SocialCheckIntervalTicks = 250;

        public static bool IsConstruct(Pawn pawn)
        {
            return ConstructUtility.IsConstruct(pawn);
        }

        private GeneExtension_ConstructPsychology Extension => def?.GetModExtension<GeneExtension_ConstructPsychology>();

        private int CheckInterval => Extension?.checkIntervalTicks ?? 250;

        private float MinSocialLevel => Extension?.minSocialLevel ?? 0.7f;

        public override void Tick()
        {
            base.Tick();

            if (pawn.IsHashIntervalTick(CheckInterval))
            {
                SuppressSocialNeeds();
            }
        }

        private void SuppressSocialNeeds()
        {
            if (pawn?.needs == null) return;

            Need socialNeed = pawn.needs.TryGetNeed(DefDatabase<NeedDef>.GetNamedSilentFail("Social"));
            if (socialNeed != null && socialNeed.CurLevelPercentage < MinSocialLevel)
            {
                socialNeed.CurLevelPercentage = MinSocialLevel;
            }
        }
    }
}
