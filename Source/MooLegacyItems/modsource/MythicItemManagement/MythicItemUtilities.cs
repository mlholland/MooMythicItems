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
        // Needed for realization to add a mythic comp to Things that don't normally have it.
        private static AccessTools.FieldRef<object, List<ThingComp>> compsField = AccessTools.FieldRefAccess<List<ThingComp>>(typeof(ThingWithComps), "comps");

        public static string RandomName()
        {
            
            return s_defaultNames.RandomElement();
        }

        public static string RandomFaction()
        {
            return s_defaultFactions.RandomElement();
        }

        public static bool IsValidDefOption(ThingDef def)
        {
            if (def == null)
            {
                DebugActions.LogErr("Null def inputted to IsValidDefOption, this should never happen.");
                return false;
            }
            // make sure it's not stackable like a thrumbo horn
            if (def.stackLimit != 1)
            {
                DebugActions.LogIfDebug("Item type '{0}' is not a valid mythic item option because it can be stacked.", def.label);
                return false;
            }
            // make sure it's not a single shot weapon like a doomday launcher
            if (def.Verbs != null && def.Verbs.Count > 0 && def.Verbs.Any(verb => verb.verbClass == typeof(Verb_ShootOneUse)))
            {
                DebugActions.LogIfDebug("Item type '{0}' is not a valid mythic item option because it has verbs (indicating that it's a gun), and one of them is Verb_ShootOneUse (indicating that it's a single-use rocket launcher).", def.label);
                return false;
            }
            // make sure it's not an unobtainable weapon like a mech gun
            if (def.destroyOnDrop)
            {
                DebugActions.LogIfDebug("Item type '{0}' is not a valid mythic item option because it gets destroyed when dropped.", def.label);
                return false;
            }
            // make sure it's not an item that destroys after running out of charges like an insanity lance.
            CompProperties_Reloadable reload = def.GetCompProperties<CompProperties_Reloadable>();
            if (reload != null && reload.destroyOnEmpty)
            {
                DebugActions.LogIfDebug("Item type '{0}' is not a valid mythic item option because it gets destroyed when it runs out of charges.", def.label);
                return false;
            }
            return true;
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
                where def.equipmentType == EquipmentType.Primary && IsValidDefOption(def)
                        orderby rnd.Next()
                select def).First();
            } else
            {
                item = (from def in DefDatabase<ThingDef>.AllDefs
                        where def.IsApparel && IsValidDefOption(def)
                        orderby rnd.Next()
                        select def).First();
            }
            MythicEffectDef effect;
            string title;
            string description;
            if (cause.createsMythicWeapon && item.IsMeleeWeapon && cause.hasDifferentMeleeOptions)
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

        public static void AddMythicCompToThing(ThingWithComps thing, string label, string desc, MythicEffectDef ability)
        {
            List<ThingComp> comps = compsField.Invoke(thing);
            CompMythic mythicComp = (CompMythic)Activator.CreateInstance(typeof(CompMythic));
            mythicComp.parent = thing;
            comps.Add(mythicComp);
            mythicComp.Initialize(new CompProperties_Mythic());
            mythicComp.newLabel = label;
            mythicComp.newDescription = desc;
            mythicComp.abilityDef = ability;
        }
    }
}
