using RimWorld;
using Verse;

namespace Industrial_Accidents
{
    [DefOf]
    public static class ClassesDefOf
    {
        public static HediffDef Crush;
        public static HediffDef ChemicalBurn;

        static ClassesDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ClassesDefOf));
        }
    }
}
