using RimWorld;
using Verse;

namespace ColonistHider
{
    [DefOf]
    public static class KeyBindings
    {
        public static KeyBindingDef? Toggle;

        static KeyBindings()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(KeyBindingCategoryDefOf));
        }
    }
}
