using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using HarmonyLib;

/* Turns the code that's buried in the CompDeepScanner class to alert the player of minerals being found into an actual incident to be used elsewhere.
 */
namespace MooMythicItems
{
    public class IncidentWorker_MineralsProspected : IncidentWorker
    {
        private static readonly string letterDesc = "MooMF_MineralsProspectedLetterText";
        private static readonly string letterTitle = "MooMF_MineralsProspectedLetterTitle";

        // Token: 0x060049D7 RID: 18903 RVA: 0x00188404 File Offset: 0x00186604
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => this.CanScatterAt(x, map), map, out intVec))
            {
                DebugActions.LogErr("Could not find a center cell for mythic mineral lump generation!");
                return false;
            }
            ThingDef thingDef = this.ChooseLumpThingDef();
            int numCells = Mathf.CeilToInt((float)thingDef.deepLumpSizeRange.RandomInRange);
            foreach (IntVec3 intVec2 in GridShapeMaker.IrregularLump(intVec, map, numCells))
            {
                if (this.CanScatterAt(intVec2, map) && !intVec2.InNoBuildEdgeArea(map))
                {
                    map.deepResourceGrid.SetAt(intVec2, thingDef, thingDef.deepCountPerCell);
                }
            }
            Find.LetterStack.ReceiveLetter(String.Format(letterTitle.Translate(), thingDef.LabelCap), String.Format(letterDesc.Translate(), thingDef.label), LetterDefOf.PositiveEvent, new LookTargets(intVec, map), null, null, null, null);
            return true;
        }

        private bool CanScatterAt(IntVec3 pos, Map map)
        {
            int num = CellIndicesUtility.CellToIndex(pos, map.Size.x);
            TerrainDef terrainDef = map.terrainGrid.TerrainAt(num);
            return (terrainDef == null || !terrainDef.IsWater || terrainDef.passability != Traversability.Impassable) && terrainDef.affordances.Contains(ThingDefOf.DeepDrill.terrainAffordanceNeeded) && !map.deepResourceGrid.GetCellBool(num);
        }

        protected ThingDef ChooseLumpThingDef()
        {
            return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight((ThingDef def) => def.deepCommonality);
        }

    }
}
