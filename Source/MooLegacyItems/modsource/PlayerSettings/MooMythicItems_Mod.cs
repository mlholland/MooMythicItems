using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace MooMythicItems
{
    public class MooMythicItems_Mod : Mod
    {

        public static MythicItemSettings settings;

        public MooMythicItems_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<MythicItemSettings>();
            new Harmony("rimworld.mooli").PatchAll();
        }

        public override string SettingsCategory() => "MooMF_SettingsTitle".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            settings.DoWindowContents(inRect);
            
        }
    }
}