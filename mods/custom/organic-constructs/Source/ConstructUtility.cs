using RimWorld;
using Verse;

namespace OrganicConstructs
{
    public static class ConstructUtility
    {
        public static bool IsConstruct(Pawn pawn)
        {
            if (pawn == null) return false;

            if (pawn.story?.traits != null)
            {
                TraitDef traitAsset = ConstructDefOf.Trait_ConstructAsset 
                    ?? NeuralImprintDefOf.Trait_ConstructAsset 
                    ?? DefDatabase<TraitDef>.GetNamedSilentFail("Trait_ConstructAsset");
                if (traitAsset != null && pawn.story.traits.HasTrait(traitAsset)) return true;
            }

            if (pawn.genes != null)
            {
                GeneDef psychGene = ConstructDefOf.Gene_ConstructPsychology 
                    ?? NeuralImprintDefOf.Gene_ConstructPsychology 
                    ?? DefDatabase<GeneDef>.GetNamedSilentFail("Gene_ConstructPsychology");
                if (psychGene != null && pawn.genes.HasActiveGene(psychGene)) return true;

                GeneDef hiberGene = ConstructDefOf.Gene_ConstructHibernation
                    ?? ConstructAugmentDefOf.Gene_ConstructHibernation
                    ?? DefDatabase<GeneDef>.GetNamedSilentFail("Gene_ConstructHibernation");
                if (hiberGene != null && pawn.genes.HasActiveGene(hiberGene)) return true;

                foreach (Gene g in pawn.genes.GenesListForReading)
                {
                    if (g is Gene_ConstructPsychology || g is Gene_ConstructHibernation)
                        return true;
                }
            }

            return false;
        }

        public static bool IsConstructEmbryo(HumanEmbryo embryo)
        {
            if (embryo == null) return false;

            CompEmbryoQuality comp = embryo.TryGetComp<CompEmbryoQuality>();
            if (comp != null && comp.isConstruct)
            {
                return true;
            }

            if (embryo.GeneSet?.GenesListForReading != null)
            {
                foreach (GeneDef g in embryo.GeneSet.GenesListForReading)
                {
                    if (g != null && (g.defName == "Gene_ConstructPsychology" || g.geneClass == typeof(Gene_ConstructPsychology)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
