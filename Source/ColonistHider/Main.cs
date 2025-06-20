using Verse;
using HarmonyLib;

namespace ColonistHider
{
    public class Mod : Verse.Mod
    {
        public override string SettingsCategory() => "Colonist Hider";

        public Mod(ModContentPack content) : base(content)
        {
            new Harmony("Nigecat.ColonistHider").PatchAll();
        }
    }
}
