using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Industrial_Accidents
{
    public static class IAccidents
    {
        //Job Accidents
        public static bool MiningAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Thing thing = (Thing)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Mining;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            List<BodyPartRecord> fingerParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Finger);
            List<BodyPartRecord> toeParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Toe);
            List<BodyPartRecord> footParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Foot);
            List<BodyPartRecord> handParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Hand);
            List<BodyPartRecord> armParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Arm);
            List<BodyPartRecord> eyeParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Eye);
            List<BodyPartRecord> targetParts = new List<BodyPartRecord>();
            targetParts.AddRange(fingerParts);
            targetParts.AddRange(handParts);
            targetParts.AddRange(armParts);
            targetParts.AddRange(toeParts);
            targetParts.AddRange(footParts);
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
                if (randChance > 5)
                {
                    if (fingerParts.Contains(selectPart) ||
                        handParts.Contains(selectPart) ||
                        footParts.Contains(selectPart) ||
                        toeParts.Contains(selectPart))
                    {
                        hediffCrush.Severity = Rand.Range(1f, 5f);
                        victim.health.AddHediff(hediffCrush, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(),
                            "BBLK_MineCrush".Translate(
                                victim.LabelShort, victim.Named("VICTIM"),
                                thing.Label, thing.Named("THING"),
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
                    "BBLK_MineCut".Translate(
                        victim.LabelShort, victim.Named("VICTIM"),
                        thing.Label, thing.Named("THING"),
                        part.label, part.Named("PART")),
                    LetterDefOf.NegativeEvent,
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            return false;
        }
        //Building Accidents
        public static bool IndustrialAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Crafting;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            if (randChance > 15 && IAccidentSettings.lethal)
            {
                Thing meat = GenSpawn.Spawn(ThingDefOf.Meat_Human, building.Position, building.Map);
                meat.stackCount = Rand.Range(30, 40);
                Thing meats = GenSpawn.Spawn(ThingDefOf.Meat_Human, victim.Position, victim.Map);
                meats.stackCount = Rand.Range(25, 35);
                FilthMaker.TryMakeFilth(building.Position, building.Map, ThingDefOf.Filth_Blood, 5);
                FilthMaker.TryMakeFilth(victim.Position, victim.Map, ThingDefOf.Filth_Blood, 5);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(),
                    "BBLK_IndSuck".Translate(
                        victim.LabelShort, victim.Named("VICTIM"),
                        building.Label, building.Named("BUILDING")),
                    LetterDefOf.NegativeEvent,
                    new TargetInfo(victim.Position, victim.Map));
                victim.Kill(null, null);
                victim.Corpse.DeSpawn();
                return true;
            }
            List<BodyPartRecord> fingerParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Finger);
            List<BodyPartRecord> toeParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Toe);
            List<BodyPartRecord> footParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Foot);
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
            targetParts.AddRange(toeParts);
            targetParts.AddRange(footParts);
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
                if (randChance > 10 && IAccidentSettings.catastrophic)
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
                if (randChance > 6)
                {
                    if (fingerParts.Contains(selectPart) ||
                        handParts.Contains(selectPart) ||
                        footParts.Contains(selectPart) ||
                        toeParts.Contains(selectPart) ||
                        junkParts.Contains(selectPart))
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
        public static bool MedievalAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Crafting;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            List<BodyPartRecord> fingerParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Finger);
            List<BodyPartRecord> toeParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Toe);
            List<BodyPartRecord> footParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Foot);
            List<BodyPartRecord> handParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Hand);
            List<BodyPartRecord> armParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Arm);
            List<BodyPartRecord> eyeParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Eye);
            List<BodyPartRecord> targetParts = new List<BodyPartRecord>();
            targetParts.AddRange(fingerParts);
            targetParts.AddRange(toeParts);
            targetParts.AddRange(footParts);
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
                Hediff hediffBurn = HediffMaker.MakeHediff(HediffDefOf.Burn, victim);
                Hediff hediffShred = HediffMaker.MakeHediff(HediffDefOf.Shredded, victim);
                if (randChance > 10 && IAccidentSettings.catastrophic)
                {
                    if (armParts.Contains(selectPart))
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
                if (randChance > 7)
                {
                    if (fingerParts.Contains(selectPart) ||
                        handParts.Contains(selectPart) ||
                        toeParts.Contains(selectPart) ||
                        footParts.Contains(selectPart))
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
                if (randChance > 5)
                {
                    if (fingerParts.Contains(selectPart) || handParts.Contains(selectPart))
                    {
                        hediffBurn.Severity = Rand.Range(1f, 5f);
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
        public static bool NeolithicAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Crafting;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            List<BodyPartRecord> fingerParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Finger);
            List<BodyPartRecord> toeParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Toe);
            List<BodyPartRecord> footParts = victim.def.race.body.GetPartsWithDef(IAccidentDefOf.Foot);
            List<BodyPartRecord> handParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Hand);
            List<BodyPartRecord> armParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Arm);
            List<BodyPartRecord> eyeParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Eye);
            List<BodyPartRecord> targetParts = new List<BodyPartRecord>();
            targetParts.AddRange(fingerParts);
            targetParts.AddRange(handParts);
            targetParts.AddRange(armParts);
            targetParts.AddRange(toeParts);
            targetParts.AddRange(footParts);
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
                if (randChance > 5)
                {
                    if (fingerParts.Contains(selectPart) ||
                        handParts.Contains(selectPart) ||
                        footParts.Contains(selectPart) ||
                        toeParts.Contains(selectPart))
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
        public static bool CookingAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Cooking;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            if (randChance > 15 && ModsConfig.BiotechActive && IAccidentSettings.catastrophic)
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
            if (randChance > 10 && IAccidentSettings.catastrophic)
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
                if (randChance > 5)
                {
                    if (fingerParts.Contains(selectPart) || handParts.Contains(selectPart))
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
        public static bool ButcheryAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Cooking;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
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
                Hediff hediffShred = HediffMaker.MakeHediff(HediffDefOf.Shredded, victim);
                Hediff hediffCut = HediffMaker.MakeHediff(HediffDefOf.Cut, victim);
                if (randChance > 10 && IAccidentSettings.catastrophic)
                {
                    if (armParts.Contains(selectPart))
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
        public static bool MechanoidAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Crafting;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            if (randChance > 10 && IAccidentSettings.catastrophic)
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
        public static bool MethLabAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Intellectual;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            if (randChance > 150 && IAccidentSettings.lethal)
            {
                GenExplosion.DoExplosion(
                    radius: 5.9f,
                    center: building.Position,
                    map: building.Map,
                    damType: DamageDefOf.Flame,
                    instigator: building);
                GenExplosion.DoExplosion(
                    radius: 3.9f,
                    center: building.Position,
                    map: building.Map,
                    damType: DamageDefOf.Bomb,
                    instigator: building,
                    damAmount: -1,
                    armorPenetration: -1f,
                    explosionSound: null,
                    weapon: null,
                    projectile: null,
                    intendedTarget: null,
                    postExplosionSpawnThingDef: ThingDefOf.Fire,
                    postExplosionSpawnChance: 0.25f,
                    postExplosionSpawnThingCount: 1);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(),
                    "BBLK_ExplosionMethLab".Translate(
                        victim.LabelShort, victim.Named("VICTIM"),
                        building.Label, building.Named("BUILDING")),
                    LetterDefOf.NegativeEvent,
                    new TargetInfo(victim.Position, victim.Map));
                victim.jobs.StopAll();
                return true;
            }
            if (randChance > 10 && ModsConfig.BiotechActive && IAccidentSettings.catastrophic)
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
        public static bool ChemicalAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Crafting;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            if (randChance > 15 && IAccidentSettings.lethal)
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
            if (randChance > 10 && ModsConfig.BiotechActive && IAccidentSettings.catastrophic)
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
        public static bool ChemfuelAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Crafting;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            if (randChance > 15 && IAccidentSettings.lethal)
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
        public static bool SewingAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Crafting;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
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
                Hediff hediffStab = HediffMaker.MakeHediff(HediffDefOf.Stab, victim);
                if (randChance > 10)
                {
                    if (fingerParts.Contains(selectPart))
                    {
                        hediffStab.Severity = Rand.Range(3f, 7f);
                        victim.health.AddHediff(hediffStab, selectPart);
                        Find.LetterStack.ReceiveLetter(
                            "BBLK_IAccidentLabel".Translate(),
                            "BBLK_IndStab".Translate(
                                victim.LabelShort, victim.Named("VICTIM"),
                                building.Label, building.Named("BUILDING"),
                                part.label, part.Named("PART")),
                            LetterDefOf.NegativeEvent,
                            new TargetInfo(victim.Position, victim.Map));
                        victim.jobs.StopAll();
                        return true;
                    }
                    hediffStab.Severity = Rand.Range(5f, randChance);
                    victim.health.AddHediff(hediffStab, selectPart);
                    Find.LetterStack.ReceiveLetter(
                        "BBLK_IAccidentLabel".Translate(),
                        "BBLK_IndStab".Translate(
                            victim.LabelShort, victim.Named("VICTIM"),
                            building.Label, building.Named("BUILDING"),
                            part.label, part.Named("PART")),
                        LetterDefOf.NegativeEvent,
                        new TargetInfo(victim.Position, victim.Map));
                    victim.jobs.StopAll();
                    return true;
                }
                hediffStab.Severity = Rand.Range(1f, 5f);
                victim.health.AddHediff(hediffStab, selectPart);
                Find.LetterStack.ReceiveLetter(
                    "BBLK_IAccidentLabel".Translate(),
                    "BBLK_IndStab".Translate(
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
        public static bool IndResearchAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Intellectual;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            if (randChance > 15 && IAccidentSettings.lethal)
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
            if (randChance > 10 && ModsConfig.BiotechActive && IAccidentSettings.catastrophic)
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
        public static bool SpaceResearchAccident(Pawn victim, float complexOffset, SkillDef skillDef)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            if (skillDef == null)
            {
                skillDef = SkillDefOf.Intellectual;
            }
            if (victim.skills.GetSkill(skillDef).levelInt > 20)
            {
                //hard cap at skill level 20
                complexOffset -= 2.0f;
            }
            else
            {
                complexOffset -= victim.skills.GetSkill(skillDef).levelInt / 10;
            }
            //float randChance = 30;
            float randChance = Rand.Range(3f, 10f) + Rand.Range(3f, 10f) + complexOffset;
            //6-20 bell curve(2d8+4) + complexity - manipulation(cap 2) - skill(cap 2)
            if (randChance > 15 && IAccidentSettings.lethal)
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
            if (randChance > 10 && ModsConfig.BiotechActive && IAccidentSettings.catastrophic)
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
    }
}
