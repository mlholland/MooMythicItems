using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;


/* This thing set maker tries to create a mythic item. Used for quest rewards and such.
 */
namespace MooMythicItems
{
    public class ThingSetMaker_MythicItemMaker : ThingSetMaker
    { 
        public ThingSetMaker_MythicItemMaker() { }
         
        protected override bool CanGenerateSub(ThingSetMakerParams parms)
        {
            DebugActions.LogIfDebug("running ThingSetMaker_MythicItemMaker CanGenerateSub");
            return MythicItemCache.CanRealizeRandomMythicItem(MooMythicItems_Mod.settings.flagCreateRandomMythicItemsIfNoneAvailable, true, true);
        }
         
        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            DebugActions.LogIfDebug("running ThingSetMaker_MythicItemMaker Generate");
            outThings.Add(MythicItemCache.RealizeRandomMythicItemFromCache());
        }
         
        protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
        {
            DebugActions.LogIfDebug("running ThingSetMaker_MythicItemMaker AllowedThingDefs");
            return ThingSetMakerUtility.GetAllowedThingDefs(parms);
        }
         
         
        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            DebugActions.LogIfDebug("running ThingSetMaker_MythicItemMaker AllGeneratableThingsDebugSub");
            return MythicItemCache.GetPossibleMythicThingDefs().AsEnumerable();
        }
         
    }
}
