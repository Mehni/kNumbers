using RimWorld;
using UnityEngine;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_FoodConsumption : PawnColumnWorker_Text
    {
        protected override TextAnchor Anchor => TextAnchor.MiddleCenter;
        
        protected override string GetTextFor(Pawn pawn)
            => (pawn?.needs?.food?.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * 60000f ?? 0).ToString("0.##");

        public override int Compare(Pawn a, Pawn b)
            => (a?.needs?.food?.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * 60000f ?? 0)
                .CompareTo(
                b?.needs?.food?.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * 60000f ?? 0);
    }
}
