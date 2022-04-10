using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

/* Mod boilerplate. Contains settings and runs all normal harmony patches.*/
namespace MooMythicItems
{
    public class MooMythicItems_Mod : Mod
    {

        public static MythicItemSettings settings;
        public static Harmony harm;

        public MooMythicItems_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<MythicItemSettings>();
            harm = new Harmony("rimworld.mooli");
            harm.PatchAll();
        }

        public override string SettingsCategory() => "MooMF_SettingsTitle".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            settings.DoWindowContents(inRect);
            
        }
    }
}