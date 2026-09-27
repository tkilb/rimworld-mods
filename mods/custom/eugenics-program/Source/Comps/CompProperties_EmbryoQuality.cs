using Verse;

namespace EugenicsProgram
{
    public class CompProperties_EmbryoQuality : CompProperties
    {
        public float baseDefectChance = 0.05f;
        public int minScreeningSkill = 8;
        public float dirtyRoomDefectMultiplier = 2.0f;
        public float biomassYieldMultiplier = 1.0f;

        public CompProperties_EmbryoQuality()
        {
            compClass = typeof(CompEmbryoQuality);
        }
    }
}
