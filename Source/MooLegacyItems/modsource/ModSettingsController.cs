using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace ModSource
{
    public class TestMod_Mod : Mod
    {

        public static EmptySettings settings;

        public TestMod_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<EmptySettings>();
        }

        public override string SettingsCategory() => "TestSettingsTitle".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            settings.DoWindowContents(inRect);
            
        }
    }
}