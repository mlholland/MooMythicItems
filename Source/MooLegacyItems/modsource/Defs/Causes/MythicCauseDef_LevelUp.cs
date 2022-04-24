using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Def for Mythic Causes whose cause is a pawn leveling up a skill to a certain level. At the moment multiple mythic causes
 * for the same skill and level are not supported. If multiple causes target the same skill at different levels, the cause 
 * with the highest level requirement will always be chosen. 
 * Note that the priority for a given Levelup Cause will always be set to equal the minimum level threshold for this cause.
 */
namespace MooMythicItems
{
    public class MythicCauseDef_LevelUp : MythicCauseDef
    {
        public SkillDef skill;
        public int minLevelThreshold = 0;
        public string subreason;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;
            if (minLevelThreshold < 1) yield return "threshold must be positive";
            if (skill == null) yield return "skill must be nonnull";
        }

        // Since the startup code is dependent on referenced Defs, we need to run said code after 
        // ResolveReferences, rather than during a PostLoad override.
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            this.priority = this.minLevelThreshold;
        }

        public override String GetPrintedReasonFragment(params object[] args)
        {
            return base.GetPrintedReasonFragment(args[0], minLevelThreshold, skill.label);
        }

    }
}
