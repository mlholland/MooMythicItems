using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Parent for all defs that contain the functionality of legacy items. Due to the wide range of effects legacy items can apply,
 * it made sense to divide them into different defs with different inputs.
 * 
 * NOTE: this shoiu
 */
namespace MooLegacyItems
{
    public class LegacyEffectDef : Def
    {
        public LegacyEffectDef() { }

        public virtual void ApparelEquipEffect(Pawn pawn, Apparel app) { }
    }
}
