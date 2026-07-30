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
        // Animal companions and other pets level real character classes, and the basic
        // feat progression our favored class pick hangs off applies to them as well — so
        // without this they get offered a favored class of their own. A pet has no
        // business having one, and its bonus options are written for player characters.
        // Dropping the selection here (rather than emptying it) means no dead card is
        // left in the level-up queue at all.
        [HarmonyPrefix]
        static bool Prefix(LevelUpState __instance, IFeatureSelection selection, ref FeatureSelectionState __result)
        {
            if (selection is not BlueprintFeatureSelection bp) return true;
            var guid = bp.AssetGuid.ToString();
            if (guid != FavoredClasses.SelectionGuid && guid != FavoredClasses.MultitalentedGuid) return true;
            if (__instance?.Unit?.Unit?.IsPet != true) return true;
            __result = null;
            return false;
        }

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

    // Save-hygiene gate for the path that actually grants progression and race features.
    //
    // LevelUpHelper.AddFeaturesFromProgression grants each feature with unit.AddFact(item),
    // which does NOT go through FeatureCollection.AddFeature/AddFact — so the gate below on
    // FeatureCollection never saw these at all. Every NPC of a playable race therefore picked
    // up the favored class selection as a fact and carried our GUID into that area's save
    // file: an audit of one save found 13 units in a single area holding it. Those saves would
    // break on removing the mod, which is exactly what the save-hygiene rules exist to stop.
    //
    // The condition is deliberately one-sided. A unit only loses the selection when it can be
    // positively identified as already spawned into the world and not on the player's side.
    // Anything not classifiable keeps it, because a player character silently missing their
    // favored class is a far worse failure than a stray fact on a bystander — and during
    // chargen the player's unit is not in the world yet, so it is never caught here.
    //
    // Filtering replaces the list reference rather than mutating it: the caller passes
    // levelEntry.Features for progressions, which is the blueprint's own list.
    [HarmonyPatch(typeof(Kingmaker.UnitLogic.Class.LevelUp.Actions.LevelUpHelper),
        nameof(Kingmaker.UnitLogic.Class.LevelUp.Actions.LevelUpHelper.AddFeaturesFromProgression))]
    internal static class LevelUpHelper_AddFeaturesFromProgression_Patch
    {
        [HarmonyPrefix]
        static void Prefix(UnitDescriptor unit, ref System.Collections.Generic.IList<BlueprintFeatureBase> features)
        {
            if (features == null || features.Count == 0) return;
            var u = unit?.Unit;
            if (u == null || !u.IsInGame || u.IsPlayerFaction) return;

            bool anyOurs = false;
            for (int i = 0; i < features.Count; i++)
            {
                if (IsOurSelection(features[i])) { anyOurs = true; break; }
            }
            if (!anyOurs) return;

            var filtered = new System.Collections.Generic.List<BlueprintFeatureBase>(features.Count);
            for (int i = 0; i < features.Count; i++)
            {
                if (!IsOurSelection(features[i])) filtered.Add(features[i]);
            }
            features = filtered;
        }

        static bool IsOurSelection(BlueprintFeatureBase feature)
        {
            if (feature == null) return false;
            var guid = feature.AssetGuid.ToString();
            return guid == FavoredClasses.SelectionGuid || guid == FavoredClasses.MultitalentedGuid;
        }
    }

    // Second line of defence, for grants that DO route through FeatureCollection — including
    // the pet-side features this mod hands out itself. It does not cover progression or race
    // features; those go through the patch above.
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

            // A favored class pick has to be attributed to the class it is for, or the
            // character sheet will not file it under that class: the progression panel is
            // built from GetClassProgressions(cls), which keeps a fact only when
            // Feature.GetSourceClass() is that class, and GetSourceClass just reads the
            // fact's source. Left alone, the source is whatever granted the selection —
            // the same one for every pick — so a half-elf's two picks would collapse onto
            // a single class instead of appearing under each.
            if (FavoredClasses.FavoredClassProgressionClass.TryGetValue(guid, out var ownerClassGuid))
            {
                var ownerClass = BlueprintCore.Utils.BlueprintTool.Get<BlueprintCharacterClass>(ownerClassGuid);
                var pickedFact = unit.Progression.Features.GetFact(picked);
                if (ownerClass != null && pickedFact != null)
                {
                    pickedFact.SetSource((Kingmaker.UnitLogic.FeatureSource)ownerClass, level);
                }
                return;
            }

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

    // The character sheet lists a class's progressions in the order their facts were added
    // to the unit, so where the favored class block lands depends purely on which came
    // first. A half-elf picks both favored classes during chargen but usually levels the
    // second class much later, so for that class the favored class fact predates the class
    // itself and the block is drawn ABOVE the class features, while for the first class it
    // is drawn below. Sorting this mod's progressions to the end makes the block sit under
    // the class features every time. Only our own blueprints move; the relative order of
    // everything else is preserved.
    [HarmonyPatch(typeof(Kingmaker.UnitLogic.UnitProgressionData), nameof(Kingmaker.UnitLogic.UnitProgressionData.GetClassProgressions))]
    internal static class UnitProgressionData_GetClassProgressions_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref Kingmaker.UnitLogic.ProgressionData[] __result)
        {
            if (__result == null || __result.Length < 2) return;

            // Stable partition in place. GetClassProgressions builds its result with
            // ToArray(), so this array is freshly allocated and nobody else holds it —
            // reordering it costs nothing, where allocating a replacement would add garbage
            // to every call, and the character sheet calls this per class per refresh.
            int insert = 0;
            for (int i = 0; i < __result.Length; i++)
            {
                if (IsOurs(__result[i])) continue;
                if (insert != i)
                {
                    var moved = __result[i];
                    for (int j = i; j > insert; j--) __result[j] = __result[j - 1];
                    __result[insert] = moved;
                }
                insert++;
            }
        }

        static bool IsOurs(Kingmaker.UnitLogic.ProgressionData data)
        {
            var bp = data?.Blueprint;
            return bp != null && FavoredClasses.AllModBlueprintGuids.Contains(bp.AssetGuid);
        }
    }

    // A stat breakdown labels each modifier by its source: the source fact's own display name
    // when the modifier has one, otherwise the modifier descriptor's name — which is why our
    // untyped bonuses read "Other" and the hit point bonus read "Bonus Hit Point". Neither
    // tells the player the bonus came from their favored class. Relabel any modifier whose
    // source is one of this mod's facts.
    //
    // This is the inner overload, reached from GetBonusSourceText(Modifier) once it has
    // established the modifier has a source; the overload has to be named explicitly because
    // the method is overloaded. UI-only path, evaluated when a breakdown tooltip is built.
    [HarmonyPatch(typeof(Kingmaker.UI.Common.StatModifiersBreakdown),
        nameof(Kingmaker.UI.Common.StatModifiersBreakdown.GetBonusSourceText),
        new[] { typeof(Kingmaker.UI.IUIDataProvider), typeof(bool) })]
    internal static class StatModifiersBreakdown_GetBonusSourceText_Patch
    {
        [HarmonyPostfix]
        static void Postfix(Kingmaker.UI.IUIDataProvider source, ref string __result)
        {
            if (source is not EntityFact fact) return;
            var blueprint = fact.Blueprint;
            if (blueprint == null) return;
            if (!FavoredClasses.AllModBlueprintGuids.Contains(blueprint.AssetGuid)) return;
            __result = FavoredClasses.BonusSourceLabel;
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

            // Drop the modifier out of the list directly instead of calling
            // RemoveModifiers. That method ends in UpdateValue(), and we are already inside
            // UpdateValue: it runs UpdateInternalModifiers() (this patch) and only then
            // ApplyModifiersFiltered(), so a removal made here is picked up by the pass
            // already in flight. Going through the public method instead started a second,
            // nested recalculation — of this value, of every dependent ModifiableValue, and
            // of the dependent facts and components — on every single HP update, for every
            // party member, every time anything touched their stats. The steps below are
            // exactly what RemoveModifiers does, minus that trailing UpdateValue().
            if (!__instance.ModifierList.TryGetValue(ModifierDescriptor.FavouredClassBonus, out var mods)) return;
            __instance.ModifierList.Remove(ModifierDescriptor.FavouredClassBonus);
            for (int i = 0; i < mods.Count; i++)
            {
                __instance.PrepareForRemoval(mods[i]);
            }
        }
    }
}
