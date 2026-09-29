using System.Linq;
using RimWorld;
using Verse;

namespace GeneSplicer
{
    public static class GeneSplicerParentalUtility
    {
        public static void ApplyParentalThoughts(HumanEmbryo embryo)
        {
            if (embryo == null) return;

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
                ? GeneSplicerDefOf.ChildBiologicallyUpgraded
                : GeneSplicerDefOf.ChildGeneticallyAltered;

            if (thought != null)
            {
                parent.needs.mood.thoughts.memories.TryGainMemory(thought);
            }
        }
    }
}
