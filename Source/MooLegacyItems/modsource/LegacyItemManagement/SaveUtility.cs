using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text;


/* Static utility methods related to saving and loading legacy items, 
 * including a bunch of fluff to ensure fault tolerance related to file encoding,
 * and mod changes.
 * 
 * Only contains two accessible fields/functions: LoadLegacyItemsFile and SaveLegacyItemsFile
 */
namespace MooLegacyItems
{
    public class SaveUtility
    {
        // todo make this reflectively based on the actual legacyItems file? Or is that too gross to be worth?
        private static int s_fieldsPerLine
        {
            get
            {
                return 5;
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
                return GenFilePaths.ConfigFolderPath.Replace("Config", "Moo_LegacyItems");
            }
        }
        
        private static string PATH_LEGACYITEMS
        {
            get
            {
                return PATH_MODDIR + Path.DirectorySeparatorChar.ToString() + "moo_legacyitems.csv";
            }
        }

        internal static List<LegacyItem> LoadLegacyItemsFile()
        {
            string legacyItemsFileText = "";
            try
            {
                bool flag = FileIO.Exists(PATH_LEGACYITEMS);
                if (flag)
                {
                    legacyItemsFileText = BytesToString(FileIO.ReadFile(PATH_LEGACYITEMS), Encoding.UTF8);
                } else {
                    return null;
                }
            }
            catch(Exception e)
            {
                Log.Error(String.Format("Moo Legacy Items: Failed to read legacy items from save file due the following error: {0}", e.Message)); 
                return null;
            }
            string[] fileLines = legacyItemsFileText.Split('\n');
            List<LegacyItem> results = new List<LegacyItem>();
            foreach(string line in fileLines)
            {
                string escapedLine = line.Replace(s_newLineReplacement, "\n");
                try
                {
                    results.Add(ConvertStringToLegacyItem(escapedLine));
                } catch (InvalidCastException e)
                {
                    // skip bad lines
                }
            }
            return results;
        }

        internal static void SaveLegacyItemsFile(List<LegacyItem> legacyItems)
        {
            FileIO.CheckOrCreateDir(PATH_MODDIR);
            List<string> encodedItems = new List<string>();
            foreach (LegacyItem item in legacyItems)
            {
                encodedItems.Add(ConvertLegacyItemToString(item).Replace("\n", s_newLineReplacement));
            }
            FileIO.WriteFile(PATH_LEGACYITEMS, StringToBytes(string.Join("\n", encodedItems), Encoding.UTF8));
        }

        /* Assumes that a legacy item is encoded as a comma separated value with the
         * fields in the same order as they're listed in the LegacyItem.cs file's field
         * ordering.
         * 
         * Doing it this way because I don't want to learn encoding libraries for something so trivial.
         */
        private static LegacyItem ConvertStringToLegacyItem(string encodedLegacyItem)
        {
            string[] values = encodedLegacyItem.Split(',');

            if(values.Length != s_fieldsPerLine)
            {
                throw new InvalidCastException(String.Format("Moo Legacy Items: Tried and failed to read a legacy item from the legacy item save file. This is probably due to either data corruption or file modification outside the game. Expected {0} comma separated values and found {1}, Line was '{2}'", s_fieldsPerLine, values.Length, encodedLegacyItem));
            }
            for (int i = 0; i < values.Length; i++)
            {
                if(values[i].Length == 0)
                {
                    throw new InvalidCastException(String.Format("Moo Legacy Items: Encountered an empty legacy item field from the legacy item save file. The following line did not have the expected {0} non-empty comma separated values: {1}", s_fieldsPerLine, encodedLegacyItem));
                }
                values[i] = values[i].Replace(s_commaReplacement, ",");
            }
            return new LegacyItem(values[0], values[1], values[2], values[3], values[4]); 
        }

        /* Encodes a legacy item into a csv line, escaping commas.
         * Done this way because it wasn't worth looking up encoding libraries for something so small.
         */
        private static string ConvertLegacyItemToString(LegacyItem legacyItem)
        {
            string result = legacyItem.itemDefName.Replace(",", s_commaReplacement);
            result += "," + legacyItem.originatorName.Replace(",", s_commaReplacement);
            result += "," + legacyItem.originatorColonyName.Replace(",", s_commaReplacement);
            result += "," + legacyItem.storyLabel.Replace(",", s_commaReplacement);
            result += "," + legacyItem.abilityLabel.Replace(",", s_commaReplacement);
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
