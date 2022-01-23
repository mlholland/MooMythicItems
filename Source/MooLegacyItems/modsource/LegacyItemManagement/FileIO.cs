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
namespace MooLegacyItems
{
    public class FileIO
    {
        // Token: 0x060001FC RID: 508 RVA: 0x0000ECC0 File Offset: 0x0000CEC0
        public static bool ExistsDir(string path)
        {
            return Directory.Exists(path);
        }

        // Token: 0x060001FD RID: 509 RVA: 0x0000ECD8 File Offset: 0x0000CED8
        public static void CheckOrCreateDir(string path)
        {
            bool flag = !Directory.Exists(path);
            if (flag)
            {
                Directory.CreateDirectory(path);
            }
        }

        // Token: 0x060001FE RID: 510 RVA: 0x0000ECFC File Offset: 0x0000CEFC
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

        // Token: 0x060001FF RID: 511 RVA: 0x0000ED58 File Offset: 0x0000CF58
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

        // Token: 0x17000041 RID: 65
        // (get) Token: 0x06000200 RID: 512 RVA: 0x0000EDB4 File Offset: 0x0000CFB4
        public static string PATH_DESKTOP
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
        }

        // Token: 0x17000042 RID: 66
        // (get) Token: 0x06000201 RID: 513 RVA: 0x0000EDBC File Offset: 0x0000CFBC
        public static string PATH_PAWNEX
        {
            get
            {
                return FileIO.PATH_DESKTOP + "\\pawnslots.txt";
            }
        }

        // Token: 0x06000202 RID: 514 RVA: 0x0000EDD0 File Offset: 0x0000CFD0
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

        // Token: 0x06000203 RID: 515 RVA: 0x0000EE40 File Offset: 0x0000D040
        public static bool Exists(string filepath)
        {
            return File.Exists(filepath);
        }

        // Token: 0x06000204 RID: 516 RVA: 0x0000EE58 File Offset: 0x0000D058
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

