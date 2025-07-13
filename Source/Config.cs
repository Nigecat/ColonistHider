using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace ColonistHider
{
    public class Config : WorldComponent
    {
        private bool _disabled = false;
        public bool Disabled
        {
            get => _disabled;
            set
            {
                if (_disabled != value)
                {
                    _disabled = value;
                    Refresh();
                }
            }
        }

        private HashSet<string> Hidden = new();

        public static Config Current
        {
            get => Find.World.GetComponent<Config>();
        }

        public Config(World world) : base(world)
        {
            // Empty
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref Hidden, "hidden", LookMode.Value);
        }

        public void Hide(Pawn pawn)
        {
            Hidden.Add(pawn.GetUniqueLoadID());
            Refresh();
        }

        public void Show(Pawn pawn)
        {
            Hidden.Remove(pawn.GetUniqueLoadID());
            Refresh();
        }

        public bool IsHidden(Pawn pawn)
        {
            return Hidden.Contains(pawn.GetUniqueLoadID());
        }

        private void Refresh()
        {
            ColonistBar_ColonistBarOnGUI.MarkDirty();
        }
    }
}