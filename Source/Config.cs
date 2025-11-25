using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace ColonistHider
{
    /// <summary>
    /// The per-world config containing the list of hidden pawns.
    /// </summary>
    public class Config : WorldComponent
    {
        private bool _disabled = false;

        /// <summary>
        /// Whether the colonist hiding system is currently disabled.
        /// </summary>
        public bool Disabled
        {
            get => _disabled;
            set
            {
                if (_disabled != value)
                {
                    _disabled = value;
                    // Refresh the colonist bar to account
                    //  for the new state
                    Refresh();
                }
            }
        }

        /// <summary>
        /// The current list of hidden pawns, tracked by their <c>GetUniqueLoadID()</c> value.
        /// </summary>
        private HashSet<string> Hidden = new();

        /// <summary>
        /// Get the current config instance for this world.
        /// This will throw a NullReferenceException if a world is not loaded.
        /// </summary>
        public static Config Current
        {
            get => Find.World.GetComponent<Config>();
        }

        /// <summary>
        /// Create a new empty config for the given world.
        /// </summary>
        /// <param name="world">The associated world.</param>
        public Config(World world) : base(world)
        {
            // Empty
        }

        /// <summary>
        /// Save config data.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref _disabled, "disabled");
            Scribe_Collections.Look(ref Hidden, "hidden", LookMode.Value);
        }

        /// <summary>
        /// Hide the given pawn, this will do nothing if they are already hidden.
        /// </summary>
        /// <param name="pawn">The pawn to hide.</param>
        public void Hide(Pawn pawn)
        {
            Hidden.Add(pawn.GetUniqueLoadID());
            Refresh();
        }

        /// <summary>
        /// Show the given pawn, this will do nothing if they are not hidden.
        /// </summary>
        /// <param name="pawn">The pawn to show.</param>
        public void Show(Pawn pawn)
        {
            Hidden.Remove(pawn.GetUniqueLoadID());
            Refresh();
        }

        /// <summary>
        /// Check whether the given pawn is currently hidden.
        /// Note that this is independent of the <c>Disabled</c> state.
        /// </summary>
        /// <param name="pawn">The pawn to check.</param>
        /// <returns>Whether the pawn is hidden.</returns>
        public bool IsHidden(Pawn pawn)
        {
            try
            {
                return Hidden.Contains(pawn.GetUniqueLoadID());
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Reload the colonist bar, as the draw calls are cached.
        /// This is called automatically by the <c>Config</c> class where relevant.
        /// </summary>
        private void Refresh()
        {
            ColonistBar_ColonistBarOnGUI.MarkDirty();
        }
    }
}