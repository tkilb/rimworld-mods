using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    public class CompGrowthVatImprinter : ThingComp, IThingHolder
    {
        public CompProperties_GrowthVatImprinter Props => (CompProperties_GrowthVatImprinter)props;

        public Thing targetDisc;
        protected ThingOwner discContainer;

        public CompGrowthVatImprinter()
        {
            discContainer = new ThingOwner<Thing>(this);
        }

        public Thing LoadedDisc => discContainer.Count > 0 ? discContainer[0] : null;
        public CompNeuralBlueprint LoadedBlueprint => LoadedDisc?.TryGetComp<CompNeuralBlueprint>();
        public bool CanAcceptDisc => LoadedDisc == null;
        public new IThingHolder ParentHolder => (IThingHolder)parent;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref discContainer, "discContainer", this);
            Scribe_References.Look(ref targetDisc, "targetDisc");

            if (discContainer == null)
            {
                discContainer = new ThingOwner<Thing>(this);
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

        public bool TryLoadDisc(Thing disc)
        {
            if (LoadedDisc != null || disc == null) return false;

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
                IntVec3 dropCell = parent.def.hasInteractionCell ? parent.InteractionCell : parent.Position;
                discContainer.TryDrop(LoadedDisc, dropCell, parent.Map, ThingPlaceMode.Near, out Thing _);
            }
        }

        public Pawn GetVatOccupant()
        {
            if (parent is IThingHolder holder)
            {
                ThingOwner heldThings = holder.GetDirectlyHeldThings();
                if (heldThings != null)
                {
                    for (int i = 0; i < heldThings.Count; i++)
                    {
                        if (heldThings[i] is Pawn p)
                        {
                            return p;
                        }
                    }
                }
            }
            return null;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (!parent.Spawned) return;

            if (targetDisc != null && (targetDisc.Destroyed || LoadedDisc != null || (!targetDisc.Spawned && !(targetDisc.holdingOwner?.Owner is Pawn_CarryTracker || targetDisc.holdingOwner?.Owner is Pawn))))
            {
                targetDisc = null;
            }

            CompPowerTrader power = parent.TryGetComp<CompPowerTrader>();
            if (power != null && !power.PowerOn) return;

            int interval = Props?.imprintIntervalTicks ?? 250;
            if (parent.IsHashIntervalTick(interval))
            {
                ImprintTick();
            }
        }

        private void ImprintTick()
        {
            CompNeuralBlueprint blueprint = LoadedBlueprint;
            if (blueprint == null || !blueprint.IsEncoded) return;

            Pawn occupant = GetVatOccupant();
            if (occupant == null || occupant.skills == null) return;

            float xp = Props?.xpPerImprintInterval ?? 15f;

            if (blueprint.skillLevels != null)
            {
                foreach (var kvp in blueprint.skillLevels)
                {
                    SkillDef skillDef = kvp.Key;
                    int targetLevel = kvp.Value;
                    SkillRecord record = occupant.skills.GetSkill(skillDef);
                    if (record != null && record.Level < targetLevel)
                    {
                        record.Learn(xp, direct: true);
                    }
                }
            }

            if (Props != null && Props.imprintPassions && blueprint.passions != null)
            {
                foreach (var kvp in blueprint.passions)
                {
                    SkillDef skillDef = kvp.Key;
                    Passion targetPassion = kvp.Value;
                    SkillRecord record = occupant.skills.GetSkill(skillDef);
                    if (record != null && (int)record.passion < (int)targetPassion)
                    {
                        record.passion = targetPassion;
                    }
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (targetDisc != null && LoadedDisc == null)
            {
                sb.Append($"Neural Imprinter: Waiting for disc delivery ({targetDisc.Label})");
            }
            else if (LoadedDisc != null)
            {
                CompNeuralBlueprint bp = LoadedBlueprint;
                string title = bp?.doctrineTitle ?? LoadedDisc.Label;
                sb.Append($"Neural Imprinter: {title} (Loaded)");

                Pawn occupant = GetVatOccupant();
                if (occupant != null && bp?.IsEncoded == true)
                {
                    sb.Append(" - Streaming proficiencies");
                }
            }
            else
            {
                sb.Append("Neural Imprinter: None (Empty)");
            }

            return sb.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            if (LoadedDisc != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Eject Neural Disc",
                    defaultDesc = "Eject the loaded neural imprint disc from this growth vat.",
                    icon = LoadedDisc.def.uiIcon,
                    action = EjectDisc
                };

                CompNeuralBlueprint bp = LoadedBlueprint;
                if (bp != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "View Imprint Doctrine",
                        defaultDesc = "Inspect the skills and passions encoded in the currently loaded neural blueprint disc.",
                        icon = parent.def.uiIcon,
                        action = () =>
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"--- {bp.doctrineTitle ?? "Neural Imprint Doctrine"} ---");
                            if (!string.IsNullOrEmpty(bp.donorName))
                            {
                                sb.AppendLine($"Mentor / Source: {bp.donorName}");
                            }
                            sb.AppendLine();
                            sb.AppendLine("Streaming Skills & Passions:");
                            if (bp.skillLevels == null || bp.skillLevels.Count == 0)
                            {
                                sb.AppendLine("  (No skills recorded)");
                            }
                            else
                            {
                                foreach (var kvp in bp.skillLevels)
                                {
                                    string passionStr = "";
                                    if (bp.passions != null && bp.passions.TryGetValue(kvp.Key, out Passion p))
                                    {
                                        if (p == Passion.Major) passionStr = " (Burning Passion 🔥🔥)";
                                        else if (p == Passion.Minor) passionStr = " (Interested Passion 🔥)";
                                    }
                                    sb.AppendLine($"  • {kvp.Key.label.CapitalizeFirst()}: Target Level {kvp.Value}{passionStr}");
                                }
                            }

                            Find.WindowStack.Add(new Dialog_MessageBox(sb.ToString(), "Close", title: bp.doctrineTitle ?? "Imprint Doctrine"));
                        }
                    };
                }
            }
            else if (targetDisc != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Cancel Neural Disc Delivery",
                    defaultDesc = $"Cancel waiting for delivery of {targetDisc.Label}.",
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true) ?? parent.def.uiIcon,
                    action = () => targetDisc = null
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "Load Neural Disc",
                    defaultDesc = "Select an available encoded neural imprint disc on the map to install into this growth vat.",
                    icon = parent.def.uiIcon,
                    action = () =>
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        List<Thing> discs = parent.Map?.listerThings.ThingsOfDef(EugenicsDefOf.NeuralBlueprintDisk);
                        if (discs != null)
                        {
                            foreach (Thing d in discs)
                            {
                                if (d.Spawned && !d.IsForbidden(Faction.OfPlayer))
                                {
                                    CompNeuralBlueprint bp = d.TryGetComp<CompNeuralBlueprint>();
                                    if (bp?.IsEncoded == true)
                                    {
                                        options.Add(new FloatMenuOption($"{d.Label} ({bp.doctrineTitle})", () =>
                                        {
                                            targetDisc = d;
                                        }));
                                    }
                                }
                            }
                        }

                        if (options.Count == 0)
                        {
                            options.Add(new FloatMenuOption("No encoded neural imprint discs found on map", null));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption opt in base.CompFloatMenuOptions(selPawn))
            {
                yield return opt;
            }

            if (targetDisc != null && targetDisc.Spawned && !targetDisc.Destroyed && LoadedDisc == null)
            {
                if (selPawn.CanReserveAndReach(targetDisc, PathEndMode.ClosestTouch, Danger.Some) &&
                    selPawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Some))
                {
                    yield return new FloatMenuOption($"Haul {targetDisc.Label} to {parent.LabelShort}", () =>
                    {
                        Job job = JobMaker.MakeJob(EugenicsDefOf.Eugenics_HaulDiscToContainer, targetDisc, parent);
                        job.count = 1;
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    });
                }
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (previousMap != null && LoadedDisc != null)
            {
                discContainer.TryDropAll(parent.Position, previousMap, ThingPlaceMode.Near);
            }
            base.PostDestroy(mode, previousMap);
        }
    }
}
