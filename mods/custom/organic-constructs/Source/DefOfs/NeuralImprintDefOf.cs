using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [DefOf]
    public static class NeuralImprintDefOf
    {
        public static TraitDef Trait_ConstructAsset;
        public static GeneDef Gene_ConstructPsychology;

        static NeuralImprintDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(NeuralImprintDefOf));
        }
    }
}
