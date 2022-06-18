using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* 
 */
namespace MooMythicItems
{
    class CompProperties_UsableOptionPerDef : CompProperties_Usable
    {

        public Type defType = null;

        public CompProperties_UsableOptionPerDef()
        {
            this.compClass = typeof(CompUsable_OptionPerDef);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (defType == null) yield return "defType must be set";
            if (!typeof(Def).IsAssignableFrom(defType)) yield return String.Format("defType '{0}' must refer to a subclass of the 'Def' type", defType);
            foreach (string val in base.ConfigErrors(parentDef)) yield return val;
        }
    }
}
