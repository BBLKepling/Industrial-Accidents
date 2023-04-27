using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Industrial_Accidents
{
    public static class IAccidentUtility
    {
        //CanFireNowSub
        public static IEnumerable<Pawn> GetWorkingPawns(Map map)
        {
            List<Pawn> pawns = map.mapPawns.AllPawns;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (!pawns[i].IsColonist && !pawns[i].IsSlaveOfColony && !pawns[i].IsColonyMech) { continue; }
                if (pawns[i].jobs.curJob == null) { continue; }
                if (!pawns[i].jobs.curJob.def.HasModExtension<IAccidentModExtension>()) { continue; }
                if ((pawns[i].Position - pawns[i].jobs.curJob.targetA.Cell).ToVector3().MagnitudeHorizontal() > 3) { continue; }
                if (ModLister.HasActiveModWithName("Research Reinvented"))
                {
                    if (pawns[i].jobs.curJob.def == IAccidentDefOf.RR_Analyse)
                    {
                        if ((pawns[i].Position - pawns[i].jobs.curJob.GetTarget(TargetIndex.B).Cell).ToVector3().MagnitudeHorizontal() > 3) { continue; }
                    }
                }
                if (pawns[i].jobs.curJob.def.GetModExtension<IAccidentModExtension>().accidentType != null)
                {
                    yield return pawns[i];
                }
                if (!(pawns[i].jobs.curJob.GetTarget(TargetIndex.A).Thing is Building)) { continue; }
                Building building = (Building)pawns[i].jobs.curJob.GetTarget(TargetIndex.A);
                if (building == null) { continue; }
                if (building.def.HasModExtension<IAccidentModExtension>())
                {
                    if (building.def.GetModExtension<IAccidentModExtension>().accidentType != null)
                    {
                        yield return pawns[i];
                    }
                }
                if (pawns[i].jobs.curJob.RecipeDef == null) { continue; }
                if (pawns[i].jobs.curJob.RecipeDef.HasModExtension<IAccidentModExtension>())
                {
                    if (pawns[i].jobs.curJob.RecipeDef.GetModExtension<IAccidentModExtension>().accidentType != null)
                    {
                        yield return pawns[i];
                    }
                }
                if (pawns[i].jobs.curJob.RecipeDef.products.NullOrEmpty()) { continue; }
                List<ThingDefCountClass> productList = pawns[i].jobs.curJob.RecipeDef.products;
                for (int q = 0; q < productList.Count; q++)
                {
                    if (productList[q].thingDef.HasModExtension<IAccidentModExtension>())
                    {
                        if (productList[q].thingDef.GetModExtension<IAccidentModExtension>().accidentType != null)
                        {
                            yield return pawns[i];
                        }
                    }
                }
            }
        }

        //TryExecuteWorker
        public static bool TryHurtPawn(Pawn victim)
        {
            // pull Mod Extension info
            // RecipeDef>1stProduct>Building>JobDef
            SkillDef skillOverride = null;
            string accType = null;
            float complexOffset = 0f;
            if (victim.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation) > 2.0f)
            {
                //hard cap at 200%
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
            }
            if (victim.jobs.curJob.def.HasModExtension<IAccidentModExtension>())
            {
                complexOffset += victim.jobs.curJob.def.GetModExtension<IAccidentModExtension>().complexity;
                if (victim.jobs.curJob.def.GetModExtension<IAccidentModExtension>().accidentType != null)
                {
                    accType = victim.jobs.curJob.def.GetModExtension<IAccidentModExtension>().accidentType;
                }
                if (victim.jobs.curJob.def.GetModExtension<IAccidentModExtension>().skillDef != null)
                {
                    skillOverride = victim.jobs.curJob.def.GetModExtension<IAccidentModExtension>().skillDef;
                }
            }
            if (accType != null)
            {
                accType = accType.ToLower();
            }
            // Job Accidents
            // These don't involve buildings so we short cuircit here
            switch (accType)
            {
                case "mining":
                    return IAccidents.MiningAccident(victim, complexOffset, skillOverride);
                case "analyseinplace":
                    return IAccidents.AnalyseInPlaceAccident(victim, complexOffset, skillOverride);
                case "analyseterrain":
                    return IAccidents.AnalyseTerrainAccident(victim, complexOffset, skillOverride);
            }
            Building building = null;
            if (ModLister.HasActiveModWithName("Research Reinvented"))
            {
                if (victim.jobs.curJob.def == IAccidentDefOf.RR_Analyse)
                {
                    building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.B);
                }
            }
            if (building == null)
            {
                building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            }
            if (building != null)
            {
                if (building.def.HasModExtension<IAccidentModExtension>())
                {
                    complexOffset += building.def.GetModExtension<IAccidentModExtension>().complexity;
                    if (building.def.GetModExtension<IAccidentModExtension>().accidentType != null)
                    {
                        accType = building.def.GetModExtension<IAccidentModExtension>().accidentType;
                    }
                    if (building.def.GetModExtension<IAccidentModExtension>().skillDef != null)
                    {
                        skillOverride = building.def.GetModExtension<IAccidentModExtension>().skillDef;
                    }
                }
            }
            RecipeDef recipe = victim.jobs.curJob.RecipeDef;
            ThingDef productThingDef = null;
            if (recipe != null)
            {
                if (!recipe.products.NullOrEmpty())
                {
                    List<ThingDefCountClass> productList = recipe.products;
                    //since the last over writes the first we reverse to make the first last
                    productList.Reverse();
                    for (int i = 0; i < productList.Count; i++)
                    {
                        if (productList[i].thingDef.HasModExtension<IAccidentModExtension>())
                        {
                            if (productList[i].thingDef.GetModExtension<IAccidentModExtension>().accidentType != null)
                            {
                                accType = productList[i].thingDef.GetModExtension<IAccidentModExtension>().accidentType;
                                productThingDef = productList[i].thingDef;
                            }
                            if (productList[i].thingDef.GetModExtension<IAccidentModExtension>().skillDef != null)
                            {
                                skillOverride = productList[i].thingDef.GetModExtension<IAccidentModExtension>().skillDef;
                            }
                        }
                    }
                    if (productThingDef != null)
                    {
                        complexOffset += productThingDef.GetModExtension<IAccidentModExtension>().complexity;
                    }
                }
                if (recipe.HasModExtension<IAccidentModExtension>())
                {
                    complexOffset += recipe.GetModExtension<IAccidentModExtension>().complexity;
                    if (recipe.GetModExtension<IAccidentModExtension>().accidentType != null)
                    {
                        accType = recipe.GetModExtension<IAccidentModExtension>().accidentType;
                    }
                    if (recipe.GetModExtension<IAccidentModExtension>().skillDef != null)
                    {
                        skillOverride = recipe.GetModExtension<IAccidentModExtension>().skillDef;
                    }
                }
            }
            accType = accType.ToLower();
            // Building Accidents
            switch (accType)
            {
                case "industrial":
                    return IAccidents.IndustrialAccident(victim, complexOffset, skillOverride, building);
                case "medieval":
                    return IAccidents.MedievalAccident(victim, complexOffset, skillOverride, building);
                case "neolithic":
                    return IAccidents.NeolithicAccident(victim, complexOffset, skillOverride, building);
                case "cooking":
                    return IAccidents.CookingAccident(victim, complexOffset, skillOverride, building);
                case "butchery":
                    return IAccidents.ButcheryAccident(victim, complexOffset, skillOverride, building);
                case "mechanoid":
                    return IAccidents.MechanoidAccident(victim, complexOffset, skillOverride, building);
                case "methlab":
                    return IAccidents.MethLabAccident(victim, complexOffset, skillOverride, building);
                case "chemical":
                    return IAccidents.ChemicalAccident(victim, complexOffset, skillOverride, building);
                case "chemfuel":
                    return IAccidents.ChemfuelAccident(victim, complexOffset, skillOverride, building);
                case "sewing":
                    return IAccidents.SewingAccident(victim, complexOffset, skillOverride, building);
                case "indresearch":
                    return IAccidents.IndResearchAccident(victim, complexOffset, skillOverride, building);
                case "neoresearch":
                    return IAccidents.NeoResearchAccident(victim, complexOffset, skillOverride, building);
                case "spaceresearch":
                    return IAccidents.SpaceResearchAccident(victim, complexOffset, skillOverride, building);
            }
            // Error Reporting
            if (
                accType != "mining" &&
                accType != "industrial" &&
                accType != "medieval" &&
                accType != "neolithic" &&
                accType != "cooking" &&
                accType != "butchery" &&
                accType != "mechanoid" &&
                accType != "methlab" &&
                accType != "chemical" &&
                accType != "chemfuel" &&
                accType != "sewing" &&
                accType != "indresearch" &&
                accType != "spaceresearch")
            {
                Log.Error("Industrial Accidents: Unsupported string in <accidentType> node");
                Log.Error("Industrial Accidents: Usable Job strings: mining");
                Log.Error("Industrial Accidents: Usable Building strings: industrial, medieval, neolithic, cooking, butchery, mechanoid, methlab, chemical, chemfuel, sewing, indresearch, spaceresearch");
                if (victim.jobs.curJob.def.HasModExtension<IAccidentModExtension>())
                {
                    string errorJob = victim.jobs.curJob.def.GetModExtension<IAccidentModExtension>().accidentType;
                    if (errorJob != null &&
                        errorJob != "mining" &&
                        errorJob != "industrial" &&
                        errorJob != "medieval" &&
                        errorJob != "neolithic" &&
                        errorJob != "cooking" &&
                        errorJob != "butchery" &&
                        errorJob != "mechanoid" &&
                        errorJob != "methlab" &&
                        errorJob != "chemical" &&
                        errorJob != "chemfuel" &&
                        errorJob != "sewing" &&
                        errorJob != "indresearch" &&
                        errorJob != "spaceresearch")
                    {
                        Log.Error("Industrial Accidents: JobDef <defName>" + victim.jobs.curJob.def.defName + "</defName> has <accidentType>" + errorJob + "</accidentType>");
                    }
                }
                if (building != null)
                {
                    if (building.def.HasModExtension<IAccidentModExtension>())
                    {
                        string errorBuilding = building.def.GetModExtension<IAccidentModExtension>().accidentType;
                        if (errorBuilding != null &&
                            errorBuilding != "mining" &&
                            errorBuilding != "industrial" &&
                            errorBuilding != "medieval" &&
                            errorBuilding != "neolithic" &&
                            errorBuilding != "cooking" &&
                            errorBuilding != "butchery" &&
                            errorBuilding != "mechanoid" &&
                            errorBuilding != "methlab" &&
                            errorBuilding != "chemical" &&
                            errorBuilding != "chemfuel" &&
                            errorBuilding != "sewing" &&
                            errorBuilding != "indresearch" &&
                            errorBuilding != "spaceresearch")
                        {
                            Log.Error("Industrial Accidents: Building <defName>" + building.def.defName + "</defName> has <accidentType>" + errorBuilding + "</accidentType>");
                        }
                    }
                }
                if (productThingDef != null)
                {
                    if (productThingDef.HasModExtension<IAccidentModExtension>())
                    {
                        string errorThingDef = productThingDef.GetModExtension<IAccidentModExtension>().accidentType;
                        if (errorThingDef != null &&
                            errorThingDef != "mining" &&
                            errorThingDef != "industrial" &&
                            errorThingDef != "medieval" &&
                            errorThingDef != "neolithic" &&
                            errorThingDef != "cooking" &&
                            errorThingDef != "butchery" &&
                            errorThingDef != "mechanoid" &&
                            errorThingDef != "methlab" &&
                            errorThingDef != "chemical" &&
                            errorThingDef != "chemfuel" &&
                            errorThingDef != "sewing" &&
                            errorThingDef != "indresearch" &&
                            errorThingDef != "spaceresearch")
                        {
                            Log.Error("Industrial Accidents: ThingDef <defName>" + productThingDef.defName + "</defName> has <accidentType>" + errorThingDef + "</accidentType>");
                        }
                    }
                }
                if (recipe != null)
                {
                    if (recipe.HasModExtension<IAccidentModExtension>())
                    {
                        string errorRecipeDef = recipe.GetModExtension<IAccidentModExtension>().accidentType;
                        if (errorRecipeDef != null &&
                            errorRecipeDef != "mining" &&
                            errorRecipeDef != "industrial" &&
                            errorRecipeDef != "medieval" &&
                            errorRecipeDef != "neolithic" &&
                            errorRecipeDef != "cooking" &&
                            errorRecipeDef != "butchery" &&
                            errorRecipeDef != "mechanoid" &&
                            errorRecipeDef != "methlab" &&
                            errorRecipeDef != "chemical" &&
                            errorRecipeDef != "chemfuel" &&
                            errorRecipeDef != "sewing" &&
                            errorRecipeDef != "indresearch" &&
                            errorRecipeDef != "spaceresearch")
                        {
                            Log.Error("Industrial Accidents: RecipeDef <defName>" + recipe.defName + "</defName> has <accidentType>" + errorRecipeDef + "</accidentType>");
                        }
                    }
                }
                return false;
            }
            if (accType == null)
            {
                Log.Error("Industrial Accidents: Variable for accType somehow returned null");
                return false;
            }
            Log.Error("Industrial Accidents: If you're seeing this something went wrong as all the checks to prevent you from seeing this failed.");
            return false;
        }
    }
}
