using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;

/* Misc utility functions related to mythic items.
 */
namespace MooMythicItems
{
    public static class MythicItemUtilities
    { 
        private static HashSet<string> s_defaultNames = new HashSet<string> { "Moo", "Tynan", "Randy", "Cassie", "Pheobe", "Hi19Hi19" };
        private static HashSet<string> s_defaultFactions = new HashSet<string> { "Tynan's Tyranny Brigade", "Randy's Rabble Rousers", "Cassie's Centurions", "Pheobe's Pilgrims" };

        public static string RandomName()
        {
            
            return s_defaultNames.RandomElement();
        }

        public static string RandomFaction()
        {
            return s_defaultFactions.RandomElement();
        }

        /* Generate a mythic item by randomly selecting among some hard-coded options. 
         */
        public static MythicItem CreateRandomMythicItem()
        {
            System.Random rnd = new System.Random();
            MythicCauseDef cause = DefDatabase<MythicCauseDef>.GetRandom();
            ThingDef item;
            if (cause.createsMythicWeapon)
            {
                item = (from def in DefDatabase<ThingDef>.AllDefs
                where def.equipmentType == EquipmentType.Primary
                orderby rnd.Next()
                select def).First();
            } else
            {
                item = (from def in DefDatabase<ThingDef>.AllDefs
                        where def.IsApparel
                        orderby rnd.Next()
                        select def).First();
            }
            MythicEffectDef effect;
            string title;
            string description;
            if (cause.createsMythicWeapon && item.IsMeleeWeapon)
            {
                effect = cause.meleeEffects.RandomElement();
                title = cause.meleeTitles.RandomElement();
                description = cause.meleeDescriptions.RandomElement();
            } else
            { 
                effect = cause.effects.RandomElement();
                title = cause.titles.RandomElement();
                description = cause.descriptions.RandomElement();
            }
            string name = s_defaultNames.RandomElement();
            return new MythicItem(item, name, name, s_defaultFactions.RandomElement(), description, title, effect, item.MadeFromStuff ? GenStuff.RandomStuffFor(item) : null, 0, "debug", "0", new List<int>());
        }

        public static bool PawnHasMythicItem(Pawn p)
        {
            ThingWithComps weapon = p.equipment.Primary;
            CompMythic mythicComp = weapon.TryGetComp<CompMythic>();
            if (mythicComp != null && mythicComp.abilityDef != null)
            {
                return true;
            }
            foreach(Apparel app in p.apparel.WornApparel)
            {
                CompMythic appMythicComp = weapon.TryGetComp<CompMythic>();
                if (appMythicComp != null && mythicComp.abilityDef != null)
                {
                    return true;
                }
            }
            return false;
        }


        public static void AddMythicCompToThing(ThingWithComps thing) { }

        [HarmonyPatch(typeof(MythicItemUtilities), nameof(MythicItemUtilities.AddMythicCompToThing))]
        static class MythicItemUtilities_PawnHasMythicItem_Postfix_Patch
        {
            static void Postfix(ThingWithComps thing)
            {

            }
        }

    }
}
