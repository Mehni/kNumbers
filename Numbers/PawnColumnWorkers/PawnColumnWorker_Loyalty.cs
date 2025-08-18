using RimWorld;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_Loyalty : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            if (pawn.guest == null) return string.Empty;

            return pawn.guest.Recruitable ? "UnwaveringlyLoyal".Translate() : "Recruitable".Translate();
        }

        public override int Compare(Pawn a, Pawn b)
        {
            bool aUnwavering = a.guest?.Recruitable == true;
            bool bUnwavering = b.guest?.Recruitable == true;

            return aUnwavering.CompareTo(bUnwavering);
        }

        protected override string GetTip(Pawn pawn)
        {
            return "UnwaveringlyLoyalDesc".Translate();
        }
    }
}
