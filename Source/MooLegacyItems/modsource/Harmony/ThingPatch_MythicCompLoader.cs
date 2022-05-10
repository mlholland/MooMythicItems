using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

/* This patch intercepts the save/load behavior of ThingWithComps and identifies if the thing in question is a mythic item. If so, it saves or loads the associated mythicComp,
 * which isn't normally loaded because it's not part of any normal defs.
 * When saving, mythic items are ID'd by the presense of a mythic comp.
 * When loading, they're identified by a flag that's saved to the ThingWithComps itself.
 */
namespace MooMythicItems
{
    public class ThingPatch_MythicCompLoader
    {
        private static readonly string isMiythicLookString = "MooMFIsMythic";

        [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.ExposeData))]
        static class ThingWithComps_InitializeComps_PostResolve_Patch
        {
            static void Postfix(ref ThingWithComps __instance, List<ThingComp> ___comps)
            {
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    CompMythic comp = __instance.TryGetComp<CompMythic>();
                    if (comp != null)
                    {
                        DebugActions.LogIfDebug("Saving a mythic item called {0}.", comp.newLabel);
                        bool isMythic = true;
                        Scribe_Values.Look(ref isMythic, isMiythicLookString);
                    }
                }
                else
                {
                    bool isMythic = false;
                    Scribe_Values.Look(ref isMythic, isMiythicLookString);
                    if (isMythic)
                    {
                        DebugActions.LogIfDebug("Found a mythic effect on a saved {0}, turning it into a mythic item.", __instance.def.label);
                        CompMythic newComp = Activator.CreateInstance<CompMythic>();
                        newComp.parent = __instance;
                        ___comps.Add(newComp);
                        newComp.Initialize(new CompProperties_Mythic());
                        newComp.PostExposeData();
                    }
                }
            }
        }
    }
}
