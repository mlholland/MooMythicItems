using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* Defines a cause that can result in a mythic item being created. Modeled off of the worker class system employed by LegoDude in several mods, including:
 * https://github.com/AndroidQuazar/VanillaFactionsExpandedAncients/blob/7646d5fb32c3185561ddbb66b7828f1331f05cee/Source/VFEAncients/PowerDef.cs#L47
 * Big thanks to Lego for explaining and suggesting this.
 * 
 * After RW is done loading in defs, each MythicCauseDef sets up the necessary infrastructure to identify when the conditions for this mythic
 * cause are met. This is almost always achieved by Harmony patching existing functions to notice when something interesting happens (like a record def getting updated to a certain threshold).
 * In addition, this setup infrastructure calls upon a workerclass to do this setup. This means that new worker classes can be created and supplied to a MythicCauseDef to allow for mythic causes
 * that depend on new patches to be identified.
 * */
namespace MooMythicItems
{
    public class MythicCauseDef : Def
    {
        // Options that this cause can draw from when creating a mythic item.
        public List<MythicEffectDef> effects;
        public List<String> descriptions;
        public List<String> titles;

        // If true, then use the pawn's currently equipped weapon to make a mythic item.
        // If false, then randomly select from their apparel to make a mythic item.
        public bool createsMythicWeapon = false;
        // if true, then use the melee lists below when creating a mythic item out of a melee weapon, and use the original lists for ranged weapons.
        public bool hasDifferentMeleeOptions = false;
        public List<MythicEffectDef> meleeEffects;
        public List<String> meleeDescriptions;
        public List<String> meleeTitles;

        /* If positive, then this is the maximum number of mythic items with identical reasons that this cause will save, regardless of the originating pawn or colony of those items.
         * This feature is incompatible with priority-based logic, and is ignored if this cause's priority is non-zero. */
        public int reasonLimit = 0;
        /* This value is always added to a created mythic item's reason after a dash. Ex: survivalist -> survivalist-0.
         * This is added even if the priority is unset and unused (AKA at 0).
         * Certain sub-types of mythic item use this priority value to determine whether or not a mythic item should overwrite another 
         * EX: the same colonist reaches the second insect kill threshold, so their original first threshold mythic item is replaced. */
        public int priority = 0;

        // Class for inputting specific logic for determining if a mythic cause is met. 
        public Type workerClass = typeof(CauseWorker);
        protected CauseWorker Worker { get; set; }

        public MythicCauseDef() { }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;
            if (workerClass == null) yield return "Worker class cannot be null";
            if (effects == null || effects.Count == 0) yield return "effects list must be nonnull and have at least one element";
            if (descriptions == null || descriptions.Count == 0) yield return "descriptions list must be nonnull and have at least one element";
            if (titles == null || titles.Count == 0) yield return "titles list must be nonnull and have at least one element";
            if (hasDifferentMeleeOptions)
            {
                if (!createsMythicWeapon) yield return "Cannot set hasDifferentMeleeOptions to true for MythicCauseDefs that don't have weaponCause set to true.";
                if (meleeEffects == null || meleeEffects.Count == 0) yield return "meleeEffects list must be nonnull and have at least one element if hasDifferentMeleeOptions set to true";
                if (meleeDescriptions == null || meleeDescriptions.Count == 0) yield return "meleeDescriptions list must be nonnull and have at least one element if hasDifferentMeleeOptions set to true";
                if (meleeTitles == null || meleeTitles.Count == 0) yield return "meleeTitles list must be nonnull and have at least one element if hasDifferentMeleeOptions set to true";
            }
            if (!typeof(CauseWorker).IsAssignableFrom(workerClass)) yield return "Worker class must be subclass of CauseWorker";
        }

