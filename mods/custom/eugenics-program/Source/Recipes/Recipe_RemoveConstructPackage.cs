using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EugenicsProgram
{
    public class Recipe_RemoveConstructPackage : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i].def == recipe.removesHediff)
                {
                    yield return hediffs[i].Part;
                }
            }
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }
            
            Hediff hediff = pawn.health.hediffSet.hediffs.Find(x => x.def == recipe.removesHediff && x.Part == part);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
                if (recipe.removesHediff.spawnThingOnRemoved != null)
                {
                    GenPlace.TryPlaceThing(ThingMaker.MakeThing(recipe.removesHediff.spawnThingOnRemoved), pawn.Position, pawn.Map, ThingPlaceMode.Near);
                }
            }
        }
    }
}
