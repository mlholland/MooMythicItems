using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;

/* Contains functions which are injected into the MythicItem class in order to save/load bladelink attributes of weapons.
 */
namespace MooMythicItems
{
    public static class MythicItem_BladelinkAddon
    {
        private static readonly string bladelinkKey = "RWR_BL_T"; // rimworld royalty bladelink traits
        // The traits field is private normally - this lets us mess with it.
        private static readonly AccessTools.FieldRef<object, List<WeaponTraitDef>> weaponTraitsField = AccessTools.FieldRefAccess<List<WeaponTraitDef>>(typeof(CompBladelinkWeapon), "traits");


        public static void AddBladelinkFunctionsToMythicItems()
        {
            MythicItem.AddConstructorAddon(bladelinkKey, EncodeBladelinkData);
            MythicItem.AddRealizeAddon(bladelinkKey, LoadEncodedBladelinkData);
        }

        public static string EncodeBladelinkData(ThingWithComps thing)
        {
            CompBladelinkWeapon linkComp = thing.TryGetComp<CompBladelinkWeapon>();
            if (linkComp != null && linkComp.TraitsListForReading != null)
            {
                return string.Join("/", linkComp.TraitsListForReading.Select(def => def.defName));
            }
            return null;
        }

        public static bool LoadEncodedBladelinkData(ThingWithComps thing, string encodedData)
        {
            CompBladelinkWeapon linkComp = thing.TryGetComp<CompBladelinkWeapon>();
            if (linkComp == null)
            {
                DebugActions.LogErr("Tried to realize a mythic {0} that had saved bladelink data, but no CompBladelinkWeapon was found on the resulting item to add this data to.", thing.def.label);
                return false;
            }
            else
            {
                List<WeaponTraitDef> traits = weaponTraitsField.Invoke(linkComp);
                if (traits == null)
                {
                    DebugActions.LogErr("Tried to extract traits from new mythic weapon's CompBladelinkWeapon comp, but the traits field was null.");
                }
                else
                {
                    traits.Clear();
                    foreach (string traitName in encodedData.Split('/'))
                    {
                        WeaponTraitDef trait = DefDatabase<WeaponTraitDef>.GetNamedSilentFail(traitName);
                        if (trait == null)
                        {
                            DebugActions.LogErr("Tried adding bladelink trait named '{0}' to a new mythic weapon, but no such trait could be found. Ignoring it.", traitName);
                            continue;
                        }
                        traits.Add(trait);
                    }
                }
            }
            return true;
        }
    
    }
}
