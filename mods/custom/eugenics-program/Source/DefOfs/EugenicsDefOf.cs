using RimWorld;
using Verse;

namespace EugenicsProgram
{
    [DefOf]
    public static class EugenicsDefOf
    {
        // ── Jobs ──────────────────────────────────────────────────────────────────
        public static JobDef Eugenics_ScanNeuralProfile;
        public static JobDef Eugenics_RecycleEmbryo;
        public static JobDef Eugenics_HaulDiscToContainer;

        // ── Items ─────────────────────────────────────────────────────────────────
        public static ThingDef NeuralBlueprintDisk;
        public static ThingDef GenomeBlueprintDisk;
        public static ThingDef GeneticNutrientPaste;

        // ── Buildings ─────────────────────────────────────────────────────────────
        public static ThingDef NeuralScanner;
        public static ThingDef EmbryoSplicingBench;

        // ── Recipes ───────────────────────────────────────────────────────────────
        public static RecipeDef Eugenics_BatchApplyBlueprint;
        public static RecipeDef Eugenics_BatchRecycleEmbryo;

        // ── Hediffs ───────────────────────────────────────────────────────────────
        public static HediffDef Eugenics_NeuralFatigue;

        static EugenicsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EugenicsDefOf));
        }
    }
}
