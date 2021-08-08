using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_HediffIsBad : PawnColumnWorker_AllHediffs
    {
        protected override IEnumerable<Hediff> VisibleHediffs(Pawn pawn)
        {
            return base.VisibleHediffs(pawn).Where(x => x.def.isBad);
        }
    }
}
