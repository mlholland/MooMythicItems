using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

/* This patch allows for custom kill-count records to be incremented.
 */
namespace MooMythicItems
{
    public class MythicMaker_Notify_PawnKilled
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

        private static RecordDef thrumboKills;
        private static RecordDef ThrumboKills
        {
            get
            {
                if (thrumboKills == null)
                {
                    thrumboKills = DefDatabase<RecordDef>.GetNamed("MooMF_killsThrumbos");
                }
                return thrumboKills;
            }
        }
        // Needed for determining what counts as a thrumbo in modded games.
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

        private static RecordDef leaderKills;
        private static RecordDef LeaderKills
        {
            get
            {
                if (leaderKills == null)
                {
                    leaderKills = DefDatabase<RecordDef>.GetNamed("MooMF_killsFactionLeaders");
                }
                return leaderKills;
            }
        }

        // Needs high priority to come before patches that read changed records 
        [HarmonyPatch(typeof(RecordsUtility), nameof(RecordsUtility.Notify_PawnKilled)), HarmonyPriority(Priority.High)]
        static class RecordsUtility_Notify_PawnKilled_PostResolve_Patch
        {
            static void Postfix(Pawn killed, Pawn killer)
            {
                // update custom records related to tracking certain mythic reasons
                UpdateNewRecords(killed, killer);
                runOnKillMythicEffects(killed, killer);
            }
        }

        private static void UpdateNewRecords(Pawn killed, Pawn killer)
        {
            if (killed.Faction != null && killed.Faction.def.Equals(FactionDefOf.Insect))
            {
                killer.records.Increment(InsectKills);
            }
            if (killed.RaceProps.Humanlike && killed.Faction != null && killed.Faction.leader != null && killed == killed.Faction.leader)
            {
                killer.records.Increment(LeaderKills);
            }
            else if (killed.RaceProps.Animal && ThrumboDefs.Contains(killed.def) && killer.Faction != killed.Faction)
            {
                killer.records.Increment(ThrumboKills);
            }
        }

        private static void runOnKillMythicEffects(Pawn killed, Pawn killer)
        {
            if (killer != null && killer.equipment != null && killer.equipment.Primary != null)
            {
                Thing killerWeapon = killer.equipment.Primary;
                CompMythic mythic = killerWeapon.TryGetComp<CompMythic>();
                if (mythic != null)
                {
                    mythic.DoOnKillEffects(killed, killer);
                }
            }
        }

    }     
}
