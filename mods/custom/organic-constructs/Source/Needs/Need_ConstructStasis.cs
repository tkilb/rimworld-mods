using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace OrganicConstructs
{
    public class Need_ConstructStasis : Need
    {
        private Gene_ConstructHibernation cachedHibernationGene;

        public Gene_ConstructHibernation HibernationGene
        {
            get
            {
                if (cachedHibernationGene == null && pawn?.genes != null)
                {
                    cachedHibernationGene = pawn.genes.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation
                        ?? pawn.genes.GenesListForReading.OfType<Gene_ConstructHibernation>().FirstOrDefault();
                }
                return cachedHibernationGene;
            }
        }

        public Need_ConstructStasis(Pawn pawn) : base(pawn)
        {
            threshPercents = new List<float> { 0.1f, 0.25f, 0.5f };
        }

        public override void SetInitialLevel()
        {
            var gene = HibernationGene;
            if (gene != null)
            {
                gene.UpdateStasisTicks();
                if (gene.inStasis)
                {
                    CurLevel = Mathf.Clamp01((float)gene.stasisTicks / Gene_ConstructHibernation.MinStasisTicks);
                }
                else
                {
                    CurLevel = Mathf.Clamp01(1f - ((float)gene.operatingTicks / Gene_ConstructHibernation.MaxOperatingTicks));
                }
            }
            else
            {
                CurLevel = 1f;
            }
        }

        public override void NeedInterval()
        {
            var gene = HibernationGene;
            if (gene == null) return;

            gene.UpdateStasisTicks();
            if (gene.inStasis)
            {
                CurLevel = Mathf.Clamp01((float)gene.stasisTicks / Gene_ConstructHibernation.MinStasisTicks);
            }
            else
            {
                CurLevel = Mathf.Clamp01(1f - ((float)gene.operatingTicks / Gene_ConstructHibernation.MaxOperatingTicks));
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                var gene = HibernationGene;
                if (gene != null && gene.inStasis)
                {
                    return gene.stasisTicks >= Gene_ConstructHibernation.MinStasisTicks ? 0 : 1;
                }
                return -1; // Falling arrow during active operation
            }
        }

        public override string GetTipString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(base.GetTipString());
            sb.AppendLine();

            var gene = HibernationGene;
            if (gene != null)
            {
                if (gene.inStasis)
                {
                    float hoursPassed = (float)gene.stasisTicks / 2500f;
                    float totalHours = (float)Gene_ConstructHibernation.MinStasisTicks / 2500f;
                    if (gene.stasisTicks >= Gene_ConstructHibernation.MinStasisTicks)
                    {
                        sb.AppendLine($"Hibernation stasis complete: {hoursPassed:F1} hours elapsed.");
                        sb.AppendLine("Safe to wake at any time. Construct will remain in stasis until woken.");
                    }
                    else
                    {
                        sb.AppendLine($"Currently in hibernation stasis: {hoursPassed:F1} / {totalHours:F0} hours.");
                        sb.AppendLine("Purging metabolic toxicity and defragmenting neural pathways.");
                    }
                }
                else
                {
                    int remainingTicks = Mathf.Max(0, Gene_ConstructHibernation.MaxOperatingTicks - gene.operatingTicks);
                    float daysRemaining = (float)remainingTicks / 60000f;
                    sb.AppendLine($"Operating margin: {daysRemaining:F1} / 30.0 days remaining.");
                    if (daysRemaining <= 2.0f)
                    {
                        sb.AppendLine("CRITICAL: Stasis required immediately to prevent emergency shutdown.");
                    }
                    else
                    {
                        sb.AppendLine("Must hibernate for 48 hours before reaching 0 to prevent emergency shutdown.");
                    }
                }
            }

            return sb.ToString().TrimEndNewlines();
        }
    }
}
