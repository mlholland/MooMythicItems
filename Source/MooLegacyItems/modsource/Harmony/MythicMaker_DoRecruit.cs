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
                        DebugActions.LogErr("tried and failed to retrieve list of animal types that are considered thrumbos for record-keeping purposes. Thrumbo tamings won't be recorded, nor will it be possible to produce thrumbo-taming-based mythic items. The def we were looking for was named: " + listDefName);
                        thrumboDefs = new List<ThingDef>();
                        return thrumboDefs;
                    }
                    thrumboDefs = thrumboDefList.values;
                }
                return thrumboDefs;
            }
        }

        // Needs high priority to come before patches that read changed records 
        [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt),
            nameof(InteractionWorker_RecruitAttempt.DoRecruit), 
            new Type[] { typeof(Pawn), typeof(Pawn), typeof(string), typeof(string), typeof(bool), typeof(bool) }, 
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal }),
            HarmonyPriority(Priority.High)]
        static class InteractionWorker_RecruitAttempt_Recruit_PostResolve_Patch
        {
            static void Postfix(Pawn recruiter, Pawn recruitee)
            {
                // increment the thrumbos tamed record if needed.
                if (ThrumboDefs.Contains(recruitee.def))
                {
                    recruiter.records.Increment(ThrumbosTamed);
                }
            }
        }
    }
}
