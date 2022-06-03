using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This def is causes a mythic item to apply an XML-specific hediff when equipped. Only works for clothing-based items, not weapons.
 */
namespace MooMythicItems
{
    class MythicEffectDef_EquipHediff : MythicEffectDef
    {
        public HediffDef equipHediff = null;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;
            if (equipHediff == null) yield return "equipHediff cannot be null";
        }

        public override void OnEquip(Pawn pawn, ThingWithComps mythicItem, ref string effectVal1, ref string effectVal2, ref string effectVal3)
        {
            if (equipHediff == null)
            {
                DebugActions.LogErr("ApplyHediffMythicEffectDef.ApparelEquipEffect {0} has no equipHediff, this is probably mis-formatted XML somewhere.", this.defName);
                return;
            }

            // If mythic item is apparel, don't both with paired logic between equip/unequip functions.
            // Instead, just leverage the existing hediffComp_RemoveIfApparelDropped hediff comp
            if (mythicItem is Apparel app)
            {

                if (pawn == null || app == null)
                {
                    DebugActions.LogErr("ApplyHediffMythicEffectDef.ApparelEquipEffect {0} has received either a null pawn or apparel input", this.defName);
                    return;
                }
                if (pawn.health.hediffSet.GetFirstHediffOfDef(equipHediff, false) == null)
                {
                    HediffComp_RemoveIfApparelDropped hediffComp_RemoveIfApparelDropped = pawn.health.AddHediff(equipHediff).TryGetComp<HediffComp_RemoveIfApparelDropped>();
                    if (hediffComp_RemoveIfApparelDropped != null)
                    {
                        hediffComp_RemoveIfApparelDropped.wornApparel = app;
                    }
                    else
                    {
                        DebugActions.LogErr("ApplyHediffMythicEffectDef.ApparelEquipEffect '{0}' Tried to tie hediff '{1}' to worn apparel, but it didn't have a hediffComp_RemoveIfApparelDropped comp to tie in. The hediff is permanent now, and this is probably a screw-up by Moo or whoever made this effect.", this.defName, equipHediff.defName);
                    }
                }
            } else
            {
                // TODO look into how other mods add hediffs on weapons, and try to make this more robust?
                pawn.health.AddHediff(equipHediff);
            }
        }


        public override void OnUnequip(Pawn pawn, ThingWithComps mythicItem, ref string effectVal1, ref string effectVal2, ref string effectVal3)
        {
            if (equipHediff == null)
            {
                // We already complained on equip.
                return;
            }

            // If mythic item is apparel, don't both with paired logic between equip/unequip functions.
            // Instead, just leverage the existing hediffComp_RemoveIfApparelDropped hediff comp
            if (!(mythicItem is Apparel app))
            {
                Hediff targetHediff = pawn.health.hediffSet.GetFirstHediffOfDef(equipHediff);
                if (targetHediff == null)
                {
                    DebugActions.LogErr("A mythic {0} is supposed to remove an associated hediff {1} when unequipped, but no such hediff could be found on wielder {2} when an unequip occurred.", mythicItem.Label, equipHediff.label, pawn.Name.ToStringFull);
                    return;
                }
                pawn.health.RemoveHediff(targetHediff);
            }
        }

        public override string EffectDescription(ThingWithComps mythicItem)
        {
            List<string> args = new List<string>() { mythicItem.def.label, equipHediff.label };
            args.AddRange(extraDescriptionFields);
            return string.Format(effectDescTranslationKey.Translate(), args.ToArray());
        }
    }
}
