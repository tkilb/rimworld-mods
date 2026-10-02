using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class Building_NeuralScanner : Building, IThingHolder, IThingHolderWithDrawnPawn
    {
        public int scanTicks = 15000;
        public float powerConsumptionScanning = 1500f;
        public float powerConsumptionIdle = 200f;

        public Thing targetDisc;

        protected ThingOwner innerContainer;
        protected ThingOwner discContainer;
        protected int ticksScanning = 0;

        public Building_NeuralScanner()
        {
            innerContainer = new ThingOwner<Thing>(this);
            discContainer = new ThingOwner<Thing>(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            NeuralScannerExtension ext = def?.GetModExtension<NeuralScannerExtension>();
            if (ext != null)
            {
                scanTicks = ext.scanTicks;
                powerConsumptionScanning = ext.powerConsumptionScanning;
                powerConsumptionIdle = ext.powerConsumptionIdle;
            }
        }

        public float HeldPawnDrawPos_Y => DrawPos.y + 0.03658537f;
        public float HeldPawnBodyAngle => Rotation.AsAngle;
        public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;
        public virtual Vector3 PawnDrawOffset => IntVec3.West.RotatedBy(Rotation).ToVector3() * (def.size.x / 4f);

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
            Occupant?.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc + PawnDrawOffset, null, neverAimWeapon: true);
        }

        public Pawn Occupant => innerContainer.Count > 0 ? (innerContainer[0] as Pawn) : null;
        public Thing LoadedDisc => discContainer.Count > 0 ? discContainer[0] : null;
        public CompNeuralBlueprint LoadedBlueprint => LoadedDisc?.TryGetComp<CompNeuralBlueprint>();
        public CompPowerTrader PowerTrader => this.TryGetComp<CompPowerTrader>();
        public bool IsPowered => PowerTrader == null || PowerTrader.PowerOn;
        public bool CanAcceptPawn => Occupant == null && LoadedDisc != null && IsPowered;
        public bool CanAcceptDisc => LoadedDisc == null && Occupant == null;
        public float ScanProgress => scanTicks > 0 ? Mathf.Clamp01((float)ticksScanning / scanTicks) : 0f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Deep.Look(ref discContainer, "discContainer", this);
            Scribe_References.Look(ref targetDisc, "targetDisc");
            Scribe_Values.Look(ref ticksScanning, "ticksScanning", 0);
            Scribe_Values.Look(ref scanTicks, "scanTicks", 15000);
            Scribe_Values.Look(ref powerConsumptionScanning, "powerConsumptionScanning", 1500f);
            Scribe_Values.Look(ref powerConsumptionIdle, "powerConsumptionIdle", 200f);

            if (innerContainer == null) innerContainer = new ThingOwner<Thing>(this);
            if (discContainer == null) discContainer = new ThingOwner<Thing>(this);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, innerContainer);
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, discContainer);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public static bool IsConstruct(Pawn pawn)
        {
            return ConstructUtility.IsConstruct(pawn);
        }

        public bool TryAcceptPawn(Pawn pawn)
        {
            if (!CanAcceptPawn || pawn == null) return false;

            if (IsConstruct(pawn))
            {
                Messages.Message("Constructs cannot be scanned: Synthetic neural architecture incompatible with scanner.", pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            pawn.DeSpawnOrDeselect();
            if (pawn.holdingOwner != null)
            {
                pawn.holdingOwner.TryTransferToContainer(pawn, innerContainer, pawn.stackCount);
            }
            else
            {
                innerContainer.TryAdd(pawn);
            }

            ticksScanning = 0;
            if (PowerTrader != null)
            {
                PowerTrader.PowerOutput = -powerConsumptionScanning;
            }
            return true;
        }

        public bool TryAcceptDisc(Thing disc)
        {
            if (!CanAcceptDisc || disc == null) return false;

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

        public void EjectOccupant()
        {
            if (Occupant != null)
            {
                Map dropMap = Map ?? MapHeld;
                IntVec3 dropLoc = dropMap != null ? (def.hasInteractionCell ? InteractionCell : Position) : IntVec3.Invalid;
                if (dropMap != null && dropLoc.IsValid)
                {
                    innerContainer.TryDrop(Occupant, dropLoc, dropMap, ThingPlaceMode.Near, out Thing _);
                }
            }
            ticksScanning = 0;
            if (PowerTrader != null)
            {
                PowerTrader.PowerOutput = -powerConsumptionIdle;
            }
        }

        public void EjectDisc()
        {
            if (LoadedDisc != null)
            {
                Map dropMap = Map ?? MapHeld;
                IntVec3 dropLoc = dropMap != null ? (def.hasInteractionCell ? InteractionCell : Position) : IntVec3.Invalid;
                if (dropMap != null && dropLoc.IsValid)
                {
                    discContainer.TryDrop(LoadedDisc, dropLoc, dropMap, ThingPlaceMode.Near, out Thing _);
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

            if (Occupant != null && LoadedDisc != null)
            {
                if (IsPowered)
                {
                    if (PowerTrader != null)
                    {
                        PowerTrader.PowerOutput = -powerConsumptionScanning;
                    }

                    ticksScanning++;

                    if (ticksScanning >= scanTicks)
                    {
                        CompleteScan();
                    }
                }
            }
            else
            {
                if (PowerTrader != null)
                {
                    PowerTrader.PowerOutput = -powerConsumptionIdle;
                }
            }
        }

        private void CompleteScan()
        {
            Pawn donor = Occupant;
            Thing disc = LoadedDisc;

            if (donor != null && disc != null)
            {
                CompNeuralBlueprint compBp = disc.TryGetComp<CompNeuralBlueprint>();
                if (compBp != null)
                {
                    compBp.EncodePawnProfile(donor);
                }

                HediffDef fatigueDef = ConstructDefOf.Construct_NeuralFatigue;
                if (fatigueDef != null)
                {
                    Hediff fatigue = HediffMaker.MakeHediff(fatigueDef, donor);
                    donor.health.AddHediff(fatigue);
                }

                Messages.Message(
                    $"Neural scan complete: Synaptic profile of {donor.LabelShortCap} successfully encoded to imprint disc.",
                    new LookTargets(this),
                    MessageTypeDefOf.PositiveEvent);
            }

            EjectOccupant();
            EjectDisc();
            ticksScanning = 0;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            EjectOccupant();
            EjectDisc();
            base.Destroy(mode);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());

            if (Occupant != null)
            {
                sb.AppendLine();
                sb.Append($"Scanning: {Occupant.LabelShortCap} ({ScanProgress:P0})");
                if (!IsPowered)
                {
                    sb.Append(" - SUSPENDED (NO POWER)");
                }
            }
            else if (targetDisc != null && LoadedDisc == null)
            {
                sb.AppendLine();
                sb.Append($"Waiting for disc delivery: {targetDisc.Label}");
            }
            else if (LoadedDisc != null)
            {
                sb.AppendLine();
                CompNeuralBlueprint bp = LoadedBlueprint;
                string discLabel = bp?.doctrineTitle ?? LoadedDisc.Label;
                sb.Append($"Ready for subject (Disc: {discLabel})");
            }
            else
            {
                sb.AppendLine();
                sb.Append("Inactive (Requires neural imprint disc)");
            }

            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;

            if (Occupant != null)
            {
                yield return Building_Casket.SelectContainedItemGizmo(this, Occupant);

                yield return new Command_Action
                {
                    defaultLabel = "Abort Scan",
                    defaultDesc = "Eject the current subject from the neural scanner and cancel the scanning operation.",
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true) ?? def?.uiIcon,
                    action = EjectOccupant
                };
            }
            else
            {
                if (LoadedDisc != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Eject Imprint Disc",
                        defaultDesc = "Remove the loaded neural blueprint disc from the scanner.",
                        icon = LoadedDisc.def.uiIcon,
                        action = EjectDisc
                    };

                    yield return new Command_Action
                    {
                        defaultLabel = "Select Subject to Scan",
                        defaultDesc = "Designate a colonist to enter the neural scanner and export their skills.",
                        icon = def?.uiIcon,
                        action = () =>
                        {
                            List<FloatMenuOption> options = new List<FloatMenuOption>();
                            foreach (Pawn p in Map.mapPawns.FreeColonistsSpawned)
                            {
                                if (p.Dead || p.Downed || !p.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
                                    continue;

                                if (IsConstruct(p))
                                {
                                    options.Add(new FloatMenuOption(
                                        $"{p.LabelShortCap} (Cannot scan constructs: Synthetic neural architecture incompatible)",
                                        () => Messages.Message("Constructs cannot be scanned: Synthetic neural architecture incompatible with scanner.", p, MessageTypeDefOf.RejectInput, false)));
                                    continue;
                                }

                                options.Add(new FloatMenuOption(p.LabelShortCap, () =>
                                {
                                    Job job = JobMaker.MakeJob(ConstructDefOf.Construct_ScanNeuralProfile, this);
                                    p.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                }));
                            }

                            if (options.Count == 0)
                            {
                                options.Add(new FloatMenuOption("No eligible colonists available", null));
                            }
                            Find.WindowStack.Add(new FloatMenu(options));
                        }
                    };
                }
                else if (targetDisc != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Cancel Disc Delivery",
                        defaultDesc = $"Cancel waiting for delivery of {targetDisc.Label}.",
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true) ?? def?.uiIcon,
                        action = () => targetDisc = null
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Load Imprint Disc",
                        defaultDesc = "Select an available neural imprint disc on the map to load into the scanner.",
                        icon = def?.uiIcon,
                        action = () =>
                        {
                            List<FloatMenuOption> options = new List<FloatMenuOption>();
                            List<Thing> discs = Map.listerThings.ThingsOfDef(ConstructDefOf.NeuralBlueprintDisk);
                            if (discs != null)
                            {
                                foreach (Thing d in discs)
                                {
                                    if (d.Spawned && !d.IsForbidden(Faction.OfPlayer))
                                    {
                                        CompNeuralBlueprint bp = d.TryGetComp<CompNeuralBlueprint>();
                                        string label = bp?.IsEncoded == true ? $"{d.Label} ({bp.doctrineTitle})" : $"{d.Label} (Blank)";
                                        options.Add(new FloatMenuOption(label, () =>
                                        {
                                            targetDisc = d;
                                        }));
                                    }
                                }
                            }

                            if (options.Count == 0)
                            {
                                options.Add(new FloatMenuOption("No neural imprint discs found on map", null));
                            }
                            Find.WindowStack.Add(new FloatMenu(options));
                        }
                    };
                }
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption opt in base.GetFloatMenuOptions(selPawn))
                yield return opt;

            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
            {
                yield return new FloatMenuOption($"Cannot use {LabelShort} (Unreachable)", null);
                yield break;
            }

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
                        ThingRequest.ForDef(ConstructDefOf.NeuralBlueprintDisk),
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

            if (CanAcceptPawn)
            {
                if (IsConstruct(selPawn))
                {
                    yield return new FloatMenuOption($"Cannot enter {LabelShort} (Constructs cannot be scanned: Synthetic neural architecture incompatible with scanner)", null);
                }
                else
                {
                    yield return new FloatMenuOption($"Enter {LabelShort}", () =>
                    {
                        Job job = JobMaker.MakeJob(ConstructDefOf.Construct_ScanNeuralProfile, this);
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    });
                }
            }
            else if (targetDisc != null && LoadedDisc == null)
            {
                yield return new FloatMenuOption($"Cannot enter {LabelShort} (Waiting for disc delivery)", null);
            }
            else if (LoadedDisc == null)
            {
                yield return new FloatMenuOption($"Cannot enter {LabelShort} (Needs neural imprint disc)", null);
            }
            else if (!IsPowered)
            {
                yield return new FloatMenuOption($"Cannot enter {LabelShort} (No power)", null);
            }
            else if (Occupant != null)
            {
                yield return new FloatMenuOption($"Cannot enter {LabelShort} (Occupied)", null);
            }
        }
    }
}
