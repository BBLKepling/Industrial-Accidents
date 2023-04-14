using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Industrial_Accidents
{
    public static class IAccidentUtility
    {
        public static IEnumerable<Pawn> GetWorkingPawns(Map map)
        {
            List<Thing> workingPawns = map.listerThings.AllThings;
            for (int i = 0; i < workingPawns.Count; i++)
            {
                if (workingPawns[i] is Pawn)
                {
                    Pawn pawn = (Pawn)workingPawns[i];
                    if (pawn.jobs.curJob?.def == JobDefOf.DoBill)
                    {
                        Building building = (Building)pawn.jobs.curJob.GetTarget(TargetIndex.A);
                        if (building.def.HasModExtension<IAccidentModExtension>() && (pawn.Position - building.Position).ToVector3().MagnitudeHorizontal() < 3)
                        {
                            yield return pawn;
                        }
                    }
                }
            }
        }
        public static bool TryHurtPawn(Pawn victim)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            string accType = (string)building.def.GetModExtension<IAccidentModExtension>().accidentType;
            //RecipeDef recipe = (RecipeDef)victim.jobs.curJob.RecipeDef;
            if (accType == "industrial")
            {
                //int craftSkill = (int)victim.skills.GetSkill(SkillDefOf.Crafting).levelInt;
                //int recipeSkill = (int)recipe.skillRequirements
                return true;
            }
            if (accType == "chemical")
            {
                //int intellSkill = (int)victim.skills.GetSkill(SkillDefOf.Intellectual).levelInt;
                GenExplosion.DoExplosion(radius: 2.9f, center: building.Position, map: building.Map, damType: DamageDefOf.Flame, instigator: building);
                return true;
            }
            if (accType != "industrial" && accType != "chemical")
            {
                Log.Error("Industrial Accidents: Unsupported string in <accidentType> node");
                Log.Error("Industrial Accidents: Usable strings: industrial, chemical");
                return false;
            }
            return false;
        }
    }
}
