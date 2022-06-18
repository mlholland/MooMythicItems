using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;
using System.Text;
using Verse.AI;

/* This comp is produces a drop-down of item use options depending on a list of valid defs. Requires a CompUseEFfect_WithDefInput to actually process the selected def, which
 * is cached as a field on the comp.
 */
namespace MooMythicItems
{
    class CompUsable_OptionPerDef : CompUsable
    { 
        public CompProperties_UsableOptionPerDef Props
        {
            get
            {
                return (CompProperties_UsableOptionPerDef)this.props;
            }
        }

        // save def that's gooing to be used to ensure that things don't break if someone save/loads mid-item use.
        public Def lastSelectedDef = null;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref this.lastSelectedDef, "lastSelectedDef");
        }

        // cache valid defs, and only load them once
        private List<Def> cachedOptions = null;
        private List<Def> Options
        {
            get
            {
                if (cachedOptions == null)
                {
                    cachedOptions = GetAllValidDefs(Props.defType);
                }
                return cachedOptions;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            string text;
            if (!this.CanBeUsedBy(myPawn, out text))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel(myPawn) + ((text != null) ? (" (" + text + ")") : ""), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            else if (!myPawn.CanReach(this.parent, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel(myPawn) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            else if (!myPawn.CanReserve(this.parent, 1, -1, null, false))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel(myPawn) + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel(myPawn) + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            else
            {
                CompUseEffect_WithDefInput useEffect = this.parent.TryGetComp<CompUseEffect_WithDefInput>();
                if (useEffect == null)
                {
                    DebugActions.LogErr("Missing comp of type 'CompUseEffect_WithDefInput' to work with option per def-based usability property.");
                    yield break;
                }
                foreach (Def d in Options)
                {
                    yield return new FloatMenuOption(useEffect.ParameterizeUseLabel(d), delegate ()
                    {
                        if (myPawn.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                        {
                            lastSelectedDef = d;
                            this.TryStartUseJob(myPawn, LocalTargetInfo.Invalid);
                        }
                    }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                }
            }
            yield break;
        }

        protected bool CanBeUsedBy(Pawn p, out string failReason)
        {
            List<ThingComp> allComps = this.parent.AllComps;
            for (int i = 0; i < allComps.Count; i++)
            {
                CompUseEffect compUseEffect = allComps[i] as CompUseEffect;
                if (compUseEffect != null && !compUseEffect.CanBeUsedBy(p, out failReason))
                {
                    return false;
                }
            }
            failReason = null;
            return true;
        }

        // made as a separate function in case extension classes want to compliate the sorting process
        // Todo maybe move this to a worker class so we can have custom selections?
        protected List<Def> GetAllValidDefs(Type defType)
        {
            List<Def> results = new List<Def>();
            foreach (var def in GenDefDatabase.GetAllDefsInDatabaseForDef(defType))
            {
                results.Add(def);
            }
            return results;
        }

    }
}
