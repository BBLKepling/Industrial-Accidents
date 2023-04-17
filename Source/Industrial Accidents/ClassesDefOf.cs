using RimWorld;
using Verse;

namespace Industrial_Accidents
{
    [DefOf]
    public static class ClassesDefOf
    {
        public static HediffDef Crush;
        public static HediffDef ChemicalBurn;
        public static BodyPartDef Finger;
        public static BodyPartDef Toe;

        static ClassesDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ClassesDefOf));
        }
    }
}
