using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

/* This patch intercepts the code that sets a pawn's skill level, and spawns a legacy item if the code is
 * settings a colonist's skill to 20.
 * 
 * 
 */
namespace MooLegacyItems
{
    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch(nameof(SkillRecord.Learn))]
    public class LegacyMaker_LearnTranspiler
    {
        // todo consider making this configurable
        private static int s_legacyLevel = 20;
        private static readonly string s_reasonPrefix = "skillLegend";
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo skillRecordLevelField = AccessTools.Field(typeof(SkillRecord), nameof(SkillRecord.levelInt));
            FieldInfo skillRecordPawnField = AccessTools.Field(typeof(SkillRecord), "pawn");
            CodeInstruction previousInstruction = null;

            foreach (CodeInstruction currentInstruction in instructions)
            {
                yield return currentInstruction;

                // We react to additions and subtractions to SkillRecord.levelInt. Those are level changes.
                if (currentInstruction.StoresField(skillRecordLevelField))
                {
                    // Resolve method to call if this is a level change.
                    if (previousInstruction.opcode == OpCodes.Add)
                    {
                        MethodInfo levelUpExtrCode = AccessTools.Method(typeof(LegacyMaker_LearnTranspiler), nameof(OnLevelUp));
                        // Put two SkillRecord instances on stack
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Dup);

                        // Put Pawn instance on stack
                        yield return new CodeInstruction(OpCodes.Ldfld, skillRecordPawnField);

                        // Call method that corresponds to level up/level down.
                        yield return new CodeInstruction(OpCodes.Call, levelUpExtrCode); 
                    } 
                }

                previousInstruction = currentInstruction;
            }
        }

        private static void OnLevelUp(SkillRecord skillRecord, Pawn pawn)
        {
            Log.Message(String.Format("moo: {0} leveled up {1} to {2}", pawn.Name, skillRecord.def.label ,skillRecord.levelInt));
            if(skillRecord.levelInt >= s_legacyLevel)
            {
                LegacyReasonToDetailOptionsDef details = LegacyReasonToDetailOptionsDef.Instance;
                String skillDefName = skillRecord.def.defName;
                List<String> descriptionOptions;
                List<String> titleOptions;
                Thing targetItem = null;
                switch(skillDefName)
                {
                    case ("Shooting"):
                        descriptionOptions = details.ShootingMasterFD;
                        titleOptions = details.ShootingMasterT;
                        targetItem = pawn.equipment.Primary;
                        break;
                    case ("Melee"):
                        descriptionOptions = details.MeleeMasterFD;
                        titleOptions = details.MeleeMasterT;
                        targetItem = pawn.equipment.Primary;
                        break;
                    case ("Construction"):
                        descriptionOptions = details.ConstructionMasterFD;
                        titleOptions = details.ConstructionMasterT;
                        targetItem = pawn.apparel.WornApparel.RandomElement();
                        break;
                    case ("Mining"):
                        descriptionOptions = details.MiningMasterFD;
                        titleOptions = details.MiningMasterT;
                        targetItem = pawn.apparel.WornApparel.RandomElement();
                        break;
                    case ("Cooking"):
                        descriptionOptions = details.CookingMasterFD;
                        titleOptions = details.CookingMasterT;
                        targetItem = pawn.apparel.WornApparel.RandomElement();
                        break;
                    case ("Plants"):
                        descriptionOptions = details.PlantsMasterFD;
                        titleOptions = details.PlantsMasterT;
                        targetItem = pawn.apparel.WornApparel.RandomElement();
                        break;
                    case ("Animals"):
                        descriptionOptions = details.AnimalsMasterFD;
                        titleOptions = details.AnimalsMasterT;
                        targetItem = pawn.apparel.WornApparel.RandomElement();
                        break;
                    case ("Crafting"):
                        descriptionOptions = details.CraftingMasterFD;
                        titleOptions = details.CraftingMasterT;
                        targetItem = pawn.apparel.WornApparel.RandomElement();
                        break;
                    case ("Artistic"):
                        descriptionOptions = details.ArtisticMasterFD;
                        titleOptions = details.ArtisticMasterT;
                        targetItem = pawn.apparel.WornApparel.RandomElement();
                        break; 
                    case ("Medicine"):
                        descriptionOptions = details.MedicineMasterFD;
                        titleOptions = details.MedicineMasterT;
                        targetItem = pawn.apparel.WornApparel.RandomElement();
                        break;
                    case ("Social"):
                        descriptionOptions = details.SocialMasterFD;
                        titleOptions = details.SocialMasterT;
                        targetItem = pawn.apparel.WornApparel.RandomElement();
                        break;
                    case ("Intellectual"):
                        descriptionOptions = details.IntellectualMasterFD;
                        titleOptions = details.IntellectualMasterT;
                        targetItem = pawn.apparel.WornApparel.RandomElement();
                        break; 
                    default:
                        Log.Error(String.Format("[Moo Legacy Items]: Failed to connect skill named '{0}' to any skills with legacy flavor. This is probably a mod compatibility issue that will require a mod. Go poke Mooloh about it in the mod page's comments", skillDefName));
                        return;
                }
                if (descriptionOptions == null || descriptionOptions.Count == 0)
                {
                    Log.Error(String.Format("[Moo Legacy Items]: Failed to generate skill mastery-based legacy item because we couldn't find any description options for '{0}'", skillDefName));
                    return;
                }
                if (titleOptions == null || titleOptions.Count == 0)
                {
                    Log.Error(String.Format("[Moo Legacy Items]: Failed to generate skill mastery-based legacy item because we couldn't find any title options for '{0}'", skillDefName));
                    return;
                }
                if (targetItem == null)
                {
                    Log.Message(String.Format("[Moo Legacy Items]: Failed to generate skill mastery-based legacy item because we couldn't find an appropriate item to add a legacy to. The pawn {0} is either naked or unarmed in an odd circumstance.", pawn.Name));
                }

                // Double check that we don't already have a skill-based legacyItem for this pawn/world/skill combo.
                // No need to check the defs, we don't need both a cowboy hat AND a duster to mark Jimbo's lvl 20 mining achievement.
                List<LegacyItem> itemsFromThisWorld = LegacyItemManager.GetLegacyItemsFromThisColony(Verse.Find.World.info.persistentRandomValue);
                String reason = s_reasonPrefix + skillDefName; 
                foreach (LegacyItem li in itemsFromThisWorld)
                {
                    if(li.reason.Equals(reason) && li.originatorId == pawn.ThingID)
                    {
                        return;
                    }
                }
                LegacyItem newLegacyItem = new LegacyItem(targetItem, pawn, descriptionOptions.RandomElement(), titleOptions.RandomElement(), "SkillMasterPlaceholder", reason);
                LegacyItemManager.SaveNewLegacyItem(newLegacyItem);
            }
        }
    }
}