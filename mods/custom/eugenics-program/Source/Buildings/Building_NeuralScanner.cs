using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    public class Building_NeuralScanner : Building, IThingHolder
    {
        public int scanTicks = 15000;
        public float powerConsumptionScanning = 1500f;
        public float powerConsumptionIdle = 200f;

        protected ThingOwner innerContainer;
        protected ThingOwner discContainer;
        protected int ticksScanning = 0;

        public Building_NeuralScanner()
        {
            innerContainer = new ThingOwner<Thing>(this);
            discContainer = new ThingOwner<Thing>(this);
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

        public bool TryAcceptPawn(Pawn pawn)
        {
            if (!CanAcceptPawn || pawn == null) return false;

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

            if (disc.holdingOwner != null)
            {
                disc.holdingOwner.TryTransferToContainer(disc, discContainer, 1);
            }
            else
            {
                discContainer.TryAdd(disc.SplitOff(1));
            }
            return true;
        }

        public void EjectOccupant()
        {
            if (Occupant != null)
            {
                innerContainer.TryDrop(Occupant, InteractionCell, Map, ThingPlaceMode.Near, out Thing _);
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
                discContainer.TryDrop(LoadedDisc, InteractionCell, Map, ThingPlaceMode.Near, out Thing _);
            }
        }

        protected override void Tick()
        {
            base.Tick();

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
                else
                {
                    // Power interrupted: scan suspended without resetting progress immediately
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

                HediffDef fatigueDef = EugenicsDefOf.Eugenics_NeuralFatigue;
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
            {
                yield return g;
            }

            if (Occupant != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Abort Scan",
                    defaultDesc = "Eject the current subject from the neural scanner and cancel the scanning operation.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Cancel", true) ?? def?.uiIcon,
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

                                options.Add(new FloatMenuOption(p.LabelShortCap, () =>
                                {
                                    Job job = JobMaker.MakeJob(EugenicsDefOf.Eugenics_ScanNeuralProfile, this);
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
                            List<Thing> discs = Map.listerThings.ThingsOfDef(EugenicsDefOf.NeuralBlueprintDisk);
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
                                            TryAcceptDisc(d);
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
            {
                yield return opt;
            }

            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
            {
                yield return new FloatMenuOption($"Cannot use {LabelShort} (Unreachable)", null);
                yield break;
            }

            if (CanAcceptPawn)
            {
                yield return new FloatMenuOption($"Enter {LabelShort}", () =>
                {
                    Job job = JobMaker.MakeJob(EugenicsDefOf.Eugenics_ScanNeuralProfile, this);
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
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
