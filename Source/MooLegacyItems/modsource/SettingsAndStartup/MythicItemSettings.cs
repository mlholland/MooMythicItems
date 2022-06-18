using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

/* The settings tab for this mod.*/
namespace MooMythicItems
{
    public class MythicItemSettings : ModSettings
    {
        public bool flagCreateRandomMythicItemsIfNoneAvailable = false;
        public bool flagNotifyItemCreation = true;
        public bool flagNotifyItemDeletion = true;
        public bool flagMythicOwnersCanCreateNewMythicItems = false;
        public bool flagDebug = false;
        public bool flagStartupDebug = false;
        // public int minimumLevelForSkillItems = 18;
        public int mythicItemSaveLimit = 100;
        public int individualItemOccurenceLimit = 3;
        public float killCountScaling = 1;
        public bool flagShowKillThresholds = false;
        public bool flagShowClearButton = false;

        // private string minimumLevelForSkillItemsString = "18";
        private string mythicItemSaveLimitInputString = "100";
        private string individualItemOccurenceLimitString = "3";
        

        // for a scrollbar
        private float lastHeight = float.MaxValue;
        private Vector2 scrollPos;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref flagCreateRandomMythicItemsIfNoneAvailable, "flagCreateRandomMythicItemsIfNoneAvailable", false, true);
            Scribe_Values.Look(ref flagNotifyItemCreation, "flagNotifyItemCreation", true, true);
            Scribe_Values.Look(ref flagNotifyItemDeletion, "flagNotifyItemDeletion", true, true);
            Scribe_Values.Look(ref flagMythicOwnersCanCreateNewMythicItems, "flagMythicOwnersCanCreateNewMythicItems", false, true);
            Scribe_Values.Look(ref flagDebug, "flagDebug", false, true);
            Scribe_Values.Look(ref flagStartupDebug, "flagStartupDebug", false, true);
            // Scribe_Values.Look(ref minimumLevelForSkillItems, "minimumLevelForSkillItems", 18, true);
            Scribe_Values.Look(ref mythicItemSaveLimit, "mythicItemSaveLimit", 100, true);
            Scribe_Values.Look(ref mythicItemSaveLimitInputString, "mythicItemSaveLimit", "100", true);
            Scribe_Values.Look(ref individualItemOccurenceLimit, "individualItemOccurenceLimit", 3, true);
            Scribe_Values.Look(ref individualItemOccurenceLimitString, "individualItemOccurenceLimit", "3", true);
            Scribe_Values.Look(ref killCountScaling, "killCountScaling", 1, true);
            Scribe_Values.Look(ref flagShowKillThresholds, "flagShowKillThresholds", false, true);
            Scribe_Values.Look(ref flagShowClearButton, "flagShowClearButton", false, true);
        }

        public void DoWindowContents(Rect inRect)
        {
            base.Write();
            var viewRect = new Rect(0, 0, inRect.width - 20f /* this is to have space for the scrollbar */, lastHeight);
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
            // Do stuff
            //lastHeight = 100f /* height of stuff */ 
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(viewRect);
            
            ls.Label("MooMF_SettingsLabel".Translate());
            // notification and basic behavior flags
            ls.CheckboxLabeled("MooMF_CreateRandomItemsIfNeeded".Translate(), ref flagCreateRandomMythicItemsIfNoneAvailable, "MooMF_CreateRandomItemsIfNeededTooltip".Translate());
            ls.CheckboxLabeled("MooMF_FlagNotifyItemCreation".Translate(), ref flagNotifyItemCreation, "MooMF_FlagNotifyItemCreationTooltip".Translate());
            ls.CheckboxLabeled("MooMF_FlagNotifyItemDeletion".Translate(), ref flagNotifyItemDeletion, "MooMF_FlagNotifyItemDeletionTooltip".Translate());
            ls.CheckboxLabeled("MooMF_FlagMythicOwnersCanCreateNewMythicItems".Translate(), ref flagMythicOwnersCanCreateNewMythicItems, "MooMF_FlagMythicOwnersCanCreateNewMythicItemsToolTip".Translate());
            // not currently used, level thresholds are set in the cause defs.
            //ls.Label("MooMF_MinLevelForSkillItems".Translate() + ": " + minimumLevelForSkillItems, -1, "MooMF_MinLevelForSkillItemsToolTip".Translate());
            //ls.IntEntry(ref minimumLevelForSkillItems, ref minimumLevelForSkillItemsString);
            ls.Label("MooMF_MaxMythicItemsSaved".Translate() + ": " + mythicItemSaveLimit, -1);
            ls.IntEntry(ref mythicItemSaveLimit, ref mythicItemSaveLimitInputString);
            ls.Label("MooMF_MaxItemOccurence".Translate() + ": " + individualItemOccurenceLimit, -1, "MooMF_MaxItemOccurenceTooltip".Translate());
            ls.IntEntry(ref individualItemOccurenceLimit, ref individualItemOccurenceLimitString);

            // kill count scaling and showing
            ls.Label("MooMF_KillCountScaling".Translate() + ": " + killCountScaling, -1, "MooMF_KillCountScalingTooltip".Translate());
            killCountScaling = (float)Math.Round(ls.Slider(killCountScaling, 0.1f, 5f), 2);
            ls.CheckboxLabeled("MooMF_ShowKillThresholds".Translate(), ref flagShowKillThresholds, "MooMF_ShowKillThresholdsToolTip".Translate());
            if (flagShowKillThresholds)
            {
                foreach (MythicCauseDef_RecordThreshold def in DefDatabase<MythicCauseDef_RecordThreshold>.AllDefs)
                {
                    if (def.workerClass == typeof(CauseWorker_Kills))
                    {
                        ls.Label(string.Format("{0}: {1}", def.label, CauseWorker_Kills.ApplySettingsScalingToThreshold(def.threshold)));
                    }
                }
            }
            // show extra logs
            ls.CheckboxLabeled("MooMF_PrintDebugLogs".Translate(), ref flagDebug, "MooMF_PrintDebugLogsTooltip".Translate());
            ls.CheckboxLabeled("MooMF_PrintExtraStartupDebugLogs".Translate(), ref flagStartupDebug, "MooMF_PrintExtraStartupDebugLogsTooltip".Translate());



            ls.Label("MooMF_ResetSettingsLabel".Translate(), -1, "MooMF_ResetSettingsLabelTooltip".Translate());
            if (Widgets.ButtonText(new Rect(0f, ls.CurHeight, 180f, 29f), "MooMF_ResetSettingsButton".Translate(), true, true, true))
            {
                DebugActions.LogIfDebug("Resetting Settings");
                Messages.Message("MooMF_ResetSettingsPressed".Translate(), MessageTypeDefOf.PositiveEvent, true);
                ResetSettings();
            }
            ls.Gap(30f);
            ls.Label("MooMF_LoadInvalidCacheLabel".Translate(), -1, "MooMF_LoadInvalidCacheTooltip".Translate());
            if (Widgets.ButtonText(new Rect(0f, ls.CurHeight, 180f, 29f), "MooMF_LoadInvalidCache".Translate(), true, true, true))
            {
                MythicItemCache.TryLoadingInvalidItems();
            }
            ls.Gap(30f);

            // danger zone - file cleanres
            ls.Label("MooMF_DeleteWarning".Translate(), -1, "MooMF_ClearAllSavedItemsDescription".Translate());
            ls.CheckboxLabeled("MooMF_ShowClearButton".Translate(), ref flagShowClearButton, "MooMF_ShowClearButtonTooltip".Translate());
            if (flagShowClearButton) {
                if (Widgets.ButtonText(new Rect(0f, ls.CurHeight, 180f, 29f), "MooMF_ClearAllSavedItemsButton".Translate(), true, true, true))
                {
                    DebugActions.LogIfDebug("MooMF_ClearSavedItemsButtonPressed".Translate());
                    Messages.Message("MooMF_ClearSavedItemsButtonPressed".Translate(), MessageTypeDefOf.PositiveEvent, true);
                    MythicItemCache.ClearCacheAndSaveFile();
                }
                ls.Gap(45f);
                if (Widgets.ButtonText(new Rect(0f, ls.CurHeight, 180f, 29f), "MooMF_ClearAllInvalidItemsButton".Translate(), true, true, true))
                {
                    DebugActions.LogIfDebug("MooMF_ClearInvalidSavedItemsButtonPressed".Translate());
                    Messages.Message("MooMF_ClearInvalidSavedItemsButtonPressed".Translate(), MessageTypeDefOf.PositiveEvent, true);
                    MythicItemCache.ClearInvalidSaveFile();
                }
                ls.Gap(30f);
            }


            lastHeight = 1000; // TODO figure out how to properly set this to the internal window height and not have it collapse it on itself...
            ls.End();
            Widgets.EndScrollView();
        }

        private void ResetSettings()
        {
            flagCreateRandomMythicItemsIfNoneAvailable = false;
            flagNotifyItemCreation = true;
            flagNotifyItemDeletion = true;
            flagMythicOwnersCanCreateNewMythicItems = false;
            flagDebug = false;
            flagStartupDebug = false;
            // public int minimumLevelForSkillItems = 18;
            mythicItemSaveLimit = 100;
            individualItemOccurenceLimit = 3;
            killCountScaling = 1;
            flagShowKillThresholds = false;
            flagShowClearButton = false;

            // private string minimumLevelForSkillItemsString = "18";
            mythicItemSaveLimitInputString = "100";
            individualItemOccurenceLimitString = "3";
        }
    }
}
