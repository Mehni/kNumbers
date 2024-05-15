using System.Linq;

namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_Certainty : PawnColumnWorker
    {
        private static readonly Texture2D BarFullTexHor = SolidColorMaterials.NewSolidColorTexture(GenUI.FillableBar_Green);

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn?.Ideo == null) return;

            // SocialCardUtility.DrawPawnCertainty(pawn, rect);
            var iconRect = new Rect(rect.x, rect.y + rect.height / 2 - 16, 32, 32);
            pawn.Ideo.DrawIcon(iconRect);

            var barRect = new Rect(rect.x + 32, rect.y + 2, rect.width - 32, rect.height - 4);
            Widgets.FillableBar(barRect, pawn.ideo.Certainty, BarFullTexHor);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, pawn.Ideo.name.Truncate(barRect.width));
            Text.Anchor = TextAnchor.UpperLeft;

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                // IdeoUIUtility.DrawIdeoPlate(rect, pawn.Ideo, pawn);
                var taggedString1 = (TaggedString)pawn.Ideo.name.Colorize(ColoredText.TipSectionTitleColor);
                taggedString1 += "\n" + "Certainty".Translate().CapitalizeFirst() + ": " + pawn.ideo.Certainty.ToStringPercent();
                if (pawn.ideo.PreviousIdeos.Any())
                    taggedString1 += "\n\n" + "Formerly".Translate().CapitalizeFirst() + ": \n" + pawn.ideo.PreviousIdeos.Select(x => x.name).ToLineList("  - ");
                TooltipHandler.TipRegion(rect, (TipSignal)taggedString1.Resolve());
            }
        }

        public override int GetMinWidth(PawnTable table) => Mathf.Max(base.GetMinWidth(table), 120);

        public override int Compare(Pawn a, Pawn b) => (a.ideo?.Certainty ?? 0).CompareTo(b.ideo?.Certainty ?? 0);
    }
}