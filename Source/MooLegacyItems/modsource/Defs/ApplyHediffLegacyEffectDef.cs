using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This def is causes a mythic item to apply an XML-specific hediff when equipped.
 */
namespace MooMythicItems
{
    class ApplyHediffMythicEffectDef : MythicEffectDef
    {

        public HediffDef equipHediff = null;

        public override void ApparelEquipEffect(Pawn pawn, Apparel app)
        {
            if (equipHediff == null)
            {
                Log.Error(String.Format("[Moo Mythic Items] ApplyHediffMythicEffectDef.ApparelEquipEffect {0} has no equipHediff, this is probably mis-formatted XML somewhere.", this.defName));
                return;
            }
            if (pawn == null || app == null)
            {
                Log.Error(String.Format("[Moo Mythic Items] ApplyHediffMythicEffectDef.ApparelEquipEffect {0} has received either a null pawn or apparel input", this.defName));
                return;
            }
            if (pawn.health.hediffSet.GetFirstHediffOfDef(equipHediff, false) == null)
            {
                HediffComp_RemoveIfApparelDropped hediffComp_RemoveIfApparelDropped = pawn.health.AddHediff(equipHediff).TryGetComp<HediffComp_RemoveIfApparelDropped>();
                if (hediffComp_RemoveIfApparelDropped != null)
                {
                    hediffComp_RemoveIfApparelDropped.wornApparel = app;
                } else
                {
                    Log.Error(String.Format("[Moo Mythic Items] ApplyHediffMythicEffectDef.ApparelEquipEffect '{0}' Tried to tie hediff '{1}' to worn apparel, but it didn't have a hediffComp_RemoveIfApparelDropped comp to tie in. The hediff is permanent now, and this is probably a screw-up my Moo.", this.defName, equipHediff.defName));
                }
            } 
        }
    }
}
