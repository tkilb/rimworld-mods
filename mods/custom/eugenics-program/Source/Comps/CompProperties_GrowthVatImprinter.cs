using Verse;

namespace EugenicsProgram
{
    public class CompProperties_GrowthVatImprinter : CompProperties
    {
        public int imprintIntervalTicks = 250;
        public float xpPerImprintInterval = 15f;
        public bool imprintPassions = true;

        public CompProperties_GrowthVatImprinter()
        {
            compClass = typeof(CompGrowthVatImprinter);
        }
    }
}
