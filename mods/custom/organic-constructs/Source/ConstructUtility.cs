using RimWorld;
using UnityEngine;
using Verse;

namespace OrganicConstructs
{
    public static class ConstructUtility
    {
        public static bool IsConstruct(Pawn pawn)
        {
            if (pawn == null) return false;

            if (pawn.kindDef?.defName == "Construct_Colonist" || pawn.kindDef?.defName == "Construct_CrewMember")
            {
                return true;
            }

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
                if (pawn.genes.Xenotype?.defName == "Construct_Base")
                {
                    return true;
                }

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

            if (pawn.story?.traits != null)
            {
                // Clear any randomly rolled human personality traits so they don't introduce unwanted stat/skill modifiers
                pawn.story.traits.allTraits.Clear();

                TraitDef traitAsset = ConstructDefOf.Trait_ConstructAsset 
                    ?? NeuralImprintDefOf.Trait_ConstructAsset 
                    ?? DefDatabase<TraitDef>.GetNamedSilentFail("Trait_ConstructAsset");
                if (traitAsset != null)
                {
                    pawn.story.traits.GainTrait(new Trait(traitAsset));
                }

                TraitDef psychopathDef = TraitDefOf.Psychopath;
                if (psychopathDef != null)
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
                // Uniform light brown (latino) synthetic dermal tone
                pawn.story.skinColorOverride = new Color(205f / 255f, 155f / 255f, 115f / 255f);

                HeadTypeDef neutralHead = DefDatabase<HeadTypeDef>.GetNamedSilentFail("Male_AverageNormal");
                if (neutralHead != null)
                {
                    pawn.story.headType = neutralHead;
                }
            }

            if (pawn.style != null && !hasCustomBeardGene)
            {
                pawn.style.beardDef = BeardDefOf.NoBeard;
            }

            ConstructNameUtility.AssignConstructNameIfNeeded(pawn);
            AssignConstructBackstories(pawn);

            if (pawn.genes != null)
            {
                XenotypeDef constructXenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail("Construct_Base");
                if (constructXenotype != null)
                {
                    if (pawn.genes.Xenotype != constructXenotype)
                    {
                        pawn.genes.SetXenotype(constructXenotype);
                    }

                    GeneDef thinGene = DefDatabase<GeneDef>.GetNamedSilentFail("Body_Thin");
                    if (thinGene != null && !pawn.genes.HasActiveGene(thinGene))
                    {
                        pawn.genes.AddGene(thinGene, xenogene: false);
                    }

                    GeneDef skinGene = DefDatabase<GeneDef>.GetNamedSilentFail("Skin_ConstructTan");
                    if (skinGene != null && !pawn.genes.HasActiveGene(skinGene))
                    {
                        pawn.genes.AddGene(skinGene, xenogene: false);
                    }
                }
            }

            pawn.Drawer?.renderer?.SetAllGraphicsDirty();
        }
    }
}
