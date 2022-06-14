using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

/* Compatibility patch for additional on-kill logic, like incrementing the new kill-count records based on modded conditions.
 * These 'patches' don't include headers, and are instead patched manually pased on whether the needed mods are present
 */
namespace MooMythicItems
{
    public class PawnKilledPatch_Compatibility
    {

        private static RecordDef insectKills;
        private static RecordDef InsectKills
        {
            get
            {
                if (insectKills == null)
                {
                    insectKills = DefDatabase<RecordDef>.GetNamed("MooMF_killsInsects");
                }
                return insectKills;
            }
        }

        // Patch to count pawns from the vanilla expanded insectoid mod's new faction as insect kills
        public static void UpdateNewRecords_VEInsectoids(Pawn killed, Pawn killer)
        {
            FactionDef insectoidFaction = FactionDef.Named("VFEI_Insect");
            if (insectoidFaction == null)
            {
                DebugActions.LogErr("Could not find vanilla explanded insectoid faction def. This should never happen, since we null-checked this during startup before allowing this code to run.");
                return;
            }
            if (killed.Faction != null && killed.Faction.def.Equals(insectoidFaction))
            {
                killer.records.Increment(InsectKills);
            }
        }

        // Patch to count pawns from the Alpha Animals mod's black hive faction as insect kills
        public static void UpdateNewRecords_BlackHiveInsects(Pawn killed, Pawn killer)
        {
            FactionDef blackHiveFaction = FactionDef.Named("AA_BlackHive");
            if (blackHiveFaction == null)
            {
                DebugActions.LogErr("Could not find Alpha Animals Black Hive faction def. This should never happen, since we null-checked this during startup before allowing this code to run.");
                return;
            }
            if (killed.Faction != null && killed.Faction.def.Equals(blackHiveFaction))
            {
                killer.records.Increment(InsectKills);
            }
        }

    }     
}
