using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;

namespace WOTRFavoredClass
{
    // Nested class sub-selections (arcane bond, school, deity...) are appended to the
    // level-up queue after the favored-class pick card, separating it from its bonus
    // card. Re-anchor OUR bonus selection right behind its parent (the favored class
    // pick) so the two cards always sit together. Vanilla selections are untouched.
    [HarmonyPatch(typeof(LevelUpState), nameof(LevelUpState.AddSelection))]
    internal static class LevelUpState_AddSelection_Patch
    {
        [HarmonyPostfix]
        static void Postfix(LevelUpState __instance, FeatureSelectionState parent, IFeatureSelection selection, FeatureSelectionState __result)
        {
            if (__result == null) return;
            if (selection is not BlueprintFeatureSelection bp) return;
            var guid = bp.AssetGuid.ToString();
            var list = __instance.Selections;

            // The per-class bonus selection is granted by the favored-class progression's
            // level entry, so the engine passes parent=null. Anchor it behind the favored
            // class pick card by looking that card up directly; on regular level-ups
            // (no pick card in queue) it simply stays appended.
            if (!FavoredClasses.BonusSelectionGuids.Contains(guid)) return;
            int fcIdx = list.FindIndex(s =>
                (s.Selection as BlueprintFeatureSelection)?.AssetGuid.ToString() == FavoredClasses.SelectionGuid);
            if (fcIdx < 0) return;
            if (!list.Remove(__result)) return;
            list.Insert(fcIdx + 1, __result);
        }
    }

    // Save-hygiene gate: never let ANY blueprint of this mod become a fact on a
    // non-player unit. Classed NPCs auto-level through the same class progressions
    // we attach the favored class selection to; without this gate their units carry
    // our GUIDs into area save files (junk data + would break those saves if the
    // mod is removed). Covers all current and future mod features.
    [HarmonyPatch(typeof(FeatureCollection))]
    internal static class FeatureCollection_PlayerFactionGate
    {
        static bool IsBlocked(FeatureCollection instance, BlueprintFeature blueprint)
        {
            // Hot path: runs on every fact grant in the game (incl. NPC spawns on
            // area load) — allocation-free BlueprintGuid comparison only.
            if (blueprint == null) return false;
            if (!FavoredClasses.AllModBlueprintGuids.Contains(blueprint.AssetGuid)) return false;
            var owner = instance.Owner;
            return owner != null && !owner.IsPlayerFaction;
        }

        [HarmonyPatch(nameof(FeatureCollection.AddFeature))]
        [HarmonyPrefix]
        static bool AddFeature_Prefix(FeatureCollection __instance, BlueprintFeature blueprint, ref Feature __result)
        {
            if (!IsBlocked(__instance, blueprint)) return true;
            __result = null;
            return false;
        }

        [HarmonyPatch(nameof(FeatureCollection.AddFact))]
        [HarmonyPrefix]
        static bool AddFact_Prefix(FeatureCollection __instance, BlueprintFeature blueprint, ref EntityFact __result)
        {
            if (!IsBlocked(__instance, blueprint)) return true;
            __result = null;
            return false;
        }
    }

    // Vanilla level-up tooltips replace the prerequisites block with "Already has
    // feature" once the unit owns the feature. Our favored-class bonuses are ranked
    // and picked repeatedly, so race and rank-progress prerequisites must stay
    // visible on every pick.
    [HarmonyPatch(typeof(Kingmaker.UI.MVVM._VM.Tooltip.Templates.TooltipTemplateLevelUpFeature), "GetBody")]
    internal static class TooltipTemplateLevelUpFeature_GetBody_Patch
    {
        [HarmonyPrefix]
        static void Prefix(Kingmaker.UI.MVVM._VM.Tooltip.Templates.TooltipTemplateLevelUpFeature __instance)
        {
            var bp = __instance.FeatureInfo?.BlueprintFeature;
            if (bp == null || !FavoredClasses.AllModGuids.Contains(bp.AssetGuid.ToString())) return;
            __instance.m_IsAquiredFeature = false;
            __instance.m_IsJustSelected = false;
        }
    }

