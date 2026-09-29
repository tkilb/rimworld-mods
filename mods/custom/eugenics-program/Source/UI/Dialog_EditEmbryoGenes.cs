using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EugenicsProgram
{
    /// <summary>
    /// Custom IMGUI dialog for editing the endogenes on a HumanEmbryo.
    ///
    /// Rules enforced:
    ///   - Operation Limit: Maximum of 2 gene edits per embryo.
    ///   - Gene ADDITION: any GeneDef available in the connected GeneBanks.
    ///   - Gene REMOVAL : a gene may only be removed if its exact GeneDef is currently
    ///     stored in one of the connected GeneBanks (acts as a molecular template).
    ///   - "Burn Disc"  : consume a GenomeBlueprintDisk from the map and apply its
    ///     stored gene list to the embryo's endogenes (overwrites all genes, counts as 1 edit).
    ///   - Splicing Failure: rolls for botch; on failure embryo collapses into 1x GeneticNutrientPaste.
    /// </summary>
    public class Dialog_EditEmbryoGenes : Window
    {
        // ── Layout constants ───────────────────────────────────────────────────────
        private const float RowHeight     = 28f;
        private const float PanelPadding  = 8f;
        private const float ScrollbarW    = 16f;
        private const float FooterHeight  = 46f;

        // ── State ──────────────────────────────────────────────────────────────────
        private readonly HumanEmbryo          embryo;
        private readonly Building_GeneAssembler assembler;

        /// Live-refreshed gene list from the embryo's GeneSet.
        private List<GeneDef> embryoGenes  = new List<GeneDef>();
        /// All genes present in the connected Gene Banks.
        private HashSet<GeneDef> bankGenes = new HashSet<GeneDef>();
        /// Genes in banks not yet on the embryo.
        private List<GeneDef> availableToAdd = new List<GeneDef>();

        private Vector2 scrollCurrent;
        private Vector2 scrollAdd;
        private Vector2 scrollDiscs;

        private string searchFilter = "";
        private int selectedTab;   // 0=Current, 1=Add, 2=Burn Disc

        // ── Constructor ────────────────────────────────────────────────────────────
        public Dialog_EditEmbryoGenes(Thing embryoThing, Building assemblerBuilding)
        {
            embryo    = (HumanEmbryo)embryoThing;
            assembler = (Building_GeneAssembler)assemblerBuilding;

            doCloseX              = true;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            forcePause            = true;

            RefreshLists();
        }

        public override Vector2 InitialSize => new Vector2(860f, 640f);

        // ── Refresh helpers ────────────────────────────────────────────────────────
        private void RefreshLists()
        {
            embryoGenes = embryo.GeneSet?.GenesListForReading?.ToList() ?? new List<GeneDef>();

            bankGenes.Clear();
            // Building_GeneAssembler.GetGenepacks() returns all Genepack items in linked Gene Banks.
            if (assembler != null)
            {
                List<Genepack> packs = assembler.GetGenepacks(includePowered: true, includeUnpowered: true);
                foreach (Genepack pack in packs)
                {
                    if (pack?.GeneSet?.GenesListForReading == null) continue;
                    foreach (GeneDef g in pack.GeneSet.GenesListForReading)
                        bankGenes.Add(g);
                }
            }

            availableToAdd = DefDatabase<GeneDef>.AllDefsListForReading
                .Where(g => bankGenes.Contains(g) && !embryoGenes.Contains(g))
                .OrderBy(g => g.label)
                .ToList();
        }

        // ── DoWindowContents ───────────────────────────────────────────────────────
        public override void DoWindowContents(Rect inRect)
        {
            // Title
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 36f);
            Widgets.Label(titleRect, "Embryonic Gene Splicing Interface");
            Text.Font = GameFont.Small;

            // Sub-label
            Rect subRect = new Rect(inRect.x, titleRect.yMax + 2f, inRect.width, 20f);
            GUI.color = Color.gray;
            Widgets.Label(subRect, BuildEmbryoSummary());
            GUI.color = Color.white;

            float contentY = subRect.yMax + PanelPadding;

            // Tab bar
            Rect tabRect = new Rect(inRect.x, contentY, inRect.width, 30f);
            DrawTabBar(tabRect);
            contentY = tabRect.yMax + PanelPadding;

            // Body
            Rect bodyRect = new Rect(inRect.x, contentY, inRect.width, inRect.yMax - contentY - FooterHeight - PanelPadding);

            switch (selectedTab)
            {
                case 0: DrawCurrentGenesPanel(bodyRect); break;
                case 1: DrawAddGenesPanel(bodyRect);     break;
                case 2: DrawBurnDiscPanel(bodyRect);     break;
            }

            // Footer
            DrawFooter(new Rect(inRect.x, inRect.yMax - FooterHeight, inRect.width, FooterHeight));
        }

        // ── Tab bar ────────────────────────────────────────────────────────────────
        private void DrawTabBar(Rect rect)
        {
            float w = rect.width / 3f;
            string[] tabs = { $"Current Genes ({embryoGenes.Count})", "Add Gene", "Burn Blueprint Disc" };
            for (int i = 0; i < tabs.Length; i++)
            {
                Rect tab = new Rect(rect.x + i * w, rect.y, w, rect.height);
                if (selectedTab == i) Widgets.DrawHighlight(tab);
                else                  Widgets.DrawLightHighlight(tab);
                if (Widgets.ButtonText(tab, tabs[i], drawBackground: false))
                    selectedTab = i;
                if (i < tabs.Length - 1)
                    Widgets.DrawLineVertical(tab.xMax, rect.y, rect.height);
            }
        }

        // ── Panel: current genes ───────────────────────────────────────────────────
        private void DrawCurrentGenesPanel(Rect rect)
        {
            Rect viewRect = new Rect(0, 0, rect.width - ScrollbarW, embryoGenes.Count * RowHeight + 4f);
            Widgets.BeginScrollView(rect, ref scrollCurrent, viewRect);

            float y = 0f;
            if (embryoGenes.Count == 0)
            {
                Widgets.Label(new Rect(0, y, viewRect.width, RowHeight), "(No genes on this embryo)");
            }
            else
            {
                foreach (GeneDef gene in embryoGenes.ToList())
                {
                    DrawGeneRow(gene, new Rect(0, y, viewRect.width, RowHeight), isRemoveMode: true);
                    y += RowHeight;
                }
            }
            Widgets.EndScrollView();
        }

        // ── Panel: add genes ───────────────────────────────────────────────────────
        private void DrawAddGenesPanel(Rect rect)
        {
            Rect searchRect  = new Rect(rect.x, rect.y, rect.width - 120f, 28f);
            Rect clearRect   = new Rect(searchRect.xMax + 4f, rect.y, 112f, 28f);
            searchFilter = Widgets.TextField(searchRect, searchFilter);
            if (Widgets.ButtonText(clearRect, "Clear filter")) searchFilter = "";

            Rect listRect = new Rect(rect.x, rect.y + 32f, rect.width, rect.height - 32f);

            List<GeneDef> filtered = string.IsNullOrWhiteSpace(searchFilter)
                ? availableToAdd
                : availableToAdd.Where(g =>
                    g.label.IndexOf(searchFilter, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    g.defName.IndexOf(searchFilter, System.StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            Rect viewRect = new Rect(0, 0, listRect.width - ScrollbarW, filtered.Count * RowHeight + 4f);
            Widgets.BeginScrollView(listRect, ref scrollAdd, viewRect);

            float y = 0f;
            if (filtered.Count == 0)
            {
                string msg = bankGenes.Count == 0
                    ? "(No Gene Banks connected or no genepacks loaded)"
                    : "(No matching genes in connected Gene Banks)";
                Widgets.Label(new Rect(0, y, viewRect.width, RowHeight), msg);
            }
            else
            {
                foreach (GeneDef gene in filtered)
                {
                    DrawGeneRow(gene, new Rect(0, y, viewRect.width, RowHeight), isRemoveMode: false);
                    y += RowHeight;
                }
            }
            Widgets.EndScrollView();
        }

        // ── Panel: burn blueprint disc ─────────────────────────────────────────────
        private void DrawBurnDiscPanel(Rect rect)
        {
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            bool maxEditsReached = quality != null && quality.editCount >= 2;

            // Warning header
            Rect hdr = new Rect(rect.x, rect.y, rect.width, 36f);
            GUI.color = new Color(1f, 0.85f, 0.3f);
            Widgets.Label(hdr, "⚠  Burning a disc OVERWRITES all current embryo endogenes, counts as 1 edit, and DESTROYS the disc.");
            GUI.color = Color.white;

            Rect listRect = new Rect(rect.x, rect.y + 40f, rect.width, rect.height - 40f);
            List<Thing> discs = FindAvailableDiscs();

            float rowH = RowHeight * 2 + 6f;
            Rect viewRect = new Rect(0, 0, listRect.width - ScrollbarW, discs.Count * rowH + 4f);
            Widgets.BeginScrollView(listRect, ref scrollDiscs, viewRect);

            float y = 0f;
            if (discs.Count == 0)
            {
                Widgets.Label(new Rect(0, y, viewRect.width, RowHeight), "(No Genome Blueprint Discs found on the map)");
            }
            else
            {
                foreach (Thing disc in discs)
                {
                    CompGenomeBlueprint comp = disc.TryGetComp<CompGenomeBlueprint>();
                    if (comp == null) continue;

                    Rect row = new Rect(0, y, viewRect.width, rowH);
                    Widgets.DrawBoxSolid(row, new Color(0.08f, 0.08f, 0.08f, 0.35f));
                    Widgets.DrawBox(row);

                    // Icon
                    GUI.DrawTexture(new Rect(row.x + 4f, row.y + 4f, 40f, 40f), disc.def.uiIcon);

                    // Title and stats
                    string title = string.IsNullOrEmpty(comp.templateLabel) ? disc.Label : comp.templateLabel;
                    Widgets.Label(new Rect(row.x + 50f, row.y + 4f, row.width - 150f, RowHeight), title);
                    GUI.color = Color.gray;
                    Widgets.Label(new Rect(row.x + 50f, row.y + 4f + RowHeight, row.width - 150f, RowHeight),
                        $"{comp.genes.Count} genes  |  Cpx {comp.ComplexityTotal}  |  Net Met {(comp.MetabolicTotal >= 0 ? "+" : "")}{comp.MetabolicTotal}");
                    GUI.color = Color.white;

                    // Tooltip
                    TooltipHandler.TipRegion(row, BuildDiscTooltip(comp));

                    // Burn button
                    Rect btnRect = new Rect(row.xMax - 90f, row.y + (rowH - 28f) / 2f, 86f, 28f);
                    if (comp.genes.Count == 0)
                    {
                        GUI.color = Color.gray;
                        Widgets.Label(btnRect, "(Empty)");
                        GUI.color = Color.white;
                    }
                    else if (maxEditsReached)
                    {
                        GUI.color = Color.gray;
                        Widgets.Label(btnRect, "(Max edits)");
                        GUI.color = Color.white;
                        TooltipHandler.TipRegion(row, "Embryo has already reached the maximum allowed genetic edits (2).");
                    }
                    else if (Widgets.ButtonText(btnRect, "Burn Disc"))
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            $"Burn '{title}' onto this embryo? This will overwrite all existing genes, count as 1 edit, and destroy the disc.",
                            () => BurnDisc(disc, comp)));
                    }

                    y += rowH + 4f;
                }
            }
            Widgets.EndScrollView();
        }

        // ── Shared gene row ────────────────────────────────────────────────────────
        private void DrawGeneRow(GeneDef gene, Rect row, bool isRemoveMode)
        {
            if (Mouse.IsOver(row)) Widgets.DrawHighlight(row);

            // Icon
            if (gene.Icon != null)
                GUI.DrawTexture(new Rect(row.x + 2f, row.y + 2f, RowHeight - 4f, RowHeight - 4f), gene.Icon);

            // Label
            Widgets.Label(new Rect(row.x + RowHeight + 2f, row.y, 260f, RowHeight), gene.label.CapitalizeFirst());

            // Stats
            GUI.color = Color.gray;
            Widgets.Label(new Rect(row.x + RowHeight + 268f, row.y, 130f, RowHeight),
                $"Cpx {(gene.biostatCpx >= 0 ? "+" : "")}{gene.biostatCpx}  Met {(gene.biostatMet >= 0 ? "+" : "")}{gene.biostatMet}");
            GUI.color = Color.white;

            // Action button
            Rect btnRect = new Rect(row.xMax - 80f, row.y + 3f, 76f, RowHeight - 6f);
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            bool maxEditsReached = quality != null && quality.editCount >= 2;

            if (isRemoveMode)
            {
                bool canRemove = bankGenes.Contains(gene);
                if (maxEditsReached)
                {
                    GUI.color = Color.gray;
                    Widgets.Label(btnRect, "(max edits)");
                    GUI.color = Color.white;
                    TooltipHandler.TipRegion(row, "Embryo has reached the maximum allowed genetic edits (2).");
                }
                else if (canRemove)
                {
                    if (Widgets.ButtonText(btnRect, "Remove"))
                        TryRemoveGene(gene);
                }
                else
                {
                    GUI.color = Color.gray;
                    Widgets.Label(btnRect, "(no template)");
                    GUI.color = Color.white;
                    TooltipHandler.TipRegion(row, "Gene removal requires a matching Genepack in a connected Gene Bank as a molecular template.");
                }
            }
            else
            {
                if (maxEditsReached)
                {
                    GUI.color = Color.gray;
                    Widgets.Label(btnRect, "(max edits)");
                    GUI.color = Color.white;
                    TooltipHandler.TipRegion(row, "Embryo has reached the maximum allowed genetic edits (2).");
                }
                else if (Widgets.ButtonText(btnRect, "Add"))
                {
                    TryAddGene(gene);
                }
            }

            // Description tooltip
            if (!string.IsNullOrEmpty(gene.description))
                TooltipHandler.TipRegion(new Rect(row.x, row.y, row.width - 84f, row.height), gene.description);
        }

        // ── Footer ─────────────────────────────────────────────────────────────────
        private void DrawFooter(Rect rect)
        {
            int cpx = embryoGenes.Sum(g => g?.biostatCpx ?? 0);
            int met = embryoGenes.Sum(g => g?.biostatMet ?? 0);
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            int edits = quality?.editCount ?? 0;
            Widgets.Label(new Rect(rect.x, rect.y + 10f, rect.width - 110f, rect.height),
                $"Genes: {embryoGenes.Count}   |   Edits: {edits}/2   |   Total Complexity: {cpx}   |   Net Metabolic Efficiency: {(met >= 0 ? "+" : "")}{met}");

            if (Widgets.ButtonText(new Rect(rect.xMax - 100f, rect.y + 6f, 96f, 34f), "Close"))
                Close();
        }

        // ── Gene mutation & failure roll ───────────────────────────────────────────

        private bool PreEditCheckAndRollFailure()
        {
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            if (quality != null && quality.editCount >= 2)
            {
                Messages.Message(
                    "Cannot edit embryo genetics: Maximum genetic edit operations (2) reached for this embryo.",
                    embryo,
                    MessageTypeDefOf.RejectInput);
                return false;
            }

            // Roll for failure/botch based on room cleanliness
            float roomCleanliness = assembler?.GetRoom()?.GetStat(RoomStatDefOf.Cleanliness) ?? 0f;
            float botchChance = Mathf.Clamp(0.08f - (roomCleanliness * 0.05f), 0.02f, 0.50f);

            if (Rand.Chance(botchChance))
            {
                // Splicing botch: Cellular structure collapses, spawn 1x Genetic Nutrient Paste
                Map map = embryo.MapHeld ?? assembler?.Map;
                IntVec3 pos = embryo.PositionHeld.IsValid ? embryo.PositionHeld : (assembler?.Position ?? IntVec3.Invalid);

                if (!embryo.Destroyed)
                {
                    embryo.Destroy(DestroyMode.Vanish);
                }

                if (map != null && pos.IsValid)
                {
                    Thing paste = ThingMaker.MakeThing(EugenicsDefOf.GeneticNutrientPaste);
                    paste.stackCount = 1;
                    GenPlace.TryPlaceThing(paste, pos, map, ThingPlaceMode.Near);
                }

                Messages.Message(
                    "Embryo gene splicing botched! Cellular structure collapsed due to genetic instability. 1x Genetic Nutrient Paste recovered.",
                    new TargetInfo(pos, map),
                    MessageTypeDefOf.NegativeEvent);

                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Crunch);
                Close();
                return false;
            }

            return true;
        }

        private void PostSuccessfulEdit(string message)
        {
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            if (quality != null)
            {
                quality.editCount++;
            }

            EugenicsParentalUtility.ApplyParentalThoughts(embryo);
            RefreshLists();

            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, embryo, MessageTypeDefOf.PositiveEvent);
            }
        }

        private void TryAddGene(GeneDef gene)
        {
            if (!PreEditCheckAndRollFailure()) return;

            embryo.GeneSet?.AddGene(gene);
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            int currentEdits = (quality?.editCount ?? 0) + 1;
            PostSuccessfulEdit($"Added gene '{gene.label}'. Edits: {currentEdits}/2.");
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High);
        }

        private void TryRemoveGene(GeneDef gene)
        {
            if (!PreEditCheckAndRollFailure()) return;

            embryo.GeneSet?.Debug_RemoveGene(gene);
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            int currentEdits = (quality?.editCount ?? 0) + 1;
            PostSuccessfulEdit($"Removed gene '{gene.label}'. Edits: {currentEdits}/2.");
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low);
        }

        private void BurnDisc(Thing disc, CompGenomeBlueprint comp)
        {
            if (!PreEditCheckAndRollFailure()) return;

            // Clear all current endogenes
            if (embryo.GeneSet != null)
            {
                foreach (GeneDef g in embryo.GeneSet.GenesListForReading.ToList())
                    embryo.GeneSet.Debug_RemoveGene(g);

                // Apply blueprint genes
                foreach (GeneDef g in comp.genes)
                    if (g != null) embryo.GeneSet.AddGene(g);
            }

            // Consume the disc
            disc.Destroy(DestroyMode.Vanish);

            PostSuccessfulEdit($"Genome blueprint '{comp.templateLabel ?? disc.Label}' burned onto embryo. Disc consumed.");
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.PsychicPulseGlobal);
        }

        // ── Utility ───────────────────────────────────────────────────────────────
        private List<Thing> FindAvailableDiscs()
        {
            if (assembler?.Map == null) return new List<Thing>();
            return assembler.Map.listerThings
                .ThingsOfDef(EugenicsDefOf.GenomeBlueprintDisk)
                .Where(d => d.Spawned && !d.IsForbidden(Faction.OfPlayer))
                .ToList();
        }

        private string BuildEmbryoSummary()
        {
            int cpx = embryoGenes.Sum(g => g?.biostatCpx ?? 0);
            int met = embryoGenes.Sum(g => g?.biostatMet ?? 0);
            CompEmbryoQuality quality = embryo.TryGetComp<CompEmbryoQuality>();
            string constructTag = (quality != null && quality.isConstruct) ? " [Construct Matrix]" : "";
            int edits = quality?.editCount ?? 0;
            return $"{embryo.LabelCap}{constructTag}  |  Genes: {embryoGenes.Count}  |  Edits: {edits}/2  |  Complexity: {cpx}  |  Net Met: {(met >= 0 ? "+" : "")}{met}";
        }

        private static string BuildDiscTooltip(CompGenomeBlueprint comp)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Blueprint: {comp.templateLabel ?? "Unnamed"}");
            sb.AppendLine($"Genes: {comp.genes.Count}  |  Complexity: {comp.ComplexityTotal}  |  Net Met: {(comp.MetabolicTotal >= 0 ? "+" : "")}{comp.MetabolicTotal}");
            sb.AppendLine();
            if (comp.genes.Count == 0)
            {
                sb.AppendLine("  (empty disc)");
            }
            else
            {
                foreach (GeneDef g in comp.genes)
                {
                    if (g == null) continue;
                    sb.AppendLine($"  • {g.label.CapitalizeFirst()} (Cpx {(g.biostatCpx >= 0 ? "+" : "")}{g.biostatCpx}, Met {(g.biostatMet >= 0 ? "+" : "")}{g.biostatMet})");
                }
            }
            return sb.ToString();
        }
    }
}
