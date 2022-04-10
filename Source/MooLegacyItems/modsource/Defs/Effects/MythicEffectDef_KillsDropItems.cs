using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This effect causes kills against specific enemies to drop items.
 */
namespace MooMythicItems
{
    public class MythicEffectDef_KillsDropItems : MythicEffectDef
    {
        public List<ThingDef> possibleDrops;
        public IntRange valueRangePerDrop = new IntRange(200, 400);
        // TODO should see if i can refactor this into... a worker maybe?
        public bool worksOnMechs = false;
        public bool worksOnInsects = false;
        public bool worksOnHumans = false;
        // Must be set. Does not work on stuffable things.
        public List<ThingDef> allowedTargetsDefList = null;


        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;
            if (valueRangePerDrop.min < 0 || valueRangePerDrop.max < 0) yield return "valueRangePerDrop cannot include negative values.";
            if (possibleDrops == null || possibleDrops.Count == 0) yield return "possibleDrops cannot be null and must contain at least one element.";
            if (allowedTargetsDefList != null && allowedTargetsDefList.Count == 0) yield return "allowedTargetsDefList cannot be set to an empty list. Either leave it null or supply at least one element.";
        }


        public override void OnKill(Pawn killedPawn, Pawn killer, ThingWithComps mythicItem, ref string effectVal1, ref string effectVal2, ref string effectVal3)
        {
            if (killedValidTarget(killedPawn.def))
            {
                Thing droppedItem = ThingMaker.MakeThing(possibleDrops.RandomElement(), null);
                // this is the calculation for the displayed meat amount, we do it this way to avoid player confusion.
                //int dropAmount = (int)(corpse.InnerPawn.def.statBases.GetStatValueFromList(StatDefOf.MeatAmount, StatDefOf.MeatAmount.defaultBaseValue) * corpse.InnerPawn.RaceProps.baseBodySize);
                droppedItem.stackCount = (int)Math.Max(1, valueRangePerDrop.RandomInRange / droppedItem.def.BaseMarketValue);
                GenPlace.TryPlaceThing(droppedItem, killedPawn.Position, killedPawn.Map, ThingPlaceMode.Near);
            }
        }     

        private bool killedValidTarget(ThingDef killedPawnDef)
        {
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
