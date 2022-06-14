using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

/* This class contains functions that make this mod compatible with the "Rimworld of Magic" mod. Specifically,
 * it includes patches for the saving and loading of mythic items in order to allow magical enhancements to persist
 * across worlds. Only saves values that can actually change dynamically, i.e. enchantment values since those can be 
 * altered by enchanting gems.
 */
namespace MooMythicItems
{
    public class RimworldOfMagic_Compatibility
    {
        public static readonly string RoMMythicKey = "RoMEnchantData";


        private static Type compEnchantedItemType;
        private static Type compEnchantmentTierType;
        private static AccessTools.FieldRef<object, bool> hasEnchantField;
        private static AccessTools.FieldRef<object, float> maxMpField, regenField, costField, cdField, xpField, resField, dmgField;
        private static AccessTools.FieldRef<object, int> maxMpTierField, regenTierField, costTierField, cdTierField, xpTierField, resTierField, dmgTierField;


        // Tries to find all the needed values from Rimworld of Magic via reflection. Returns true if all values were found and set, false otherwise.
        public static bool InitializeReflectionValues()
        {
            compEnchantedItemType = AccessTools.TypeByName("TorannMagic.Enchantment.CompEnchantedItem");
            if(compEnchantedItemType == null)
            {
                Log.Error("Mythic Framework failed to find Rimworld of Magic Enchantment Class");
                return false;
            }

            compEnchantmentTierType = AccessTools.TypeByName("TorannMagic.Enchantment.EnchantmentTier");
            if (compEnchantmentTierType == null)
            {
                Log.Error("Mythic Framework failed to find Rimworld of Magic Enchantment Tier Class");
                return false;
            }

            hasEnchantField = AccessTools.FieldRefAccess<bool>(compEnchantedItemType, "hasEnchantment");
            if (hasEnchantField == null)
            {
                Log.Error("Mythic Framework failed to find Rimworld of Magic hasEnchantment field");
                return false;
            }

            //private static readonly AccessTools.FieldRef<object, List<WeaponTraitDef>> weaponTraitsField = AccessTools.FieldRefAccess<List<WeaponTraitDef>>(typeof(CompBladelinkWeapon), "traits");
            maxMpField = AccessTools.FieldRefAccess<float>(compEnchantedItemType, "maxMP");
            regenField = AccessTools.FieldRefAccess<float>(compEnchantedItemType, "mpRegenRate");
            costField = AccessTools.FieldRefAccess<float>(compEnchantedItemType, "mpCost");
            cdField = AccessTools.FieldRefAccess<float>(compEnchantedItemType, "coolDown");
            xpField = AccessTools.FieldRefAccess<float>(compEnchantedItemType, "xpGain");
            resField = AccessTools.FieldRefAccess<float>(compEnchantedItemType, "arcaneRes");
            dmgField = AccessTools.FieldRefAccess<float>(compEnchantedItemType, "arcaneDmg");

            if (maxMpField == null ||
                regenField == null ||
                costField == null ||
                cdField == null ||
                xpField == null ||
                resField == null ||
                dmgField == null )
            {
                Log.Error("Mythic Framework failed to find Rimworld of Magic enchantment value fields");
                return false;
            }

            maxMpTierField = AccessTools.FieldRefAccess<int>(compEnchantedItemType, "maxMPTier");
            regenTierField = AccessTools.FieldRefAccess<int>(compEnchantedItemType, "mpRegenRateTier");
            costTierField = AccessTools.FieldRefAccess<int>(compEnchantedItemType, "mpCostTier");
            cdTierField = AccessTools.FieldRefAccess<int>(compEnchantedItemType, "coolDownTier");
            xpTierField = AccessTools.FieldRefAccess<int>(compEnchantedItemType, "xpGainTier");
            resTierField = AccessTools.FieldRefAccess<int>(compEnchantedItemType, "arcaneResTier");
            dmgTierField = AccessTools.FieldRefAccess<int>(compEnchantedItemType, "arcaneDmgTier");

            if (maxMpTierField == null ||
                regenTierField == null ||
                costTierField == null ||
                cdTierField == null ||
                xpTierField == null ||
                resTierField == null ||
                dmgTierField == null)
            {
                Log.Error("Mythic Framework failed to find Rimworld of Magic enchantment tier fields");
                return false;
            }
            return true;
        }

        // After realizing a mythic item, check if it contains enchantment data, and if so, apply it to the item.
        public static void RealizePatch(ThingWithComps __result, MythicItem __instance)
        {
            if (__instance != null && __instance.extraItemData != null && __instance.extraItemData.ContainsKey(RoMMythicKey))
            {
                ThingComp enchantComp = null;
                foreach (ThingComp comp in __result.AllComps)
                {
                    if (comp.GetType() == compEnchantedItemType)
                    {
                        Log.Message("found enchant comp");
                        enchantComp = comp;
                        break;
                    }
                }
                if (enchantComp == null)
                {
                    DebugActions.LogErr("Tried loading Rimworld of Magic Enchant data when instantiating a mythic item, but the resulting {0} had no CompEnchantedItem to attach enchant data to.", __result.def.label);
                    return;
                }
                ApplyEncodedComp(__instance.extraItemData[RoMMythicKey], enchantComp);
            } 
        }


