using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

/* The settings tab for this mod, will eventually include a bunch of controls.*/
namespace MooMythicItems
{
    public class DebugActions : ModSettings
    {
        [DebugAction("Spawning", "Spawn Random Mythic Item", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceRandomizedMythicItem()
        {
            DebugSpawnMythicItem(MythicItemCache.RealizeRandomlyGeneratedMythicItem(), UI.MouseCell(), false);
        }

        [DebugAction("Spawning", "Spawn Mythic Item", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceSavedMythicItem()
        {
            Thing item = MythicItemCache.RealizeRandomMythicItemFromCacheWithOptions(false, false, false, true);
            if (item == null)
            {
                if (MooMythicItems_Mod.settings.flagDebug)
                {
                    Log.Message("[Moo Mythic Items] " + "MooMF_FailedToCreateLIFromDebug".Translate());
                }
                Messages.Message("MooMF_FailedToCreateLIFromDebug".Translate(), MessageTypeDefOf.NeutralEvent, true);
            }
            DebugSpawnMythicItem(item, UI.MouseCell(), false);
        }

        [DebugAction("Spawning", "Try place Saved Mythic Item - Record World", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceSavedMythicItemRecordWorld()
        {
            DebugSpawnMythicItem(MythicItemCache.RealizeRandomMythicItemFromCacheWithOptions(false, false, true, true), UI.MouseCell(), false);
            
        }

        public static void DebugSpawnMythicItem(Thing item, IntVec3 c,  bool direct = false, ThingStyleDef thingStyleDef = null)
        {
            if (direct)
            {
                GenPlace.TryPlaceThing(item, c, Find.CurrentMap, ThingPlaceMode.Direct, null, null, default(Rot4));
                return;
            }
            GenPlace.TryPlaceThing(item, c, Find.CurrentMap, ThingPlaceMode.Near, null, null, default(Rot4));
        }

        [DebugAction("Spawning", "Try place all mythic item types", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DebugSpawnAllMythicItems()
        {
            // Todo
        }
    }
}
