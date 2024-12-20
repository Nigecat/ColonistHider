using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace ColonistHider
{
    public class Config : WorldComponent
    {
        public bool Enabled = true;

        // List of blacklisted pawn ThingIDs
        private List<string> _blacklist = new List<string>();
        public List<Pawn> Blacklist
        {
            get
            {
                return Enabled && !_blacklist.NullOrEmpty()
                    ? _blacklist.Select(id => Find.World.PlayerPawnsForStoryteller.ToList().Find(pawn => pawn.ThingID == id)).ToList()
                    : new List<Pawn>();
            }
        }

        public Config(World world) : base(world)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref Enabled, "active", true);
            Scribe_Collections.Look(ref _blacklist, "blacklist", LookMode.Value);
        }

        public bool IsPawnBlacklisted(Pawn pawn)
        {
            return Enabled && _blacklist.Contains(pawn.ThingID);
        }

        public void AddToBlacklist(Pawn pawn)
        {
            if (!IsPawnBlacklisted(pawn))
            {
                _blacklist.Add(pawn.ThingID);
            }
        }

        public void RemoveFromBlacklist(Pawn pawn)
        {
            _blacklist.Remove(pawn.ThingID);
        }
    }
}
