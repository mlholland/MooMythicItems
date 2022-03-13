using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;


/* This thing set maker tries to create a legacy item. If it for whatever reason it can't make a legacy item, it fails to create a legacy item
 * it functions like a normal ThingSetMaker_MarketValue.
 */
namespace MooLegacyItems
{
    public class ThingSetMaker_LegacyItemMaker : ThingSetMaker
    { 
        public ThingSetMaker_LegacyItemMaker()
        { 
        }
         
        protected override bool CanGenerateSub(ThingSetMakerParams parms)
        {
            if (MooLegacyItems_Mod.settings.flagDebug)
            {
                Log.Message("[Moo Legacy Items] running ThingSetMaker_LegacyItemMaker CanGenerateSub");
            }
            return LegacyItemManager.CanRealizeRandomLegacyItem(MooLegacyItems_Mod.settings.flagCreateRandomLegacyItemsIfNoneAvailable, true, true);
        }
         
        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            if (MooLegacyItems_Mod.settings.flagDebug)
            {
                Log.Message("[Moo Legacy Items] running ThingSetMaker_LegacyItemMaker Generate");
            }
            outThings.Add(LegacyItemManager.RealizeRandomLegacyItemFromCache());
        }
         
        protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
        {
            if (MooLegacyItems_Mod.settings.flagDebug)
            {
                Log.Message("[Moo Legacy Items] running ThingSetMaker_LegacyItemMaker AllowedThingDefs");
            }
            return ThingSetMakerUtility.GetAllowedThingDefs(parms);
        }
         
         
        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            if(MooLegacyItems_Mod.settings.flagDebug)
            {
                Log.Message("[Moo Legacy Items] running ThingSetMaker_LegacyItemMaker AllGeneratableThingsDebugSub");
            }
            return LegacyItemManager.GetPossibleLegacyItemDefs().AsEnumerable();
        }
         
    }
}
