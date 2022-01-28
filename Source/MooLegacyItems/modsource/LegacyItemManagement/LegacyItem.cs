using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

/* A LegacyItem is an item that was created as a result of a colonist's actions in a colony, which can
 * then be generated into an actual Thing in another colony. The data saved in this class structure is 
 * just the minimal info needed to create the actual in-game item.
 */
namespace MooLegacyItems
{
    public class LegacyItem
    {
        public String itemDefName { get; }
        public String originatorFullName { get; } // todo think carefully about name and colony strings we want to record
        public String originatorShortName { get; }
        public String originatorColonyName { get; }
        public String originatorFactionName { get; }
        public String storyLabel { get; } // todo make legacy stories a def, and this a defName?
        public String abilityLabel { get; } // todo make legacy abilities a def, and this a defName?
        public String stuffDefName { get; } 

        /* This should only be called by the SaveUtility, all other calls from within the game should use the other constructor*/
        public LegacyItem(string itemDefName, string originatorFullName, string originatorShortName, string originatorColonyName, string originatorFactionName, string storyLabel, string abilityLabel, string stuffDefName = null)
        {
            // TODO add checks to ensure that itemDefName and stuffDefName are valid
            this.itemDefName = itemDefName;
            this.originatorFullName = originatorFullName;
            this.originatorShortName = originatorShortName;
            this.originatorColonyName = originatorColonyName;
            this.originatorFactionName = originatorFactionName;
            this.storyLabel = storyLabel;
            this.abilityLabel = abilityLabel;
            this.stuffDefName = stuffDefName;
        }

        public LegacyItem(Thing item, Pawn originator, string originatorColonyName, string storyLabel, string abilityLabel)
        {
            this.itemDefName = item.def.defName;
            this.originatorFullName = originator.Name.ToStringFull;
            this.originatorShortName = originator.Name.ToStringShort;
            this.originatorColonyName = originatorColonyName;
            this.originatorFactionName = originator.Faction.Name;
            this.storyLabel = storyLabel;
            this.abilityLabel = abilityLabel;
            if (item.Stuff != null)
            {
                stuffDefName = item.Stuff.defName;
            }
        }


        public Thing Realize()
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamed(this.itemDefName);
            ThingDef stuff = null;
            if (this.stuffDefName != null && this.stuffDefName.Length > 0)
            {
                stuff = DefDatabase<ThingDef>.GetNamed(this.stuffDefName);
            }
            Thing thing = ThingMaker.MakeThing(def, stuff);
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQuality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            }
            if (thing.def.Minifiable)
            {
                thing = thing.MakeMinified();
            }
             
            CompLegacy legacyComp = thing.TryGetComp<CompLegacy>();
            if (legacyComp == null)
            {
                throw new InvalidCastException(String.Format("Moo Legacy Items - Legacy Item realization failed. The item def {0} had no legacy comp to modify, yet a saved legacy item was based on one", thing.def.defName));
            }
            else
            {
                legacyComp.newLabel = this.originatorShortName + "'s ";
                legacyComp.newDescription = String.Format(this.storyLabel.Translate(), this.originatorFullName, this.originatorShortName, this.originatorColonyName, this.originatorFactionName, def.label);
            }
            return thing;
        }
        
        // TODO add more details once legacyItem fields more concrete
        public override string ToString() {
            return String.Format("Legacy item - {0} made by {1} from {2} due to {3} with power {4}", itemDefName, originatorFullName, originatorColonyName, storyLabel, abilityLabel);
        }
    }
}
