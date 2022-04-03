using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Fires the indident defined in the associated comp properties on the ability-user's map.
 * Used for incidents whose only variable is the map they take place on (so no faction, point, or targetted location variance).
 */
namespace MooMythicItems
{
    class CompAbilityEffect_MapWideIncident : CompAbilityEffect
    {

        public CompProperties_MapWideIncident Props
        {
            get
            {
                return (CompProperties_MapWideIncident)this.props;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = this.parent.pawn.Map; 
            Props.incident.Worker.TryExecute(incidentParms);
        }
    }
}
