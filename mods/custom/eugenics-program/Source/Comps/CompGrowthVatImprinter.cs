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

            bool isConstruct = IsConstruct(occupant);

            // Construct check: prevent stacking if already imprinted
            if (isConstruct && GameComponent_NeuralImprintTracker.Instance?.HasBeenImprinted(occupant) == true)
            {
                WipePassions(occupant);
                return;
            }

            float xp = Props?.xpPerImprintInterval ?? 15f;
            float skillMult = isConstruct ? 0.5f : 1.0f;

            if (blueprint.skillLevels != null)
            {
                foreach (var kvp in blueprint.skillLevels)
                {
                    SkillDef skillDef = kvp.Key;
                    int targetLevel = Mathf.RoundToInt(kvp.Value * skillMult);
                    SkillRecord record = occupant.skills.GetSkill(skillDef);
                    if (record != null && record.Level < targetLevel)
                    {
                        record.Learn(xp, direct: true);
                    }
                }
            }

            if (isConstruct)
            {
                // Explicitly wipe passions for constructs
                WipePassions(occupant);
            }
            else if (Props != null && Props.imprintPassions && blueprint.passions != null)
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

        public void OnPawnDecanted(Pawn pawn, bool fromEmbryo, HumanEmbryo embryo = null)
        {
            if (pawn == null) return;

            bool isConstruct = IsConstruct(pawn, embryo);
            if (!isConstruct)
            {
                // Non-construct: standard human gestation / maturation, no synthetic neural overrides
                return;
            }

            // Ensure construct property trait is attached if missing
            TraitDef traitAsset = NeuralImprintDefOf.Trait_ConstructAsset ?? DefDatabase<TraitDef>.GetNamedSilentFail("Trait_ConstructAsset");
            if (traitAsset != null && pawn.story?.traits != null && !pawn.story.traits.HasTrait(traitAsset))
            {
                pawn.story.traits.GainTrait(new Trait(traitAsset));
            }

            CompNeuralBlueprint bp = LoadedBlueprint;
            bool hasValidBlueprint = bp != null && bp.IsEncoded;

            if (hasValidBlueprint)
            {
                bool alreadyImprinted = GameComponent_NeuralImprintTracker.Instance?.HasBeenImprinted(pawn) == true;
                if (!alreadyImprinted)
                {
                    // Apply baseline reflexes first so unencoded skills are not left at 0 if decanted from embryo
                    if (fromEmbryo)
                    {
                        ApplyBaselineSkills(pawn);
                    }

                    // Transfer skills at 50% multiplier (0.5 of scanned donor skills)
                    if (bp.skillLevels != null)
                    {
                        foreach (var kvp in bp.skillLevels)
                        {
                            SkillDef skillDef = kvp.Key;
                            int targetLevel = Mathf.RoundToInt(kvp.Value * 0.5f);
                            SkillRecord record = pawn.skills?.GetSkill(skillDef);
                            if (record != null)
                            {
                                record.Level = Mathf.Max(record.Level, targetLevel);
                                record.xpSinceLastLevel = 0f;
                            }
                        }
                    }

                    // Explicitly wipe passions for constructs: record.passion = Passion.None
                    WipePassions(pawn);

                    GameComponent_NeuralImprintTracker.Instance?.RegisterImprint(pawn);

                    Messages.Message(
                        $"Construct {pawn.LabelShortCap} decanted with imprinted neural profile ({bp.doctrineTitle ?? LoadedDisc.Label}): 50% donor skills transferred, passions neutralized.",
                        pawn,
                        MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    WipePassions(pawn);
                }
            }
            else
            {
                // Baseline decanting (no neural data):
                // Shooting: 4, Melee: 4, Social: 2, Intellectual: 2, Artistic: 0, all other skills: 3.
                // Passions forced to Passion.None.
                ApplyBaselineSkills(pawn);
                WipePassions(pawn);

                Messages.Message(
                    $"Construct {pawn.LabelShortCap} decanted with innate baseline neural reflexes (no neural blueprint loaded).",
                    pawn,
                    MessageTypeDefOf.NeutralEvent);
            }
        }

        public static void ApplyBaselineSkills(Pawn pawn)
        {
            if (pawn?.skills == null) return;

            foreach (SkillDef skill in DefDatabase<SkillDef>.AllDefs)
            {
                SkillRecord record = pawn.skills.GetSkill(skill);
                if (record == null) continue;

                int level;
                if (skill == SkillDefOf.Shooting) level = 4;
                else if (skill == SkillDefOf.Melee) level = 4;
                else if (skill == SkillDefOf.Social) level = 2;
                else if (skill == SkillDefOf.Intellectual) level = 2;
                else if (skill == SkillDefOf.Artistic) level = 0;
                else level = 3;

                record.Level = level;
                record.xpSinceLastLevel = 0f;
                record.passion = Passion.None;
            }
        }

        public static void WipePassions(Pawn pawn)
        {
            if (pawn?.skills?.skills == null) return;

            foreach (SkillRecord record in pawn.skills.skills)
            {
                record.passion = Passion.None;
            }
        }

        public static bool IsConstruct(Pawn pawn, HumanEmbryo embryo = null)
        {
            if (pawn != null)
            {
                if (Gene_ConstructPsychology.IsConstruct(pawn)) return true;
                if (pawn.story?.traits != null)
                {
                    TraitDef traitAsset = NeuralImprintDefOf.Trait_ConstructAsset ?? DefDatabase<TraitDef>.GetNamedSilentFail("Trait_ConstructAsset");
                    if (traitAsset != null && pawn.story.traits.HasTrait(traitAsset)) return true;
                }
                if (pawn.genes != null)
                {
                    GeneDef geneDef = NeuralImprintDefOf.Gene_ConstructPsychology ?? DefDatabase<GeneDef>.GetNamedSilentFail("Gene_ConstructPsychology");
                    if (geneDef != null && pawn.genes.HasActiveGene(geneDef)) return true;
                }
            }

            if (embryo != null)
            {
                if (embryo.GeneSet != null && embryo.GeneSet.GenesListForReading != null)
                {
                    foreach (var g in embryo.GeneSet.GenesListForReading)
                    {
                        if (g != null && (g.defName == "Gene_ConstructPsychology" || g.geneClass == typeof(Gene_ConstructPsychology)))
                            return true;
                    }
                }
                var comp = embryo.TryGetComp<CompEmbryoQuality>();
                if (comp != null && comp.isConstruct)
                {
                    return true;
                }
            }

            return false;
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
                    if (IsConstruct(occupant))
                    {
                        sb.Append(" - Imprinting construct (50% skill rate, passions neutralized)");
                    }
                    else
                    {
                        sb.Append(" - Streaming proficiencies");
                    }
                }
            }
            else
            {
                sb.Append("Neural Imprinter: None (Empty - Innate baseline reflexes on construct decant)");
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

                            Pawn occupant = GetVatOccupant();
                            bool isConstruct = IsConstruct(occupant);
                            if (isConstruct)
                            {
                                sb.AppendLine("Target: Construct (50% Transfer Multiplier, Passions Disabled)");
                            }
                            else
                            {
                                sb.AppendLine("Target: Standard Pawn (100% Transfer Multiplier)");
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
                                    if (!isConstruct && bp.passions != null && bp.passions.TryGetValue(kvp.Key, out Passion p))
                                    {
                                        if (p == Passion.Major) passionStr = " (Burning Passion 🔥🔥)";
                                        else if (p == Passion.Minor) passionStr = " (Interested Passion 🔥)";
                                    }
                                    int effective = isConstruct ? Mathf.RoundToInt(kvp.Value * 0.5f) : kvp.Value;
                                    string constructNote = isConstruct ? $" [Construct 50%: {effective}]" : "";
                                    sb.AppendLine($"  • {kvp.Key.label.CapitalizeFirst()}: Scanned Level {kvp.Value}{constructNote}{passionStr}");
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
