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
    class CompAbilityEffect_GiveSpecificInspiration : CompAbilityEffect
    {

        public CompAbilityEffect_GiveSpecificInspiration() { } 

        public CompProperties_GiveSpecificInspiration Props
        {
            get
            {
                return (CompProperties_GiveSpecificInspiration)this.props;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn pawn = target.Pawn;
            if (pawn != null)
            {
                if (Props.inspiration != null)
                {
                    base.Apply(target, dest);
                    pawn.mindState.inspirationHandler.TryStartInspiration(Props.inspiration, "LetterPsychicInspiration".Translate(pawn.Named("PAWN"), this.parent.pawn.Named("CASTER")), true);
                }
            }
            if (Props.applyToSelfAsWellAsTarget && pawn != this.parent.pawn)
            {
                pawn.mindState.inspirationHandler.TryStartInspiration(Props.inspiration, "LetterPsychicInspiration".Translate(pawn.Named("PAWN"), this.parent.pawn.Named("CASTER")), true);
            }
        }

        // Token: 0x060050F8 RID: 20728 RVA: 0x001B49E6 File Offset: 0x001B2BE6
        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return this.Valid(target, false);
        }

        // Token: 0x060050F9 RID: 20729 RVA: 0x001B49F0 File Offset: 0x001B2BF0
        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
           if (Props.selfMustBeValid && !PawnCanReceiveInspirationNow(this.parent.pawn, Props.inspiration)) return false;
            Pawn pawn = target.Pawn;
            if (pawn != null && !Props.targetedPawnMustBeValid)
            {
                if (!PawnCanReceiveInspirationNow(pawn, Props.inspiration))
                {
                    return false;
                }
            }
            return base.Valid(target, throwMessages);
        }

        public static bool PawnCanReceiveInspirationNow(Pawn pawn, InspirationDef inspiration)
        {
            return pawn != null && inspiration != null && AbilityUtility.ValidateNoInspiration(pawn, false, null) && inspiration.Worker.InspirationCanOccur(pawn);
        }
    }
}
