using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

/* Misc utility functions related to mythic items.
 */
namespace MooMythicItems
{
    [StaticConstructorOnStartup]
    public static class MythicItemUtilities
    { 
        private static HashSet<ThingDef> s_defaultDefs = null;
        private static HashSet<string> s_defaultNames = null; 
        private static HashSet<string> s_defaultFactions = null;
        private static HashSet<string> s_defaultDescriptions = null;
        private static HashSet<string> s_defaultTitles = null;
        private static HashSet<MythicEffectDef> s_defaultAbilities = null;

        static MythicItemUtilities()
        {
            PopulateDefaultGeneratorSets();
        }



        // todo get this to a reasonable combination before live release
        private static void PopulateDefaultGeneratorSets()
        {
            s_defaultDefs = new HashSet<ThingDef> { ThingDef.Named("Bow_Great"), ThingDef.Named("Gun_ChargeRifle"), ThingDef.Named("Gun_Revolver"), ThingDef.Named("Gun_BoltActionRifle"), ThingDef.Named("Gun_PumpShotgun"), ThingDef.Named("Gun_Autopistol") };
            //s_defaultDefs = new HashSet<ThingDef> { ThingDef.Named("Apparel_FlakVest"), };
            s_defaultNames = new HashSet<string> { "Moo", "Tynan", "Randy", "Cassie", "Pheobe" }; 
            s_defaultFactions = new HashSet<string> { "Tynan's Tyranny Brigade", "Randy's Rabble Rousers", "Carrie's Centurions" };
            s_defaultDescriptions = new HashSet<string> { "MooMF_MythicStory_HumanKills1_Ranged_1", "MooMF_MythicStory_HumanKills1_Ranged_2" };
            s_defaultTitles = new HashSet<string> { "MooMF_MythicTitle_HumanKills2_Ranged_1", "MooMF_MythicTitle_HumanKills2_Ranged_2", "MooMF_MythicTitle_HumanKills2_Ranged_3" };
            //s_defaultAbilities = new HashSet<MythicEffectDef> { DefDatabase<MythicEffectDef>.GetNamed("MooMF_ConstructionBoost") }; // TODO set this to weapon effects
            //s_defaultAbilities = new HashSet<MythicEffectDef> { DefDatabase<MythicEffectDef>.GetNamed("MooMF_GrantTamingInspiration") }; // TODO removeMooMF_GrantHerdFeedersWisdom
            s_defaultAbilities = new HashSet<MythicEffectDef> { DefDatabase<MythicEffectDef>.GetNamed("MooMF_GrantDiscoverUndergroundDeposit") }; // TODO remov
        }
        
        /* Generate a mythic item by randomly selecting among some hard-coded options. 
         */
        public static MythicItem CreateRandomMythicItem()
        {
            string name = s_defaultNames.RandomElement();
            return new MythicItem(s_defaultDefs.RandomElement(), name, name, s_defaultFactions.RandomElement(),s_defaultDescriptions.RandomElement(), s_defaultTitles.RandomElement(),s_defaultAbilities.RandomElement(), null, 0, "debug", "0", new List<int>());
        }

        public static List<ThingDef> RandomItemDefOptions()
        {
            return s_defaultDefs.ToList();
        }
        

    }
}
