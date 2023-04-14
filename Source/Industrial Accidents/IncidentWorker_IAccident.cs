using RimWorld;
using System.Linq;
using Verse;

namespace Industrial_Accidents
{
    internal class IncidentWorker_IAccident : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return IAccidentUtility.GetWorkingPawns((Map)parms.target).Any();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!IAccidentUtility.GetWorkingPawns((Map)parms.target).TryRandomElement(out var result))
            {
                return false;
            }
            return IAccidentUtility.TryHurtPawn(result);
        }
    }
}
