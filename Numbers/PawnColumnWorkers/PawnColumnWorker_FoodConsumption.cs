using RimWorld;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_FoodConsumption : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
            => RaceProperties.NutritionEatenPerDay(pawn);

        public override int Compare(Pawn a, Pawn b)
            => (a?.needs?.food?.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * 60000f ?? 0)
                .CompareTo(
                b?.needs?.food?.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * 60000f ?? 0);
    }
}
