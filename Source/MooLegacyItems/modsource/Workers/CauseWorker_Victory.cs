using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using HarmonyLib;

/* Worker class emplyed by some MythicCauseDef_RecordThreshold to determine if a pawn has recruited or tamed a sufficient number of people 
 * or animals (as recognized by a supplied RecordDef related to taming or recruiting), and if so, creates a mythic item for the occasion.
 * More or less identical to CauseWorker_Kills, except for the function that is patched to watch for value changes.
 */
namespace MooMythicItems
{
    public class CauseWorker_Victory : CauseWorker
    {
        // todo maybe add subreason for victory type
        public static readonly string victoryPrefix = "victory";

        private static readonly string space = "GameOverColonistsEscaped";
        private static readonly string royal = "EndGameIntroText"; // From Script_EndGame_RoyalAscent - will need to test this.
        private static readonly string archo = "GameOverColonistsTranscended";
        private static readonly string printReasonKey = "MooMF_PrintVictoryReason";

        public static Dictionary<string, List<MythicCauseDef_Victory>> victoryToTriggerMap = new Dictionary<string, List<MythicCauseDef_Victory>>();
        public CauseWorker_Victory(MythicCauseDef def) : base(def) { }

        // TODO consider abstracting int-record-based cause workers to have a shared parent that sets up the recordsWatched dictionary.
        public override void EnableCauseRecognition(Harmony harm)
        {
            base.EnableCauseRecognition(harm);
            if (!victoryToTriggerMap.ContainsKey(space))
            {
                victoryToTriggerMap[space] = new List<MythicCauseDef_Victory>();
            }
            if (!victoryToTriggerMap.ContainsKey(royal))
            {
                victoryToTriggerMap[royal] = new List<MythicCauseDef_Victory>();
            }
            if (!victoryToTriggerMap.ContainsKey(archo))
            {
                victoryToTriggerMap[archo] = new List<MythicCauseDef_Victory>();
            }


            addCauseToVictoryMap(def as MythicCauseDef_Victory);
        }

        private static void addCauseToVictoryMap(MythicCauseDef_Victory causeDef)
        {
            if (causeDef == null)
            {
                return;
            }
            if (causeDef.spaceVictory)
            {
                victoryToTriggerMap[space].Add(causeDef);
            }
            if (causeDef.royalVictory)
            {
                victoryToTriggerMap[royal].Add(causeDef);
            }
            if (causeDef.archoVictory)
            {
                victoryToTriggerMap[archo].Add(causeDef);
            }
        }

        public override string GetReasonFragmentKey()
        {
            return printReasonKey;
        }

        [HarmonyPatch(typeof(GameVictoryUtility), nameof(GameVictoryUtility.MakeEndCredits))]
        static class GameVictoryUtility_Postfix_Patch
        {
            public static void Postfix(IList<Pawn> escaped , string colonistsEscapeeTKey)
            {
                MythicCauseDef_Victory causeDef = GetValidCauseDef(colonistsEscapeeTKey);
                if (causeDef == null)
                {
                    return;
                }
                // pick a valid pawn
                List<Pawn> options = new List<Pawn>();
                if (escaped != null && escaped.Count > 0)
                {
                    foreach (Pawn p in escaped)
                    {
                        if (!p.NonHumanlikeOrWildMan() && p.IsColonist)
                        {
                            options.Add(p);
                        }
                    }
                }
                else
                {
                    foreach (Pawn p in PawnsFinder.AllMaps_FreeColonists)
                    {
                        options.Add(p);
                    }
                }
                
                if (options.Count == 0)
                {
                    return;
                }
                Pawn selectedPawn = options.RandomElement();
                MythicItem newItem = causeDef.TryCreateMythicItem(selectedPawn, victoryPrefix);
                if (newItem != null)
                {
                    MythicItemCache.TrySaveOrOverwriteNewItem(newItem, selectedPawn, null, 0, 0, causeDef.GetPrintedReasonFragment(newItem.ownerFullName));
                }
            }

            // TODO Royal and Space victories have same escape key - need further differention somehow.
            private static MythicCauseDef_Victory GetValidCauseDef(string colonistsEscapeeTKey)
            {
                if (space == colonistsEscapeeTKey)
                {
                    if (victoryToTriggerMap.ContainsKey(space) && victoryToTriggerMap[space].Count > 0)
                    {
                        return victoryToTriggerMap[space].RandomElement();
                    }
                    return null;
                }
                if (archo == colonistsEscapeeTKey)
                {
                    if (victoryToTriggerMap.ContainsKey(archo) && victoryToTriggerMap[archo].Count > 0)
                    {
                        return victoryToTriggerMap[archo].RandomElement();
                    }
                    return null;
                } else // does royalty have a translation string for victory? Where is it?
                {
                    if (victoryToTriggerMap.ContainsKey(royal) && victoryToTriggerMap[royal].Count > 0)
                    {
                        return victoryToTriggerMap[royal].RandomElement();
                    }
                    return null;
                }
                return null;
            }
        }
        

    }
}
