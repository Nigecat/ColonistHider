using Verse;
using UnityEngine;
using HarmonyLib;

namespace ColonistHider
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            new Harmony(SettingsCategory()).PatchAll();
        }

        public override string SettingsCategory() => "!ColonistHider";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard settings = new Listing_Standard();

            settings.Begin(inRect);

            if (Find.World == null)
            {
                settings.Label("Settings are only accessible when a save is loaded.");
            }
            else
            {
                Config config = Find.World.GetComponent<Config>();

                bool wasEnabled = config.Enabled;
                settings.CheckboxLabeled("Enabled", ref config.Enabled, "Disabling this will display all colonists.");
                if (wasEnabled != config.Enabled)
                {
                    ColonistBar_ColonistBarOnGUI.MarkDirty();
                }

                foreach (var pawn in config.Blacklist)
                {
                    settings.Label(pawn.Name.ToStringShort);

                    if (Widgets.ButtonText(new Rect(inRect.xMax - 110, settings.CurHeight - 25, 100, 30), "Show"))
                    {
                        config.RemoveFromBlacklist(pawn);
                        ColonistBar_ColonistBarOnGUI.MarkDirty();
                    }

                    settings.Gap(5f);
                }
            }

            settings.End();
        }
    }
}
