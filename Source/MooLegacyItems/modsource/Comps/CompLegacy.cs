using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This comp is what actually contains the data on an in-game item that makes it a legacy item, and 
 * gives that items the unusual text/abilities to show it. All items that could possibly be a legacy 
 * item have this comp, but it's only populated in actual legacy items.
 */
namespace MooLegacyItems
{
    class CompLegacy : ThingComp
    {
        // todo cache fully modified values to save overhead?
        public String newLabel = null;

        public String newDescription = null;

        public override string TransformLabel(string label)
        {

            if (newLabel != null)
            {
                return newLabel + label;
            }

            return base.TransformLabel(label);
        }

        public override string GetDescriptionPart()
        {
            return newDescription; 
        }
    }
}
