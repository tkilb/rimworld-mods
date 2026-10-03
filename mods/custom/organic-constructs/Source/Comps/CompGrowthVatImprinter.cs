using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace OrganicConstructs
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

            if (isConstruct)
            {
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
                return;
            }

            Gene_ConstructPsychology.ApplyConstructPhysiology(pawn);

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
                    if (fromEmbryo)
                    {
                        ApplyBaselineSkills(pawn);
                    }

                    if (bp.skillLevels != null)
                    {
                        foreach (var kvp in bp.skillLevels)
                        {
                            SkillDef skillDef = kvp.Key;
                            int targetLevel = kvp.Value;
                            SkillRecord record = pawn.skills?.GetSkill(skillDef);
                            if (record != null)
                            {
                                record.Level = targetLevel;
                                record.xpSinceLastLevel = 0f;
                            }
                        }
                    }

                    WipePassions(pawn);
                    GameComponent_NeuralImprintTracker.Instance?.RegisterImprint(pawn);

                    Messages.Message(
                        $"Construct {pawn.LabelShortCap} decanted with imprinted neural profile ({bp.doctrineTitle ?? LoadedDisc.Label}): skills transferred from disc, passions neutralized.",
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
                if (fromEmbryo)
                {
                    ApplyBaselineSkills(pawn);
                    WipePassions(pawn);

                    Messages.Message(
                        $"Construct {pawn.LabelShortCap} decanted with innate baseline neural reflexes (no neural blueprint loaded).",
                        pawn,
                        MessageTypeDefOf.NeutralEvent);
                }
                else
                {
                    WipePassions(pawn);
                }
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
            if (pawn != null && ConstructUtility.IsConstruct(pawn)) return true;
            if (embryo != null && ConstructUtility.IsConstructEmbryo(embryo)) return true;
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
                yield return g;

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
                        defaultLabel = "View Neural Imprint",
                        defaultDesc = "Inspect the skills and passions encoded in the currently loaded neural blueprint disc.",
                        icon = parent.def.uiIcon,
                        action = () =>
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"--- {bp.doctrineTitle ?? "Neural Imprint"} ---");
                            if (!string.IsNullOrEmpty(bp.donorName))
                            {
                                sb.AppendLine($"Mentor / Source: {bp.donorName}");
                            }
                            sb.AppendLine();

                            Pawn occupant = GetVatOccupant();
                            bool isConstruct = IsConstruct(occupant);
                            if (isConstruct)
                            {
                                sb.AppendLine("Target: Construct (1:1 Disc Inheritance, Passions Disabled)");
                            }
                            else
                            {
                                sb.AppendLine("Target: Standard Pawn (Vat Learning Imprint)");
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
                                    sb.AppendLine($"  • {kvp.Key.label.CapitalizeFirst()}: Scanned Level {kvp.Value}{passionStr}");
                                }
                            }

                            Find.WindowStack.Add(new Dialog_MessageBox(sb.ToString(), "Close", title: bp.doctrineTitle ?? "Neural Imprint"));
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
                        List<Thing> discs = parent.Map?.listerThings.ThingsOfDef(ConstructDefOf.NeuralBlueprintDisk);
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

            // ── Reclaim Biomass Gizmo (Less triggering alternative to abort) ──
            if (parent is Building_GrowthVat vat)
            {
                bool isConstructEmbryo = vat.selectedEmbryo != null && ConstructUtility.IsConstructEmbryo(vat.selectedEmbryo);
                Pawn occupant = Traverse.Create(vat).Field("selectedPawn").GetValue<Pawn>();
                bool isConstructOccupant = occupant != null && ConstructUtility.IsConstruct(occupant);

                if (isConstructEmbryo || isConstructOccupant)
                {
                    int pasteCount = CalculateReclaimPasteCount(vat);
                    yield return new Command_Action
                    {
                        defaultLabel = "Reclaim Biomass",
                        defaultDesc = $"Terminate this construct's gestation and dissolve its biological material into Genetic Nutrient Paste. Reclaims approximately 75% of the total nutritional investment (yields {pasteCount}x paste).",
                        icon = DefDatabase<ThingDef>.GetNamedSilentFail("GeneticNutrientPaste")?.uiIcon ?? ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true) ?? parent.def.uiIcon,
                        action = () =>
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                                $"Are you sure you want to reclaim this construct's biomass?\n\nThe developing construct will be terminated and dissolved into {pasteCount}x Genetic Nutrient Paste (75% return on invested nutrition).",
                                () => ReclaimBiomass(vat),
                                destructive: true
                            ));
                        }
                    };
                }
            }
        }

        public static int CalculateReclaimPasteCount(Building_GrowthVat vat)
        {
            float synthesisNutrition = 4.0f; // 40 raw meat + 40 raw plant food from synthesis bill
            float vatNutrition = 0f;

            if (vat.selectedEmbryo != null)
            {
                // Embryo phase: 6 nutrition/day over 3 days = 18 nutrition total
                int gest = Traverse.Create(vat).Field("gestationTicks").GetValue<int>();
                float progressFraction = Mathf.Clamp01(gest / 540000f);
                vatNutrition = progressFraction * 18f;
            }
            else
            {
                Pawn occupant = Traverse.Create(vat).Field("selectedPawn").GetValue<Pawn>();
                if (occupant != null)
                {
                    // Embryo phase was completed (18 nutrition).
                    // Maturation phase: 3 nutrition/day over 12 days = 36 nutrition total.
                    const long Age13Ticks = 13L * 3600000L;
                    float progressFraction = Mathf.Clamp01((float)occupant.ageTracker.AgeBiologicalTicks / Age13Ticks);
                    vatNutrition = 18f + (progressFraction * 36f);
                }
            }

            float totalNutrition = synthesisNutrition + vatNutrition;
            float reclaimedNutrition = totalNutrition * 0.75f;
            // GeneticNutrientPaste provides 0.9 nutrition per unit
            return Mathf.Max(1, Mathf.RoundToInt(reclaimedNutrition / 0.9f));
        }

        public static void ReclaimBiomass(Building_GrowthVat vat)
        {
            if (vat == null) return;
            int pasteCount = CalculateReclaimPasteCount(vat);

            if (vat.selectedEmbryo != null)
            {
                HumanEmbryo embryo = vat.selectedEmbryo;
                vat.selectedEmbryo = null;
                if (vat.innerContainer.Contains(embryo))
                {
                    vat.innerContainer.Remove(embryo);
                }
                if (!embryo.Destroyed)
                {
                    embryo.Destroy();
                }
            }

            Pawn occupant = Traverse.Create(vat).Field("selectedPawn").GetValue<Pawn>();
            if (occupant != null)
            {
                Traverse.Create(vat).Field("selectedPawn").SetValue(null);
                if (vat.innerContainer.Contains(occupant))
                {
                    vat.innerContainer.Remove(occupant);
                }
                if (!occupant.Destroyed)
                {
                    occupant.Destroy();
                }
            }

            Traverse.Create(vat).Field("gestationTicks").SetValue(0);

            // Spawn GeneticNutrientPaste stack
            ThingDef pasteDef = DefDatabase<ThingDef>.GetNamedSilentFail("GeneticNutrientPaste") ?? ThingDefOf.MealNutrientPaste;
            if (pasteDef != null && vat.Map != null)
            {
                Thing paste = ThingMaker.MakeThing(pasteDef);
                paste.stackCount = pasteCount;
                IntVec3 spawnLoc = vat.def.hasInteractionCell ? vat.InteractionCell : vat.Position;
                GenPlace.TryPlaceThing(paste, spawnLoc, vat.Map, ThingPlaceMode.Near);
            }

            SoundDef sound = SoundDef.Named("Recipe_ButcherCorpseFlesh");
            if (sound != null && vat.Map != null)
            {
                sound.PlayOneShot(new TargetInfo(vat.Position, vat.Map));
            }

            Messages.Message(
                $"Construct biomass reclaimed: Gestation terminated and {pasteCount}x Genetic Nutrient Paste recovered (75% nutritional return).",
                vat,
                MessageTypeDefOf.NeutralEvent);
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption opt in base.CompFloatMenuOptions(selPawn))
                yield return opt;

            if (targetDisc != null && targetDisc.Spawned && !targetDisc.Destroyed && LoadedDisc == null)
            {
                if (selPawn.CanReserveAndReach(targetDisc, PathEndMode.ClosestTouch, Danger.Some) &&
                    selPawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Some))
                {
                    yield return new FloatMenuOption($"Haul {targetDisc.Label} to {parent.LabelShort}", () =>
                    {
                        Job job = JobMaker.MakeJob(ConstructDefOf.Construct_HaulDiscToContainer, targetDisc, parent);
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
