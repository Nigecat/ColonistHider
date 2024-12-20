using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ColonistHider
{
    [HarmonyPatch(typeof(ColonistBarColonistDrawer))]
    [HarmonyPatch(nameof(ColonistBarColonistDrawer.DrawColonist))]
    public static class ColonistBarColonistDrawer_DrawColonist
    {
        public static void Prefix(Rect rect, Pawn colonist)
        {
            if (Mouse.IsOver(rect) && Input.GetMouseButtonDown(1))
            {
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>
                {
                    new FloatMenuOption("Hide", () =>
                    {
                        Find.World.GetComponent<Config>().AddToBlacklist(colonist);
                        ColonistBar_ColonistBarOnGUI.MarkDirty();
                    })
                }));
            }
        }
    }

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

    [HarmonyPatch(typeof(ColonistBar))]
    [HarmonyPatch("CheckRecacheEntries")]
    public static class ColonistBar_CheckRecacheEntries
    {
        private static List<Pawn> Purge(List<Pawn> pawns)
        {
            Config config = Find.World.GetComponent<Config>();
            return pawns.Where(pawn => !config.IsPawnBlacklisted(pawn)).ToList();
        }

        public static bool Prefix(ColonistBar __instance, ref bool ___entriesDirty, ref List<ColonistBar.Entry> ___cachedEntries, ref List<Map> ___tmpMaps, ref List<Pawn> ___tmpPawns, ref List<Caravan> ___tmpCaravans, ref List<int> ___cachedReorderableGroups, ref ColonistBarDrawLocsFinder ___drawLocsFinder, ref List<Vector2> ___cachedDrawLocs, ref float ___cachedScale)
        {
            if (!___entriesDirty)
            {
                return false;
            }
            ___entriesDirty = false;
            ___cachedEntries.Clear();
            int num = 0;
            if (Find.PlaySettings.showColonistBar)
            {
                ___tmpMaps.Clear();
                ___tmpMaps.AddRange(Find.Maps);
                ___tmpMaps.SortBy((Map x) => !x.IsPlayerHome, (Map x) => x.uniqueID);
                for (int i = 0; i < ___tmpMaps.Count; i++)
                {
                    ___tmpPawns.Clear();



                    // ======================= MODIFICATIONS BEGIN HERE =======================
                    // ___tmpPawns.AddRange(___tmpMaps[i].mapPawns.FreeColonists);
                    ___tmpPawns.AddRange(Purge(___tmpMaps[i].mapPawns.FreeColonists));
                    // ======================= MODIFICATIONS  END  HERE =======================



                    ___tmpPawns.AddRange(___tmpMaps[i].mapPawns.ColonyMutantsPlayerControlled);
                    List<Thing> list = ___tmpMaps[i].listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (!list[j].IsDessicated())
                        {
                            Pawn innerPawn = ((Corpse)list[j]).InnerPawn;
                            if (innerPawn != null && innerPawn.IsColonist)
                            {
                                ___tmpPawns.Add(innerPawn);
                            }
                        }
                    }
                    IReadOnlyList<Pawn> allPawnsSpawned = ___tmpMaps[i].mapPawns.AllPawnsSpawned;
                    for (int k = 0; k < allPawnsSpawned.Count; k++)
                    {
                        if (allPawnsSpawned[k].carryTracker.CarriedThing is Corpse corpse && !corpse.IsDessicated() && corpse.InnerPawn.IsColonist)
                        {
                            ___tmpPawns.Add(corpse.InnerPawn);
                        }
                    }
                    foreach (Pawn tmpPawn in ___tmpPawns)
                    {
                        if (tmpPawn.playerSettings.displayOrder == -9999999)
                        {
                            tmpPawn.playerSettings.displayOrder = Mathf.Max(___tmpPawns.MaxBy((Pawn p) => p.playerSettings.displayOrder).playerSettings.displayOrder, 0) + 1;
                        }
                    }
                    PlayerPawnsDisplayOrderUtility.Sort(___tmpPawns);
                    foreach (Pawn tmpPawn2 in ___tmpPawns)
                    {
                        ___cachedEntries.Add(new ColonistBar.Entry(tmpPawn2, ___tmpMaps[i], num));
                    }
                    if (!___tmpPawns.Any())
                    {
                        ___cachedEntries.Add(new ColonistBar.Entry(null, ___tmpMaps[i], num));
                    }
                    num++;
                }
                ___tmpCaravans.Clear();
                ___tmpCaravans.AddRange(Find.WorldObjects.Caravans);
                ___tmpCaravans.SortBy((Caravan x) => x.ID);
                for (int l = 0; l < ___tmpCaravans.Count; l++)
                {
                    if (!___tmpCaravans[l].IsPlayerControlled)
                    {
                        continue;
                    }
                    ___tmpPawns.Clear();
                    ___tmpPawns.AddRange(___tmpCaravans[l].PawnsListForReading);
                    PlayerPawnsDisplayOrderUtility.Sort(___tmpPawns);
                    for (int m = 0; m < ___tmpPawns.Count; m++)
                    {
                        if (___tmpPawns[m].IsColonist || ___tmpPawns[m].IsColonyMutantPlayerControlled)
                        {
                            ___cachedEntries.Add(new ColonistBar.Entry(___tmpPawns[m], null, num));
                        }
                    }
                    num++;
                }
            }
            ___cachedReorderableGroups.Clear();
            foreach (ColonistBar.Entry cachedEntry in ___cachedEntries)
            {
                _ = cachedEntry;
                ___cachedReorderableGroups.Add(-1);
            }
            __instance.drawer.Notify_RecachedEntries();
            ___tmpPawns.Clear();
            ___tmpMaps.Clear();
            ___tmpCaravans.Clear();
            ___drawLocsFinder.CalculateDrawLocs(___cachedDrawLocs, out ___cachedScale, num);

            return false;
        }
    }
}
