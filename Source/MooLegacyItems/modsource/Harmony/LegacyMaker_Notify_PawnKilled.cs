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

        private static RecordDef insectKills;
        private static RecordDef InsectKills
        {
            get
            {
                if (insectKills == null)
                {
                    insectKills = DefDatabase<RecordDef>.GetNamed("MooLI_killsInsects");
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
                    thrumboKills = DefDatabase<RecordDef>.GetNamed("MooLI_killsThrumbos");
                }
                return thrumboKills;
            }
        }
        private static List<ThingDef> thrumboDefs;
        private static List<ThingDef> ThrumboDefs
        {
            get
            {
                if (thrumboDefs == null)
                {
                    string listDefName = "MooLI_ThrumboDefList";
                    ThingDefListDef thrumboDefList = DefDatabase<ThingDefListDef>.GetNamed(listDefName);
                    if (thrumboDefList == null)
                    {
                        Log.Error("[Moo Legacy Items] tried and failed to retrieve list of animal types that are considered thrumbos for record-keeping purposes. Thrumbo kills won't be recorded, nor will it be possible to produce thrumbo-kill-based legacy items. The def we were looking for was named: " + listDefName);
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
                    leaderKills = DefDatabase<RecordDef>.GetNamed("MooLI_killsFactionLeaders");
                }
                return leaderKills;
            }
        }

        // Possible todo, make these configurable either as a def, or via settings
        private static readonly int leaderKillsThreshold1 = 3;
        private static readonly int thrumboKillsThreshold1 = 6;
        private static readonly int manyHumanKillsThreshold1 = 100;
        private static readonly int manyHumanKillsThreshold2 = 500;
        private static readonly int manyHumanKillsThreshold3 = 1000;
        private static readonly int manyInsectKillsThreshold1 = 200;
        private static readonly int manyInsectKillsThreshold2 = 800;
        private static readonly int manyMechKillsThreshold1 = 50;
        private static readonly int manyMechKillsThreshold2 = 300;

        private static readonly LegacyReasonToDetailOptionsDef details = LegacyReasonToDetailOptionsDef.Instance;

        [HarmonyPatch(typeof(RecordsUtility), nameof(RecordsUtility.Notify_PawnKilled))]
        static class RecordsUtility_Notify_PawnKilled_PostResolve_Patch
        {
            static void Postfix(Pawn killed, Pawn killer)
            {
                // update custom records related to tracking certain legacy reasons
                UpdateNewRecords(killed, killer);

                // make sure there's even a potential weapon to make into an heirloom. TODO should probably check other stuff, like not wood and not single use
                if (killer.equipment?.Primary == null)
                {
                    return;
                }

                LegacyItem newItem = TryCreatingNewLegacyItem(killed, killer);
                // create the legacy item if needed
                if (newItem != null)
                {
                    // double check that a more impressive kill count-based legacy item hasn't already been made for this pawn
                    LegacyItem cachedLegacyItem = LegacyItemManager.GetSimilarCachedLegacyItem(null, "kills", killer.ThingID, Find.World.info.persistentRandomValue);
                    if (cachedLegacyItem != null && !shouldReplaceLegacyItem(cachedLegacyItem.reason, newItem.reason))
                    {
                        return;
                    }
                    LegacyItemManager.SaveNewLegacyItem(newItem);
                }
            }
        }

        private static void UpdateNewRecords(Pawn killed, Pawn killer)
        {
            if (killedInsectFaction(killed, killer))
            {
                killer.records.Increment(InsectKills);
            }
            if (killedLeader(killed, killer))
            {
                killer.records.Increment(LeaderKills);
            }
            else if (killedNonAllyThrumbo(killed, killer))
            {
                killer.records.Increment(ThrumboKills);
            }
        }

        private static LegacyItem TryCreatingNewLegacyItem(Pawn killed, Pawn killer)
        {
            Thing item = killer.equipment.Primary;
            string reason = "";
            List<string> titles = null, descs = null;
            List<LegacyEffectDef> effects = null;

            bool isRanged = killer.equipment.Primary.def.IsRangedWeapon;
            RaceProperties raceProps = killed.RaceProps;

            if (raceProps.Humanlike)
            {
                if (killedLeader(killed, killer) && killer.records.GetValue(LeaderKills) == leaderKillsThreshold1)
                {
                    reason = createLegacyKillReason("leader", 3);
                    if (isRanged)
                    {
                        descs = details.LeaderSlayerRFD;
                        titles = details.LeaderSlayerRT;
                        effects = details.LeaderSlayerRA;
                    }
                    else
                    {
                        descs = details.LeaderSlayerMFD;
                        titles = details.LeaderSlayerMT;
                        effects = details.LeaderSlayerMA;
                    }
                }
                else if (killer.records.GetValue(RecordDefOf.KillsHumanlikes) == manyHumanKillsThreshold1)
                {
                    reason = createLegacyKillReason("humanoid", 1);
                    if (isRanged)
                    {
                        descs = details.manyKillsRFD;
                        titles = details.manyKillsRT;
                        effects = details.manyKillsRA;
                    }
                    else
                    {
                        descs = details.manyKillsMFD;
                        titles = details.manyKillsMT;
                        effects = details.manyKillsMA;
                    }
                }
                else if (killer.records.GetValue(RecordDefOf.KillsHumanlikes) == manyHumanKillsThreshold2)
                {
                    reason = createLegacyKillReason("humanoid", 2);
                    if (isRanged)
                    {
                        descs = details.moreKillsRFD;
                        titles = details.moreKillsRT;
                        effects = details.moreKillsRA;
                    }
                    else
                    {
                        descs = details.moreKillsMFD;
                        titles = details.moreKillsMT;
                        effects = details.moreKillsMA;
                    }
                }
                else if (killer.records.GetValue(RecordDefOf.KillsHumanlikes) == manyHumanKillsThreshold3)
                {
                    reason = createLegacyKillReason("humanoid", 3);
                    if (isRanged)
                    {
                        descs = details.mostKillsRFD;
                        titles = details.mostKillsRT;
                        effects = details.mostKillsRA;
                    }
                    else
                    {
                        descs = details.mostKillsMFD;
                        titles = details.mostKillsMT;
                        effects = details.mostKillsMA;
                    }
                }
            }
            else if (killedInsectFaction(killed, killer))
            {
                if (killer.records.GetValue(InsectKills) == manyInsectKillsThreshold1)
                {
                    reason = createLegacyKillReason("insect", 2);
                    if (isRanged)
                    {
                        descs = details.manyInsectKillsRFD;
                        titles = details.manyInsectKillsRT;
                        effects = details.manyInsectKillsRA;
                    }
                    else
                    {
                        descs = details.manyInsectKillsMFD;
                        titles = details.manyInsectKillsMT;
                        effects = details.manyInsectKillsMA;
                    }
                }
            }
            else if (killedNonAllyThrumbo(killed, killer))
            {
                if (killer.records.GetValue(InsectKills) == thrumboKillsThreshold1)
                {
                    reason = createLegacyKillReason("thrumbo", 2);
                    if (isRanged)
                    {
                        descs = details.ThrumboSlayerRFD;
                        titles = details.ThrumboSlayerRT;
                        effects = details.ThrumboSlayerRA;
                    }
                    else
                    {
                        descs = details.ThrumboSlayerMFD;
                        titles = details.ThrumboSlayerMT;
                        effects = details.ThrumboSlayerMA;
                    }
                }
            }
            else if (raceProps.IsMechanoid)
            {
                float mechKills = killer.records.GetValue(RecordDefOf.KillsMechanoids);
                if (mechKills == manyMechKillsThreshold1)
                {
                    reason = createLegacyKillReason("mech", 2);
                    if (isRanged)
                    {
                        descs = details.manyMechKillsRFD;
                        titles = details.manyMechKillsRT;
                        effects = details.manyMechKillsRA;
                    }
                    else
                    {
                        descs = details.manyMechKillsMFD;
                        titles = details.manyMechKillsMT;
                        effects = details.manyMechKillsMA;
                    }
                }
                /*else if (mechKills == s_manyMechKillsThreshold2)
                {
                    trySaveItem = true;
                    reason = "mech-kills-2";
                    if (isRanged)
                    {
                        descs = details.mech;
                        titles = details.moreMechKillsRT;
                        effects = details.moreMechKillsRA;
                    }
                    else
                    {
                        descs = details.moreMechKillsMFD;
                        titles = details.moreMechKillsMT;
                        effects = details.manyMechKillsMA;
                    }
                } */
            }

            // create the legacy item if needed
            if (titles != null)
            {
                return new LegacyItem(item, killer, descs.RandomElement(), titles.RandomElement(), effects.RandomElement(), reason);
            }
            return null;
        }


        private static string createLegacyKillReason(string detail, int priority)
        {
            if (priority < 1)
            {
                string result = detail + "-" + "kills" + "-1";
                Log.Error(String.Format("[Moo Legacy Items] tried to create a legacy kill reason string with a non-positive priority value {0}. Defaulting to priority 1 to create reason '{1}'", priority, result));
            }
            return detail + "-" + "kills" + "-" + priority;
        }

        // used to determine order of importance among various kill count resons
        // Returns true if the new reason's ending integer value is greater than the previous, and false for all other reasons
        // Ex: humanoid-kills-1 is replaced by humanoid-kills-2, but it is also replaced by thrumbo-kills-2, which is technically the first tier of thrumbo kills
        //
        private static bool shouldReplaceLegacyItem(string oldReason, string newReason)
        {
            string[] oldSplit = oldReason.Split('-');
            if (oldSplit.Length != 3)
            {
                Log.Error(String.Format("[Moo Legacy Items] tried to compare the creation reasons for two kill-based legacy items, but the old reason '{0}' does not follow the expected format of '<detail>-kills-<priority>'. Defaulting to false - do not replace.", oldReason));
                return false;
            }
            int oldPrio = 0;
            bool parsed = int.TryParse(oldSplit[2], out oldPrio);
            if (!parsed)
            {
                Log.Error(String.Format("[Moo Legacy Items] tried to compare the creation reasons for two kill-based legacy items, but the old reason '{0}' does not follow the expected format of '<detail>-kills-<priority>'. Defaulting to false - do not replace.", oldReason));
                return false;
            }

            string[] newSplit = newReason.Split('-');
            if (newSplit.Length != 3)
            {
                Log.Error(String.Format("[Moo Legacy Items] tried to compare the creation reasons for two kill-based legacy items, but the new reason '{0}' does not follow the expected format of '<detail>-kills-<priority>'. Defaulting to false - do not replace.", newReason));
                return false;
            }
            int newPrio = 0;
            parsed = int.TryParse(newSplit[2], out newPrio);
            if (!parsed)
            {
                Log.Error(String.Format("[Moo Legacy Items] tried to compare the creation reasons for two kill-based legacy items, but the new reason '{0}' does not follow the expected format of '<detail>-kills-<priority>'. Defaulting to false - do not replace.", newReason));
                return false;
            }

            return newPrio > oldPrio;
        }

        private static bool killedLeader(Pawn killed, Pawn killer)
        {
            return killed.RaceProps.Humanlike && killed.Faction != null && killed.Faction.leader != null && killed == killed.Faction.leader;
        }

        private static bool killedInsectFaction(Pawn killed, Pawn killer)
        {
            return killed.Faction != null && killed.Faction.def.Equals(FactionDefOf.Insect);
        }

        private static bool killedNonAllyThrumbo(Pawn killed, Pawn killer)
        {
            return killed.RaceProps.Animal && ThrumboDefs.Contains(killed.def) && killer.Faction != killed.Faction;
        }

    }     
}
