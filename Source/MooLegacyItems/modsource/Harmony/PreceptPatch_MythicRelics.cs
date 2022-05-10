﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

/* This harmony patch identifies if a relic precept is actually a mythic relic precept, and performs custom logic if so.
 */
namespace MooMythicItems
{
    [HarmonyPatch(typeof(Precept_Relic))]
    [HarmonyPatch(nameof(Precept_Relic.GenerateRelic))]
    public class PreceptPatch_MythicRelics
    {
        static void Postfix(ref Thing __result, Precept_Relic __instance) //pass the __result by ref to alter it.
        {
            Precept_MythicRelic mythicRelicPrecept = __instance as Precept_MythicRelic;
            if (mythicRelicPrecept != null) { 
                if (__instance.ThingDef.CompDefFor<CompMythic>() == null)
                {
                    // todo print warning
                    Log.Error(String.Format("Trying to add mythic attributes to a ideology relic, but the relic def '{0}' doesn't have a mythic comp to modify.", __instance.ThingDef.defName));
                    return;
                }

                DebugActions.LogIfDebug("Adding Mythic Features to Relic: {0}", __result.Label);
                CompMythic relicMythicComp = __result.TryGetComp<CompMythic>();
                relicMythicComp.newLabel = mythicRelicPrecept.newLabel;
                relicMythicComp.newDescription = mythicRelicPrecept.newDescription;
                relicMythicComp.abilityDef = mythicRelicPrecept.abilityDef;
            } 
        }
    }
}