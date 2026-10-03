using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    public static class ConstructNameUtility
    {
        public const string ConstructLastName = "[CONSTRUCT]";
        public const string ConstructIdPrefix = "c-";
        public const string NicknamePrefix = "C-";

        public static bool HasConstructName(Pawn pawn)
        {
            if (pawn?.Name is NameTriple nt)
            {
                return nt.Last == ConstructLastName || (!string.IsNullOrEmpty(nt.First) && nt.First.StartsWith(ConstructIdPrefix));
            }
            return false;
        }

        public static void AssignConstructNameIfNeeded(Pawn pawn)
        {
            if (pawn == null || HasConstructName(pawn))
            {
                return;
            }

            string constructId = GenerateConstructId();
            string nickname = GenerateNextAvailableNickname();

            pawn.Name = new NameTriple(constructId, nickname, ConstructLastName);
            pawn.babyNamingDeadline = -1;
        }

        public static string GenerateConstructId()
        {
            HashSet<string> existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead != null)
            {
                foreach (Pawn p in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
                {
                    if (p.Name is NameTriple nt && !string.IsNullOrEmpty(nt.First) && nt.First.StartsWith(ConstructIdPrefix))
                    {
                        existingIds.Add(nt.First);
                    }
                }
            }

            string id;
            do
            {
                // Generate a terse 4-hex-character lowercase UUID (e.g. "c-8f4a")
                id = ConstructIdPrefix + Guid.NewGuid().ToString("N").Substring(0, 4).ToLowerInvariant();
            }
            while (existingIds.Contains(id));

            return id;
        }

        public static string GenerateNextAvailableNickname()
        {
            HashSet<int> usedNumbers = new HashSet<int>();
            if (PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead != null)
            {
                foreach (Pawn p in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
                {
                    if (p.Name is NameTriple nt && !string.IsNullOrEmpty(nt.Nick) && nt.Nick.StartsWith(NicknamePrefix))
                    {
                        string suffix = nt.Nick.Substring(NicknamePrefix.Length);
                        if (int.TryParse(suffix, out int parsedNum) && parsedNum > 0)
                        {
                            usedNumbers.Add(parsedNum);
                        }
                    }
                }
            }

            int num = 1;
            while (usedNumbers.Contains(num))
            {
                num++;
            }

            return NicknamePrefix + num.ToString("D2");
        }
    }
}
