using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

/* This comp is required on all abilityDefs that represent the active abilities that are gained by using mythic items.
 * It is given a reference back to the MythicAbilityDef that made this ability, that way cooldowns can be set on a per-item basis rather than per-colonist, thereby avoiding potential
 * abuses by giving an item to different people to reuse it quickly.
 */
namespace MooMythicItems
{
    class CompAbilityEffect_MythicCooldownSync : CompAbilityEffect
    {
        public CompAbilityEffect_MythicCooldownSync() { }

        // todo cache fully modified values to save overhead?
        public MythicEffectDef_GrantAbility originatorMythicEffect  = null; // TODO Scribe this?
         
        
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        { 
            if (originatorMythicEffect == null)
            {
                Log.Error("[Moo Mythic Items] Tried to apply a mythic ability's cooldown to the associated mythic item, but the relevant comp doesn't have a saved mythic item reference.");
                return;
            } 
            originatorMythicEffect.coolDownLongTicksRemaining = originatorMythicEffect.coolDownLongTicks;
        }
        public override void Apply(GlobalTargetInfo target)
        { 
            if (originatorMythicEffect == null)
            {
                Log.Error("[Moo Mythic Items] Tried to apply a mythic ability's cooldown to the associated mythic item, but the relevant comp doesn't have a saved mythic item reference.");
                return;
            }
            originatorMythicEffect.coolDownLongTicksRemaining = originatorMythicEffect.coolDownLongTicks;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return true;
        }

        // Token: 0x06005062 RID: 20578 RVA: 0x00012A6D File Offset: 0x00010C6D
        public override bool CanApplyOn(GlobalTargetInfo target)
        {
            return true;
        }
    }
}
