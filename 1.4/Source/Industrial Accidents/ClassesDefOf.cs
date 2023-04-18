using RimWorld;
using Verse;

namespace Industrial_Accidents
{
    [DefOf]
    public static class ClassesDefOf
    {
        public static HediffDef Crush;
        public static HediffDef ChemicalBurn;
        public static BodyPartDef Ear;
        public static BodyPartDef Tongue;
        public static BodyPartDef Finger;
        public static BodyPartDef Toe;
        public static BodyPartDef Foot;

        static ClassesDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ClassesDefOf));
        }
    }
}
