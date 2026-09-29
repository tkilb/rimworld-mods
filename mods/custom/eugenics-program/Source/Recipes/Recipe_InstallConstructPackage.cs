using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace EugenicsProgram
{
    public class Recipe_InstallConstructPackage : Recipe_Surgery
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (!base.AvailableOnNow(thing, part))
                return false;

            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                bool hasTrait = pawn.story?.traits?.HasTrait(ConstructAugmentDefOf.Trait_ConstructAsset) ?? false;
                bool hasGene = pawn.genes?.HasActiveGene(ConstructAugmentDefOf.Gene_ConstructPsychology) ?? false;
                
                if (!hasTrait && !hasGene)
                    return false;
            }

            return true;
        }

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                bool hasTrait = pawn.story?.traits?.HasTrait(ConstructAugmentDefOf.Trait_ConstructAsset) ?? false;
                bool hasGene = pawn.genes?.HasActiveGene(ConstructAugmentDefOf.Gene_ConstructPsychology) ?? false;
                
                if (!hasTrait && !hasGene)
                {
                    return new AcceptanceReport("Cannot graft: Incompatible biological neural bus (Requires Construct).");
                }
            }

            return base.AvailableReport(thing, part);
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            bool isConstruct = false;
            if (pawn.story?.traits?.HasTrait(ConstructAugmentDefOf.Trait_ConstructAsset) == true ||
                pawn.genes?.HasActiveGene(ConstructAugmentDefOf.Gene_ConstructPsychology) == true)
            {
                isConstruct = true;
            }

            if (!isConstruct)
            {
                Messages.Message("Cannot graft: Incompatible biological neural bus (Requires Construct).", pawn, MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    // Surgery failed
                    HandleFailure(pawn, billDoer, recipe);
                    return;
                }

                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }

            // Success
            pawn.health.AddHediff(recipe.addsHediff, part, null, null);
            
            // Add Neural Assimilation (Success - 5000 ticks)
            Hediff assimilation = HediffMaker.MakeHediff(ConstructAugmentDefOf.Eugenics_ConstructAssimilation, pawn, null);
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
            Hediff assimilation = HediffMaker.MakeHediff(ConstructAugmentDefOf.Eugenics_ConstructAssimilation, pawn, null);
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
                    Thing salvagedItem = ThingMaker.MakeThing(itemDef);
                    GenPlace.TryPlaceThing(salvagedItem, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    Messages.Message($"Surgery failed, but the bio-augment package was salvaged.", pawn, MessageTypeDefOf.NeutralEvent, false);
                }
                else
                {
                    Messages.Message($"Surgery failed.", pawn, MessageTypeDefOf.NegativeEvent, false);
                }
            }
            else
            {
                Messages.Message($"Surgery failed and the bio-augment package was ruined.", pawn, MessageTypeDefOf.NegativeEvent, false);
            }
        }
    }
}