    // Bookkeeping on every favored-class bonus pick:
    // 1. A wrapper reward pick (bonus combat feat, Magical Tail) also adds one rank
    //    of the wrapper's pick counter, so the counter shows the true pick total.
    // 2. When a counter crosses a divisor threshold, one rank of the separate
    //    visible EFFECT feature is granted — that feature carries the mechanics and
    //    appears in the character sheet with rank = earned whole bonuses.
    // Facts granted here bypass the normal level-up selection flow, which is what
    // sets Feature.Source automatically (see SelectFeature.Apply in the decompile:
    // "feature.SetSource(selectionState.Source, level)"). Without an explicit
    // SetSource call the granted fact has no source at all, so we tag it with the
    // mod's dedicated "Favored Class" source marker the same native way.
    [HarmonyPatch(typeof(SelectFeature), nameof(SelectFeature.Apply))]
    internal static class SelectFeature_Apply_BookkeepingPatch
    {
        [HarmonyPostfix]
        static void Postfix(SelectFeature __instance, LevelUpState state, UnitDescriptor unit)
        {
            var picked = __instance.Item?.Feature ?? __instance.m_ItemFeature;
            if (picked == null) return;
            var guid = picked.AssetGuid;
            int level = state?.NextCharacterLevel ?? unit.Progression.CharacterLevel;
            var source = (Kingmaker.UnitLogic.FeatureSource)BlueprintCore.Utils.BlueprintTool.Get<BlueprintProgression>(FavoredClasses.SourceMarkerGuid);

            if (FavoredClasses.RewardPickCounters.TryGetValue(guid, out var counterGuid))
            {
                var counterBp = BlueprintCore.Utils.BlueprintTool.Get<BlueprintFeature>(counterGuid);
                if (counterBp != null)
                {
                    unit.AddFact<Feature>(counterBp)?.SetSource(source, level);
                }
                return;
            }

            if (!FavoredClasses.EffectGrants.TryGetValue(guid, out var grant)) return;
            int rank = unit.Progression.Features.GetFact(picked)?.GetRank() ?? 0;
            if (rank <= 0 || rank % grant.Divisor != 0) return;
            var effectBp = BlueprintCore.Utils.BlueprintTool.Get<BlueprintFeature>(grant.EffectGuid);
            if (effectBp != null)
            {
                unit.AddFact<Feature>(effectBp)?.SetSource(source, level);
            }
        }
    }

    // Character sheet: feature descriptions are static, so ranked accumulative
    // bonuses show only their pick count. Append a computed line with the earned
    // whole bonus and progress toward the next increment.
    [HarmonyPatch(typeof(Kingmaker.UI.MVVM._VM.Tooltip.Templates.TooltipTemplateFeature), nameof(Kingmaker.UI.MVVM._VM.Tooltip.Templates.TooltipTemplateFeature.GetBody))]
    internal static class TooltipTemplateFeature_GetBody_Patch
    {
        [HarmonyPostfix]
        static System.Collections.Generic.IEnumerable<Owlcat.Runtime.UI.Tooltips.ITooltipBrick> Postfix(
            System.Collections.Generic.IEnumerable<Owlcat.Runtime.UI.Tooltips.ITooltipBrick> result,
            Kingmaker.UI.MVVM._VM.Tooltip.Templates.TooltipTemplateFeature __instance)
        {
            var fact = __instance.m_Feature;
            if (fact?.Blueprint == null) return result;
            if (!FavoredClasses.BonusDisplays.TryGetValue(fact.Blueprint.AssetGuid, out var info)) return result;

            int rank = fact.GetRank();
            int whole = rank / info.Divisor;
            string line = info.Kind switch
            {
                FavoredClasses.BonusDisplayKind.Feet => $"Current bonus: +{whole * 5} ft.",
                FavoredClasses.BonusDisplayKind.SkillRanks => $"Current bonus: +{whole} skill ranks",
                FavoredClasses.BonusDisplayKind.Feats => $"Feats gained: {whole}",
                FavoredClasses.BonusDisplayKind.HitPoints => $"Current bonus: +{whole} hit points",
                _ => $"Current bonus: +{whole}",
            };
            if (info.Divisor > 1)
            {
                line += $" (progress toward next: {rank % info.Divisor}/{info.Divisor})";
            }
            return System.Linq.Enumerable.Append(result,
                new Kingmaker.UI.MVVM._VM.Tooltip.Bricks.TooltipBrickText(line));
        }
    }

    // WOTR silently grants +1 HP per level of the character's highest base class
    // (ModifiableValueHitPoints.UpdateInternalModifiers adds a modifier with
    // ModifierDescriptor.FavouredClassBonus on every stat update). Our mod replaces
    // that automatic bonus with the player-chosen per-level favored class bonus,
    // so the automatic one must be stripped — mirroring the original ZFavoredClass
    // DisableAutomaticFavoredClassHitPoints behaviour. NPC units keep the vanilla
    // bonus so enemy stat blocks are unaffected.
    [HarmonyPatch(typeof(ModifiableValueHitPoints), "UpdateInternalModifiers")]
    internal static class ModifiableValueHitPoints_UpdateInternalModifiers_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ModifiableValueHitPoints __instance)
        {
            var owner = __instance.Owner;
            if (owner == null || !owner.IsPlayerFaction) return;
            __instance.RemoveModifiers(ModifierDescriptor.FavouredClassBonus);
        }
    }
}
