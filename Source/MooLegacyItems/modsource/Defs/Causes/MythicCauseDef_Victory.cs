using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Def for mythic item creation triggers based on actually beating the game. Unlike other triggers,
 * multiple triggers may be set up to target the same victory conditions. If multiple triggers apply to a
 * given victory, a single valid trigger is selected at random to produce a mythic item.
 *
 */
namespace MooMythicItems
{
    public class MythicCauseDef_Victory : MythicCauseDef
    {
        // base code only seems to input escaped colonists in archonexus victory. Will need more robust logic to re-enable this.
        //public bool selectFromAbandoned = false;
        public bool spaceVictory = false;
        public bool royalVictory = false;
        public bool archoVictory = false;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;
            if (!spaceVictory && !royalVictory && !archoVictory) yield return "Victory mythic trigger must target at least one victory condition";
        }
    }
}
