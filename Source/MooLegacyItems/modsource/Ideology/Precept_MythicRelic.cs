using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

/* A unique type of relic precept that stores extra information about the mythic item backing this relic.
 * Due to the nature of RW's code, this kind of inheritance can only get us so far. The actual use/functionality
 * of this precept had to be jammed into a harmony patch (Precept_Relic_GenerateRelic.cs).
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