using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace Industrial_Accidents
{
    internal class CodeDump
    {
        public static bool TryHurtPawn(Pawn victim)
        {
            Building building = (Building)victim.jobs.curJob.GetTarget(TargetIndex.A);
            RecipeDef recipe = victim.jobs.curJob.RecipeDef;
            int complexOffset = building.def.GetModExtension<IAccidentModExtension>().complexity;
            string accType = building.def.GetModExtension<IAccidentModExtension>().accidentType;
            accType.ToLower();

            if (accType == "industrial")
            {
                int craftSkill = (victim.skills.GetSkill(SkillDefOf.Crafting).levelInt / 2);
                int craftReq = GetSkillReq(recipe, SkillDefOf.Crafting);
                int randChance = 30;
                //int randChance = Rand.Range(1, 20) + complexOffset + craftReq - craftSkill;
                List<BodyPartRecord> targetParts = new List<BodyPartRecord>();
                List<BodyPartRecord> handParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Hand);
                List<BodyPartRecord> armParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Arm);
                List<BodyPartRecord> eyeParts = victim.def.race.body.GetPartsWithDef(BodyPartDefOf.Eye);
                targetParts.AddRange(handParts);
                targetParts.AddRange(armParts);
                targetParts.AddRange(eyeParts);
                HediffSet pawnParts = victim.health.hediffSet;
                IEnumerable<BodyPartRecord> hasParts = CompareBodyParts(targetParts, pawnParts);
                BodyPartRecord selectPart = hasParts.RandomElement();
                if (hasParts.Any())
                {
                    if (randChance > 20)
                    {
                        Hediff hediffRemove = HediffMaker.MakeHediff(HediffDefOf.Shredded, victim);
                        hediffRemove.Severity = 50;
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
                        Hediff hediffCut = HediffMaker.MakeHediff(ClassesDefOf.Crush, victim);
                        hediffCut.Severity = Rand.Range(5f, randChance);
                        victim.health.AddHediff(hediffCut, selectPart);
                        return true;
                    }
                    if (randChance > 10)
                    {
                        Hediff hediffCut = HediffMaker.MakeHediff(HediffDefOf.Cut, victim);
                        hediffCut.Severity = Rand.Range(5f, randChance);
                        victim.health.AddHediff(hediffCut, selectPart);
                        return true;
                    }
                    if (randChance > 5)
                    {
                        Hediff hediffCut = HediffMaker.MakeHediff(ClassesDefOf.Crush, victim);
                        hediffCut.Severity = Rand.Range(1f, 5f);
                        victim.health.AddHediff(hediffCut, selectPart);
                        return true;
                    }
                    Hediff hediffCrush = HediffMaker.MakeHediff(HediffDefOf.Cut, victim);
                    hediffCrush.Severity = Rand.Range(1f, 5f);
                    victim.health.AddHediff(hediffCrush, selectPart);
                    return true;
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
                int intellReq = GetSkillReq(recipe, SkillDefOf.Intellectual);
                int randChance = 0;
                //int randChance = (Rand.Range(1, 20) + complexOffset + intellReq - intellSkill);
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
                List<BodyPartRecord> allParts = victim.def.race.body.AllParts;
                HediffSet pawnParts = victim.health.hediffSet;
                IEnumerable<BodyPartRecord> hasParts = CompareBodyParts(allParts, pawnParts);
                if (hasParts.Any())
                {
                    Hediff hediffHurt = HediffMaker.MakeHediff(ClassesDefOf.ChemicalBurn, victim);
                    hediffHurt.Severity = (randChance - 3);
                    victim.health.AddHediff(hediffHurt, hasParts.RandomElement());
                    return true;
                }
                return false;
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
    }
}
