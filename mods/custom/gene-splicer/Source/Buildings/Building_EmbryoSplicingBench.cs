using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace GeneSplicer
{
    public class Building_EmbryoSplicingBench : Building, IThingHolder
    {
        public Thing targetEmbryo;
        protected ThingOwner embryoContainer;

        // Staged splice modifications (Approach B: staged in UI, executed by doctor work job)
        public List<GeneDef> stagedGenesToAdd = new List<GeneDef>();
        public List<GeneDef> stagedGenesToRemove = new List<GeneDef>();

        public Building_EmbryoSplicingBench()
        {
            embryoContainer = new ThingOwner<Thing>(this, oneStackOnly: true);
        }

        public HumanEmbryo ContainedEmbryo => embryoContainer?.Count > 0 ? embryoContainer[0] as HumanEmbryo : null;
        public bool HasEmbryo => ContainedEmbryo != null;
        public bool CanAcceptEmbryo => ContainedEmbryo == null;
        public bool HasPendingSpliceOrder => HasEmbryo && (stagedGenesToAdd.Count > 0 || stagedGenesToRemove.Count > 0);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref embryoContainer, "embryoContainer", this);
            Scribe_References.Look(ref targetEmbryo, "targetEmbryo");
            Scribe_Collections.Look(ref stagedGenesToAdd, "stagedGenesToAdd", LookMode.Def);
            Scribe_Collections.Look(ref stagedGenesToRemove, "stagedGenesToRemove", LookMode.Def);

            if (embryoContainer == null)
            {
                embryoContainer = new ThingOwner<Thing>(this, oneStackOnly: true);
            }
            if (stagedGenesToAdd == null) stagedGenesToAdd = new List<GeneDef>();
            if (stagedGenesToRemove == null) stagedGenesToRemove = new List<GeneDef>();
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, embryoContainer);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return embryoContainer;
        }

        public bool TryAcceptEmbryo(Thing embryo)
        {
            if (embryo == null || HasEmbryo) return false;

            bool added = false;
            if (embryo.Spawned)
            {
                Thing split = embryo.SplitOff(1);
                added = embryoContainer.TryAdd(split);
            }
            else if (embryo.holdingOwner != null)
            {
                added = embryo.holdingOwner.TryTransferToContainer(embryo, embryoContainer, 1) > 0;
            }
            else
            {
                added = embryoContainer.TryAdd(embryo.SplitOff(1));
            }

            if (added)
            {
                targetEmbryo = null;
            }
            return added;
        }

        public void EjectEmbryo()
        {
            ClearSpliceOrder();
            if (HasEmbryo)
            {
                embryoContainer.TryDrop(ContainedEmbryo, InteractionCell, Map, ThingPlaceMode.Near, out _);
            }
        }

        public void ClearSpliceOrder()
        {
            stagedGenesToAdd.Clear();
            stagedGenesToRemove.Clear();
        }

        protected override void Tick()
        {
            base.Tick();
            if (targetEmbryo != null && (targetEmbryo.Destroyed || HasEmbryo || (!targetEmbryo.Spawned && !(targetEmbryo.holdingOwner?.Owner is Pawn_CarryTracker || targetEmbryo.holdingOwner?.Owner is Pawn))))
            {
                targetEmbryo = null;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            EjectEmbryo();
            base.Destroy(mode);
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder();
            sb.Append(base.GetInspectString());

            if (targetEmbryo != null && !HasEmbryo)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.Append($"Awaiting Embryo: {targetEmbryo.LabelCap}");
            }

            if (HasEmbryo)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.AppendLine($"Contained Embryo: {ContainedEmbryo.LabelCap}");
                CompEmbryoQuality quality = ContainedEmbryo.TryGetComp<CompEmbryoQuality>();
                int edits = quality?.editCount ?? 0;
                sb.AppendLine($"Embryo Genetic Edits: {edits}/2");

                if (HasPendingSpliceOrder)
                {
                    sb.Append($"[Splice Order Pending] +{stagedGenesToAdd.Count} / -{stagedGenesToRemove.Count} genes. Awaiting doctor.");
                }
            }
            else
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.Append("No embryo loaded.");
            }

            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;

            if (Faction != Faction.OfPlayer) yield break;

            if (CanAcceptEmbryo)
            {
                yield return new Command_Action
                {
                    defaultLabel = targetEmbryo != null ? "Cancel Embryo Loading" : "Load Embryo",
                    defaultDesc = "Designate an embryo on the map to be carried and inserted into this Embryo Splicing Bench.",
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Haul", true),
                    action = () =>
                    {
                        if (targetEmbryo != null)
                        {
                            targetEmbryo = null;
                            return;
                        }

                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        List<Thing> embryos = Map.listerThings.ThingsMatching(ThingRequest.ForDef(ThingDefOf.HumanEmbryo));
                        foreach (Thing t in embryos)
                        {
                            if (!t.Spawned || t.IsForbidden(Faction.OfPlayer)) continue;
                            CompEmbryoQuality eq = t.TryGetComp<CompEmbryoQuality>();
                            if (eq != null && eq.editCount >= 2) continue;

                            options.Add(new FloatMenuOption($"Load {t.LabelCap}", () =>
                            {
                                targetEmbryo = t;
                            }));
                        }

                        if (options.Count == 0)
                        {
                            options.Add(new FloatMenuOption("No eligible embryos available", null));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
            }
            else if (HasEmbryo)
            {
                // Open Splicing UI
                yield return new Command_Action
                {
                    defaultLabel = "Edit Genes",
                    defaultDesc = "Open the embryonic gene splicing interface to stage gene modifications from connected Gene Banks.",
                    icon = ContainedEmbryo.def.uiIcon,
                    action = () =>
                    {
                        Find.WindowStack.Add(new Dialog_EditEmbryoGenes(this));
                    }
                };

                // Cancel pending order gizmo
                if (HasPendingSpliceOrder)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Cancel Splice Order",
                        defaultDesc = "Cancel the pending gene modification plan for this embryo.",
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true),
                        action = () =>
                        {
                            ClearSpliceOrder();
                            Messages.Message("Cancelled embryo splicing order.", this, MessageTypeDefOf.NeutralEvent);
                        }
                    };
                }

                // Eject embryo gizmo
                yield return new Command_Action
                {
                    defaultLabel = "Eject Embryo",
                    defaultDesc = "Safely eject the contained human embryo onto the interaction cell.",
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true),
                    action = EjectEmbryo
                };
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption opt in base.GetFloatMenuOptions(selPawn))
                yield return opt;

            if (!selPawn.CanReserveAndReach(this, PathEndMode.InteractionCell, Danger.Some))
                yield break;

            // Option 1: Perform Doctor Splicing job
            if (HasPendingSpliceOrder)
            {
                if (selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
                {
                    yield return new FloatMenuOption("Cannot perform splicing (incapable of Doctoring)", null);
                }
                else
                {
                    yield return new FloatMenuOption($"Perform embryo gene splicing ({stagedGenesToAdd.Count + stagedGenesToRemove.Count} edits)", () =>
                    {
                        Job job = JobMaker.MakeJob(GeneSplicerDefOf.SpliceEmbryo, this);
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    });
                }
            }

            // Option 2: Load designated embryo or find embryo to load
            if (CanAcceptEmbryo)
            {
                if (targetEmbryo != null && targetEmbryo.Spawned && selPawn.CanReserveAndReach(targetEmbryo, PathEndMode.Touch, Danger.Some))
                {
                    yield return new FloatMenuOption($"Load designated embryo ({targetEmbryo.LabelShort})", () =>
                    {
                        Job job = JobMaker.MakeJob(GeneSplicerDefOf.LoadEmbryoToBench, targetEmbryo, this);
                        job.count = 1;
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    });
                }
            }
        }

        // ── Facilities & Bank Gene Query ───────────────────────────────────────────

        public List<Genepack> GetLinkedGenepacks(bool includeUnpowered = true)
        {
            List<Genepack> genepacks = new List<Genepack>();
            var compFacilities = GetComp<CompAffectedByFacilities>();
            if (compFacilities?.LinkedFacilitiesListForReading == null) return genepacks;

            foreach (Thing facility in compFacilities.LinkedFacilitiesListForReading)
            {
                if (facility is Building b && !includeUnpowered)
                {
                    var power = b.GetComp<CompPowerTrader>();
                    if (power != null && !power.PowerOn) continue;
                }

                var container = facility.TryGetComp<CompGenepackContainer>();
                if (container != null && container.ContainedGenepacks != null)
                {
                    genepacks.AddRange(container.ContainedGenepacks);
                }
                else if (facility is IThingHolder holder)
                {
                    ThingOwner directlyHeld = holder.GetDirectlyHeldThings();
                    if (directlyHeld != null)
                    {
                        foreach (Thing t in directlyHeld)
                        {
                            if (t is Genepack gp) genepacks.Add(gp);
                        }
                    }
                }
            }
            return genepacks;
        }

        public HashSet<GeneDef> GetAvailableBankGenes()
        {
            HashSet<GeneDef> set = new HashSet<GeneDef>();
            foreach (Genepack pack in GetLinkedGenepacks(includeUnpowered: false))
            {
                if (pack?.GeneSet?.GenesListForReading == null) continue;
                foreach (GeneDef gene in pack.GeneSet.GenesListForReading)
                {
                    set.Add(gene);
                }
            }
            return set;
        }

        // ── Splicing Execution & Botch Roll (Approach B) ───────────────────────────

        public float CalculateBotchChance(Pawn doctor)
        {
            float roomCleanliness = this.GetRoom()?.GetStat(RoomStatDefOf.Cleanliness) ?? 0f;
            float doctorSkill = doctor?.skills?.GetSkill(SkillDefOf.Medicine)?.Level ?? 6f;
            float manipulation = doctor?.health?.capacities?.GetLevel(PawnCapacityDefOf.Manipulation) ?? 1f;
            float sight = doctor?.health?.capacities?.GetLevel(PawnCapacityDefOf.Sight) ?? 1f;

            // Base failure chance: 20%
            float botch = 0.20f;

            // Medicine skill reduces botch by 1.2% per level (e.g. lvl 10 = -12%, lvl 15 = -18%)
            botch -= (doctorSkill * 0.012f);

            // Manipulation affects dexterity (higher is better, below 100% penalizes heavily)
            botch -= ((manipulation - 1f) * 0.10f);

            // Sight affects precision
            botch -= ((sight - 1f) * 0.05f);

            // Room cleanliness penalty/bonus (clean hospital ~0.6 -> -3%, dirty room -1.0 -> +5%)
            botch -= (roomCleanliness * 0.05f);

            return Mathf.Clamp(botch, 0.01f, 0.50f);
        }

        public void CompleteSpliceOperation(Pawn doctor)
        {
            if (!HasEmbryo || !HasPendingSpliceOrder) return;

            HumanEmbryo embryo = ContainedEmbryo;
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            int currentEdits = quality?.editCount ?? 0;
            int requestedEdits = stagedGenesToAdd.Count + stagedGenesToRemove.Count;

            if (currentEdits + requestedEdits > 2)
            {
                Messages.Message("Cannot complete splicing: Exceeds the 2-gene edit cap.", this, MessageTypeDefOf.RejectInput);
                ClearSpliceOrder();
                return;
            }

            float botchChance = CalculateBotchChance(doctor);
            if (Rand.Chance(botchChance))
            {
                // Botch: Catastrophic cellular collapse -> 1x GeneticNutrientPaste
                embryoContainer.Remove(embryo);
                embryo.Destroy(DestroyMode.Vanish);

                Thing paste = ThingMaker.MakeThing(GeneSplicerDefOf.GeneticNutrientPaste);
                paste.stackCount = 1;
                GenPlace.TryPlaceThing(paste, InteractionCell, Map, ThingPlaceMode.Near);

                ClearSpliceOrder();
                SoundDefOf.Crunch.PlayOneShot(new TargetInfo(Position, Map));

                Messages.Message(
                    $"Embryonic gene splicing botched by {doctor.LabelShort}! Cellular structure collapsed into Genetic Nutrient Paste (Botch chance: {botchChance:P1}).",
                    new TargetInfo(Position, Map),
                    MessageTypeDefOf.NegativeEvent);
                return;
            }

            // Success: Apply mutations
            foreach (GeneDef gene in stagedGenesToAdd)
            {
                embryo.GeneSet?.AddGene(gene);
            }
            foreach (GeneDef gene in stagedGenesToRemove)
            {
                embryo.GeneSet?.Debug_RemoveGene(gene);
            }

            if (quality != null)
            {
                quality.editCount += requestedEdits;
            }

            GeneSplicerParentalUtility.ApplyParentalThoughts(embryo);

            Messages.Message(
                $"Embryonic gene splicing completed successfully by {doctor.LabelShort} (+{stagedGenesToAdd.Count}/-{stagedGenesToRemove.Count} genes).",
                new TargetInfo(Position, Map),
                MessageTypeDefOf.PositiveEvent);

            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High);
            ClearSpliceOrder();
        }
    }
}
