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
        public String originatorName { get; } // todo think carefully about name and colony strings we want to record
        public String originatorColonyName { get; }
        public String storyLabel { get; } // todo make legacy stories a def, and this a defName?
        public String abilityLabel { get; } // todo make legacy abilities a def, and this a defName?

        public LegacyItem(string itemDefName, string originatorName, string originatorColonyName, string storyLabel, string abilityLabel)
        {
            this.itemDefName = itemDefName;
            this.originatorName = originatorName;
            this.originatorColonyName = originatorColonyName;
            this.storyLabel = storyLabel;
            this.abilityLabel = abilityLabel;
        }

        public LegacyItem(Def itemDef, Pawn originator, string originatorColonyName, string storyLabel, string abilityLabel)
        {
            this.itemDefName = itemDef.defName;
            this.originatorName = originator.Name.ToStringFull;
            this.originatorColonyName = originatorColonyName;
            this.storyLabel = storyLabel;
            this.abilityLabel = abilityLabel;
        }

        public Thing Realize()
        {
            throw new NotImplementedException();
            return null;
        }

        public override string ToString() {
            return String.Format("Legacy item - {0} made by {1} from {2} due to {3} with power {4}", itemDefName, originatorName, originatorColonyName, storyLabel, abilityLabel);
        }
    }
}
