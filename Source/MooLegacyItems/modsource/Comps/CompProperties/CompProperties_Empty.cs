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
    class CompProperties_Empty : CompProperties_AbilityEffect
    {
        public IncidentDef incident;

        public CompProperties_Empty()
        {
            this.compClass = typeof(CompAbilityEffect_Empty);
        }
    }
}
