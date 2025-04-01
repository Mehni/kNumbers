namespace Numbers
{
    using RimWorld;
    using Verse;
    using UnityEngine;

    public class PawnColumnWorker_AnimalWildness : PawnColumnWorker_Text
    {
        protected override TextAnchor Anchor => TextAnchor.MiddleCenter;
        
        public override int Compare(Pawn a, Pawn b)
            => (a.AnimalOrWildMan() || b.AnimalOrWildMan())
                ? GetValue(a).CompareTo(GetValue(b))
                : 0;

        protected override string GetTextFor(Pawn pawn)
            => pawn.AnimalOrWildMan()
                ? GetValue(pawn).ToStringPercent()
                : string.Empty;

        private float GetValue(Pawn pawn)
            => pawn.RaceProps.wildness;
    }
}
