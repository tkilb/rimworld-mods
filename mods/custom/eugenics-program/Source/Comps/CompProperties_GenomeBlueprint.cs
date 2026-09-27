using Verse;

namespace EugenicsProgram
{
    public class CompProperties_GenomeBlueprint : CompProperties
    {
        public int maxStoredGenes = 30;
        public string defaultTemplateLabel = "Construct Caste";
        public float marketValuePerGene = 150f;

        public CompProperties_GenomeBlueprint()
        {
            compClass = typeof(CompGenomeBlueprint);
        }
    }
}
