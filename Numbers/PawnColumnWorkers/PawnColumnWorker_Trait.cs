using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_Trait : PawnColumnWorker_Text
    {
        private readonly bool _traitRarityColorsModIsActive = ModsConfig.IsActive("carnysenpai.traitraritycolors");

        protected override TextAnchor Anchor => TextAnchor.MiddleCenter;
        
        protected override string GetTextFor(Pawn pawn)
        {
            return pawn.story?.traits?.allTraits.Select(GetLabel).ToCommaList();
        }

        private string GetLabel(Trait trait)
        {
            return _traitRarityColorsModIsActive ? trait.CurrentData.label.CapitalizeFirst() : trait.Label;
        }
    }
}