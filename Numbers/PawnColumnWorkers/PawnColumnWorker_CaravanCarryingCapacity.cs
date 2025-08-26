using RimWorld;
using UnityEngine;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_CaravanCarryingCapacity : PawnColumnWorker_Text
    {
        public override int Compare(Pawn a, Pawn b)
        {
            float capA = MassUtility.Capacity(a);
            float capB = MassUtility.Capacity(b);
            return capA.CompareTo(capB);
        }

        protected override string GetTextFor(Pawn pawn)
        {
            if (pawn == null)
                return "";

            float capacity = MassUtility.Capacity(pawn);
            return $"{capacity:0.#} kg";
        }

        protected override string GetTip(Pawn pawn)
        {
            if (pawn == null)
                return null;
            return "Amount of mass that can be carried in caravan. It is not same as carrying capacity.";
        }
    }
}