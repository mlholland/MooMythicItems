using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

/* The settings tab for this mod, will eventually include a bunch of controls.*/
namespace MooLegacyItems
{
    public class LegacyItemSettings : ModSettings
    {
        public bool flagCreateRandomLegacyItemsIfNoneAvailable = true;
        public bool flagDebug = false;
        public int minimumLevelForSkillItems = 18;
        public int legacyItemSaveLimit = 100;
        public int individualItemOccurenceLimit = 3;

        private string minimumLevelForSkillItemsString = "18";
        private string legacyItemSaveLimitInputString = "100";
        private string individualItemOccurenceLimitString = "3";


        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref flagCreateRandomLegacyItemsIfNoneAvailable, "flagCreateRandomLegacyItemsIfNoneAvailable", true, true);
            Scribe_Values.Look(ref flagDebug, "flagDebug", false, true);
            Scribe_Values.Look(ref minimumLevelForSkillItems, "minimumLevelForSkillItems", 18, true);
            Scribe_Values.Look(ref legacyItemSaveLimit, "legacyItemSaveLimit", 100, true);
            Scribe_Values.Look(ref legacyItemSaveLimitInputString, "legacyItemSaveLimit", "100", true);
            Scribe_Values.Look(ref individualItemOccurenceLimit, "individualItemOccurenceLimit", 3, true);
            Scribe_Values.Look(ref individualItemOccurenceLimitString, "individualItemOccurenceLimit", "3", true);
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.Label("MooLI_SettingsLabel".Translate());
            /*if (Widgets.ButtonText(new Rect(0f, ls.CurHeight, 180f, 29f), "MooLI_ManualSaveButton".Translate(), true, true, true))
            {
                Log.Message("save was pressed");
                LegacyItemSaveUtility.SaveLegacyItemsFile(testList);
            } 
            ls.Gap(30f);

            if (Widgets.ButtonText(new Rect(0f, ls.CurHeight, 180f, 29f), "MooLI_ManualLoadBUtton".Translate(), true, true, true))
            {
                Log.Message("load was pressed");
                List<LegacyItem> loadedList = LegacyItemSaveUtility.LoadLegacyItemsFile();

                Log.Message(String.Format("loaded {0} items", loadedList.Count));
                foreach (LegacyItem li in loadedList)
                {
                    Log.Message(li.ToString());
                }
            } 
            ls.Gap(30f);
            */
            ls.CheckboxLabeled("MooLI_CreateRandomItemsIfNeeded".Translate(), ref flagCreateRandomLegacyItemsIfNoneAvailable, "MooLI_CreateRandomItemsIfNeededTooltip".Translate());
            ls.CheckboxLabeled("MooLI_PrintDebugLogs".Translate(), ref flagDebug, null);
            ls.Label("MooLI_MinLevelForSkillItems".Translate() + ": " + minimumLevelForSkillItems, -1, "MooLI_MinLevelForSkillItemsToolTip".Translate());
            ls.IntEntry(ref minimumLevelForSkillItems, ref minimumLevelForSkillItemsString);
            ls.Label("MooLI_MaxLegacyItemsSaved".Translate() + ": " + legacyItemSaveLimit, -1);
            ls.IntEntry(ref legacyItemSaveLimit, ref legacyItemSaveLimitInputString);
            ls.Label("MooLI_MaxItemOccurence".Translate() + ": " + individualItemOccurenceLimit, -1, "MooLI_MaxItemOccurenceTooltip".Translate());
            ls.IntEntry(ref individualItemOccurenceLimit, ref individualItemOccurenceLimitString);

            if (Widgets.ButtonText(new Rect(0f, ls.CurHeight, 180f, 29f), "MooLI_ClearAllSavedItemsButton".Translate(), true, true, true))
            {
                Log.Message("Moo legacy items: Reset button pressed. Removing all cached legacy items, and blanking the legacy item save file.");
                LegacyItemManager.ClearCacheAndSaveFile();
            }
            ls.Gap(30f);


            ls.End();

            base.Write();
        }
    }
}
