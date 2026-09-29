using Verse;

namespace OrganicConstructs
{
    public class CompProperties_GenomeBlueprint : CompProperties
    {
        public string defaultTemplateLabel = "Construct Caste";

        public CompProperties_GenomeBlueprint()
        {
            compClass = typeof(CompGenomeBlueprint);
        }
    }
}
