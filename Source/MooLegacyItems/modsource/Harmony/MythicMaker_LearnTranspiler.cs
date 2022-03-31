using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

/* This patch intercepts the code that sets a pawn's skill level, and spawns a mythic item if the code is
 * settings a colonist's skill to 20. Big thanks to the LevelUp mod that I jury-rigged this on. Transpilers are scary.
 */
namespace MooMythicItems
{
    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch(nameof(SkillRecord.Learn))]
    public class MythicMaker_LearnTranspiler
    {
        // todo consider making this configurable
        private static int s_mythicLevel = 20;
        private static readonly string s_reasonPrefix = "skillLegend-";
        
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
                        MethodInfo levelUpExtrCode = AccessTools.Method(typeof(MythicMaker_LearnTranspiler), nameof(OnLevelUp));
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
            // If the pawn's new skill level exceeds the level requirement, try to make a mythic item
            if(skillRecord.levelInt >= MooMythicItems_Mod.settings.minimumLevelForSkillItems)
            {
                if (MooMythicItems_Mod.settings.flagDebug)
                {
                    Log.Message(String.Format("moo: {0} leveled up {1} to {2}, which breaks the minimum mythic skill threshold of {3}. Attempting to create a mythic item.", pawn.Name, skillRecord.def.label, skillRecord.levelInt, MooMythicItems_Mod.settings.minimumLevelForSkillItems));
                }
                MythicReasonToDetailOptionsDef details = MythicReasonToDetailOptionsDef.Instance;
                String skillDefName = skillRecord.def.defName;
                List<String> descriptionOptions;
                List<String> titleOptions;
                List<MythicEffectDef> abilityOptions = null;
                string reason = s_reasonPrefix + skillDefName;
                Thing targetItem = null;

                // Set title, description, and ability options based on the specific skill
                switch (skillDefName)
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
                        abilityOptions = details.ConstructionMasterA;
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
                        Log.Error(String.Format("[Moo Mythic Items]: Failed to connect skill named '{0}' to any skills with mythic flavor. This is probably a mod compatibility issue that will require a mod. Go poke Mooloh about it in the mod page's comments", skillDefName));
                        return;
                }

                // validation
                if (descriptionOptions == null || descriptionOptions.Count == 0)
                {
                    Log.Error(String.Format("[Moo Mythic Items]: Failed to generate skill mastery-based mythic item because we couldn't find any description options for '{0}'", skillDefName));
                    return;
                }
                if (titleOptions == null || titleOptions.Count == 0)
                {
                    Log.Error(String.Format("[Moo Mythic Items]: Failed to generate skill mastery-based mythic item because we couldn't find any title options for '{0}'", skillDefName));
                    return;
                }
                if (abilityOptions == null || abilityOptions.Count == 0)
                {
                    Log.Error(String.Format("[Moo Mythic Items]: Failed to generate skill mastery-based mythic item because we couldn't find any ability options for '{0}'", skillDefName));
                    return;
                }
                if (targetItem == null)
                {
                    Log.Message(String.Format("[Moo Mythic Items]: Failed to generate skill mastery-based mythic item because we couldn't find an appropriate item to add a mythic to. The pawn {0} is either naked or unarmed in an odd circumstance.", pawn.Name));
                    return;
                }

                // If we've reached this code, then we have all the info we need to make a new mythic item, as well as the justification.
                // But before we do that, we double check that skill-based mythicItem for this pawn/world/skill combo.
                // double check that a more impressive kill count-based mythic item hasn't already been made for this pawn
                MythicItem cachedMythicItem = MythicItemManager.GetSimilarCachedMythicItem(null, reason, pawn.ThingID, Find.World.info.persistentRandomValue);
                if (cachedMythicItem != null)
                {
                    if (MooMythicItems_Mod.settings.flagDebug)
                    {
                        Log.Message(String.Format("[Moo Mythic Items]: Not creating this mythic item because a similar one is already saved."));
                    }
                    return;
                }

                MythicItem newMythicItem = new MythicItem(targetItem, pawn, descriptionOptions.RandomElement(), titleOptions.RandomElement(), abilityOptions.RandomElement(), reason);
                MythicItemManager.SaveNewMythicItem(newMythicItem);
            }
        }
    }
}