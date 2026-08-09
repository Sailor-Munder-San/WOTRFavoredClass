using System;
using System.Collections.Generic;
using System.Linq;

namespace WOTRFavoredClass
{
    // Opt-in diagnostic logging, driven from the mod's UMM menu.
    //
    // The shape is deliberate. A favored class bonus is picked a few dozen times in a
    // playthrough, but the components delivering it run inside rulebook events — on every
    // attack, every AC calculation, every stat refresh. Logging a line per event would produce
    // a log too large to read and slow the game while doing it. So there are two channels:
    //
    //   Event / First — rare, meaningful things (a pick, a grant, a gate blocking something).
    //                   Written straight to the log.
    //   Tally         — hot paths. Counted in memory and written as a summary, so an hour of
    //                   play yields one readable block instead of ten thousand lines.
    //
    // Everything is behind a plain static bool. When diagnostics are off the cost at each call
    // site is one static field read, which is why the instrumentation can sit on hot paths at
    // all. Call sites test `Diagnostics.Enabled` themselves before building any string, so no
    // interpolation or closure is allocated while disabled.
    internal static class Diagnostics
    {
        internal static bool Enabled;

        // Adds the per-event detail on hot paths that Tally normally folds into a counter.
        // Genuinely verbose — meant for a short reproduction, not an hour of play.
        internal static bool VerboseHotPaths;

        private static readonly HashSet<string> FirstSeen = new();
        private static readonly Dictionary<string, int> Counters = new();
        private static float LastDumpTime;
        private const float DumpIntervalSeconds = 120f;

        internal static void Log(string category, string message)
        {
            Main.Log($"[diag:{category}] {message}");
        }

        // Rare, meaningful event — always written when diagnostics are on.
        internal static void Event(string category, string message)
        {
            if (!Enabled) return;
            Log(category, message);
        }

        // Writes only the first time this key is seen, then counts silently. For things that
        // happen once per character or once per blueprint but sit on a repeating path.
        internal static void First(string category, string key, string message)
        {
            if (!Enabled) return;
            if (FirstSeen.Add(key))
            {
                Log(category, message);
            }
            Tally(key);
        }

        // Hot path: counted, not logged.
        internal static void Tally(string key)
        {
            if (!Enabled) return;
            Counters.TryGetValue(key, out int n);
            Counters[key] = n + 1;
        }

        // Called from UMM's update tick. Cheap when disabled, and even when enabled it only
        // touches the clock until the interval elapses.
        internal static void Tick()
        {
            if (!Enabled || Counters.Count == 0) return;
            float now = UnityEngine.Time.realtimeSinceStartup;
            if (now - LastDumpTime < DumpIntervalSeconds) return;
            LastDumpTime = now;
            Dump("periodic");
        }

        internal static void Dump(string reason)
        {
            if (Counters.Count == 0)
            {
                Main.Log($"[diag:summary] ({reason}) nothing recorded yet.");
                return;
            }
            Main.Log($"[diag:summary] ({reason}) {Counters.Count} distinct events:");
            foreach (var kv in Counters.OrderByDescending(k => k.Value))
            {
                Main.Log($"[diag:summary]    {kv.Value,8}x  {kv.Key}");
            }
        }

        internal static void Reset()
        {
            FirstSeen.Clear();
            Counters.Clear();
            LastDumpTime = UnityEngine.Time.realtimeSinceStartup;
        }

        internal static int DistinctEvents => Counters.Count;

        internal static int TotalEvents
        {
            get
            {
                int total = 0;
                foreach (var v in Counters.Values) total += v;
                return total;
            }
        }

        // Short, stable label for a unit — the log is read by a human comparing lines.
        internal static string Describe(Kingmaker.EntitySystem.Entities.UnitEntityData unit)
        {
            if (unit == null) return "<null unit>";
            var name = unit.CharacterName;
            if (string.IsNullOrEmpty(name)) name = unit.Blueprint?.name ?? "?";
            return $"{name}({(unit.IsPet ? "pet" : unit.IsPlayerFaction ? "party" : "npc")})";
        }
    }
}
