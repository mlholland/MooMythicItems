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
        public static readonly string mythicEffectPrefixKey = "MooMF_AbilityDescription_Addon";

        public CompMythic() { }

        // todo cache fully modified values to save overhead?
        public String newLabel = null;

        public String newDescription = null;
        
        public MythicEffectDef abilityDef = null;
        
        // for arbitrary data storage by specific mythic effect logic.
        private string effectVal1 = null, effectVal2 = null, effectVal3 = null;

        // Need to persist flavor and special abilities across saves
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<String>(ref newLabel, "newLabel");
            Scribe_Values.Look<String>(ref newDescription, "newDescription");
            Scribe_Values.Look<String>(ref effectVal1, "effectVal1");
            Scribe_Values.Look<String>(ref effectVal2, "effectVal2");
            Scribe_Values.Look<String>(ref effectVal3, "effectVal3");
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
            if (abilityDef != null)
            {
                return newDescription + string.Format(mythicEffectPrefixKey.Translate(), abilityDef.EffectDescription(this.parent));
            }
            return newDescription; 
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            if (abilityDef != null)
            {
                this.abilityDef.OnEquip(pawn, this.parent, ref effectVal1, ref effectVal2, ref effectVal3);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            if (abilityDef != null)
            {
                this.abilityDef.OnUnequip(pawn, this.parent, ref effectVal1, ref effectVal2, ref effectVal3);
            }
        }


        public virtual void DoOnKillEffects(Pawn killed, Pawn killer)
        {
            if (abilityDef != null)
            {
                this.abilityDef.OnKill(killed, killer, this.parent, ref effectVal1, ref effectVal2, ref effectVal3);
            }
        }

    }
}
