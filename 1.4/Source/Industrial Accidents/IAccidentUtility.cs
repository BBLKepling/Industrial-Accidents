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
                if (ModLister.HasActiveModWithName("Research Reinvented"))
                {
                    if (!(pawns[i].jobs.curJob?.def == JobDefOf.DoBill) && !(pawns[i].jobs.curJob?.def == JobDefOf.Research) && !(pawns[i].jobs.curJob?.def == IAccidentDefOf.RR_Research)) { continue; }
                }
                else
                {
                    if (!(pawns[i].jobs.curJob?.def == JobDefOf.DoBill) && !(pawns[i].jobs.curJob?.def == JobDefOf.Research)) { continue; }
                }
                Building building = (Building)pawns[i].jobs.curJob.GetTarget(TargetIndex.A);
                if (building == null) { continue; }
                if ((pawns[i].Position - building.Position).ToVector3().MagnitudeHorizontal() > 3) { continue; }
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
            RecipeDef recipe = victim.jobs.curJob.RecipeDef;
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            // pull Mod Extension info
            // RecipeDef>1stProduct>Building
            SkillDef skillOverride = null;
            ThingDef productThingDef = null;
            string accType = null;
            int complexOffset = 0;
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
            //int manipOffset = (int)victim.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
            accType = accType.ToLower();
            // Accidents
            switch (accType)
            {
                case "industrial":
                    return IAccidents.IndustrialAccident(victim, complexOffset, skillOverride);
                case "medieval":
                    return IAccidents.MedievalAccident(victim, complexOffset, skillOverride);
                case "neolithic":
                    return IAccidents.NeolithicAccident(victim, complexOffset, skillOverride);
                case "cooking":
                    return IAccidents.CookingAccident(victim, complexOffset, skillOverride);
                case "butchery":
                    return IAccidents.ButcheryAccident(victim, complexOffset, skillOverride);
                case "mechanoid":
                    return IAccidents.MechanoidAccident(victim, complexOffset, skillOverride);
                case "methlab":
                    return IAccidents.MethLabAccident(victim, complexOffset, skillOverride);
                case "chemical":
                    return IAccidents.ChemicalAccident(victim, complexOffset, skillOverride);
                case "chemfuel":
                    return IAccidents.ChemfuelAccident(victim, complexOffset, skillOverride);
                case "sewing":
                    return IAccidents.SewingAccident(victim, complexOffset, skillOverride);
                case "indresearch":
                    return IAccidents.IndResearchAccident(victim, complexOffset, skillOverride);
                case "spaceresearch":
                    return IAccidents.SpaceResearchAccident(victim, complexOffset, skillOverride);
            }
            // Error Reporting
            if (
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
                Log.Error("Industrial Accidents: Usable strings: industrial, medieval, neolithic, cooking, butchery, mechanoid, methlab, chemical, chemfuel, sewing, indresearch, spaceresearch");
                if (building != null)
                {
                    if (building.def.HasModExtension<IAccidentModExtension>())
                    {
                        string errorBuilding = building.def.GetModExtension<IAccidentModExtension>().accidentType;
                        if (errorBuilding != null &&
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
