using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    public class Recipe_InstallConstructPackage : Recipe_InstallImplant
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return base.AvailableOnNow(thing, part);
        }

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            if (thing is Pawn pawn)
            {
                if (!ConstructUtility.IsConstruct(pawn))
                {
                    return new AcceptanceReport("Cannot graft: Incompatible biological neural bus (Requires Construct).");
                }
                if (recipe.addsHediff != null && pawn.health.hediffSet.HasHediff(recipe.addsHediff, part))
                {
                    return new AcceptanceReport("Already has this bioware package installed.");
                }
            }

            return base.AvailableReport(thing, part);
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            bool isConstruct = ConstructUtility.IsConstruct(pawn);

            if (!isConstruct)
            {
                Messages.Message("Cannot graft: Incompatible biological neural bus (Requires Construct).", pawn, MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    HandleFailure(pawn, billDoer, recipe);
                    return;
                }

                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }

            // Extract and eject any existing bioware augment(s) to maintain single-package neural bus limit
            List<Hediff> existingBiowares = pawn.health.hediffSet.hediffs
                .Where(h => h.def != null && BiowareSkillUtility.IsBiowareAugment(h.def))
                .ToList();

            foreach (Hediff existing in existingBiowares)
            {
                ThingDef itemDef = existing.def.spawnThingOnRemoved;
                if (itemDef != null)
                {
                    Map map = pawn.Map ?? billDoer?.Map;
                    IntVec3 pos = pawn.PositionHeld;
                    if (map != null && pos.IsValid)
                    {
                        Thing ejected = ThingMaker.MakeThing(itemDef);
                        GenPlace.TryPlaceThing(ejected, pos, map, ThingPlaceMode.Near);
                    }
                }

                pawn.health.RemoveHediff(existing);
                Messages.Message($"Extracted previous bioware package ({existing.Label}) from {pawn.NameShortColored} to accommodate new package.", pawn, MessageTypeDefOf.NeutralEvent, false);
            }

            // Success - graft new bioware
            pawn.health.AddHediff(recipe.addsHediff, part, null, null);

            // Add Neural Assimilation (Success - 5000 ticks)
            Hediff assimilation = HediffMaker.MakeHediff(ConstructAugmentDefOf.Construct_Assimilation, pawn, null);
            HediffComp_Disappears disappears = assimilation.TryGetComp<HediffComp_Disappears>();
            if (disappears != null)
            {
                disappears.ticksToDisappear = 5000;
            }
            pawn.health.AddHediff(assimilation, null, null, null);

            Messages.Message("Surgery successful.", pawn, MessageTypeDefOf.PositiveEvent, false);
        }

        private void HandleFailure(Pawn pawn, Pawn billDoer, RecipeDef recipe)
        {
            // Neural Assimilation (Failure - 10000 ticks)
            Hediff assimilation = HediffMaker.MakeHediff(ConstructAugmentDefOf.Construct_Assimilation, pawn, null);
            HediffComp_Disappears disappears = assimilation.TryGetComp<HediffComp_Disappears>();
            if (disappears != null)
            {
                disappears.ticksToDisappear = 10000;
            }
            pawn.health.AddHediff(assimilation, null, null, null);

            // Salvage calculation
            int doctorSkill = billDoer.skills?.GetSkill(SkillDefOf.Medicine)?.Level ?? 0;
            float salvageChance = 0.20f + (doctorSkill * 0.05f);

            if (Rand.Value < salvageChance)
            {
                ThingDef itemDef = recipe.ingredients.FirstOrDefault(x => x.IsFixedIngredient)?.FixedIngredient;
                if (itemDef == null && recipe.addsHediff?.spawnThingOnRemoved != null)
                {
                    itemDef = recipe.addsHediff.spawnThingOnRemoved;
                }

                if (itemDef != null)
                {
                    Map map = pawn.Map ?? billDoer.Map;
                    IntVec3 pos = pawn.PositionHeld;
                    if (map != null && pos.IsValid)
                    {
                        Thing salvagedItem = ThingMaker.MakeThing(itemDef);
                        GenPlace.TryPlaceThing(salvagedItem, pos, map, ThingPlaceMode.Near);
                        Messages.Message("Surgery failed, but the bioware augment was salvaged.", pawn, MessageTypeDefOf.NeutralEvent, false);
                    }
                }
                else
                {
                    Messages.Message("Surgery failed.", pawn, MessageTypeDefOf.NegativeEvent, false);
                }
            }
            else
            {
                Messages.Message("Surgery failed and the bioware augment was ruined.", pawn, MessageTypeDefOf.NegativeEvent, false);
            }
        }
    }
}
