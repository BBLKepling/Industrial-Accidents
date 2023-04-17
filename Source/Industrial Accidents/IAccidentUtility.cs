using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Industrial_Accidents
{
    public static class IAccidentUtility
    {
        //CanFireNowSub
        public static IEnumerable<Pawn> GetWorkingPawns(Map map)
        {
            List<Thing> workingPawns = map.listerThings.AllThings;
            for (int i = 0; i < workingPawns.Count; i++)
            {
                if (workingPawns[i] is Pawn)
                {
                    Pawn pawn = (Pawn)workingPawns[i];
                    if (pawn.IsColonist && pawn.jobs.curJob?.def == JobDefOf.DoBill)
                    {
                        Building building = (Building)pawn.jobs.curJob.GetTarget(TargetIndex.A);
                        if (building != null)
                        {
                            if ((pawn.Position - building.Position).ToVector3().MagnitudeHorizontal() < 3)
                            {
                                if (building.def.HasModExtension<IAccidentModExtension>())
                                {
                                    if (building.def.GetModExtension<IAccidentModExtension>().accidentType != null)
                                    {
                                        yield return pawn;
                                    }
                                }
                                if (pawn.jobs.curJob.RecipeDef != null)
                                {
                                    if (pawn.jobs.curJob.RecipeDef.HasModExtension<IAccidentModExtension>())
                                    {
                                        if (pawn.jobs.curJob.RecipeDef.GetModExtension<IAccidentModExtension>().accidentType != null)
                                        {
                                            yield return pawn;
                                        }
                                    }
                                    if (!pawn.jobs.curJob.RecipeDef.products.NullOrEmpty())
                                    {
                                        List<ThingDefCountClass> productList = pawn.jobs.curJob.RecipeDef.products;
                                        for (int q = 0; q < productList.Count; q++)
                                        {
                                            if (productList[q].thingDef.HasModExtension<IAccidentModExtension>())
                                            {
                                                if (productList[q].thingDef.GetModExtension<IAccidentModExtension>().accidentType != null)
                                                {
                                                    yield return pawn;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
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
            // determine accident type and complexity offset
            // RecipeDef>1stProduct>Building
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
                }
            }
            accType.ToLower();
            // Accidents
            if (accType == "industrial")
            {
                return IndustrialAccident(victim, complexOffset);
            }
            if (accType == "cooking")
            {
                //return CookingAccident(victim, complexOffset);
                Messages.Message("cooking", MessageTypeDefOf.NegativeEvent, false);
                return true;
            }
            // Error Reporting
            if (accType != "industrial" && accType != "cooking")
            {
                Log.Error("Industrial Accidents: Unsupported string in <accidentType> node");
                Log.Error("Industrial Accidents: Usable strings: industrial, cooking");
                if (building != null)
                {
                    if (building.def.HasModExtension<IAccidentModExtension>())
                    {
                        string errorBuilding = building.def.GetModExtension<IAccidentModExtension>().accidentType;
                        if (errorBuilding != null && errorBuilding != "industrial" && errorBuilding != "cooking")
                        {
                            Log.Error("Industrial Accidents: <defName>" + building.def.defName + "</defName> has <accidentType>" + errorBuilding + "</accidentType>");
                        }
                    }
                }
                if (productThingDef != null)
                {
                    if (productThingDef.HasModExtension<IAccidentModExtension>())
                    {
                        string errorThingDef = productThingDef.GetModExtension<IAccidentModExtension>().accidentType;
                        if (errorThingDef != null && errorThingDef != "industrial" && errorThingDef != "cooking")
                        {
                            Log.Error("Industrial Accidents: <defName>" + productThingDef.defName + "</defName> has <accidentType>" + errorThingDef + "</accidentType>");
                        }
                    }
                }
                if (recipe != null)
                {
                    if (recipe.HasModExtension<IAccidentModExtension>())
                    {
                        string errorRecipeDef = recipe.GetModExtension<IAccidentModExtension>().accidentType;
                        if (errorRecipeDef != null && errorRecipeDef != "industrial" && errorRecipeDef != "cooking")
                        {
                            Log.Error("Industrial Accidents: <defName>" + recipe.defName + "</defName> has <accidentType>" + errorRecipeDef + "</accidentType>");
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

        //Accident Methods
        public static bool IndustrialAccident(Pawn victim, int complexOffset)
        {
            int craftSkill = victim.skills.GetSkill(SkillDefOf.Crafting).levelInt;
            //int randChance = 30;
            int randChance = Rand.Range(1, 20) + complexOffset - craftSkill;
            List<BodyPartRecord> fingerParts = victim.def.race.body.GetPartsWithDef(ClassesDefOf.Finger);
            List<BodyPartRecord> handParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Hand);
            List<BodyPartRecord> armParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Arm);
            List<BodyPartRecord> eyeParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Eye);
            List<BodyPartRecord> targetParts = new List<BodyPartRecord>();
            targetParts.AddRange(fingerParts);
            targetParts.AddRange(handParts);
            targetParts.AddRange(armParts);
            targetParts.AddRange(eyeParts);
            HediffSet pawnParts = victim.health.hediffSet;
            List<BodyPartRecord> hasParts = new List<BodyPartRecord>();
            foreach (BodyPartRecord part in targetParts)
            {
                if (!pawnParts.PartIsMissing(part))
                {
                    hasParts.Add(part);
                }
            }
            if (hasParts.Any())
            {
                BodyPartRecord selectPart = hasParts.RandomElement();
                if (randChance > 20)
                {
                    Hediff hediffRemove = HediffMaker.MakeHediff(HediffDefOf.Shredded, victim);
                    hediffRemove.Severity = 50;
                    if (fingerParts.Contains(selectPart))
                    {
                        Messages.Message("Finger", MessageTypeDefOf.NegativeEvent, true);
                    }
                    if (handParts.Contains(selectPart))
                    {
                        Messages.Message("Hand", MessageTypeDefOf.NegativeEvent, true);
                    }
                    if (armParts.Contains(selectPart))
                    {
                        Messages.Message("Arm", MessageTypeDefOf.NegativeEvent, true);
                    }
                    if (eyeParts.Contains(selectPart))
                    {
                        Messages.Message("Eye", MessageTypeDefOf.NegativeEvent, true);
                    }
                    victim.health.AddHediff(hediffRemove, selectPart);
                    victim.jobs.StopAll();
                    return true;
                }
                if (randChance > 15)
                {
                    Hediff hediffCrush = HediffMaker.MakeHediff(ClassesDefOf.Crush, victim);
                    hediffCrush.Severity = Rand.Range(5f, randChance);
                    victim.health.AddHediff(hediffCrush, selectPart);
                    victim.jobs.StopAll();
                    return true;
                }
                if (randChance > 10)
                {
                    Hediff hediffCuts = HediffMaker.MakeHediff(HediffDefOf.Cut, victim);
                    hediffCuts.Severity = Rand.Range(5f, randChance);
                    victim.health.AddHediff(hediffCuts, selectPart);
                    victim.jobs.StopAll();
                    return true;
                }
                if (randChance > 5)
                {
                    Hediff hediffCrush = HediffMaker.MakeHediff(ClassesDefOf.Crush, victim);
                    hediffCrush.Severity = Rand.Range(1f, 5f);
                    victim.health.AddHediff(hediffCrush, selectPart);
                    victim.jobs.StopAll();
                    return true;
                }
                Hediff hediffCut = HediffMaker.MakeHediff(HediffDefOf.Cut, victim);
                hediffCut.Severity = Rand.Range(1f, 5f);
                victim.health.AddHediff(hediffCut, selectPart);
                victim.jobs.StopAll();
                return true;
            }
            return false;
        }
        public static bool CookingAccident(Pawn victim, int complexOffset)
        {
            return true;
        }
        public static bool ButcheryAccident(Pawn victim, int complexOffset)
        {
            return true;
        }
        public static bool MethLabAccident(Pawn victim, int complexOffset)
        {
            return true;
        }
        public static bool ChemicalAccident(Pawn victim, int complexOffset)
        {
            return true;
        }
        public static bool ChemfuelAccident(Pawn victim, int complexOffset)
        {
            return true;
        }
        public static bool SewingAccident(Pawn victim, int complexOffset)
        {
            return true;
        }
    }
}
