using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace OrganicConstructs
{
    public static class ConstructStabilityUtility
    {
        public static float CalculateStability(int complexity, int metabolism, IEnumerable<GeneDef> genes = null)
        {
            float stability = 1.0f;
            
            stability -= complexity * 0.015f;
            
            if (metabolism > 0)
            {
                stability += metabolism * 0.02f;
            }
            else if (metabolism < 0)
            {
                stability -= Mathf.Abs(metabolism) * 0.04f;
            }

            if (genes != null)
            {
                foreach (GeneDef gene in genes)
                {
                    if (gene.defName == "Gene_MitochondrialOverdrive")
                    {
                        stability -= 0.25f;
                    }
                    else if (gene.defName == "Gene_GenomicCompression")
                    {
                        stability -= 0.20f;
                    }
                }
            }

            return Mathf.Clamp(stability, 0.05f, 1.0f);
        }

        public static string GetStabilityRatingLabel(float stability)
        {
            if (stability >= 0.90f) return "Industrial Grade";
            if (stability >= 0.75f) return "Stable";
            if (stability >= 0.60f) return "Unstable";
            return "Highly Volatile";
        }

        public static Color GetStabilityColor(float stability)
        {
            if (stability >= 0.90f) return Color.green;
            if (stability >= 0.75f) return Color.white;
            if (stability >= 0.60f) return new Color(1f, 0.5f, 0f); // Orange
            return Color.red;
        }
    }
}
