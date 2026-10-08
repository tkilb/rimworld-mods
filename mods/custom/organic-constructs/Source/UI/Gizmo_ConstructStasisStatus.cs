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
            Rect inRect = overRect.ContractedBy(6f);

            TextAnchor prevAnchor = Text.Anchor;
            GameFont prevFont = Text.Font;
            bool prevWrap = Text.WordWrap;

            try
            {
                Text.Font = GameFont.Tiny;
                Text.WordWrap = false;

                Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 20f);
                Rect barRect = new Rect(inRect.x, inRect.y + 24f, inRect.width, 32f);

                if (gene.inStasis)
                {
                    gene.UpdateStasisTicks();
                    float hours = (float)gene.stasisTicks / 2500f;
                    float totalHours = (float)Gene_ConstructHibernation.MinStasisTicks / 2500f;
                    float pct = Mathf.Clamp01((float)gene.stasisTicks / Gene_ConstructHibernation.MinStasisTicks);

                    Text.Anchor = TextAnchor.UpperLeft;
                    Widgets.Label(titleRect, gene.stasisTicks >= Gene_ConstructHibernation.MinStasisTicks ? "Stasis complete" : "Stasis hibernation");

                    Widgets.FillableBar(barRect, pct, StasisBarTex, EmptyBarTex, doBorder: false);

                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(barRect, gene.stasisTicks >= Gene_ConstructHibernation.MinStasisTicks ? $"Ready ({hours:F1}h)" : $"{hours:F1} / {totalHours:F0}h");
                }
                else
                {
                    int remainingTicks = Mathf.Max(0, Gene_ConstructHibernation.MaxOperatingTicks - gene.operatingTicks);
                    float days = (float)remainingTicks / 60000f;
                    float pct = Mathf.Clamp01(1f - ((float)gene.operatingTicks / Gene_ConstructHibernation.MaxOperatingTicks));

                    Texture2D barTex = (days <= 2.0f) ? CriticalBarTex : FullBarTex;

                    Text.Anchor = TextAnchor.UpperLeft;
                    Widgets.Label(titleRect, days <= 2.0f ? "Stasis critical" : "Operating margin");

                    Widgets.FillableBar(barRect, pct, barTex, EmptyBarTex, doBorder: false);

                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(barRect, $"{days:F1} / 30.0d");
                }
            }
            finally
            {
                Text.Anchor = prevAnchor;
                Text.Font = prevFont;
                Text.WordWrap = prevWrap;
            }

            TooltipHandler.TipRegion(overRect, gene.GetStasisTooltip());

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
