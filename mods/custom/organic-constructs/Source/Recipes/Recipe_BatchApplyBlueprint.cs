using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class Recipe_BatchApplyBlueprint : RecipeWorker
    {
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

        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipeDef, Map map)
        {
            if (ingredient is HumanEmbryo)
            {
                return;
            }
            base.ConsumeIngredient(ingredient, recipeDef, map);
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);

            HumanEmbryo embryo = ingredients?.OfType<HumanEmbryo>().FirstOrDefault();
            if (embryo == null) return;

            Building_ConstructSynthesizer bench = billDoer?.CurJob?.targetA.Thing as Building_ConstructSynthesizer
                ?? (billDoer?.Map != null ? GenClosest.ClosestThingReachable(billDoer.Position, billDoer.Map, ThingRequest.ForDef(ConstructDefOf.ConstructSynthesizer), PathEndMode.Touch, TraverseParms.For(billDoer)) as Building_ConstructSynthesizer : null);

            CompGenomeBlueprint blueprint = bench?.LoadedBlueprintComp;
            if (blueprint == null || blueprint.genes.NullOrEmpty())
            {
                Messages.Message(
                    "Batch splicing aborted: No genome blueprint disc loaded in construct synthesizer.",
                    bench ?? (Thing)billDoer,
                    MessageTypeDefOf.RejectInput);
                DropEmbryo(billDoer, bench, embryo);
                return;
            }

            CompEmbryoQuality comp = embryo.TryGetComp<CompEmbryoQuality>();

            // Enforce edit limit: block further editing if already at 2 edits
            if (comp != null && comp.editCount >= 2)
            {
                Messages.Message(
                    "Batch splicing aborted: Embryo has already undergone the maximum allowed genetic edits (2).",
                    embryo,
                    MessageTypeDefOf.RejectInput);
                DropEmbryo(billDoer, bench, embryo);
                return;
            }

            // Failure roll on botch based on intellectual/doctor skill and room cleanliness
            int doctorSkill = billDoer?.skills?.GetSkill(SkillDefOf.Medicine)?.Level
                           ?? (billDoer?.skills?.GetSkill(SkillDefOf.Intellectual)?.Level ?? 6);
            float roomCleanliness = billDoer?.GetRoom()?.GetStat(RoomStatDefOf.Cleanliness) ?? 0f;

            float cleanlinessAdjustment = roomCleanliness < 0f
                ? Mathf.Abs(roomCleanliness) * 0.15f
                : -roomCleanliness * 0.05f;

            float failureChance = Mathf.Clamp(0.15f - (doctorSkill * 0.01f) + cleanlinessAdjustment, 0.02f, 0.80f);

            if (Rand.Chance(failureChance))
            {
                // Splicing botch: Cellular structure collapses, destroy embryo and spawn 1x GeneticNutrientPaste
                IntVec3 dropLoc = billDoer?.Position ?? bench?.Position ?? IntVec3.Invalid;
                Map map = billDoer?.Map ?? bench?.Map;

                if (billDoer?.carryTracker?.CarriedThing == embryo)
                {
                    billDoer.carryTracker.TryDropCarriedThing(billDoer.Position, ThingPlaceMode.Near, out _);
                }
                else if (embryo.holdingOwner != null)
                {
                    embryo.holdingOwner.Remove(embryo);
                }

                if (!embryo.Destroyed)
                {
                    embryo.Destroy(DestroyMode.Vanish);
                }

                if (map != null && dropLoc.IsValid)
                {
                    Thing paste = ThingMaker.MakeThing(ConstructDefOf.GeneticNutrientPaste);
                    paste.stackCount = 1;
                    GenPlace.TryPlaceThing(paste, dropLoc, map, ThingPlaceMode.Near);
                }

                Messages.Message(
                    $"Batch splicing botched! Instability caused embryonic structure to collapse. 1x Genetic Nutrient Paste reclaimed (Failure chance was {failureChance:P0}).",
                    new TargetInfo(dropLoc, map),
                    MessageTypeDefOf.NegativeEvent);
                return;
            }

            // Ensure GeneSet is initialized
            if (embryo.GeneSet == null)
            {
                embryo.TryPopulateGenes();
                if (embryo.GeneSet == null)
                {
                    HarmonyLib.AccessTools.Field(typeof(GeneSetHolderBase), "geneSet")?.SetValue(embryo, new GeneSet());
                }
            }

            // Apply all genes from the blueprint to the embryo
            if (embryo.GeneSet != null)
            {
                List<GeneDef> existing = embryo.GeneSet.GenesListForReading.ToList();
                for (int i = 0; i < existing.Count; i++)
                {
                    embryo.GeneSet.Debug_RemoveGene(existing[i]);
                }

                for (int i = 0; i < blueprint.genes.Count; i++)
                {
                    GeneDef g = blueprint.genes[i];
                    if (g != null)
                    {
                        embryo.GeneSet.AddGene(g);
                    }
                }
            }

            if (comp != null)
            {
                comp.isConstruct = true;
                comp.editCount++;
                comp.blueprintLabel = !string.IsNullOrEmpty(blueprint.templateLabel) ? blueprint.templateLabel : "Construct Caste";

                // Ensure core construct genes are present on construct matrices
                if (embryo.GeneSet != null)
                {
                    string[] coreGenes = new string[]
                    {
                        "Gene_ConstructPsychology",
                        "Construct_MetabolicallyEfficient",
                        "Gene_MandatorySterility",
                        "Gene_ConstructHibernation"
                    };
                    for (int i = 0; i < coreGenes.Length; i++)
                    {
                        GeneDef g = DefDatabase<GeneDef>.GetNamedSilentFail(coreGenes[i]);
                        if (g != null && !embryo.GeneSet.GenesListForReading.Contains(g))
                        {
                            embryo.GeneSet.AddGene(g);
                        }
                    }
                }
            }

            DropEmbryo(billDoer, bench, embryo);

            string bpName = !string.IsNullOrEmpty(blueprint.templateLabel) ? blueprint.templateLabel : "genome blueprint";
            Messages.Message(
                $"Successfully batch-spliced {embryo.LabelShortCap} using '{bpName}' ({blueprint.genes.Count} genes imprinted).",
                embryo,
                MessageTypeDefOf.PositiveEvent);
        }

        private static void DropEmbryo(Pawn billDoer, Building_ConstructSynthesizer bench, HumanEmbryo embryo)
        {
            if (embryo == null) return;

            if (billDoer?.carryTracker?.CarriedThing == embryo)
            {
                billDoer.carryTracker.TryDropCarriedThing(billDoer.Position, ThingPlaceMode.Near, out _);
            }
            else if (!embryo.Spawned)
            {
                IntVec3 dropLoc = bench?.InteractionCell ?? billDoer?.Position ?? IntVec3.Invalid;
                Map map = bench?.Map ?? billDoer?.Map;
                if (map != null && dropLoc.IsValid)
                {
                    if (embryo.holdingOwner != null)
                    {
                        embryo.holdingOwner.TryDrop(embryo, dropLoc, map, ThingPlaceMode.Near, out _);
                    }
                    else
                    {
                        GenPlace.TryPlaceThing(embryo, dropLoc, map, ThingPlaceMode.Near);
                    }
                }
            }
        }
    }
}
