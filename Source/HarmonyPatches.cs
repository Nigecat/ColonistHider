using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonistHider
{
    [HarmonyPatch(typeof(PlaySettings))]
    [HarmonyPatch("DoMapControls")]
    public static class PlaySettings_DoMapControls
    {
        public static void Postfix(WidgetRow row)
        {
            Config config = Config.Current;

            bool disabled = config.Disabled;
            row.ToggleableIcon(ref disabled, TexButton.ShowColonistBar, "Toggle hidden colonists.\n\nDisplays all hidden colonists in the colonist bar.");
            config.Disabled = disabled;
        }
    }

    // ------------------------------------------------------------------------------------------------------

    [HarmonyPatch(typeof(ColonistBarColonistDrawer))]
    [HarmonyPatch(nameof(ColonistBarColonistDrawer.HandleClicks))]
    public static class ColonistBarColonistDrawer_HandleClicks
    {
        public static void Prefix(Rect rect, Pawn colonist)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
            {
                Config config = Config.Current;

                FloatMenuOption option = config.IsHidden(colonist)
                    ? new FloatMenuOption("Show", () => config.Show(colonist))
                    : new FloatMenuOption("Hide", () => config.Hide(colonist));

                Find.WindowStack.Add(new FloatMenu(new() { option }));
            }
        }
    }

    // ------------------------------------------------------------------------------------------------------

    [HarmonyPatch(typeof(ColonistBar))]
    [HarmonyPatch(nameof(ColonistBar.ColonistBarOnGUI))]
    public static class ColonistBar_ColonistBarOnGUI
    {
        private static bool Dirty = true;

        public static void Prefix(ColonistBar __instance)
        {
            if (Dirty)
            {
                __instance.MarkColonistsDirty();
                __instance.GetType().GetTypeInfo().GetDeclaredMethod("CheckRecacheEntries").Invoke(__instance, new object[] { });
                Dirty = false;
            }
        }

        public static void MarkDirty()
        {
            Dirty = true;
        }
    }

    // ------------------------------------------------------------------------------------------------------

    [HarmonyPatch(typeof(ColonistBar))]
    [HarmonyPatch("CheckRecacheEntries")]
    public static class ColonistBar_CheckRecacheEntries
    {
        public static void Prefix(bool ___entriesDirty, out bool __state)
        {
            __state = ___entriesDirty;
        }

        public static void Postfix(
            List<ColonistBar.Entry> ___cachedEntries,
            List<Vector2> ___cachedDrawLocs,
            ref float ___cachedScale,
            ColonistBarColonistDrawer ___drawer,
            ColonistBarDrawLocsFinder ___drawLocsFinder,
            bool __state
        )
        {
            Config config = Config.Current;

            if (__state && !config.Disabled)
            {
                ___cachedEntries.RemoveWhere((entry) => config.IsHidden(entry.pawn));
                ___drawer.Notify_RecachedEntries();
                ___drawLocsFinder.CalculateDrawLocs(___cachedDrawLocs, out ___cachedScale, ___cachedEntries.Select((entry) => entry.group).Max() + 1);
            }
        }
    }
}