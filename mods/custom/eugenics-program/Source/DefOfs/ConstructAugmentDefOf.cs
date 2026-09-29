using RimWorld;
using Verse;

namespace EugenicsProgram
{
    [DefOf]
    public static class ConstructAugmentDefOf
    {
        // Hediffs
        public static HediffDef Eugenics_ConstructAssimilation;
        public static HediffDef Eugenics_InterruptedStasis;

        // Genes
        public static GeneDef Gene_ConstructHibernation;

        // Traits
        public static TraitDef Trait_ConstructAsset; 

        // Genes to check
        public static GeneDef Gene_ConstructPsychology;

        static ConstructAugmentDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ConstructAugmentDefOf));
        }
    }
}
