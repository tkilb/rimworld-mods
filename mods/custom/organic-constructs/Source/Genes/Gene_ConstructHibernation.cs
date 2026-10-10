using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class Gene_ConstructHibernation : Gene
    {
        public int operatingTicks = 0;
        public int stasisTicks = 0;
        public bool inStasis = false;
        public bool maintenanceWarningFired = false;
        public bool autoWake = false;
        public bool notifiedWakeOK = false;
        public bool autoStasisNotificationFired = false;
        public int stasisStartTick = -1;

        private static readonly CachedTexture WakeCommandTex = new CachedTexture("UI/Gizmos/Wake");
        private static readonly CachedTexture AutoWakeCommandTex = new CachedTexture("UI/Gizmos/DeathrestAutoWake");

        public const int MaxOperatingTicks = 1800000; // 30 days
        public const int WarningTicksRemaining = 75000; // 1.25 days
        public const int AutoStasisTicksRemaining = 10000; // 4 hours before shutdown (1 hour = 2500 ticks)
        public const int MinStasisTicks = 120000; // 48 hours in standard bed
        public const int PodMinStasisTicks = 90000; // 36 hours in powered stasis pod
        public const int MinLockoutTicks = 15000; // 6 hours mandatory minimum commitment (1h = 2500 ticks)

        /// <summary>Checks if the construct is resting in a powered Building_ConstructStasisPod.</summary>
        public bool IsInPoweredStasisPod()
        {
            if (pawn == null) return false;
            Building_Bed bed = pawn.CurrentBed();
            if (bed is Building_ConstructStasisPod pod)
            {
                return pod.IsPowered;
            }
            return false;
        }

        /// <summary>Returns the required stasis duration in ticks for the current resting spot.</summary>
        public int GetTargetStasisTicks()
        {
            return IsInPoweredStasisPod() ? PodMinStasisTicks : MinStasisTicks;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref operatingTicks, "operatingTicks", 0);
            Scribe_Values.Look(ref stasisTicks, "stasisTicks", 0);
            Scribe_Values.Look(ref inStasis, "inStasis", false);
            Scribe_Values.Look(ref maintenanceWarningFired, "maintenanceWarningFired", false);
            Scribe_Values.Look(ref autoWake, "autoWake", false);
            Scribe_Values.Look(ref notifiedWakeOK, "notifiedWakeOK", false);
            Scribe_Values.Look(ref autoStasisNotificationFired, "autoStasisNotificationFired", false);
            Scribe_Values.Look(ref stasisStartTick, "stasisStartTick", -1);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (!inStasis)
                {
                    stasisStartTick = -1;
                    stasisTicks = 0;
                }
                else if (pawn?.health?.hediffSet != null && !pawn.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis))
                {
                    pawn.health.AddHediff(ConstructDefOf.Construct_InStasis);
                }
            }
        }

        /// <summary>Recomputes elapsed stasis ticks from the game clock and pod rate.</summary>
        public void UpdateStasisTicks()
        {
            if (!inStasis)
            {
                stasisTicks = 0;
                stasisStartTick = -1;
                return;
            }
            if (stasisStartTick < 0)
            {
                // Fallback: anchor start tick to current game tick
                stasisStartTick = Find.TickManager.TicksGame - stasisTicks;
            }
            // If in a powered pod, ticks progress at 1.333x speed (48h/36h) so 36 real hours yields 48h equivalent
            // Alternatively, stasisTicks tracks direct progress towards GetTargetStasisTicks().
            stasisTicks = Mathf.Max(0, Find.TickManager.TicksGame - stasisStartTick);
        }

        public override void Tick()
        {
            base.Tick();

            if (inStasis)
            {
                UpdateStasisTicks();
                int targetTicks = GetTargetStasisTicks();
                if (stasisTicks >= targetTicks)
                {
                    if (autoWake)
                    {
                        Wake();
                        return;
                    }
                    if (!notifiedWakeOK)
                    {
                        notifiedWakeOK = true;
                        if (PawnUtility.ShouldSendNotificationAbout(pawn))
                        {
                            Messages.Message($"{pawn.NameShortColored} has completed the required stasis duration and is ready to wake.", pawn, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
                return;
            }

            if (pawn.IsHashIntervalTick(250))
            {
                Need socialNeed = pawn.needs?.TryGetNeed(DefDatabase<NeedDef>.GetNamedSilentFail("Social"));
                if (socialNeed != null && socialNeed.CurLevelPercentage < 0.7f)
                {
                    socialNeed.CurLevelPercentage = 0.7f;
                }

                // Auto-seek bed and enter stasis 4 hours before emergency shutdown, unless drafted
                if (pawn.Spawned && !pawn.Downed && !pawn.Drafted && !pawn.InMentalState && pawn.Faction == Faction.OfPlayer)
                {
                    int remainingTicks = MaxOperatingTicks - operatingTicks;
                    if (remainingTicks <= AutoStasisTicksRemaining && operatingTicks < MaxOperatingTicks)
                    {
                        if (pawn.CurJobDef != ConstructDefOf.Construct_EnterConstructStasis)
                        {
                            if (!autoStasisNotificationFired)
                            {
                                autoStasisNotificationFired = true;
                                Messages.Message($"{pawn.NameShortColored} is automatically seeking a bed for stasis before emergency shutdown.", pawn, MessageTypeDefOf.CautionInput);
                            }

                            Building_Bed bed = RestUtility.FindBedFor(pawn);
                            Job job = bed != null
                                ? JobMaker.MakeJob(ConstructDefOf.Construct_EnterConstructStasis, bed)
                                : JobMaker.MakeJob(ConstructDefOf.Construct_EnterConstructStasis, pawn.Position);
                            if (!pawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds))
                            {
                                job = JobMaker.MakeJob(ConstructDefOf.Construct_EnterConstructStasis, pawn.Position);
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
                            }
                        }
                    }
                }
            }

            operatingTicks++;
            if (operatingTicks >= MaxOperatingTicks - WarningTicksRemaining && !maintenanceWarningFired)
            {
                maintenanceWarningFired = true;
                Find.LetterStack.ReceiveLetter("Construct Maintenance Required",
                    $"{pawn.NameShortColored} is approaching the 30-day operating limit. They must enter stasis soon or they will trigger an emergency shutdown.",
                    LetterDefOf.NegativeEvent, pawn);
            }

            if (operatingTicks >= MaxOperatingTicks && pawn.Spawned && !pawn.Downed)
            {
                // Emergency shutdown (comatose)
                if (!pawn.health.hediffSet.HasHediff(ConstructDefOf.Construct_Assimilation))
                {
                    pawn.health.AddHediff(ConstructDefOf.Construct_Assimilation);
                    Messages.Message($"{pawn.NameShortColored} has reached maximum operating age and entered emergency shutdown.", pawn, MessageTypeDefOf.NegativeEvent);
                }
            }
        }

        public void Wake()
        {
            if (inStasis)
            {
                ApplyWakeEffects();
            }
        }

        public void ApplyWakeEffects()
        {
            if (!inStasis) return;
            UpdateStasisTicks();
            int targetTicks = GetTargetStasisTicks();
            bool cleanWake = (stasisTicks >= targetTicks);
            bool inPod = IsInPoweredStasisPod();

            inStasis = false;
            stasisStartTick = -1;
            stasisTicks = 0;
            notifiedWakeOK = false;

            // Remove InStasis hediff
            Hediff inStasisHediff = pawn.health.hediffSet.GetFirstHediffOfDef(ConstructDefOf.Construct_InStasis);
            if (inStasisHediff != null)
            {
                pawn.health.RemoveHediff(inStasisHediff);
            }

            if (!cleanWake)
            {
                // Interrupted Stasis: pod applies buffered minor version, standard bed applies full version
                HediffDef interruptHediff = inPod ? ConstructDefOf.Construct_MinorInterruptedStasis : ConstructDefOf.Construct_InterruptedStasis;
                if (interruptHediff != null)
                {
                    pawn.health.AddHediff(interruptHediff);
                }

                // Incentive mechanic: Early wake only grants at most 50% operating margin (15 days)
                // If the construct had less than 15 days margin remaining, refund up to half margin (900,000 ticks).
                // If they already had more than 15 days margin, preserve their existing operatingTicks.
                int halfOperatingTicks = MaxOperatingTicks / 2;
                if (operatingTicks > halfOperatingTicks)
                {
                    operatingTicks = halfOperatingTicks;
                }
                maintenanceWarningFired = false;
                autoStasisNotificationFired = false;

                string penaltyText = inPod
                    ? $"{pawn.NameShortColored} was awakened from stasis pod early: buffered -10% cognition and operating margin capped at 15 days. Return to stasis for a full refresh."
                    : $"{pawn.NameShortColored} was awakened from stasis early: severe -20% cognition and operating margin capped at 15 days. Return to stasis for a full refresh.";
                Messages.Message(penaltyText, pawn, MessageTypeDefOf.NegativeEvent);
            }
            else
            {
                // Clean wake
                operatingTicks = 0;
                maintenanceWarningFired = false;
                autoStasisNotificationFired = false;

                // Clear interrupted debuffs if lingering from prior interruptions
                Hediff interruptedHediff = pawn.health.hediffSet.GetFirstHediffOfDef(ConstructDefOf.Construct_InterruptedStasis);
                if (interruptedHediff != null)
                {
                    pawn.health.RemoveHediff(interruptedHediff);
                }
                Hediff minorInterrupted = pawn.health.hediffSet.GetFirstHediffOfDef(ConstructDefOf.Construct_MinorInterruptedStasis);
                if (minorInterrupted != null)
                {
                    pawn.health.RemoveHediff(minorInterrupted);
                }

                if (pawn.needs != null)
                {
                    if (pawn.needs.rest != null)
                    {
                        pawn.needs.rest.CurLevel = 1f;
                    }
                    if (pawn.needs.food != null && pawn.needs.food.CurLevelPercentage < 0.5f)
                    {
                        pawn.needs.food.CurLevelPercentage = 0.5f;
                    }
                }

                // Remove malnutrition if present from before stasis
                Hediff mal = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
                if (mal != null)
                {
                    pawn.health.RemoveHediff(mal);
                }

                // Remove coma if they were in emergency shutdown
                Hediff coma = pawn.health.hediffSet.GetFirstHediffOfDef(ConstructDefOf.Construct_Assimilation);
                if (coma != null)
                {
                    pawn.health.RemoveHediff(coma);
                }

                Messages.Message($"{pawn.NameShortColored} completed a full stasis cycle and is fully refreshed.", pawn, MessageTypeDefOf.PositiveEvent);
            }

            // End stasis job if the pawn is currently doing it
            if (pawn.Spawned && pawn.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
        }

        public string GetStasisTooltip()
        {
            if (inStasis)
            {
                float hours = (float)stasisTicks / 2500f;
                int targetTicks = GetTargetStasisTicks();
                float totalHours = (float)targetTicks / 2500f;
                string locationStr = IsInPoweredStasisPod() ? "Stasis Pod (Accelerated 36h cycle)" : "Standard Bed (48h cycle)";

                if (stasisTicks >= targetTicks)
                {
                    return $"Construct hibernation stasis ({locationStr})\nStatus: Complete ({hours:F1}h elapsed)\nReady to wake safely. Stasis will continue until woken.";
                }
                if (stasisTicks < MinLockoutTicks)
                {
                    float lockHours = (float)(MinLockoutTicks - stasisTicks) / 2500f;
                    return $"Construct hibernation stasis ({locationStr})\nProgress: {hours:F1} / {totalHours:F0} hours\n[LOCKED] Initial cryo-neural synchronization in progress. Minimum commitment: {lockHours:F1}h remaining before early wake is permitted.";
                }
                return $"Construct hibernation stasis ({locationStr})\nProgress: {hours:F1} / {totalHours:F0} hours\nPurging synthetic cellular toxicity and defragmenting neural pathways.";
            }
            else
            {
                int remainingTicks = Mathf.Max(0, MaxOperatingTicks - operatingTicks);
                float days = (float)remainingTicks / 60000f;
                string tip = $"Construct operating margin\nRemaining: {days:F1} / 30.0 days\nMust enter stasis (36h in pod, 48h in bed) before reaching 0 to avoid emergency shutdown.";
                if (days <= 2.0f)
                {
                    tip += "\n\nCRITICAL: Emergency shutdown imminent!";
                }
                return tip;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (pawn.Faction == Faction.OfPlayer)
            {
                if (Find.Selector != null && Find.Selector.SelectedPawns.Count == 1)
                {
                    yield return new Gizmo_ConstructStasisStatus(this);
                }

                if (inStasis)
                {
                    Command_Action wakeAction = new Command_Action
                    {
                        defaultLabel = "Wake",
                        defaultDesc = "Wake this construct from stasis.",
                        icon = WakeCommandTex.Texture,
                        action = delegate
                        {
                            int targetTicks = GetTargetStasisTicks();
                            if (stasisTicks < MinLockoutTicks)
                            {
                                float lockHours = (float)(MinLockoutTicks - stasisTicks) / 2500f;
                                Messages.Message($"Cannot wake {pawn.NameShortColored}: initial stasis synchronization requires a minimum 6-hour commitment ({lockHours:F1}h remaining).", pawn, MessageTypeDefOf.RejectInput);
                                return;
                            }

                            if (stasisTicks < targetTicks)
                            {
                                int remaining = targetTicks - stasisTicks;
                                float remHours = remaining / 2500f;
                                bool inPod = IsInPoweredStasisPod();
                                string penaltyDesc = inPod
                                    ? "buffered early wake (1-day -10% cognitive deficit, operating margin capped at 15 days)"
                                    : "neural defragmentation failure (3-day -20% cognitive lag, operating margin capped at 15 days)";

                                Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation(
                                    $"Warning: {pawn.NameShortColored} has not completed the required stasis duration ({remHours:F1} hours remaining). Waking them early will cause {penaltyDesc}.\n\nWake them anyway?",
                                    delegate
                                    {
                                        Wake();
                                    },
                                    destructive: true
                                );
                                Find.WindowStack.Add(window);
                            }
                            else
                            {
                                Wake();
                            }
                        }
                    };

                    if (stasisTicks < MinLockoutTicks)
                    {
                        float lockHours = (float)(MinLockoutTicks - stasisTicks) / 2500f;
                        wakeAction.Disable($"Cryo-synchronization lockout in progress ({lockHours:F1}h remaining).");
                    }

                    yield return wakeAction;

                    Command_Toggle autoWakeToggle = new Command_Toggle
                    {
                        defaultLabel = "Auto-wake",
                        defaultDesc = "Automatically wake this construct once the minimum required stasis duration has elapsed.",
                        icon = AutoWakeCommandTex.Texture,
                        isActive = () => autoWake,
                        toggleAction = delegate
                        {
                            autoWake = !autoWake;
                            if (autoWake && inStasis)
                            {
                                UpdateStasisTicks();
                                if (stasisTicks >= GetTargetStasisTicks())
                                {
                                    Wake();
                                }
                            }
                        }
                    };
                    yield return autoWakeToggle;
                }
                else
                {
                    if (pawn.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis)
                    {
                        Command_Action cancelStasis = new Command_Action
                        {
                            defaultLabel = "Cancel stasis",
                            defaultDesc = "Cancel heading to stasis and resume normal activities.",
                            icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true),
                            action = delegate
                            {
                                if (pawn.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis && !inStasis)
                                {
                                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                                }
                            }
                        };
                        yield return cancelStasis;
                    }
                }
            }
        }
    }

    public class JobDriver_ConstructStasis : JobDriver
    {
        public Building_Bed Bed => job.GetTarget(TargetIndex.A).Thing as Building_Bed;

        private Gene_ConstructHibernation HibernationGene => pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;

        public override bool PlayerInterruptable => !OnLastToil;

        public override bool CanBeginNowWhileLyingDown()
        {
            return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (Bed != null)
            {
                return pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed);
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (!job.targetA.IsValid)
            {
                Building_Bed bed = RestUtility.FindBedFor(pawn);
                if (bed != null)
                {
                    job.SetTarget(TargetIndex.A, bed);
                }
                else
                {
                    job.SetTarget(TargetIndex.A, pawn.Position);
                }
            }

            if (Bed != null)
            {
                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
                yield return Toils_Bed.GotoBed(TargetIndex.A);
            }
            else
            {
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            }

            Toil layDown = Toils_LayDown.LayDown(TargetIndex.A, Bed != null, lookForOtherJobs: false, canSleep: true, gainRestAndHealth: true, PawnPosture.LayingOnGroundFaceUp, deathrest: true);
            layDown.AddPreInitAction(delegate
            {
                job.forceSleep = true;
                asleep = true;
                var gene = HibernationGene;
                if (gene != null)
                {
                    gene.inStasis = true;
                    if (gene.stasisStartTick < 0)
                    {
                        gene.stasisStartTick = Find.TickManager.TicksGame;
                        gene.stasisTicks = 0;
                        gene.notifiedWakeOK = false;
                    }
                }
            });
            layDown.initAction = (System.Action)System.Delegate.Combine(layDown.initAction, (System.Action)delegate
            {
                if (pawn.Drafted)
                {
                    pawn.drafter.Drafted = false;
                }
                if (!pawn.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis))
                {
                    pawn.health.AddHediff(ConstructDefOf.Construct_InStasis);
                }
                PortraitsCache.SetDirty(pawn);
                GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
            });
            layDown.AddPreTickAction(delegate
            {
                asleep = true;
                job.forceSleep = true;
            });
            // Note: NO AddFinishAction calling ApplyWakeEffects!
            // Temporary interruptions (such as doctor tending or surgical operations) must not terminate stasis.
            // Stasis only ends via Gene_ConstructHibernation.Wake() (auto-wake or player command).

            yield return layDown;
        }

        public override string GetReport()
        {
            var gene = HibernationGene;
            if (gene != null && gene.inStasis)
            {
                gene.UpdateStasisTicks();
                if (gene.stasisTicks >= Gene_ConstructHibernation.MinStasisTicks)
                {
                    return "in stasis (complete, ready to wake).";
                }
                int remaining = Mathf.Max(0, Gene_ConstructHibernation.MinStasisTicks - gene.stasisTicks);
                return $"in stasis ({(remaining / 2500f):F1}h remaining).";
            }
            return "in stasis.";
        }
    }
}
