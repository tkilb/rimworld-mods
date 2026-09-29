using Verse;

namespace OrganicConstructs
{
    public class CompProperties_GrowthVatImprinter : CompProperties
    {
        public int imprintIntervalTicks = 250;
        public float xpPerImprintInterval = 15f;
        public bool imprintPassions = false;

        public CompProperties_GrowthVatImprinter()
        {
            compClass = typeof(CompGrowthVatImprinter);
        }
    }
}
