using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    public class Recipe_BatchRecycleEmbryo : RecipeWorker
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            Messages.Message(
                "Embryo biomass liquefied into Genetic Nutrient Paste.",
                new LookTargets(billDoer),
                MessageTypeDefOf.NeutralEvent);
        }
    }
}
