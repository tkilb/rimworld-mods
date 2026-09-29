using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class Recipe_SynthesizeBlankEmbryo : RecipeWorker
    {
        private static readonly string[] ConstructBaselineGenes = new string[]
        {
            "Gene_ConstructPsychology",
            "Construct_MetabolicallyEfficient",
            "Gene_MandatorySterility",
            "RobustDigestion",
            "StrongStomach",
            "PsychicAbility_Deaf",
            "Beauty_VeryUgly",
            "Gene_ConstructHibernation",
            "LowSleep",
            "Learning_Slow"
        };

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);

            HumanEmbryo embryo = (HumanEmbryo)ThingMaker.MakeThing(ThingDefOf.HumanEmbryo);

            // Clear pawn sources to ensure Father = null and Mother = null
            CompHasPawnSources sources = embryo.TryGetComp<CompHasPawnSources>();
            if (sources?.pawnSources != null)
            {
                sources.pawnSources.Clear();
            }

            // Mark as construct matrix on CompEmbryoQuality
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            if (quality != null)
            {
                quality.isConstruct = true;
                quality.editCount = 0;
            }

            // Populate construct baseline endogenes
            if (embryo.GeneSet != null)
            {
                List<GeneDef> current = embryo.GeneSet.GenesListForReading.ToList();
                for (int i = 0; i < current.Count; i++)
                {
                    embryo.GeneSet.Debug_RemoveGene(current[i]);
                }

                for (int i = 0; i < ConstructBaselineGenes.Length; i++)
                {
                    GeneDef g = DefDatabase<GeneDef>.GetNamedSilentFail(ConstructBaselineGenes[i]);
                    if (g != null)
                    {
                        embryo.GeneSet.AddGene(g);
                    }
                }
            }

            Building_ConstructSynthesizer bench = billDoer?.CurJob?.targetA.Thing as Building_ConstructSynthesizer
                ?? (billDoer?.Map != null ? GenClosest.ClosestThingReachable(billDoer.Position, billDoer.Map, ThingRequest.ForDef(ConstructDefOf.ConstructSynthesizer), PathEndMode.Touch, TraverseParms.For(billDoer)) as Building_ConstructSynthesizer : null);

            IntVec3 dropLoc = billDoer?.Position ?? bench?.Position ?? IntVec3.Invalid;
            Map map = billDoer?.Map ?? bench?.Map;

            if (map != null && dropLoc.IsValid)
            {
                GenPlace.TryPlaceThing(embryo, dropLoc, map, ThingPlaceMode.Near);
            }

            Messages.Message(
                "Synthetic construct embryo matrix successfully synthesized with baseline construct genome.",
                embryo,
                MessageTypeDefOf.PositiveEvent);
        }
    }
}
