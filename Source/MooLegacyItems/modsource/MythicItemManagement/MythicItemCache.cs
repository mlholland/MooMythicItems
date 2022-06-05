using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

/* This class is contains a cache of mythic items that the game can use to produce in-game mythic items. It is also in charge of saving new items,
 * and producing data about the current state of the cache.
 * 
 * This item is also responsible for turning mythic items into in-game things, since there's some bookkeeping that we need to do here
 * when that happens.
 */
namespace MooMythicItems
{
    [StaticConstructorOnStartup]
    public static class MythicItemCache
    { 
        // all elements of this list are assumed to be valid in the current modlist,
        // since they're checked upon loading the save file, and other incoming elements
        // must be coming from the live game itself.
        private static List<MythicItem> s_cachedItems = new List<MythicItem>();
        private static readonly string noReasonKey = "MooMF_NoMythicReasonGiven";

        private static readonly string newItemMessageKey = "MooMF_CreatedNewItemMessage";

        static MythicItemCache()
        {
            List<MythicItem> savedItems = SaveUtility.LoadMythicItemsFile();
            DebugActions.LogIfDebug("found {0} items from the save file into the cache", savedItems.Count);
            foreach (MythicItem savedItem in savedItems)
            {
                s_cachedItems.Add(savedItem);
            }
            DebugActions.LogIfDebug("loaded {0} items from the save file into the cache", s_cachedItems.Count);
        } 

        /* The main function for saving new mythic items. Returns true if the cache and save state were modified, and false otherwise.
         * Can modify the cache by either adding newItem to the cache, or by replacing a lower-priority version of this type of mythic
         * item with the new item.
         * The saving function can fail for the following reasons:
         *  - The originator pawn already has a mythic item.
         *  - The inputted mythic item is effectively the same as an already saved item (same reason, originating pawn, and originating world)
         *  - The inputted item is a equal or lower priority as a saved mythic item from the same person in a similar mythic 'class'
         *  - Too many mythic items of the same type have already been cached.
         */
        public static bool TrySaveOrOverwriteNewItem(MythicItem newItem, Pawn originator, string reasonPrefix, int priority, int globalLimit, string printedReasonFragment)
        {
            if (!MooMythicItems_Mod.settings.flagMythicOwnersCanCreateNewMythicItems && MythicItemUtilities.PawnHasMythicItem(originator))
            {
                DebugActions.LogIfDebug("Stopped creation of mythic item with reason '{0}' because the originator pawn '{1}' possesses a mythic item.", newItem.reason, newItem.ownerShortName);
                return false;
            }

            // First, make sure that an identical mythic item hasn't already been saved.
            // *Two mythic items are identical if they have the same reason, pawn, and originating world. We ignore the specific thingDef used.
            if (MythicItemCache.GetFirstSimilarCachedMythicItem(null, newItem.reason, newItem.originatorId, newItem.prv) != null)
            {
                DebugActions.LogIfDebug("Stopped creation of mythic item with reason '{0}' because we have already " +
                        "have the exact same item saved.", newItem.reason);
                return false;
            }

            // For mythic reasons with priorities, check and see if this pawn has ever created a mythic item for a similar reason before.
            // If they have, and any of those mythic items have a equal or higher priority than this one, then don't create it.
            // If a lower-priority item of the same category is found, replace it with the new item, and make the new item's starting
            // list of seen worlds equal the replaced item's list.
            if (priority > 0)
            {
                List<MythicItem> similarSavedItems = MythicItemCache.GetAllSimilarCachedMythicItems(null, reasonPrefix, newItem.originatorId, newItem.prv);
                int savedItemsWithPrio = 0;
                MythicItem itemToOverwrite = null;
                foreach (MythicItem mi in similarSavedItems)
                {
                    int savedItemPrio = mi.ExtractPriorityFromReason();
                    if (savedItemPrio > 0)
                    {
                        savedItemsWithPrio++;
                        if (savedItemPrio >= priority)
                        {
                            DebugActions.LogIfDebug("Stopped creation of mythic item with reason '{0}' because we have already " +
                                    "have a similar item saved for '{1} with a higher priority reason '{2}'.", newItem.reason, newItem.ownerShortName, mi.reason);
                            return false;
                        }
                        itemToOverwrite = mi;
                    }
                }
                if (savedItemsWithPrio > 1)
                {
                    DebugActions.LogWarn("While trying to create priority-based mythic item for {0} based on reason {1}, more " +
                        "than 1 saved mythic item with a similar reason and a priority was found. This shouldn't occur in normal gameplay.", newItem.ownerShortName, newItem.reason);
                }
                if (itemToOverwrite != null)
                {
                    s_cachedItems.Remove(itemToOverwrite);
                    newItem.worldsUsedIn = itemToOverwrite.worldsUsedIn;
                    SaveNewMythicItem(newItem, printedReasonFragment);
                    return true;
                }
            }
            // For items with global reason limits, make sure that this new item won't cause the total number of mythic items
            else if (globalLimit > 0 && MythicItemCache.GetAllSimilarCachedMythicItems(null, newItem.reason, null, 0).Count >= globalLimit)
            {
                DebugActions.LogIfDebug("Stopped mythic item creation with reason '{0}' because we have already " +
                        "reached the reasonLimit {1} for this category of item.", newItem.reason, globalLimit);
                return false;
            }
            SaveNewMythicItem(newItem, printedReasonFragment);
            return true;
        }

