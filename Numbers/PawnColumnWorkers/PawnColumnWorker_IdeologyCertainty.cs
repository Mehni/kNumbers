using RimWorld;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_IdeologyCertainty : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            float? certainty = pawn.ideo?.Certainty;
            return certainty is float value && value >= 0f
                ? $"{value * 100f:F0}%"
                : "-";
        }

        protected override string GetTip(Pawn pawn)
        {
            string ideoName = pawn.ideo?.Ideo?.name ?? "None";
            string certainty = GetTextFor(pawn);
            return $"Certainty in {ideoName}: {certainty}";
        }

        protected override string GetHeaderTip(PawnTable table)
            => "The Certainty of Pawn in Ideology.";

        public override int Compare(Pawn a, Pawn b)
        {
            float certaintyA = a.ideo?.Certainty ?? -1f;
            float certaintyB = b.ideo?.Certainty ?? -1f;
            return certaintyA.CompareTo(certaintyB);
        }
    }
}
