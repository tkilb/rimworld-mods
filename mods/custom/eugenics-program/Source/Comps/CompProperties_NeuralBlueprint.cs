using Verse;

namespace EugenicsProgram
{
    public class CompProperties_NeuralBlueprint : CompProperties
    {
        public string defaultDoctrineTitle = "Standard Combat Doctrine";
        public int defaultSkillCap = 14;
        public int minSkillToEncode = 6;
        public int maxPassions = 3;
        public float marketValuePerSkillPoint = 40f;

        public CompProperties_NeuralBlueprint()
        {
            compClass = typeof(CompNeuralBlueprint);
        }
    }
}
