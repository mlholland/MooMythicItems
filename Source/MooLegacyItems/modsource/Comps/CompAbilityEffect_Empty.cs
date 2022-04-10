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
    class CompAbilityEffect_Empty : CompAbilityEffect
    {

        public CompProperties_Empty Props
        {
            get
            {
                return (CompProperties_Empty)this.props;
            }
        }
    }
}
