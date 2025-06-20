using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonistHider
{
    [HarmonyPatch(typeof(ColonistBarColonistDrawer))]
    [HarmonyPatch(nameof(ColonistBarColonistDrawer.HandleClicks))]
    public static class ColonistBarColonistDrawer_HandleClicks
    {
        public static void Prefix(Rect rect, Pawn colonist)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
            {
                Find.WindowStack.Add(new FloatMenu(
                    new()
                    {
                        new FloatMenuOption("Hide", () => Config.Current.Hide(colonist))
                    }
                ));
            }
        }
    }
}
