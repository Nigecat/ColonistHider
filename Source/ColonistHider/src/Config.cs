using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ColonistHider
{
    public class Config : MapComponent
    {
        public bool Enabled = true;

        // List of blacklisted pawn ThingIDs
        private List<string> _blacklist = new List<string>();
        public List<Pawn> Blacklist
        {
            get
            {
                return Enabled && !_blacklist.NullOrEmpty()
                    ? _blacklist.Select(id => Find.CurrentMap.mapPawns.AllPawns.Find(pawn => pawn.ThingID == id)).ToList()
                    : new List<Pawn>();
            }
        }

        public Config(Map map) : base(map)
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
            if (Enabled)
            {
                return _blacklist.Contains(pawn.ThingID);
            }
            else
            {
                return false;
            }
        }

        public void AddToBlacklist(Pawn pawn)
        {
            // fixme this may add duplicate entries 
            _blacklist.Add(pawn.ThingID);
        }

        public void RemoveFromBlacklist(Pawn pawn)
        {
            _blacklist.Remove(pawn.ThingID);
        }
    }
}
