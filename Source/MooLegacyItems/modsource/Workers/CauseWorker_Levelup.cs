using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

/* Worker class emplyed by some MythicCauseDef_SkillThreshold to determine if a skill has increased to the point that we should create a mythic item for it.
 */
namespace MooMythicItems
{
    public class CauseWorker_LevelUp : CauseWorker
    {

        public static readonly string masteryPrefix = "skill-mastery-";
        public static Dictionary<SkillDef, List<MythicCauseDef_LevelUp>> skillsWatched = new Dictionary<SkillDef, List<MythicCauseDef_LevelUp>>();

        public CauseWorker_LevelUp(MythicCauseDef def) : base(def) { }

        //
        public override void enableCauseRecognition(Harmony harm)
        {
            base.enableCauseRecognition(harm);
            MythicCauseDef_LevelUp causeDef = def as MythicCauseDef_LevelUp;
            if (causeDef == null)
            {
                Log.Error(String.Format("[Moo Mythic Items] Kill-recording cause worker was supplied a mythic cause def {0} that wasn't the RecordThreshold sub-type." +
                    " Use a different worker class, or correct the def class.", def.defName));
                return;
            }
            if (!skillsWatched.ContainsKey(causeDef.skill))
            {
                skillsWatched.Add(causeDef.skill, new List<MythicCauseDef_LevelUp>());
            } else
            {
                foreach(MythicCauseDef_LevelUp savedCause in skillsWatched[causeDef.skill])
                {
                    if (savedCause.minLevelThreshold == causeDef.minLevelThreshold)
                    {
                        Log.Error(String.Format("Moo Mythic Items] Encountered error while loading mythic item creation causes. Two skill-based causes, '{0}' and '{1}' have the same level threshold." +
                            " The second cause '{1}' will be ignored.", savedCause.defName, causeDef.defName));
                        return;
                            
                    }
                }
            }
            skillsWatched[causeDef.skill].Add(causeDef);
            if (MooMythicItems_Mod.settings.flagDebug)
            {
                Log.Message(String.Format("[Moo Mythic Items] accounting for new mythic cause that waits for skill '{0}' to reach a level of {1}.", causeDef.skill.defName, causeDef.minLevelThreshold));
            }
        }

        // This code is added into the vanilla skill gain code right after a level up occurs via the transpiler below.
        private static void OnLevelUp(SkillRecord skillRecord, Pawn pawn)
        {
            if (skillsWatched.ContainsKey(skillRecord.def))
            {
                MythicCauseDef_LevelUp bestCause = null;
                int curThreshold = -1;
                foreach (MythicCauseDef_LevelUp causeDef in skillsWatched[skillRecord.def])
                {
                    if (causeDef.minLevelThreshold > curThreshold && skillRecord.levelInt >= causeDef.minLevelThreshold)
                    {
                        curThreshold = causeDef.minLevelThreshold;
                        bestCause = causeDef;
                    }
                }
                if (curThreshold > -1)
                {
                    if (MooMythicItems_Mod.settings.flagDebug)
                    {
                        Log.Message(String.Format("[Moo Mythic Items] Trying to create new mythic item for {0} based on cause {1}.", pawn.Name, bestCause.defName));
                    }
                    string reason = masteryPrefix + skillRecord.def.skillLabel;
                    MythicItem newItem = bestCause.TryCreateMythicItem(pawn, reason);
                    if (newItem != null)
                    {
                        // When doing reason checks, we only care about mythic items related to the same skill,
                        // So we input a reason fragment that specifies the skill in question.
                        MythicItemCache.TrySaveOrOverwriteNewItem(newItem, reason, bestCause.priority, bestCause.reasonLimit);
                    }
                }
            }
        }


        // Transpiler which injects code into vanilla level up behavior.
        // Needs to be a trnaspiler instead of a patch because there's no way to know before/after the function
        // if a level up occurred without just recreating the entire function, and that sounds like a recipe for
        // mod incompatibility.
        // TBH I don't really understand how this works. I copied and trimmed down a similar transpiler from the
        // LevelUp! Mod's github. Big thanks to that mod's author.
        [HarmonyPatch(typeof(SkillRecord)), HarmonyPatch(nameof(SkillRecord.Learn))]
        private static class Learn_Transpiler
        {
            static FieldInfo skillRecordLevelField = AccessTools.Field(typeof(SkillRecord), nameof(SkillRecord.levelInt));
            static FieldInfo skillRecordPawnField = AccessTools.Field(typeof(SkillRecord), "pawn");
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
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
                            MethodInfo levelUpExtrCode = AccessTools.Method(typeof(CauseWorker_LevelUp), nameof(OnLevelUp));
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
        }

    }
}
