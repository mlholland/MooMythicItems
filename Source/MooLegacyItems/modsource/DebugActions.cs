using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

/* The settings tab for this mod, will eventually include a bunch of controls.*/
namespace MooMythicItems
{
    public class DebugActions : ModSettings
    {
        public static readonly string debugPrefix = "MooMF_LogPrefix";


        public static void MooLog(string printVal, params object[] args)
        {
            Log.Message(debugPrefix.Translate() + String.Format(printVal, args));
        }

        public static void LogIfDebug(string printVal, params object[] args)
        {
            if (MooMythicItems_Mod.settings.flagDebug)
            {
                DebugActions.MooLog(printVal, args); 
            }
        }

        public static void LogErr(string printVal, params object[] args)
        {
            Log.Error(debugPrefix.Translate() + String.Format(printVal, args));
        }

        public static void LogWarn(string printVal, params object[] args)
        {
            Log.Warning(debugPrefix.Translate() + String.Format(printVal, args));
        }

        [DebugAction("Spawning", "Create Mythic Item", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void CreateAndPlaceMythicItem()
        {
            // select a cause
            List<DebugMenuOption> causeList = new List<DebugMenuOption>();
            foreach (MythicCauseDef cause in DefDatabase<MythicCauseDef>.AllDefs)
            {
                causeList.Add(new DebugMenuOption(cause.defName, DebugMenuOptionMode.Action, delegate ()
                {
                    // then select a cause
                    List<DebugMenuOption> effectList = new List<DebugMenuOption>();
                    foreach (MythicEffectDef effectDef in cause.effects)
                    {
                        
                        effectList.Add(new DebugMenuOption(effectDef.defName, DebugMenuOptionMode.Action, delegate ()
                        {
                            // select the item - ranged weapons or apparel
                            List<DebugMenuOption> itemList = new List<DebugMenuOption>();
                            if (cause.createsMythicWeapon)
                            {
                                foreach (ThingDef rangedWeaponDef in from def in DefDatabase<ThingDef>.AllDefs
                                                               where def.equipmentType == EquipmentType.Primary && MythicItemUtilities.IsValidDefOption(def)
                                                               && (cause.hasDifferentMeleeOptions ? !def.IsMeleeWeapon : true)
                                                                     select def into d
                                                               orderby d.defName
                                                               select d)
                                {
                                    itemList.Add(new DebugMenuOption(rangedWeaponDef.defName, DebugMenuOptionMode.Tool, delegate ()
                                    {
                                        string name = MythicItemUtilities.RandomName();
                                        MythicItem createdItem = new MythicItem(rangedWeaponDef, name, name, MythicItemUtilities.RandomFaction(),
                                            cause.descriptions.RandomElement(),
                                            cause.titles.RandomElement(),
                                            effectDef,
                                            rangedWeaponDef.MadeFromStuff ? GenStuff.RandomStuffFor(rangedWeaponDef) : null, 0, "debug", "debug", new List<int>());
                                        DebugSpawnMythicItem(createdItem.Realize(), UI.MouseCell(), false);
                                    }));
                                }
                            } else
                            {
                                foreach (ThingDef apparelDef in from def in DefDatabase<ThingDef>.AllDefs
                                                                     where def.IsApparel && MythicItemUtilities.IsValidDefOption(def)
                                                                     select def into d
                                                                     orderby d.defName
                                                                     select d)
                                {
                                    itemList.Add(new DebugMenuOption(apparelDef.defName, DebugMenuOptionMode.Tool, delegate ()
                                    {
                                        string name = MythicItemUtilities.RandomName();
                                        MythicItem createdItem = new MythicItem(apparelDef, name, name, MythicItemUtilities.RandomFaction(),
                                            cause.descriptions.RandomElement(),
                                            cause.titles.RandomElement(),
                                            effectDef,
                                            apparelDef.MadeFromStuff ? GenStuff.RandomStuffFor(apparelDef) : null, 0, "debug", "debug", new List<int>());
                                        DebugSpawnMythicItem(createdItem.Realize(), UI.MouseCell(), false);
                                    }));
                                }
                            }
                            Find.WindowStack.Add(new Dialog_DebugOptionListLister(itemList));
                        }));
                    }
                    if (!cause.createsMythicWeapon || !cause.hasDifferentMeleeOptions)
                    {
                        Find.WindowStack.Add(new Dialog_DebugOptionListLister(effectList));
                        return;
                    }
                    // account for melee stuff if needed
                    foreach (MythicEffectDef effectDef in cause.meleeEffects)
                    {
                        effectList.Add(new DebugMenuOption(effectDef.defName, DebugMenuOptionMode.Action, delegate ()
                        {
                            // select the item - melee
                            List<DebugMenuOption> meleeItemList = new List<DebugMenuOption>();
                            foreach (ThingDef weaponDef in from def in DefDatabase<ThingDef>.AllDefs
                                                           where def.equipmentType == EquipmentType.Primary && def.IsMeleeWeapon && MythicItemUtilities.IsValidDefOption(def)
                                                           select def into d
                                                           orderby d.defName
                                                           select d)
                            {
                                ThingDef localWeaponDef = weaponDef;
                                meleeItemList.Add(new DebugMenuOption(localWeaponDef.defName, DebugMenuOptionMode.Tool, delegate ()
                                {
                                    string name = MythicItemUtilities.RandomName();
                                    MythicItem createdItem = new MythicItem(localWeaponDef, name, name, MythicItemUtilities.RandomFaction(),
                                        cause.meleeDescriptions.RandomElement(), 
                                        cause.meleeTitles.RandomElement(), 
                                        effectDef, 
                                        localWeaponDef.MadeFromStuff ? GenStuff.RandomStuffFor(localWeaponDef) : null, 0, "debug", "debug", new List<int>());
                                    DebugSpawnMythicItem(createdItem.Realize(), UI.MouseCell(), false);
                                }));
                            }
                            Find.WindowStack.Add(new Dialog_DebugOptionListLister(meleeItemList));
                        }));
                    }

                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(effectList));
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(causeList));
        }

        [DebugAction("Spawning", "Spawn Random Mythic Item", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceRandomizedMythicItem()
        {
            DebugSpawnMythicItem(MythicItemUtilities.CreateRandomMythicItem().Realize(), UI.MouseCell(), false);
        }

        [DebugAction("Spawning", "Spawn Saved Mythic Item", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceSavedMythicItem()
        {
            List<DebugMenuOption> itemList = new List<DebugMenuOption>();
            foreach (MythicItem mi in MythicItemCache.GetMythicItems(true, true))
            {
                itemList.Add(new DebugMenuOption(mi.GetFormattedTitle(), DebugMenuOptionMode.Tool, delegate ()
                {
                    DebugSpawnMythicItem(MythicItemCache.RealizeSelectedMythicItem(mi, false), UI.MouseCell(), false);
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(itemList));
        }

        [DebugAction("Spawning", "Try place Saved Mythic Item - Record World", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryPlaceSavedMythicItemRecordWorld()
        {
            List<DebugMenuOption> itemList = new List<DebugMenuOption>();
            foreach (MythicItem mi in MythicItemCache.GetMythicItems(true, true))
            {
                itemList.Add(new DebugMenuOption(mi.GetFormattedTitle(), DebugMenuOptionMode.Tool, delegate ()
                {
                    DebugSpawnMythicItem(MythicItemCache.RealizeSelectedMythicItem(mi, true), UI.MouseCell(), false);
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(itemList));
        }

        // simple helper function just places a thing.
        public static void DebugSpawnMythicItem(Thing item, IntVec3 c,  bool direct = false, ThingStyleDef thingStyleDef = null)
        {
            if (direct)
            {
                GenPlace.TryPlaceThing(item, c, Find.CurrentMap, ThingPlaceMode.Direct, null, null, default(Rot4));
                return;
            }
            GenPlace.TryPlaceThing(item, c, Find.CurrentMap, ThingPlaceMode.Near, null, null, default(Rot4));
        }

        /*[DebugAction("Spawning", "Try place all mythic item types", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DebugSpawnAllMythicItems()
        {
            // Todo
        }*/
    }
}
