﻿using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

/* This class is the overseer that keeps track of legacy items. It has the ability to select a legacy
 * item from the saved list of items, save new values to that file, ensure various consistency rules 
 * (like not spawning a legacy item that was generated by the current colony), and so on.
 */
namespace MooLegacyItems
{
    [StaticConstructorOnStartup]
    public static class LegacyItemManager
    { 
        // all elements of this list are assumed to be valid in the current modlist,
        // since they're checked upon loading the save file, and other incoming elements
        // must be coming from the live game itself.
        public static List<LegacyItem> s_cachedItems = new List<LegacyItem>();

        private static HashSet<string> s_defaultDefs = null;
        private static HashSet<string> s_defaultNames = null;
        private static HashSet<string> s_defaultColonies = null;
        private static HashSet<string> s_defaultStories = null;
        private static HashSet<string> s_defaultAbilities = null;

        static LegacyItemManager()
        {
            List<LegacyItem> savedItems = SaveUtility.LoadLegacyItemsFile();
            Log.Message(String.Format("found {0} items from the save file into the cache", savedItems.Count));
            foreach (LegacyItem savedItem in savedItems)
            {
                // todo correctly do Def checks
                //if (DefDatabase<Def>.GetNamed(savedItem.itemDefName, false) != null)
                //{
                    s_cachedItems.Add(savedItem);
                //}
            }
            Log.Message(String.Format("loaded {0} items from the save file into the cache", s_cachedItems.Count));
        } 

        private static void PopulateDefaultGeneratorSets()
        {
            s_defaultDefs = new HashSet<string> { "Bow_Great", "Gun_ChargeRifle", "Gun_Revolver", "Gun_BoltActionRifle", "Gun_PumpShotgun", "Gun_Autopistol" };
            s_defaultNames = new HashSet<string> { "Moo", "Tynan", "Randy", "Cassie", "Pheobe", "HI19HI19" };
            s_defaultColonies = new HashSet<string> { "The Last Bastion", "The Frozen Outpost", "Balrog's Rest", "Seahome" };
            s_defaultStories = new HashSet<string> { "Ran everyday until their hair fell out.", "Won an eating contest.", "Pulled off their thumb, then put it on again. HOW!? Also, took my nose and never gabe it back. >:(", "Stubbed their toe." };
            s_defaultAbilities = new HashSet<string> { "Bloodthirsty", "Unyielding", "Ceaseless" };
         }

        /* New values are added to the end of line, to keep an implicitly time-ordered list of items */
        public static void SaveNewLegacyItem(LegacyItem newLegacyItem)
        {
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
                s_cachedItems = s_cachedItems.GetRange(s_cachedItems.Count - MooLegacyItems_Mod.settings.legacyItemSaveLimit, MooLegacyItems_Mod.settings.legacyItemSaveLimit);
            }
            SaveUtility.SaveLegacyItemsFile(s_cachedItems); 
        }

        /* Return the first legacy item from the cached list that is not from the specified colony.
         * Includes optional parameter to keep the returned value from the cache, which is false by default.
         */
        public static LegacyItem GetFirstLegacyItemNotFromThisColony(String colonyId, bool keepReturnValueInCache=false)
        {
            List<LegacyItem> returnToStack = new List<LegacyItem>();
            LegacyItem result = null;
            while(s_cachedItems.Count > 0)
            {
                LegacyItem next = s_cachedItems.Pop();
                // legacy items from the current colony are ignored, and eventually re-added in the same order.
                if(next.originatorColonyName == colonyId)
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
            if (MooLegacyItems_Mod.settings.flagCreateDefaultLegacyItemsIfNoneAvailable)
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

        public static int CountLegacyItemsProducedByThisColony(String colonyId)
        { 
            throw new NotImplementedException();
            return 0;
        }
        
        public static LegacyItem CreateDefaultLegacyItem()
        {
            if(s_defaultDefs == s_defaultDefs)
            {
                PopulateDefaultGeneratorSets();
            }
            return new LegacyItem(s_defaultDefs.RandomElement(), s_defaultNames.RandomElement(), s_defaultColonies.RandomElement(), s_defaultStories.RandomElement(), s_defaultAbilities.RandomElement());
        }


    }
}
