using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace MooLegacyItems
{
    public class MooLegacyItems_Mod : Mod
    {

        public static LegacyItemSettings settings;

        public MooLegacyItems_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<LegacyItemSettings>();
            new Harmony("rimworld.mooli").PatchAll();
        }

        public override string SettingsCategory() => "MooLI_SettingsTitle".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            settings.DoWindowContents(inRect);
            
        }
    }
}