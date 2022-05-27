using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text;


/* Static utility methods related to saving and loading mythic items, 
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
        private static int s_fieldsPerLine
        {
            get
            {
                return 12;
            }
        }

        // If someone has this string in their pawn or settlement name... then RIP me I guess
        private static string s_commaReplacement
        {
            get
            {
                return "~~comma~~";
            }
        }

        // If someone has this string in their pawn or settlement name... then RIP me I guess
        private static string s_newLineReplacement
        {
            get
            {
                return "~~newline~~";
            }
        }

        private static string PATH_MODDIR
        {
            get
            {
                return GenFilePaths.ConfigFolderPath.Replace("Config", "Moo_MythicItems");
            }
        }
        
        private static string PATH_MythicITEMS
        {
            get
            {
                return PATH_MODDIR + Path.DirectorySeparatorChar.ToString() + "moo_mythicitems.csv";
            }
        }

        /* Loads the mythic items from the csv located at <RimworldFiles>/PATH_LEGACYITEMS. Mythic items with
         * invalid def or stuff types are ignored, and will be cleared from the file upon the next save.*/
        internal static List<MythicItem> LoadMythicItemsFile()
        {
            string mythicItemsFileText = "";
            List<MythicItem> results = new List<MythicItem>();
            try
            {
                bool flag = FileIO.Exists(PATH_MythicITEMS);
                if (flag)
                {
                    mythicItemsFileText = BytesToString(FileIO.ReadFile(PATH_MythicITEMS), Encoding.UTF8);
                } else {
                    return results;
                }
            }
            catch(Exception e)
            {
                DebugActions.LogErr("Failed to read mythic items from save file due the following error: {0}", e.Message); 
                return results;
            }
            string[] fileLines = mythicItemsFileText.Split('\n');
            foreach (string line in fileLines)
            { 
                string escapedLine = line.Replace(s_newLineReplacement, "\n");
                MythicItem parsedItem = ConvertStringToMythicItem(escapedLine);
                if (parsedItem != null) results.Add(parsedItem);
            }
            return results;
        }

        /*Saves the inputted list to <RimworldFiles>/PATH_LEGACYITEMS as a csv, overwriting whatever is currently saved there.*/
        internal static void SaveMythicItemsFile(List<MythicItem> mythicItems)
        {
            FileIO.CheckOrCreateDir(PATH_MODDIR);
            List<string> encodedItems = new List<string>();
            foreach (MythicItem item in mythicItems)
            {
                encodedItems.Add(ConvertMythicItemToString(item).Replace("\n", s_newLineReplacement));
            }
            FileIO.WriteFile(PATH_MythicITEMS, StringToBytes(string.Join("\n", encodedItems), Encoding.UTF8));
        }

        /* Assumes that a mythic item is encoded as a comma separated value with the
         * fields in the same order as they're listed in the MythicItem.cs file's field
         * ordering.
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
            List<int> worldsUsedIn = new List<int>();
            if (values[11].Length > 0)
            {
                foreach (string val in values[11].Split(':'))
                {
                    int intVal;
                    bool parsed = int.TryParse(val, out intVal);
                    if (!parsed)
                    {
                        DebugActions.LogErr("Failed to parse a string to an int as expected. The string was {0}", val);
                        continue;
                    }
                    worldsUsedIn.Add(intVal);
                }
            }

            ThingDef itemDef = DefDatabase<ThingDef>.GetNamed(values[0]);
            MythicEffectDef abiltyDef = DefDatabase<MythicEffectDef>.GetNamed(values[6]);
            ThingDef stuffDef = null;
            if (values[7].Length > 0)
            {
                stuffDef = DefDatabase<ThingDef>.GetNamed(values[7]);
            }


            return new MythicItem(itemDef, values[1], values[2], values[3], values[4], values[5], abiltyDef, stuffDef, int.Parse(values[8]), values[9], values[10], worldsUsedIn); 
        }


        /* Encodes a mythic item into a csv line, escaping commas.
         * Done this way because it wasn't worth looking up encoding libraries for something so small.
         */
        private static string ConvertMythicItemToString(MythicItem mythicItem)
        {
            string result = mythicItem.itemDef.defName.Replace(",", s_commaReplacement);
            result += "," + mythicItem.ownerFullName.Replace(",", s_commaReplacement);
            result += "," + mythicItem.ownerShortName.Replace(",", s_commaReplacement); 
            result += "," + mythicItem.factionName.Replace(",", s_commaReplacement);
            result += "," + mythicItem.descriptionTranslationString.Replace(",", s_commaReplacement);
            result += "," + mythicItem.titleTranslationString.Replace(",", s_commaReplacement);
            result += "," + mythicItem.abilityDef.defName.Replace(",", s_commaReplacement);
            result += "," + mythicItem.stuffDef.defName.Replace(",", s_commaReplacement);
            result += "," + mythicItem.prv;
            result += "," + mythicItem.reason.Replace(",", s_commaReplacement);
            result += "," + mythicItem.originatorId.Replace(",", s_commaReplacement);
            result += "," + string.Join(":", mythicItem.worldsUsedIn);
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
