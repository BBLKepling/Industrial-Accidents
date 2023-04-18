using RimWorld;
using System.Collections.Generic;
using System.Security.AccessControl;
using Verse;
using Verse.AI;

namespace Industrial_Accidents
{
    public static class IAccidentUtilityOld
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
            string accType = building.def.GetModExtension<IAccidentModExtension>().accidentType;
            accType.ToLower();

            if (accType == "industrial")
            {
                return IndustrialTree(victim, building);
            }

            if (accType == "cooking")
            {
                return CookingTree(victim, building);
            }

            if (accType == "chemical")
            {
                return ChemicalTree(victim, building);
            }

            if (accType != "industrial" && accType != "cooking" && accType != "chemical")
            {
                Log.Error("Industrial Accidents: Unsupported string in <accidentType> node");
                Log.Error("Industrial Accidents: Usable strings: industrial, cooking, chemical");
                return false;
            }
            Log.Error("Industrial Accidents: If you're seeing this something went wrong as all the checks to prevent you from seeing this failed.");
            return false;
        }
        private static bool IndustrialTree(Pawn victim, Building building)
        {
            RecipeDef recipe = victim.jobs.curJob.RecipeDef;
            string accSubType = GetSubIAcc(recipe, "industrial");
            if (accSubType == "industrialgeneric")
            {
                return IndustrialGenericBranch(victim, building);
            }
            if (accSubType != "industrialgeneric")
            {
                Log.Error("Industrial Accidents: Unsupported string in <accidentType> node");
                Log.Error("Industrial Accidents: Usable strings: industrial, cooking, chemical");
                return false;
            }
            return false;
        }
        private static bool IndustrialGenericBranch(Pawn victim, Building building)
        {
            return false;
        }
        private static bool CookingTree(Pawn victim, Building building)
        {
            RecipeDef recipe = victim.jobs.curJob.RecipeDef;
            if(CheckRecipeProducts(recipe, ThingDefOf.SmokeleafJoint))
            {
                return CookingSmokeleafBranch(victim, building);
            }
            return CookingGenericBranch(victim, building);
        }
        private static bool CookingSmokeleafBranch(Pawn victim, Building building)
        {
            return false;
        }
        private static bool CookingGenericBranch(Pawn victim, Building building)
        {
            return false;
        }
        private static bool ChemicalTree(Pawn victim, Building building)
        {
            return false;
        }
        private static bool CheckRecipeProducts(RecipeDef recipeDef, ThingDef thingDef)
        {
            if (recipeDef != null)
            {
                if (!recipeDef.products.NullOrEmpty())
                {
                    List<ThingDefCountClass> thingList = recipeDef.products;
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i].thingDef == thingDef)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static string GetSubIAcc(RecipeDef recipeDef, string accType)
        {
            if (recipeDef != null)
            {
                if (!recipeDef.products.NullOrEmpty())
                {
                    List<ThingDefCountClass> productList = recipeDef.products;
                    for (int i = 0; i < productList.Count; i++)
                    {
                        if (productList[i].thingDef.HasModExtension<IAccidentSubModExtension>())
                        {
                            if (productList[i].thingDef.GetModExtension<IAccidentSubModExtension>().accidentSubType != null)
                            {
                                return productList[i].thingDef.GetModExtension<IAccidentSubModExtension>().accidentSubType;
                            }
                        }
                    }
                }
            }
            return accType + "generic";
        }
        private static IEnumerable<BodyPartRecord> CompareBodyParts(List<BodyPartRecord> bodyPartRecord, HediffSet hediffSet)
        {
            foreach (BodyPartRecord part in bodyPartRecord)
            {
                if (!hediffSet.PartIsMissing(part))
                {
                    yield return part;
                }
            }
        }
        private static int GetSkillReq(RecipeDef recipeDef, SkillDef skillDef)
        {
            if (recipeDef != null)
            {
                if (!recipeDef.skillRequirements.NullOrEmpty())
                {
                    List<SkillRequirement> skillReq = recipeDef.skillRequirements;
                    for (int i = 0; i < skillReq.Count; i++)
                    {
                        if (skillReq[i].skill == skillDef)
                        {
                            return skillReq[i].minLevel;
                        }
                    }
                }
            }
            return 0;
        }
    }
}
