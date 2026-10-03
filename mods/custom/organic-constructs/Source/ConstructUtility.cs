using RimWorld;
using Verse;

namespace OrganicConstructs
{
    public static class ConstructUtility
    {
        public static bool IsConstruct(Pawn pawn)
        {
            if (pawn == null) return false;

            if (pawn.story != null)
            {
                if (pawn.story.Childhood?.defName == "Construct_Childhood" || pawn.story.Adulthood?.defName == "Construct_Adulthood")
                {
                    return true;
                }

                TraitDef traitAsset = ConstructDefOf.Trait_ConstructAsset 
                    ?? NeuralImprintDefOf.Trait_ConstructAsset 
                    ?? DefDatabase<TraitDef>.GetNamedSilentFail("Trait_ConstructAsset");
                if (traitAsset != null && pawn.story.traits != null && pawn.story.traits.HasTrait(traitAsset))
                {
                    return true;
                }
            }

            if (pawn.genes != null)
            {
                GeneDef hiberGene = ConstructDefOf.Gene_ConstructHibernation
                    ?? ConstructAugmentDefOf.Gene_ConstructHibernation
                    ?? DefDatabase<GeneDef>.GetNamedSilentFail("Gene_ConstructHibernation");
                if (hiberGene != null && pawn.genes.HasActiveGene(hiberGene)) return true;

                foreach (Gene g in pawn.genes.GenesListForReading)
                {
                    if (g is Gene_ConstructHibernation)
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
                    if (g != null && (g.defName == "Gene_ConstructHibernation" || g.geneClass == typeof(Gene_ConstructHibernation)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void AssignConstructBackstories(Pawn pawn)
        {
            if (pawn?.story == null) return;

            BackstoryDef childDef = ConstructDefOf.Construct_Childhood 
                ?? DefDatabase<BackstoryDef>.GetNamedSilentFail("Construct_Childhood");
            if (childDef != null)
            {
                pawn.story.Childhood = childDef;
            }

            if (pawn.ageTracker == null || pawn.ageTracker.AgeBiologicalYears >= 13)
            {
                BackstoryDef adultDef = ConstructDefOf.Construct_Adulthood 
                    ?? DefDatabase<BackstoryDef>.GetNamedSilentFail("Construct_Adulthood");
                if (adultDef != null)
                {
                    pawn.story.Adulthood = adultDef;
                }
            }

            if (pawn.story.traits != null)
            {
                TraitDef traitAsset = ConstructDefOf.Trait_ConstructAsset 
                    ?? NeuralImprintDefOf.Trait_ConstructAsset 
                    ?? DefDatabase<TraitDef>.GetNamedSilentFail("Trait_ConstructAsset");
                if (traitAsset != null && !pawn.story.traits.HasTrait(traitAsset))
                {
                    pawn.story.traits.GainTrait(new Trait(traitAsset));
                }

                TraitDef psychopathDef = TraitDefOf.Psychopath;
                if (psychopathDef != null && !pawn.story.traits.HasTrait(psychopathDef))
                {
                    pawn.story.traits.GainTrait(new Trait(psychopathDef));
                }
            }
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
            AssignConstructBackstories(pawn);

            pawn.Drawer?.renderer?.SetAllGraphicsDirty();
        }
    }
}