        // Since the startup code is dependent on referenced Defs, we need to run said code after 
        // ResolveReferences, rather than during a PostLoad override.
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            Worker = (CauseWorker)Activator.CreateInstance(workerClass, this);
            Worker.EnableCauseRecognition(MooMythicItems_Mod.harm);
        }

        public virtual MythicItem TryCreateMythicItem(Pawn originator, string reason)
        {
            DebugActions.LogIfDebug("Mythic Cause '{0}' is trying to create a mythic item.", this.defName);
            if (reason == null || reason.Length == 0)
            {
                DebugActions.LogErr("Mythic Item Creation Failed. an empty or null reason was inputted.");
                return null;
            }
            if (originator == null || !originator.IsColonist || originator.NonHumanlikeOrWildMan())// only make mythic items for colonists, not animals, and not raiders
            {
                DebugActions.LogIfDebug("Mythic Item Creation Failed. The inputted pawn either wasn't a colonist, wasn't humanoid, or didn't exist at all.");
                return null;
            }
            // Get the item to turn into a mythic item, and return null if nothing could be found that fits this cause's requirements.
            Thing item = null;
            string title = titles.RandomElement(), description = descriptions.RandomElement();
            MythicEffectDef effect = effects.RandomElement();
            if (createsMythicWeapon)
            {
                if (originator.equipment == null)
                {
                    DebugActions.LogIfDebug("Mythic Item Creation Failed. The inputted pawn {0} did not have any equipment", originator.Name);
                    return null;
                }
                item = originator.equipment.Primary;
                // don't allow stackabout stuff like thrumbo horns and wood
                // don't allow single use stuff like rocket launchers
                if (item == null)
                {
                    DebugActions.LogIfDebug("Mythic Item Creation Failed. The inputted pawn {0} did not have any weapon equipped.", originator.Name);
                    return null;
                }
                if (!MythicItemUtilities.IsValidDefOption(item.def))
                {
                    DebugActions.LogIfDebug("Mythic Item Creation Failed. The inputted pawn {0} did not have a valid weapon equipped.", originator.Name);
                    return null;
                }
                // modify attributes selected if the user has a melee weapon and this cause has separate melee options.
                if (hasDifferentMeleeOptions && !item.def.IsRangedWeapon)
                {
                    title = meleeTitles.RandomElement();
                    description = meleeDescriptions.RandomElement();
                    effect = meleeEffects.RandomElement();
                }
            }
            else 
            {
                if (originator.apparel != null && originator.apparel.AnyApparel && originator.apparel.WornApparel != null && originator.apparel.WornApparelCount > 0)
                {
                    item = originator.apparel.WornApparel.RandomElement();
                } else
                {
                    DebugActions.LogIfDebug("Mythic Item Creation Failed. The inputted pawn {0} is not wearing any apparel", originator.Name);
                    return null;
                }
            }
            // make sure we got 
            if (item == null)
            {
                DebugActions.LogErr("Mythic Item Creation Failed. Failed to find a valid item to make a mythic item for an unknown reason.");
                return null;
            }
            if (title == null)
            {
                DebugActions.LogErr("Mythic Item Creation Failed - no valid title option was found. This should never happen, and indicates a malformed MythicCauseDef. The cause def in use was '{0}'", this.defName);
                return null;
            }
            if (description == null)
            {
                DebugActions.LogErr("Mythic Item Creation Failed - no valid description option was found. This should never happen, and indicates a malformed MythicCauseDef. The cause def in use was '{0}'", this.defName);
                return null;
            }
            if (effect == null)
            {
                DebugActions.LogErr("Mythic Item Creation Failed - no valid effect option was found. This should never happen, and indicates a malformed MythicCauseDef. The cause def in use was '{0}'", this.defName);
                return null;
            }
            return new MythicItem(item, originator, description, title, effect, reason + "-" + priority);
        }

        /** This string should be a sentence fragment, which will be injected into the printed statement about a mythic item being produced */
        public virtual String GetPrintedReasonFragment(params object[] args) {
            string translationKey = Worker.GetReasonFragmentKey();
            if (translationKey == null)
            {
                DebugActions.LogErr("Causeworker is returning a null format key. That shouldn't happen.");
                return null;
            }
            return String.Format(translationKey.Translate(), args);
        }
    }
}
