using RimWorld;
using UnityEngine;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_ToxicBuildup : PawnColumnWorker_Text
    {
        private static readonly Color LowToxicColor = Color.gray;
        private static readonly Color MediumToxicColor = Color.yellow;  // yellowish
        private static readonly Color HighToxicColor = new Color(0.9f, 0.5f, 0f);    // orange
        private static readonly Color CriticalToxicColor = Color.red;

        public override int Compare(Pawn a, Pawn b)
        {
            float aSeverity = GetToxicSeverity(a);
            float bSeverity = GetToxicSeverity(b);
            return aSeverity.CompareTo(bSeverity);
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            GUI.color = GetColorForToxicSeverity(GetToxicSeverity(pawn));
            base.DoCell(rect, pawn, table);
            GUI.color = Color.white;
        }

        protected override string GetTextFor(Pawn pawn)
        {
            float severity = GetToxicSeverity(pawn);
            return $"{(severity * 100f):0.#}%";
        }

        protected override string GetTip(Pawn pawn)
        {
            return HediffDefOf.ToxicBuildup.description;
        }

        private float GetToxicSeverity(Pawn pawn)
        {
            if (pawn?.health == null) return 0f;
            Hediff toxicHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
            return toxicHediff?.Severity ?? 0f;
        }

        private Color GetColorForToxicSeverity(float severity)
        {
            if (severity <= 0f) return LowToxicColor;
            if (severity < 0.25f) return MediumToxicColor;
            if (severity < 0.5f) return HighToxicColor;
            return CriticalToxicColor;
        }
    }
}