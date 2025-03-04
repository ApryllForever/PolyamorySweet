
namespace PolyamorySweetLove
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool BuyPendantsAnytime { get; set; } = false;
        public int PendantPrice { get; set; } = 5000;
        public int MinPointsToMarry { get; set; } = 2500;
        public int MinPointsToDate { get; set; } = 2000;
        public bool PreventHostileDivorces { get; set; } = false;
        public bool ComplexDivorce { get; set; } = true;
        public bool RoommateRomance { get; set; } = false;
        public bool RomanceAllVillagers { get; set; } = false;
        public bool GayPregnancies { get; set; } = true;
        public bool ImpregnatingMother { get; set; } = true;
        public bool ImpregnatingFemmeNPC { get; set; } = false;
        public int MaxChildren { get; set; } = 2;
        public bool ShowParentNames { get; set; } = true;
        public string SpouseSleepOrder { get; set; } = "";
        public float PercentChanceForSpouseInBed { get; set; } = .25f;
        public int PercentChanceForSpouseInKitchen { get; set; } = 25;
        public int PercentChanceForSpouseAtPatio { get; set; } = 25;

        public float PercentChanceForSpouseAtPorch { get; set; } = 0.37f;
        public float PercentChanceForBirthingQuestion { get; set; } = 0.05f;
        public float PercentChanceForBirthSex { get; set; } = 0.6f;

        public bool WinterPatio { get; set; } = true;
        public string SpouseOrder { get; set; } = "";

        //public bool RemoveSpouseOrdinaryDialogue { get; set; } = false;
    }
}