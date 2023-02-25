using System.Reflection;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using RimWorld.Planet;
using HugsLib.Settings;

using System;
using System.Linq;
using System.Collections.Generic;

namespace ColonistHider
{
    public class Mod : HugsLib.ModBase
    {
        public class Blacklist : SettingHandleConvertible
        {
            public List<string> blacklist = new List<string>();


            public override bool ShouldBeSaved
            {
                get { return blacklist.Count > 0; }
            }

            public override void FromString(string value)
            {
                blacklist = value.Split('|').ToList();
            }

            public override string ToString()
            {
                return blacklist != null ? string.Join("|", blacklist.ToArray()) : "";
            }
        }

        public static Mod Instance { get; private set; }

        public static SettingHandle<Blacklist> blacklist;

        public override string ModIdentifier
        {
            get { return "ColonistHider"; }
        }

        public Mod()
        {
            Instance = this;
        }

        public override void DefsLoaded()
        {
            blacklist = Settings.GetHandle<Blacklist>("blacklist", "Pawn Blacklist", null);
            if (blacklist.Value == null) blacklist.Value = new Blacklist();
        }

        public override void SettingsChanged()
        {
            ColonistBar_ColonistBarOnGUI.Dirty = true;
            base.SettingsChanged();
        }
    }

    [HarmonyPatch(typeof(ColonistBar))]
    [HarmonyPatch("ColonistBarOnGUI")]
    public static class ColonistBar_ColonistBarOnGUI
    {
        public static bool Dirty = true;

        public static void Prefix(ColonistBar __instance)
        {
            if (Dirty)
            {
                __instance.MarkColonistsDirty();
                __instance.GetType().GetTypeInfo().GetDeclaredMethod("CheckRecacheEntries").Invoke(__instance, new object[] { });
                Dirty = false;
            }
        }
    }

    [HarmonyPatch(typeof(ColonistBar))]
    [HarmonyPatch("CheckRecacheEntries")]
    public static class ColonistBar_CheckRecacheEntries
    {
        private static List<Pawn> Purge(List<Pawn> pawns)
        {
            Mod.Blacklist value = Mod.blacklist.Value;
            if (value != null && value.blacklist != null)
            {
                return pawns.Where(pawn => !value.blacklist.Contains(pawn.Name.ToStringShort)).ToList();
            }

            return pawns;
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
                    ___tmpPawns.AddRange(Purge(___tmpMaps[i].mapPawns.FreeColonists));
                    // ======================= MODIFICATIONS  END  HERE =======================



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
                    List<Pawn> allPawnsSpawned = ___tmpMaps[i].mapPawns.AllPawnsSpawned;
                    for (int k = 0; k < allPawnsSpawned.Count; k++)
                    {
                        if (allPawnsSpawned[k].carryTracker.CarriedThing is Corpse corpse && !corpse.IsDessicated() && corpse.InnerPawn.IsColonist)
                        {
                            ___tmpPawns.Add(corpse.InnerPawn);
                        }
                    }
                    PlayerPawnsDisplayOrderUtility.Sort(___tmpPawns);
                    for (int l = 0; l < ___tmpPawns.Count; l++)
                    {
                        ___cachedEntries.Add(new ColonistBar.Entry(___tmpPawns[l], ___tmpMaps[i], num));
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
                for (int m = 0; m < ___tmpCaravans.Count; m++)
                {
                    if (!___tmpCaravans[m].IsPlayerControlled)
                    {
                        continue;
                    }
                    ___tmpPawns.Clear();
                    ___tmpPawns.AddRange(___tmpCaravans[m].PawnsListForReading);
                    PlayerPawnsDisplayOrderUtility.Sort(___tmpPawns);
                    for (int n = 0; n < ___tmpPawns.Count; n++)
                    {
                        if (___tmpPawns[n].IsColonist)
                        {
                            ___cachedEntries.Add(new ColonistBar.Entry(___tmpPawns[n], null, num));
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
