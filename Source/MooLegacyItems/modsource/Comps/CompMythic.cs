using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This comp is what actually contains the data on an in-game item that makes it a mythic item, and 
 * gives that items the unusual text/abilities to show it. All items that could possibly be a mythic 
 * item have this comp, but it's only populated with data in actual mythic items.
 */
namespace MooMythicItems
{
    class CompMythic : ThingComp
    {
        // todo cache fully modified values to save overhead?
        public String newLabel = null;

        public String newDescription = null;
        
        public MythicEffectDef abilityDef = null;

        // Need to persist flavor and special abilities across saves
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<String>(ref newLabel, "newLabel");
            Scribe_Values.Look<String>(ref newDescription, "newDescription");
            Scribe_Defs.Look<MythicEffectDef>(ref abilityDef, "abilityDef"); 
        } 

        public override string TransformLabel(string label)
        {
            if (newLabel != null)
            {
                return newLabel;
            }

            return base.TransformLabel(label);
        }

        public override string GetDescriptionPart()
        {
            return newDescription; 
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (abilityDef == null)
            {
                return;
            }
            Apparel app = this.parent as Apparel;
            if (app != null)
            {
                this.abilityDef.ApparelEquipEffect(pawn, app);
            }
        }
    }
}
