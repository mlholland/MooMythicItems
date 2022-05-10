using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse.AI;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse.Grammar;

/* This patch allows item stash quests to properly generate mythic items when the inputted slate includes them.
 */
namespace MooMythicItems
{
    // Priority set to high to make sure the rule removing happens immeditely after the original function concludes.
    [HarmonyPatch(typeof(SitePartWorker_ItemStash), nameof(SitePartWorker_ItemStash.Notify_GeneratedByQuestGen)), HarmonyPriority(Priority.High)]
    public class SitePatch_MakeMythicItem
    {
        static void Postfix(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants) //pass the __result by ref to alter it.
        {
            // Do nothing if no mythic item added to slate.
            MythicItem mythicItem = slate.Get<MythicItem>("itemStashOnlyMythicItem", null, false);
            if (mythicItem == null) return; 
            

            // Remove the normal itemStashContents value rules added by the original function itemStashContentsValue
            // It's impossible to configure the slate to not add those, so we just remove them after the fact.
            // Unfortunately we need to do this by index, because there's no way to determine what items/value were randomly generated for the quest.
            outExtraDescriptionRules.RemoveAt(outExtraDescriptionRules.Count - 1);
            outExtraDescriptionRules.RemoveAt(outExtraDescriptionRules.Count - 1);
            // The rest of this patch is more or less what the original function does to generate items, except we replace certain values set by the original.
            List<Thing> list = new List<Thing>();
            part.things = new ThingOwner<Thing>(part, false, LookMode.Deep);
            list.Add(MythicItemCache.RealizeSelectedMythicItem(mythicItem, true));
            part.things.TryAddRangeOrTransfer(list, false, false);
            slate.Set<List<Thing>>("generatedItemStashThings", list, false);
            outExtraDescriptionRules.Add(new Rule_String("itemStashContents", GenLabel.ThingsLabel(list, "  - ")));
            outExtraDescriptionRules.Add(new Rule_String("itemStashContentsValue", GenThing.GetMarketValue(list).ToStringMoney(null)));
        }
    }
}