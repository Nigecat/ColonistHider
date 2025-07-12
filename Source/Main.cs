using HarmonyLib;
using Verse;

namespace ColonistHider
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            new Harmony("Nigecat.ColonistHider").PatchAll();
        }
    }
}
