using RimWorld;
using Verse;

namespace EugenicsProgram
{
    public class Gene_ConstructPsychology : Gene
    {
        private const int SocialCheckIntervalTicks = 250;

        public static bool IsConstruct(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            if (pawn.genes != null)
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (gene is Gene_ConstructPsychology || gene.def.defName == "Gene_ConstructPsychology")
                    {
                        return true;
                    }
                }
            }

            if (pawn.story?.traits != null)
            {
                if (pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamedSilentFail("Trait_ConstructAsset")))
                {
                    return true;
                }
            }

            return false;
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
            if (pawn?.needs == null)
            {
                return;
            }

            // Suppress loneliness or social recreation starvation according to XML config
            Need socialNeed = pawn.needs.TryGetNeed(DefDatabase<NeedDef>.GetNamedSilentFail("Social"));
            if (socialNeed != null && socialNeed.CurLevelPercentage < MinSocialLevel)
            {
                socialNeed.CurLevelPercentage = MinSocialLevel;
            }
        }
    }
}
