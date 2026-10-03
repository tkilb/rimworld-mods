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

        public override void PostAdd()
        {
            base.PostAdd();
            ApplyConstructPhysiology(pawn);
        }

        public static void ApplyConstructPhysiology(Pawn pawn)
        {
            if (pawn == null) return;

            pawn.gender = Gender.None;

            bool hasCustomHairGene = false;
            bool hasCustomBeardGene = false;
            bool hasCustomBodyGene = false;

            if (pawn.genes != null)
            {
                foreach (Gene g in pawn.genes.GenesListForReading)
                {
                    if (g.def == null || !g.Active) continue;
                    if (g.def.hairTagFilter != null && g.def.defName != "Hair_BaldOnly")
                        hasCustomHairGene = true;
                    if (g.def.beardTagFilter != null && g.def.defName != "Beard_NoBeardOnly")
                        hasCustomBeardGene = true;
                    if (g.def.bodyType != null)
                        hasCustomBodyGene = true;
                }
            }

            if (pawn.story != null)
            {
                if (!hasCustomHairGene)
                {
                    pawn.story.hairDef = HairDefOf.Bald;
                }
                if (!hasCustomBodyGene)
                {
                    pawn.story.bodyType = BodyTypeDefOf.Thin;
                }
            }

            if (pawn.style != null && !hasCustomBeardGene)
            {
                pawn.style.beardDef = BeardDefOf.NoBeard;
            }

            ConstructNameUtility.AssignConstructNameIfNeeded(pawn);

            pawn.Drawer?.renderer?.SetAllGraphicsDirty();
        }

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
