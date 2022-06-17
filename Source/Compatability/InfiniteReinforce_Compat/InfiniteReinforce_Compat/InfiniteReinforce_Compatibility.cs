using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using MooMythicItems;
using InfiniteReinforce;
using Verse;
using HarmonyLib;

namespace InfiniteReinforce_Compat
{
    [StaticConstructorOnStartup]
    public class InfiniteReinforce_Compatibility
    {
        public static readonly string InfusedMythicKey = "InfiniteReinforceData";
        private static readonly AccessTools.FieldRef<object, int> reinforcedField = AccessTools.FieldRefAccess<int>(typeof(ThingComp_Reinforce), "reinforced");
        private static readonly AccessTools.FieldRef<object, int> discountField = AccessTools.FieldRefAccess<int>(typeof(ThingComp_Reinforce), "discount");
        private static readonly AccessTools.FieldRef<object, Dictionary<StatDef, float>> statField = AccessTools.FieldRefAccess<Dictionary<StatDef, float>>(typeof(ThingComp_Reinforce), "statboost");
        private static readonly AccessTools.FieldRef<object, Dictionary<StatDef, int>> statCountField = AccessTools.FieldRefAccess<Dictionary<StatDef, int>>(typeof(ThingComp_Reinforce), "reinforcedcount");
        private static readonly AccessTools.FieldRef<object, Dictionary<ReinforceDef, float>> customField = AccessTools.FieldRefAccess<Dictionary<ReinforceDef, float>>(typeof(ThingComp_Reinforce), "custom");
        private static readonly AccessTools.FieldRef<object, Dictionary<ReinforceDef, int>> customCountField = AccessTools.FieldRefAccess<Dictionary<ReinforceDef, int>>(typeof(ThingComp_Reinforce), "customcount");
        private static readonly AccessTools.FieldRef<object, IRDifficultFlag> difficultyField = AccessTools.FieldRefAccess<IRDifficultFlag>(typeof(ThingComp_Reinforce), "difficult");

        private static readonly char fieldSep = '^';
        private static readonly char listSep = '&';
        private static readonly char pairSep = '$';

        static InfiniteReinforce_Compatibility()
        {
            Log.Message("Infinite Reinforce found, applying compatibility patch for mythic items.");
            MythicItem.AddConstructorAddon(InfusedMythicKey, ConstructorFunc);
            MythicItem.AddRealizeAddon(InfusedMythicKey, RealizeFunc);
        }

        // After realizing a mythic item, check if it contains enchantment data, and if so, apply it to the item.
        public static bool RealizeFunc(ThingWithComps thing, string encodedData)
        {
            ThingComp_Reinforce reinforceComp = thing.TryGetComp<ThingComp_Reinforce>();
            if (reinforceComp == null || encodedData == null || encodedData.Length == 0)
            {
                // TODO make and add comp to thing, check how original mod does this
                return false;
            }
            DebugActions.LogIfDebug("Applying saved infinite reinforce data to mythic item being placed in the world");
            ApplyEncodedComp(encodedData, reinforceComp);
            return true;
        }

        // After initializing a mythic item from an in-game item, check if it has reinforcement data worth saving.
        public static string ConstructorFunc(ThingWithComps item)
        {
            DebugActions.LogIfDebug("Running infinite reinforce patch.");
            if (item == null) return null;
            ThingComp_Reinforce reinforceComp = item.TryGetComp<ThingComp_Reinforce>();
            if (reinforceComp != null)
            {
                DebugActions.LogIfDebug("saving infinite reinforce comp...");
                return EncodeReinforcementComp(reinforceComp);
            }
            return null;
        }


        // helper function to encode reinforcement data into a single, dash-separated string.
        private static string EncodeReinforcementComp(ThingComp_Reinforce reinforceComp)
        {
            List<string> values = new List<string>();
            if (reinforceComp == null || reinforceComp.ReinforcedCount == 0) return null;

            values.Add(reinforcedField.Invoke(reinforceComp) + "");
            values.Add(discountField.Invoke(reinforceComp) + "");
            values.Add(EncodeFloatDict<StatDef>(statField.Invoke(reinforceComp)));
            values.Add(EncodeIntDict<StatDef>(statCountField.Invoke(reinforceComp)));
            values.Add(EncodeFloatDict<ReinforceDef>(customField.Invoke(reinforceComp)));
            values.Add(EncodeIntDict<ReinforceDef>(customCountField.Invoke(reinforceComp)));
            values.Add(difficultyField.Invoke(reinforceComp).ToString());
            return string.Join(fieldSep + "", values); // whoops don't use dashes cause of negative numbers
        }

