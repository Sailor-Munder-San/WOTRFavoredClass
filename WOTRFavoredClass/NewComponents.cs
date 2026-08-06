using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Parts;

namespace WOTRFavoredClass
{
    // Rank-scaled variant of vanilla IncreaseSpellSchoolCasterLevel: +1 caster level
    // per rank of the carrier feature for spells of the given school.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a04")]
    public class IncreaseSpellSchoolCasterLevelPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public SpellSchool School;
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            bool match = false;
            if (evt.AbilityData != null)
            {
                match = evt.AbilityData.SpellSchool == School;
            }
            else
            {
                // Plain loop, no LINQ: matches this file's hot-path convention (see
                // FavoredEnemyCheck below) — grabs the first SpellComponent, same as
                // FirstOrDefault() did, without an extension-method call per cast.
                SpellComponent comp = null;
                foreach (var c in evt.Spell.GetComponents<SpellComponent>()) { comp = c; break; }
                if (comp != null) match = comp.School == School;
            }
            if (match)
            {
                evt.AddBonusCasterLevel(bonus, ModifierDescriptor.UntypedStackable);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }

    // +1 caster level per rank for spells carrying a given descriptor. The school-based
    // component above cannot express this: a descriptor such as Good is orthogonal to the
    // school. Reads the descriptor off the cast context, which is where the engine keeps it.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a15")]
    public class IncreaseSpellDescriptorCasterLevelPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public SpellDescriptorWrapper Descriptors;
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            var spell = evt.Spell;
            if (spell == null || !spell.IsSpell) return;
            if (!spell.SpellDescriptor.HasAnyFlag(Descriptors)) return;
            evt.AddBonusCasterLevel(bonus, ModifierDescriptor.UntypedStackable);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }

    // Rank-scaled variant of vanilla IncreaseSpellSchoolDC: +1 DC per rank of the
    // carrier feature for spells of the given school.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a05")]
    public class IncreaseSpellSchoolDCPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public SpellSchool School;
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            bool match = false;
            if (evt.AbilityData != null)
            {
                match = evt.AbilityData.SpellSchool == School;
            }
            else
            {
                // Plain loop, no LINQ: matches this file's hot-path convention (see
                // FavoredEnemyCheck below) — grabs the first SpellComponent, same as
                // FirstOrDefault() did, without an extension-method call per cast.
                SpellComponent comp = null;
                foreach (var c in evt.Spell.GetComponents<SpellComponent>()) { comp = c; break; }
                if (comp != null) match = comp.School == School;
            }
            if (match)
            {
                evt.AddBonusDC(bonus, ModifierDescriptor.UntypedStackable);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }

    // Damage bonus per rank against a target carrying one of the listed buffs CAST BY THE
    // OWNER (cavalier challenge: the vanilla challenge ability puts CavalierChallengeBuffTarget
    // on the challenged foe with the cavalier as its caster). Requiring the caster to be the
    // owner is what keeps two cavaliers from feeding off each other's challenge, the same
    // reasoning as ACBonusAgainstCasterBuffPerRank below.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a16")]
    public class DamageBonusAgainstCasterBuffTargetPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleAttackWithWeapon>, IRulebookHandler<RuleAttackWithWeapon>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public BlueprintBuffReference[] m_Buffs = new BlueprintBuffReference[0];
        public int Divisor = 1;

        // Halfling cavalier: "+1/2 to effective class level for determining the damage he deals
        // when making an attack of opportunity against a challenged foe". The challenge's extra
        // damage IS the cavalier's class level, so raising that level for this purpose is simply
        // more damage — the same bonus as the Aasimar/Dwarf entries, restricted to attacks of
        // opportunity. RuleAttackWithWeapon exposes IsAttackOfOpportunity directly.
        public bool OnlyAttackOfOpportunity;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            if (OnlyAttackOfOpportunity && !evt.IsAttackOfOpportunity) return;
            var target = evt.Target;
            if (target == null) return;
            // Plain nested loops, no LINQ or closures: this runs on every weapon attack.
            bool marked = false;
            foreach (var buff in target.Buffs.Enumerable)
            {
                if (buff.Context?.MaybeCaster != Owner) continue;
                for (int i = 0; i < m_Buffs.Length; i++)
                {
                    if (m_Buffs[i].Get() == buff.Blueprint) { marked = true; break; }
                }
                if (marked) break;
            }
            if (marked)
            {
                evt.AddTemporaryModifier(
                    evt.Initiator.Stats.AdditionalDamage.AddModifier(bonus, Fact, ModifierDescriptor.UntypedStackable));
            }
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt) { }
    }

    // Damage bonus per rank on attacks made with a weapon of one of the listed categories, and
    // optionally only while the owner carries one of the listed buffs (orc shifter: "add 1/5 to
    // the damage dealt when using the shifter claws ability").
    //
    // Both halves are needed to mean "the shifter claws ability" specifically. The category alone
    // is too broad, because an animal form's claws are claws too. The buff alone is too broad in
    // the other direction, because the modal stays on while its owner swings a sword. WOTR ships
    // only ONE shifter-specific claw weapon blueprint (ShifterClaw1d10x3) against seven claw
    // modals, so the lower-level claws reuse the generic Claw1dX blueprints that animal forms
    // also use — which is why matching the weapon blueprint cannot work and the modal's own buff
    // is the thing to key on.
    //
    // The category is tested first: it is a field read, whereas the buff scan walks a collection,
    // and the overwhelming majority of attacks in play are not claws at all.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a18")]
    public class WeaponCategoryDamageBonusPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleAttackWithWeapon>, IRulebookHandler<RuleAttackWithWeapon>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public Kingmaker.Enums.WeaponCategory[] Categories = new Kingmaker.Enums.WeaponCategory[0];
        public BlueprintBuffReference[] m_RequiredOwnerBuffs = new BlueprintBuffReference[0];
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            var category = evt.Weapon?.Blueprint?.Category;
            if (category == null) return;
            // Plain loops, no LINQ: this runs on every weapon attack.
            bool match = false;
            for (int i = 0; i < Categories.Length; i++)
            {
                if (Categories[i] == category.Value) { match = true; break; }
            }
            if (!match) return;

            if (m_RequiredOwnerBuffs.Length > 0)
            {
                bool active = false;
                foreach (var buff in Owner.Buffs.Enumerable)
                {
                    for (int i = 0; i < m_RequiredOwnerBuffs.Length; i++)
                    {
                        if (m_RequiredOwnerBuffs[i].Get() == buff.Blueprint) { active = true; break; }
                    }
                    if (active) break;
                }
                if (!active) return;
            }

            evt.AddTemporaryModifier(
                evt.Initiator.Stats.AdditionalDamage.AddModifier(bonus, Fact, ModifierDescriptor.UntypedStackable));
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt) { }
    }

    // Dodge AC bonus per rank against attackers of a given size or larger (shifter halfling:
    // "increase the AC bonus from defensive instinct by 1/4 against creatures of size Large or
    // larger"). Reads State.Size rather than OriginalSize so an enlarged attacker counts, which
    // is how the tabletop size categories work in play.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a17")]
    public class ACBonusAgainstLargerCreaturesPerRank : UnitFactComponentDelegate,
        ITargetRulebookHandler<Kingmaker.RuleSystem.Rules.RuleCalculateAC>,
        IRulebookHandler<Kingmaker.RuleSystem.Rules.RuleCalculateAC>,
        ISubscriber, ITargetRulebookSubscriber
    {
        public Kingmaker.Enums.Size MinimumSize = Kingmaker.Enums.Size.Large;
        public int Divisor = 1;

        public void OnEventAboutToTrigger(Kingmaker.RuleSystem.Rules.RuleCalculateAC evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            var attacker = evt.Initiator;
            if (attacker == null) return;
            if (attacker.State.Size < MinimumSize) return;
            evt.AddModifier(bonus, Fact, ModifierDescriptor.Dodge);
        }

        public void OnEventDidTrigger(Kingmaker.RuleSystem.Rules.RuleCalculateAC evt) { }
    }

    // "+1/4 dodge bonus to AC against favored enemies": applies to the owner's AC
    // only when the attacker matches one of the owner's favored enemy entries.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a06")]
    public class ACBonusAgainstFavoredEnemyPerRank : UnitFactComponentDelegate,
        ITargetRulebookHandler<Kingmaker.RuleSystem.Rules.RuleCalculateAC>,
        IRulebookHandler<Kingmaker.RuleSystem.Rules.RuleCalculateAC>,
        ISubscriber, ITargetRulebookSubscriber
    {
        public int Divisor = 1;

        public void OnEventAboutToTrigger(Kingmaker.RuleSystem.Rules.RuleCalculateAC evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            var part = Owner.Get<Kingmaker.UnitLogic.Parts.UnitPartFavoredEnemy>();
            if (part == null) return;
            var attacker = evt.Initiator;
            if (attacker == null) return;
            if (FavoredEnemyCheck.IsFavored(part, attacker))
            {
                evt.AddModifier(bonus, Fact, ModifierDescriptor.Dodge);
            }
        }

        public void OnEventDidTrigger(Kingmaker.RuleSystem.Rules.RuleCalculateAC evt) { }
    }

    // Shared favored-enemy match check for the three components below — plain loops
    // (no LINQ/closures) since RuleCalculateAC/RuleCalculateAttackBonus/
    // RuleAttackWithWeapon fire on every attack roll and AC calculation in combat.
    internal static class FavoredEnemyCheck
    {
        public static bool IsFavored(Kingmaker.UnitLogic.Parts.UnitPartFavoredEnemy part, UnitEntityData target)
        {
            if (part == null || target == null) return false;
            foreach (var entry in part.Entries)
            {
                foreach (var feature in entry.CheckedFeatures)
                {
                    if (feature != null && target.Descriptor.HasFact(feature)) return true;
                }
            }
            return false;
        }
    }

    // Rank-scaled resource-pool bonus: adds Fact.GetRank() to the max amount of the
    // named BlueprintAbilityResource (rage rounds, ki pool, arcane pool...). Uses the
    // same native query interface TabletopTweaks-Core uses for its weapon-training-
    // scaled resource bonus, so no Harmony patch is needed — the engine calls this on
    // every max-amount recalculation.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a08")]
    public class IncreaseResourceAmountPerRank : UnitFactComponentDelegate, IResourceAmountBonusHandler, IUnitSubscriber, ISubscriber
    {
        public BlueprintAbilityResourceReference m_Resource;

        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            if (!Fact.Active) return;
            if (m_Resource?.Get() != resource) return;
            bonus += Fact.GetRank();
        }
    }

    // A half-race qualifies for its OWN bonuses and for those of BOTH parent races:
    //   Half-Orc → Half-Orc, Human, Orc
    //   Half-Elf → Half-Elf, Human, Elf
    //
    // The rule lives here rather than in the entry data on purpose. It used to be applied by
    // hand, by listing Half-Elf and Half-Orc alongside Human in each entry's Races array, and
    // that silently failed: an audit found 11 of 108 entries had missed one or both, including
    // every paladin energy resistance and the whole Orc line — a half-orc could not reach the
    // orc shifter's claw bonus at all. Encoding it once, where the check happens, means no entry
    // can forget it again and the data can name the race the tabletop names.
    //
    // Guid comparison, not blueprint identity: this runs for every option on every refresh of
    // the level-up screen.
    internal static class RaceHeritage
    {
        private static readonly BlueprintGuid HalfElf = BlueprintGuid.Parse("b3646842ffbd01643ab4dac7479b20b0");
        private static readonly BlueprintGuid HalfOrc = BlueprintGuid.Parse("1dc20e195581a804890ddc74218bfd8e");
        private static readonly BlueprintGuid Human = BlueprintGuid.Parse("0a5d473ead98b0646b94495af250fdc4");
        private static readonly BlueprintGuid Elf = BlueprintGuid.Parse("25a5878d125338244896ebd3238226c8");
        private static readonly BlueprintGuid Orc = BlueprintGuid.Parse("7088a348ef0646dabdb3900fb187fb21");

        internal static bool Qualifies(BlueprintGuid actualRace, BlueprintGuid listedRace)
        {
            if (actualRace == listedRace) return true;
            if (actualRace == HalfElf) return listedRace == Human || listedRace == Elf;
            if (actualRace == HalfOrc) return listedRace == Human || listedRace == Orc;
            return false;
        }

        // True for a race that reaches bonuses beyond its own, so the tooltip can say so.
        internal static bool IsDualHeritage(BlueprintGuid race) => race == HalfElf || race == HalfOrc;
    }

    // Race gate for racial favored class bonuses (WOTR has no vanilla race prerequisite).
    // Stateless per the save-hygiene invariants.
    [AllowedOn(typeof(BlueprintFeature), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a02")]
    public class PrerequisiteRaceAny : Prerequisite
    {
        public BlueprintRaceReference[] m_Races = new BlueprintRaceReference[0];

        public override bool CheckInternal(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
        {
            var race = unit.Progression.Race;
            if (race == null) return false;
            var actual = race.AssetGuid;
            // Plain loop, no closure: the level-up screen re-evaluates every option's
            // prerequisites on each refresh, and this gate sits on most of our entries.
            for (int i = 0; i < m_Races.Length; i++)
            {
                if (RaceHeritage.Qualifies(actual, m_Races[i].Guid)) return true;
            }
            return false;
        }

        public override string GetUITextInternal(UnitDescriptor unit)
        {
            var text = "Race: " + string.Join(" or ", m_Races.Select(r => r.Get()?.Name ?? "?"));
            // Otherwise a half-orc offered an orc bonus reads "Race: Orc" and looks like a bug.
            var race = unit?.Progression?.Race;
            if (race != null && RaceHeritage.IsDualHeritage(race.AssetGuid))
            {
                text += $" (a {race.Name} counts as both parent races)";
            }
            return text;
        }
    }

    // Display-only prerequisite: always passes, shows how many times an incremental
    // bonus has been taken and how far the next threshold is (mirrors the progress
    // line that cycle-based bonuses get from PrerequisiteFactRankCycle).
    [AllowedOn(typeof(BlueprintFeature), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a07")]
    public class PrerequisiteRankProgressDisplay : Prerequisite
    {
        public BlueprintUnitFactReference m_Fact;
        public int Step = 2;

        public override bool CheckInternal(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
        {
            return true;
        }

        public override string GetUITextInternal(UnitDescriptor unit)
        {
            int rank = FactRanks.Of(unit, m_Fact);
            return $"Progress toward next bonus: {rank % Step}/{Step}";
        }
    }

    // Rank of a fact on a unit, without a closure. The level-up screen re-evaluates
    // prerequisites for every option on every refresh, and the rank-cycle gate below asks
    // for two ranks per evaluation — each one a scan of the unit's whole fact list, which
    // for a high-level character is hundreds of entries.
    internal static class FactRanks
    {
        public static int Of(UnitDescriptor unit, BlueprintUnitFactReference bpRef)
        {
            var bp = bpRef?.Get();
            if (bp == null || unit == null) return 0;
            var facts = unit.Facts.m_Facts;
            for (int i = 0; i < facts.Count; i++)
            {
                var fact = facts[i];
                if (fact != null && fact.Blueprint == bp) return fact.GetRank();
            }
            return 0;
        }
    }

    // Cadence gate for "1/N of a bonus" favored class options. The partial fact
    // counts EVERY pick (including reward picks, which add a partial rank via the
    // SelectFeature postfix), the full fact counts rewards taken:
    // ready = partial - full*N >= N-1  (rewards unlock on picks N, 2N, 3N...).
    [AllowedOn(typeof(BlueprintFeature), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a03")]
    public class PrerequisiteFactRankCycle : Prerequisite
    {
        public BlueprintUnitFactReference m_Partial;
        public BlueprintUnitFactReference m_Full;
        public int Divisor = 6;
        public bool Not;

        static int RankOf(UnitDescriptor unit, BlueprintUnitFactReference bpRef) => FactRanks.Of(unit, bpRef);

        public override bool CheckInternal(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
        {
            int need = Divisor - 1;
            bool ready = RankOf(unit, m_Partial) - RankOf(unit, m_Full) * Divisor >= need;
            return Not ? !ready : ready;
        }

        public override string GetUITextInternal(UnitDescriptor unit)
        {
            int need = Divisor - 1;
            int progress = RankOf(unit, m_Partial) - RankOf(unit, m_Full) * Divisor;
            return Not
                ? $"Progress toward next bonus: {progress}/{need}"
                : $"Requires {need} accumulated picks (current: {progress}/{need})";
        }
    }

    // "+1/2 skill rank": every two ranks of the carrier feature yield one extra skill
    // point. Mirrors vanilla AddSkillPointPerCharacterLevel — ApplySkillPoints computes
    // TotalSkillPoints as (cumulative total + extra - spent), so the contribution here
    // must be cumulative as well: floor(rank / 2) at any moment.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a01")]
    public class AddSkillPointPerFavoredRank : UnitFactComponentDelegate, IUnitCalculateSkillPointsOnLevelupHandler, IUnitSubscriber, ISubscriber
    {
        public void HandleUnitCalculateSkillPointsOnLevelup(LevelUpState state, ref int extraSkillPoints)
        {
            extraSkillPoints += Fact.GetRank() / 2;
        }
    }

    // +1 to concentration checks per rank. Rides the native concentration subsystem:
    // RuleCalculateAbilityParams accumulates the bonus at cast time, and
    // IConcentrationBonusProvider reports the same value for static UI queries
    // (both real-world users of this API — TTT-Core AlliedSpellcaster, PrestigePlus
    // DefendCharge — implement the pair together).
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a09")]
    public class ConcentrationBonusPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>,
        ISubscriber, IInitiatorRulebookSubscriber, IConcentrationBonusProvider
    {
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            evt.AddBonusConcentration(bonus);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }

        public int GetStaticConcentrationBonus(EntityFactComponent runtime)
        {
            var fact = runtime?.Fact;
            return fact != null ? fact.GetRank() / Divisor : 0;
        }
    }

    // Flat damage bonus per rank for a whitelisted set of abilities (kineticist
    // blasts). Matches both the exact ability and its variant parent, because blast
    // forms (extended range, kinetic blade...) are child abilities whose Parent
    // points back to the base blast.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a0a")]
    public class AbilityDamageBonusPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public BlueprintAbilityReference[] m_Abilities = new BlueprintAbilityReference[0];
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            var srcAbility = evt.Reason.Context?.SourceAbility;
            if (srcAbility == null) return;
            bool match = false;
            for (int i = 0; i < m_Abilities.Length; i++)
            {
                var bp = m_Abilities[i].Get();
                if (bp != null && (bp == srcAbility || bp == srcAbility.Parent)) { match = true; break; }
            }
            if (!match) return;
            foreach (var dmg in evt.DamageBundle)
            {
                if (!dmg.Precision) dmg.AddModifier(bonus, Fact);
            }
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt) { }
    }

    // Flat damage bonus per rank for spells matching a spell descriptor (fire, acid...).
    // Same RuleCalculateDamage hook TTT-Core's BonusDamagePerDie uses.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a0b")]
    public class SpellDescriptorDamageBonusPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public SpellDescriptorWrapper Descriptors;
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            var context = evt.Reason.Context;
            if (context?.SourceAbility == null || !context.SourceAbility.IsSpell) return;
            if (!context.SpellDescriptor.HasAnyFlag(Descriptors)) return;
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            foreach (var dmg in evt.DamageBundle)
            {
                if (!dmg.Precision) dmg.AddModifier(bonus, Fact);
            }
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt) { }
    }

    // Flat damage bonus per rank for spell-inflicted damage of a specific energy type
    // (negative energy...). Applies only to the matching energy entries of the bundle.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a0c")]
    public class EnergyTypeDamageBonusPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public DamageEnergyType EnergyType;
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            var context = evt.Reason.Context;
            if (context?.SourceAbility == null) return;
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            foreach (var dmg in evt.DamageBundle)
            {
                if (dmg is EnergyDamage e && e.EnergyType == EnergyType && !dmg.Precision)
                {
                    e.AddModifier(bonus, Fact);
                }
            }
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt) { }
    }

    // Attack roll bonus per rank against the owner's favored enemies. Same
    // UnitPartFavoredEnemy check as ACBonusAgainstFavoredEnemyPerRank, but on the
    // initiator side of RuleCalculateAttackBonus (pattern proven by PrestigePlus
    // ViciousAimComp). UntypedStackable so it stacks with the ranger's own
    // FavoredEnemy-descriptor class bonus.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a0d")]
    public class AttackBonusAgainstFavoredEnemyPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateAttackBonus>, IRulebookHandler<RuleCalculateAttackBonus>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleCalculateAttackBonus evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            var part = Owner.Get<UnitPartFavoredEnemy>();
            if (part == null) return;
            var target = evt.Target;
            if (target == null) return;
            if (FavoredEnemyCheck.IsFavored(part, target))
            {
                evt.AddModifier(bonus, Fact, ModifierDescriptor.UntypedStackable);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonus evt) { }
    }

    // Damage bonus per rank against the owner's favored enemies: a temporary
    // AdditionalDamage stat modifier scoped to the single attack (pattern proven by
    // TTT-Core DamageBonusOrderOfCockatriceTTT and PrestigePlus ViciousAimComp).
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a0e")]
    public class DamageBonusAgainstFavoredEnemyPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleAttackWithWeapon>, IRulebookHandler<RuleAttackWithWeapon>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            var part = Owner.Get<UnitPartFavoredEnemy>();
            if (part == null) return;
            var target = evt.Target;
            if (target == null) return;
            if (FavoredEnemyCheck.IsFavored(part, target))
            {
                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalDamage.AddModifier(bonus, Fact, ModifierDescriptor.UntypedStackable));
            }
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt) { }
    }

    // Dodge AC bonus per rank against attackers carrying one of the listed buffs
    // CAST BY THE OWNER (slayer studied target: the vanilla buff sits on the studied
    // enemy with the slayer as its caster — same-GUID blueprint as Kingmaker, which
    // is exactly the fact ZFC's ACBonusAgainstFactOwner checked; we additionally
    // require the caster to be the owner so two slayers don't share marks).
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a0f")]
    public class ACBonusAgainstCasterBuffPerRank : UnitFactComponentDelegate,
        ITargetRulebookHandler<Kingmaker.RuleSystem.Rules.RuleCalculateAC>,
        IRulebookHandler<Kingmaker.RuleSystem.Rules.RuleCalculateAC>,
        ISubscriber, ITargetRulebookSubscriber
    {
        public BlueprintBuffReference[] m_Buffs = new BlueprintBuffReference[0];
        public int Divisor = 1;

        public void OnEventAboutToTrigger(Kingmaker.RuleSystem.Rules.RuleCalculateAC evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            var attacker = evt.Initiator;
            if (attacker == null) return;
            // Plain nested loops, no LINQ/closures: this walks the ATTACKER's full buff
            // list on every AC calculation in combat, typically the largest collection
            // among this mod's hot-path checks.
            bool marked = false;
            foreach (var buff in attacker.Buffs.Enumerable)
            {
                if (buff.Context?.MaybeCaster != Owner) continue;
                for (int i = 0; i < m_Buffs.Length; i++)
                {
                    if (m_Buffs[i].Get() == buff.Blueprint) { marked = true; break; }
                }
                if (marked) break;
            }
            if (marked)
            {
                evt.AddModifier(bonus, Fact, ModifierDescriptor.Dodge);
            }
        }

        public void OnEventDidTrigger(Kingmaker.RuleSystem.Rules.RuleCalculateAC evt) { }
    }

    // Flat healing bonus per rank for a whitelisted set of abilities (paladin lay on
    // hands). RuleHealDamage.AdditionalBonus is the native flat-bonus channel — the
    // engine folds it in as (Bonus + AdditionalBonus) before multipliers, so this
    // stacks additively with the ability's own healing exactly like the tabletop
    // "+1/2 hit point to lay on hands" wording. SelfOnly implements the tiefling
    // variant's "but only when the paladin uses that ability on herself" by comparing
    // the heal target to the owner, rather than hard-coding the self-cast ability.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a11")]
    public class HealBonusForAbilitiesPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public BlueprintAbilityReference[] m_Abilities = new BlueprintAbilityReference[0];
        public bool SelfOnly;
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleHealDamage evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            if (SelfOnly && evt.Target != Owner) return;
            var srcAbility = evt.Reason.Context?.SourceAbility;
            if (srcAbility == null) return;
            for (int i = 0; i < m_Abilities.Length; i++)
            {
                var bp = m_Abilities[i].Get();
                if (bp != null && (bp == srcAbility || bp == srcAbility.Parent))
                {
                    evt.AdditionalBonus.Add(ModifierDescriptor.UntypedStackable, bonus);
                    return;
                }
            }
        }

        public void OnEventDidTrigger(RuleHealDamage evt) { }
    }

    // Natural armor bonus per rank that applies only while a qualifying form is
    // active: either one of the explicitly listed buffs (alchemist mutagen /
    // cognatogen) or any polymorph buff (druid wild shape). Polymorph is detected by
    // the presence of the native Polymorph buff component rather than an enumerated
    // form list, so every wild shape form — including ones added by other mods —
    // counts. Blueprint lookups are cached per blueprint guid because this runs on
    // every AC calculation in combat.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a12")]
    public class NaturalACWhileTransformedPerRank : UnitFactComponentDelegate,
        ITargetRulebookHandler<Kingmaker.RuleSystem.Rules.RuleCalculateAC>,
        IRulebookHandler<Kingmaker.RuleSystem.Rules.RuleCalculateAC>,
        ISubscriber, ITargetRulebookSubscriber
    {
        public BlueprintBuffReference[] m_Buffs = new BlueprintBuffReference[0];
        public bool AnyPolymorph;
        public int Divisor = 1;

        // blueprint guid -> does this buff blueprint carry a Polymorph component.
        // Static, never serialized: pure lookup memoization of immutable blueprint data.
        static readonly Dictionary<BlueprintGuid, bool> PolymorphCache = new();

        static bool IsPolymorphBuff(BlueprintBuff bp)
        {
            if (bp == null) return false;
            if (PolymorphCache.TryGetValue(bp.AssetGuid, out var known)) return known;
            bool isPolymorph = false;
            foreach (var comp in bp.ComponentsArray)
            {
                if (comp is Kingmaker.UnitLogic.Buffs.Polymorph) { isPolymorph = true; break; }
            }
            PolymorphCache[bp.AssetGuid] = isPolymorph;
            return isPolymorph;
        }

        bool IsTransformed()
        {
            foreach (var buff in Owner.Buffs.Enumerable)
            {
                var bp = buff.Blueprint;
                if (bp == null) continue;
                if (AnyPolymorph && IsPolymorphBuff(bp)) return true;
                for (int i = 0; i < m_Buffs.Length; i++)
                {
                    if (m_Buffs[i].Get() == bp) return true;
                }
            }
            return false;
        }

        public void OnEventAboutToTrigger(Kingmaker.RuleSystem.Rules.RuleCalculateAC evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            if (!IsTransformed()) return;
            evt.AddModifier(bonus, Fact, ModifierDescriptor.NaturalArmor);
        }

        public void OnEventDidTrigger(Kingmaker.RuleSystem.Rules.RuleCalculateAC evt) { }
    }

    // Restores extra points of a resource on rest, on top of whatever the class's own
    // rest logic already restored. WOTR has no "points regained per day" field on
    // BlueprintAbilityResource (only the maximum), so the arcanist's partial reservoir
    // refill is driven by rest triggers — this rides the same native IUnitRestHandler
    // event AddRestTrigger uses and simply tops the pool up further. Restore() clamps
    // to the maximum, so if a class ever refills fully this is harmlessly inert.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a13")]
    public class RestoreResourceOnRestPerRank : UnitFactComponentDelegate,
        IUnitRestHandler, IGlobalSubscriber, ISubscriber
    {
        public BlueprintAbilityResourceReference m_Resource;
        public int Divisor = 1;

        public void HandleUnitRest(UnitEntityData unit)
        {
            if (unit?.Descriptor != Owner) return;
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            var resource = m_Resource?.Get();
            if (resource == null) return;
            Owner.Resources.Restore(resource, bonus);
        }
    }

    // +1 caster level per rank when casting one of the character's patron spells.
    // A witch's patron is a progression that grants its spells through AddKnownSpell
    // components, so the patron -> spell mapping is read out of the patron
    // progressions themselves at install time (see FavoredClasses.PatronSpells)
    // rather than hard-coded. The hot path costs one hash lookup for the overwhelming
    // majority of casts, which are not patron spells at all.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a14")]
    public class PatronSpellCasterLevelPerRank : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public int Divisor = 1;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            int bonus = Fact.GetRank() / Divisor;
            if (bonus <= 0) return;
            var spell = evt.Spell;
            if (spell == null) return;
            // Cheap reject first: almost every cast is not a patron spell anywhere.
            if (!FavoredClasses.AnyPatronSpell.Contains(spell.AssetGuid)) return;
            // It is SOME patron's spell — now confirm it belongs to THIS character's
            // patron, so a witch does not get the bonus for another patron's spell she
            // happens to know by other means.
            foreach (var entry in FavoredClasses.PatronSpells)
            {
                if (!entry.Value.Contains(spell.AssetGuid)) continue;
                var patron = entry.Key.Get();
                if (patron != null && Owner.Progression.m_Progressions.ContainsKey(patron))
                {
                    evt.AddBonusCasterLevel(bonus, ModifierDescriptor.UntypedStackable);
                    return;
                }
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }

    // Keeps the referenced feature present on the owner's animal companions while
    // this fact is active. Grants are idempotent and value-free: the companion-side
    // feature computes its own magnitude from the MASTER's counter rank (native
    // MasterFeatureRank config), so ranks never need syncing. Grants route through
    // FeatureCollection.AddFact, so the player-faction gate covers NPC companions.
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("9c41d35e668d4dfd8f7f2f8a3b1c5a10")]
    public class GrantFeatureToPetsWhileActive : UnitFactComponentDelegate, IPartyHandler, IGlobalSubscriber, ISubscriber
    {
        public BlueprintUnitFactReference m_Feature;

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            foreach (var petRef in Owner.Pets)
            {
                TryGrant(petRef.Entity);
            }
        }

        // Mirrors OnTurnOn: if the owner's counter is ever deactivated without being
        // fully removed (e.g. TabletopTweaks-Base's manual feature-suppression
        // toggle), the pet must not keep the granted feature indefinitely.
        public override void OnTurnOff()
        {
            base.OnTurnOff();
            foreach (var petRef in Owner.Pets)
            {
                TryRevoke(petRef.Entity);
            }
        }

        void TryGrant(UnitEntityData pet)
        {
            if (pet == null) return;
            var bp = m_Feature?.Get() as BlueprintFeature;
            if (bp == null || pet.Descriptor.HasFact(bp)) return;
            var source = (FeatureSource)BlueprintCore.Utils.BlueprintTool.Get<BlueprintProgression>(FavoredClasses.SourceMarkerGuid);
            pet.Descriptor.AddFact<Feature>(bp)?.SetSource(source, 1);
        }

        void TryRevoke(UnitEntityData pet)
        {
            if (pet == null) return;
            var bp = m_Feature?.Get() as BlueprintFeature;
            if (bp == null) return;
            var fact = pet.Descriptor.Facts.m_Facts.FirstOrDefault(f => f.Blueprint == bp);
            if (fact != null) pet.Descriptor.Facts.Remove(fact);
        }

        public void HandleAddCompanion(UnitEntityData unit) => TryGrantIfOurPet(unit);

        // Also handled, and not redundant with OnTurnOn. Verified symptom: a save in which the
        // master held Companion Hit Points at rank 3 while the pet carried no companion feature
        // at all, so neither OnTurnOn nor HandleAddCompanion had reached it. Which of the two
        // missed is not established — OnTurnOn walking an empty Owner.Pets before the pet is
        // linked on load, and a companion restored rather than added, are both consistent with
        // it. This covers the remaining entry point either way; TryGrant is idempotent, so an
        // extra call costs nothing.
        public void HandleCompanionActivated(UnitEntityData unit) => TryGrantIfOurPet(unit);

        void TryGrantIfOurPet(UnitEntityData unit)
        {
            if (unit != null && unit.IsPet && unit.Master == Owner)
            {
                TryGrant(unit);
            }
        }

        public void HandleCompanionRemoved(UnitEntityData unit, bool stayInGame) { }

        public void HandleCapitalModeChanged() { }
    }
}
