using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This comp is creates a mythic item from the user. The mythic cause def to draw from is selected by ???????
 */
namespace MooMythicItems
{
    class CompUseEFfect_CreateMythicItemFromUser : CompUseEffect_WithDefInput
    {
        public static readonly string MythMakerReason = "MythMaker";

        // extracts the cause def from the input comp, then tries to make a mythic item. Destroys the comp parent (assumed to be an item) on a success.
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            CompUsable_OptionPerDef inputComp = this.parent.TryGetComp<CompUsable_OptionPerDef>();
            if (inputComp == null)
            {
                DebugActions.LogErr("Comp '{0}' which requires def input failed to find inputting sibling comp '{1}'", this.GetType().ToString(), typeof(CompUsable_OptionPerDef).ToString());
                return;
            }

            MythicCauseDef def = inputComp.lastSelectedDef as MythicCauseDef;
            if (def == null)
            {
                DebugActions.LogErr("Inputted def that wasn't actually a MythicCauseDef into the manual mythic maker code: {0}", inputComp.lastSelectedDef.defName);
            }

            DebugActions.LogIfDebug("Using myth maker to create a mythic item using causedef '{0}'", def.label);
            MythicItem item = def.TryCreateMythicItem(usedBy, MythMakerReason);
            if (item == null)
            {
                Messages.Message(string.Format("MooMF_MythMakerFailed".Translate(), usedBy.Name.ToStringFull), MessageTypeDefOf.RejectInput, true);
            } else
            {
                MythicItemCache.SaveNewMythicItem(item, String.Format("MooMF_MythMakerReason".Translate(), usedBy.Name.ToStringShort));
                this.parent.Destroy();
            }
        }

        public override string ParameterizeUseLabel(Def input)
        {
            MythicCauseDef def = input as MythicCauseDef;
            if (def == null)
            {
                DebugActions.LogErr("Inputted def that wasn't actually a MythicCauseDef into the manual mythic maker code: {0}", input.defName);
                return "???";
            }
            return string.Format(Props.parameterizeableUseLabel, def.createsMythicWeapon ? "MooMF_Weapon".Translate() : "MooMF_Apparel".Translate(), def.label);
        }
    }
}
