using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [DefOf]
    public static class ConstructAugmentDefOf
    {
        // Hediffs
        public static HediffDef Construct_Assimilation;
        public static HediffDef Construct_InterruptedStasis;

        // Genes
        public static GeneDef Gene_ConstructHibernation;
        public static GeneDef Gene_ConstructPsychology;

        // Traits
        public static TraitDef Trait_ConstructAsset;

        static ConstructAugmentDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ConstructAugmentDefOf));
        }
    }
}
