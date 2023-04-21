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

        [MayRequire("PeteTimesSix.ResearchReinvented")]
        public static JobDef RR_Research;
        static IAccidentDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IAccidentDefOf));
        }
    }
}
