using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

/* 
 */
namespace MooLegacyItems
{
    [HarmonyPatch(typeof(Precept_Relic))]
    [HarmonyPatch(nameof(Precept_Relic.GenerateRelic))]
    public class Precept_Relic_GenerateRelic
    {
        static void Postfix(ref Thing __result, Precept_Relic __instance) //pass the __result by ref to alter it.
        {
            if (true) // TODO Change this to setting flag
            {
                Precept_LegacyRelic legacyRelicPrecept = __instance as Precept_LegacyRelic;
                if (legacyRelicPrecept != null) { 
                    if (__instance.ThingDef.CompDefFor<CompLegacy>() == null)
                    {
                        // todo print warning
                        Log.Error(String.Format("[Moo Legacy Item] Trying to add legacy attributes to a relic, but the relic def '{0}' doesn't have a legacy comp to modify.", __instance.ThingDef.defName));
                        return;
                    }

                    if (MooLegacyItems_Mod.settings.flagDebug)
                    {
                        Log.Message(String.Format("[Moo Legacy Items] Adding Legacy Features to Relic: {0}", __result.Label));
                    }
                    CompLegacy relicLegacyComp = __result.TryGetComp<CompLegacy>();
                    relicLegacyComp.newLabel = legacyRelicPrecept.newLabel;
                    relicLegacyComp.newDescription = legacyRelicPrecept.newDescription;
                    relicLegacyComp.abilityDef = legacyRelicPrecept.abilityDef;
                }
            }
        }
    }
}