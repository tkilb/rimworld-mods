using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    public class GameComponent_NeuralImprintTracker : GameComponent
    {
        private HashSet<int> imprintedConstructIds = new HashSet<int>();

        public GameComponent_NeuralImprintTracker(Game game)
        {
        }

        public static GameComponent_NeuralImprintTracker Instance => Current.Game?.GetComponent<GameComponent_NeuralImprintTracker>();

        public bool HasBeenImprinted(Pawn pawn)
        {
            return pawn != null && imprintedConstructIds.Contains(pawn.thingIDNumber);
        }

        public void RegisterImprint(Pawn pawn)
        {
            if (pawn != null)
            {
                imprintedConstructIds.Add(pawn.thingIDNumber);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref imprintedConstructIds, "imprintedConstructIds", LookMode.Value);
            if (imprintedConstructIds == null)
            {
                imprintedConstructIds = new HashSet<int>();
            }
        }
    }

    [HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.ApplyBirthOutcome))]
    public static class Patch_PregnancyUtility_ApplyBirthOutcome
    {
        [HarmonyPostfix]
        public static void Postfix(Thing __result, Thing birtherThing)
        {
            if (birtherThing is Building_GrowthVat vat && __result is Pawn newborn)
            {
                CompGrowthVatImprinter imprinter = vat.GetComp<CompGrowthVatImprinter>();
                if (imprinter != null)
                {
                    imprinter.OnPawnDecanted(newborn, fromEmbryo: true, embryo: vat.selectedEmbryo);
                }

                if (vat.selectedEmbryo != null && ConstructUtility.IsConstruct(newborn))
                {
                    CompEmbryoQuality quality = vat.selectedEmbryo.TryGetComp<CompEmbryoQuality>();
                    if (quality != null && quality.isConstruct)
                    {
                        float stability = ConstructStabilityUtility.CalculateStability(vat.selectedEmbryo.GeneSet.ComplexityTotal, vat.selectedEmbryo.GeneSet.MetabolismTotal, vat.selectedEmbryo.GeneSet.GenesListForReading);
                        bool isVolatile = stability < 0.60f || (quality.blueprintLabel != null && quality.blueprintLabel.ToLower().Contains("volatile"));
                        if (isVolatile && Rand.Chance(0.15f))
                        {
                            HediffDef defectDef = DefDatabase<HediffDef>.GetNamedSilentFail("Construct_Assimilation") ?? HediffDefOf.CryptosleepSickness;
                            if (defectDef != null)
                            {
                                newborn.health.AddHediff(defectDef);
                                Messages.Message($"Volatile genetic stability caused assimilation delay or cellular defect in {newborn.LabelShort}.", newborn, MessageTypeDefOf.NegativeEvent);
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Building_GrowthVat), "FinishPawn")]
    public static class Patch_Building_GrowthVat_FinishPawn
    {
        [HarmonyPrefix]
        public static void Prefix(Building_GrowthVat __instance, out Pawn __state)
        {
            __state = Traverse.Create(__instance).Field("selectedPawn").GetValue<Pawn>();
        }

        [HarmonyPostfix]
        public static void Postfix(Building_GrowthVat __instance, Pawn __state)
        {
            if (__state != null)
            {
                CompGrowthVatImprinter imprinter = __instance.GetComp<CompGrowthVatImprinter>();
                if (imprinter != null)
                {
                    imprinter.OnPawnDecanted(__state, fromEmbryo: false, embryo: null);
                }
            }
        }
    }
}
