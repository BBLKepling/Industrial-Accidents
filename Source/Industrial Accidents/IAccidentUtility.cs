using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
        private static IEnumerable<BodyPartRecord> compareBodyParts(List<BodyPartRecord> bodyPartRecord, HediffSet hediffSet)
        {
            foreach (BodyPartRecord part in bodyPartRecord)
            {
                if (!hediffSet.PartIsMissing(part))
                {
                    yield return part;
                }
            }
        }
        private static int getSkillReq(RecipeDef recipeDef, SkillDef skillDef)
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
        public static bool TryHurtPawn(Pawn victim)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            RecipeDef recipe = (RecipeDef)victim.jobs.curJob.RecipeDef;
            string accType = building.def.GetModExtension<IAccidentModExtension>().accidentType;
            int complexOffset = building.def.GetModExtension<IAccidentModExtension>().complexity;

            if (accType == "industrial")
            {
                int craftSkill = (victim.skills.GetSkill(SkillDefOf.Crafting).levelInt / 2);
                int craftReq = getSkillReq(recipe, SkillDefOf.Crafting);
                int randChance = (Rand.Range(1, 20) + complexOffset + craftReq  - craftSkill);
                List<BodyPartRecord> handParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Hand);
                handParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Arm));
                HediffSet pawnParts = victim.health.hediffSet;
                IEnumerable<BodyPartRecord> hasParts = compareBodyParts(handParts, pawnParts);
                if (hasParts.Any())
                {
                    if (randChance > 20)
                    {
                        Hediff hediffHurt = HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, victim);
                        hediffHurt.Severity = Rand.Range(1f, 5f);
                        victim.health.AddHediff(hediffHurt, hasParts.RandomElement());
                        return true;
                    }
                    if (randChance > 10)
                    {
                        Hediff hediffHurt = HediffMaker.MakeHediff(HediffDefOf.Cut, victim);
                        hediffHurt.Severity = Rand.Range(1f, 5f);
                        victim.health.AddHediff(hediffHurt, hasParts.RandomElement());
                        return true;
                    }
                }
                return false;
            }

            if (accType == "cooking")
            {
                //int cookSkill = (victim.skills.GetSkill(SkillDefOf.Cooking).levelInt / 2);
                //int randChance = (Rand.Range(1, 20) + complexOffset - cookSkill);
                victim.TryAttachFire(1f);
                return true;
            }

            if (accType == "chemical")
            {
                int intellSkill = (victim.skills.GetSkill(SkillDefOf.Intellectual).levelInt / 2);
                int randChance = (Rand.Range(1,20) + complexOffset - intellSkill);
                if (randChance > 20)
                {
                    GenExplosion.DoExplosion(radius: 2.9f, center: building.Position, map: building.Map, damType: DamageDefOf.Bomb, instigator: building);
                    return true;
                }
                if (randChance > 15)
                {
                    GenExplosion.DoExplosion(radius: 3.9f, center: building.Position, map: building.Map, damType: DamageDefOf.Flame, instigator: building);
                    return true;
                }
                if (randChance > 10)
                {
                    GenExplosion.DoExplosion(radius: 5.9f, center: building.Position, map: building.Map, damType: DamageDefOf.Smoke, instigator: building, damAmount: -1, armorPenetration: -1, explosionSound: null, weapon: null, projectile: null, intendedTarget: null, postExplosionSpawnThingDef: ThingDefOf.Filth_Ash, postExplosionSpawnChance: 1f, postExplosionSpawnThingCount: 3, postExplosionGasType: GasType.BlindSmoke);
                    return true;
                }
                if (randChance > 5)
                {
                    List<BodyPartRecord> handParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Hand);
                    handParts.AddRange(victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Arm));
                    HediffSet pawnParts = victim.health.hediffSet;
                    IEnumerable<BodyPartRecord> hasParts = compareBodyParts(handParts, pawnParts);
                    if (hasParts.Any())
                    {
                        Hediff hediffHurt = HediffMaker.MakeHediff(ClassesDefOf.ChemicalBurn, victim);
                        hediffHurt.Severity = (randChance - 3);
                        victim.health.AddHediff(hediffHurt, hasParts.RandomElement());
                        return true;
                    }
                }
                return false;
            }

            if (accType != "industrial" && accType != "cooking" && accType != "chemical")
            {
                Log.Error("Industrial Accidents: Unsupported string in <accidentType> node");
                Log.Error("Industrial Accidents: Usable strings: industrial, cooking, chemical");
                return false;
            }
            return false;
        }
    }
}
