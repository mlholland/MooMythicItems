using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This def is an effect that can be applied to a mythic item. When applied, it causes the mythic item to grant
 * the wielder/wearer an ability.
 */
namespace MooMythicItems
{
    class MythicEffectDef_GrantAbility : MythicEffectDef
    {
        public AbilityDef mythicAbilityDef;
        public bool startOnCooldown = false;
        private int coolDownLongTicks; // note: 30 long ticks == 1 in-game day
        private int coolDownLongTicksRemaining;

        private Ability ability;

        public override void OnEquip(Pawn pawn, ThingWithComps mythicItem)
        {
            // 1 long tick == 2000 normal ticks
            coolDownLongTicks = mythicAbilityDef.cooldownTicksRange.max / 2000;
            if (coolDownLongTicksRemaining == 0)
            {
                if (pawn == null || mythicAbilityDef == null)
                {
                    Log.Error(String.Format("[Moo Mythic Items] Tried to add a mythic ability to a pawn, but the inputted {0} was null", pawn == null ? "pawn" : "ability"));
                }

                // This is basically the same code as abilityTracker.addAbility, except we wrench it out in order to save the ability that's created.
                if (!pawn.abilities.abilities.Any((Ability a) => a.def == mythicAbilityDef))
                {
                    ability = AbilityUtility.MakeAbility(mythicAbilityDef, pawn);
                    pawn.abilities.abilities.Add(ability);
                    pawn.abilities.Notify_TemporaryAbilitiesChanged();
                    if (startOnCooldown)
                    {
                        ability.StartCooldown(mythicAbilityDef.cooldownTicksRange.RandomInRange);
                    }
                    foreach (CompAbilityEffect cae in ability.EffectComps)
                    {
                        if (cae is CompAbilityEffect_MythicCooldownSync syncher)
                        {
                            syncher.originatorMythicEffect = this;
                            return;
                        }
                    }
                    Log.Error(String.Format("[Moo Mythic Items] tried and failed to synchronize Mythic Item Bookkeeping code with ability cooldown code. Item-handoff bugs are possible."));
                } else
                {
                    Log.Warning(String.Format("[Moo Mythic Items] tried to grant pawn {0} the ability {1}, but they already have it. This could be a rare case of giving a pawn multiple mythic items, but is otherwise likely a bug of some sort", pawn.Name, mythicAbilityDef.label));
                }
                //pawn.abilities.GainAbility(mythicAbility);
            }
        }

        public override void OnUnequip(Pawn pawn, ThingWithComps mythicItem)
        { 
            if (pawn == null || mythicAbilityDef == null)
            {
                Log.Error(String.Format("[Moo Mythic Items] Tried to remove a mythic ability from a pawn, but the inputted {0} was null", pawn == null ? "pawn" : "ability"));
            }
            this.coolDownLongTicksRemaining = ability.CooldownTicksRemaining
            pawn.abilities.RemoveAbility(mythicAbilityDef); 
        }




        public override void LongTick(Pawn currentHolder)
        {
            if (coolDownLongTicksRemaining > 0)
            {
                coolDownLongTicksRemaining--;
                Log.Message("lowering cooldown to coolDownLongTicksRemaining");
            }
        }

        // TODO: remove ability
        // TODO: Cooldown associated with item
    }
}
