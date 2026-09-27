using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    public class Building_EmbryoSplicingBench : Building_WorkTable, IThingHolder
    {
        protected ThingOwner discContainer;

        public Building_EmbryoSplicingBench()
        {
            discContainer = new ThingOwner<Thing>(this, oneStackOnly: true);
        }

        public Thing LoadedDisc => discContainer?.Count > 0 ? discContainer[0] : null;
        public CompGenomeBlueprint LoadedBlueprintComp => LoadedDisc?.TryGetComp<CompGenomeBlueprint>();
        public bool HasLoadedBlueprint => LoadedBlueprintComp != null && (LoadedBlueprintComp.genes?.Count ?? 0) > 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref discContainer, "discContainer", this);
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

        public void EjectDisc()
        {
            if (LoadedDisc != null)
            {
                discContainer.TryDrop(LoadedDisc, InteractionCell, Map, ThingPlaceMode.Near, out _);
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

            if (LoadedDisc != null)
            {
                sb.AppendLine();
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
                sb.AppendLine();
                sb.Append("No Genome Blueprint Disc loaded (Required for batch genome imprinting)");
            }

            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

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
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "Load Blueprint Disc",
                    defaultDesc = "Select an available genome blueprint disc on the map to install into the bench.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", false) ?? def?.uiIcon,
                    action = () =>
                    {
                        var options = new List<FloatMenuOption>();
                        List<Thing> discs = Map?.listerThings.ThingsOfDef(EugenicsDefOf.GenomeBlueprintDisk);
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
                                        TryAcceptDisc(d);
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
    }
}
