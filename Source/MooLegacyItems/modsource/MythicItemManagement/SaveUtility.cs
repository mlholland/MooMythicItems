using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text;


/* Static utility functions related to saving and loading mythic items, 
 * including a bunch of fluff to ensure fault tolerance related to file encoding,
 * and mod changes.
 * 
 * Only contains two accessible fields/functions: LoadMythicItemsFile and SaveMythicItemsFile
 */
namespace MooMythicItems
{
    public class SaveUtility
    {
        // todo make this reflectively based on the actual mythicItems file? Or is that too gross to be worth?
        private static readonly int s_fieldsPerLine = 15;

        // If someone has this string in their pawn or settlement name... then RIP this mod I guess
        private static readonly string s_commaReplacement = "~~comma~~";
        private static readonly string s_newLineReplacement = "~~newline~~"; 
        private static readonly string s_pipeReplacement = "~~pipe~~";
        private static readonly string s_ColonReplacement = "~~colon~~";

        private static string pathModDirectory
        {
            get
            {
                return GenFilePaths.ConfigFolderPath.Replace("Config", "Moo_MythicItems");
            }
        }
        
        private static string pathMythicItemsFile
        {
            get
            {
                return pathModDirectory + Path.DirectorySeparatorChar.ToString() + "moo_mythicitems.csv";
            }
        }

        private static string pathInvalidMythicItemsFile
        {
            get
            {
                return pathModDirectory + Path.DirectorySeparatorChar.ToString() + "moo_mythicitems.invalid.csv";
            }
        }


        /* Loads the mythic items from the csv located at <RimworldFiles>/PATH_LEGACYITEMS. Mythic items with
         * invalid mythic items (which can occur if other mods are removed due to missing defs) are returned in 
         * a secondary list in raw string form.*/
        internal static Tuple<List<MythicItem>, List<string>> LoadMythicItemsFile()
        {
            string mythicItemsFileText = "";
            try
            {
                bool flag = FileIO.Exists(pathMythicItemsFile);
                if (flag)
                {
                    mythicItemsFileText = BytesToString(FileIO.ReadFile(pathMythicItemsFile), Encoding.UTF8);
                } else {
                    return null;
                }
            }
            catch(Exception e)
            {
                DebugActions.LogErr("Failed to read mythic items from save file due the following error: {0}", e.Message); 
                return null;
            }
            return DecodeMythicItems(mythicItemsFileText.Split('\n').ToList());
        }

        /*Saves the inputted list to <RimworldFiles>/PATH_LEGACYITEMS as a csv, overwriting whatever is currently saved there.*/
        internal static void SaveMythicItemsFile(List<MythicItem> mythicItems)
        {
            FileIO.CheckOrCreateDir(pathModDirectory);
            List<string> encodedItems = new List<string>();
            foreach (MythicItem item in mythicItems)
            {
                encodedItems.Add(ConvertMythicItemToCSVString(item).Replace("\n", s_newLineReplacement));
            }
            FileIO.WriteFile(pathMythicItemsFile, StringToBytes(string.Join("\n", encodedItems), Encoding.UTF8));
        }

        internal static void SaveInvalidMythicItemsFile(List<string> mythicItems)
        {
            FileIO.CheckOrCreateDir(pathModDirectory);
            if (mythicItems == null || mythicItems.Count == 0)
            {
                FileIO.WriteFile(pathInvalidMythicItemsFile, StringToBytes("", Encoding.UTF8));
            }
            FileIO.WriteFile(pathInvalidMythicItemsFile, StringToBytes(string.Join("\n", mythicItems), Encoding.UTF8));
        }

        internal static void AddToInvalidMythicItemsFile(List<string> mythicItems)
        {
            if (mythicItems == null || mythicItems.Count == 0) return;
            FileIO.CheckOrCreateDir(pathModDirectory);
            string previous = ReadInvalidMythicItemsFileAsText();
            if (previous == null || previous.Length == 0)
            {
                FileIO.WriteFile(pathInvalidMythicItemsFile, StringToBytes(string.Join("\n", mythicItems), Encoding.UTF8));
                return;
            }
            FileIO.WriteFile(pathInvalidMythicItemsFile, StringToBytes(ReadInvalidMythicItemsFileAsText() + "\n" +  string.Join("\n", mythicItems), Encoding.UTF8));
        }

