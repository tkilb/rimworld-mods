using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GeneSplicer
{
    public class Dialog_EditEmbryoGenes : Window
    {
        private const float RowHeight = 30f;
        private const float PanelPadding = 8f;
        private const float FooterHeight = 50f;

        private readonly Building_EmbryoSplicingBench bench;
        private readonly HumanEmbryo embryo;

        private List<GeneDef> currentEmbryoGenes = new List<GeneDef>();
        private HashSet<GeneDef> bankGenes = new HashSet<GeneDef>();
        private List<GeneDef> availableToAdd = new List<GeneDef>();

        private List<GeneDef> tempStagedAdd = new List<GeneDef>();
        private List<GeneDef> tempStagedRemove = new List<GeneDef>();

        private Vector2 scrollCurrent;
        private Vector2 scrollAdd;
        private string searchFilter = "";
        private int selectedTab = 0; // 0=Current Genes, 1=Add from Bank

        public Dialog_EditEmbryoGenes(Building_EmbryoSplicingBench bench)
        {
            this.bench = bench;
            this.embryo = bench.ContainedEmbryo;

            doCloseX = true;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            forcePause = true;

            tempStagedAdd = new List<GeneDef>(bench.stagedGenesToAdd);
            tempStagedRemove = new List<GeneDef>(bench.stagedGenesToRemove);

            RefreshLists();
        }

        public override Vector2 InitialSize => new Vector2(860f, 620f);

        private void RefreshLists()
        {
            currentEmbryoGenes = embryo?.GeneSet?.GenesListForReading?.ToList() ?? new List<GeneDef>();
            bankGenes = bench.GetAvailableBankGenes();

            availableToAdd = DefDatabase<GeneDef>.AllDefsListForReading
                .Where(g => bankGenes.Contains(g) && !currentEmbryoGenes.Contains(g) && !tempStagedAdd.Contains(g))
                .OrderBy(g => g.label)
                .ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 36f);
            Widgets.Label(titleRect, "Embryonic Germline Splicing Console");
            Text.Font = GameFont.Small;

            Rect subRect = new Rect(inRect.x, titleRect.yMax, inRect.width, 22f);
            GUI.color = Color.gray;
            Widgets.Label(subRect, $"Embryo: {embryo?.LabelCap ?? "None"} | Linked Gene Banks: {bench.GetLinkedGenepacks(false).Count} genepacks available");
            GUI.color = Color.white;

            Rect contentRect = new Rect(inRect.x, subRect.yMax + 8f, inRect.width, inRect.height - subRect.yMax - FooterHeight - 16f);
            DrawTabsAndContent(contentRect);

            Rect footerRect = new Rect(inRect.x, inRect.height - FooterHeight, inRect.width, FooterHeight);
            DrawFooter(footerRect);
        }

        private void DrawTabsAndContent(Rect rect)
        {
            List<TabRecord> tabs = new List<TabRecord>
            {
                new TabRecord("Current Embryo Genes", () => { selectedTab = 0; }, selectedTab == 0),
                new TabRecord("Add From Gene Banks", () => { selectedTab = 1; }, selectedTab == 1)
            };
            TabDrawer.DrawTabs(rect, tabs);

            Rect inner = rect.ContractedBy(PanelPadding);
            inner.yMin += 4f;

            if (selectedTab == 0)
            {
                DrawCurrentGenesTab(inner);
            }
            else
            {
                DrawAddGenesTab(inner);
            }
        }

        private void DrawCurrentGenesTab(Rect rect)
        {
            CompEmbryoQuality quality = embryo?.TryGetComp<CompEmbryoQuality>();
            int currentEdits = quality?.editCount ?? 0;
            int totalEditsAfter = currentEdits + tempStagedAdd.Count + tempStagedRemove.Count;

            Rect infoRect = new Rect(rect.x, rect.y, rect.width, 28f);
            Widgets.Label(infoRect, "Gene removal requires the exact gene pack to be stored in a connected Gene Bank (molecular template).");

            Rect listRect = new Rect(rect.x, infoRect.yMax + 4f, rect.width, rect.height - infoRect.height - 4f);
            float viewHeight = currentEmbryoGenes.Count * (RowHeight + 2f) + 10f;
            Rect viewRect = new Rect(0, 0, listRect.width - 20f, Mathf.Max(viewHeight, listRect.height));

            Widgets.BeginScrollView(listRect, ref scrollCurrent, viewRect);
            float curY = 0f;

            for (int i = 0; i < currentEmbryoGenes.Count; i++)
            {
                GeneDef gene = currentEmbryoGenes[i];
                Rect row = new Rect(0, curY, viewRect.width, RowHeight);
                if (i % 2 == 1) Widgets.DrawLightHighlight(row);

                bool isStagedForRemoval = tempStagedRemove.Contains(gene);
                bool inBank = bankGenes.Contains(gene);

                // Gene Label & Icon
                Rect labelRect = new Rect(row.x + 6f, row.y + 4f, 300f, 24f);
                if (isStagedForRemoval) GUI.color = Color.red;
                Widgets.Label(labelRect, gene.LabelCap);
                GUI.color = Color.white;

                // Stats: Cpx & Met
                Rect statsRect = new Rect(labelRect.xMax + 10f, row.y + 4f, 220f, 24f);
                GUI.color = Color.gray;
                Widgets.Label(statsRect, $"Cpx: {gene.biostatCpx} | Met: {(gene.biostatMet >= 0 ? "+" : "")}{gene.biostatMet}");
                GUI.color = Color.white;

                // Action Button
                Rect btnRect = new Rect(row.xMax - 140f, row.y + 2f, 130f, 26f);
                if (isStagedForRemoval)
                {
                    if (Widgets.ButtonText(btnRect, "Undo Remove"))
                    {
                        tempStagedRemove.Remove(gene);
                        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low);
                    }
                }
                else
                {
                    if (!inBank)
                    {
                        GUI.color = Color.gray;
                        Widgets.Label(btnRect, "No template in bank");
                        GUI.color = Color.white;
                    }
                    else if (totalEditsAfter >= 2)
                    {
                        GUI.color = Color.gray;
                        Widgets.Label(btnRect, "Cap reached (2/2)");
                        GUI.color = Color.white;
                    }
                    else
                    {
                        if (Widgets.ButtonText(btnRect, "Stage Remove"))
                        {
                            tempStagedRemove.Add(gene);
                            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High);
                        }
                    }
                }

                curY += RowHeight + 2f;
            }

            Widgets.EndScrollView();
        }

        private void DrawAddGenesTab(Rect rect)
        {
            CompEmbryoQuality quality = embryo?.TryGetComp<CompEmbryoQuality>();
            int currentEdits = quality?.editCount ?? 0;
            int totalEditsAfter = currentEdits + tempStagedAdd.Count + tempStagedRemove.Count;

            Rect searchRect = new Rect(rect.x, rect.y, 250f, 28f);
            searchFilter = Widgets.TextField(searchRect, searchFilter);

            Rect countLabelRect = new Rect(searchRect.xMax + 12f, rect.y + 4f, 300f, 24f);
            GUI.color = Color.gray;
            Widgets.Label(countLabelRect, $"Filter genes... ({availableToAdd.Count} in connected banks)");
            GUI.color = Color.white;

            Rect listRect = new Rect(rect.x, searchRect.yMax + 6f, rect.width, rect.height - searchRect.height - 6f);
            List<GeneDef> filtered = string.IsNullOrEmpty(searchFilter)
                ? availableToAdd
                : availableToAdd.Where(g => g.label.ToLower().Contains(searchFilter.ToLower())).ToList();

            float viewHeight = (filtered.Count + tempStagedAdd.Count) * (RowHeight + 2f) + 10f;
            Rect viewRect = new Rect(0, 0, listRect.width - 20f, Mathf.Max(viewHeight, listRect.height));

            Widgets.BeginScrollView(listRect, ref scrollAdd, viewRect);
            float curY = 0f;

            // Staged genes first
            for (int i = 0; i < tempStagedAdd.Count; i++)
            {
                GeneDef gene = tempStagedAdd[i];
                Rect row = new Rect(0, curY, viewRect.width, RowHeight);
                Widgets.DrawLightHighlight(row);

                Rect labelRect = new Rect(row.x + 6f, row.y + 4f, 300f, 24f);
                GUI.color = Color.green;
                Widgets.Label(labelRect, $"{gene.LabelCap} [Staged]");
                GUI.color = Color.white;

                Rect statsRect = new Rect(labelRect.xMax + 10f, row.y + 4f, 220f, 24f);
                GUI.color = Color.gray;
                Widgets.Label(statsRect, $"Cpx: {gene.biostatCpx} | Met: {(gene.biostatMet >= 0 ? "+" : "")}{gene.biostatMet}");
                GUI.color = Color.white;

                Rect btnRect = new Rect(row.xMax - 140f, row.y + 2f, 130f, 26f);
                if (Widgets.ButtonText(btnRect, "Undo Add"))
                {
                    tempStagedAdd.Remove(gene);
                    RefreshLists();
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low);
                    break;
                }

                curY += RowHeight + 2f;
            }

            // Available to add
            for (int i = 0; i < filtered.Count; i++)
            {
                GeneDef gene = filtered[i];
                Rect row = new Rect(0, curY, viewRect.width, RowHeight);
                if (i % 2 == 1) Widgets.DrawLightHighlight(row);

                Rect labelRect = new Rect(row.x + 6f, row.y + 4f, 300f, 24f);
                Widgets.Label(labelRect, gene.LabelCap);

                Rect statsRect = new Rect(labelRect.xMax + 10f, row.y + 4f, 220f, 24f);
                GUI.color = Color.gray;
                Widgets.Label(statsRect, $"Cpx: {gene.biostatCpx} | Met: {(gene.biostatMet >= 0 ? "+" : "")}{gene.biostatMet}");
                GUI.color = Color.white;

                Rect btnRect = new Rect(row.xMax - 140f, row.y + 2f, 130f, 26f);
                if (totalEditsAfter >= 2)
                {
                    GUI.color = Color.gray;
                    Widgets.Label(btnRect, "Cap reached (2/2)");
                    GUI.color = Color.white;
                }
                else
                {
                    if (Widgets.ButtonText(btnRect, "Stage Add"))
                    {
                        tempStagedAdd.Add(gene);
                        RefreshLists();
                        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High);
                        break;
                    }
                }

                curY += RowHeight + 2f;
            }

            Widgets.EndScrollView();
        }

        private void DrawFooter(Rect rect)
        {
            CompEmbryoQuality quality = embryo?.TryGetComp<CompEmbryoQuality>();
            int currentEdits = quality?.editCount ?? 0;
            int stagedTotal = tempStagedAdd.Count + tempStagedRemove.Count;
            int totalEdits = currentEdits + stagedTotal;

            // Recalculate preview Complexity & Metabolic Efficiency
            int cpx = currentEmbryoGenes.Where(g => !tempStagedRemove.Contains(g)).Sum(g => g.biostatCpx) + tempStagedAdd.Sum(g => g.biostatCpx);
            int met = currentEmbryoGenes.Where(g => !tempStagedRemove.Contains(g)).Sum(g => g.biostatMet) + tempStagedAdd.Sum(g => g.biostatMet);

            Rect statusRect = new Rect(rect.x, rect.y + 6f, rect.width - 320f, rect.height - 12f);
            Widgets.Label(statusRect, $"Edits: {totalEdits}/2 (Staged: +{tempStagedAdd.Count}/-{tempStagedRemove.Count}) | Projected Cpx: {cpx} | Met: {(met >= 0 ? "+" : "")}{met}");

            Rect btnClear = new Rect(rect.xMax - 310f, rect.y + 6f, 95f, 36f);
            if (Widgets.ButtonText(btnClear, "Reset"))
            {
                tempStagedAdd.Clear();
                tempStagedRemove.Clear();
                RefreshLists();
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low);
            }

            Rect btnCommit = new Rect(rect.xMax - 205f, rect.y + 6f, 110f, 36f);
            if (Widgets.ButtonText(btnCommit, "Commit Order"))
            {
                bench.stagedGenesToAdd = new List<GeneDef>(tempStagedAdd);
                bench.stagedGenesToRemove = new List<GeneDef>(tempStagedRemove);

                if (bench.HasPendingSpliceOrder)
                {
                    Messages.Message("Embryonic gene splicing order confirmed. A colonist assigned to Doctoring will perform the operation at the bench.", bench, MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("Embryo splicing order cleared.", bench, MessageTypeDefOf.NeutralEvent);
                }
                Close();
            }

            Rect btnClose = new Rect(rect.xMax - 85f, rect.y + 6f, 80f, 36f);
            if (Widgets.ButtonText(btnClose, "Close"))
            {
                Close();
            }
        }
    }
}
