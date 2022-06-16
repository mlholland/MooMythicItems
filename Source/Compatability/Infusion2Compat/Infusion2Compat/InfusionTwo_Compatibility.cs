using System.Collections.Generic;
using System.Linq;
using Infusion;
using MooMythicItems;
using Verse;

/* This file produces a separate assembly from the main Mythic Items mod, which is only loaded when the
 * Infusions 2 mod is also loaded. It allows mythic items to save and load infusions from said mod.*/
namespace Infusion2Compat
{
    [StaticConstructorOnStartup]
    public static class InfusionTwo_Compatibility
    {
        public static readonly string InfusedMythicKey = "InfusionTwoData";
        public static readonly string NoInfusions = "NO_INFUSIONS2";


        static InfusionTwo_Compatibility()
        {
            Log.Message("Infusions 2 found, applying compatibility patch for mythic items.");
            MythicItem.AddConstructorAddon(InfusedMythicKey, ConstructorFunc);
            MythicItem.AddRealizeAddon(InfusedMythicKey, RealizeFunc);
        }

        // After realizing a mythic item, check if it contains enchantment data, and if so, apply it to the item.
        public static bool RealizeFunc(ThingWithComps thing, string encodedData)
        { 
            CompInfusion targetComp = thing.TryGetComp<CompInfusion>();
            if (targetComp == null || encodedData == null || encodedData.Length == 0)
            {
                // TODO make and add comp to thing, check how original mod does this
                return false;
            }
            MooMythicItems.DebugActions.LogIfDebug("Applying saved infusion 2 data to mythic item being placed in the world");
            ApplyEncodedComp(encodedData, targetComp);
            return true;
        }

        // After initializing a mythic item from an in-game item, check if it has enchantment data worth saving.
        public static string ConstructorFunc(ThingWithComps item)
        {
            MooMythicItems.DebugActions.LogIfDebug("Running infusions 2 patch.");
            if (item == null) return null;
            CompInfusion infusedComp = item.TryGetComp<CompInfusion>();
            if (infusedComp != null)
            {
                MooMythicItems.DebugActions.LogIfDebug("saving infusion 2 comp...");
                return EncodeInfusionComp(infusedComp);
            }
            return null;
        }


        // helper function to encode infusion data into a single, dash-separated string.
        private static string EncodeInfusionComp(CompInfusion infusionComp)
        {
            List<string> values = new List<string>();
            if (infusionComp == null || infusionComp.Infusions == null || infusionComp.Infusions.Count() == 0) return NoInfusions;
            return string.Join("^", from def in infusionComp.Infusions select def.defName); // whoops don't use dashes cause of negative numbers
        }

        // helper function to decode infusion data and apply its values to an in-game comp.
        private static void ApplyEncodedComp(string encodedComp, CompInfusion targetComp)
        {
            MooMythicItems.DebugActions.LogIfDebug("Applying Infusions 2 infusions to Mythic Item. Encoding is {0}", encodedComp);
            List<string> infusionDefNames = encodedComp.Split('^').ToList();
            List<InfusionDef> infusions = new List<InfusionDef>();
            // short circuit if this item has no infusions - clear any randomly added infusions, then return.
            if (infusionDefNames.Count == 1 && infusionDefNames[0] == NoInfusions)
            {
                CompInfusionModule.removeAllInfusions(targetComp);
                return;
            }
            // find defs for each saved infusion, then override the comp's current infusions with them.
            foreach (string defName in infusionDefNames)
            {
                InfusionDef def = DefDatabase<InfusionDef>.GetNamedSilentFail(defName);
                if (def == null)
                {
                    MooMythicItems.DebugActions.LogErr("Tried to load infusion 2 data for mythic item, but failed to find infusion named {0}, ignoring this value.", defName);
                    continue;
                }
                infusions.Add(def);
            }
            CompInfusionModule.setInfusions(infusions, targetComp);
        }
    }
}
