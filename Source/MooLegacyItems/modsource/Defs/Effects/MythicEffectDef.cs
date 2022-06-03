using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Parent for all defs that contain the functionality of mythic items. Due to the wide range of effects mythic items can apply,
 * it made sense to divide them into different defs with different inputs.
 * 
 * Note that for all practical purposes, this is an abstract class. The only reason it's not implented as an abstract class or
 * an interface is to allow this class and its children to be saved by the Scribe_Defs feature to save overhead.
 * 
 * Remember, NO PERSISTENT DATA FOR SPECIFIC ITEMS CAN GO IN HERE. THAT's WHAT THE INPUTTED DATA FIELDS ARE FOR.
 */
namespace MooMythicItems
{
    public class MythicEffectDef : Def
    {
        public MythicEffectDef() { }

        public string effectDescTranslationKey = null;
        public List<string> extraDescriptionFields = new List<string>(); // For arbitary extra info that needs to be injected into a description.

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;
            if (effectDescTranslationKey == null) yield return "effectDescTranslationKey cannot be null";
        }

        public virtual void OnEquip(Pawn pawn, ThingWithComps mythicItem, ref string effectVal1, ref string effectVal2, ref string effectVal3) { }

        public virtual void OnUnequip(Pawn pawn, ThingWithComps mythicItem, ref string effectVal1, ref string effectVal2, ref string effectVal3) { }

        // Mythic weapons only
        public virtual void OnKill(Pawn killedPawn, Pawn killer, ThingWithComps mythicItem, ref string effectVal1, ref string effectVal2, ref string effectVal3) { }

        public virtual string EffectDescription(ThingWithComps mythicItem)
        {
            List<string> args = new List<string>() { mythicItem.def.label};
            args.AddRange(extraDescriptionFields);
            return string.Format(effectDescTranslationKey.Translate(), args.ToArray());
        }
    }
}
