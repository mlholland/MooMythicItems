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
        public bool flagCreateDefaultLegacyItemsIfNoneAvailable = true;
        public int legacyItemSaveLimit = 100;

        private string legacyItemSaveLimitInputString = "100";


        private static List<LegacyItem> testList = new List<LegacyItem> {
            new LegacyItem("stick", "john", "john's world", "john killed 100 people", "+100 damage"),
            new LegacyItem("gun", "jimbo", "jimbo's vengeance", "lvl 20 plants", "+50 plant work speed")
        };

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref flagCreateDefaultLegacyItemsIfNoneAvailable, "flagCreateDefaultLegacyItemsIfNoneAvailable", true, true);
            Scribe_Values.Look(ref legacyItemSaveLimit, "legacyItemSaveLimit", 100, true);
            Scribe_Values.Look(ref legacyItemSaveLimitInputString, "legacyItemSaveLimit", "100", true);
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
            ls.Label("MooLI_MaxLegacyItemsSaved".Translate() + ": " + legacyItemSaveLimit, -1);
            ls.IntEntry(ref legacyItemSaveLimit, ref legacyItemSaveLimitInputString);

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
