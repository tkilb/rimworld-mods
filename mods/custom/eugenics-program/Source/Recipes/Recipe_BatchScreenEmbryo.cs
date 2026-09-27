using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EugenicsProgram
{
    public class Recipe_BatchScreenEmbryo : RecipeWorker
    {
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

            CompEmbryoQuality comp = embryo.TryGetComp<CompEmbryoQuality>();
            if (comp != null)
            {
                if (billDoer != null)
                {
                    float roomCleanliness = billDoer.GetRoom()?.GetStat(RoomStatDefOf.Cleanliness) ?? 1f;
                    if (roomCleanliness < 0f && !comp.hasDefects)
                    {
                        float contamChance = (comp.Props?.dirtyRoomDefectMultiplier ?? 2f) * 0.03f;
                        if (Rand.Chance(contamChance))
                        {
                            comp.IntroduceComplication("Screening contamination (dirty environment)", 0.35f);
                        }
                    }
                }

                comp.PerformScreening(billDoer);
            }

            if (billDoer?.carryTracker?.CarriedThing == embryo)
            {
                billDoer.carryTracker.TryDropCarriedThing(billDoer.Position, ThingPlaceMode.Near, out _);
            }
            else if (embryo.holdingOwner != null)
            {
                embryo.holdingOwner.TryDrop(embryo, billDoer?.Position ?? IntVec3.Invalid, billDoer?.Map, ThingPlaceMode.Near, out _);
            }
        }
    }
}
