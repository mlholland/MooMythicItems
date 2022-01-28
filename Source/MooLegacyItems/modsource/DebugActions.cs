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
        [DebugAction("Spawning", "Try place Randomized Legacy Item", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceRandomizedLegacyItem()
        {
            DebugSpawnLegacyItem(LegacyItemManager.CreateDefaultLegacyItem(), UI.MouseCell(), false);
        }

        [DebugAction("Spawning", "Try place Saved Legacy Item", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceSavedLegacyItem()
        {
            DebugSpawnLegacyItem(LegacyItemManager.GetFirstLegacyItemNotFromThisColony("~~~", true), UI.MouseCell(), false);
        }

        public static void DebugSpawnLegacyItem(LegacyItem legacyItem, IntVec3 c,  bool direct = false, ThingStyleDef thingStyleDef = null)
        {
            Thing thing = legacyItem.Realize();

            if (direct)
            {
                GenPlace.TryPlaceThing(thing, c, Find.CurrentMap, ThingPlaceMode.Direct, null, null, default(Rot4));
                return;
            }
            GenPlace.TryPlaceThing(thing, c, Find.CurrentMap, ThingPlaceMode.Near, null, null, default(Rot4));
        }


    }
}
