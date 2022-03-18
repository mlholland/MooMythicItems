using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

/* 
 */
namespace MooMythicItems
{

   public class Precept_MythicRelic : Precept_Relic
    {
        public String newLabel = null;
        public String newDescription = null;
        public MythicEffectDef abilityDef = null;

        public Precept_MythicRelic() { }


        public Precept_MythicRelic(MythicItem mythicItem, Ideo ideo)
        {
            this.ideo = ideo;
            this.newLabel = mythicItem.GetFormattedTitle();
            this.newDescription = mythicItem.GetFormattedDescription();
            this.abilityDef = mythicItem.abilityDef;
            this.def = PreceptDefOf.IdeoRelic;
            this.name = mythicItem.GetFormattedTitle();
            this.descOverride = mythicItem.GetFormattedDescription();
            this.ThingDef = mythicItem.itemDef;
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
                Scribe_Defs.Look<MythicEffectDef>(ref this.abilityDef, "abilityDef");
            }
        }


    }
}