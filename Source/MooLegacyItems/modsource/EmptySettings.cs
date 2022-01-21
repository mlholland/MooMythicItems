using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ModSource
{
    public class EmptySettings : ModSettings
    {
        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.Label("Test_Label".Translate());
            ls.End();
            base.Write();
        }
    }
}
