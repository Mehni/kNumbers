namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using System.Linq;
    using Verse;

    public class PawnColumnWorker_AllowedAreaWithMassAssign : PawnColumnWorker_AllowedAreaWide
    {

        protected override void HeaderClicked(Rect headerRect, PawnTable table)
        {
            if (Find.CurrentMap == null)
            {
                return;
            }
            var allAreas = Find.CurrentMap.areaManager.AllAreas;
            var assignableAreas = 1 + allAreas.Count(area => area.AssignableAsAllowed());
            var rectWidth = headerRect.width / (float)assignableAreas;
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;
            var num3 = 1;
            foreach (var area in allAreas)
            {
                if (area.AssignableAsAllowed())
                {
                    var num4 = (float)num3 * rectWidth;
                    var rect = new Rect(headerRect.x + num4, headerRect.y, rectWidth, headerRect.height);
                    if (Mouse.IsOver(rect) && Event.current.control && Event.current.button == 0)
                    {
                        table.PawnsListForReading.ForEach(pawn => pawn.playerSettings.AreaRestriction = area);
                    }
                    num3++;
                }
            }
            Text.WordWrap = true;
            Text.Font = GameFont.Small;

            base.HeaderClicked(headerRect, table);
        }
    }
}