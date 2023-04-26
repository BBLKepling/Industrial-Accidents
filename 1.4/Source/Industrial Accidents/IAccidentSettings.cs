using UnityEngine;
using Verse;

namespace Industrial_Accidents
{
    public class IAccidentSettings : ModSettings
    {
        public static bool lethal;
        public static bool catastrophic;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref lethal, "lethal");
            Scribe_Values.Look(ref catastrophic, "catastrophic");
            base.ExposeData();
        }
    }
    public class IAccidentMod : Mod
    {
        public IAccidentMod(ModContentPack content) : base(content)
        {
            GetSettings<IAccidentSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("BBLK_LethalExplanation".Translate(), ref IAccidentSettings.lethal, "BBLK_LethalToolTip".Translate());
            listingStandard.CheckboxLabeled("BBLK_CatastrophicExplanation".Translate(), ref IAccidentSettings.catastrophic, "BBLK_CatastrophicToolTip".Translate());
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory() => "BBLK_IAccidents_Settings".Translate();
    }
}
