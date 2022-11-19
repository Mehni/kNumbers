using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Numbers
{
    public class PawnColumnWorker_FoodRestrictionMassAssign : PawnColumnWorker_FoodRestriction
    {

        private static bool dragging = false;
        private static Dictionary<int, Texture2D> textureCache = new();

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.Faction != Faction.OfPlayer && !pawn.IsPrisonerOfColony)
                return;

            if (pawn.foodRestriction == null)
            {
                DoNoTrackerMsg(rect);
                return;
            }

            var allFoodRestrictions = Current.Game.foodRestrictionDatabase.AllFoodRestrictions;

            float width = rect.width / allFoodRestrictions.Count;
            
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;

            // draw rectangle selector for all food restrictions
            for (var index = 0; index < allFoodRestrictions.Count; index++)
            {
                var foodRestriction = allFoodRestrictions[index];
                var offset = index * width;
                DoFoodSelector(new Rect(rect.x + offset, rect.y, width, rect.height), pawn, foodRestriction);
            }
            
            Text.WordWrap = true;
            Text.Font = GameFont.Small;
        }

        public static void DoNoTrackerMsg(Rect rect)
        {
            Verse.Text.Anchor = TextAnchor.MiddleCenter;
            Verse.Text.Font = GameFont.Tiny;
            TaggedString label;
            GUI.color = Color.gray;
            label = "Food restrictions not supported";
            Widgets.Label(rect, label);
            Verse.Text.Anchor = TextAnchor.UpperLeft;
            Verse.Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }
        
        private void DoFoodSelector(Rect rect, Pawn pawn, FoodRestriction foodRestriction)
        {
            if (pawn.foodRestriction == null)
                return;
            
            MouseoverSounds.DoRegion(rect);
            rect = rect.ContractedBy(1f);
            
            // generate color texture depending on label string and cache it
            var baseRand = foodRestriction.label.GetHashCode();
            
            if (!textureCache.ContainsKey(baseRand))
            {
                // create new Texture2D if not already cached
                var color = new Color(Rand.ValueSeeded(baseRand), Rand.ValueSeeded(baseRand+1),
                    Rand.ValueSeeded(baseRand+2));
                color = Color.Lerp(color, Color.gray, 0.5f);
                textureCache.Add(baseRand, SolidColorMaterials.NewSolidColorTexture(color));
            }
            
            // draw a box like RimWorld.AreaAllowedGUI.DoAreaSelector is doing it
            GUI.DrawTexture(rect, textureCache[baseRand] );
            Text.Anchor = TextAnchor.MiddleLeft;
            string str = foodRestriction.label;
            Rect rect1 = rect;
            rect1.xMin += 3f;
            rect1.yMin += 2f;
            Widgets.Label(rect1, str);
            if (pawn.foodRestriction.CurrentFoodRestriction == foodRestriction)
                Widgets.DrawBox(rect, 2);
            if (Event.current.rawType == EventType.MouseUp && Event.current.button == 0)
                dragging = false;
            if (!Input.GetMouseButton(0) && Event.current.type != EventType.MouseDown)
                dragging = false;
            if (Mouse.IsOver(rect))
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    dragging = true;
                if (dragging && pawn.foodRestriction.CurrentFoodRestriction != foodRestriction)
                {
                    pawn.foodRestriction.CurrentFoodRestriction = foodRestriction;
                    SoundDefOf.Designate_DragStandard_Changed_NoCam.PlayOneShotOnCamera();
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, (TipSignal) str);
        }

        public override int GetMinWidth(PawnTable table) => Mathf.Min(def.width, GetOptimalWidth(table));

        public override int GetOptimalWidth(PawnTable table)
        {
            var allFoodRestrictions = Current.Game.foodRestrictionDatabase.AllFoodRestrictions;
            var estimatedBoxSize = Text.CalcSize(allFoodRestrictions.Select(x => x.label).ToCommaList());
            return (int) Mathf.Max(estimatedBoxSize.x + 48, def.width);
        }

        protected override void HeaderClicked(Rect headerRect, PawnTable table)
        {
            base.HeaderClicked(headerRect, table);
            if (!Event.current.shift)
                return;

            if (Event.current.button == 0)
            {
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
            }
            else
            {
                if (Event.current.button != 1)
                    return;
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
            }
            
            var allFoodRestrictions = Current.Game.foodRestrictionDatabase.AllFoodRestrictions;
            
            var rectWidth = headerRect.width / allFoodRestrictions.Count;
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;

            for (var index = 0; index < allFoodRestrictions.Count; index++)
            {
                var foodRestriction = allFoodRestrictions[index];
                var offset = index * rectWidth;
                var rect = new Rect(headerRect.x + offset, headerRect.y, rectWidth, headerRect.height);
                if (Mouse.IsOver(rect) && Event.current.control && Event.current.shift && Event.current.button == 0)
                {
                    foreach (var pawn1 in table.PawnsListForReading.Where(pawn => pawn.foodRestriction != null && pawn.Faction == Faction.OfPlayer))
                    {
                        pawn1.foodRestriction.CurrentFoodRestriction = foodRestriction;
                    }
                    break;
                }
            }
            Text.WordWrap = true;
            Text.Font = GameFont.Small;
            
        }

        protected override string GetHeaderTip(PawnTable table)
        {
            return base.GetHeaderTip(table) 
                   + "\n" + "Shift + control + click: Set all pawns to zone";
        }
    }
}