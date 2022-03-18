using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;


/* This thing set maker tries to create a mythic item. If it for whatever reason it can't make a mythic item, it fails to create a mythic item
 * it functions like a normal ThingSetMaker_MarketValue. This class is what allows mythic items to be generated as quest rewards.
 */
namespace MooMythicItems
{
    public class ThingSetMaker_MythicItemMaker : ThingSetMaker
    { 
        public ThingSetMaker_MythicItemMaker()
        { 
        }
         
        protected override bool CanGenerateSub(ThingSetMakerParams parms)
        {
            if (MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message("[Moo Mythic Items] running ThingSetMaker_MythicItemMaker CanGenerateSub");
            }
            return MythicItemManager.CanRealizeRandomMythicItem(MooMythicItems_Mod.settings.flagCreateRandomMythicItemsIfNoneAvailable, true, true);
        }
         
        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            if (MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message("[Moo Mythic Items] running ThingSetMaker_MythicItemMaker Generate");
            }
            outThings.Add(MythicItemManager.RealizeRandomMythicItemFromCache());
        }
         
        protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
        {
            if (MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message("[Moo Mythic Items] running ThingSetMaker_MythicItemMaker AllowedThingDefs");
            }
            return ThingSetMakerUtility.GetAllowedThingDefs(parms);
        }
         
         
        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            if(MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message("[Moo Mythic Items] running ThingSetMaker_MythicItemMaker AllGeneratableThingsDebugSub");
            }
            return MythicItemManager.GetPossibleMythicItemDefs().AsEnumerable();
        }
         
    }
}
