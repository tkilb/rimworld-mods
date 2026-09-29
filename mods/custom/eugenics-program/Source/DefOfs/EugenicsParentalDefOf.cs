using System.Linq;
using RimWorld;
using Verse;

namespace EugenicsProgram
{
    [DefOf]
    public static class EugenicsParentalDefOf
    {
        public static ThoughtDef ChildGeneticallyAltered;
        public static ThoughtDef ChildBiologicallyUpgraded;

        static EugenicsParentalDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EugenicsParentalDefOf));
        }
    }

    public static class EugenicsParentalUtility
    {
        public static void ApplyParentalThoughts(HumanEmbryo embryo)
        {
            if (embryo == null) return;

            CompEmbryoQuality comp = embryo.TryGetComp<CompEmbryoQuality>();
            if (comp != null && comp.isConstruct) return;

            Pawn father = embryo.Father;
            Pawn mother = embryo.Mother;

            TryApplyThoughtToParent(father);
            TryApplyThoughtToParent(mother);
        }

        private static void TryApplyThoughtToParent(Pawn parent)
        {
            if (parent == null || parent.Dead || parent.needs?.mood?.thoughts?.memories == null) return;

            bool isTranshumanist = parent.story?.traits?.allTraits.Any(t =>
                t.def != null && (t.def.defName == "Transhumanist" || t.def.defName == "BodyModder")) == true;

            ThoughtDef thought = isTranshumanist
                ? EugenicsParentalDefOf.ChildBiologicallyUpgraded
                : EugenicsParentalDefOf.ChildGeneticallyAltered;

            if (thought != null)
            {
                parent.needs.mood.thoughts.memories.TryGainMemory(thought);
            }
        }
    }
}
