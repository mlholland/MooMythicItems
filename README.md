# MooMythicItems
A Rimworld mod which chronicles the triumphs of your colonists by creating 'Mythic Items' based on their achievements. Mythic Items are weapons and apparel with special names, descriptions, and abilities, which appear in your OTHER colonies as quest rewards.

Steamlink: TODO once published

# Info for modders

I designed this mod to make it possible for modders to add their own flavor text to existing mythic creation triggers, to add their own mythic effects, or add entirely new mythic creation triggers. For code examples, skip to the patch examples section, or continue reading the defs section for a small code overview.

## Defs
This mod adds some new defs. Here's an overview of the important ones. Once you've skimmed this, I encourage you to check out the actual [usages](https://github.com/mlholland/MooMythicItems/tree/main/1.3/Defs) of these defs to understand what they look like in practice.
 - MythicCauseDef: This represents a trigger that can create a mythic item, along with all the title, description, and effect options needed to then generate a new mythic item when the trigger is met. This def has several child def classes, each of which has extra fields. Check the [def files](https://github.com/mlholland/MooMythicItems/tree/main/Source/MooLegacyItems/modsource/Defs/Causes) for a full list of fields. One important and confusing field to be aware of is the 'workerClass'. This field links to a separate class which contains the code that actually recognizes when the mythic cause is met. This is needed to separate cause defs from trigger code, since many triggers use the same inputs, but require unique code to recognize when the trigger is met. The causeWorker files can be found [here](https://github.com/mlholland/MooMythicItems/tree/main/Source/MooLegacyItems/modsource/Workers). Here's a quick description of existing MythicCauseDefs:
   - Trigger when a colonist reaches a certain level in a selected skill. If multiple cause defs target the same skill, then it will create a mythic item using the highest skill requirement, and overwrite saved mythic items that rely on cause defs with lower requirements. 
   - Trigger when the game is won via any vanilla victory condition.
   - Trigger when a specific pawn record reaches a specified threshold. This is the most flexible trigger, and subsequently has more causeWorkers devoted to it. This def has the relative priority of each cause def set manually to determine which kill thresholds are 'more impressive' for the purposes of overwriting items from the same colonist. 
 - MythicEffectDef: This represents the custom behavior that a mythic item grants the user. The only field in the parent def class is the 'effectDescTranslationKey' field, which is the translation key for the effect description. All other fields will be found in the [child classes](https://github.com/mlholland/MooMythicItems/tree/main/Source/MooLegacyItems/modsource/Defs/Effects). At the moment, the following categories of effects are available:
   - Give the user a specified hediff while the item is equipped. Hediffs used for this effect must have the HediffCompProperties_RemoveIfApparelDropped comp if the hediff is meant to be granted by apparel-based mythic items, but NOT for hediffs that are meant to be granted by weapon-based mythic items. This is a result of vanilla Rimworld code.
   - Grant the user an ability while the item is equipped. This can be configured to have the ability only cooldown while worn, or to always start on cooldown when equipped. 
   - (weapon only) Cause enemies killed by someone wielding this weapon to drop extra items. The amount of loot in silver can be configured, as can the possible drops, and this can be configured to only work on specific enemies. There are flags for insects only, humanoids only, mechs only, or you can provide a list of allowed creature defs. Note that it applies to all kills by the wielder, not just kills with the weapon.
   - (weapon only) Cause kills by someone wielding this weapon to grant the user a (presumably temporary) hediff. Can be configured to provide a certain amount of hediff severity on each kill up to a stack limit. Also allows for valid target configuration like the looting effect. Note that it applies to all kills by the wielder, not just kills with the weapon.
 If you want to create your own effects that can't be based of these. You'll need a new mythicEffectDef child class. Be aware that that there are only 3 points where you can add custom behavior to a mythic item: On equip, unequip, and when the user kills something (weapons only).
 - [RecipeHediffRequirementsDef](https://github.com/mlholland/MooMythicItems/blob/main/Source/MooLegacyItems/modsource/Defs/RecipeHediffRequirementsDef.cs). This def allows us to restrict recipes to only be performed by colonists who currently have a specific hediff. This is used in combination with the 'grant a hediff' mythic effect to allow mythic items to 'unlock' special recipes like Epicurean Delights.
 
 ## Examples
 
At the moment I just have 3 simple examples - 1 patch and 2 conditional defs. Feel free to ask about more advanced topics. If there appears to be interest, I'll add more complex examples that involve c# coding.

  - [How to add new title, description, or effect options to an existing MythicCauseDef](https://github.com/mlholland/MooMythicItems/blob/main/1.3/Patches/examples/addValuesToExistingCauseDef.xml)
  - [How to create an entirely new mythic effect](https://github.com/mlholland/MooMythicItems/blob/main/1.3/Defs/Examples/addNewEffectDefUsingExistingOptions.xml)
  - [How to create a new mythic cause](https://github.com/mlholland/MooMythicItems/blob/main/1.3/Defs/Examples/addNewCauseDefUsingExistingOptions.xml)


 
