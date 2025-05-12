using RimWorld;
using UnityEngine;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_LeatherType : PawnColumnWorker_Text
    {
        protected override TextAnchor Anchor => TextAnchor.MiddleCenter;
        
        protected override string GetTextFor(Pawn pawn)
        {
            return pawn.RaceProps?.leatherDef?.LabelCap;
        }
    }
}
