using RimWorld;
using Verse;

namespace EugenicsProgram
{
    public class ThoughtWorker_ConstructProperty : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p == null || !p.RaceProps.Humanlike)
            {
                return ThoughtState.Inactive;
            }

            if (Gene_ConstructPsychology.IsConstruct(p))
            {
                // Constructs do not feel loneliness or normal existential dread
                return ThoughtState.ActiveAtStage(0);
            }

            return ThoughtState.Inactive;
        }
    }
}