        // helper function to decode infusion data and apply its values to an in-game comp.
        private static void ApplyEncodedComp(string EncodedComp, ThingComp_Reinforce reinforceComp)
        {
            DebugActions.LogIfDebug("Applying saved reinforcements to Mythic Item");
            if (EncodedComp == null) return;
            List<string> fields = EncodedComp.Split(fieldSep).ToList();
            if (fields.Count != 7)
            {
                DebugActions.LogErr("Found {0} entries when loading mythic item's infinite reinforcement data when we expected 6. Ignoring these values.", fields.Count);
                return;
            }
            int reinforced = 0;
            if(!int.TryParse(fields[0], out reinforced)) {

                DebugActions.LogErr("Failed to read reinforcement value when loading infinite reinforcement data for mythic item: {0}", fields[0]);
                return;
            }
            int discount = 0;
            if (!int.TryParse(fields[1], out discount))
            {

                DebugActions.LogErr("Failed to read discount value when loading infinite reinforcement data for mythic item: {0}", fields[1]);
                return;
            }
            Dictionary<StatDef, float> statBoost = DecodeFloatDict<StatDef>(fields[2]);
            Dictionary<StatDef, int> reinforcedCount = DecodeIntDict<StatDef>(fields[3]);
            Dictionary<ReinforceDef, float> custom = DecodeFloatDict<ReinforceDef>(fields[4]);
            Dictionary<ReinforceDef, int> customCount = DecodeIntDict<ReinforceDef>(fields[5]);
            IRDifficultFlag difficulty = IRDifficultFlag.None;
            if (!Enum.TryParse(fields[6], out difficulty))
            {

                DebugActions.LogErr("Failed to read difficulty value when loading infinite reinforcement data for mythic item: {0}", fields[6]);
                return;
            }
            // data checking
            // TODO LOL

            // done checking... actually set values
            reinforcedField.Invoke(reinforceComp) = reinforced;
            discountField.Invoke(reinforceComp) = discount;
            statField.Invoke(reinforceComp) = statBoost;
            statCountField.Invoke(reinforceComp) = reinforcedCount;
            customField.Invoke(reinforceComp) = custom;
            customCountField.Invoke(reinforceComp) = customCount;
            difficultyField.Invoke(reinforceComp) = difficulty;

            // find defs for each saved infusion, then override the comp's current infusions with them.

        }

        private static string EncodeIntDict<T>(Dictionary<T, int> input) where T : Def
        {
            string result = "";
            if (input == null) return result;
            foreach (KeyValuePair<T, int> pair in input.AsEnumerable())
            {
                result += pair.Key.defName + pairSep + pair.Value + listSep;
            }
            if (result.Length > 0) result = result.Substring(0, result.Length - 1); // remove the last pipe if non-empty
            return result;
        }

        private static string EncodeFloatDict<T>(Dictionary<T, float> input) where T : Def
        {
            string result = "";
            if (input == null) return result;
            foreach (KeyValuePair<T, float> pair in input.AsEnumerable())
            {
                result += pair.Key.defName + pairSep + string.Format("{0:N4}", pair.Value) + listSep;
            }
            if (result.Length > 0) result = result.Substring(0, result.Length - 1); // remove the last pipe if non-empty
            return result;
        }

        // honestly i should have further parameterized this to accept either int or float outputs... but I was lazy.
        private static Dictionary<T, int> DecodeIntDict<T>(string encodedDict) where T: Def
        {
            Dictionary<T, int> result = new Dictionary<T, int>();
            if (encodedDict == null || encodedDict.Length == 0) return result;
            foreach(string encodedPair in encodedDict.Split(listSep))
            {
                List<String> pair = encodedPair.Split(pairSep).ToList();
                if (pair.Count != 2)
                {
                    DebugActions.LogErr("While loading mythic item with infinite reinforce data, encountered an unreadable entry: {0}", encodedPair);
                    continue;
                }
                T def = DefDatabase<T>.GetNamedSilentFail(pair[0]);
                if (def == null)
                {
                    DebugActions.LogErr("While loading mythic item with infinite reinforce data, encountered an unknown def: {0}", pair[0] == null ? "null" : pair[0]);
                    continue;
                }
                int count = 0;
                if(!int.TryParse(pair[1], out count))
                {
                    DebugActions.LogErr("While loading mythic item with infinite reinforce data, encountered an unreadable count interget: {0}", pair[1] == null ? "null" : pair[1]);
                    continue;
                }
                result[def] = count;
            }
            return result;
        }

        private static Dictionary<T, float> DecodeFloatDict<T>(string encodedDict) where T : Def
        {
            Dictionary<T, float> result = new Dictionary<T, float>();
            if (encodedDict == null || encodedDict.Length == 0) return result;
            foreach (string encodedPair in encodedDict.Split(listSep))
            {
                List<String> pair = encodedPair.Split(pairSep).ToList();
                if (pair.Count != 2)
                {
                    DebugActions.LogErr("While loading mythic item with infinite reinforce data, encountered an unreadable entry: {0}", encodedPair);
                    continue;
                }
                T def = DefDatabase<T>.GetNamedSilentFail(pair[0]);
                if (def == null)
                {
                    DebugActions.LogErr("While loading mythic item with infinite reinforce data, encountered an unknown def: {0}", pair[0] == null ? "null" : pair[0]);
                    continue;
                }
                float value = 0;
                if (!float.TryParse(pair[1], out value))
                {
                    DebugActions.LogErr("While loading mythic item with infinite reinforce data, encountered an unreadable decimal: {0}", pair[1] == null ? "null" : pair[1]);
                    continue;
                }
                result[def] = value;
            }
            return result;
        }


    }
}
