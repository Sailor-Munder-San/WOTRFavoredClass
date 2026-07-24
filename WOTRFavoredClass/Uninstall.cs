using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;

namespace WOTRFavoredClass
{
    // Clean-uninstall support: strips every fact / progression record / selection
    // record created by this mod from all player characters in the LOADED save.
    // After running it, save the game — that save no longer references any mod
    // blueprint and survives mod removal.
    internal static class Uninstall
    {
        public static string LastResult = "";

        public static void StripFromLoadedSave()
        {
            var player = Game.Instance?.Player;
            if (player == null)
            {
                LastResult = "No save loaded — load a game first.";
                return;
            }

            int factCount = 0, progCount = 0, selCount = 0, unitCount = 0;
            // Party + remote companions + every unit in the loaded area (classed NPCs
            // may carry mod facts from builds made before the player-faction gate).
            var units = player.AllCharacters
                .Concat(player.RemoteCompanions)
                .Concat(Game.Instance.State.Units)
                .Distinct().ToList();
            foreach (var unit in units)
            {
                var d = unit?.Descriptor;
                if (d == null) continue;
                unitCount++;

                var toRemove = new List<EntityFact>();
                foreach (var fact in d.Facts.m_Facts)
                {
                    var bp = fact?.Blueprint;
                    if (bp != null && FavoredClasses.AllModGuids.Contains(bp.AssetGuid.ToString()))
                        toRemove.Add(fact);
                }
                foreach (var fact in toRemove)
                {
                    // Remove strips ONE rank per call on ranked facts — repeat until
                    // the fact is actually gone from the manager.
                    for (int guard = 0; guard < 32 && d.Facts.m_Facts.Contains(fact); guard++)
                    {
                        d.Facts.Remove(fact);
                    }
                    factCount++;
                }

                var progressions = d.Progression.m_Progressions;
                foreach (var key in progressions.Keys
                             .Where(k => FavoredClasses.AllModGuids.Contains(k.AssetGuid.ToString())).ToList())
                {
                    progressions.Remove(key);
                    progCount++;
                }

                var selections = d.Progression.m_Selections;
                foreach (var key in selections.Keys
                             .Where(k => FavoredClasses.AllModGuids.Contains(k.AssetGuid.ToString())).ToList())
                {
                    selections.Remove(key);
                    selCount++;
                }
            }

            LastResult = $"Stripped {factCount} facts, {progCount} progressions, {selCount} selections " +
                         $"from {unitCount} characters. SAVE the game now, then the mod can be removed safely.";
            Main.Log(LastResult);
        }
    }
}
