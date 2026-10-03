using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [DefOf]
    public static class NeuralImprintDefOf
    {
        public static TraitDef Trait_ConstructAsset;

        static NeuralImprintDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(NeuralImprintDefOf));
        }
    }
}
