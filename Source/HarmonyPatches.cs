using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonistHider
{
    /// <summary>
    /// Patch for the map control widgets.
    /// This adds an extra widget with a toggle for the <c>Config.Disabled</c> property.
    /// </summary>
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

    /// <summary>
    /// Patch for the colonist bar event handler.
    /// This adds a "Show" right-click button if a colonist is hidden, and a "Hide" button if they are not.
    /// </summary>
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

    /// <summary>
    /// Patch for the colonist bar draw cache.
    /// This allows the <c>Config</c> class to request a redraw/recache of the colonist bar when necessary.
    /// </summary>
    [HarmonyPatch(typeof(ColonistBar))]
    [HarmonyPatch(nameof(ColonistBar.ColonistBarOnGUI))]
    public static class ColonistBar_ColonistBarOnGUI
    {
        /// <summary>
        /// Whether the colonist bar is dirty.
        /// Setting this to <c>true</c> will cause it to be recached then reset to <c>false</c> the next update.
        /// </summary>
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

    /// <summary>
    /// Patch for colonist bar entry cacher.
    /// This will take into account our hidden pawns and filter them out of the display list.
    /// </summary>
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

    // ------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Patch for the colonist bar drawer.
    /// This will add an indicator to all hidden colonists when they are visible (through <c>Config.Disabled</c>).
    /// </summary>
    [HarmonyPatch(typeof(ColonistBarColonistDrawer))]
    [HarmonyPatch(nameof(ColonistBarColonistDrawer.DrawColonist))]
    public static class ColonistBarColonistDrawer_DrawColonist
    {
        public static void Postfix(Rect rect, Pawn colonist)
        {
            Config config = Config.Current;

            if (config.Disabled && config.IsHidden(colonist))
            {
                Widgets.DrawBoxSolid(rect, Color.gray.ToTransparent(0.4f));
            }
        }
    }
}
