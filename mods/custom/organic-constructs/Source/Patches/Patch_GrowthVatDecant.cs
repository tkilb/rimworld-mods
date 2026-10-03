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
        public static readonly Dictionary<Building_GrowthVat, Pawn> pendingConstructMaturation = new Dictionary<Building_GrowthVat, Pawn>();

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
                    ConstructNameUtility.AssignConstructNameIfNeeded(newborn);
                    CompEmbryoQuality quality = vat.selectedEmbryo.TryGetComp<CompEmbryoQuality>();
                    if (quality != null && quality.isConstruct)
                    {
                        GeneSet gs = vat.selectedEmbryo.GeneSet;
                        float stability = gs != null
                            ? ConstructStabilityUtility.CalculateStability(gs.ComplexityTotal, gs.MetabolismTotal, gs.GenesListForReading)
                            : 1.0f; // Safe default: treat unknown embryos as stable
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

                        // Despawn baby from world so it can transition directly into growth vat maturation
                        if (newborn.Spawned)
                        {
                            newborn.DeSpawn();
                        }

                        Gene_ConstructPsychology.ApplyConstructPhysiology(newborn);
                        CompGrowthVatImprinter.WipePassions(newborn);

                        // Queue newborn construct for vat insertion after FinishEmbryo() finishes vanilla cleanup
                        pendingConstructMaturation[vat] = newborn;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Building_GrowthVat), "FinishEmbryo")]
    public static class Patch_Building_GrowthVat_FinishEmbryo
    {
        [HarmonyPostfix]
        public static void Postfix(Building_GrowthVat __instance)
        {
            if (Patch_PregnancyUtility_ApplyBirthOutcome.pendingConstructMaturation.TryGetValue(__instance, out Pawn newborn))
            {
                Patch_PregnancyUtility_ApplyBirthOutcome.pendingConstructMaturation.Remove(__instance);

                if (newborn != null && !newborn.Destroyed)
                {
                    // Defensive cleanup: Ensure no remnant HumanEmbryo items linger in innerContainer
                    List<Thing> embryosToRemove = null;
                    foreach (Thing t in __instance.innerContainer)
                    {
                        if (t is HumanEmbryo)
                        {
                            if (embryosToRemove == null) embryosToRemove = new List<Thing>();
                            embryosToRemove.Add(t);
                        }
                    }
                    if (embryosToRemove != null)
                    {
                        foreach (Thing emb in embryosToRemove)
                        {
                            __instance.innerContainer.Remove(emb);
                            if (!emb.Destroyed) emb.Destroy();
                        }
                    }

                    // Seamlessly retain the newborn construct inside the Growth Vat for maturation (never drop as baby)
                    if (!__instance.innerContainer.Contains(newborn))
                    {
                        __instance.innerContainer.TryAddOrTransfer(newborn, canMergeWithExistingStacks: false);
                    }

                    Traverse trav = Traverse.Create(__instance);
                    trav.Field("selectedPawn").SetValue(newborn);
                    trav.Field("startTick").SetValue(Find.TickManager.TicksGame);
                    trav.Field("gestationTicks").SetValue(0);
                    __instance.selectedEmbryo = null;

                    Messages.Message(
                        $"Construct embryonic synthesis complete. Physical form stabilized inside growth vat for maturation.",
                        __instance,
                        MessageTypeDefOf.PositiveEvent);
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
