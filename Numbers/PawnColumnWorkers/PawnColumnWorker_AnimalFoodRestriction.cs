using RimWorld;
using UnityEngine;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_AnimalFoodRestriction : PawnColumnWorker_FoodRestriction
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.foodRestriction == null)
            {
                PawnColumnWorker_FoodRestrictionMassAssign.DoNoTrackerMsg(rect);
            }
            else
            {
                base.DoCell(rect, pawn, table);    
            }
        }
    }
}