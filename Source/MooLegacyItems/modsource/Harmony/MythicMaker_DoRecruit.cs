using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

/* This patch runs after InteractionWorker_RecruitAttempt.DoRecruit to see if the recruited thing was a thrumbo (or anything we consider a thrumbo according to a def list).
 * If so, it updates the tamer's records and possibly creates a legacy item based on it.
 */
namespace MooMythicItems
{
    public class MythicMaker_DoRecruit
    {

        private static readonly string thrumboTameReason = "taming_thrumbos";

        private static RecordDef thrumbosTamed;
        private static RecordDef ThrumbosTamed
        {
            get
            {
                if (thrumbosTamed == null)
                {
                    thrumbosTamed = DefDatabase<RecordDef>.GetNamed("MooMF_thrumbosTamed");
                }
                return thrumbosTamed;
            }
        }

        private static List<ThingDef> thrumboDefs;
        private static List<ThingDef> ThrumboDefs
        {
            get
            {
                if (thrumboDefs == null)
                {
                    string listDefName = "MooMF_ThrumboDefList";
                    ThingDefListDef thrumboDefList = DefDatabase<ThingDefListDef>.GetNamed(listDefName, false);
                    if (thrumboDefList == null)
                    {
                        Log.Error("[Moo Mythic Items] tried and failed to retrieve list of animal types that are considered thrumbos for record-keeping purposes. Thrumbo tamings won't be recorded, nor will it be possible to produce thrumbo-taming-based mythic items. The def we were looking for was named: " + listDefName);
                        thrumboDefs = new List<ThingDef>();
                        return thrumboDefs;
                    }
                    thrumboDefs = thrumboDefList.values;
                }
                return thrumboDefs;
            }
        }

        // Possible todo, make these configurable either as a def, or via settings
        private static readonly int thrumboTamingThreshold = 3;

        private static readonly MythicReasonToDetailOptionsDef details = MythicReasonToDetailOptionsDef.Instance;

        [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt),
            nameof(InteractionWorker_RecruitAttempt.DoRecruit), 
            new Type[] { typeof(Pawn), typeof(Pawn), typeof(string), typeof(string), typeof(bool), typeof(bool) }, 
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
        static class InteractionWorker_RecruitAttempt_Recruit_PostResolve_Patch
        {
            static void Postfix(Pawn recruiter, Pawn recruitee)
            {
                // increment the thrumbos tamed record if needed.
                if (ThrumboDefs.Contains(recruitee.def))
                {
                    recruiter.records.Increment(ThrumbosTamed);
                    // create a mythic item if...
                    // This pawn has tamed the required number of thrumbos
                    // There isn't already a mythic item related to thrumbo-taming from any world
                    if (recruiter.records.GetValue(ThrumbosTamed) == thrumboTamingThreshold
                        && MythicItemManager.GetSimilarCachedMythicItem(null, thrumboTameReason, null, 0) == null
                        && recruiter.apparel.WornApparel.Count != 0)
                    {
                        MythicItemManager.SaveNewMythicItem(new MythicItem(recruiter.apparel.WornApparel.RandomElement(),
                            recruiter,
                            details.ThrumboFriendFD.RandomElement(),
                            details.ThrumboFriendT.RandomElement(),
                            details.ThrumboFriendA.RandomElement(),
                            thrumboTameReason));

                    }
                }

            }
        }
    }
}
