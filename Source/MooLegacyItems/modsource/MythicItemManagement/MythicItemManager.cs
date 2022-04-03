﻿using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

/* This class is the overseer that keeps track of mythic items that have been recorded, but aren't currently in the world. It has the ability to select a mythic
 * item from the saved list of items, save new values to that file, ensure various consistency rules 
 * (like not spawning a mythic item that was generated by the current colony), and so on.
 * 
 * This item is also responsible for turning mythic items into in-game things, since there's some bookkeeping that we need to do here
 * when that happens.
 */
namespace MooMythicItems
{
    [StaticConstructorOnStartup]
    public static class MythicItemManager
    { 
        // all elements of this list are assumed to be valid in the current modlist,
        // since they're checked upon loading the save file, and other incoming elements
        // must be coming from the live game itself.
        private static List<MythicItem> s_cachedItems = new List<MythicItem>();

        private static HashSet<ThingDef> s_defaultDefs = null;
        private static HashSet<string> s_defaultNames = null; 
        private static HashSet<string> s_defaultFactions = null;
        private static HashSet<string> s_defaultDescriptions = null;
        private static HashSet<string> s_defaultTitles = null;
        private static HashSet<MythicEffectDef> s_defaultAbilities = null;

        static MythicItemManager()
        {
            List<MythicItem> savedItems = SaveUtility.LoadMythicItemsFile();
            if (MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("found {0} items from the save file into the cache", savedItems.Count));
            }
            foreach (MythicItem savedItem in savedItems)
            {
                s_cachedItems.Add(savedItem);
            }
            if (MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("loaded {0} items from the save file into the cache", s_cachedItems.Count));
            }
            PopulateDefaultGeneratorSets();
        } 

        // todo get this to a reasonable combination before live release
        private static void PopulateDefaultGeneratorSets()
        {
            //s_defaultDefs = new HashSet<ThingDef> { ThingDef.Named("Bow_Great"), ThingDef.Named("Gun_ChargeRifle"), ThingDef.Named("Gun_Revolver"), ThingDef.Named("Gun_BoltActionRifle"), ThingDef.Named("Gun_PumpShotgun"), ThingDef.Named("Gun_Autopistol") };
            s_defaultDefs = new HashSet<ThingDef> { ThingDef.Named("Apparel_FlakVest"), };
            s_defaultNames = new HashSet<string> { "Moo", "Tynan", "Randy", "Cassie", "Pheobe" }; 
            s_defaultFactions = new HashSet<string> { "Tynan's Tyranny Brigade", "Randy's Rabble Rousers", "Carrie's Centurions" };
            s_defaultDescriptions = new HashSet<string> { "MooMF_MythicStory_100Kills_Ranged_1", "MooMF_MythicStory_100Kills_Ranged_2" };
            s_defaultTitles = new HashSet<string> { "MooMF_MythicTitle_100Kills_Ranged_1", "MooMF_MythicTitle_100Kills_Ranged_2", "MooMF_MythicTitle_100Kills_Ranged_3" };
            //s_defaultAbilities = new HashSet<MythicEffectDef> { DefDatabase<MythicEffectDef>.GetNamed("MooMF_ConstructionBoost") }; // TODO set this to weapon effects
            s_defaultAbilities = new HashSet<MythicEffectDef> { DefDatabase<MythicEffectDef>.GetNamed("MooMF_TestAbility") }; // TODO remove
        }

        /* New values are added to the end of line, to keep an implicitly time-ordered list of items */
        public static void SaveNewMythicItem(MythicItem newMythicItem)
        {
            if (MooMythicItems_Mod.settings.flagNotifyItemCreation)
            {
                Messages.Message(string.Format("MooMF_CreatedNewItemMessage".Translate(), newMythicItem.ownerFullName, newMythicItem.itemDef.label, newMythicItem.reason), MessageTypeDefOf.PositiveEvent, true);
            }
            if (MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("Caching and saving new mythic item: {0}", newMythicItem.ToString()));
            }
            CacheMythicItem(newMythicItem);
            SaveCachedMythicItems();
        }

        /* Add a new mythic item to the cache without saving the cache to disk.
         */
        private static void CacheMythicItem(MythicItem newMythicItem)
        {
            s_cachedItems.Add(newMythicItem);
        }

        /* Save the current entire cached list of mythic items to disk, overwriting the previously saved csv file.
         * This is the point where mythic item save limits are enforced on both the cache and the resulting save file.
         */
        public static void SaveCachedMythicItems()
        {
            if (s_cachedItems.Count > MooMythicItems_Mod.settings.mythicItemSaveLimit)
            {
                s_cachedItems.RemoveAt(0);
            }
            SaveUtility.SaveMythicItemsFile(s_cachedItems); 
        }

        /* Return the first mythic item from the cached list that is not from the specified colony.
         * Includes optional parameter to keep the returned value from the cache, which is false by default.
         */
        public static MythicItem GetFirstMythicItemNotFromThisColony(int colonyId, bool keepReturnValueInCache=false)
        {
            List<MythicItem> returnToStack = new List<MythicItem>();
            MythicItem result = null;
            while(s_cachedItems.Count > 0)
            {
                MythicItem next = s_cachedItems.Pop();
                // mythic items from the current colony are ignored, and eventually re-added in the same order.
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
            // if we found no cached value from another colony, create a mythic item if settings permit it
            if (MooMythicItems_Mod.settings.flagCreateRandomMythicItemsIfNoneAvailable)
            {
                return CreateDefaultMythicItem();
            }
            return null;
        }

        public static void ClearCacheAndSaveFile()
        {
            s_cachedItems.Clear();
            SaveCachedMythicItems();
        }
        
        public static List<MythicItem> GetMythicItemsFromThisColony(int colonyID)
        {
            List<MythicItem> results = new List<MythicItem>();
            foreach(MythicItem mi in s_cachedItems)
            {
                if (mi.prv == colonyID)
                {
                    results.Add(mi);
                }
            } 
            return results;
        }

        // I made this a unique function from the above just to minimize the chance of accidentally abusing/modifying the return value of GetMythicItemsFromThisColony
        public static int CountMythicItemsProducedByThisColony(int colonyID)
        {
            int count = 0;
            foreach (MythicItem mi in s_cachedItems)
            {
                if (mi.prv == colonyID)
                {
                    count++;
                }
            }
            return count;
        }
        
        /* Generate a mythic item by randomly selecting among some hard-coded options. 
         */
        public static MythicItem CreateDefaultMythicItem()
        {
            string name = s_defaultNames.RandomElement();
            return new MythicItem(s_defaultDefs.RandomElement(), name, name, s_defaultFactions.RandomElement(),s_defaultDescriptions.RandomElement(), s_defaultTitles.RandomElement(),s_defaultAbilities.RandomElement(), null, 0, "debug", "0", new List<int>());
        }

       /* Answers the question of 'given the current cached mythic items and settings, could we create a mythic item for the current world?'*/
        public static bool CanRealizeRandomMythicItem(bool createRandomMythicItemsIfNoValidOptionsFound, bool excludeItemsFromThisWorld, bool allowDuplicatesInCurrentMap)
        {
            if (createRandomMythicItemsIfNoValidOptionsFound)
            {
                return true;
            }
            HashSet<MythicItem> validItems = new HashSet<MythicItem>();
            int currentMapPrv = Find.World.info.persistentRandomValue;

            // determine valid options
            foreach (MythicItem mi in s_cachedItems)
            {
                if (excludeItemsFromThisWorld && mi.prv == currentMapPrv)
                {
                    continue;
                }
                if (!allowDuplicatesInCurrentMap && mi.worldsUsedIn.Contains(currentMapPrv))
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        /* This is the normal way of producing a mythic item for in-game use.*/
        public static Thing RealizeRandomMythicItemFromCache()
        {
            return RealizeRandomMythicItemFromCacheWithOptions(MooMythicItems_Mod.settings.flagCreateRandomMythicItemsIfNoneAvailable, true, true, true);
        }

        /* Return the first mythic item from the cached list that is not from the specified colony. Contains a bunch of optional parameters, most of which are only used for debugging/testing.
         */
        public static Thing RealizeRandomMythicItemFromCacheWithOptions(bool createRandomMythicItemsIfNoValidOptionsFound, bool excludeItemsFromThisWorld, bool addThisMapToItemWorldsUsedInListUponRealization, bool allowDuplicatesInCurrentMap)
        {
            HashSet<MythicItem> validItems = new HashSet<MythicItem>();
            int currentMapPrv = Find.World.info.persistentRandomValue;

            // determine valid options
            foreach(MythicItem mi in s_cachedItems)
            { 
                if (excludeItemsFromThisWorld && mi.prv == currentMapPrv)
                {
                    continue;
                }
                if (!allowDuplicatesInCurrentMap && mi.worldsUsedIn.Contains(currentMapPrv))
                {
                    continue;
                }
                validItems.Add(mi);
            }
            // If no valid options were found, either create a random item if that's an option, or return nothing.
            if (validItems.Count == 0)
            {
                if (createRandomMythicItemsIfNoValidOptionsFound)
                {
                    return RealizeMythicItem(CreateDefaultMythicItem());
                }
                return null;
            }
            // pick an option, then realize it and perform behind-the-scenes bookkeeping 
            MythicItem selectedItem = validItems.RandomElement();
            if (addThisMapToItemWorldsUsedInListUponRealization && !selectedItem.worldsUsedIn.Contains(currentMapPrv))
            {
                selectedItem.worldsUsedIn.Add(currentMapPrv);
                if (MooMythicItems_Mod.settings.individualItemOccurenceLimit > 0 && selectedItem.worldsUsedIn.Count >= MooMythicItems_Mod.settings.individualItemOccurenceLimit)
                {
                    s_cachedItems.Remove(selectedItem);
                }
                SaveCachedMythicItems();
            }
            return RealizeMythicItem(selectedItem);
        }

        /* Turn a mythic item into a real Thing that can show up in-game.
         * Kept private in this file to ensure that realization only occurs in concert with behind-the-scenes management of saved mythic items.
         */
        private static Thing RealizeMythicItem(MythicItem mi)
        {
            if (MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("[Moo Mythic Items] Realizing mythic item: {0}", mi.ToString()));
            }
            ThingDef def = mi.itemDef;
            ThingDef stuff = null;
            if (MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("[Moo Mythic Items] Realized mythic item has stuff type: {0}", mi.stuffDef));
            }
            if (mi.stuffDef != null)
            {
                stuff  = mi.stuffDef;
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

            CompMythic mythicComp = thing.TryGetComp<CompMythic>();
            if (mythicComp == null)
            {
                throw new InvalidCastException(String.Format("[Moo Mythic Items] Mythic Item realization failed. The item def {0} had no mythic comp to modify, yet a saved mythic item was based on one", thing.def.defName));
            }
            else
            {
                mythicComp.newLabel = String.Format(mi.titleTranslationString.Translate(), mi.ownerShortName, def.label);
                mythicComp.newDescription = String.Format(mi.descriptionTranslationString.Translate(), mi.ownerFullName, mi.ownerShortName, mi.factionName, def.label);
                mythicComp.abilityDef = mi.abilityDef;
            }

            if(MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("[Moo Mythic Items] Created a mythic item with the following attributes: {0}", mi.ToString()));
            }
            return thing;
        } 

        public static HashSet<ThingDef> GetPossibleMythicItemDefs()
        {
            HashSet<ThingDef> result = new HashSet<ThingDef>();
            if (MooMythicItems_Mod.settings.flagCreateRandomMythicItemsIfNoneAvailable)
            {
                foreach(ThingDef def in s_defaultDefs)
                {
                    result.Add(def);
                }
            }
            HashSet<MythicItem> validItems = new HashSet<MythicItem>();
            int currentMapPrv = Find.World.info.persistentRandomValue;

            // determine valid options
            foreach (MythicItem mi in s_cachedItems)
            {
                if (mi.prv == currentMapPrv)
                {
                    continue;
                }
                if (mi.worldsUsedIn.Contains(currentMapPrv))
                {
                    continue;
                }
                result.Add(mi.itemDef);
            }
            return result;
        }

        /* Used to check for similarity between potential new items and existing items in order to prevent identical mythic items from being 
         * repeated created (for example if a pawn keeps reaching level 20 in a skill due to vanilla skill decay).
         * itemDef, pawnId, and originatingWorldId must be exact matches if non-null (or non zero for the worldId), but the reasonFragment
         * only needs to a substring of a mythicItem's reason string to match. This allows for general reason matching (like any kill or skill-based reason).
         */
        public static MythicItem GetSimilarCachedMythicItem(ThingDef itemDef, string reasonFragment, string pawnId, int originatingWorldId)
        {
            foreach (MythicItem mi in s_cachedItems)
            {
                if (itemDef != null && itemDef != mi.itemDef) { continue; }
                if (reasonFragment != null && !mi.reason.Contains(reasonFragment)) { continue; }
                if (pawnId != null && mi.originatorId != pawnId) { continue; }
                if (originatingWorldId != 0 && originatingWorldId != mi.prv) { continue; }
                return mi;
            }
            return null;
        }

        public static List<MythicItem> GetMythicItems(bool includeItemsFromThisWorld, bool includeItemsSeenInThisWorld)
        {
            List<MythicItem> results = new List<MythicItem>();
            int currentWorldPrv = Find.World.info.persistentRandomValue;
            foreach(MythicItem mi in s_cachedItems)
            {
                if (!includeItemsFromThisWorld && currentWorldPrv == mi.prv)
                {
                    continue;
                }
                if (!includeItemsSeenInThisWorld && mi.worldsUsedIn.Contains(currentWorldPrv))
                {
                    continue;
                }
                results.Add(mi);
            }
            return results;
        }


    }
}
