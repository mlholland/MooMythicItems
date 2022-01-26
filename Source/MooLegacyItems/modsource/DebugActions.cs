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
            Log.Message(String.Format("Moo Legacy Items - def name: {0}", legacyItem.itemDefName));
            ThingDef def = DefDatabase<ThingDef>.GetNamed(legacyItem.itemDefName);
            ThingDef stuff = GenStuff.RandomStuffFor(def);
            Thing thing = ThingMaker.MakeThing(def, stuff);
            if (thingStyleDef != null)
            {
                thing.StyleDef = thingStyleDef;
            }
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQuality.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
            }
            if (thing.def.Minifiable)
            {
                thing = thing.MakeMinified();
            }

            CompLegacy legacyComp = thing.TryGetComp<CompLegacy>();
            if (legacyComp == null)
            {
                Log.Error(String.Format("Moo Legacy Items - Debug action to place a legacy item failed. The item def {0} had no legacy comp to modify", thing.def.defName));
            } else
            {
                legacyComp.newLabel = legacyItem.originatorName + "'s ";
                legacyComp.newDescription = String.Format(legacyItem.storyLabel, def.label, legacyItem.originatorName);
            }

            if (direct)
            {
                GenPlace.TryPlaceThing(thing, c, Find.CurrentMap, ThingPlaceMode.Direct, null, null, default(Rot4));
                return;
            }
            GenPlace.TryPlaceThing(thing, c, Find.CurrentMap, ThingPlaceMode.Near, null, null, default(Rot4));
        }


    }
}