        internal static string ReadInvalidMythicItemsFileAsText()
        {
            string invalidMythicItemsFileText = "";
            try
            {
                bool flag = FileIO.Exists(pathInvalidMythicItemsFile);
                if (flag)
                {
                    invalidMythicItemsFileText = BytesToString(FileIO.ReadFile(pathInvalidMythicItemsFile), Encoding.UTF8);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception e)
            {
                DebugActions.LogErr("Failed to read mythic items from save file due the following error: {0}", e.Message);
                return "";
            }
            return invalidMythicItemsFileText;
        }

        internal static Tuple<List<MythicItem>, List<string>> ReadInvalidMythicItemsFile()
        {
            FileIO.CheckOrCreateDir(pathModDirectory);
            return DecodeMythicItems(ReadInvalidMythicItemsFileAsText().Split('\n').ToList());
        }


        // returns valid mythic items as mythic items, and invalid mythic items (usually due to mod removals) in their raw string form.
        private static Tuple<List<MythicItem>, List<string>> DecodeMythicItems(List<string> encodedMythicItems)
        {
            List<MythicItem> results = new List<MythicItem>();
            List<string> badResults = new List<string>();
            foreach (string line in encodedMythicItems)
            {
                if (line.Trim().Length == 0) continue;
                string escapedLine = line.Replace(s_newLineReplacement, "\n");
                MythicItem parsedItem = ConvertStringToMythicItem(escapedLine);
                if (parsedItem != null) results.Add(parsedItem);
                else badResults.Add(line);
            }
            return new Tuple<List<MythicItem>, List<string>>(results, badResults);
        }

        /* Assumes that a mythic item is encoded as a comma separated value with the
         * fields in the same order as they're listed in the MythicItem.cs file's field
         * ordering. Also assumes that list values are separated by pipes "|", and dictionary pairs are separated by colons ":".
         * 
         * Doing it this way because I don't want to learn/involve encoding libraries for something so trivial.
         */
        private static MythicItem ConvertStringToMythicItem(string encodedMythicItem)
        {
            // ignore empty lines.
            if (encodedMythicItem.Trim().Length == 0) return null;

            string[] values = encodedMythicItem.Split(',');

            if(values.Length != s_fieldsPerLine)
            {
                DebugActions.LogErr("Tried and failed to read a mythic item from the mythic item save file. This is probably due to either data corruption or file modification outside the game. Expected {0} comma separated values and found {1}. The string in question was: '{2}'", s_fieldsPerLine, values.Length, encodedMythicItem);
                return null;
            }
            for (int i = 0; i < values.Length; i++)
            {

                values[i] = values[i].Replace(s_commaReplacement, ",");
            }
            List<int> worldsUsedIn = ConvertArgToIntList(values[11]);
            List<string> extraTitles = ConvertArgToStringList(values[12]);
            List<string> extraDescriptions= ConvertArgToStringList(values[13]);
            Dictionary<string, string> extraInfo = ConvertArgToStringDict(values[14]);

            ThingDef itemDef = DefDatabase<ThingDef>.GetNamed(values[0], false);
            if (itemDef == null)
            {
                DebugActions.LogWarn("Tried and failed to load a mythic item from the mythic save file because we could not recognize the item {0}. It's probably an item from an inactive mod.", values[0]);
                return null;
            }
            MythicEffectDef abiltyDef = DefDatabase<MythicEffectDef>.GetNamed(values[6]);
            ThingDef stuffDef = null;
            if (values[7].Length > 0)
            {
                stuffDef = DefDatabase<ThingDef>.GetNamed(values[7]);
            }


            return new MythicItem(itemDef, values[1], values[2], values[3], values[4], values[5], abiltyDef, stuffDef, int.Parse(values[8]), values[9], values[10], worldsUsedIn, extraTitles, extraDescriptions, extraInfo); 
        }


        private static List<int> ConvertArgToIntList(string arg)
        {
            List<int> result = new List<int>();
            if (arg == null || arg.Length == 0) return result;
            // each value is just expected to be a number, not user-produced text of some sort, so don't bother with sanitizing.
            foreach (string val in arg.Split('|'))
            {
                int intVal;
                bool parsed = int.TryParse(val, out intVal);
                if (!parsed)
                {
                    DebugActions.LogErr("Failed to parse a string to an int as expected while loading saved mythic items. The string was {0}", val);
                    continue;
                }
                result.Add(intVal);
            }
            return result;
        }

        private static string ConvertIntListToArg(List<int> input)
        {
            if (input == null || input.Count == 0) return "";
            return string.Join("|", input);
        }

        private static List<string> ConvertArgToStringList(string arg)
        {
            List<string> result = new List<string>();
            if (arg == null || arg.Length == 0) return result;
            // each value is just expected to be a number, not user-produced text of some sort, so don't bother with sanitizing.
            foreach (string val in arg.Split('|'))
            {
                result.Add(val.Replace(s_pipeReplacement, "|"));
            }
            return result;
        }

        private static string ConvertStringListToArg(List<String> input)
        {
            string result = "";
            if (input == null || input.Count == 0) return result;
            foreach(string val in input)
            {
                result += val.Replace("|", s_pipeReplacement);
                result += "|";
            }
            if (result.Length > 0) result = result.Substring(0, result.Length - 1); // remove the last pipe if non-empty
            return result;
        }

        private static Dictionary<string, string> ConvertArgToStringDict(string arg)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (arg == null || arg.Length == 0) return result;
            // each value is just expected to be a number, not user-produced text of some sort, so don't bother with sanitizing.
            foreach (string val in arg.Split('|'))
            {
                string[] split = val.Split(':');
                if (split == null || split.Length != 2)
                {
                    DebugActions.LogErr("Tried to parse invalid key-value pair while reading a dictionary. The value was {0}. It should have been two string separated by a single colon ':'.", val);
                    continue;
                }
                result.Add(split[0].Replace(s_pipeReplacement, "|").Replace(s_ColonReplacement, ":"), split[1].Replace(s_pipeReplacement, "|").Replace(s_ColonReplacement, ":"));
            }
            return result;
        }

