using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_Trait : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            return pawn.story?.traits?.allTraits.Select(x => x.Label).ToCommaList();
        }
    }
}
