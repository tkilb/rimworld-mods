using Verse;

namespace OrganicConstructs
{
    public class CompProperties_NeuralBlueprint : CompProperties
    {
        public string defaultDoctrineTitle = "Standard Combat Doctrine";
        public int defaultSkillCap = 14;
        public int minSkillToEncode = 6;

        public CompProperties_NeuralBlueprint()
        {
            compClass = typeof(CompNeuralBlueprint);
        }
    }
}
