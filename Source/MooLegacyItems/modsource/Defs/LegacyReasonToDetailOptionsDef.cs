using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This Def is meant to be a one-off that links reasons for legacy item creation (ex: getting 100 kills) to 
 * translation strings related to them. This lets
 * 
 * notes about field names:
 * FD = flavor descriptions AKA stuff to add to the description of an item
 * T = title - intended to fully replace the original
 * M = melee - for reasons that apply to weapons
 * R = ranged - for reasons that apply to weapons
 * A = Ability - should be a defName for a LegacyEffectDef instance
 */
namespace MooLegacyItems
{
    class LegacyReasonToDetailOptionsDef : Def
    {
        // keep a single instance
        private static readonly string s_defName = "MooLI_ReasonsToFlavorMap";
        private static LegacyReasonToDetailOptionsDef instance;
        public static LegacyReasonToDetailOptionsDef Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = DefDatabase<LegacyReasonToDetailOptionsDef>.GetNamed(s_defName);
                }
                return instance;
            }
        }

        // general many kills
        // many = 100
        public List<String> manyKillsMFD;
        public List<String> manyKillsMT;
        public List<String> manyKillsRFD;
        public List<String> manyKillsRT;
        // more = 500
        public List<String> moreKillsMFD;
        public List<String> moreKillsMT;
        public List<String> moreKillsRFD;
        public List<String> moreKillsRT;
        // most = 1000
        public List<String> mostKillsMFD;
        public List<String> mostKillsMT;
        public List<String> mostKillsRFD;
        public List<String> mostKillsRT;

        // many kills on specific enemy types
        // many = 50
        public List<String> manyInsectKillsMFD;
        public List<String> manyInsectKillsMT;
        public List<String> manyInsectKillsRFD;
        public List<String> manyInsectKillsRT;
        // many = 50
        public List<String> manyMechKillsMFD;
        public List<String> manyMechKillsMT;
        public List<String> manyMechKillsRFD;
        public List<String> manyMechKillsRT;

        // skill mastery
        public List<String> ShootingMasterFD;
        public List<String> ShootingMasterT;

        public List<String> MeleeMasterFD;
        public List<String> MeleeMasterT;

        public List<String> ConstructionMasterFD;
        public List<String> ConstructionMasterT;
        public List<LegacyEffectDef> ConstructionMasterA;

        public List<String> MiningMasterFD;
        public List<String> MiningMasterT;

        public List<String> CookingMasterFD;
        public List<String> CookingMasterT;

        public List<String> PlantsMasterFD;
        public List<String> PlantsMasterT;

        public List<String> AnimalsMasterFD;
        public List<String> AnimalsMasterT;

        public List<String> CraftingMasterFD;
        public List<String> CraftingMasterT;

        public List<String> ArtisticMasterFD;
        public List<String> ArtisticMasterT;

        public List<String> MedicineMasterFD;
        public List<String> MedicineMasterT;

        public List<String> SocialMasterFD;
        public List<String> SocialMasterT;

        public List<String> IntellectualMasterFD;
        public List<String> IntellectualMasterT;

        // misc interesting events
        // Killing enemy settlements
        public List<String> ColonySlayerMFD;
        public List<String> ColonySlayerMT;
        public List<String> ColonySlayerRFD;
        public List<String> ColonySlayerRT;
        // killing thrumbos
        public List<String> TrumboSlayerMFD;
        public List<String> ThrumboSlayerMT;
        public List<String> TrumboSlayerRFD;
        public List<String> ThrumboSlayerRT;
        // Being the last colonist standing during a late game raid
        public List<String> LastStandingMFD;
        public List<String> LastStandingMT;
        public List<String> LastStandingRFD;
        public List<String> LastStandingRT;

    }
}
