using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    /// <summary>
    /// Recipe worker for synthesizing blank synthetic embryo matrices at the Embryo Splicing Bench.
    /// Creates a donorless construct embryo with locked construct baseline endogenes.
    /// </summary>
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

            // 1. Clear pawn sources to ensure Father = null and Mother = null
            CompHasPawnSources sources = embryo.TryGetComp<CompHasPawnSources>();
            if (sources?.pawnSources != null)
            {
                sources.pawnSources.Clear();
            }

            // 2. Mark as construct matrix on CompEmbryoQuality
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            if (quality != null)
            {
                quality.isConstruct = true;
                quality.editCount = 0;
            }

            // 3. Populate construct baseline endogenes
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

            // 4. Place embryo in the world
            Building_EmbryoSplicingBench bench = billDoer?.CurJob?.targetA.Thing as Building_EmbryoSplicingBench
                ?? (billDoer?.Map != null ? GenClosest.ClosestThingReachable(billDoer.Position, billDoer.Map, ThingRequest.ForDef(EugenicsDefOf.EmbryoSplicingBench), PathEndMode.Touch, TraverseParms.For(billDoer)) as Building_EmbryoSplicingBench : null);

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
