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

        public bool IsPowered => (powerComp == null || powerComp.PowerOn);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
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
            return s;
        }
    }
}
