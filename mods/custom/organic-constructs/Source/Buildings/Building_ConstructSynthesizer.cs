using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class Building_ConstructSynthesizer : Building_WorkTable, IThingHolder
    {
        public Thing targetDisc;
        protected ThingOwner discContainer;

        public Building_ConstructSynthesizer()
        {
            discContainer = new ThingOwner<Thing>(this, oneStackOnly: true);
        }

        public Thing LoadedDisc => discContainer?.Count > 0 ? discContainer[0] : null;
        public CompGenomeBlueprint LoadedBlueprintComp => LoadedDisc?.TryGetComp<CompGenomeBlueprint>();
        public bool HasLoadedBlueprint => LoadedBlueprintComp != null && (LoadedBlueprintComp.genes?.Count ?? 0) > 0;
        public bool CanAcceptDisc => LoadedDisc == null;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref discContainer, "discContainer", this);
            Scribe_References.Look(ref targetDisc, "targetDisc");
            if (discContainer == null)
            {
                discContainer = new ThingOwner<Thing>(this, oneStackOnly: true);
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, discContainer);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return discContainer;
        }

        public bool TryAcceptDisc(Thing disc)
        {
            if (disc == null || LoadedDisc != null) return false;

            bool added = false;
            if (disc.Spawned)
            {
                Thing split = disc.SplitOff(1);
                added = discContainer.TryAdd(split);
            }
            else if (disc.holdingOwner != null)
            {
                added = disc.holdingOwner.TryTransferToContainer(disc, discContainer, 1) > 0;
            }
            else
            {
                added = discContainer.TryAdd(disc.SplitOff(1));
            }

            if (added)
            {
                targetDisc = null;
            }
            return added;
        }

        public void EjectDisc()
        {
            if (LoadedDisc != null)
            {
                Map dropMap = Map ?? MapHeld;
                IntVec3 dropLoc = dropMap != null ? (def.hasInteractionCell ? InteractionCell : Position) : IntVec3.Invalid;
                if (dropMap != null && dropLoc.IsValid)
                {
                    discContainer.TryDrop(LoadedDisc, dropLoc, dropMap, ThingPlaceMode.Near, out _);
                }
            }
        }

        protected override void Tick()
        {
            base.Tick();
            if (targetDisc != null && (targetDisc.Destroyed || LoadedDisc != null || (!targetDisc.Spawned && !(targetDisc.holdingOwner?.Owner is Pawn_CarryTracker || targetDisc.holdingOwner?.Owner is Pawn))))
            {
                targetDisc = null;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            EjectDisc();
            base.Destroy(mode);
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder();
            sb.Append(base.GetInspectString());

            if (targetDisc != null && LoadedDisc == null)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.Append($"Waiting for blueprint delivery: {targetDisc.Label}");
            }
            else if (LoadedDisc != null)
            {
                if (sb.Length > 0) sb.AppendLine();
                CompGenomeBlueprint bp = LoadedBlueprintComp;
                string label = bp?.templateLabel ?? LoadedDisc.Label;
                int geneCount = bp?.genes?.Count ?? 0;
                int cpx = bp?.ComplexityTotal ?? 0;
                int met = bp?.MetabolicTotal ?? 0;
                string metStr = met >= 0 ? "+" + met : met.ToString();
                sb.Append($"Loaded Blueprint: {label} ({geneCount} genes, Cpx: {cpx}, Met: {metStr})");
            }
            else
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.Append("No Genome Blueprint Disc loaded (Required for batch genome imprinting)");
            }

            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;

            if (LoadedDisc != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Eject Blueprint Disc",
                    defaultDesc = $"Eject the loaded genome blueprint disc ({LoadedBlueprintComp?.templateLabel ?? LoadedDisc.Label}).",
                    icon = LoadedDisc.def.uiIcon,
                    action = EjectDisc
                };
            }
            else if (targetDisc != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Cancel Blueprint Delivery",
                    defaultDesc = $"Cancel waiting for delivery of {targetDisc.Label}.",
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true) ?? def?.uiIcon,
                    action = () => targetDisc = null
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "Load Blueprint Disc",
                    defaultDesc = "Select an available genome blueprint disc on the map to install into the synthesizer.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", false) ?? def?.uiIcon,
                    action = () =>
                    {
                        var options = new List<FloatMenuOption>();
                        List<Thing> discs = Map?.listerThings.ThingsOfDef(ConstructDefOf.GenomeBlueprintDisk);
                        if (discs != null)
                        {
                            foreach (Thing d in discs)
                            {
                                if (d.Spawned && !d.IsForbidden(Faction.OfPlayer))
                                {
                                    CompGenomeBlueprint bp = d.TryGetComp<CompGenomeBlueprint>();
                                    string title = !string.IsNullOrEmpty(bp?.templateLabel)
                                        ? $"{d.Label} ({bp.templateLabel}, {bp.genes?.Count ?? 0} genes)"
                                        : $"{d.Label} (Empty)";
                                    options.Add(new FloatMenuOption(title, () =>
                                    {
                                        targetDisc = d;
                                    }));
                                }
                            }
                        }

                        if (options.Count == 0)
                        {
                            options.Add(new FloatMenuOption("No genome blueprint discs found on map", null));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption opt in base.GetFloatMenuOptions(selPawn))
                yield return opt;

            if (LoadedDisc == null && !selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Hauling))
            {
                if (targetDisc != null && targetDisc.Spawned && !targetDisc.Destroyed)
                {
                    if (selPawn.CanReserveAndReach(targetDisc, PathEndMode.ClosestTouch, Danger.Some) &&
                        selPawn.CanReserveAndReach(this, PathEndMode.Touch, Danger.Some))
                    {
                        yield return new FloatMenuOption($"Haul {targetDisc.Label} to {LabelShort}", () =>
                        {
                            Job job = JobMaker.MakeJob(ConstructDefOf.Construct_HaulDiscToContainer, targetDisc, this);
                            job.count = 1;
                            selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        });
                    }
                }
                else if (targetDisc == null && selPawn.CanReserveAndReach(this, PathEndMode.Touch, Danger.Some))
                {
                    Thing availableDisc = GenClosest.ClosestThingReachable(
                        Position,
                        Map,
                        ThingRequest.ForDef(ConstructDefOf.GenomeBlueprintDisk),
                        PathEndMode.ClosestTouch,
                        TraverseParms.For(selPawn),
                        validator: d => d.Spawned && !d.IsForbidden(Faction.OfPlayer) && !d.Destroyed && selPawn.CanReserve(d)
                    );

                    if (availableDisc != null)
                    {
                        yield return new FloatMenuOption($"Load {availableDisc.Label} into {LabelShort}", () =>
                        {
                            targetDisc = availableDisc;
                            Job job = JobMaker.MakeJob(ConstructDefOf.Construct_HaulDiscToContainer, availableDisc, this);
                            job.count = 1;
                            selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        });
                    }
                }
            }
        }
    }
}
