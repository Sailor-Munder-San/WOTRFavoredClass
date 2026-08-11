using System;
using System.Linq;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.Root;
using UnityModManagerNet;

namespace WOTRFavoredClass
{
    public static class Main
    {
        internal static UnityModManager.ModEntry ModEntry;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            // No OnGUI, no OnUpdate and no OnToggle. The mod has no settings, so its Unity Mod
            // Manager page is deliberately blank, nothing of ours runs per frame, and there is no
            // enabled flag to read — the blueprints are built once at startup and stay.
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(typeof(Main).Assembly);
            Log("Loaded, waiting for BlueprintsCache init.");
            return true;
        }

        // The mod's only output: a handful of lines at startup saying what was installed, plus
        // any error. Nothing is counted, sampled or accumulated while the game runs.
        internal static void Log(string msg) => ModEntry?.Logger.Log(msg);

        // Anchored on LoadAllJson, the LAST blueprint-loading step of startup, because every
        // other content mod has finished by then. The startup coroutine runs
        // LoadPackTOC() and only afterwards LoadAllJson(path), and mods split across both:
        // PrestigePlus registers its classes in a LoadPackTOC postfix, while EbonsContentMod
        // creates its playable races — and configures arcanist exploits and faith magic — in a
        // LoadAllJson postfix. Anchoring on LoadPackTOC therefore ran before those races
        // existed, so they silently got no favored class option, and the mirrored reward pools
        // missed the content added alongside them.
        //
        // LoadAllJson can be called more than once (ClockworkScenarioIndex.Init also calls it),
        // hence the guard flag.
        [HarmonyPatch(typeof(StartGameLoader), nameof(StartGameLoader.LoadAllJson))]
        static class StartGameLoader_LoadAllJson_Patch
        {
            static bool Initialized;

            [HarmonyPriority(HarmonyLib.Priority.Last)]
            [HarmonyAfter("PrestigePlus", "Swashbuckler", "TabletopTweaks-Base", "MicroscopicContentExpansion", "ExpandedContent", "EbonsContentMod")]
            [HarmonyPostfix]
            static void Postfix()
            {
                if (Initialized) return;
                Initialized = true;
                try
                {
                    var classes = BlueprintRoot.Instance.Progression.CharacterClasses
                        .Where(c => c != null)
                        .ToList();
                    var races = BlueprintRoot.Instance.Progression.CharacterRaces
                        .Where(r => r != null)
                        .ToList();
                    Log($"LoadAllJson (post-mods). Visible character classes: {classes.Count}, character races: {races.Count}");
                    FavoredClasses.Install();
                }
                catch (Exception e)
                {
                    Log($"ERROR in LoadAllJson hook: {e}");
                }
            }
        }
    }
}
