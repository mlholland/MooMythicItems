using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

/* The settings tab for this mod, will eventually include a bunch of controls.*/
namespace MooMythicItems
{
    public class MythicItemSettings : ModSettings
    {
        public bool flagCreateRandomMythicItemsIfNoneAvailable = true;
        public bool flagNotifyItemCreation = true;
        public bool flagDebug = false;
        public int minimumLevelForSkillItems = 18;
        public int mythicItemSaveLimit = 100;
        public int individualItemOccurenceLimit = 3;

        private string minimumLevelForSkillItemsString = "18";
        private string mythicItemSaveLimitInputString = "100";
        private string individualItemOccurenceLimitString = "3";


        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref flagCreateRandomMythicItemsIfNoneAvailable, "flagCreateRandomMythicItemsIfNoneAvailable", true, true);
            Scribe_Values.Look(ref flagNotifyItemCreation, "flagNotifyItemCreation", true, true);
            Scribe_Values.Look(ref flagDebug, "flagDebug", false, true);
            Scribe_Values.Look(ref minimumLevelForSkillItems, "minimumLevelForSkillItems", 18, true);
            Scribe_Values.Look(ref mythicItemSaveLimit, "mythicItemSaveLimit", 100, true);
            Scribe_Values.Look(ref mythicItemSaveLimitInputString, "mythicItemSaveLimit", "100", true);
            Scribe_Values.Look(ref individualItemOccurenceLimit, "individualItemOccurenceLimit", 3, true);
            Scribe_Values.Look(ref individualItemOccurenceLimitString, "individualItemOccurenceLimit", "3", true);
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.Label("MooMF_SettingsLabel".Translate());
            /*if (Widgets.ButtonText(new Rect(0f, ls.CurHeight, 180f, 29f), "MooMF_ManualSaveButton".Translate(), true, true, true))
            {
                Log.Message("save was pressed");
                MythicItemSaveUtility.SaveMythicItemsFile(testList);
            } 
            ls.Gap(30f);

            if (Widgets.ButtonText(new Rect(0f, ls.CurHeight, 180f, 29f), "MooMF_ManualLoadBUtton".Translate(), true, true, true))
            {
                Log.Message("load was pressed");
                List<MythicItem> loadedList = MythicItemSaveUtility.LoadMythicItemsFile();

                Log.Message(String.Format("loaded {0} items", loadedList.Count));
                foreach (MythicItem mi in loadedList)
                {
                    Log.Message(li.ToString());
                }
            } 
            ls.Gap(30f);
            */
            ls.CheckboxLabeled("MooMF_CreateRandomItemsIfNeeded".Translate(), ref flagCreateRandomMythicItemsIfNoneAvailable, "MooMF_CreateRandomItemsIfNeededTooltip".Translate());
            ls.CheckboxLabeled("MooMF_FlagNotifyItemCreation".Translate(), ref flagNotifyItemCreation, null);
            ls.CheckboxLabeled("MooMF_PrintDebugLogs".Translate(), ref flagDebug, null);
            ls.Label("MooMF_MinLevelForSkillItems".Translate() + ": " + minimumLevelForSkillItems, -1, "MooMF_MinLevelForSkillItemsToolTip".Translate());
            ls.IntEntry(ref minimumLevelForSkillItems, ref minimumLevelForSkillItemsString);
            ls.Label("MooMF_MaxMythicItemsSaved".Translate() + ": " + mythicItemSaveLimit, -1);
            ls.IntEntry(ref mythicItemSaveLimit, ref mythicItemSaveLimitInputString);
            ls.Label("MooMF_MaxItemOccurence".Translate() + ": " + individualItemOccurenceLimit, -1, "MooMF_MaxItemOccurenceTooltip".Translate());
            ls.IntEntry(ref individualItemOccurenceLimit, ref individualItemOccurenceLimitString);

            if (Widgets.ButtonText(new Rect(0f, ls.CurHeight, 180f, 29f), "MooMF_ClearAllSavedItemsButton".Translate(), true, true, true))
            {
                Log.Message("[Moo mythic items] Reset button pressed. Removing all cached mythic items, and clearing the mythic item save file.");
                MythicItemManager.ClearCacheAndSaveFile();
            }
            ls.Gap(30f);


            ls.End();

            base.Write();
        }
    }
}
