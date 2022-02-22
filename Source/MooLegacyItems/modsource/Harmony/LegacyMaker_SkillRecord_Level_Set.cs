using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

/* This patch intercepts the code that sets a pawn's skill level, and spawns a legacy item if the code is
 * settings a colonist's skill to 20.
 * 
 * 
 */
namespace MooLegacyItems
{
    public class LegacyMaker_SkillRecord_Level_Set
    {
        // todo consider making this configurable
        private static int s_legacyLevel = 20;

        private static string s_legacySkillStoryPrefix = "MooLI_LegacyStory_Mastery_";


        // note to tomorrow self - this doesn't work because "Level" isn't the actual value, it's the internal field "levelInt", which is set in many more ways
        [HarmonyPatch(typeof(SkillRecord))]
        [HarmonyPatch("Level", MethodType.Setter)]
        static class SkillRecord_Level_Set_PreResolve_Patch
        {
            static void Prefix(SkillRecord __instance, int  value)
            {
                Log.Message(String.Format("lvling from {0} to {1}", __instance.Level, value));
                if (value - __instance.Level == 1 && value == s_legacyLevel)
                {
                    Log.Message(String.Format("pass 1"));
                    // try to create a legacy item
                    // TODO replace this with a more robust function that prefers different pieces of equipment based on the skill in question
                    Thing legacyThing = __instance.Pawn?.equipment?.Primary;
                    if (legacyThing != null)
                    {
                        Log.Message(String.Format("pass 2"));
                        LegacyItem newItem = new LegacyItem(legacyThing, __instance.Pawn, "TODO GET COLONY NAME", s_legacySkillStoryPrefix + __instance.def.defName, "Masterful");
                        LegacyItemManager.SaveNewLegacyItem(newItem);
                    };
                } 
            }
        }
    }     
}
