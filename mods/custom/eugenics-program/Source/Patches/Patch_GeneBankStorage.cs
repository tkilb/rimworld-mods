using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace EugenicsProgram.Patches
{
    [StaticConstructorOnStartup]
    public static class GeneBankStoragePatcher
    {
        static GeneBankStoragePatcher()
        {
            var harmony = new Harmony("com.eugenicsprogram.genebankstorage");

            // Patch CompGenepackContainer.CanStore
            var canStoreMethod = AccessTools.Method(typeof(CompGenepackContainer), "CanStore");
            if (canStoreMethod != null)
            {
                harmony.Patch(canStoreMethod, null, new HarmonyMethod(typeof(Patch_GeneBankStorage), nameof(Patch_GeneBankStorage.CanStore_Postfix)));
            }

            // Patch CompGenepackContainer.Accepts
            var acceptsMethod = AccessTools.Method(typeof(CompGenepackContainer), "Accepts");
            if (acceptsMethod != null)
            {
                harmony.Patch(acceptsMethod, null, new HarmonyMethod(typeof(Patch_GeneBankStorage), nameof(Patch_GeneBankStorage.Accepts_Postfix)));
            }

            // Patch WorkGiver if it exists
            var wgType = AccessTools.TypeByName("RimWorld.WorkGiver_HaulToGenepackContainer") ?? typeof(WorkGiver_HaulToGeneBank);
            if (wgType != null)
            {
                var potentialWorkReq = AccessTools.PropertyGetter(wgType, "PotentialWorkThingRequest");
                if (potentialWorkReq != null)
                {
                    harmony.Patch(potentialWorkReq, null, new HarmonyMethod(typeof(Patch_GeneBankStorage), nameof(Patch_GeneBankStorage.PotentialWorkThingRequest_Postfix)));
                }

                var hasJob = AccessTools.Method(wgType, "HasJobOnThing");
                if (hasJob != null)
                {
                    harmony.Patch(hasJob, null, new HarmonyMethod(typeof(Patch_GeneBankStorage), nameof(Patch_GeneBankStorage.HasJobOnThing_Postfix)));
                }

                var jobOnThing = AccessTools.Method(wgType, "JobOnThing");
                if (jobOnThing != null)
                {
                    harmony.Patch(jobOnThing, null, new HarmonyMethod(typeof(Patch_GeneBankStorage), nameof(Patch_GeneBankStorage.JobOnThing_Postfix)));
                }
            }
        }
    }

    public static class Patch_GeneBankStorage
    {
        public static bool IsBlueprintDisk(Thing thing)
        {
            return thing != null && (thing.def.defName == "GenomeBlueprintDisk" || thing.def.defName == "NeuralBlueprintDisk");
        }

        public static void CanStore_Postfix(Thing thing, ref bool __result)
        {
            if (IsBlueprintDisk(thing))
            {
                __result = true;
            }
        }

        public static void Accepts_Postfix(CompGenepackContainer __instance, Thing thing, ref bool __result)
        {
            if (IsBlueprintDisk(thing))
            {
                // Accept if not full, or if discs don't count towards capacity, just accept if power on
                if (!__instance.Full || true) 
                {
                    __result = true;
                }
            }
        }

        // WorkGiver Patches
        public static void PotentialWorkThingRequest_Postfix(ref ThingRequest __result)
        {
            // Allow WorkGiver to scan our discs as well by scanning all HaulableAlways
            // (RimWorld's hauling system will filter by HasJobOnThing anyway)
            __result = ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways);
        }

        public static void HasJobOnThing_Postfix(Pawn pawn, Thing t, bool forced, ref bool __result)
        {
            if (!__result && IsBlueprintDisk(t))
            {
                if (pawn.CanReserve(t, 1, -1, null, forced))
                {
                    Thing bank = FindValidGeneBank(pawn, t);
                    if (bank != null)
                    {
                        __result = true;
                    }
                }
            }
        }

        public static void JobOnThing_Postfix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            if (__result == null && IsBlueprintDisk(t))
            {
                Thing bank = FindValidGeneBank(pawn, t);
                if (bank != null)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer, t, bank, bank.InteractionCell);
                    job.count = t.stackCount;
                    __result = job;
                }
            }
        }

        private static Thing FindValidGeneBank(Pawn pawn, Thing disk)
        {
            return GenClosest.ClosestThingReachable(disk.Position, disk.Map, ThingRequest.ForGroup(ThingRequestGroup.GenepackHolder), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, delegate(Thing x)
            {
                if (x.IsForbidden(pawn) || !pawn.CanReserve(x)) return false;
                CompGenepackContainer comp = x.TryGetComp<CompGenepackContainer>();
                if (comp == null || !comp.PowerOn) return false;
                
                // Allow if discs don't break capacity
                return true;
            });
        }
    }
}
