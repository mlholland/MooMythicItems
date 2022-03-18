using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Simple list for containing a list of ThingDefs of some sort of another. Very generic. Perhaps too much so
 */
namespace MooMythicItems
{
    public class ThingDefListDef : Def
    {
        public List<ThingDef> values;
    }
}