        // After initializing a mythic item from an in-game item, check if it has enchantment data worth saving.
        public static void CreationPatch(MythicItem __instance, ThingWithComps item)
        {
            if (item == null) return;
            ThingComp enchantComp = null;
            foreach (ThingComp comp in item.AllComps)
            {
                if (comp.GetType() == compEnchantedItemType)
                {
                    enchantComp = comp;
                    break;
                }
            }
            if (enchantComp == null)
            {
                DebugActions.LogErr("Rimworld of Magic is active, but no enchantment comp was found an a {0} that was turned into a mythic item. Enchant data, if any, will not be preserved.", item.def.label);
                return;
            }

            if (hasEnchantField.Invoke(enchantComp))
            {
                DebugActions.LogIfDebug("Saving Rimworld of Magic Enchant Data to Mythic Item...");
                string encodedEnchantComp = EncodeEnchantmentComp(enchantComp);
                __instance.extraItemData[RoMMythicKey] = encodedEnchantComp;
            } 
        }

        // helper function to encode enchant data into a single, dash-separated string.
        private static string EncodeEnchantmentComp(ThingComp enchantComp)
        {
            List<string> values = new List<string>();
            values.Add(string.Format("{0:N2}", maxMpField.Invoke(enchantComp)));
            values.Add(string.Format("{0}", maxMpTierField.Invoke(enchantComp)));

            values.Add(string.Format("{0:N2}", regenField.Invoke(enchantComp)));
            values.Add(string.Format("{0}", regenTierField.Invoke(enchantComp)));

            values.Add(string.Format("{0:N2}", costField.Invoke(enchantComp)));
            values.Add(string.Format("{0}", costTierField.Invoke(enchantComp)));

            values.Add(string.Format("{0:N2}", cdField.Invoke(enchantComp)));
            values.Add(string.Format("{0}", cdTierField.Invoke(enchantComp)));

            values.Add(string.Format("{0:N2}", xpField.Invoke(enchantComp)));
            values.Add(string.Format("{0}", xpTierField.Invoke(enchantComp)));

            values.Add(string.Format("{0:N2}", resField.Invoke(enchantComp)));
            values.Add(string.Format("{0}", resTierField.Invoke(enchantComp)));

            values.Add(string.Format("{0:N2}", dmgField.Invoke(enchantComp)));
            values.Add(string.Format("{0}", dmgTierField.Invoke(enchantComp)));

            return string.Join("^", values); // whoops don't use dashes in case of negative numbers
        }

        // helper function to decode enchant data and apply its values to an in-game comp.
        private static void ApplyEncodedComp(string EncodedComp, ThingComp enchantComp)
        {
            DebugActions.LogIfDebug("Applying Rimworld of Magic Gemstone Enchantments to Mythic Item");
            List<string> values = EncodedComp.Split('^').ToList();
            if (values.Count != 14)
            {
                DebugActions.LogErr("While trying to instantiate a saved mythic item into the world, an incorrect number of values related to Rimworld of Magic Enchantment data were found. Saved enchantments from the original item will not be applied. Expected 14 values and found {0}", values.Count);
                return;
            }
            hasEnchantField.Invoke(enchantComp) = true;

            int tier = 0;
            float mp = 0, regen = 0, cost = 0, cd = 0, xp = 0, res = 0, dmg = 0;

            float.TryParse(values[0], out mp);
            int.TryParse(values[1], out tier);
            if (mp != 0)
            {
                DebugActions.LogIfDebug("Setting max MP to {0}", mp);
                maxMpField.Invoke(enchantComp) = mp;
                maxMpTierField.Invoke(enchantComp) = tier;
            }

            float.TryParse(values[2], out regen);
            int.TryParse(values[3], out tier);
            if (regen != 0)
            {
                DebugActions.LogIfDebug("Setting MP regen to {0}", regen);
                regenField.Invoke(enchantComp) = regen;
                regenTierField.Invoke(enchantComp) = tier;
            }

            float.TryParse(values[4], out cost);
            int.TryParse(values[5], out tier);
            if (cost != 0)
            {
                DebugActions.LogIfDebug("Setting MP Cost Reduction to {0}", cost);
                costField.Invoke(enchantComp) = cost;
                costTierField.Invoke(enchantComp) = tier;
            }

            float.TryParse(values[6], out cd);
            int.TryParse(values[7], out tier);
            if (cd != 0)
            {
                DebugActions.LogIfDebug("Setting cooldown rate change to {0}", cd);
                cdField.Invoke(enchantComp) = cd;
                cdTierField.Invoke(enchantComp) = tier;
            }

            float.TryParse(values[8], out xp);
            int.TryParse(values[9], out tier);
            if (xp != 0)
            {
                DebugActions.LogIfDebug("Setting xp gain change to {0}", xp);
                xpField.Invoke(enchantComp) = xp;
                xpTierField.Invoke(enchantComp) = tier;
            }

            float.TryParse(values[10], out res);
            int.TryParse(values[11], out tier);
            if (res != 0)
            {
                DebugActions.LogIfDebug("Setting arcane res to {0}", res);
                resField.Invoke(enchantComp) = res;
                resTierField.Invoke(enchantComp) = tier;
            }

            float.TryParse(values[12], out dmg);
            int.TryParse(values[13], out tier);
            if (dmg != 0)
            {
                DebugActions.LogIfDebug("Setting arcane dmg to {0}", dmg);
                dmgField.Invoke(enchantComp) = dmg;
                dmgTierField.Invoke(enchantComp) = tier;
            }
        }
    }     
}
