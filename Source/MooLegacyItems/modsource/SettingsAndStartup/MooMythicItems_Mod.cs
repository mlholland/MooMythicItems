using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
/* Mod and startup boilerplate. Contains settings and runs all normal harmony patches, before running extra mod-conditional patches.*/
namespace MooMythicItems
{
    public class MooMythicItems_Mod : Mod
    {

        public static MythicItemSettings settings;
        public static Harmony harm;


        public MooMythicItems_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<MythicItemSettings>();
            harm = new Harmony("rimworld.moomf");
            harm.PatchAll();

            MythicItem_BladelinkAddon.AddBladelinkFunctionsToMythicItems();
            // Apply extra patches based on what other mods are present
            CheckForVEInsectoids();
            CheckForAlphaAnimals();
            //foreach (ModMetaData mod in ModLister.AllInstalledMods) Log.Message(mod.PackageId);
        }

        public override string SettingsCategory() => "MooMF_SettingsTitle".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            settings.DoWindowContents(inRect);

        }

        private void CheckForVEInsectoids()
        {
            // Count insectoids from VE Insectoids mod
            if (ModsConfig.IsActive("OskarPotocki.VFE.Insectoid")) 
            {
                System.Reflection.MethodInfo original = typeof(PawnKilledPatch_RecordsAndKillEffects).GetMethod("UpdateNewRecords");
                System.Reflection.MethodInfo patch = typeof(PawnKilledPatch_Compatibility).GetMethod(nameof(PawnKilledPatch_Compatibility.UpdateNewRecords_VEInsectoids));
                if (original == null || patch == null)
                {
                    Log.Error("Mythic Framework harmony patch for counting VE Insectoid kills failed to find necessary methods. Something went very wrong.");
                }
                else
                {
                    harm.Patch(original, postfix: new HarmonyMethod(patch));
                }
            }

        }

        private void CheckForAlphaAnimals()
        {
            // Count black hive insects from Alpha Animals mod
            if (ModsConfig.IsActive("sarg.alphaanimals")) 
            {
                System.Reflection.MethodInfo original = typeof(PawnKilledPatch_RecordsAndKillEffects).GetMethod("UpdateNewRecords");
                System.Reflection.MethodInfo patch = typeof(PawnKilledPatch_Compatibility).GetMethod(nameof(PawnKilledPatch_Compatibility.UpdateNewRecords_BlackHiveInsects));

                if (patch == null || original == null)
                {
                    Log.Error("Mythic Framework harmony patch for counting Black Hive kills failed to find necessary methods. Something went very wrong.");
                }
                else
                {
                    harm.Patch(original, postfix: new HarmonyMethod(patch));
                }
            }
        }
       
    }
}