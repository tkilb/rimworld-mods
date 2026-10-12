using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace OrganicConstructs
{
    /// <summary>
    /// A 1x1 vertical stasis pod designed exclusively for biological constructs.
    /// Provides accelerated neural purging (36h vs 48h) when powered and buffers early wake shock.
    /// </summary>
    public class Building_ConstructStasisPod : Building_Bed
    {
        private CompPowerTrader powerComp;
        private CompGlower glowerComp;
        private Graphic activeGraphicInt;

        public bool IsPowered => (powerComp == null || powerComp.PowerOn);

        public bool IsOccupied
        {
            get
            {
                if (!Spawned) return false;
                for (int i = 0; i < SleepingSlotsCount; i++)
                {
                    if (GetCurOccupant(i) != null)
                        return true;
                }
                return false;
            }
        }

        public Graphic ActiveGraphic
        {
            get
            {
                if (activeGraphicInt == null && def.graphicData != null)
                {
                    string activePath = def.graphicData.texPath + "_active";
                    activeGraphicInt = GraphicDatabase.Get<Graphic_Multi>(
                        activePath,
                        def.graphicData.shaderType?.Shader ?? ShaderDatabase.Cutout,
                        def.graphicData.drawSize,
                        DrawColor,
                        DrawColorTwo
                    );
                }
                return activeGraphicInt;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (IsOccupied)
                {
                    Graphic g = ActiveGraphic;
                    if (g != null) return g;
                }
                return base.Graphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            glowerComp = GetComp<CompGlower>();
            UpdateGlowerState();
        }

        protected override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(60))
            {
                UpdateGlowerState();
            }
        }

        public void UpdateGlowerState()
        {
            if (glowerComp == null || !Spawned) return;

            bool shouldGlow = IsOccupied && IsPowered;
            if (Map != null)
            {
                if (shouldGlow)
                {
                    Map.glowGrid.RegisterGlower(glowerComp);
                }
                else
                {
                    Map.glowGrid.DeRegisterGlower(glowerComp);
                }
            }
        }

        public override string GetInspectString()
        {
            string s = base.GetInspectString();
            if (IsPowered)
            {
                s += "\n" + "Stasis purge rate: 133% (36h cycle, buffered shock dampening)";
            }
            else
            {
                s += "\n" + "Stasis purge rate: 100% (Unpowered: 48h cycle, standard shock)";
            }
            if (IsOccupied)
            {
                s += "\n" + "Status: Active construct stasis cycle (Synaptic defragmentation in progress)";
            }
            return s;
        }
    }
}