        /* New values are added to the end of line, to keep an implicitly time-ordered list of items */
        public static void SaveNewMythicItem(MythicItem newMythicItem, string reason)
        {
            if (MooMythicItems_Mod.settings.flagNotifyItemCreation)
            {
                if (reason == null) reason = string.Format(noReasonKey.Translate(), newMythicItem.ownerFullName);
                Messages.Message(string.Format(newItemMessageKey.Translate(), reason, newMythicItem.itemDef.label, newMythicItem.GetFormattedTitle()), MessageTypeDefOf.PositiveEvent, true);
            }
            DebugActions.LogIfDebug("Caching and saving new mythic item: {0}", newMythicItem.ToString());
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
        private static void SaveCachedMythicItems()
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
                return MythicItemUtilities.CreateRandomMythicItem();
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

        /* Return the first mythic item from the cached list that is not from the specified colony. Contains a bunch of optional parameters, most of which are only used for debugging/testing.
  */
        public static MythicItem SelectRandomMythicItemFromCacheWithOptions(bool createRandomMythicItemsIfNoValidOptionsFound, bool excludeItemsFromThisWorld, bool allowDuplicatesInCurrentMap)
        {
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
                validItems.Add(mi);
            }
            // If no valid options were found, either create a random item if that's an option, or return nothing.
            if (validItems.Count == 0)
            {
                if (createRandomMythicItemsIfNoValidOptionsFound)
                {
                    return MythicItemUtilities.CreateRandomMythicItem();
                }
                return null;
            }
            // pick an option, then realize it and perform behind-the-scenes bookkeeping 
            return validItems.RandomElement();
        }


        public static MythicItem SelectRandomMythicItemFromCache()
        {
            return SelectRandomMythicItemFromCacheWithOptions(MooMythicItems_Mod.settings.flagCreateRandomMythicItemsIfNoneAvailable, true, true);
        }


        /* Answers the question of 'given the current cached mythic items and settings, could we create a mythic item for the current world?'*/
        public static bool CanRealizeRandomMythicItem(bool createRandomMythicItemsIfNoValidOptionsFound, bool excludeItemsFromThisWorld, bool allowDuplicatesInCurrentMap)
        {
            return SelectRandomMythicItemFromCacheWithOptions(createRandomMythicItemsIfNoValidOptionsFound, excludeItemsFromThisWorld, allowDuplicatesInCurrentMap) != null;
        }

