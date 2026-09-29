using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    public class Recipe_BatchApplyBlueprint : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (!base.AvailableOnNow(thing, part)) return false;
            if (thing is Building_EmbryoSplicingBench bench)
            {
                return bench.HasLoadedBlueprint;
            }
            return false;
        }

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            if (thing is Building_EmbryoSplicingBench bench && !bench.HasLoadedBlueprint)
            {
                return new AcceptanceReport("Missing genome blueprint disc in splicing bench receptacle.");
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

            Building_EmbryoSplicingBench bench = billDoer?.CurJob?.targetA.Thing as Building_EmbryoSplicingBench
                ?? (billDoer?.Map != null ? GenClosest.ClosestThingReachable(billDoer.Position, billDoer.Map, ThingRequest.ForDef(EugenicsDefOf.EmbryoSplicingBench), PathEndMode.Touch, TraverseParms.For(billDoer)) as Building_EmbryoSplicingBench : null);

            CompGenomeBlueprint blueprint = bench?.LoadedBlueprintComp;
            if (blueprint == null || blueprint.genes.NullOrEmpty()) return;

            CompEmbryoQuality comp = embryo.TryGetComp<CompEmbryoQuality>();

            // 1. Enforce 2-edit limit: block further editing if already at 2 edits
            if (comp != null && comp.editCount >= 2)
            {
                Messages.Message(
                    "Batch splicing aborted: Embryo has already undergone the maximum allowed genetic edits (2).",
                    embryo,
                    MessageTypeDefOf.RejectInput);
                DropEmbryo(billDoer, bench, embryo);
                return;
            }

            // 2. Failure roll on botch based on doctor skill and room cleanliness
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
                    Thing paste = ThingMaker.MakeThing(EugenicsDefOf.GeneticNutrientPaste);
                    paste.stackCount = 1;
                    GenPlace.TryPlaceThing(paste, dropLoc, map, ThingPlaceMode.Near);
                }

                Messages.Message(
                    $"Embryo splicing botched by {billDoer?.LabelShort ?? "operator"}! Cellular structure collapsed into 1x Genetic Nutrient Paste.",
                    new TargetInfo(dropLoc, map),
                    MessageTypeDefOf.NegativeEvent);

                return;
            }

            // 3. Successful splicing: apply blueprint endogenes
            if (embryo.GeneSet != null)
            {
                List<GeneDef> currentGenes = embryo.GeneSet.GenesListForReading.ToList();
                for (int i = 0; i < currentGenes.Count; i++)
                {
                    embryo.GeneSet.Debug_RemoveGene(currentGenes[i]);
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

            // 4. Increment editCount
            if (comp != null)
            {
                comp.editCount++;
            }

            // 5. Apply parental thoughts to biological parents (if natural embryo)
            EugenicsParentalUtility.ApplyParentalThoughts(embryo);

            // 6. Drop embryo safely
            DropEmbryo(billDoer, bench, embryo);

            string templateName = !string.IsNullOrEmpty(blueprint.templateLabel) ? blueprint.templateLabel : "genome blueprint";
            Messages.Message(
                $"Batch splicing complete: Embryo spliced with '{templateName}' ({blueprint.genes.Count} genes). Edits: {comp?.editCount ?? 1}/2.",
                embryo,
                MessageTypeDefOf.PositiveEvent);
        }

        private static void DropEmbryo(Pawn billDoer, Building_EmbryoSplicingBench bench, HumanEmbryo embryo)
        {
            if (billDoer?.carryTracker?.CarriedThing == embryo)
            {
                billDoer.carryTracker.TryDropCarriedThing(billDoer.Position, ThingPlaceMode.Near, out _);
            }
            else if (embryo.holdingOwner != null)
            {
                embryo.holdingOwner.TryDrop(embryo, billDoer?.Position ?? bench?.Position ?? IntVec3.Invalid, billDoer?.Map ?? bench?.Map, ThingPlaceMode.Near, out _);
            }
        }
    }
}
