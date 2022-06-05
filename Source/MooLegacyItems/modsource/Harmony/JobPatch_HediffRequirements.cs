using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse.AI;

/* This patch modifies job acceptance to allow some jobs to require the worker to have a specific hediff in order to perform them.
 * Certain mythic abilities "unlock" the ability to perform jobs, usually to make special, impressive items. This code is part of that
 * system.
 */
namespace MooMythicItems
{
    [HarmonyPatch(typeof(Bill))]
    [HarmonyPatch(nameof(Bill.PawnAllowedToStartAnew))]
    public class JobPatch_HediffRequirements
    {
        static void Postfix(Pawn p, Bill __instance, ref bool __result) //pass the __result by ref to alter it.
        {
            bool newVal = RecipeHediffRequirementsDef.PawnMeetsHediffRequirement(p, __instance.recipe);
            if (!newVal && __result)
            {
                __result = false;
                JobFailReason.Is(String.Format("MooMF_RecipeRequiresHediffs".Translate(), p.Name.ToStringShort, __instance.recipe.label));
            }
        }
    }
}