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
        public ThingDef itemDef { get; }
        public String ownerFullName { get; } //Owner in this context refers to the original owner
        public String ownerShortName { get; } 
        public String factionName { get; }
        public String descriptionTranslationString { get; }
        public String titleTranslationString { get; }
        public LegacyEffectDef abilityDef { get; } 
        public ThingDef stuffDef { get; }
        public int prv { get; } // stands for a map's persistentRandomValue, which AFAIK is the closest thing to a playthrough UID I'll be able to find
        public String reason; // generic book-keeping string for determining the origin of legacy items. Opted not to make this an enum to avoid mod compatibility issues.'
        public String originatorId; // the ThingId of the pawn related to this item's creation for duplication checking.
        public List<int> worldsUsedIn; // A list of game private random values that this item has been created in. Used for bookkeeping to prevent the same item from being used too much.


        /* This should only be called by the SaveUtility, all other calls from within the game should use the other constructor*/
        public LegacyItem(
            ThingDef itemDef, 
            string originatorFullName, 
            string originatorShortName,  
            string originatorFactionName,
            string descriptionTranslationString,
            string titleTranslationString,
            LegacyEffectDef abilityDef,
            ThingDef stuffDef, 
            int prv, 
            string reason, 
            string originatorId,
            List<int> worldsUsedIn)
        {
            // TODO add checks to ensure that itemDefName and stuffDefName are valid
            this.itemDef = itemDef;
            this.ownerFullName = originatorFullName;
            this.ownerShortName = originatorShortName;
            this.factionName = originatorFactionName;
            this.descriptionTranslationString = descriptionTranslationString;
            this.titleTranslationString = titleTranslationString;
            this.abilityDef = abilityDef;
            this.stuffDef = stuffDef;
            this.prv = prv;
            this.reason = reason;
            this.originatorId = originatorId;
            this.worldsUsedIn = worldsUsedIn;
            if (itemDef.MadeFromStuff && stuffDef == null)
            {
                Log.Error(String.Format("[Moo Legacy Items] tried to load a saved legacy item based on a {0}, but no stuff type was supplied. The default stuff type will be used instead.", itemDef.defName));
            }
        }

        public LegacyItem(Thing item, Pawn originator,string descriptionTranslationString, string titleTranslationString, LegacyEffectDef abilityDef, string reason)
        {
            this.itemDef = item.def;
            this.ownerFullName = originator.Name.ToStringFull;
            this.ownerShortName = originator.Name.ToStringShort;
            this.factionName = originator.Faction.Name;
            this.descriptionTranslationString = descriptionTranslationString;
            this.titleTranslationString = titleTranslationString;
            this.abilityDef = abilityDef;
            
            this.prv = Find.World.info.persistentRandomValue;
            if (item.Stuff != null)
            {
                stuffDef = item.Stuff;
            } else
            {
                stuffDef = null;
            }

            this.reason = reason;
            this.originatorId = originator.ThingID;

            this.worldsUsedIn = new List<int>();
        }
        
        // TODO add more details once legacyItem fields more concrete
        public override string ToString() {
            return String.Format("Legacy item - {0} made by {1} due to {2} with power {3}", itemDef.defName, ownerFullName, descriptionTranslationString, abilityDef.defName);
        }
    }
}
