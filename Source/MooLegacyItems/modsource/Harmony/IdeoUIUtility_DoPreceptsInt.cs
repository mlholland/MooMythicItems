using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

/* This patch adds additional UI elements to the Ideologion editing screen. Specifically it adds a few buttons right after the relics list, each related to creating
 * relics with legacies.
 */
namespace MooLegacyItems
{
    [HarmonyPatch(typeof(IdeoUIUtility)), HarmonyPatch("DoPreceptsInt")]
    public class IdeoUIUtility_DoPreceptsInt
    {
        private static readonly PreceptDef relicPrecept = PreceptDefOf.IdeoRelic;

        // Slightly wider than normal buttons for differentiation
        private static readonly Vector2 AddLegacyRelicButtonSize = new Vector2(200f, 30f);

        static void Postfix(string categoryLabel, string addPreceptLabel, bool mainPrecepts, Ideo ideo, IdeoEditMode editMode, ref float curY, float width, Func<PreceptDef, bool> filter, bool sortFloatMenuOptionsByLabel) //pass the __result by ref to alter it.
        {
            // First, only add the legacy-relic buttons right after the normal relic stuff, and only if we're in an editing mode
            if (categoryLabel == "IdeoRelics".Translate() && editMode != IdeoEditMode.None)
            {
                curY += 15;
                float num2 = width - (width - IdeoUIUtility.PreceptBoxSize.x * 3f - 16f) / 2f;
                // add a button that lists relics based on saved legacy items
                Rect rect = new Rect(num2 - AddLegacyRelicButtonSize.x, curY - AddLegacyRelicButtonSize.y, AddLegacyRelicButtonSize.x, AddLegacyRelicButtonSize.y);
                if (Widgets.ButtonText(rect, "MooLI_AddLegacyRelic".Translate(addPreceptLabel).CapitalizeFirst() + "...", true, true, true))
                {
                    // todo - add relic limit check
                    // sub todo - add duck's limit removal check

                    // todo - get relics from actual saved LI list
                    List<FloatMenuOption> opts = new List<FloatMenuOption>();
                    int maxRelics = relicPrecept.maxCount;
                    if (relicPrecept.preceptInstanceCountCurve != null)
								{
                        maxRelics = Mathf.Max(maxRelics, Mathf.RoundToInt(relicPrecept.preceptInstanceCountCurve.Last<CurvePoint>().y));
                    }
                    if (ideo.GetPreceptCountOfDef(relicPrecept) >= maxRelics && !relicPrecept.ignoreLimitsInEditMode)
                    {

                        opts.Add(new FloatMenuOption("MaxRelicCount".Translate(maxRelics), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                    }
                    else {
                        List<LegacyItem> cachedItems = LegacyItemManager.GetLegacyItems(false, false);
                        foreach (LegacyItem li in cachedItems)
                        {
                            Action action = delegate ()
                            {
                                Precept_LegacyRelic precept = new Precept_LegacyRelic(li, ideo);
                                RitualPatternDef pat = relicPrecept.ritualPatternBase;
                                ideo.AddPrecept(precept, true, null, pat);
                                ideo.anyPreceptEdited = true;
                            // todo remove LI from saved list once I reconfigure the Legacy Precept to save the entire legacy item properly
                            };
                            opts.Add(new FloatMenuOption(li.GetFormattedTitle(), action, li.itemDef.uiIcon, IdeoUIUtility.GetIconAndLabelColor(relicPrecept.impact), MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                        }
                        if (cachedItems.Count == 0)
                        {
                            opts.Add(new FloatMenuOption("MooLI_NoSavedLegacyOptionsForRelics".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                        }
                    }
                    // todo if no LI's found, add single no-op drop down option
                    // TODO in other code somewhere, add legacy item back to save list if removed from precept
                    FloatMenu menu = new FloatMenu(opts, "Saved Legacy Item Options");
                    // menu.doCloseButton = true;
                    Find.WindowStack.Add(menu);
                }
                curY += 15;
            }
        }
        
    }
}