﻿using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

/* This class is the overseer that keeps track of legacy items. It has the ability to select a legacy
 * item from the saved list of items, save new values to that file, ensure various consistency rules 
 * (like not spawning a legacy item that was generated by the current colony), and so on.
 * 
 * This item is also responsible for turning legacyItems into in-game things, since there's some bookkeeping that we need to do here
 * when that happens.
 */
namespace MooLegacyItems
{
    [StaticConstructorOnStartup]
    public static class LegacyItemManager
    { 
        // all elements of this list are assumed to be valid in the current modlist,
        // since they're checked upon loading the save file, and other incoming elements
        // must be coming from the live game itself.
        private static List<LegacyItem> s_cachedItems = new List<LegacyItem>();

        private static HashSet<ThingDef> s_defaultDefs = null;
        private static HashSet<string> s_defaultNames = null; 
        private static HashSet<string> s_defaultFactions = null;
        private static HashSet<string> s_defaultDescriptions = null;
        private static HashSet<string> s_defaultTitles = null;
        private static HashSet<LegacyEffectDef> s_defaultAbilities = null;

        static LegacyItemManager()
        {
            List<LegacyItem> savedItems = SaveUtility.LoadLegacyItemsFile();
            if (MooLegacyItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("found {0} items from the save file into the cache", savedItems.Count));
            }
            foreach (LegacyItem savedItem in savedItems)
            {
                // todo correctly do Def checks
                //if (DefDatabase<Def>.GetNamed(savedItem.itemDefName, false) != null)
                //{
                    s_cachedItems.Add(savedItem);
                //}
            }
            if (MooLegacyItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("loaded {0} items from the save file into the cache", s_cachedItems.Count));
            }
            PopulateDefaultGeneratorSets();
        } 

        private static void PopulateDefaultGeneratorSets()
        {
            s_defaultDefs = new HashSet<ThingDef> { ThingDef.Named("Bow_Great"), ThingDef.Named("Gun_ChargeRifle"), ThingDef.Named("Gun_Revolver"), ThingDef.Named("Gun_BoltActionRifle"), ThingDef.Named("Gun_PumpShotgun"), ThingDef.Named("Gun_Autopistol") };
            s_defaultNames = new HashSet<string> { "Moo", "Tynan", "Randy", "Cassie", "Pheobe" }; 
            s_defaultFactions = new HashSet<string> { "Tynan's Tyranny Brigade", "Randy's Rabble Rousers", "Carrie's Centurions" };
            s_defaultDescriptions = new HashSet<string> { "MooLI_LegacyStory_100Kills_Ranged_1", "MooLI_LegacyStory_100Kills_Ranged_2" };
            s_defaultTitles = new HashSet<string> { "MooLI_LegacyTitle_100Kills_Ranged_1", "MooLI_LegacyTitle_100Kills_Ranged_2", "MooLI_LegacyTitle_100Kills_Ranged_3" };
            s_defaultAbilities = new HashSet<LegacyEffectDef> { DefDatabase<LegacyEffectDef>.GetNamed("MooLI_ConstructionBoost") }; // TODO set this to weapon effects
         }

        /* New values are added to the end of line, to keep an implicitly time-ordered list of items */
        public static void SaveNewLegacyItem(LegacyItem newLegacyItem)
        {
            if (MooLegacyItems_Mod.settings.flagNotifyItemCreation)
            {
                Messages.Message(string.Format("MooLI_CreatedNewItemMessage".Translate(), newLegacyItem.ownerFullName, newLegacyItem.itemDef.label, newLegacyItem.reason), MessageTypeDefOf.PositiveEvent, true);
            }
            if (MooLegacyItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("Caching and saving new legacy item: {0}", newLegacyItem.ToString()));
            }
            CacheLegacyItem(newLegacyItem);
            SaveCachedLegacyItems();
        }

        /* Add a new legacy item to the cache without saving the cache to disk.
         */
        public static void CacheLegacyItem(LegacyItem newLegacyItem)
        {
            s_cachedItems.Add(newLegacyItem);
        }

        /* Save the current entire cached list of legacy items to disk, overwriting the previously saved csv file.
         * This is the point where legacy item save limits are enforced on both the cache and the resulting save file.*/
        public static void SaveCachedLegacyItems()
        {
            if (s_cachedItems.Count > MooLegacyItems_Mod.settings.legacyItemSaveLimit)
            {
                s_cachedItems.RemoveAt(0);
            }
            SaveUtility.SaveLegacyItemsFile(s_cachedItems); 
        }

        /* Return the first legacy item from the cached list that is not from the specified colony.
         * Includes optional parameter to keep the returned value from the cache, which is false by default.
         */
        public static LegacyItem GetFirstLegacyItemNotFromThisColony(int colonyId, bool keepReturnValueInCache=false)
        {
            List<LegacyItem> returnToStack = new List<LegacyItem>();
            LegacyItem result = null;
            while(s_cachedItems.Count > 0)
            {
                LegacyItem next = s_cachedItems.Pop();
                // legacy items from the current colony are ignored, and eventually re-added in the same order.
                if(next.prv == colonyId)
                {
                    returnToStack.Add(next);
                }
                else
                {
                    result = next;
                    if (keepReturnValueInCache)
                    {
                        returnToStack.Add(next);
                    }
                    break;
                }
            }
            // re-add items from this colony back to the front of the list,
            // as well as the returned value if input params permit.
            s_cachedItems.InsertRange(0, returnToStack);
            // return the cached value if we found one
            if (result != null)
            {
                return result;
            }
            // if we found no cached value from another colony, create a legacy item if settings permit it
            if (MooLegacyItems_Mod.settings.flagCreateRandomLegacyItemsIfNoneAvailable)
            {
                return CreateDefaultLegacyItem();
            }
            return null;
        }

        public static void ClearCacheAndSaveFile()
        {
            s_cachedItems.Clear();
            SaveCachedLegacyItems();
        }
        
        public static List<LegacyItem> GetLegacyItemsFromThisColony(int colonyID)
        {
            List<LegacyItem> results = new List<LegacyItem>();
            foreach(LegacyItem li in s_cachedItems)
            {
                if (li.prv == colonyID)
                {
                    results.Add(li);
                }
            } 
            return results;
        }

        public static int CountLegacyItemsProducedByThisColony(int colonyID)
        {
            int count = 0;
            foreach (LegacyItem li in s_cachedItems)
            {
                if (li.prv == colonyID)
                {
                    count++;
                }
            }
            return count;
        }
        
        /* Generate a legacy item by randomly selecting among some hard-coded options. 
         */
        public static LegacyItem CreateDefaultLegacyItem()
        {
            string name = s_defaultNames.RandomElement();
            return new LegacyItem(s_defaultDefs.RandomElement(), name, name, s_defaultFactions.RandomElement(),s_defaultDescriptions.RandomElement(), s_defaultTitles.RandomElement(),s_defaultAbilities.RandomElement(), null, 0, "debug", "0", new List<int>());
        }

       /*   */
        public static bool CanRealizeRandomLegacyItem(bool createRandomLegacyItemsIfNoValidOptionsFound, bool excludeItemsFromThisWorld, bool allowDuplicatesInCurrentMap)
        {
            if (createRandomLegacyItemsIfNoValidOptionsFound)
            {
                return true;
            }
            HashSet<LegacyItem> validItems = new HashSet<LegacyItem>();
            int currentMapPrv = Find.World.info.persistentRandomValue;

            // determine valid options
            foreach (LegacyItem li in s_cachedItems)
            {
                if (excludeItemsFromThisWorld && li.prv == currentMapPrv)
                {
                    continue;
                }
                if (!allowDuplicatesInCurrentMap && li.worldsUsedIn.Contains(currentMapPrv))
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        /* This is the normal way of producing a legacy item for in-game use.*/
        public static Thing RealizeRandomLegacyItemFromCache()
        {
            return RealizeRandomLegacyItemFromCacheWithOptions(MooLegacyItems_Mod.settings.flagCreateRandomLegacyItemsIfNoneAvailable, true, true, true);
        }

        /* Return the first legacy item from the cached list that is not from the specified colony. Contains a bunch of optional parameters, most of which are only used for debugging/testing.
         */
        public static Thing RealizeRandomLegacyItemFromCacheWithOptions(bool createRandomLegacyItemsIfNoValidOptionsFound, bool excludeItemsFromThisWorld, bool addThisMapToItemWorldsUsedInListUponRealization, bool allowDuplicatesInCurrentMap)
        {
            HashSet<LegacyItem> validItems = new HashSet<LegacyItem>();
            int currentMapPrv = Find.World.info.persistentRandomValue;

            // determine valid options
            foreach(LegacyItem li in s_cachedItems)
            { 
                if (excludeItemsFromThisWorld && li.prv == currentMapPrv)
                {
                    continue;
                }
                if (!allowDuplicatesInCurrentMap && li.worldsUsedIn.Contains(currentMapPrv))
                {
                    continue;
                }
                validItems.Add(li);
            }
            // If no valid options were found, either create a random item if that's an option, or return nothing.
            if (validItems.Count == 0)
            {
                if (createRandomLegacyItemsIfNoValidOptionsFound)
                {
                    return RealizeLegacyItem(CreateDefaultLegacyItem());
                }
                return null;
            }
            // pick an option, then realize it and perform behind-the-scenes bookkeeping 
            LegacyItem selectedItem = validItems.RandomElement();
            if (addThisMapToItemWorldsUsedInListUponRealization && !selectedItem.worldsUsedIn.Contains(currentMapPrv))
            {
                selectedItem.worldsUsedIn.Add(currentMapPrv);
                if (MooLegacyItems_Mod.settings.individualItemOccurenceLimit > 0 && selectedItem.worldsUsedIn.Count >= MooLegacyItems_Mod.settings.individualItemOccurenceLimit)
                {
                    s_cachedItems.Remove(selectedItem);
                }
                SaveCachedLegacyItems();
            }
            return RealizeLegacyItem(selectedItem);
        }

        /* Turn a legacy item into a real Thing that can show up in-game.
         * Kept private in this file to ensure that realization only occurs in concert with behind-the-scenes management of saved legacy items.
         */
        private static Thing RealizeLegacyItem(LegacyItem li)
        {
            if (MooLegacyItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("[Moo Legacy Items] Realizing legacy item: {0}", li.ToString()));
            }
            ThingDef def = li.itemDef;
            ThingDef stuff = null;
            if (MooLegacyItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("[Moo Legacy Items] Realized legacy item has stuff type: {0}", li.stuffDef));
            }
            if (li.stuffDef != null)
            {
                stuff  = li.stuffDef;
            }
            Thing thing = ThingMaker.MakeThing(def, stuff);
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQuality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            }
            if (thing.def.Minifiable)
            {
                thing = thing.MakeMinified();
            }

            CompLegacy legacyComp = thing.TryGetComp<CompLegacy>();
            if (legacyComp == null)
            {
                throw new InvalidCastException(String.Format("Moo Legacy Items - Legacy Item realization failed. The item def {0} had no legacy comp to modify, yet a saved legacy item was based on one", thing.def.defName));
            }
            else
            {
                legacyComp.newLabel = String.Format(li.titleTranslationString.Translate(), li.ownerShortName, li.itemDef);
                legacyComp.newDescription = String.Format(li.descriptionTranslationString.Translate(), li.ownerFullName, li.ownerShortName, li.factionName, def.label);
                legacyComp.abilityDef = li.abilityDef;
            }

            if(MooLegacyItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("[Moo Legacy Items] Created a legacy item with the following attributes: {0}", li.ToString()));
            }
            return thing;
        } 

        public static HashSet<ThingDef> GetPossibleLegacyItemDefs()
        {
            HashSet<ThingDef> result = new HashSet<ThingDef>();
            if (MooLegacyItems_Mod.settings.flagCreateRandomLegacyItemsIfNoneAvailable)
            {
                foreach(ThingDef def in s_defaultDefs)
                {
                    result.Add(def);
                }
            }
            HashSet<LegacyItem> validItems = new HashSet<LegacyItem>();
            int currentMapPrv = Find.World.info.persistentRandomValue;

            // determine valid options
            foreach (LegacyItem li in s_cachedItems)
            {
                if (li.prv == currentMapPrv)
                {
                    continue;
                }
                if (li.worldsUsedIn.Contains(currentMapPrv))
                {
                    continue;
                }
                result.Add(li.itemDef);
            }
            return result;
        }

        /* Used to check for similarity between potential new items and existing items in order to prevent identical legacy items from being 
         * repeated created (for example if a pawn keeps reaching level 20 in a skill due to vanilla skill decay).
         * itemDef, pawnId, and originatingWorldId must be exact matches if def (or non zero for the worldId), but the reasonFragment
         * only needs to be contained by a legacyItem's reason string to match. This allows for general reason matching (like any kill or skill based item).
         */
        public static LegacyItem GetSimilarCachedLegacyItem(ThingDef itemDef, string reasonFragment, string pawnId, int originatingWorldId)
        {
            foreach (LegacyItem li in s_cachedItems)
            {
                if (itemDef != null && itemDef != li.itemDef) { continue; }
                if (reasonFragment != null && !li.reason.Contains(reasonFragment)) { continue; }
                if (pawnId != null && li.originatorId != pawnId) { continue; }
                if (originatingWorldId != 0 && originatingWorldId != li.prv) { continue; }
                return li;
            }
            return null;
        }

        public static List<LegacyItem> GetLegacyItems(bool includeItemsFromThisWorld, bool includeItemsSeenInThisWorld)
        {
            List<LegacyItem> results = new List<LegacyItem>();
            int currentWorldPrv = Find.World.info.persistentRandomValue;
            foreach(LegacyItem li in s_cachedItems)
            {
                if (!includeItemsFromThisWorld && currentWorldPrv == li.prv)
                {
                    continue;
                }
                if (!includeItemsSeenInThisWorld && li.worldsUsedIn.Contains(currentWorldPrv))
                {
                    continue;
                }
                results.Add(li);
            }
            return results;
        }


    }
}
