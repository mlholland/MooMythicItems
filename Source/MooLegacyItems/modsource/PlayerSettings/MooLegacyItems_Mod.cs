using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace MooLegacyItems
{
    public class MooLegacyItems_Mod : Mod
    {

        public static LegacyItemSettings settings;

        public MooLegacyItems_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<LegacyItemSettings>();
        }

        public override string SettingsCategory() => "MooLI_SettingsTitle".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            settings.DoWindowContents(inRect);
            
        }
    }
}