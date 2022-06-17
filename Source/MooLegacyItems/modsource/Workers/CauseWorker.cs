using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using HarmonyLib;

/* Worker class employed by MythicCauseDefs to setup the checks needed to identify when a mythic item should be created.
 * 
 * ... Or something, I'll still not certain that my use case - requiring new patches for each cause def, merits a separate worker class.
 */
namespace MooMythicItems
{
    public abstract class CauseWorker 
    {
        public MythicCauseDef def;

        public CauseWorker(MythicCauseDef def) => this.def = def;

        // Run at the end of startup to setup necessary checks to recognize when a mythic item cause has been achieved, and
        // To create a mythic item when that occurs.
        // A harmony instance is required as input, since this is very likely to require harmony patches of some sort.
        public virtual void EnableCauseRecognition(Harmony harm)
        {
            if(MooMythicItems_Mod.settings.flagStartupDebug)
            {
                DebugActions.MooLog(String.Format("Printing example mythic text for mythic cause '{0}'", def.defName));
                String exampleItemName = def.createsMythicWeapon ? "Charge Rifle" : "Flak Vest";
                String exampleOwnerName = "Randy";
                String exampleFullName = "Randy Random";
                String exampleFaction = "Penumbria's Survivors";
                foreach (String title in def.titles)
                {
                    DebugActions.MooLog("Name: " + String.Format(title.Translate(), exampleOwnerName, exampleItemName));
                }
                foreach (String desc in def.descriptions)
                {
                    DebugActions.MooLog("Description: " + String.Format(desc.Translate(), exampleFullName, exampleOwnerName, exampleFaction,  exampleItemName));
                }
                DebugActions.MooLog("Reason Fragment: " +  GetPrintedReasonFragment(exampleOwnerName));
                // TODO add effect translation in child implementations
                if (def.hasDifferentMeleeOptions)
                {
                    exampleItemName = "Steel Longsword";
                    foreach (String title in def.meleeTitles)
                    {
                         DebugActions.MooLog("Name: " + String.Format(title.Translate(), exampleOwnerName, exampleItemName));
                    }
                    foreach (String desc in def.meleeDescriptions)
                    {
                        DebugActions.MooLog("Description: " + String.Format(desc.Translate(), exampleFullName, exampleOwnerName, exampleFaction, exampleItemName));
                    }
                }
            }
        }

        /** This string should be a sentence fragment, which will be injected into the printed statement about a mythic item being produced */
        public virtual String GetPrintedReasonFragment(params object[] args)
        {
            string translationKey = this.GetReasonFragmentKey();
            if (translationKey == null)
            {
                DebugActions.LogErr("Causeworker is returning a null format key. That shouldn't happen.");
                return null;
            }
            return String.Format(translationKey.Translate(), args);
        }

        public abstract string GetReasonFragmentKey();
    }
}
