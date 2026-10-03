using Verse;

namespace OrganicConstructs
{
    public class CompProperties_NeuralBlueprint : CompProperties
    {
        public string defaultDoctrineTitle = "Standard Neural Imprint";
        public int defaultSkillCap = 14;

        public CompProperties_NeuralBlueprint()
        {
            compClass = typeof(CompNeuralBlueprint);
        }
    }
}
