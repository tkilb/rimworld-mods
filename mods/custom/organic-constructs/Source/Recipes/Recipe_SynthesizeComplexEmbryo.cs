using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class Recipe_SynthesizeComplexEmbryo : RecipeWorker
    {
        private static readonly string[] CoreConstructGenes = new string[]
        {
            "Instability_Major",
            "Gene_MandatorySterility",
            "Gene_ConstructHibernation",
            "Immunity_SuperStrong",
            "Pain_Reduced",
            "Robust",
            "MeleeDamage_Strong",
            "MoveSpeed_Quick",
            "WoundHealing_Fast",
            "Superclotting"
        };

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (!base.AvailableOnNow(thing, part)) return false;
            return thing is Building_ConstructSynthesizer;
        }

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            if (thing is Building_ConstructSynthesizer)
            {
                return AcceptanceReport.WasAccepted;
            }
            return base.AvailableReport(thing, part);
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);

            Building_ConstructSynthesizer bench = billDoer?.CurJob?.targetA.Thing as Building_ConstructSynthesizer
                ?? (billDoer?.Map != null ? (GenClosest.ClosestThingReachable(billDoer.Position, billDoer.Map, ThingRequest.ForDef(ConstructDefOf.ConstructSynthesizer), PathEndMode.Touch, TraverseParms.For(billDoer)) as Building_ConstructSynthesizer
                    ?? (DefDatabase<ThingDef>.GetNamedSilentFail("ShipConstructSynthesizer") != null ? GenClosest.ClosestThingReachable(billDoer.Position, billDoer.Map, ThingRequest.ForDef(DefDatabase<ThingDef>.GetNamedSilentFail("ShipConstructSynthesizer")), PathEndMode.Touch, TraverseParms.For(billDoer)) as Building_ConstructSynthesizer : null)) : null);

            CompGenomeBlueprint blueprint = bench?.LoadedBlueprintComp;
            if (blueprint == null || blueprint.genes.NullOrEmpty())
            {
                Messages.Message(
                    "Synthesis aborted: No genome blueprint disc loaded in construct synthesizer. Ingredients lost.",
                    bench ?? (Thing)billDoer,
                    MessageTypeDefOf.RejectInput);
                return;
            }

            float discStability = ConstructStabilityUtility.CalculateStability(blueprint.ComplexityTotal, blueprint.MetabolicTotal, blueprint.genes);
            float baseCollapseChance = (1f - discStability) * 0.5f;

            // Failure roll on botch based on doctor/intellectual skill and room cleanliness
            int doctorSkill = billDoer?.skills?.GetSkill(SkillDefOf.Medicine)?.Level
                           ?? (billDoer?.skills?.GetSkill(SkillDefOf.Intellectual)?.Level ?? 6);
            float roomCleanliness = billDoer?.GetRoom()?.GetStat(RoomStatDefOf.Cleanliness) ?? 0f;

            float cleanlinessAdjustment = roomCleanliness < 0f
                ? Mathf.Abs(roomCleanliness) * 0.15f
                : -roomCleanliness * 0.05f;

            float failureChance = Mathf.Clamp(baseCollapseChance - (doctorSkill * 0.01f) + cleanlinessAdjustment, 0.02f, 0.80f);

            IntVec3 dropLoc = billDoer?.Position ?? bench?.Position ?? IntVec3.Invalid;
            Map map = billDoer?.Map ?? bench?.Map;

            if (Rand.Chance(failureChance))
            {
                if (map != null && dropLoc.IsValid)
                {
                    Thing paste = ThingMaker.MakeThing(ConstructDefOf.GeneticNutrientPaste);
                    paste.stackCount = 1;
                    GenPlace.TryPlaceThing(paste, dropLoc, map, ThingPlaceMode.Near);
                }

                Messages.Message(
                    $"Synthesis botched! Genetic instability caused embryo structure to collapse. 1x Genetic Nutrient Paste reclaimed (Failure chance was {failureChance:P0}).",
                    new TargetInfo(dropLoc, map),
                    MessageTypeDefOf.NegativeEvent);
                return;
            }

            HumanEmbryo embryo = (HumanEmbryo)ThingMaker.MakeThing(ThingDefOf.HumanEmbryo);

            CompHasPawnSources sources = embryo.TryGetComp<CompHasPawnSources>();
            if (sources?.pawnSources != null)
            {
                sources.pawnSources.Clear();
            }

            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            string bpName = !string.IsNullOrEmpty(blueprint.templateLabel) ? blueprint.templateLabel : "Complex Construct";
            if (quality != null)
            {
                quality.isConstruct = true;
                quality.editCount = 1;
                quality.blueprintLabel = bpName;
            }

            if (embryo.GeneSet == null)
            {
                embryo.TryPopulateGenes();
                if (embryo.GeneSet == null)
                {
                    HarmonyLib.AccessTools.Field(typeof(GeneSetHolderBase), "geneSet")?.SetValue(embryo, new GeneSet());
                }
            }

            if (embryo.GeneSet != null)
            {
                List<GeneDef> current = embryo.GeneSet.GenesListForReading.ToList();
                for (int i = 0; i < current.Count; i++)
                {
                    embryo.GeneSet.Debug_RemoveGene(current[i]);
                }

                // Add genes from loaded blueprint
                for (int i = 0; i < blueprint.genes.Count; i++)
                {
                    GeneDef g = blueprint.genes[i];
                    if (g != null)
                    {
                        embryo.GeneSet.AddGene(g);
                    }
                }

                // Ensure core construct genes are always present
                for (int i = 0; i < CoreConstructGenes.Length; i++)
                {
                    GeneDef g = DefDatabase<GeneDef>.GetNamedSilentFail(CoreConstructGenes[i]);
                    if (g != null && !embryo.GeneSet.GenesListForReading.Contains(g))
                    {
                        embryo.GeneSet.AddGene(g);
                    }
                }

                // Aesthetic defaulting: bald & beardless unless overridden by cosmetic genes
                bool hasHairGene = embryo.GeneSet.GenesListForReading.Any(g => g.hairTagFilter != null || g.forcedHair != null);
                if (!hasHairGene)
                {
                    GeneDef bald = DefDatabase<GeneDef>.GetNamedSilentFail("Hair_BaldOnly");
                    if (bald != null && !embryo.GeneSet.GenesListForReading.Contains(bald))
                    {
                        embryo.GeneSet.AddGene(bald);
                    }
                }

                bool hasBeardGene = embryo.GeneSet.GenesListForReading.Any(g => g.beardTagFilter != null);
                if (!hasBeardGene)
                {
                    GeneDef noBeard = DefDatabase<GeneDef>.GetNamedSilentFail("Beard_NoBeardOnly");
                    if (noBeard != null && !embryo.GeneSet.GenesListForReading.Contains(noBeard))
                    {
                        embryo.GeneSet.AddGene(noBeard);
                    }
                }
            }

            if (map != null && dropLoc.IsValid)
            {
                GenPlace.TryPlaceThing(embryo, dropLoc, map, ThingPlaceMode.Near);
            }

            Messages.Message(
                $"Successfully synthesized Complex Construct embryo imprinted with '{bpName}' ({blueprint.genes.Count} genes).",
                embryo,
                MessageTypeDefOf.PositiveEvent);
        }
    }
}
