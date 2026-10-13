using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace OrganicConstructs
{
    /// <summary>
    /// Handles backward-compatible save game migration for deprecated Defs.
    /// Runs upon loading an existing save game, converting any physical instances
    /// to their standard RimWorld equivalents and purging deprecated Def references
    /// from stockpile, bill, and outfit filters.
    /// </summary>
    public class GameComponent_ConstructMigration : GameComponent
    {
        public GameComponent_ConstructMigration(Game game) : base()
        {
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            MigrateDeprecatedDefs();
        }

        public static void MigrateDeprecatedDefs()
        {
            ThingDef deprecatedPaste = DefDatabase<ThingDef>.GetNamedSilentFail("GeneticNutrientPaste");
            if (deprecatedPaste == null) return;

            ThingDef mealNutrientPaste = ThingDefOf.MealNutrientPaste;
            int convertedItemsCount = 0;
            int cleanedFiltersCount = 0;

            // 1. Convert physical items spawned on all maps
            if (Find.Maps != null)
            {
                foreach (Map map in Find.Maps)
                {
                    if (map?.listerThings == null) continue;
                    List<Thing> things = map.listerThings.ThingsOfDef(deprecatedPaste).ToList();
                    foreach (Thing t in things)
                    {
                        int stackCount = t.stackCount;
                        IntVec3 pos = t.Position;
                        Map m = t.Map;
                        t.Destroy(DestroyMode.Vanish);

                        if (mealNutrientPaste != null && m != null)
                        {
                            Thing newMeal = ThingMaker.MakeThing(mealNutrientPaste);
                            newMeal.stackCount = stackCount;
                            GenPlace.TryPlaceThing(newMeal, pos, m, ThingPlaceMode.Near);
                            convertedItemsCount += stackCount;
                        }
                    }
                }
            }

            // 2. Convert items in pawn inventories (colonists, prisoners, pack animals)
            if (PawnsFinder.AllMaps_Spawned != null)
            {
                foreach (Pawn p in PawnsFinder.AllMaps_Spawned)
                {
                    if (p?.inventory?.innerContainer != null)
                    {
                        List<Thing> invThings = p.inventory.innerContainer.Where(t => t.def == deprecatedPaste).ToList();
                        foreach (Thing t in invThings)
                        {
                            int stack = t.stackCount;
                            p.inventory.innerContainer.Remove(t);
                            t.Destroy(DestroyMode.Vanish);

                            if (mealNutrientPaste != null)
                            {
                                Thing replacement = ThingMaker.MakeThing(mealNutrientPaste);
                                replacement.stackCount = stack;
                                p.inventory.innerContainer.TryAdd(replacement);
                                convertedItemsCount += stack;
                            }
                        }
                    }
                }
            }

            if (Find.WorldObjects?.Caravans != null)
            {
                foreach (Caravan caravan in Find.WorldObjects.Caravans)
                {
                    if (caravan?.PawnsListForReading != null)
                    {
                        foreach (Pawn p in caravan.PawnsListForReading)
                        {
                            if (p?.inventory?.innerContainer != null)
                            {
                                List<Thing> invThings = p.inventory.innerContainer.Where(t => t.def == deprecatedPaste).ToList();
                                foreach (Thing t in invThings)
                                {
                                    int stack = t.stackCount;
                                    p.inventory.innerContainer.Remove(t);
                                    t.Destroy(DestroyMode.Vanish);

                                    if (mealNutrientPaste != null)
                                    {
                                        Thing replacement = ThingMaker.MakeThing(mealNutrientPaste);
                                        replacement.stackCount = stack;
                                        p.inventory.innerContainer.TryAdd(replacement);
                                        convertedItemsCount += stack;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 3. Purge from stockpile zones and storage buildings
            if (Find.Maps != null)
            {
                foreach (Map map in Find.Maps)
                {
                    if (map?.zoneManager != null)
                    {
                        foreach (Zone zone in map.zoneManager.AllZones)
                        {
                            if (zone is Zone_Stockpile stockpile && stockpile.settings?.filter != null)
                            {
                                if (stockpile.settings.filter.Allows(deprecatedPaste))
                                {
                                    stockpile.settings.filter.SetAllow(deprecatedPaste, false);
                                    cleanedFiltersCount++;
                                }
                            }
                        }
                    }

                    if (map?.listerBuildings != null)
                    {
                        foreach (Building b in map.listerBuildings.allBuildingsColonist)
                        {
                            if (b is Building_Storage storage && storage.settings?.filter != null)
                            {
                                if (storage.settings.filter.Allows(deprecatedPaste))
                                {
                                    storage.settings.filter.SetAllow(deprecatedPaste, false);
                                    cleanedFiltersCount++;
                                }
                            }
                        }
                    }
                }
            }

            // 4. Purge from Outfit / Apparel policies
            if (Current.Game?.outfitDatabase != null)
            {
                foreach (ApparelPolicy policy in Current.Game.outfitDatabase.AllOutfits)
                {
                    if (policy.filter != null && policy.filter.Allows(deprecatedPaste))
                    {
                        policy.filter.SetAllow(deprecatedPaste, false);
                        cleanedFiltersCount++;
                    }
                }
            }

            if (convertedItemsCount > 0 || cleanedFiltersCount > 0)
            {
                Log.Message($"[Organic Constructs] Migrated deprecated GeneticNutrientPaste: Converted {convertedItemsCount} item(s) to Nutrient Paste and purged from {cleanedFiltersCount} filter(s). Saving your game will make this permanent.");
            }
        }
    }
}