        private static string ConvertStringDictToArg(Dictionary<string, string> input)
        {
            string result = "";
            if (input == null) return result;
            foreach(KeyValuePair<string, string> pair in input.AsEnumerable())
            {
                result += pair.Key.Replace("|", s_pipeReplacement).Replace(":", s_ColonReplacement);
                result += ":";
                result += pair.Value.Replace("|", s_pipeReplacement).Replace(":", s_ColonReplacement);
                result += "|";
            }
            if (result.Length > 0) result = result.Substring(0, result.Length - 1); // remove the last pipe if non-empty
            return result;
        }

        /* Encodes a mythic item into a csv line, escaping commas.
         * Done this way because it wasn't worth looking up encoding libraries for something so small.
         */
        private static string ConvertMythicItemToCSVString(MythicItem mythicItem)
        {
            string result = mythicItem.itemDef.defName.Replace(",", s_commaReplacement); 
            result += "," + mythicItem.ownerFullName.Replace(",", s_commaReplacement); 
            result += "," + mythicItem.ownerShortName.Replace(",", s_commaReplacement); 
            result += "," + mythicItem.factionName.Replace(",", s_commaReplacement); 
            result += "," + mythicItem.descriptionTranslationString.Replace(",", s_commaReplacement); 
            result += "," + mythicItem.titleTranslationString.Replace(",", s_commaReplacement); 
            result += "," + mythicItem.abilityDef.defName.Replace(",", s_commaReplacement); 
            result += "," + (mythicItem.stuffDef == null ? "" : mythicItem.stuffDef.defName.Replace(",", s_commaReplacement)); // this, unlike most other values, can be null 
            result += "," + mythicItem.prv; 
            result += "," + mythicItem.reason.Replace(",", s_commaReplacement); 
            result += "," + mythicItem.originatorId.Replace(",", s_commaReplacement); 
            result += "," + ConvertIntListToArg(mythicItem.worldsUsedIn); 
            result += "," + ConvertStringListToArg(mythicItem.extraTitleValues); 
            result += "," + ConvertStringListToArg(mythicItem.extraDescriptionValues); 
            result += "," + ConvertStringDictToArg(mythicItem.extraItemData);
            return result;
        }

        private static byte[] StringToBytes(string text, Encoding enc)
        {
            bool flag = text == null;
            byte[] result;
            if (flag)
            {
                result = null;
            }
            else
            {
                bool flag2 = enc == null;
                if (flag2)
                {
                    result = Encoding.Default.GetBytes(text);
                }
                else
                {
                    result = enc.GetBytes(text);
                }
            }
            return result;
        }

        private static string BytesToString(byte[] input, Encoding enc)
        {
            bool flag = input.NullOrEmpty<byte>();
            string result;
            if (flag)
            {
                result = "";
            }
            else
            {
                result = enc.GetString(input);
            }
            return result;
        }

    }
}
