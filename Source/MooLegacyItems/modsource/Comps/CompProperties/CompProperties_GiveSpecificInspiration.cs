using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Used for abilities that are defined entirely through their non-comp attributes, since vanilla abilityDefs require at least one
 * comp to function for some reason.
 */
namespace MooMythicItems
{
    class CompProperties_GiveSpecificInspiration: CompProperties_AbilityEffect
    {
        public InspirationDef inspiration;
        public bool targetedPawnMustBeValid = false;
        public bool appliesInArea = false;
        public bool selfMustBeValid = false;
        public bool applyToSelfAsWellAsTarget = false;
        public int radius = 0;

        public CompProperties_GiveSpecificInspiration()
        {
            this.compClass = typeof(CompAbilityEffect_GiveSpecificInspiration);
        }
    }
}
