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

            CompEmbryoQuality comp = embryo.TryGetComp<CompEmbryoQuality>();
            if (comp != null && billDoer != null)
            {
                float roomCleanliness = billDoer.GetRoom()?.GetStat(RoomStatDefOf.Cleanliness) ?? 1f;
                if (roomCleanliness < 0f && !comp.hasDefects)
                {
                    float defectChance = (comp.Props?.dirtyRoomDefectMultiplier ?? 2f) * 0.04f;
                    if (Rand.Chance(defectChance))
                    {
                        comp.IntroduceComplication("Laboratory contamination (dirty environment)", 0.35f);
                    }
                }
            }

            if (billDoer?.carryTracker?.CarriedThing == embryo)
            {
                billDoer.carryTracker.TryDropCarriedThing(billDoer.Position, ThingPlaceMode.Near, out _);
            }
            else if (embryo.holdingOwner != null)
            {
                embryo.holdingOwner.TryDrop(embryo, billDoer?.Position ?? bench?.Position ?? IntVec3.Invalid, billDoer?.Map ?? bench?.Map, ThingPlaceMode.Near, out _);
            }

            string templateName = !string.IsNullOrEmpty(blueprint.templateLabel) ? blueprint.templateLabel : "genome blueprint";
            Messages.Message(
                $"Batch splicing complete: Embryo spliced with '{templateName}' ({blueprint.genes.Count} genes).",
                embryo,
                MessageTypeDefOf.PositiveEvent);
        }
    }
}