        /* Return the first mythic item from the cached list that is not from the specified colony. Contains a bunch of optional parameters, most of which are only used for debugging/testing.
         */
        public static Thing RealizeRandomMythicItemFromCacheWithOptions(bool createRandomMythicItemsIfNoValidOptionsFound, bool excludeItemsFromThisWorld, bool addWorldToCount, bool allowDuplicatesInCurrentMap)
        {
            // pick an option, then realize it and perform behind-the-scenes bookkeeping 
            MythicItem selectedItem = SelectRandomMythicItemFromCacheWithOptions(createRandomMythicItemsIfNoValidOptionsFound, excludeItemsFromThisWorld, allowDuplicatesInCurrentMap);
            if (addWorldToCount) AddThisWorldToWorldsSeen(selectedItem);
            return selectedItem.Realize();
        }

        /* This is the normal way of producing a mythic item for in-game use.*/
        public static Thing RealizeRandomMythicItemFromCache()
        {
            return RealizeRandomMythicItemFromCacheWithOptions(MooMythicItems_Mod.settings.flagCreateRandomMythicItemsIfNoneAvailable, true, true, true);
        }


        public static Thing RealizeSelectedMythicItem(MythicItem mi, bool addWorldToCount)
        {
            if (addWorldToCount) AddThisWorldToWorldsSeen(mi);
            return mi.Realize();
        }

        public static bool AddThisWorldToWorldsSeen(MythicItem mi)
        {
            int currentMapPrv = Find.World.info.persistentRandomValue;
            if (!mi.worldsUsedIn.Contains(currentMapPrv))
            {
                mi.worldsUsedIn.Add(currentMapPrv);
                if (MooMythicItems_Mod.settings.individualItemOccurenceLimit > 0 && mi.worldsUsedIn.Count >= MooMythicItems_Mod.settings.individualItemOccurenceLimit)
                {
                    s_cachedItems.Remove(mi);
                    SaveCachedMythicItems();
                    DebugActions.LogIfDebug("Mythic Item {0} has been used in {1} worlds, removing it from the saved list of mythic items.", mi.GetFormattedTitle(), mi.worldsUsedIn.Count);
                    if (MooMythicItems_Mod.settings.flagNotifyItemDeletion)
                    {
                        Messages.Message(string.Format("MooMF_MythicItemLastOccurence".Translate(), mi.GetFormattedTitle(), MooMythicItems_Mod.settings.individualItemOccurenceLimit), MessageTypeDefOf.PositiveEvent, true);
                    }
                    return true;
                }
                SaveCachedMythicItems();
            }
            return false;
        }

        public static HashSet<ThingDef> GetPossibleMythicThingDefs()
        {
            HashSet<ThingDef> result = new HashSet<ThingDef>();
            if (MooMythicItems_Mod.settings.flagCreateRandomMythicItemsIfNoneAvailable)
            {
                foreach(ThingDef def in from def in DefDatabase<ThingDef>.AllDefs
                                        where def.equipmentType == EquipmentType.Primary || def.IsApparel
                                        select def into d
                                        orderby d.defName
                                        select d)
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

        /* These two methods are used to check for similarity between potential new items and existing items in order to prevent identical mythic items from being 
         * repeatedly created (for example if a pawn keeps reaching level 20 in a skill due to vanilla skill decay).
         * itemDef, pawnId, and originatingWorldId must be exact matches if non-null (or non zero for the worldId), but the reasonFragment
         * only needs to a substring of a mythicItem's reason string to match. This allows for general reason matching (like any kill or skill-based reason).
         */
        public static MythicItem GetFirstSimilarCachedMythicItem(ThingDef itemDef, string reasonFragment, string pawnId, int originatingWorldId)
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

        public static List<MythicItem> GetAllSimilarCachedMythicItems(ThingDef itemDef, string reasonFragment, string pawnId, int originatingWorldId)
        {
            List<MythicItem> results = new List<MythicItem>();
            foreach (MythicItem mi in s_cachedItems)
            {
                if (itemDef != null && itemDef != mi.itemDef) { continue; }
                if (reasonFragment != null && !mi.reason.Contains(reasonFragment)) { continue; }
                if (pawnId != null && mi.originatorId != pawnId) { continue; }
                if (originatingWorldId != 0 && originatingWorldId != mi.prv) { continue; }
                results.Add(mi);
            }
            return results;
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
