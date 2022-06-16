using System.Collections.Generic;
using System.Linq;
using MooMythicItems;
using Verse;
using Infused;

/* This file produces a separate assembly from the main mythic items mod, which is conditionally loaded when the infusions (1) mod is also active.
   It causes mythic items to save/load infusion data.*/
namespace InfusedCompat
{
    [StaticConstructorOnStartup]
    public class Infused_Compatibility
    {
        public static readonly string InfusedMythicKey = "InfusionOneData";
        public static readonly string NoInfusions = "NO_INFUSIONS";


        static Infused_Compatibility()
        {
            Log.Message("Infusions found, applying compatibility patch for mythic items.");
            MythicItem.AddConstructorAddon(InfusedMythicKey, ConstructorFunc);
            MythicItem.AddRealizeAddon(InfusedMythicKey, RealizeFunc);
        }

        // After realizing a mythic item, check if it contains enchantment data, and if so, apply it to the item.
        public static bool RealizeFunc(ThingWithComps thing, string encodedData)
        {
            CompInfused targetComp = thing.TryGetComp<CompInfused>();
            if (targetComp == null || encodedData == null || encodedData.Length == 0)
            {
                // TODO make and add comp to thing, check how original mod does this
                return false;
            }
            DebugActions.LogIfDebug("Applying saved infusion data to mythic item being placed in the world");
            ApplyEncodedComp(encodedData, targetComp);
            return true;
        }

        // After initializing a mythic item from an in-game item, check if it has enchantment data worth saving.
        public static string ConstructorFunc(ThingWithComps item)
        {
            DebugActions.LogIfDebug("Running infusions patch.");
            if (item == null) return null;
            CompInfused infusedComp = item.TryGetComp<CompInfused>();
            if (infusedComp != null)
            {
                DebugActions.LogIfDebug("saving infusion comp...");
                return EncodeInfusionComp(infusedComp);
            }
            return null;
        }


        // helper function to encode infusion data into a single, dash-separated string.
        private static string EncodeInfusionComp(CompInfused infusionComp)
        {
            List<string> values = new List<string>();
            if (infusionComp == null || infusionComp.Infusions == null || infusionComp.Infusions.Count() == 0) return NoInfusions;
            return string.Join("^", from def in infusionComp.Infusions select def.defName); // whoops don't use dashes cause of negative numbers
        }

        // helper function to decode infusion data and apply its values to an in-game comp.
        private static void ApplyEncodedComp(string EncodedComp, CompInfused targetComp)
        {
            DebugActions.LogIfDebug("Applying Rimworld of Magic Gemstone Enchantments to Mythic Item");
            List<string> infusionDefNames = EncodedComp.Split('^').ToList();
            List<Infused.Def> infusions = new List<Infused.Def>();
            // short circuit if this item has no infusions - clear any randomly added infusions, then return.
            if (infusionDefNames.Count == 1 && infusionDefNames[0] == NoInfusions)
            {
                targetComp.SetInfusions(infusions);
                return;
            }
            // find defs for each saved infusion, then override the comp's current infusions with them.
            foreach(string defName in infusionDefNames)
            {
                Infused.Def def = DefDatabase<Infused.Def>.GetNamedSilentFail(defName);
                if (def == null)
                {
                    DebugActions.LogErr("Tried to load infusion data for mythic item, but failed to find infusion named {0}, ignoring this value.", defName);
                    continue;
                }
                infusions.Add(def);
            }
            targetComp.SetInfusions(infusions);
        }
    }
}
