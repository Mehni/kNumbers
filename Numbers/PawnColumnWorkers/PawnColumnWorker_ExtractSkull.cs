using RimWorld;
using System.Linq;
using Verse;

namespace Numbers
{
    // Credits to Marek15 for the original code
    public class PawnColumnWorker_ExtractSkull : PawnColumnWorker_Checkbox
    {
        private readonly Designator_ExtractSkull desSkull = new();

        protected override bool HasCheckbox(Pawn pawn)
        {
            if (!pawn.RaceProps.Humanlike || !pawn.Dead)
                return false;

            var corpse = pawn.Corpse;
            if (corpse == null || corpse.InnerPawn == null)
                return false;

            return corpse.InnerPawn.health.hediffSet.GetNotMissingParts()
                .Any(part => part.def == BodyPartDefOf.Head);
        }

        protected override bool GetValue(Pawn pawn)
        {
            if (pawn?.ParentHolder is not Thing thing)
                return false;

            var designations = thing.Map?.designationManager?.AllDesignations;
            if (designations == null)
                return false;

            return designations.Any(d => d.target == thing && d.def == DesignationDefOf.ExtractSkull);
        }

        protected override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            if (pawn?.ParentHolder is not Thing thing || thing.Map == null)
                return;

            if (value)
                desSkull.DesignateThing(thing);
            else
                thing.Map.designationManager.TryRemoveDesignationOn(thing, DesignationDefOf.ExtractSkull);
        }
    }
}
