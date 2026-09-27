using RimWorld;
using Verse;

namespace EugenicsProgram
{
    [DefOf]
    public static class EugenicsDefOf
    {
        public static JobDef Eugenics_ScanNeuralProfile;
        public static ThingDef NeuralBlueprintDisk;
        public static ThingDef NeuralScanner;
        public static HediffDef Eugenics_NeuralFatigue;

        static EugenicsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EugenicsDefOf));
        }
    }
}
