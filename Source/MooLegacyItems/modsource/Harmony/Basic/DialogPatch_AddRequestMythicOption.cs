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
using RimWorld.QuestGen;

/* Modifies the creation of dialogue with factions (the text options you have when you call a faction) to allow for finding a mythic item
 * if you're allied with them and willing to pay a pretty penny.
 */
namespace MooMythicItems
{
    [HarmonyPatch(typeof(FactionDialogMaker), nameof(FactionDialogMaker.FactionDialogFor))]
    public class DialogPatch_AddRequestMythicOption
    {
        public static int MythicItemQuestCost = 7500;
        public static int BaseGoodwillCost = 75;
        public static int MinDaysToUnlockQuest = 90;

        // This was private in FactionDialogMaker, and I like to avoid fieldRefs when possible.
        private static int AmountSendableSilver(Map map)
        {
            return (from t in TradeUtility.AllLaunchableThingsForTrade(map, null)
                    where t.def == ThingDefOf.Silver
                    select t).Sum((Thing t) => t.stackCount);
        }

        static void Postfix(DiaNode __result, Pawn negotiator, Faction faction)
        {
            if (negotiator == null || faction == null) return;
            Map map = negotiator.Map;
            // Todo check available mythic items, available silver, faction relation/goodwill individually, and send different failure reasons depending on what's wrong

            if (map == null && !map.IsPlayerHome && faction.PlayerRelationKind != FactionRelationKind.Ally)
            {
                return;
            }
            DiaOption diaOption = new DiaOption(String.Format("MooMF_RequestMythicItemDialogOption".Translate(), MythicItemQuestCost, BaseGoodwillCost));
            if (!MythicItemCache.CanRealizeRandomMythicItem(MooMythicItems_Mod.settings.flagCreateRandomMythicItemsIfNoneAvailable, true, false))
            {
                diaOption.Disable(String.Format("MooMF_NoMythicItemsExist".Translate()));
                __result.options.Insert(__result.options.Count - 1, diaOption);
                return;
            }
            else if (Find.TickManager.TicksGame < MinDaysToUnlockQuest * 60000)
            {
                diaOption.Disable(String.Format("MooMF_MinDaysRequired".Translate(), MinDaysToUnlockQuest));
                __result.options.Insert(__result.options.Count - 1, diaOption);
                return;
            }
            else if (faction.PlayerGoodwill < BaseGoodwillCost)
            {
                diaOption.Disable("NeedGoodwill".Translate(BaseGoodwillCost.ToString("F0")));
                __result.options.Insert(__result.options.Count - 1, diaOption);
                return;
            }
            else if (AmountSendableSilver(map) < MythicItemQuestCost)
            {
                diaOption.Disable("NeedSilverLaunchable".Translate(MythicItemQuestCost.ToString("F0")));
                __result.options.Insert(__result.options.Count - 1, diaOption);
                return;
            }
            else
            {
                Slate slate = new Slate();
                slate.Set<float>("points", StorytellerUtility.DefaultThreatPointsNow(Find.World), false);
                slate.Set<Pawn>("asker", faction.leader, false);
                MythicItem mi = MythicItemCache.SelectRandomMythicItemFromCache();
                slate.Set<MythicItem>("itemStashOnlyMythicItem", mi, false); // This is used by another patch to replace the normal quest contents with a mythic item
                // need to make new diaOption to set complex values (at least that's how vanilla code did it).
                __result.options.Insert(__result.options.Count - 1, new DiaOption(String.Format("MooMF_RequestMythicItemDialogOption".Translate(), MythicItemQuestCost, -1 * Faction.OfPlayer.CalculateAdjustedGoodwillChange(faction, -BaseGoodwillCost)))
                {
                    action = delegate ()
                    {
                        Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.OpportunitySite_ItemStash, slate);
                        if (!quest.hidden && quest.root.sendAvailableLetter)
                        {
                            QuestUtility.SendLetterQuestAvailable(quest);
                        }
                        TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, MythicItemQuestCost, map, null);
                        Faction.OfPlayer.TryAffectGoodwillWith(faction, -BaseGoodwillCost, false, true, null, null);
                    },
                    link = new DiaNode(String.Format("MooMF_MythicItemRequestedDialogFollowUp".Translate(), faction.LeaderTitle))
                    {
                        options = new List<DiaOption>() { new DiaOption("OK".Translate())
                        {
                            linkLateBind = () => FactionDialogMaker.FactionDialogFor(negotiator, faction)
                        }}
                    }
                });
            }
        }
    }
}