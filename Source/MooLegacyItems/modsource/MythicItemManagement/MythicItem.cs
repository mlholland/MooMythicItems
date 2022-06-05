using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;

/* A MythicItem is an item that was created as a result of a colonist's actions, which can
 * then be generated into an actual Thing in another colony. The data saved in this class structure is 
 * the minimal info needed to create the actual in-game item, without any direct references to the original 
 * colony or pawn.
 */
namespace MooMythicItems
{
    public class MythicItem
    {
        private static readonly string bladelink_key_traits = "RWR_BL_T"; // rimworld royalty bladelink traits
        // The traits field is private normally - this lets us mess with it.
        private static readonly AccessTools.FieldRef<object, List<WeaponTraitDef>> weaponTraitsField = AccessTools.FieldRefAccess<List<WeaponTraitDef>>(typeof(CompBladelinkWeapon), "traits");


        public ThingDef itemDef { get; }
        public String ownerFullName { get; } // Owner in this context refers to the original owner
        public String ownerShortName { get; } 
        public String factionName { get; }
        public String descriptionTranslationString { get; }
        public String titleTranslationString { get; }
        public MythicEffectDef abilityDef { get; } 
        public ThingDef stuffDef { get; }
        public int prv { get; } // stands for a map's persistentRandomValue, which AFAIK is the closest thing to a playthrough UID I'll be able to find
        public String reason; // generic book-keeping string for determining the origin of mythic items. Opted not to make this an enum to avoid mod compatibility issues.'
        public String originatorId; // the ThingId of the pawn related to this item's creation for duplication checking.
        public List<int> worldsUsedIn; // A list of game private random values that this item has been created in. Used for bookkeeping to prevent the same item from being used too much.
                                       // public String colony; // TODO make use of (map.Parent as Settlement).Name to get the colony name, then incorporate it into flavor perhaps

        // These values are intended to be used by other modders to add extra info to their own descriptions.
        // These two lists have their entire contents added to the input list of formatted translation strings.
        // Bear in mind that the first 2 values for titles, and the first 4 values for descriptions are already reserved,
        // Also, I recommend that you only use these values for flavor text in NEW cause defs that your mod adds, as this will become jarbled if two mods try to add values to existing causes.
        public List<string> extraTitleValues;
        public List<string> extraDescriptionValues;

        // This value is designed to allow extra info to be saved to a mythic item.
        // I do nothing to help with parsing this data, just saving it to/from disk. There are two things you need to do to make thiswork.
        // 1. Add data to this field by patching the SECOND constructor below to pull data from the inputted item. field.
        // 2. Patch the realize function to read that data and append the necessary data the resulting in-game thing.
        // Make sure you select keys for your info that is unlikely to be used by anyone else. Ex: Don't use a key named "data", use a key named "your-mod's-initials-data".
        // For an example of how to do this, you can see how persona weapons have their persona type saved via this system.
        public Dictionary<string, string> extraItemData;

