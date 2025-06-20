using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace ColonistHider
{
    public class Config : WorldComponent
    {
        public bool Enabled { get; private set; } = true;
        private HashSet<string> Blacklist = new();

        public static Config Current
        {
            get => Find.World.GetComponent<Config>();
        }

        public Config(World world) : base(world)
        {
            // Empty
        }

        public override void WorldComponentUpdate()
        {
            if (KeyBindings.Toggle != null && KeyBindings.Toggle.JustPressed)
            {
                Enabled = !Enabled;
                Refresh();
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref Blacklist, "blacklist", LookMode.Value);
        }

        public void Hide(Pawn pawn)
        {
            Blacklist.Add(pawn.GetUniqueLoadID());
            Refresh();
        }

        public void Show(Pawn pawn)
        {
            Blacklist.Remove(pawn.GetUniqueLoadID());
            Refresh();
        }

        public bool IsHidden(Pawn pawn)
        {
            return Blacklist.Contains(pawn.GetUniqueLoadID());
        }

        public void Refresh()
        {
            ColonistBar_ColonistBarOnGUI.MarkDirty();
        }
    }
}