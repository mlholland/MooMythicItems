using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* 
 */
namespace MooMythicItems
{
    class CompProperties_MythicCooldownSync : AbilityCompProperties
    {
        public CompProperties_MythicCooldownSync()
        {
            this.compClass = typeof(CompAbilityEffect_MythicCooldownSync);
        }
        
    }
}
