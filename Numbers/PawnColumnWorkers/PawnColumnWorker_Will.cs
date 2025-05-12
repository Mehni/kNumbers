using RimWorld;
using Verse;
using UnityEngine;

namespace Numbers
{
    public class PawnColumnWorker_Will : PawnColumnWorker_Text
    {
        protected override TextAnchor Anchor => TextAnchor.MiddleCenter;
        
        protected override string GetTextFor(Pawn pawn)
            => pawn.guest.will.ToString("F1");

        protected override string GetTip(Pawn pawn)
        {
            var range = pawn.kindDef.initialWillRange.Value;

            return string.Format("{0} : {1}-{2}", "WillFromPawnKind".Translate(pawn.kindDef.LabelCap), range.min, range.max);
        }

        protected override string GetHeaderTip(PawnTable table)
            => "WillLevelDesc".Translate();

        public override int Compare(Pawn a, Pawn b)
            => a.guest?.will.CompareTo(b?.guest?.will) ?? 0;
    }
}
