using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This effect causes kills against specific enemies to grant the killer a hediff for a limited duration.
 * Can be configured to increase the hediff severity on subsequent kills, which allows for stacking mechanics.
 */
namespace MooMythicItems
{
    public class MythicEffectDef_KillstreakHediff : MythicEffectDef
    {
        // TODO should see if i can refactor this into... a worker maybe?
        public bool worksOnMechs = false;
        public bool worksOnInsects = false;
        public bool worksOnHumans = false;
        public bool worksOnAll = false;
        // Must be set. Does not work on stuffable things.
        public List<ThingDef> allowedTargetsDefList = null;
        public int maxStacks = 1;
        public int effectDurationTicks = 0;
        public HediffDef hediffToAdd = null;
        public bool resetDurationOnNewStack = true;
        public float severityPerStack = 1;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;
            if (allowedTargetsDefList != null && allowedTargetsDefList.Count == 0) yield return "allowedTargetsDefList cannot be set to an empty list. Either leave it null or supply at least one element.";
            if (maxStacks < 1) yield return "maxStacks must be a positive integer.";
            if (effectDurationTicks < 1) yield return "effectDurationTicks must be a positive integer.";
            if (severityPerStack <= 0) yield return "severityPerStack must be a positive value.";
            if (hediffToAdd == null) yield return "hediffToAdd must be set.";
            else if (severityPerStack * maxStacks > hediffToAdd.maxSeverity + 0.0001) yield return String.Format("Severity at max stacks ({0} * {1} = {2}) cannot exceed target hediff's max possible severity (3).", severityPerStack, maxStacks, severityPerStack * maxStacks, hediffToAdd.maxSeverity);
        }


        public override void OnKill(Pawn killedPawn, Pawn killer, ThingWithComps mythicItem, ref string effectVal1, ref string effectVal2, ref string effectVal3)
        {
            //Log.Message(killer.Name.ToStringFull);
            //Log.Message(hediffToAdd.defName);
            if (killedValidTarget(killedPawn.def))
            {
                if (killer != null && killer.health != null && killer.health.hediffSet != null)
                {
                    Hediff hediff = null;
                    if (killer.health.hediffSet.HasHediff(hediffToAdd))
                    {
                        Log.Message("DA");
                        hediff = killer.health.hediffSet.GetFirstHediffOfDef(hediffToAdd);
                        if (hediff != null && hediff.Severity + 0.0001 < severityPerStack * maxStacks) // Added value is just defence against floating point problems
                        {
                            hediff.Severity = Math.Min(hediff.Severity + severityPerStack, hediff.def.maxSeverity);
                        }
                    } else
                    {
                        hediff = killer.health.AddHediff(hediffToAdd);
                        killer.health.AddHediff(hediff);
                        if (hediff != null)
                        {
                            hediff.Severity = severityPerStack;
                        }
                    }
                    if(hediff != null && resetDurationOnNewStack)
                    {
                        Log.Message("E");
                        HediffComp_Disappears comp = hediff.TryGetComp<HediffComp_Disappears>();
                        if (comp != null)
                        {
                            comp.ticksToDisappear = effectDurationTicks;
                        }
                    }
                }
            }
        }     

        private bool killedValidTarget(ThingDef killedPawnDef)
        {
            if (worksOnAll)
            {
                return true;
            }
            if (allowedTargetsDefList != null)
            {
                return allowedTargetsDefList.Contains(killedPawnDef);
            } 
            if (worksOnHumans && killedPawnDef.race.intelligence == Intelligence.Humanlike) // TODO verify that this is a valid way of checking human-ness
            {
                return true;
            }
            if (worksOnMechs && killedPawnDef.race.IsMechanoid)
            {
                return true;
            }
            if (worksOnInsects && killedPawnDef.race.FleshType == FleshTypeDefOf.Insectoid)
            {
                return true;
            }
            return false;
        }
    }
}
