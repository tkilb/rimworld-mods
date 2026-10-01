using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class Building_ConstructGenomeArchitect : Building, IThingHolder
    {
        public ThingOwner<Thing> discContainer;
        public CompPowerTrader powerComp;

        private List<Building> _cachedGeneBanks;
        private int _geneBankCacheTick = -999;
        private const int CacheInterval = 250; // Re-scan every ~4 seconds

        public Thing LoadedDisc => discContainer.InnerListForReading.Count > 0 ? discContainer.InnerListForReading[0] : null;
        public CompGenomeBlueprint LoadedBlueprintComp => LoadedDisc?.TryGetComp<CompGenomeBlueprint>();
        public bool HasBlankDisc => LoadedBlueprintComp != null && LoadedBlueprintComp.IsBlank;

        public List<Building> ConnectedGeneBanks
        {
            get
            {
                if (!Spawned) return new List<Building>();
                int tick = Find.TickManager.TicksGame;
                if (_cachedGeneBanks == null || tick - _geneBankCacheTick > CacheInterval)
                {
                    _cachedGeneBanks = new List<Building>();
                    foreach (Thing t in GenRadial.RadialDistinctThingsAround(Position, Map, 16f, true))
                    {
                        if (t is Building bank && bank.def.defName == "GeneBank")
                        {
                            CompPowerTrader pwr = bank.GetComp<CompPowerTrader>();
                            if (pwr != null && pwr.PowerOn)
                                _cachedGeneBanks.Add(bank);
                        }
                    }
                    _geneBankCacheTick = tick;
                }
                return _cachedGeneBanks;
            }
        }

        public Building_ConstructGenomeArchitect()
        {
            discContainer = new ThingOwner<Thing>(this, oneStackOnly: true);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref discContainer, "discContainer", this);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return discContainer;
        }

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            if (LoadedDisc != null)
            {
                str += $"\nLoaded Disc: {LoadedDisc.LabelShort}";
                if (HasBlankDisc)
                {
                    str += " (Blank)";
                }
                else
                {
                    str += $" (Burned: {LoadedBlueprintComp.templateLabel})";
                }
            }
            else
            {
                str += "\nLoaded Disc: None";
            }
            str += $"\nConnected Gene Banks: {ConnectedGeneBanks.Count}";
            return str;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            if (LoadedDisc == null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Load Blank Disc",
                    defaultDesc = "Order a colonist to haul a blank Genome Blueprint Disc to this machine.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", false) ?? def?.uiIcon,
                    action = () =>
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (Pawn p in Map.mapPawns.FreeColonistsSpawned)
                        {
                            if (p.WorkTypeIsDisabled(WorkTypeDefOf.Hauling)) continue;
                            
                            Thing blankDisc = GenClosest.ClosestThingReachable(
                                Position, Map,
                                ThingRequest.ForDef(ConstructDefOf.GenomeBlueprintDisk),
                                PathEndMode.ClosestTouch,
                                TraverseParms.For(p),
                                validator: t => !t.IsForbidden(p) && p.CanReserve(t) && t.TryGetComp<CompGenomeBlueprint>() != null && t.TryGetComp<CompGenomeBlueprint>().IsBlank
                            );

                            if (blankDisc != null)
                            {
                                options.Add(new FloatMenuOption($"{p.LabelShortCap} (Haul)", () =>
                                {
                                    Job job = JobMaker.MakeJob(ConstructDefOf.Construct_HaulDiscToContainer, blankDisc, this);
                                    job.count = 1;
                                    p.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                }));
                            }
                        }
                        if (options.Count == 0)
                        {
                            options.Add(new FloatMenuOption("No blank discs or available haulers", null));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "Eject Disc",
                    defaultDesc = "Eject the currently loaded genome blueprint disc.",
                    icon = LoadedDisc.def.uiIcon,
                    action = () =>
                    {
                        if (discContainer.TryDrop(LoadedDisc, Position, Map, ThingPlaceMode.Near, out Thing dropped))
                        {
                            Messages.Message("Disc ejected.", this, MessageTypeDefOf.NeutralEvent);
                        }
                    }
                };
            }

            if (HasBlankDisc && powerComp != null && powerComp.PowerOn)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Configure Genome",
                    defaultDesc = "Open the Genome Architect interface to configure a new construct genome template.",
                    icon = GeneSetHolderBase.GeneticInfoTex.Texture ?? def?.uiIcon,
                    action = () =>
                    {
                        Find.WindowStack.Add(new Dialog_ConfigureConstructGenome(this));
                    }
                };
            }
        }
    }
}
