using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using HarmonyLib;

/* Worker class emplyed by some MythicCauseDef_RecordThreshold to determine if a pawn has killed a sufficient number of specific types
 * of creatures to merit a mythic item.
 */
namespace MooMythicItems
{
    public class CauseWorker_Kills : CauseWorker
    {

        public static readonly string killReasonPrefix = "kills-";
        private static readonly string printReasonKey = "MooMF_PrintKillCountReason";
        public static Dictionary<RecordDef, List<MythicCauseDef_RecordThreshold>> recordsWatched = new Dictionary<RecordDef, List<MythicCauseDef_RecordThreshold>>();

        public CauseWorker_Kills(MythicCauseDef def) : base(def) { }

        public override void EnableCauseRecognition(Harmony harm)
        {
            base.EnableCauseRecognition(harm);
            MythicCauseDef_RecordThreshold causeDef = def as MythicCauseDef_RecordThreshold;
            if (causeDef == null)
            {
                DebugActions.LogErr("Kill-recording cause worker was supplied a mythic cause def {0} that wasn't the RecordThreshold sub-type." +
                    " Use a different worker class, or correct the def class.", def.defName);
                return;
            }
            if (!recordsWatched.ContainsKey(causeDef.record))
            {
                recordsWatched.Add(causeDef.record, new List<MythicCauseDef_RecordThreshold>());
            }
            else
            {
                foreach (MythicCauseDef_RecordThreshold savedCause in recordsWatched[causeDef.record])
                {
                    if (savedCause.threshold == causeDef.threshold)
                    {
                        DebugActions.LogErr("Encountered error while loading mythic item creation causes. Two kill count-based causes, '{0}' and '{1}' have the same threshold on the same record." +
                            " The second cause '{1}' will be ignored.", savedCause.defName, causeDef.defName);
                        return;

                    }
                }
            }
            recordsWatched[causeDef.record].Add(causeDef);
        }

        public override string GetReasonFragmentKey()
        {
            return printReasonKey;
        }

        [HarmonyPatch(typeof(RecordsUtility), nameof(RecordsUtility.Notify_PawnKilled))]
        static class RecordsUtility_Notify_PawnKilled_Patch
        {
            /* Prefix records the value of each watched record before the notify event possibly updates them
             */
            public static void Prefix(ref Dictionary<RecordDef, int> __state, Pawn killed, Pawn killer)
            {
                __state = new Dictionary<RecordDef, int>();
                foreach (RecordDef record in recordsWatched.Keys)
                {
                    __state.Add(record, killer.records.GetAsInt(record));
                }
            }

            /* postfix compares current values to those recorded by prefix, and if any were incremented
             * up to a threshold, it tries to create a mythic item.*/
             // TODO should probably extract the whole priority logic out into the parent causeworker for general access
            public static void Postfix(ref Dictionary<RecordDef, int> __state, Pawn killed, Pawn killer)
            {
                Tuple<MythicItem, MythicCauseDef_RecordThreshold> creationResult = CreateMythicItemIfCauseMet(__state, killer);
                if (creationResult != null && creationResult.Item1 != null && creationResult.Item2 != null)
                {
                    MythicItemCache.TrySaveOrOverwriteNewItem(creationResult.Item1, killer, killReasonPrefix, creationResult.Item2.priority, creationResult.Item2.reasonLimit, creationResult.Item2.GetPrintedReasonFragment(creationResult.Item1.ownerFullName));
                }
            }
        }
        
        private static Tuple<MythicItem, MythicCauseDef_RecordThreshold> CreateMythicItemIfCauseMet(Dictionary<RecordDef, int> prefixValues, Pawn killer)
        {
            int bestPrio = -1;
            MythicCauseDef_RecordThreshold bestCause = null;

            foreach (KeyValuePair<RecordDef, List<MythicCauseDef_RecordThreshold>> kv in recordsWatched)
            {
                int prefixVal = prefixValues[kv.Key];
                int curVal = killer.records.GetAsInt(kv.Key);
                // Check if this record was incremented, and compare the new value to relevant thresholds if so.
                if (curVal - prefixVal == 1)
                {
                    foreach (MythicCauseDef_RecordThreshold causeDef in kv.Value)
                    {
                        if (curVal == causeDef.threshold && causeDef.priority > bestPrio)
                        {
                            bestPrio = causeDef.priority;
                            bestCause = causeDef;
                        }
                    }
                }
            }

            if (bestCause != null)
            {
                return new Tuple<MythicItem, MythicCauseDef_RecordThreshold>(bestCause.TryCreateMythicItem(killer, killReasonPrefix + bestCause.subreason), bestCause);
            }
            return new Tuple<MythicItem, MythicCauseDef_RecordThreshold>(null, null);
        }



    }
}
