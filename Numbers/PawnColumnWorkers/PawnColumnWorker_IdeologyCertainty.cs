using RimWorld;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_IdeologyCertainty : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            float? certainty = pawn.ideo?.Certainty;
            return certainty is float value && value >= 0f ? $"{value * 100f:F0}%" : "-";
        }

        protected override string GetTip(Pawn pawn)
        {
            Ideo ideo = pawn.ideo?.Ideo;
            string certainty = GetTextFor(pawn);

            NamedArgument ideoArg = ideo != null ? ideo.Named("IDEO") : "None".Named("IDEO");

            return "CertaintyInIdeo".Translate(pawn.Named("PAWN"), ideoArg) + $": {certainty}";
        }

        protected override string GetHeaderTip(PawnTable table) => "Certainty".Translate();

        public override int Compare(Pawn a, Pawn b)
        {
            float certaintyA = a.ideo?.Certainty ?? -1f;
            float certaintyB = b.ideo?.Certainty ?? -1f;
            return certaintyA.CompareTo(certaintyB);
        }
    }
}
