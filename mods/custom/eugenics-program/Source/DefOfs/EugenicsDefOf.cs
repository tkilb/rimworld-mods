using RimWorld;
using Verse;

namespace EugenicsProgram
{
    [DefOf]
    public static class EugenicsDefOf
    {
        // ── Jobs ──────────────────────────────────────────────────────────────────
        public static JobDef Eugenics_ScanNeuralProfile;
        public static JobDef Eugenics_ScreenEmbryo;
        public static JobDef Eugenics_RecycleEmbryo;

        // ── Items ─────────────────────────────────────────────────────────────────
        public static ThingDef NeuralBlueprintDisk;
        public static ThingDef GenomeBlueprintDisk;
        public static ThingDef GeneticNutrientPaste;

        // ── Buildings ─────────────────────────────────────────────────────────────
        public static ThingDef NeuralScanner;

        // ── Hediffs ───────────────────────────────────────────────────────────────
        public static HediffDef Eugenics_NeuralFatigue;

        static EugenicsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EugenicsDefOf));
        }
    }
}
