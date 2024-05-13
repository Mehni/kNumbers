namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using RimWorld;
    using Verse;

    public class PawnTable_NumbersMain : PawnTable
    {
        private List<PawnColumnDef> _originalColumns;

        public PawnTable_NumbersMain(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight) : base(def, pawnsGetter, uiWidth, uiHeight)
        {
            PawnTableDef = def;
            _originalColumns = def.columns;

            SetMinMaxSize(def.minWidth, uiWidth, 0, (int)(uiHeight * Numbers_Settings.maxHeight));
            SetDirty();
        }

        public PawnTableDef PawnTableDef { get; protected set; }

        Queue<Predicate<PawnColumnDef>> filtersToApply = new();

        protected override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
        {
            if (PawnTableDef == NumbersDefOf.Numbers_MainTable)
            {
                return PlayerPawnsDisplayOrderUtility.InOrder(input);
            }
            return base.LabelSortFunction(input);
        }

        public int RemoveColumns(Predicate<PawnColumnDef> predicate)
        {
            return PawnTableDef.columns.RemoveAll(predicate);
        }

        // Postponing column removal until PawnTable drawing is finished
        // Immediate removal causes change of column indexes in middle of drawing process
        // it makes vanilla game to render nonsense for a single update
        // and also causes support issues for Grouped Pawn Tables mod (caches mismatch)
        public void EnqueueColumnRemoval(Predicate<PawnColumnDef> predicate)
        {
            filtersToApply.Enqueue(predicate);
        }

        public void ApplyColumnRemoval()
        {
            while (filtersToApply.Count > 0)
            {
                RemoveColumns(filtersToApply.Dequeue());
            }
        }
    }
}
