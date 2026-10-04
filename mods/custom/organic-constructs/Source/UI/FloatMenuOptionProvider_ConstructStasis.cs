using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    /// <summary>
    /// Provides the right-click "Enter stasis" float menu option on beds for construct colonists.
    /// </summary>
    public class FloatMenuOptionProvider_ConstructStasis : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            Pawn pawn = context?.FirstSelectedPawn;
            if (pawn == null || !pawn.Spawned || pawn.Downed)
            {
                return false;
            }

            var gene = pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            return gene != null && !gene.inStasis;
        }

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            if (!(clickedThing is Building_Bed bed) || !bed.def.building.bed_humanlike)
            {
                return null;
            }

            Pawn pawn = context?.FirstSelectedPawn;
            if (pawn == null)
            {
                return null;
            }

            if (!pawn.CanReach(bed, PathEndMode.OnCell, Danger.Deadly))
            {
                return new FloatMenuOption("Cannot enter stasis: " + "CannotUseNoPath".Translate(), null);
            }

            if (bed.IsForbidden(pawn))
            {
                return new FloatMenuOption("Cannot enter stasis: " + "CannotPrioritizeForbidden".Translate(bed.Label), null);
            }

            if (bed.CompAssignableToPawn != null)
            {
                if (!bed.CompAssignableToPawn.CanAssignTo(pawn))
                {
                    return new FloatMenuOption("Cannot enter stasis: " + bed.CompAssignableToPawn.CanAssignTo(pawn).Reason, null);
                }

                if (!bed.CompAssignableToPawn.HasFreeSlot && !RestUtility.BedOwnerWillShare(bed, pawn, pawn.guest?.GuestStatus) && !bed.IsOwner(pawn))
                {
                    return new FloatMenuOption("Cannot enter stasis: " + "SomeoneElseSleeping".Translate(bed.OwnersForReading.Select(p => p.LabelShort).ToCommaList()), null);
                }
            }

            if (!RestUtility.IsValidBedFor(bed, pawn, pawn, checkSocialProperness: false))
            {
                return new FloatMenuOption("Cannot enter stasis: " + "Inaccessible".Translate(), null);
            }

            return new FloatMenuOption("Enter stasis", delegate
            {
                Job job = JobMaker.MakeJob(ConstructDefOf.Construct_EnterConstructStasis, bed);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
        }
    }

    /// <summary>
    /// Safeguard to ensure FloatMenuOptionProvider_ConstructStasis is present in FloatMenuMakerMap.providers
    /// even if the provider list was initialized prior to assembly registration.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ConstructStasisMenuInitializer
    {
        static ConstructStasisMenuInitializer()
        {
            try
            {
                var field = typeof(FloatMenuMakerMap).GetField("providers", BindingFlags.NonPublic | BindingFlags.Static);
                if (field?.GetValue(null) is IList list)
                {
                    bool existsStasis = false;
                    bool existsCarry = false;
                    foreach (var item in list)
                    {
                        if (item is FloatMenuOptionProvider_ConstructStasis)
                        {
                            existsStasis = true;
                        }
                        else if (item is FloatMenuOptionProvider_CarryConstructToBed)
                        {
                            existsCarry = true;
                        }
                    }
                    if (!existsStasis)
                    {
                        list.Add(new FloatMenuOptionProvider_ConstructStasis());
                    }
                    if (!existsCarry)
                    {
                        list.Add(new FloatMenuOptionProvider_CarryConstructToBed());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[OrganicConstructs] Failed to ensure FloatMenuOptionProvider registration: {ex.Message}");
            }
        }
    }
}
