using System;
using System.Collections.Generic;
using System.Linq; 
using RimWorld;
using Verse;

/* This comp is creates a mythic item from the user. The mythic cause def to draw from is selected by ???????
 */
namespace MooMythicItems
{
    class CompUseEffect_WithDefInput : CompUseEffect
    {
        public CompProperties_UseEffectWithDefInput Props
        {
            get
            {
                return (CompProperties_UseEffectWithDefInput)this.props;
            }
        }

        public virtual string ParameterizeUseLabel(Def input)
        {
            MythicCauseDef def = input as MythicCauseDef;
            if (def == null)
            {
                DebugActions.LogErr("Inputted def that wasn't actually a MythicCauseDef into the manual mythic maker code: {0}", input.defName);
                return "???";
            }
            return string.Format(Props.parameterizeableUseLabel, def.label);
        }
    }
}
