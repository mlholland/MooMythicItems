using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

/* This def is an effect that can be applied to a mythic item. When applied, it causes the mythic item to grant
 * the wielder/wearer an ability.
 * 
 * DEV NOTE: THis Mythic effect uses the following inputted values in the following ways:
 *  - effectVal1 is the last time the mythic item was unequipped in ticks.
 *  - effectVal2 is the remaining cooldown in ticks for the mythic item's associted ability when it was last unequipped.
 */
namespace MooMythicItems
{
    class MythicEffectDef_GrantAbility : MythicEffectDef
    {
        public bool startOnCooldown = false;
        public bool coolDownWhileUnequipped = true;
        public AbilityDef mythicAbilityDef;
        
        // need both for logic because coolDownWhileUnequipped is toggle-able
        private static readonly string tickKey = "tickKey";
        private static readonly string cooldownRemaining = "cdKey";
        private static readonly AccessTools.FieldRef<object, int> coolDownTicksField = AccessTools.FieldRefAccess<int>(typeof(Ability), "cooldownTicks");

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;
            if (mythicAbilityDef == null) yield return "mythicAbilityDef cannot be null";
            if (mythicAbilityDef.label == null) yield return "mythicAbilityDef label cannot be null";
            if (coolDownWhileUnequipped && startOnCooldown) yield return "coolDownWhileUnequipped and startOnCooldown should not both be true. The associated ability will always just start on cooldown.";
        }

        public override void OnEquip(Pawn pawn, ThingWithComps mythicItem, ref string effectVal1, ref string effectVal2, ref string effectVal3)
        { 
            if (pawn == null || mythicAbilityDef == null)
            {
                DebugActions.LogErr("Tried to add a mythic ability to a pawn, but the inputted {0} was null", pawn == null ? "pawn" : "ability");
            }

            // The first four lines of this are basically the same code as abilityTracker.addAbility. We wrench it out in order to save the ability that's created.
            if (!pawn.abilities.abilities.Any((Ability a) => a.def == mythicAbilityDef))
            {
                Ability ability = AbilityUtility.MakeAbility(mythicAbilityDef, pawn);
                foreach (Command c in ability.GetGizmos())
                {
                    c.icon = mythicItem.def.uiIcon;
                    if (mythicItem.Stuff != null)
                    {
                        c.defaultIconColor = mythicItem.Stuff.stuffProps.color;
                    }
                }
                pawn.abilities.abilities.Add(ability);
                pawn.abilities.Notify_TemporaryAbilitiesChanged();
                int curTick = Find.TickManager.TicksGame;
                int lastTickUnequipped = getSavedTickValue(effectVal1);
                int cooldownTicksRemaining = getSavedTickValue(effectVal2);
                if (startOnCooldown)
                {
                    ability.StartCooldown(mythicAbilityDef.cooldownTicksRange.RandomInRange);
                }
                else if (cooldownTicksRemaining != 0) // if this is non-zero, then this item was dropped while its mythic ability was on cooldown.
                {
                    ability.StartCooldown(mythicAbilityDef.cooldownTicksRange.RandomInRange);
                    ref int abilityCooldownTicks = ref coolDownTicksField.Invoke(ability);
                    if (!coolDownWhileUnequipped)
                    {
                        abilityCooldownTicks = cooldownTicksRemaining;
                    } else if (curTick < lastTickUnequipped + cooldownTicksRemaining)
                    {
                        abilityCooldownTicks = lastTickUnequipped + cooldownTicksRemaining - curTick;
                    }
                }
            } else
            {
                DebugActions.LogWarn("tried to grant pawn {0} the ability {1}, but they already have it. This could be a rare case of giving a pawn multiple mythic items, but is otherwise likely a bug of some sort", pawn.Name, mythicAbilityDef.label);
            }
            //pawn.abilities.GainAbility(mythicAbility); 
        }

        public override void OnUnequip(Pawn pawn, ThingWithComps mythicItem, ref string effectVal1, ref string effectVal2, ref string effectVal3)
        { 
            if (pawn == null || mythicAbilityDef == null)
            {
                DebugActions.LogErr("Tried to remove a mythic ability from a pawn, but the inputted {0} was null", pawn == null ? "pawn" : "ability");
            }
            effectVal1 = Find.TickManager.TicksGame.ToString();

            Ability ability = pawn.abilities.GetAbility(mythicAbilityDef);
            if (ability == null)
            {
                DebugActions.LogErr("A pawn just unequipped a mythic item that was supposed to grant a mythic ability. But no mythic ability '{0}' was found to remove from the pawn '{1}'.",
                    mythicAbilityDef.label, pawn.Name);
            } else
            {
                effectVal2 = ability.CooldownTicksRemaining.ToString();
                pawn.abilities.RemoveAbility(mythicAbilityDef);
            }
        }

        private int getSavedTickValue(string value)
        {
            if (value == null)
            {
                return 0;
            }
            int result = 0;
            bool success = int.TryParse(value, out result);
            if (success)
            {
                return result;
            }
            DebugActions.LogErr("Tried to pull saved time data about a mythic item's cooldown, but the saved value ({0}) could not be converted back into an numeric value.", value);
            return 0;
        }

        public override string EffectDescription(ThingWithComps mythicItem)
        {
            return string.Format(effectDescTranslationKey.Translate(), mythicItem.def.label, mythicAbilityDef.label);
        }
    }
}
