using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

/* This patch intercepts the code that increments a pawn's # things killed records to check and
 * see if that pawn is worthy of creating a legacy item.
 * 
 * Legacy checks that are encompassed by this patch include
 * - 100 humanlike kills
 * - 1000 humanlike kills TODO
 * - 100 mech kills TODO 
 * - 100 insect kills TODO
 */
namespace MooLegacyItems
{
    public class LegacyMaker_Notify_PawnKilled
    {
        [HarmonyPatch(typeof(RecordsUtility), nameof(RecordsUtility.Notify_PawnKilled))]
        static class RecordsUtility_Notify_PawnKilled_PostResolve_Patch
        {
            static void Postfix(Pawn killed, Pawn killer)
            {
                killer.records.Increment(RecordDefOf.Kills);
                RaceProperties raceProps = killed.RaceProps;
                if (raceProps.Humanlike)
                {
                    if(killer.records.GetValue(RecordDefOf.KillsHumanlikes) == 5) // todo more checks, like isColonist
                    {
                        // todo maybe check if it exists already
                        if (killer.equipment?.Primary != null)
                        {
                            LegacyReasonToDetailOptionsDef details = LegacyReasonToDetailOptionsDef.Instance;
                            // todo add more robust story and ability selection
                            LegacyItem newItem = new LegacyItem(killer.equipment.Primary, 
                                killer,
                                killer.equipment.Primary.def.IsRangedWeapon ?
                                details.manyKillsMFD.RandomElement() :
                                details.manyKillsRFD.RandomElement(),
                                killer.equipment.Primary.def.IsRangedWeapon ?
                                details.manyKillsMT.RandomElement() :
                                details.manyKillsRT.RandomElement(),
                                DefDatabase<LegacyEffectDef>.GetNamed("MooLI_ConstructionBoost"), // todo set to a weapon effect
                                "100kills");
                            LegacyItemManager.SaveNewLegacyItem(newItem);
                        }
                    }
                }
                if (raceProps.Animal)
                {
                    //killer.records.Increment(RecordDefOf.KillsAnimals);
                }
                if (raceProps.IsMechanoid)
                {
                    //killer.records.Increment(RecordDefOf.KillsMechanoids);
                }
            }
        }
    }     
}
