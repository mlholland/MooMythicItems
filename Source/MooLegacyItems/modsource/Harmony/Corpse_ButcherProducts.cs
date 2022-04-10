using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

/* This patch checks if a butcher has a buff that increases butcher yields, and if so, increases the resulting butcher yields.
 */
namespace MooMythicItems
{
    public class Corpse_ButcherProducts
    {
        private static List<ThingDef> thrumboDefs;
        private static List<ThingDef> ThrumboDefs
        {
            get
            {
                if (thrumboDefs == null)
                {
                    string listDefName = "MooMF_ThrumboDefList";
                    ThingDefListDef thrumboDefList = DefDatabase<ThingDefListDef>.GetNamed(listDefName);
                    if (thrumboDefList == null)
                    {
                        Log.Error("[Moo Mythic Items] tried and failed to retrieve list of animal types that are considered thrumbos for record-keeping purposes. Thrumbo kills won't be recorded, nor will it be possible to produce thrumbo-kill-based mythic items. The def we were looking for was named: " + listDefName);
                        thrumboDefs = new List<ThingDef>();
                        return thrumboDefs;
                    }
                    thrumboDefs = thrumboDefList.values;
                }
                return thrumboDefs;
            }
        }

        [HarmonyPatch(typeof(Corpse), nameof(Corpse.ButcherProducts))]
        static class Corpse_ButcherProducts_Preresolve_Patch
        {
            static void Postfix(ref IEnumerable<Thing> __result, Corpse __instance, Pawn butcher, float efficiency)
            {
                if (butcher != null && butcher.health != null && butcher.health.hediffSet != null && ThrumboDefs != null && 
                    butcher.health.hediffSet.HasHediff(HediffDef.Named("MooMF_ThrumboButcher")) && ThrumboDefs.Contains(__instance.InnerPawn.def))
                {
                    __result = ButcherProductsSpecial(__result, 2); // TODO make this multiplier part of the related cause/hediff or something?
                }
            }
        }

        // copy of the original butcher code, but with the corpse as an input, and an additional multiplier add
        private static IEnumerable<Thing> ButcherProductsSpecial(IEnumerable<Thing> __result, int multiplier)
        {
            List<Thing> results = new List<Thing>();
            foreach(Thing thing in __result)
            {
                thing.stackCount *= multiplier;
                results.Add(thing);
            }
            return results.AsEnumerable();
        }

    }     
}
