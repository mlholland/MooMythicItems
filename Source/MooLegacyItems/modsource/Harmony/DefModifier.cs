using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

/* This patch adds the CompMythic comp to all defs that match the CanBeLegaciedPredicate. 
 * In practice this is most weapons and wearable items that are not single use or stackable.
 * 
 * Bear in mind that the CompMythic comp doesn't immediately make all items mythic items, rather
 * it gives these items the capacity to be mythic items if that comp is further modified by an external
 * source (like other code pathways in this mod).
 */
namespace MooMythicItems
{
    public class DefModifier
    {
        [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
        static class DefGenerator_GenerateImpliedDefs_PostResolve_Patch
        {
            static void Postfix()
            {
                DebugActions.LogIfDebug("Interrupting GenerateImpliedDefs make weapons and clothing 'mythic-able'");
                var defs = DefDatabase<ThingDef>.AllDefs.Where(InjectPredicate).ToList();

                var compProperties = new Verse.CompProperties { compClass = typeof(CompMythic) };

                foreach (var def in defs)
                { 
                    def.comps.Add(compProperties);
                }
            }
        }

        static bool InjectPredicate(ThingDef def)
        {
            if (!def.HasComp(typeof(CompQuality)))
            {
                return false;
            }
            if (def.HasComp(typeof(CompMythic)))
            {
                return false;
            }
            if (def.Verbs.Any(v => typeof(Verb_ShootOneUse).IsAssignableFrom(v.GetType())))
            {
                return false;
            }
            if(!def.IsWeapon && !def.IsApparel)
            {
                return false;
            }
            return true;
        }
    }
}
