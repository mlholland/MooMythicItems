using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

/* Extracted from the character editor mod, since that's the only mod I know that has 
 * colony-agnostic save data.
 */
namespace MooMythicItems
{
    public class FileIO
    {
        public static bool ExistsDir(string path)
        {
            return Directory.Exists(path);
        }
        
        public static void CheckOrCreateDir(string path)
        {
            bool flag = !Directory.Exists(path);
            if (flag)
            {
                Directory.CreateDirectory(path);
            }
        }
        
        public static string[] GetDirFolderList(string path, string searchPattern, bool rekursiv = true)
        {
            bool flag = !Directory.Exists(path);
            string[] result;
            if (flag)
            {
                result = null;
            }
            else
            {
                try
                {
                    result = Directory.GetDirectories(path, searchPattern, rekursiv ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                }
                catch (UnauthorizedAccessException)
                {
                    result = null;
                }
                catch
                {
                    result = null;
                }
            }
            return result;
        }
        
        public static string[] GetDirFileList(string path, string searchPattern, bool rekursiv = true)
        {
            bool flag = !Directory.Exists(path);
            string[] result;
            if (flag)
            {
                result = null;
            }
            else
            {
                try
                {
                    result = Directory.GetFiles(path, searchPattern, rekursiv ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                }
                catch (UnauthorizedAccessException)
                {
                    result = null;
                }
                catch
                {
                    result = null;
                }
            }
            return result;
        }
        
        public static bool WriteFile(string filepath, byte[] bytes)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }
        
        public static bool Exists(string filepath)
        {
            return File.Exists(filepath);
        }
        
        public static byte[] ReadFile(string filepath)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] array = new byte[(int)fileStream.Length];
                fileStream.Read(array, 0, (int)fileStream.Length);
                fileStream.Close();
                return array;
            }
            catch (Exception ex)
            {
                bool flag = fileStream != null;
                if (flag)
                {
                    fileStream.Close();
                }
                Log.Error(ex.Message);
            }
            return new byte[0];
        }
    }
}

