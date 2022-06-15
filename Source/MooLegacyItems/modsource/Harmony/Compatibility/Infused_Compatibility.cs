using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Reflection;

/* This class contains functions that make this mod compatible with the "Infused" mod. Specifically,
 * it includes patches for the saving and loading of mythic items in order to allow infusions to persist
 * across worlds. Note that this is for the original Infused mod, not the second successor mod.
 */
namespace MooMythicItems
{
    public class Infused_Compatibility
    {
        /*public static readonly string InfusedKey = "InfusedOneData";


        private static Type compInfusedType;
        private static Type infusedDefType;
        private static ConstructorInfo infusedDefConstructor;
        private static ConstructorInfo infusedDefListConstructor;
        private static AccessTools.FieldRef<object, bool> isNewField;
        private static AccessTools.FieldRef<object, List<Def>> infusionsField;


        // Tries to find all the needed values from  Infused via reflection. Returns true if all values were found and set, false otherwise.
        public static bool InitializeReflectionValues()
        {
            compInfusedType = AccessTools.TypeByName("Infused.CompInfused");
            if(compInfusedType == null)
            {
                Log.Error("Mythic Framework failed to find Infused CompInfused Class");
                return false;
            }

            infusedDefType = AccessTools.TypeByName("Infused.Def");
            if (infusedDefType == null)
            {
                Log.Error("Mythic Framework failed to find Infused.Def Class");
                return false;
            }
            infusedDefConstructor = AccessTools.Constructor(infusedDefType);

            isNewField = AccessTools.FieldRefAccess<bool>(compInfusedType, "isNew");
            if (isNewField == null)
            {
                Log.Error("Mythic Framework failed to find isNew field");
                return false;
            }

            infusionsField = AccessTools.FieldRefAccess<List<Def>>(compInfusedType, "infusions");
            if (infusionsField == null)
            {
                Log.Error("Mythic Framework failed to find infusions field");
                return false;
            }

            return true;
        }

        // After realizing a mythic item, check if it contains infusions, and add them to the resulting item if so.
        public static void RealizePatch(ThingWithComps __result, MythicItem __instance)
        {
            if (__instance != null)
            {
                ThingComp infusedComp = null;
                foreach (ThingComp comp in __result.AllComps)
                {
                    if (comp.GetType() == compInfusedType)
                    {
                        infusedComp = comp;
                        break;
                    }
                }
                if (infusedComp == null)
                {
                    Log.Message("no INfused comp found on mythic item");
                    return;
                }
                ClearInfusionComp(infusedComp); // If there's no infusion data saved, then the item shouldn't spawn with infusions ever.
                if (__instance.extraItemData != null && __instance.extraItemData.ContainsKey(InfusedKey))
                {
                    
                    ApplyEncodedComp(__instance.extraItemData[InfusedKey], infusedComp); 
                } 
            } 
        }


        // After initializing a mythic item from an in-game item, check if it has infusion data worth saving.
        public static void CreationPatch(MythicItem __instance, ThingWithComps item)
        {
            if (item == null) return;
            ThingComp infusedComp = null;
            foreach (ThingComp comp in item.AllComps)
            {
                if (comp.GetType() == compInfusedType)
                {
                    infusedComp = comp;
                    break;
                }
            }
            if (infusedComp == null)
            {
                DebugActions.LogErr("Infused is active, but no infusion comp was found an a {0} that was turned into a mythic item. Infusion data, if any, will not be preserved.", item.def.label);
                return;
            }

            IList infusions = infusionsField.Invoke(infusedComp);
            if (infusions != null && infusions.Count > 0)
            {
                DebugActions.LogIfDebug("Saving Infusion Data to Mythic Item...");
                string encodedEnchantComp = EncodeEnchantmentComp(infusions);
                __instance.extraItemData[InfusedKey] = encodedEnchantComp;
            } 
        }

        // helper function to encode enchant data into a single, dash-separated string.
        private static string EncodeEnchantmentComp(IList infusions)
        {
            return string.Join("^", from def in infusions.Cast<Def>() select def.defName); // whoops don't use dashes in case of negative numbers
        }

        // helper function to decode enchant data and apply its values to an in-game comp.
        private static void ApplyEncodedComp(string EncodedComp, ThingComp infusedComp)
        {
            DebugActions.LogIfDebug("Applying Infusions to Mythic Item");
            List<string> values = EncodedComp.Split('^').ToList();
            if (values.Count == 0)
            {
                DebugActions.LogErr("While trying to instantiate a saved mythic item into the world, infusion data was found, but it contained nothing. No infusions will be applied to the resulting item.");
                return;
            }
            isNewField.Invoke(infusedComp) = true;
            if (infusionsField.Invoke(infusedComp) == null)
            {
                //infusionsField.Invoke(infusedComp) = (List<Def>)infusedDefConstructor.Invoke(new List<Def>());
            }

            foreach (string defName in values)
            {
                bool found = false;
                // this is easier than trying to parameterize the defDatabase normally 
                // It's also horribly inefficient, but we can get away with this because this is almost never called.
                foreach (Def infusionDef in GenDefDatabase.GetAllDefsInDatabaseForDef(infusedDefType))
                {
                    if(infusionDef.defName == defName)
                    {
                        infusionsField.Invoke(infusedComp).Add(infusionDef);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    DebugActions.LogErr("Failed to find infusion def named {0}. It will not be added to a mythic item as originally intended.", defName);
                    continue;
                }
            }
            
        }

        private static void ClearInfusionComp(ThingComp infusedComp)
        {
            if (infusionsField.Invoke(infusedComp) != null)
            {
                infusionsField.Invoke(infusedComp) = null;
                isNewField.Invoke(infusedComp) = true;
            }
            
        }*/
    }     
}
