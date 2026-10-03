using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [DefOf]
    public static class ConstructDefOf
    {
        // ── Jobs ──────────────────────────────────────────────────────────────────
        public static JobDef Construct_ScanNeuralProfile;
        public static JobDef Construct_RecycleEmbryo;
        public static JobDef Construct_HaulDiscToContainer;
        public static JobDef Construct_EnterConstructStasis;

        // ── Items ─────────────────────────────────────────────────────────────────
        public static ThingDef NeuralBlueprintDisk;
        public static ThingDef GenomeBlueprintDisk;
        public static ThingDef GeneticNutrientPaste;

        // ── Buildings ─────────────────────────────────────────────────────────────
        public static ThingDef NeuralScanner;
        public static ThingDef ConstructSynthesizer;
        public static ThingDef ConstructGenomeArchitect;

        // ── Recipes ───────────────────────────────────────────────────────────────
        public static RecipeDef Construct_BatchRecycleEmbryo;

        // ── Hediffs ───────────────────────────────────────────────────────────────
        public static HediffDef Construct_NeuralFatigue;
        public static HediffDef Construct_Assimilation;
        public static HediffDef Construct_InterruptedStasis;
        public static HediffDef Construct_InStasis;

        // ── Backstories ───────────────────────────────────────────────────────────
        public static BackstoryDef Construct_Childhood;
        public static BackstoryDef Construct_Adulthood;

        // ── Genes ─────────────────────────────────────────────────────────────────
        public static GeneDef Gene_ConstructHibernation;

        // ── Traits ────────────────────────────────────────────────────────────────
        public static TraitDef Trait_ConstructAsset;

        // ── Needs ─────────────────────────────────────────────────────────────────
        public static NeedDef Construct_Stasis;

        // ── Thoughts ──────────────────────────────────────────────────────────────
        public static ThoughtDef Construct_KindPity;
        public static ThoughtDef Construct_Expectations;

        static ConstructDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ConstructDefOf));
        }
    }
}
