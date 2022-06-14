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

## Want To Contribute Flavor Text Directly?

Note: This section assumes an sufficient understanding of Git to make pull requests. 

If you wish to write and add new titles and descriptions directly to the mod, then there are only a few files you need to work with. First are the [translation key files](https://github.com/mlholland/MooMythicItems/tree/main/Languages/English/Keyed). These are the files that map names to blocks of text, like [mythic titles](https://github.com/mlholland/MooMythicItems/blob/main/Languages/English/Keyed/Titles.xml) and [mythic flavor descriptions](https://github.com/mlholland/MooMythicItems/blob/main/Languages/English/Keyed/Flavor_Descriptions.xml). The second group of files to be aware of are the [cause def files](https://github.com/mlholland/MooMythicItems/tree/main/1.3/Defs/MythicCauseDefs). These are explained above, but to recap, they are the files that define both a trigger that creates a mythic item, and as well as the pool of mythic titles, descriptions, and effects to draw from when making the resulting item.

In order to add new titles and descriptions, you must first define them as translation strings in the appropriate translation file. Simply assign them a unique key (that is <this> part of each line), and write whatever you like. When writing titles and descriptions, there are certain values you can enter which will be replaced by specific information in-game. For example adding `{0}` to a title will insert the creator pawn's short name (nickname if present, first name otherwise) into the title. See the relevant translation files for info on what you can inject into titles and descriptions.
 
Once you've created new titles and descriptions as translation keys, you must enter those key values into a MythicCauseDef. Just add it as a new line in one of 4 locations within the appropriate MythicCauseDef's XML: titles, descriptions, meleeTitles, or meleeDescriptions. If a mythic cause creates a weapon, and has uses different flavor text for melee and ranged weapons, then the `titles` and `descriptions` values are used for ranged weapons. 

### Testing 
 
If you would like to test your flavor text. Follow these steps: 
 - Prep Install the [Hugslib](https://steamcommunity.com/workshop/filedetails/?id=818773962) mod for some extra helper features.
 - Run Rimworld with the mod running locally (place it in the mods directory of your Rimworld game folder, then select the version of this mod from the mod selection list that has a folder next to its name instead of a Steam icon). 
 - Go to Rimworld's options and turn on development mode.
 - Go the this Mod's settings (click mod settings from the game options window) and enable the "Print Extra Startup Logs" option. This will cause the mod to print ALL flavor text to the game's logs on startup, with example values filling in for all the `{0}`'s and such.
 - Restart the game.
 - Once the game opens, click the left-most button from the array of buttons in the top center of the screen to open up the logs.
 - Click the Files button and select "Open log file".
 - You know have open a text file that contains all the flavor text currently in the mod. Ctrl-f for your flavor tex and see if it sounds ok with example names plugged in.
 
 If you want to see what your flavor text looks like on an in-game item. Perform the following steps:
 - Once again run Rimworld with this mod locally.
 - Enable development mode.
 - Click the debug actions menu from the buttons in the top center of the screen (fourth from the left, the first of the two triangle buttons).
 - Select the "Create Mythic Weapon" button near the bottom of the "Spwaning" list.
 - Select the mythic cause your flavor text is for (these are defNames in the XML), then select any mythic effect, then select a melee or ranged weapon if that matters. This will take you back to the game map. Spam click on the map's terrain to create a bunch of mythic items using that creation trigger. These randomly select from the title and description options of that mythic cause, so once a bunch of items are on the ground. Check them out to find one that uses your flavor text. In the future I plan to make it possible to select the description and title directly, but for now this is the only option.
 
 ### Submitting.
 
 Once you made your changes. Commit them to a new Git branch, push the branch to Github, and make a pull request to merge your branch into the main branch of this repository. I'll take a look, make some suggestions if I feel the need, then merge it into the main code base once any suggestions are resolved. From there the changes will be included the next time I update the mod!.
 
