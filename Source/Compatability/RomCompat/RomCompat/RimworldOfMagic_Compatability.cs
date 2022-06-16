using System;
using System.Collections.Generic;
using System.Linq;
using TorannMagic.Enchantment;
using MooMythicItems;
using Verse;

namespace RomCompat
{

    /* Loads functions into the core mod to allow for enchantments to be properly saved/loaded as part of mythic items*/

    [StaticConstructorOnStartup]
    public static class RimworldOfMagic_Compatibility
    {
        public static readonly string RoMMythicKey = "RoMEnchantData";


        static RimworldOfMagic_Compatibility()
        {
            Log.Message("Rimworld of Magic found, applying compatibility patch for mythic items.");
            MythicItem.AddConstructorAddon(RoMMythicKey, ConstructorFunc);
            MythicItem.AddRealizeAddon(RoMMythicKey, RealizeFunc);
        }

        // After realizing a mythic item, check if it contains enchantment data, and if so, apply it to the item.
        public static bool RealizeFunc(ThingWithComps thing, string encodedData)
        { 
            CompEnchantedItem targetComp = thing.TryGetComp<CompEnchantedItem>();
            if (targetComp == null)
            {
                // TODO make and add comp to thing, check how original mod does this
                return false;
            }
            ApplyEncodedComp(encodedData, targetComp);
            return true;
        }


        // After initializing a mythic item from an in-game item, check if it has enchantment data worth saving.
        public static string ConstructorFunc(ThingWithComps item)
        {
            DebugActions.LogIfDebug("Running magic patch.");
            if (item == null) return null;
            CompEnchantedItem enchantComp = item.TryGetComp<CompEnchantedItem>();
            if (enchantComp != null && enchantComp.HasEnchantment)
            {
                DebugActions.LogIfDebug("saving enchant comp...");
                return EncodeEnchantmentComp(enchantComp); 
            }
            else if (enchantComp != null)
            {
                DebugActions.LogIfDebug("enchant comp 'HasEnchantment' value is false, not saving.");
            }
            else
            {
                DebugActions.LogIfDebug("no enchant comp found to save");
            }
            return null;
        }


        // helper function to encode enchant data into a single, dash-separated string.
        private static string EncodeEnchantmentComp(CompEnchantedItem enchantComp)
        {
            List<string> values = new List<string>();
            values.Add(string.Format("{0:N2}", enchantComp.maxMP));
            values.Add(enchantComp.maxMPTier.ToString());

            values.Add(string.Format("{0:N2}", enchantComp.mpRegenRate));
            values.Add(enchantComp.mpRegenRateTier.ToString());

            values.Add(string.Format("{0:N2}", enchantComp.mpCost));
            values.Add(enchantComp.mpCostTier.ToString());

            values.Add(string.Format("{0:N2}", enchantComp.coolDown));
            values.Add(enchantComp.coolDownTier.ToString());

            values.Add(string.Format("{0:N2}", enchantComp.xpGain));
            values.Add(enchantComp.xpGainTier.ToString());

            values.Add(string.Format("{0:N2}", enchantComp.arcaneRes));
            values.Add(enchantComp.arcaneResTier.ToString());

            values.Add(string.Format("{0:N2}", enchantComp.arcaneDmg));
            values.Add(enchantComp.arcaneDmgTier.ToString());
            return string.Join("^", values); // whoops don't use dashes cause of negative numbers
        }

        // helper function to decode enchant data and apply its values to an in-game comp.
        private static void ApplyEncodedComp(string EncodedComp, CompEnchantedItem targetComp)
        {
            DebugActions.LogIfDebug("Applying Rimworld of Magic Gemstone Enchantments to Mythic Item");
            List<string> values = EncodedComp.Split('^').ToList();
            if (values.Count != 14)
            {
                DebugActions.LogErr("While trying to instantiate a saved mythic item into the world, an incorrect number of values related to Rimworld of Magic Enchantment data were found. Saved enchantments from the original item will not be applied. Expected 14 values and found {0}", values.Count);
                return;
            }
            targetComp.HasEnchantment = true;

            EnchantmentTier tier = EnchantmentTier.Undefined;
            float mp = 0, regen = 0, cost = 0, cd = 0, xp = 0, res = 0, dmg = 0;

            float.TryParse(values[0], out mp);
            Enum.TryParse(values[1], out tier);
            if (mp != 0)
            {
                DebugActions.LogIfDebug("Setting max MP to {0}", mp);
                targetComp.maxMP = mp;
                targetComp.maxMPTier = tier;
            }

            float.TryParse(values[2], out regen);
            Enum.TryParse(values[3], out tier);
            if (regen != 0)
            {
                DebugActions.LogIfDebug("Setting MP regen to {0}", regen);
                targetComp.mpRegenRate = regen;
                targetComp.mpRegenRateTier = tier;
            }

            float.TryParse(values[4], out cost);
            Enum.TryParse(values[5], out tier);
            if (cost != 0)
            {
                DebugActions.LogIfDebug("Setting MP Cost Reduction to {0}", cost);
                targetComp.mpCost = cost;
                targetComp.mpCostTier = tier;
            }

            float.TryParse(values[6], out cd);
            Enum.TryParse(values[7], out tier);
            if (cd != 0)
            {
                DebugActions.LogIfDebug("Setting cooldown rate change to {0}", cd);
                targetComp.coolDown = cd;
                targetComp.coolDownTier = tier;
            }

            float.TryParse(values[8], out xp);
            Enum.TryParse(values[9], out tier);
            if (xp != 0)
            {
                DebugActions.LogIfDebug("Setting xp gain change to {0}", xp);
                targetComp.xpGain = xp;
                targetComp.xpGainTier = tier;
            }

            float.TryParse(values[10], out res);
            Enum.TryParse(values[11], out tier);
            if (res != 0)
            {
                DebugActions.LogIfDebug("Setting arcane res to {0}", res);
                targetComp.arcaneRes = res;
                targetComp.arcaneResTier = tier;
            }

            float.TryParse(values[12], out dmg);
            Enum.TryParse(values[13], out tier);
            if (dmg != 0)
            {
                DebugActions.LogIfDebug("Setting arcane dmg to {0}", dmg);
                targetComp.arcaneDmg = dmg;
                targetComp.arcaneDmgTier = tier;
            }
        }
    }
}