        /* This should only be called by the SaveUtility, all other calls from within the game should use the other constructor*/
        public MythicItem(
            ThingDef itemDef,
            string originatorFullName,
            string originatorShortName,
            string originatorFactionName,
            string descriptionTranslationString,
            string titleTranslationString,
            MythicEffectDef abilityDef,
            ThingDef stuffDef,
            int prv,
            string reason,
            string originatorId,
            List<int> worldsUsedIn,
            List<string> extraTitleValues = null,
            List<string> extraDescriptionValues = null,
            Dictionary<string, string> extraItemData = null
            )
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
                DebugActions.LogErr("tried to load a saved mythic item based on a {0}, which is usually made from stuff, but no stuff def was supplied. The default stuff type will be used instead.", itemDef.defName);
                this.stuffDef = itemDef.defaultStuff;
            }
            this.extraTitleValues = extraTitleValues == null ? new List<string>() : extraTitleValues;
            this.extraDescriptionValues = extraDescriptionValues == null ? new List<string>() : extraDescriptionValues;
            this.extraItemData = extraItemData == null ? new Dictionary<string, string>() : extraItemData; 
        }

        public MythicItem(Thing item, 
            Pawn originator,
            string descriptionTranslationString, 
            string titleTranslationString, 
            MythicEffectDef abilityDef, 
            string reason, 
            List<string> extraTitleValues = null, 
            List<string> extraDescriptionValues = null, 
            Dictionary<string, string> extraItemData = null)
        {
            this.itemDef = item.def;
            this.ownerFullName = originator.Name.ToStringFull;
            this.ownerShortName = originator.Name.ToStringShort;
            this.factionName = originator.Faction.Name;
            this.descriptionTranslationString = descriptionTranslationString;
            this.titleTranslationString = titleTranslationString;
            this.abilityDef = abilityDef;
            
            this.prv = Find.World.info.persistentRandomValue;
            this.stuffDef = item.Stuff;

            this.reason = reason;
            this.originatorId = originator.ThingID;

            this.worldsUsedIn = new List<int>();

            this.extraTitleValues = extraTitleValues == null ? new List<string>() : extraTitleValues;
            this.extraDescriptionValues = extraDescriptionValues == null ? new List<string>() : extraDescriptionValues;
            this.extraItemData = extraItemData == null ? new Dictionary<string, string>() : extraItemData;

            // extra item data in vanilla
            CompBladelinkWeapon linkComp = item.TryGetComp<CompBladelinkWeapon>();
            if (linkComp != null && linkComp.TraitsListForReading != null)
            {
                this.extraItemData[bladelink_key_traits] =string.Join("/", linkComp.TraitsListForReading.Select(def => def.defName));
            }
            
        }
        
        // TODO add more details once mythicItem fields more concrete
        public override string ToString() {
            return String.Format("Mythic item - {0} made by {1} due to {2} with power {3}", itemDef.defName, ownerFullName, descriptionTranslationString, abilityDef.defName);
        }

        // Produce the title that replaces the normal item label
        public string GetFormattedTitle()
        {
            if (extraTitleValues != null && extraTitleValues.Count > 0)
            {
                return string.Format(this.titleTranslationString.Translate(), ownerShortName, itemDef.label, extraTitleValues);
            }
            return string.Format(this.titleTranslationString.Translate(), ownerShortName, itemDef.label);
        }

        // Produce the description that's appended to the item's normal description.
        public string GetFormattedDescription()
        {
            if (extraDescriptionValues != null && extraDescriptionValues.Count > 0)
            { 
                return string.Format(this.descriptionTranslationString.Translate(), ownerFullName, ownerShortName, factionName, itemDef.label, extraDescriptionValues);
            }
            return string.Format(this.descriptionTranslationString.Translate(), ownerFullName,ownerShortName, factionName, itemDef.label);
        }

        public int ExtractPriorityFromReason()
        {
            if (Int32.TryParse(reason.Split('-').Last(), out int result))
            {
                return result;
            }
            return 0;
        }

        /* Turn a mythic item into a real Thing that can show up in-game.
         * MODDER INFO. If you try to patch this file for whatever reason, you'll probably just need to input the hand-typed function name
         * into the harmony header, since this function is internal.
        */
        internal Thing Realize()
        {
            DebugActions.LogIfDebug("Realizing mythic item: {0}", this.ToString());
            ThingDef def = this.itemDef;
            ThingDef stuff = null;
            if (this.stuffDef != null)
            {
                stuff = this.stuffDef;
            }
            ThingWithComps thing = (ThingWithComps)ThingMaker.MakeThing(def, stuff);

            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQuality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            }
            if (thing.def.Minifiable)
            {
                thing = thing.MakeMinified();
            }

            MythicItemUtilities.AddMythicCompToThing(thing, String.Format(this.titleTranslationString.Translate(), this.ownerShortName, def.label), String.Format(this.descriptionTranslationString.Translate(), this.ownerFullName, this.ownerShortName, this.factionName, def.label), this.abilityDef);
        
            if(this.extraItemData != null && this.extraItemData.ContainsKey(bladelink_key_traits))
            {
                CompBladelinkWeapon linkComp = thing.TryGetComp<CompBladelinkWeapon>();
                if (linkComp == null)
                {
                    DebugActions.LogErr("Tried to realize a mythic {0} that had saved bladelink data, but no CompBladelinkWeapon was found on the resulting item to add this data to.", this.itemDef.label);
                } else
                {
                    List<WeaponTraitDef> traits = weaponTraitsField.Invoke(linkComp);
                    if (traits == null)
                    {
                        DebugActions.LogErr("Tried to extract traits from new mythic weapon's CompBladelinkWeapon comp, but the traits field was null.");
                    }
                    else
                    {
                        traits.Clear();
                        foreach (string traitName in this.extraItemData[bladelink_key_traits].Split('/'))
                        {
                            WeaponTraitDef trait = DefDatabase<WeaponTraitDef>.GetNamedSilentFail(traitName);
                            if (trait == null)
                            {
                                DebugActions.LogErr("Tried adding bladelink trait named '{0}' to a new mythic weapon, but no such trait could be found. Ignoring it.", traitName);
                                continue;
                            }
                            traits.Add(trait);
                        }
                    }
                }
            }

            return thing;
        }
    }
}
