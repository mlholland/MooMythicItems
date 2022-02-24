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
    [Serializable]
    public class LegacyItem
    {
        public String itemDefName { get; }
        public String ownerFullName { get; } //Owner in this context refers to the original owner
        public String ownerShortName { get; } 
        public String factionName { get; }
        public String descriptionTranslationString { get; }
        public String titleTranslationString { get; }
        public String abilityPlaceholder { get; } 
        public String stuffDefName { get; }
        public int prv { get; } // stands for a map's persistentRandomValue, which AFAIK is the closest thing to a playthrough UID I'll be able to find
        public String reason; // generic book-keeping string for determining the origin of legacy items. Opted not to make this an enum to avoid mod compatibility issues.'
        public String originatorId; // the ThingId of the pawn related to this item's creation for duplication checking.


        /* This should only be called by the SaveUtility, all other calls from within the game should use the other constructor*/
        public LegacyItem(
            string itemDefName, 
            string originatorFullName, 
            string originatorShortName,  
            string originatorFactionName,
            string descriptionTranslationString,
            string titleTranslationString,
            string abilityLabel,
            string stuffDefName, 
            int prv, 
            string reason, 
            string originatorId)
        {
            // TODO add checks to ensure that itemDefName and stuffDefName are valid
            this.itemDefName = itemDefName;
            this.ownerFullName = originatorFullName;
            this.ownerShortName = originatorShortName;
            this.factionName = originatorFactionName;
            this.descriptionTranslationString = descriptionTranslationString;
            this.titleTranslationString = titleTranslationString;
            this.abilityPlaceholder = abilityLabel;
            this.stuffDefName = stuffDefName;
            this.prv = prv;
            this.reason = reason;
            this.originatorId = originatorId;
        }

        public LegacyItem(Thing item, Pawn originator,string descriptionTranslationString, string titleTranslationString, string abilityLabel, string reason)
        {
            this.itemDefName = item.def.defName;
            this.ownerFullName = originator.Name.ToStringFull;
            this.ownerShortName = originator.Name.ToStringShort;
            this.factionName = originator.Faction.Name;
            this.descriptionTranslationString = descriptionTranslationString;
            this.titleTranslationString = titleTranslationString;
            this.abilityPlaceholder = abilityLabel;
            
            this.prv = Find.World.info.persistentRandomValue;
            if (item.Stuff != null)
            {
                stuffDefName = item.Stuff.defName;
            } else
            {
                stuffDefName = "";
            }

            this.reason = reason;
            this.originatorId = originator.ThingID;
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
                legacyComp.newLabel = String.Format(this.titleTranslationString.Translate(), this.ownerShortName, this.itemDefName);
                legacyComp.newDescription = String.Format(this.descriptionTranslationString.Translate(), this.ownerFullName, this.ownerShortName,  this.factionName, def.label);
            }
            return thing;
        }
        
        // TODO add more details once legacyItem fields more concrete
        public override string ToString() {
            return String.Format("Legacy item - {0} made by {1} due to {2} with power {3}", itemDefName, ownerFullName, descriptionTranslationString, abilityPlaceholder);
        }
    }
}
