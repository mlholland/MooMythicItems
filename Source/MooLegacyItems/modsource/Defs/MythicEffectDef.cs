using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Parent for all defs that contain the functionality of mythic items. Due to the wide range of effects mythic items can apply,
 * it made sense to divide them into different defs with different inputs.
 * 
 * NOTE: this shoiu
 */
namespace MooMythicItems
{
    public class MythicEffectDef : Def
    {
        public MythicEffectDef() { }

        public virtual void ApparelEquipEffect(Pawn pawn, Apparel app) { }
    }
}
