using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Def for Mythic Causes whose cause is a record def reaching a specific threshold. The worker for this def is expected to consume
 * the consume an inputted MythicCauseDef_RecordThreshold and track when the relevant record is updated (usually through a harmony patch),
 * then produce a mythic item when the threshold specified in the def is met.
 *
 */
namespace MooMythicItems
{
    public class MythicCauseDef_RecordThreshold : MythicCauseDef
    {
        public RecordDef record;
        public int threshold = -1;
        public string subreason; // Added to the reason to differentiate causes that rely on the same worker.

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;
            if (threshold < 1) yield return "threshold must be positive";
            if (record == null) yield return "record must be nonnull";
            if (subreason == null) yield return "subreason must be nonnull";
        }
    }
}
