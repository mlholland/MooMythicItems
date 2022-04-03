using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Fires the indident defined in the associated comp properties on the ability-user's map.
 */
namespace MooMythicItems
{
    class CompProperties_MapWideIncident : CompProperties_AbilityEffect
    {
        public IncidentDef incident;

        public CompProperties_MapWideIncident()
        {
            this.compClass = typeof(CompAbilityEffect_MapWideIncident);
        }
    }
}
