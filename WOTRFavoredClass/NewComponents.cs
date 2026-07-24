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
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
                var comp = evt.Spell.GetComponents<SpellComponent>().FirstOrDefault();
                if (comp != null) match = comp.School == School;
            }
            if (match)
            {
                evt.AddBonusCasterLevel(bonus, ModifierDescriptor.UntypedStackable);
            }
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
                var comp = evt.Spell.GetComponents<SpellComponent>().FirstOrDefault();
                if (comp != null) match = comp.School == School;
            }
            if (match)
            {
                evt.AddBonusDC(bonus, ModifierDescriptor.UntypedStackable);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
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
            bool favored = part.Entries.Any(e =>
                e.CheckedFeatures.Any(p => p != null && attacker.Descriptor.HasFact(p)));
            if (favored)
            {
                evt.AddModifier(bonus, Fact, ModifierDescriptor.Dodge);
            }
        }

        public void OnEventDidTrigger(Kingmaker.RuleSystem.Rules.RuleCalculateAC evt) { }
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
            return race != null && m_Races.Any(r => r.Get() == race);
        }

        public override string GetUITextInternal(UnitDescriptor unit)
        {
            return "Race: " + string.Join(" or ", m_Races.Select(r => r.Get()?.Name ?? "?"));
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
            var bp = m_Fact?.Get();
            int rank = 0;
            if (bp != null)
            {
                var fact = unit.Facts.m_Facts.FirstOrDefault(f => f.Blueprint == bp);
                rank = fact?.GetRank() ?? 0;
            }
            return $"Progress toward next bonus: {rank % Step}/{Step}";
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

        static int RankOf(UnitDescriptor unit, BlueprintUnitFactReference bpRef)
        {
            var bp = bpRef?.Get();
            if (bp == null) return 0;
            var fact = unit.Facts.m_Facts.FirstOrDefault(f => f.Blueprint == bp);
            return fact?.GetRank() ?? 0;
        }

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
            return fact != null ? fact.GetRank() / System.Math.Max(1, Divisor) : 0;
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
            bool match = m_Abilities.Any(r =>
            {
                var bp = r.Get();
                return bp != null && (bp == srcAbility || bp == srcAbility.Parent);
            });
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
            bool favored = part.Entries.Any(e =>
                e.CheckedFeatures.Any(p => p != null && target.Descriptor.HasFact(p)));
            if (favored)
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
            bool favored = part.Entries.Any(e =>
                e.CheckedFeatures.Any(p => p != null && target.Descriptor.HasFact(p)));
            if (favored)
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
            bool marked = attacker.Buffs.Enumerable.Any(b =>
                b.Context?.MaybeCaster == Owner &&
                m_Buffs.Any(r => r.Get() == b.Blueprint));
            if (marked)
            {
                evt.AddModifier(bonus, Fact, ModifierDescriptor.Dodge);
            }
        }

        public void OnEventDidTrigger(Kingmaker.RuleSystem.Rules.RuleCalculateAC evt) { }
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

        void TryGrant(UnitEntityData pet)
        {
            if (pet == null) return;
            var bp = m_Feature?.Get() as BlueprintFeature;
            if (bp == null || pet.Descriptor.HasFact(bp)) return;
            var source = (FeatureSource)BlueprintCore.Utils.BlueprintTool.Get<BlueprintProgression>(FavoredClasses.SourceMarkerGuid);
            pet.Descriptor.AddFact<Feature>(bp)?.SetSource(source, 1);
        }

        public void HandleAddCompanion(UnitEntityData unit)
        {
            if (unit != null && unit.IsPet && unit.Master == Owner)
            {
                TryGrant(unit);
            }
        }

        public void HandleCompanionActivated(UnitEntityData unit) { }

        public void HandleCompanionRemoved(UnitEntityData unit, bool stayInGame) { }

        public void HandleCapitalModeChanged() { }
    }
}
