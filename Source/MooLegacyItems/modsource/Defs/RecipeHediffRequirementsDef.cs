using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* These defs all add their values to a large static map, which is then consumed by a harmony patch in bill acceptance to
 * ensure that only pawns with the required hediffs can accept a job. In order for the job to be accepted, the pawn in question
 * must possess ANY of the listed hediffs. ATM there's no way to modify this logic - it's just an OR-ing of all the possible hediffDef
 * options.
 * 
 * TODO decide on how to enable debug mode on this - perhaps a settings flag, or just make it part of god mode?
 */
namespace MooMythicItems
{
    public class RecipeHediffRequirementsDef : Def
    {
        public List<HediffDef> hediffDefOptions;
        public RecipeDef recipeDef;


        private static Dictionary<RecipeDef, HashSet<HediffDef>> totalMapping = new Dictionary<RecipeDef, HashSet<HediffDef>>();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;
            if (recipeDef == null) yield return "recipeDef cannot be null";
            if (hediffDefOptions == null || hediffDefOptions.Count == 0) yield return "RequiredHediffDefs cannot be null or empty";
        }

        // Since the startup code is dependent on referenced Defs, we need to run said code after 
        // ResolveReferences, rather than during a PostLoad override.
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if(!totalMapping.ContainsKey(recipeDef))
            {
                totalMapping[recipeDef] = new HashSet<HediffDef>();
            }
            HashSet<HediffDef> realList = totalMapping[recipeDef];
            foreach(HediffDef hediffDef in hediffDefOptions)
            {
                realList.Add(hediffDef);
            }
        }

        public static bool PawnMeetsHediffRequirement(Pawn p, RecipeDef recipeDef)
        {
            if(totalMapping.ContainsKey(recipeDef))
            {
                foreach(HediffDef hediffDef in totalMapping[recipeDef])
                {
                    if(p.health.hediffSet.HasHediff(hediffDef))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }
    }
}
