using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

/* The settings tab for this mod, will eventually include a bunch of controls.*/
namespace MooLegacyItems
{
    public class DebugActions : ModSettings
    {
        [DebugAction("Spawning", "Try place Legacy Item Allow Pregen", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceRandomizedLegacyItem()
        {
            DebugSpawnLegacyItem(LegacyItemManager.RealizeRandomLegacyItemFromCacheWithOptions(true, false, false, true), UI.MouseCell(), false);
        }

        [DebugAction("Spawning", "Try place Saved Legacy Item", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceSavedLegacyItem()
        {
            DebugSpawnLegacyItem(LegacyItemManager.RealizeRandomLegacyItemFromCacheWithOptions(false, false, false, true), UI.MouseCell(), false);
        }

        [DebugAction("Spawning", "Try place Saved Legacy Item - Record World", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceSavedLegacyItemRecordWorld()
        {
            DebugSpawnLegacyItem(LegacyItemManager.RealizeRandomLegacyItemFromCacheWithOptions(false, false, true, true), UI.MouseCell(), false);
        }


        public static void DebugSpawnLegacyItem(Thing item, IntVec3 c,  bool direct = false, ThingStyleDef thingStyleDef = null)
        {
            if (direct)
            {
                GenPlace.TryPlaceThing(item, c, Find.CurrentMap, ThingPlaceMode.Direct, null, null, default(Rot4));
                return;
            }
            GenPlace.TryPlaceThing(item, c, Find.CurrentMap, ThingPlaceMode.Near, null, null, default(Rot4));
        }

        [DebugAction("Spawning", "Try place all legacy item types", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DebugSpawnAllLegacyItems()
        {
            // Todo
        }
    }
}
