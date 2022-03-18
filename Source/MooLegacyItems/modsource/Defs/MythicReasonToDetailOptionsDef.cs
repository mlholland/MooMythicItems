using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This Def is meant to be a one-off that links reasons for mythic item creation (ex: getting 100 kills) to 
 * translation strings related to them. This lets
 * 
 * notes about field names:
 * FD = flavor descriptions AKA stuff to add to the description of an item
 * T = title - intended to fully replace the original
 * M = melee - for reasons that apply to weapons
 * R = ranged - for reasons that apply to weapons
 * A = Ability - should be a defName for a MythicEffectDef instance
 */
namespace MooMythicItems
{
    class MythicReasonToDetailOptionsDef : Def
    {
        // keep a single instance
        private static readonly string s_defName = "MooMF_ReasonsToFlavorMap";
        private static MythicReasonToDetailOptionsDef instance;
        public static MythicReasonToDetailOptionsDef Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = DefDatabase<MythicReasonToDetailOptionsDef>.GetNamed(s_defName);
                }
                return instance;
            }
        }

        // general many kills
        // many = 100
        public List<String> manyKillsMFD;
        public List<String> manyKillsMT;
        public List<MythicEffectDef> manyKillsMA;
        public List<String> manyKillsRFD;
        public List<String> manyKillsRT;
        public List<MythicEffectDef> manyKillsRA;
        // more = 500
        public List<String> moreKillsMFD;
        public List<String> moreKillsMT;
        public List<MythicEffectDef> moreKillsMA;
        public List<String> moreKillsRFD;
        public List<String> moreKillsRT;
        public List<MythicEffectDef> moreKillsRA;
        // most = 1000
        public List<String> mostKillsMFD;
        public List<String> mostKillsMT;
        public List<MythicEffectDef> mostKillsMA;
        public List<String> mostKillsRFD;
        public List<String> mostKillsRT;
        public List<MythicEffectDef> mostKillsRA;

        // many kills on specific enemy types
        // many = 50
        public List<String> manyInsectKillsMFD;
        public List<String> manyInsectKillsMT;
        public List<MythicEffectDef> manyInsectKillsMA;

        public List<String> manyInsectKillsRFD;
        public List<String> manyInsectKillsRT;
        public List<MythicEffectDef> manyInsectKillsRA;
        // many = 50
        public List<String> manyMechKillsMFD;
        public List<String> manyMechKillsMT;
        public List<MythicEffectDef> manyMechKillsMA;
        public List<String> manyMechKillsRFD;
        public List<String> manyMechKillsRT;
        public List<MythicEffectDef> manyMechKillsRA;

        // skill mastery
        public List<String> ShootingMasterFD;
        public List<String> ShootingMasterT;
        public List<MythicEffectDef> ShootingMasterA;

        public List<String> MeleeMasterFD;
        public List<String> MeleeMasterT;
        public List<MythicEffectDef> MeleeMasterA;

        public List<String> ConstructionMasterFD;
        public List<String> ConstructionMasterT;
        public List<MythicEffectDef> ConstructionMasterA;

        public List<String> MiningMasterFD;
        public List<String> MiningMasterT;
        public List<MythicEffectDef> MiningMasterA;

        public List<String> CookingMasterFD;
        public List<String> CookingMasterT;
        public List<MythicEffectDef> CookingMasterA;

        public List<String> PlantsMasterFD;
        public List<String> PlantsMasterT;
        public List<MythicEffectDef> PlantsMasterA;

        public List<String> AnimalsMasterFD;
        public List<String> AnimalsMasterT;
        public List<MythicEffectDef> AnimalsMasterA;

        public List<String> CraftingMasterFD;
        public List<String> CraftingMasterT;
        public List<MythicEffectDef> CraftingMasterA;

        public List<String> ArtisticMasterFD;
        public List<String> ArtisticMasterT;
        public List<MythicEffectDef> ArtisticMasterA;

        public List<String> MedicineMasterFD;
        public List<String> MedicineMasterT;
        public List<MythicEffectDef> MedicineMasterA;

        public List<String> SocialMasterFD;
        public List<String> SocialMasterT;
        public List<MythicEffectDef> SocialMasterA;

        public List<String> IntellectualMasterFD;
        public List<String> IntellectualMasterT;
        public List<MythicEffectDef> IntellectualMasterA;

        // misc interesting events
        // Killing enemy settlements
        public List<String> ColonySlayerMFD;
        public List<String> ColonySlayerMT;
        public List<MythicEffectDef> ColonySlayerMA;
        public List<String> ColonySlayerRFD;
        public List<String> ColonySlayerRT;
        public List<MythicEffectDef> ColonySlayerRA;
        // killing thrumbos
        public List<String> ThrumboSlayerMFD;
        public List<String> ThrumboSlayerMT;
        public List<MythicEffectDef> ThrumboSlayerMA;
        public List<String> ThrumboSlayerRFD;
        public List<String> ThrumboSlayerRT;
        public List<MythicEffectDef> ThrumboSlayerRA;
        // killing faction leaders
        public List<String> LeaderSlayerMFD;
        public List<String> LeaderSlayerMT;
        public List<MythicEffectDef> LeaderSlayerMA;
        public List<String> LeaderSlayerRFD;
        public List<String> LeaderSlayerRT;
        public List<MythicEffectDef> LeaderSlayerRA;
        // Being the last colonist standing during a late game raid
        public List<String> LastStandingMFD;
        public List<String> LastStandingMT;
        public List<MythicEffectDef> LastStandingMA;
        public List<String> LastStandingRFD;
        public List<String> LastStandingRT;
        public List<MythicEffectDef> LastStandingRA;

    }
}
