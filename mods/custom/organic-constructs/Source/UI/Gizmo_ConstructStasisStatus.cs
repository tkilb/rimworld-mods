using RimWorld;
using UnityEngine;
using Verse;

namespace OrganicConstructs
{
    [StaticConstructorOnStartup]
    public class Gizmo_ConstructStasisStatus : Gizmo
    {
        public Gene_ConstructHibernation gene;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.55f, 0.85f));
        private static readonly Texture2D CriticalBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.85f, 0.25f, 0.2f));
        private static readonly Texture2D StasisBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.6f, 0.35f, 0.85f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.08f, 0.08f, 0.08f));

        public Gizmo_ConstructStasisStatus(Gene_ConstructHibernation gene)
        {
            this.gene = gene;
            this.Order = -100f;
        }

        public override float GetWidth(float maxWidth) => 140f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(overRect);
            Rect rect = overRect.ContractedBy(6f);

            Text.Font = GameFont.Tiny;
            if (gene.inStasis)
            {
                float hours = (float)gene.stasisTicks / 2500f;
                float totalHours = (float)Gene_ConstructHibernation.MinStasisTicks / 2500f;
                float pct = Mathf.Clamp01((float)gene.stasisTicks / Gene_ConstructHibernation.MinStasisTicks);

                Rect labelRect = new Rect(rect.x, rect.y, rect.width, 18f);
                Widgets.Label(labelRect, "Stasis Cycle");

                Rect barRect = new Rect(rect.x, rect.y + 20f, rect.width, 22f);
                Widgets.FillableBar(barRect, pct, StasisBarTex, EmptyBarTex, true);

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(barRect, $"{hours:F1} / {totalHours:F0}h ({pct:P0})");
                Text.Anchor = TextAnchor.UpperLeft;

                Rect descRect = new Rect(rect.x, rect.y + 44f, rect.width, 18f);
                Widgets.Label(descRect, "Defragmenting...");
            }
            else
            {
                int remainingTicks = Mathf.Max(0, Gene_ConstructHibernation.MaxOperatingTicks - gene.operatingTicks);
                float days = (float)remainingTicks / 60000f;
                float pct = Mathf.Clamp01(1f - ((float)gene.operatingTicks / Gene_ConstructHibernation.MaxOperatingTicks));

                Texture2D barTex = (days <= 2.0f) ? CriticalBarTex : FullBarTex;

                Rect labelRect = new Rect(rect.x, rect.y, rect.width, 18f);
                Widgets.Label(labelRect, days <= 2.0f ? "STASIS CRITICAL" : "Operating Margin");

                Rect barRect = new Rect(rect.x, rect.y + 20f, rect.width, 22f);
                Widgets.FillableBar(barRect, pct, barTex, EmptyBarTex, true);

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(barRect, $"{days:F1} / 30.0d");
                Text.Anchor = TextAnchor.UpperLeft;

                Rect descRect = new Rect(rect.x, rect.y + 44f, rect.width, 18f);
                Widgets.Label(descRect, $"{pct:P0} capacity");
            }
            Text.Font = GameFont.Small;

            TooltipHandler.TipRegion(overRect, gene.GetStasisTooltip());

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
