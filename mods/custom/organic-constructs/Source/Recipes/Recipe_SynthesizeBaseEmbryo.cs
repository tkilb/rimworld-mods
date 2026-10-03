using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class Recipe_SynthesizeBaseEmbryo : RecipeWorker
    {
        private static readonly string[] BaseConstructGenes = new string[]
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
            "Superclotting",
            "RobustDigestion",
            "StrongStomach",
            "PsychicAbility_Deaf",
            "Beauty_VeryUgly",
            "LowSleep",
            "Learning_Slow",
            "Hair_BaldOnly",
            "Beard_NoBeardOnly"
        };

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (!base.AvailableOnNow(thing, part)) return false;
            return thing is Building_ConstructSynthesizer;
        }

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

            // Mark as construct on CompEmbryoQuality
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            if (quality != null)
            {
                quality.isConstruct = true;
                quality.editCount = 0;
                quality.blueprintLabel = "Base Construct";
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

            // Populate construct baseline endogenes
            if (embryo.GeneSet != null)
            {
                List<GeneDef> current = embryo.GeneSet.GenesListForReading.ToList();
                for (int i = 0; i < current.Count; i++)
                {
                    embryo.GeneSet.Debug_RemoveGene(current[i]);
                }

                for (int i = 0; i < BaseConstructGenes.Length; i++)
                {
                    GeneDef g = DefDatabase<GeneDef>.GetNamedSilentFail(BaseConstructGenes[i]);
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
                "Successfully synthesized Base Construct embryo with standard baseline genome.",
                embryo,
                MessageTypeDefOf.PositiveEvent);
        }
    }

    // Retained for backward compatibility if referenced by saved data
    public class Recipe_SynthesizeBlankEmbryo : Recipe_SynthesizeBaseEmbryo
    {
    }
}
