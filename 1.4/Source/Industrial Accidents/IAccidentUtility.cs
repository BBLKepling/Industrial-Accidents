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
            accType = accType.ToLower();
            // Accidents
            if (accType == "industrial")
            {
                return IndustrialAccident(victim, complexOffset, skillOverride);
            }
            if (accType == "medieval")
            {
                //return MedievalAccident(victim, complexOffset, skillOverride);
            }
            if (accType == "neolithic")
            {
                return NeolithicAccident(victim, complexOffset, skillOverride);
            }
            if (accType == "cooking")
            {
                return CookingAccident(victim, complexOffset, skillOverride);
            }
            if (accType == "butchery")
            {
                //return ButcheryAccident(victim, complexOffset, skillOverride);
            }
            if (accType == "mechanoid")
            {
                return MechanoidAccident(victim, complexOffset, skillOverride);
            }
            if (accType == "methlab")
            {
                return MethLabAccident(victim, complexOffset, skillOverride);
            }
            if (accType == "chemical")
            {
                //return ChemicalAccident(victim, complexOffset, skillOverride);
            }
            if (accType == "chemfuel")
            {
                //return ChemfuelAccident(victim, complexOffset, skillOverride);
            }
            if (accType == "sewing")
            {
                //return SewingAccident(victim, complexOffset, skillOverride);
            }
            // Error Reporting
            if (accType != "industrial" && accType != "neolithic" && accType != "cooking" && accType != "mechanoid" && accType != "methlab")
            {
                Log.Error("Industrial Accidents: Unsupported string in <accidentType> node");
                Log.Error("Industrial Accidents: Usable strings: industrial, neolithic, cooking, mechanoid, methlab");
                if (building != null)
                {
                    if (building.def.HasModExtension<IAccidentModExtension>())
                    {
                        string errorBuilding = building.def.GetModExtension<IAccidentModExtension>().accidentType;
                        if (errorBuilding != null && errorBuilding != "industrial" && errorBuilding != "neolithic" && errorBuilding != "cooking" && errorBuilding != "mechanoid" && errorBuilding != "methlab")
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
                        if (errorThingDef != null && errorThingDef != "industrial" && errorThingDef != "neolithic" && errorThingDef != "cooking" && errorThingDef != "mechanoid" && errorThingDef != "methlab")
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
                        if (errorRecipeDef != null && errorRecipeDef != "industrial" && errorRecipeDef != "neolithic" && errorRecipeDef != "cooking" && errorRecipeDef != "mechanoid" && errorRecipeDef != "methlab")
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

        //Accidents
        public static bool IndustrialAccident(Pawn victim, int complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Crafting;
            }
            int craftSkill = victim.skills.GetSkill(skillDef).levelInt;
            //int randChance = 30;
            int randChance = Rand.Range(1, 20) + complexOffset - craftSkill;
            List<BodyPartRecord> fingerParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Finger);
            List<BodyPartRecord> handParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Hand);
            List<BodyPartRecord> armParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Arm);
            List<BodyPartRecord> eyeParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Eye);
            List<BodyPartRecord> junkParts = new List<BodyPartRecord>();
            if (ModLister.HasActiveModWithName("(UNOFFICIAL 1.4) The Birds and the Bees"))
            {
                junkParts.AddRange(victim.def.race.body.GetPartsWithDef(IAccidentDefOf.ReproductiveOrgans));
            }
            List<BodyPartRecord> targetParts = new List<BodyPartRecord>();
            targetParts.AddRange(fingerParts);
            targetParts.AddRange(handParts);
            targetParts.AddRange(armParts);
            targetParts.AddRange(eyeParts);
            if (victim.gender == Gender.Male)
            {
                targetParts.AddRange(junkParts);
            }
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
                BodyPartDef part = selectPart.def;
                Hediff hediffShred = HediffMaker.MakeHediff(HediffDefOf.Shredded, victim);
                Hediff hediffCrush = HediffMaker.MakeHediff(IAccidentDefOf.Crush, victim);
                Hediff hediffCut = HediffMaker.MakeHediff(HediffDefOf.Cut, victim);
                if (randChance > 20)
                {
                    if (armParts.Contains(selectPart) || junkParts.Contains(selectPart))
                    {
                        hediffShred.Severity = 50;
                        victim.health.AddHediff(hediffShred, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_IndCataRip".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING"), 
                                part.label, part.Named("PART")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                    if (eyeParts.Contains(selectPart))
                    {
                        hediffShred.Severity = 50;
                        victim.health.AddHediff(hediffShred, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_IndCataEye".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                    hediffCut.Severity = 50;
                    victim.health.AddHediff(hediffCut, selectPart);
                    Find.LetterStack.ReceiveLetter(
                        "BBLK_IAccidentLabel".Translate(), 
                        "BBLK_IndCataCut".Translate(
                            victim.LabelShort, victim.Named("VICTIM"), 
                            building.Label, building.Named("BUILDING"), 
                            part.label, part.Named("PART")), 
                        LetterDefOf.NegativeEvent, 
                        new TargetInfo(victim.Position, victim.Map));
                    victim.jobs.StopAll();
                    return true;
                }
                if (randChance > 15)
                {
                    if (fingerParts.Contains(selectPart) || junkParts.Contains(selectPart))
                    {
                        hediffCrush.Severity = Rand.Range(3f, 7f);
                        victim.health.AddHediff(hediffCrush, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_IndCrush".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING"), 
                                part.label, part.Named("PART")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                    if (handParts.Contains(selectPart))
                    {
                        hediffCrush.Severity = Rand.Range(3f, randChance - 5);
                        victim.health.AddHediff(hediffCrush, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_IndCrush".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING"), 
                                part.label, part.Named("PART")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                }
                if (randChance > 10)
                {
                    if (fingerParts.Contains(selectPart) || junkParts.Contains(selectPart))
                    {
                        hediffCut.Severity = Rand.Range(2f, 6f);
                        victim.health.AddHediff(hediffCut, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_IndCut".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING"), 
                                part.label, part.Named("PART")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                    hediffCut.Severity = Rand.Range(5f, randChance);
                    victim.health.AddHediff(hediffCut, selectPart);
                    Find.LetterStack.ReceiveLetter(
                        "BBLK_IAccidentLabel".Translate(), 
                        "BBLK_IndCut".Translate(
                            victim.LabelShort, victim.Named("VICTIM"), 
                            building.Label, building.Named("BUILDING"), 
                            part.label, part.Named("PART")), 
                        LetterDefOf.NegativeEvent, 
                        new TargetInfo(victim.Position, victim.Map));
                    victim.jobs.StopAll();
                    return true;
                }
                if (randChance > 5)
                {
                    if (fingerParts.Contains(selectPart) || handParts.Contains(selectPart) || junkParts.Contains(selectPart))
                    {
                        hediffCrush.Severity = Rand.Range(1f, 5f);
                        victim.health.AddHediff(hediffCrush, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_IndCrush".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING"), 
                                part.label, part.Named("PART")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                }
                hediffCut.Severity = Rand.Range(1f, 5f);
                victim.health.AddHediff(hediffCut, selectPart);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(), 
                    "BBLK_IndCut".Translate(
                        victim.LabelShort, victim.Named("VICTIM"), 
                        building.Label, building.Named("BUILDING"), 
                        part.label, part.Named("PART")), 
                    LetterDefOf.NegativeEvent, 
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            return false;
        }
        public static bool MedievalAccident(Pawn victim, int complexOffset, SkillDef skillDef)
        {
            return false;
        }
        public static bool NeolithicAccident(Pawn victim, int complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Crafting;
            }
            int craftSkill = victim.skills.GetSkill(skillDef).levelInt;
            //int randChance = 30;
            int randChance = Rand.Range(1, 20) + complexOffset - craftSkill;
            List<BodyPartRecord> fingerParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Finger);
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
                BodyPartDef part = selectPart.def;
                Hediff hediffCrush = HediffMaker.MakeHediff(IAccidentDefOf.Crush, victim);
                Hediff hediffCut = HediffMaker.MakeHediff(HediffDefOf.Cut, victim);
                if (randChance > 15)
                {
                    if (fingerParts.Contains(selectPart))
                    {
                        hediffCrush.Severity = Rand.Range(3f, 7f);
                        victim.health.AddHediff(hediffCrush, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_IndCrush".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING"), 
                                part.label, part.Named("PART")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                    if (handParts.Contains(selectPart))
                    {
                        hediffCrush.Severity = Rand.Range(3f, randChance - 5);
                        victim.health.AddHediff(hediffCrush, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_IndCrush".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING"), 
                                part.label, part.Named("PART")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                }
                if (randChance > 10)
                {
                    if (fingerParts.Contains(selectPart))
                    {
                        hediffCut.Severity = Rand.Range(3f, 7f);
                        victim.health.AddHediff(hediffCut, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_IndCut".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING"), 
                                part.label, part.Named("PART")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                    hediffCut.Severity = Rand.Range(5f, randChance);
                    victim.health.AddHediff(hediffCut, selectPart);
                    Find.LetterStack.ReceiveLetter(
                        "BBLK_IAccidentLabel".Translate(), 
                        "BBLK_IndCut".Translate(
                            victim.LabelShort, victim.Named("VICTIM"), 
                            building.Label, building.Named("BUILDING"), 
                            part.label, part.Named("PART")), 
                        LetterDefOf.NegativeEvent, 
                        new TargetInfo(victim.Position, victim.Map));
                    victim.jobs.StopAll();
                    return true;
                }
                if (randChance > 5)
                {
                    if (fingerParts.Contains(selectPart) || handParts.Contains(selectPart))
                    {
                        hediffCrush.Severity = Rand.Range(1f, 5f);
                        victim.health.AddHediff(hediffCrush, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_IndCrush".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING"), 
                                part.label, part.Named("PART")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                }
                hediffCut.Severity = Rand.Range(1f, 5f);
                victim.health.AddHediff(hediffCut, selectPart);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(), 
                    "BBLK_IndCut".Translate(
                        victim.LabelShort, victim.Named("VICTIM"), 
                        building.Label, building.Named("BUILDING"), 
                        part.label, part.Named("PART")), 
                    LetterDefOf.NegativeEvent, 
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            return false;
        }
        public static bool CookingAccident(Pawn victim, int complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Cooking;
            }
            int craftSkill = victim.skills.GetSkill(skillDef).levelInt;
            //int randChance = 18;
            int randChance = Rand.Range(1, 20) + complexOffset - craftSkill;
            if (randChance > 20)
            {
                GenExplosion.DoExplosion(
                    radius: 1.9f, 
                    center: building.Position, 
                    map: building.Map, 
                    damType: DamageDefOf.Flame, 
                    instigator: building);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(), 
                    "BBLK_GreaseFire".Translate(
                        victim.LabelShort, victim.Named("VICTIM"), 
                        building.Label, building.Named("BUILDING")), 
                    LetterDefOf.NegativeEvent, 
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            if (randChance > 15 && ModsConfig.BiotechActive)
            {
                GenExplosion.DoExplosion(
                    radius: 5.9f, 
                    center: building.Position, 
                    map: building.Map, 
                    damType: DamageDefOf.ToxGas, 
                    instigator: building, 
                    damAmount: -1, 
                    armorPenetration: -1f, 
                    explosionSound: null, 
                    weapon: null, 
                    projectile: null, 
                    intendedTarget: null, 
                    postExplosionSpawnThingDef: null, 
                    postExplosionSpawnChance: 0f, 
                    postExplosionSpawnThingCount: 1, 
                    postExplosionGasType: GasType.ToxGas);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(), 
                    "BBLK_ExplosionSpice".Translate(
                        victim.LabelShort, victim.Named("VICTIM"), 
                        building.Label, building.Named("BUILDING")), 
                    LetterDefOf.NegativeEvent, 
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            List<BodyPartRecord> fingerParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Finger);
            //List<BodyPartRecord> tongueParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Tongue);
            List<BodyPartRecord> handParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Hand);
            List<BodyPartRecord> armParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Arm);
            List<BodyPartRecord> eyeParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Eye);
            List<BodyPartRecord> targetParts = new List<BodyPartRecord>();
            targetParts.AddRange(fingerParts);
            //targetParts.AddRange(tongueParts);
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
                BodyPartDef part = selectPart.def;
                Hediff hediffBurn = HediffMaker.MakeHediff(HediffDefOf.Burn, victim);
                Hediff hediffCut = HediffMaker.MakeHediff(HediffDefOf.Cut, victim);
                if (randChance > 10)
                {
                    if (fingerParts.Contains(selectPart))
                    {
                        hediffBurn.Severity = Rand.Range(3f, 7f);
                        victim.health.AddHediff(hediffBurn, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(), 
                            "BBLK_CookBurn".Translate(
                                victim.LabelShort, victim.Named("VICTIM"), 
                                building.Label, building.Named("BUILDING"), 
                                part.label, part.Named("PART")), 
                            LetterDefOf.NegativeEvent, 
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                    hediffBurn.Severity = Rand.Range(5f, randChance);
                    victim.health.AddHediff(hediffBurn, selectPart);
                    Find.LetterStack.ReceiveLetter(
                        "BBLK_IAccidentLabel".Translate(), 
                        "BBLK_CookBurn".Translate(
                            victim.LabelShort, victim.Named("VICTIM"), 
                            building.Label, building.Named("BUILDING"), 
                            part.label, part.Named("PART")), 
                        LetterDefOf.NegativeEvent, 
                        new TargetInfo(victim.Position, victim.Map));
                    victim.jobs.StopAll();
                    return true;
                }
                hediffCut.Severity = Rand.Range(1f, 5f);
                victim.health.AddHediff(hediffCut, selectPart);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(), 
                    "BBLK_IndCut".Translate(
                        victim.LabelShort, victim.Named("VICTIM"), 
                        building.Label, building.Named("BUILDING"), 
                        part.label, part.Named("PART")), 
                    LetterDefOf.NegativeEvent, 
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            return false;
        }
        public static bool ButcheryAccident(Pawn victim, int complexOffset, SkillDef skillDef)
        {
            return false;
        }
        public static bool MechanoidAccident(Pawn victim, int complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Crafting;
            }
            int craftSkill = victim.skills.GetSkill(skillDef).levelInt;
            //int randChance = 30;
            int randChance = Rand.Range(1, 20) + complexOffset - craftSkill;
            if (randChance > 20)
            {
                GenExplosion.DoExplosion(
                    radius: 5.9f,
                    center: building.Position,
                    map: building.Map,
                    damType: DamageDefOf.EMP,
                    instigator: building);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(),
                    "BBLK_ExplosionMech".Translate(
                        victim.LabelShort, victim.Named("VICTIM"),
                        building.Label, building.Named("BUILDING")),
                    LetterDefOf.NegativeEvent,
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            List<BodyPartRecord> fingerParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Finger);
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
                BodyPartDef part = selectPart.def;
                Hediff hediffCrush = HediffMaker.MakeHediff(IAccidentDefOf.Crush, victim);
                Hediff hediffCut = HediffMaker.MakeHediff(HediffDefOf.Cut, victim);
                if (randChance > 15)
                {
                    if (fingerParts.Contains(selectPart))
                    {
                        hediffCrush.Severity = Rand.Range(3f, 7f);
                        victim.health.AddHediff(hediffCrush, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(),
                            "BBLK_IndCrush".Translate(
                                victim.LabelShort, victim.Named("VICTIM"),
                                building.Label, building.Named("BUILDING"),
                                part.label, part.Named("PART")),
                            LetterDefOf.NegativeEvent,
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                    if (handParts.Contains(selectPart))
                    {
                        hediffCrush.Severity = Rand.Range(3f, randChance - 5);
                        victim.health.AddHediff(hediffCrush, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(),
                            "BBLK_IndCrush".Translate(
                                victim.LabelShort, victim.Named("VICTIM"),
                                building.Label, building.Named("BUILDING"),
                                part.label, part.Named("PART")),
                            LetterDefOf.NegativeEvent,
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                }
                if (randChance > 10)
                {
                    if (fingerParts.Contains(selectPart))
                    {
                        hediffCut.Severity = Rand.Range(3f, 7f);
                        victim.health.AddHediff(hediffCut, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(),
                            "BBLK_IndCut".Translate(
                                victim.LabelShort, victim.Named("VICTIM"),
                                building.Label, building.Named("BUILDING"),
                                part.label, part.Named("PART")),
                            LetterDefOf.NegativeEvent,
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                    hediffCut.Severity = Rand.Range(5f, randChance);
                    victim.health.AddHediff(hediffCut, selectPart);
                    Find.LetterStack.ReceiveLetter(
                        "BBLK_IAccidentLabel".Translate(),
                        "BBLK_IndCut".Translate(
                            victim.LabelShort, victim.Named("VICTIM"),
                            building.Label, building.Named("BUILDING"),
                            part.label, part.Named("PART")),
                        LetterDefOf.NegativeEvent,
                        new TargetInfo(victim.Position, victim.Map));
                    victim.jobs.StopAll();
                    return true;
                }
                if (randChance > 5)
                {
                    if (fingerParts.Contains(selectPart) || handParts.Contains(selectPart))
                    {
                        hediffCrush.Severity = Rand.Range(1f, 5f);
                        victim.health.AddHediff(hediffCrush, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(),
                            "BBLK_IndCrush".Translate(
                                victim.LabelShort, victim.Named("VICTIM"),
                                building.Label, building.Named("BUILDING"),
                                part.label, part.Named("PART")),
                            LetterDefOf.NegativeEvent,
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                }
                hediffCut.Severity = Rand.Range(1f, 5f);
                victim.health.AddHediff(hediffCut, selectPart);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(),
                    "BBLK_IndCut".Translate(
                        victim.LabelShort, victim.Named("VICTIM"),
                        building.Label, building.Named("BUILDING"),
                        part.label, part.Named("PART")),
                    LetterDefOf.NegativeEvent,
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            return false;
        }
        public static bool MethLabAccident(Pawn victim, int complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Intellectual;
            }
            int craftSkill = victim.skills.GetSkill(skillDef).levelInt;
            //int randChance = 30;
            int randChance = Rand.Range(1, 20) + complexOffset - craftSkill;
            if (randChance > 20)
            {
                GenExplosion.DoExplosion(
                    radius: 3.9f, 
                    center: building.Position, 
                    map: building.Map, 
                    damType: DamageDefOf.Bomb, 
                    instigator: building);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(), 
                    "BBLK_ExplosionBomb".Translate(
                        victim.LabelShort, victim.Named("VICTIM"), 
                        building.Label, building.Named("BUILDING")), 
                    LetterDefOf.NegativeEvent, 
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            if (randChance > 15 && ModsConfig.BiotechActive)
            {
                GenExplosion.DoExplosion(
                    radius: 5.9f, 
                    center: building.Position, 
                    map: building.Map, 
                    damType: DamageDefOf.ToxGas, 
                    instigator: building, 
                    damAmount: -1, 
                    armorPenetration: -1f, 
                    explosionSound: null, 
                    weapon: null, 
                    projectile: null, 
                    intendedTarget: null, 
                    postExplosionSpawnThingDef: null, 
                    postExplosionSpawnChance: 0f, 
                    postExplosionSpawnThingCount: 1, 
                    postExplosionGasType: GasType.ToxGas);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(), 
                    "BBLK_ExplosionToxic".Translate(
                        victim.LabelShort, victim.Named("VICTIM"), 
                        building.Label, building.Named("BUILDING")), 
                    LetterDefOf.NegativeEvent, 
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            if (randChance > 10)
            {
                GenExplosion.DoExplosion(
                    radius: 5.9f, 
                    center: building.Position, 
                    map: building.Map, 
                    damType: DamageDefOf.Smoke, 
                    instigator: building, 
                    damAmount: -1, 
                    armorPenetration: -1, 
                    explosionSound: null, 
                    weapon: null, 
                    projectile: null, 
                    intendedTarget: null, 
                    postExplosionSpawnThingDef: ThingDefOf.Filth_Ash, 
                    postExplosionSpawnChance: 0.5f, 
                    postExplosionSpawnThingCount: 1, 
                    postExplosionGasType: GasType.BlindSmoke);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(), 
                    "BBLK_ExplosionSmoke".Translate(
                        victim.LabelShort, victim.Named("VICTIM"), 
                        building.Label, building.Named("BUILDING")), 
                    LetterDefOf.NegativeEvent, 
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            if (randChance > 5)
            {
                GenExplosion.DoExplosion(
                    radius: 5.9f,
                    center: building.Position,
                    map: building.Map,
                    damType: DamageDefOf.Smoke,
                    instigator: building,
                    damAmount: -1,
                    armorPenetration: -1,
                    explosionSound: null,
                    weapon: null,
                    projectile: null,
                    intendedTarget: null,
                    postExplosionSpawnThingDef: ThingDefOf.Filth_Ash,
                    postExplosionSpawnChance: 0.5f,
                    postExplosionSpawnThingCount: 1,
                    postExplosionGasType: GasType.BlindSmoke);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(),
                    "BBLK_ExplosionSmoke".Translate(
                        victim.LabelShort, victim.Named("VICTIM"),
                        building.Label, building.Named("BUILDING")),
                    LetterDefOf.NegativeEvent,
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            List<BodyPartRecord> targetParts = new List<BodyPartRecord>();
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Finger));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Toe));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Foot));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Ear));
            //targetParts.AddRange(victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Tongue));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Hand));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Arm));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Eye));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Head));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Jaw));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Leg));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Nose));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Torso));
            targetParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Neck));
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
                BodyPartDef part = selectPart.def;
                Hediff hediffChemBurn = HediffMaker.MakeHediff(IAccidentDefOf.ChemicalBurn, victim);
                hediffChemBurn.Severity = Rand.Range(1f, 7f);
                victim.health.AddHediff(hediffChemBurn, selectPart);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(), 
                    "BBLK_ChemBurn".Translate(
                        victim.LabelShort, victim.Named("VICTIM"), 
                        building.Label, building.Named("BUILDING"), 
                        part.label, part.Named("PART")), 
                    LetterDefOf.NegativeEvent, 
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            return false;
        }
        public static bool ChemicalAccident(Pawn victim, int complexOffset, SkillDef skillDef)
        {
            return false;
        }
        public static bool ChemfuelAccident(Pawn victim, int complexOffset, SkillDef skillDef)
        {
            return false;
        }
        public static bool SewingAccident(Pawn victim, int complexOffset, SkillDef skillDef)
        {
            return false;
        }
    }
}
