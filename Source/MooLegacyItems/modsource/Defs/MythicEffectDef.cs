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
 */
namespace MooMythicItems
{
    public class MythicEffectDef : Def
    {
        public MythicEffectDef() { }
        
        public virtual void OnEquip(Pawn pawn, ThingWithComps mythicItem) { }

        public virtual void OnUnequip(Pawn pawn, ThingWithComps mythicItem) { }

        public virtual void Tick(Pawn currentHolder) { }
    }
}
