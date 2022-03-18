using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

/* 
 */
namespace MooLegacyItems
{

   public class Precept_LegacyRelic : Precept_Relic
    {
        public String newLabel = null;
        public String newDescription = null;
        public LegacyEffectDef abilityDef = null;

        public Precept_LegacyRelic() { }


        public Precept_LegacyRelic(LegacyItem legacyItem, Ideo ideo)
        {
            this.ideo = ideo;
            this.newLabel = legacyItem.GetFormattedTitle();
            this.newDescription = legacyItem.GetFormattedDescription();
            this.abilityDef = legacyItem.abilityDef;
            this.def = PreceptDefOf.IdeoRelic;
            this.name = legacyItem.GetFormattedTitle();
            this.descOverride = legacyItem.GetFormattedDescription();
            this.ThingDef = legacyItem.itemDef;
            this.PostMake();
        }

        public override string GenerateNameRaw()
        {
            return newLabel;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (!GameDataSaveLoader.IsSavingOrLoadingExternalIdeo)
            {
                Scribe_Values.Look<string>(ref this.newLabel, "newLabel", null, true);
                Scribe_Values.Look<string>(ref this.newDescription, "newDescription", null, true);
                Scribe_Defs.Look<LegacyEffectDef>(ref this.abilityDef, "abilityDef");
            }
        }


    }
}