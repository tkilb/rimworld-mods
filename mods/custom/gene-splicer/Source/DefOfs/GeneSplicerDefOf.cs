using RimWorld;
using Verse;

namespace GeneSplicer
{
    [DefOf]
    public static class GeneSplicerDefOf
    {
        public static ThingDef EmbryoSplicingBench;
        public static ThingDef GeneticNutrientPaste;

        public static JobDef LoadEmbryoToBench;
        public static JobDef SpliceEmbryo;

        public static ThoughtDef ChildGeneticallyAltered;
        public static ThoughtDef ChildBiologicallyUpgraded;

        static GeneSplicerDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(GeneSplicerDefOf));
        }
    }
}
