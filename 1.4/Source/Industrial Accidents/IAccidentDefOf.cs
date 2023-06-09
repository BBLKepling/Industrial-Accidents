﻿using RimWorld;
using Verse;

namespace Industrial_Accidents
{
    [DefOf]
    public static class IAccidentDefOf
    {
        public static HediffDef Crush;
        public static HediffDef ChemicalBurn;

        public static BodyPartDef Ear;
        public static BodyPartDef Finger;
        public static BodyPartDef Toe;
        public static BodyPartDef Foot;
        //public static BodyPartDef Tongue;

        [MayRequire("conit.thebirdsandthebees")]
        public static BodyPartDef ReproductiveOrgans;

        [MayRequire("DankPyon.Medieval.Overhaul")]
        public static ThingDef DankPyon_Bone;

        [MayRequire("sihv.rombones")]
        public static ThingDef BoneItem;

        [MayRequire("PeteTimesSix.ResearchReinvented")]
        public static JobDef RR_Analyse;
        static IAccidentDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IAccidentDefOf));
        }
    }
}
