using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace OrganicConstructs
{
    public class Dialog_ConfigureConstructGenome : Window
    {
        private Building_ConstructGenomeArchitect architect;
        private Pawn pawn;

        private string templateName = "Custom Genome";
        private HashSet<GeneDef> selectedGenes = new HashSet<GeneDef>();
        
        private List<GeneDef> availableGenes = new List<GeneDef>();
        private string searchFilter = "";

        private Vector2 leftScrollPosition;
        private Vector2 rightScrollPosition;

        private static readonly string[] CoreGenes = new string[]
        {
            "Gene_ConstructPsychology",
            "Instability_Major",
            "Gene_MandatorySterility",
            "Gene_ConstructHibernation",
            "Immunity_SuperStrong",
            "Pain_Reduced",
            "Robust",
            "MeleeDamage_Strong",
            "MoveSpeed_Quick",
            "WoundHealing_Fast",
            "Superclotting"
        };

        public override Vector2 InitialSize => new Vector2(920f, 680f);

        public Dialog_ConfigureConstructGenome(Building_ConstructGenomeArchitect architect, Pawn pawn = null)
        {
            this.architect = architect;
            this.pawn = pawn;
            this.doCloseX = true;
            this.closeOnClickedOutside = false;
            this.absorbInputAroundWindow = true;

            foreach (string g in CoreGenes)
            {
                GeneDef def = DefDatabase<GeneDef>.GetNamedSilentFail(g);
                if (def != null)
                {
                    selectedGenes.Add(def);
                }
            }

            GatherAvailableGenes();
        }
        
        private void GatherAvailableGenes()
        {
            availableGenes.Clear();
            if (architect != null && architect.ConnectedGeneBanks != null)
            {
                foreach (Building bank in architect.ConnectedGeneBanks)
                {
                    CompGenepackContainer comp = bank.GetComp<CompGenepackContainer>();
                    if (comp != null && comp.ContainedGenepacks != null)
                    {
                        foreach (Genepack pack in comp.ContainedGenepacks)
                        {
                            if (pack != null && pack.GeneSet != null && pack.GeneSet.GenesListForReading != null)
                            {
                                foreach (GeneDef gene in pack.GeneSet.GenesListForReading)
                                {
                                    if (!availableGenes.Contains(gene))
                                    {
                                        availableGenes.Add(gene);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            availableGenes.SortBy(g => g.label);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 35f);
            Widgets.Label(titleRect, "Construct Genome Architect");
            Text.Font = GameFont.Small;

            float bottomBarHeight = 80f;
            Rect mainRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, inRect.height - 40f - bottomBarHeight - 10f);
            
            float leftWidth = mainRect.width * 0.45f;
            float rightWidth = mainRect.width * 0.55f - 10f;
            
            Rect leftRect = new Rect(mainRect.x, mainRect.y, leftWidth, mainRect.height);
            Rect rightRect = new Rect(mainRect.x + leftWidth + 10f, mainRect.y, rightWidth, mainRect.height);

            DoLeftColumn(leftRect);
            DoRightColumn(rightRect);
            DoBottomBar(new Rect(inRect.x, inRect.yMax - bottomBarHeight, inRect.width, bottomBarHeight));
        }

        private void DoLeftColumn(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect innerRect = rect.ContractedBy(10f);
            
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(innerRect.x, innerRect.y, innerRect.width, 30f), "Selected Genes");
            Text.Font = GameFont.Small;

            float y = innerRect.y + 35f;
            
            Rect viewRect = new Rect(0f, 0f, innerRect.width - 16f, selectedGenes.Count * 28f + 80f);
            Widgets.BeginScrollView(new Rect(innerRect.x, y, innerRect.width, innerRect.height - 35f), ref leftScrollPosition, viewRect);
            
            float listY = 0f;
            
            Widgets.Label(new Rect(0f, listY, viewRect.width, 24f), "Core Construct Genes (Locked):");
            listY += 28f;
            
            foreach (string g in CoreGenes)
            {
                GeneDef def = DefDatabase<GeneDef>.GetNamedSilentFail(g);
                if (def != null)
                {
                    Rect rowRect = new Rect(0f, listY, viewRect.width, 24f);
                    if (Mouse.IsOver(rowRect)) Widgets.DrawHighlight(rowRect);
                    
                    Widgets.Label(new Rect(24f, listY, viewRect.width - 24f, 24f), def.LabelCap);
                    TooltipHandler.TipRegion(rowRect, def.description);
                    
                    GUI.DrawTexture(new Rect(2f, listY + 2f, 20f, 20f), Widgets.CheckboxOnTex);
                    
                    listY += 28f;
                }
            }
            
            listY += 10f;
            Widgets.Label(new Rect(0f, listY, viewRect.width, 24f), "Selected Adaptations:");
            listY += 28f;

            GeneDef toRemove = null;
            foreach (GeneDef def in selectedGenes.Where(g => !CoreGenes.Contains(g.defName)).ToList())
            {
                Rect rowRect = new Rect(0f, listY, viewRect.width, 24f);
                if (Mouse.IsOver(rowRect)) Widgets.DrawHighlight(rowRect);

                if (Widgets.ButtonImage(new Rect(2f, listY + 2f, 20f, 20f), Widgets.CheckboxOffTex))
                {
                    toRemove = def;
                }

                Widgets.Label(new Rect(24f, listY, viewRect.width - 24f, 24f), def.LabelCap);
                TooltipHandler.TipRegion(rowRect, def.description);
                listY += 28f;
            }

            Widgets.EndScrollView();

            // Apply removal after draw loop to avoid invalidating the enumerator mid-frame
            if (toRemove != null) selectedGenes.Remove(toRemove);
        }

        private void DoRightColumn(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect innerRect = rect.ContractedBy(10f);

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(innerRect.x, innerRect.y, innerRect.width, 30f), "Available Genepacks");
            Text.Font = GameFont.Small;

            searchFilter = Widgets.TextField(new Rect(innerRect.x, innerRect.y + 35f, innerRect.width, 24f), searchFilter);
            
            float y = innerRect.y + 65f;
            
            var filtered = availableGenes.Where(g => string.IsNullOrEmpty(searchFilter) || g.label.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            
            Rect viewRect = new Rect(0f, 0f, innerRect.width - 16f, filtered.Count * 28f);
            Widgets.BeginScrollView(new Rect(innerRect.x, y, innerRect.width, innerRect.height - 65f), ref rightScrollPosition, viewRect);
            
            float listY = 0f;
            foreach (GeneDef def in filtered)
            {
                Rect rowRect = new Rect(0f, listY, viewRect.width, 24f);
                if (Mouse.IsOver(rowRect)) Widgets.DrawHighlight(rowRect);
                
                bool isCore = CoreGenes.Contains(def.defName);
                bool isSelected = selectedGenes.Contains(def);
                
                if (isCore)
                {
                    GUI.color = Color.gray;
                    Widgets.Label(new Rect(24f, listY, viewRect.width - 24f, 24f), def.LabelCap + " (Core)");
                    GUI.color = Color.white;
                }
                else
                {
                    bool check = isSelected;
                    Widgets.Checkbox(new Vector2(2f, listY + 2f), ref check, 20f);
                    if (check != isSelected)
                    {
                        if (check) selectedGenes.Add(def);
                        else selectedGenes.Remove(def);
                    }
                    Widgets.Label(new Rect(24f, listY, viewRect.width - 24f, 24f), def.LabelCap);
                }
                
                TooltipHandler.TipRegion(rowRect, def.description);
                listY += 28f;
            }
            
            Widgets.EndScrollView();
        }

        private void DoBottomBar(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect innerRect = rect.ContractedBy(10f);
            
            int cpx = selectedGenes.Sum(g => g.biostatCpx);
            int met = selectedGenes.Sum(g => g.biostatMet);
            
            float stability = ConstructStabilityUtility.CalculateStability(cpx, met, selectedGenes.ToList());
            
            Rect statsRect = new Rect(innerRect.x, innerRect.y, 250f, innerRect.height);
            Widgets.Label(new Rect(statsRect.x, statsRect.y, statsRect.width, 20f), $"Complexity: {cpx}");
            Widgets.Label(new Rect(statsRect.x, statsRect.y + 20f, statsRect.width, 20f), $"Net Metabolism: {met}");
            Widgets.Label(new Rect(statsRect.x, statsRect.y + 40f, statsRect.width, 20f), $"Genome Stability: {stability.ToStringPercent()}");
            
            Rect nameRect = new Rect(innerRect.x + 260f, innerRect.y + 15f, 200f, 30f);
            templateName = Widgets.TextField(nameRect, templateName);
            
            Rect buttonRect = new Rect(innerRect.width - 150f, innerRect.y + 15f, 150f, 30f);
            
            bool canBurn = architect != null && architect.LoadedDisc != null && architect.LoadedBlueprintComp != null && !architect.LoadedBlueprintComp.isBurned;
            
            if (!canBurn)
            {
                GUI.color = Color.grey;
            }
            
            if (Widgets.ButtonText(buttonRect, "Burn Master Disc", true, true, canBurn))
            {
                if (canBurn)
                {
                    architect.LoadedBlueprintComp.BurnGenome(templateName, selectedGenes.ToList());
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                    Messages.Message($"Successfully burned template '{templateName}' onto Genome Blueprint.", MessageTypeDefOf.PositiveEvent);
                    this.Close();
                }
            }
            
            GUI.color = Color.white;
        }
    }
}
