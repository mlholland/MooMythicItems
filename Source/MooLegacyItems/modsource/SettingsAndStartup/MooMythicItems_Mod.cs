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

            // Apply extra patches based on what other mods are present
            CheckForVEInsectoids();
            CheckForAlphaAnimals();
            CheckForRimworldOfMagic();

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

        private void CheckForRimworldOfMagic()
        { 
            if (ModsConfig.IsActive("Torann.ARimworldOfMagic") || ModsConfig.IsActive("torann.arimworldofmagic_steam"))
            {
                if(!RimworldOfMagic_Compatibility.InitializeReflectionValues())
                {
                    Log.Error("Mythic Framework failed to find all values needed to work with Rimworld of Magic Patch. Enchant data will not be saved across worlds.");
                    return;
                }


                // Load non-standard enchantment data from Rimworld of Magic
                System.Reflection.MethodInfo original_realize = typeof(MythicItem).GetMethod("Realize");
                System.Reflection.MethodInfo patch_realize = typeof(RimworldOfMagic_Compatibility).GetMethod(nameof(RimworldOfMagic_Compatibility.RealizePatch));
                if (original_realize == null)
                {
                    Log.Error("Mythic Framework harmony patch for load Rimworld of Magic Enchantment data failed to find realize function. Something went very wrong.");
                    return;
                }
                else if (patch_realize == null)
                {
                    Log.Error("Mythic Framework harmony patch for load Rimworld of Magic Enchantment data failed to find realize patch function. Something went very wrong.");
                    return;
                }
                else
                {
                    harm.Patch(original_realize, postfix: new HarmonyMethod(patch_realize));
                }

                // Save non-standard enchantment data from Rimworld of Magic
                System.Reflection.ConstructorInfo original_constructor = typeof(MythicItem).GetConstructor(new Type[] {
                    typeof(ThingWithComps),
                    typeof(Pawn),
                    typeof(string),
                    typeof(string),
                    typeof(MythicEffectDef),
                    typeof(string),
                    typeof(List<string>),
                    typeof(List<string>),
                    typeof(Dictionary<string, string>)
                });
                System.Reflection.MethodInfo patch_constructor = typeof(RimworldOfMagic_Compatibility).GetMethod(nameof(RimworldOfMagic_Compatibility.CreationPatch));
                if (original_constructor == null)
                {
                    Log.Error("Mythic Framework harmony patch for save Rimworld of Magic Enchantment data failed to find constructor. Something went very wrong.");
                    return;
                }
                else if (patch_constructor == null)
                {
                    Log.Error("Mythic Framework harmony patch for save Rimworld of Magic Enchantment data failed to find constructor patch function. Something went very wrong.");
                    return;
                }
                else
                {
                    harm.Patch(original_constructor, postfix: new HarmonyMethod(patch_constructor));
                }
            }
        }
    }
}