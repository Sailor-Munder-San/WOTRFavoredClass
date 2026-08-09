# Favored class bonus matrix for WOTR

Vanilla playable WOTR races: Human, Elf, Half-Elf, Dwarf, Gnome, Halfling, Half-Orc,
Aasimar, Tiefling, Oread, Dhampir, Kitsune (12).

## Races from Ebon's Content Mod (installed)

The races "lost" relative to the KM version are NOT gone: EbonsContentMod adds them as
full chargen races with their OWN GUIDs (not to be confused with the hidden vanilla monster
blueprints of the same names — Goblin/Fetchling/Duergar exist twice in the data; for
`PrerequisiteRaceAny` you need Ebon's mod GUID, since that is what ends up in
`unit.Progression.Race`).

| Race | GUID (EbonsContentMod) | Used by bonuses |
|---|---|---|
| Goblin | `93fb4931c7b34ec4a023f429e3b16239` | bardic performance, alchemist fire res |
| Fetchling | `29454c0ec53946c48cd34bcad4311ab7` | cold/elec res, negative dmg, studied dodge, companion DR, arcane pool |
| Hobgoblin | `be0a8e971f8e4ab6975154dade7a2446` | bombs, grapple/trip, negative dmg, favored enemy bonus, concentration |
| Drow | `5d357ab2ba684b76b7f13e8f3fe441c4` | disarm, concentration, teamwork feat, cruelty, fervor |
| Suli | `d5398269cc1442d7802469cbe7fdf151` | 4× energy res (ranger), arcane pool |
| Duergar | `ac2584f867f24c8499b8c77572dd4a61` | judgment |
| Ganzi | `14be0c2967a842febd853380ad785ce5` | Oracle bonus known spell (enchantment school) |

Ebon's other races (Sylph, Undine, Svirfneblin, Samsaran, Strix, Ifrit, Changeling, Kuru,
Vishkanya, Shabti, Android, Skinwalker, Orc, Rougarou, Nagaji, Mongrel, Ascending Succubus)
had no entries in ZFC — bonuses for them would be homebrew, so they are out of scope for v1.

## Third-party classes (installed)

| Class | GUID | Source |
|---|---|---|
| Swashbuckler | `338abf2723c14c1ab0f17cd7e3020444` | Swashbuckler mod (panache `ac63bfcf...`, charmed life `e6ad4ad4...`) |
| Antipaladin | `8939eff25a0a4b77ad1ab6be4c760a6c` | MicroscopicContentExpansion (cruelty selection `402fccae...` from its Blueprints.json) |

## Porting status (current as of Wave 8)

The full ledger of the original's 91 bonuses is the published "Favored Class Bonus Ledger"
artifact. Summary by wave:

- **Wave 1 (universal)**: HP, skill rank — done (v0.1.0).
- **Wave 2 (racial PnP)**: speed (barb/bloodrager/monk), dodge vs favored enemies, natural AC,
  necromancy CL, enchantment DC + Magical Tail (kitsune), warpriest combat feat — done
  (v0.3–0.4 + monk in Wave 5).
- **Wave 3 (resource pools)**: rage, bloodrage, bardic/skald performance, bombs, ki,
  arcane pool, arcane reservoir — done. Plus Wave 5: judgment (Duergar), panache,
  charmed life (Swashbuckler).
- **Wave 4 (1/6 wrappers)**: rogue talent, witch hex, arcanist exploit, shaman hex,
  slayer talent, wild talent, magus arcana — done. Plus Wave 5: teamwork feat (Inquisitor;
  Drow ÷4, RAW Blood of Shadows p.15 — corrected from the ÷6 inherited from ZFC; Half-Elf and
  Halfling were added later as a substitute for their own unimplementable mechanic, see the
  fidelity gaps), cruelty (Antipaladin/Drow, ÷4).
- **Wave 5**: concentration ×4 (the native RuleCheckConcentration/AddBonusConcentration DO
  exist in WOTR), CMB grapple/trip + disarm (native CMBBonusForManeuver),
  kineticist blast damage ×2 (blast GUIDs are identical to KM), acid/fire/negative spell damage,
  favored enemy attack/damage, energy resistance ×7, studied target dodge (the buff has the same
  GUID as in KM), companion DR + saves (MasterFeatureRank plus granting to the pet via
  IPartyHandler).
- **Wave 6 (bonus known spells)**: 12 entries at ÷2. The mechanism is
  `BlueprintParametrizedFeature` + `LearnSpellParametrized` (native, same as ZFC) inside the
  same wrapper pattern (ProgressGuid + RewardSelectionGuid) — no new component was needed.
  Alchemist/Bard/Inquisitor/Oracle/Shaman/Sorcerer/Witch/Skald/Arcanist use the class's own
  list; Oracle/Ganzi (enchantment) and Sorcerer/Goblin (fire) use custom filtered lists built
  on the fly from the native Wizard/Cleric lists (`BuildFilteredSpellList`, the same technique
  PrestigePlus and EbonsContentMod use). Wizard is a single entry with a generic branch plus
  7 school branches (Thassilonian Specialist, gated on
  `FeatureReplaceSpellbookRefs.ThassilonianXFeature`): the player sees one choice, and
  prerequisites decide which of the 8 branches is available. Arcanist excludes Unlettered
  Arcanist / Nature Mage / Magic Deceiver (`AddPrerequisiteNoArchetype`; all three confirmed
  to have their own Spellbook/SpellList via the native SpellbookRefs/SpellListRefs). Ravener
  Hunter is not excluded (confirmed with the user — the archetype does not exist in vanilla WOTR).

- **Wave 7**: Lay on Hands ×2 (Paladin), Wild Shape natural AC (Druid), the mutagen condition
  for Dwarf Alchemist natural AC, Eldritch Scion arcana, Channel Energy, Harm Undead, Fervor.
  Cross-checked against the README and `Custom/*.json` of the original mod
  (github.com/Holic75/KingmakerFavoredClass) — those JSON files sit in our clone at
  `reference/ZFavoredClass-source/ZFavoredClass/Custom/` and hold the exact
  divisors/classes/races for the "third-party" bonuses that are absent from `Core.cs`.
  Channel Energy and Fervor were initially implemented incorrectly (as extra uses per day) —
  they are in fact a bonus to the MAGNITUDE of the healing/damage; fixed.
  Two new components: `HealBonusForAbilitiesPerRank` (native
  `RuleHealDamage.AdditionalBonus.Add` — a flat addition to healing, filtered by ability
  blueprint; `SelfOnly` compares `evt.Target` against the owner, which is more precise than
  ZFC's hard-coded self-ability) and `NaturalACWhileTransformedPerRank` (AC only while the
  required form is active). The damage half of lay on hands ("to heal **or harm**") is covered
  by reusing the existing `AbilityDamageBonusPerRank` — no new code was needed. Wild shape is
  detected by the presence of the native `Kingmaker.UnitLogic.Buffs.Polymorph` component on an
  active buff rather than by a list of forms, so it also works with forms added by other mods.
  Eldritch Scion in WOTR is an **archetype** of magus (`d078b2ef...`; the identically named
  class `f5b8c63b...` is the hidden spellbook helper and stays in `ExcludedClasses`), so the
  entry is attached to the Magus class with `PrerequisiteArchetypeLevel`, mirrors the
  Charisma-based `EldritchMagusArcanaSelection` (`d4b54d9b...`), while the regular magus arcana
  and arcane pool entries got `PrerequisiteNoArchetype` against the scion.

- **Wave 8**: fixed Favored Enemy (Hobgoblin Ranger) — as in the original it is now a choice of
  ONE already-taken favored enemy (+1 to it, maximum +1 per enemy) rather than a bonus against
  all of them at once; the reward pool is built from the vanilla `FavoriteEnemySelection`
  (`16cc2c93...`), one feature per enemy type, with `PrerequisiteFeature` on the corresponding
  favored enemy plus a bonus against Instant Enemy, as in the original. The old counter
  `...b55` was converted into a hidden stub. Added Arcane Reservoir regen
  (Gnome Arcanist ÷6) and Patron Spells CL (Halfling Witch ÷4) — both were previously listed
  as unimplementable, see below.
  - Regen: `BlueprintAbilityResource` genuinely has no "amount restored" field, but restoration
    on rest goes through the `IUnitRestHandler` event (the same one the native `AddRestTrigger`
    uses), and `UnitAbilityResourceCollection.Restore(bp, amount)` is public. The new
    `RestoreResourceOnRestPerRank` simply tops the resource up beyond whatever the class
    restored. `Restore` clamps to the maximum, so if the arcanist's reservoir does refill
    completely, the bonus is harmless (it just does nothing) rather than broken.
  - Patron CL: the patron→spells map is built at Install by walking the 15 patron
    `BlueprintProgression`s and their `AddKnownSpell` components — that is, the patron itself
    is the source of truth rather than a hard-coded list (patrons from other mods are picked up
    automatically). On the hot cast path there is first a single hash lookup against the union
    of all patron spells (almost always an immediate exit), and only for an actual patron spell
    is it checked which patron it belongs to.

- **Wave 9**: where the favored class pick itself lives, and half-elf Multitalented.
  The pick lives on `BasicFeatsProgression` (`5b72dd2c...`) level 1 — a progression every
  character gets, with no class restriction, so the entry fires exactly once during chargen.
  Its **position** in chargen comes from `FeatureGroup.Racial`, not from what holds it: the
  card appears right after the racial heritage step regardless. A half-elf additionally carries
  Multitalented, which does hang off the race, because that one genuinely is a racial trait.
  - Hanging the generic pick off the races was tried and reverted. It placed the card
    correctly, but the group alone already does that — the vanilla background selection proves
    it, sitting in the same phase with its own group while not being a race feature. Being a
    race feature cost two things: the race screen listed "Favored Class" as a trait of every
    race before anything was chosen, and `SelectRace.Apply` handed the selection to every NPC
    of a playable race. A favored class is not a racial trait and should not read as one.
  - Both selections carry `FeatureGroup.Racial`, which is what actually decides *where* in
    chargen they appear: `CharGenFeatureSelectorPhaseVM.GetFeaturePriority` maps `Racial` and
    the heritage groups to the `RaceFeatures` phase and everything else to `Features`, which
    sits after ability scores and skills. Attaching to the race is not enough on its own — an
    ungrouped selection still ends up stranded at the far end of chargen. Within one phase the
    order follows the level-up state index, so appending the main pick before Multitalented
    gives: racial heritage, favored class, Multitalented.
  - It got there in two steps. Originally it was on the level-1 entry of *every* class, which
    meant it came back whenever a character multiclassed — and since every option was gated on
    the "already chosen" marker, that repeat card had no selectable option at all. Moving it to
    `BasicFeatsProgression` (`5b72dd2c...`, the anchor the original mod uses) fixed the repeat,
    because that progression has no class restriction and so fires once at character level 1 —
    but it is processed late in chargen, far from the race step, while Multitalented (hanging
    off the race) appeared immediately after the race was picked. Races are the better anchor
    for both, and `CharacterRaces` covers races added by other mods while excluding monster
    and pet races.
  - With the pick offered only once, the marker prerequisites became unnecessary and were
    removed — which is also what makes a second pick possible. Taking the same class twice is
    still impossible without any explicit check: a `BlueprintProgression` is not an
    `IFeatureSelection`, so `MeetsPrerequisites` rejects it as soon as its rank reaches `Ranks`
    (1). `ZFCWNoFavoredClass` got `Ranks = 2` so a half-elf can decline both slots.
  - Multitalented is a second `BlueprintFeatureSelection` carrying the same options, appended
    to the half-elf race's `m_Features` rather than to a progression — races grant their
    features during chargen, exactly when the second choice is due, and this keeps the option
    off every other race without a race prerequisite. Same technique as the original, which
    copies its favored class selection onto the half-elf.
  - Pets are excluded at the source. An animal companion levels a real character class and
    gets the basic feat progression too, so it was being offered a favored class of its own.
    A prefix on `LevelUpState.AddSelection` drops our selection outright when
    `LevelUpState.Unit.Unit.IsPet`, so no dead card is queued at all.
  - Which classes may be favored is now decided by `IsFavoredClassCandidate`, testing the
    same native flags the game's own class-selection screen uses (`CharGenClassPhaseVM`
    filters on `HideInUI`): skip `PrestigeClass`, `IsMythic` and `HideInUI`. Classes added by
    other mods are judged by that rule rather than needing to be listed. The explicit
    `ExcludedClasses` list is kept as a backstop for the pet/summon/monster/technical classes
    that actually turn up on player-controlled units. Previously only `PrestigeClass` and
    Eldritch Scion were filtered, so the documented rule below was not actually enforced.
  - A pick is re-sourced to its own class. The sheet's class panel is built from
    `GetClassProgressions(cls)`, which keeps a fact only when `Feature.GetSourceClass()` is
    that class, and `GetSourceClass` just reads the fact's source. While the selection lived
    on each class's level-1 entry the source was that class; hanging it off the basic feat
    progression made every pick share one unrelated source, so picks landed in the wrong
    place and a half-elf's two picks collapsed onto one. The `SelectFeature.Apply` postfix
    now calls `SetSource` with the class the picked progression belongs to.
  - Placement within a class block is forced. `GetClassProgressions` returns progressions in
    the order their facts were added to the unit, so the favored class block sat above or
    below the class features depending on which came first — a half-elf picks both favored
    classes during chargen but levels the second class much later, so for that class the
    favored class fact predated the class itself. A postfix sorts this mod's progressions to
    the end of that array, leaving everything else in place.

- **Performance note — never call `RemoveModifiers` from inside a stat recalculation.** The
  patch that strips WOTR's automatic favored class hit points hangs off
  `ModifiableValueHitPoints.UpdateInternalModifiers`, and used to call the public
  `RemoveModifiers`. That method ends in `UpdateValue()`, which is exactly the method already
  running — `UpdateValue` calls `UpdateInternalModifiers()` first and `ApplyModifiersFiltered()`
  afterwards. So every hit point update kicked off a second, nested recalculation of the value,
  of every dependent `ModifiableValue`, and of the dependent facts and components, for every
  party member, every time anything touched their stats. The patch now removes the entry from
  `ModifierList` directly and calls `PrepareForRemoval` on each modifier — the same steps
  `RemoveModifiers` performs, minus the trailing `UpdateValue()` — so the pass already in flight
  picks the removal up. Verified by IL inspection that `UpdateInternalModifiers` has no caller
  other than `UpdateValue` for this type.

- **Wave 10**: 16 new entries filling gaps against the tabletop favored class lists.
  Bomb damage (Half-Orc, Tiefling alchemists); companion hit points (Goblin/Half-Orc druids and
  hunters) and companion natural armour (Oread ranger); druid and paladin energy resistance, four
  entries each, following the Suli ranger precedent; skald concentration (Gnome); warpriest
  blessings (Dwarf, Elf, Nagaji); good-descriptor caster level (Aasimar sorcerer); and two more
  filtered known-spell lists — Drow sorcerer and Kitsune shaman. Companion DR gained the Druid
  class and the Svirfneblin race; rogue talent gained Changeling, Kitsune and Samsaran; panache
  gained Kitsune. Four Ebon races are newly referenced: Svirfneblin, Samsaran, Changeling, Nagaji.
  - One new component, `IncreaseSpellDescriptorCasterLevelPerRank`. The existing school-based
    component cannot express it: a descriptor like Good is orthogonal to the school.
  - **Descriptor gap — partly wrong, corrected below.** This originally read that
    `SpellDescriptor` has no `Chaotic`, `Pain`, `Shadow` or `Darkness`, and three bonuses were
    written off on that basis. Only `Pain`, `Shadow` and `Darkness` are genuinely absent. The
    chaotic one is spelled **`Chaos`**, and searching for "Chaotic" is what missed it — see the
    Wave 13 note. The Drow sorcerer list is still curse-or-evil rather than the tabletop
    "curse, evil, or pain", since `Pain` really does not exist.
  - Bomb damage lists ten bomb abilities explicitly. Discovery bombs are separate root abilities
    rather than children of the standard bomb, so a bomb added by another mod would not be
    covered — the same scoping as the kineticist blast lists.
  - Companion hit points is one entry covering Druid+Hunter × Goblin+Half-Orc. The cross product
    also grants Druid/Half-Orc and Hunter/Goblin, which the tabletop lists do not, because a
    single pet-side feature can only read one master counter. Companion DR already worked this
    way.

- **Energy resistance was rank SQUARED, from Wave 5 until v0.1.5 — fixed.** The native
  `AddDamageResistanceEnergy` multiplies its `Value` by the fact's rank on its own
  (`CalculateValue` is `Fact.GetRank() * Value`), and all 15 entries were also passing
  `ContextValues.Rank()` as that value. Resistance therefore came out as rank², so a fourth pick
  read **16** instead of 4. One pick gives `1 * 1 == 1`, which is why it survived every release
  and a full playbook run unnoticed. Each entry now passes a flat `ContextValues.Constant(1)`,
  and the redundant `ContextRankConfig` is gone.
  - The behaviour is **not** shared across the family, so it cannot be assumed either way:
    `AddDamageResistancePhysical` (companion DR), `AddContextStatBonus` and
    `AddCMBBonusForManeuver` all take the value as-is and do need the explicit rank. All three
    were checked against the assembly and are correct.
  - Saves need no migration: only the computed value changes, the fact and its rank are
    untouched, so an existing character's resistance simply drops to the correct number on load.
  - `AUDIT-PLAYBOOK.md` C4 is a new automated check for this bug class — it decompiles every
    native component we hand `ContextValues.Rank()` and fails if it finds an `EntityFact.GetRank`
    call inside.

- **Energy resistance folded into one card per class.** The four acid/cold/electricity/fire
  entries used to sit in the bonus list as four sibling cards, which is most of what a Suli
  ranger, Gnome druid or Human paladin saw when opening it. They are now children of a single
  "Energy Resistance" selection that unfolds on pick — the same native nesting wrapper mode
  already uses. Four folders: Fetchling (barbarian, sorcerer — two energies), Suli (ranger),
  Gnome (druid), Human (paladin). Card counts drop accordingly: paladin 7→4, druid 8→5,
  ranger 8→5, barbarian 4→3.
  - Children keep their GUIDs, ranks, components and tooltips untouched; only their position
    changes, so saves holding those facts stay valid. Each folder is a new blueprint
    (`…bd9`–`…bdc`) and carries no mechanics of its own — the resistance value still comes from
    the child's own rank, and the folder stays out of `BonusDisplays`.
  - The folder claims the list slot of its first child, so the order the defs table declares is
    preserved rather than the folders being appended at the end.
  - **Goblin alchemist fire resistance is deliberately NOT foldered**: it is the only energy
    option that race/class pair has, so a folder would be one extra click to reach a single
    choice. The rule applied is "fold only where one character actually faces a choice among
    the children".
  - Every other multi-entry family was checked against that rule and does not qualify. Bonus
    known spells, companion bonuses (DR / saves / hit points / natural armour) and Lay on Hands
    each have **disjoint race sets** within a class, so no character is ever offered two at
    once; the magus arcana pair is mutually exclusive by archetype. Energy resistance is the
    only family where folding removes clicks instead of adding one.

- **Wave 11: cavalier and shifter.** Both are vanilla WOTR classes (`3adc3439…`, `a406d6eb…`)
  that already had a favored class progression but not a single racial bonus, so the only options
  were the universal hit point and skill rank. Ten new entries cover 7 of their 11 tabletop lines;
  the classes with bonuses go from 26 to 28.
  - **Cavalier**: challenge damage (Aasimar ÷4, Dwarf ÷2), mount hit points (Elf, Goblin,
    Half-Orc) and mount speed (Gnome, Half-Elf, Nagaji, ÷5).
  - **Shifter**: base speed (Elf, ÷5), defensive instinct dodge vs Large or larger (Halfling ÷4),
    and energy resistance (Gnome) as four entries behind one folder card — the tabletop wording
    here *is* "acid, cold, electricity, or fire … then select a new type", so the folder matches
    the rule rather than merely tidying the list.
  - Two new components. `DamageBonusAgainstCasterBuffTargetPerRank` keys off
    `CavalierChallengeBuffTarget` (`4f021832…`), which the vanilla challenge puts on the foe with
    the cavalier as its caster; requiring that caster to be the owner stops two cavaliers feeding
    off each other's challenge, the same guard `ACBonusAgainstCasterBuffPerRank` already uses.
    `ACBonusAgainstLargerCreaturesPerRank` reads `State.Size` rather than `OriginalSize`, so an
    enlarged attacker counts.
  - The mount reuses the companion plumbing unchanged — a mount is an animal companion, so it is
    a pet and `GrantFeatureToPetsWhileActive` plus `MasterFeatureRank` apply as-is. Its counters
    are separate blueprints from the druid/hunter ones so the two scale from their own picks.
  - Mount speed is `floor(rank/5) × 5`, the same arithmetic as the character-side speed bonus:
    five picks are worth +5 feet and four are worth nothing, exactly as written.
  - **Orc** is newly referenced (`7088a348…`, EbonsContentMod) for the shifter claw entry.

- **Wave 11b: all four deferred cavalier/shifter lines, resolved by research.** Every one had
  been written off for the wrong reason — in each case the mechanism was assumed missing when it
  was only unlocated. Cavalier and shifter are now complete at 6 and 5 entries.
  - **Cavalier attack-of-opportunity damage (Halfling ÷2).** `RuleAttackWithWeapon` exposes
    `IsAttackOfOpportunity` as a plain public property, so no hook was needed. And no class-level
    substitution was needed either: the challenge's extra damage *is* the cavalier's class level,
    so "+1/2 to effective class level for this purpose" is just more damage on that attack. It
    became a flag on the component the Aasimar/Dwarf entries already use.
  - **Shifter claw damage (Orc ÷5).** `WeaponCategory` has a dedicated `Claw` value, and
    `RuleAttackWithWeapon.Weapon` is a public field whose `Blueprint.Category` names it — so the
    new `WeaponCategoryDamageBonusPerRank` filters on the weapon actually being swung rather than
    needing the seven `ShifterClawAbilityLevel*` activatables enumerated.
    - Narrowed to the ability itself, so an animal form's claws do **not** benefit. The modal is
      an activatable, and an activatable applies a buff — which lives in its `m_Buff` field, not
      in its components, which is why the first dump of `ShifterClawAbilityLevel*` looked empty
      and the approach was briefly written off. Keying on the seven `ShifterClawBuffLevel*` buffs
      is what makes "when using the shifter claws ability" exact.
    - Matching the weapon blueprint instead does **not** work: WOTR ships only one
      shifter-specific claw weapon (`ShifterClaw1d10x3`) against seven claw modals, so the lower
      tiers reuse the generic `Claw1dX` blueprints that animal forms also carry.
    - Both halves of the test are required. Category alone is too broad (an animal form's claws
      are claws); the buff alone is too broad the other way (the modal stays on while its owner
      swings a sword). Category is tested first — a field read against a collection walk — and
      most attacks in play are not claws, so the buff scan rarely runs.
  - **Cavalier banner bonus (Human, Kitsune ÷4).** Implemented by raising the number the banner
    already computes, instead of adding a bonus beside it — which is both what the tabletop line
    says and the only thing that works, since both banner effects apply their modifier with
    `ModifierDescriptor.Morale` and same-descriptor bonuses do not stack.
    - One `ContextRankConfig` on `CavalierBannerBuff` governs the whole banner:
      `SavingThrowBonusAgainstDescriptor` computes `Bonus.Calculate() + Value` and
      `ChargeAttackBonus` uses `Bonus` alone, and both read that same Default rank. So a single
      number drives saves-vs-fear and the charge attack together.
    - The rank resolves against the **caster**, not the owner: `ContextRankConfig.GetBaseValue`
      opens by taking `MechanicsContext.MaybeCaster`. The buff sits on the ally, but the caster
      is the cavalier — which is what makes the cavalier's own counter reachable from a buff on
      somebody else. (An earlier reading of this file claimed no such bridge existed, on the
      strength of `ContextRankBaseValueType` having no caster-feature-rank member. That was
      wrong: the bridge is the context, not the base value type.)
    - `ContextRankConfig_GetValue_BannerPatch` therefore postfixes `GetValue`. **Cost**:
      `GetValue` has exactly one caller, `MechanicsContext.RecalculateRanks`, so ranks are
      computed when a context is built or refreshed rather than per roll, and the guard is a
      reference comparison against the config cached at install. The vanilla blueprint is read,
      never written.
    - It reads the **effect feature's** rank, not the counter's. Built the other way round at
      first — counter read directly, divided inside the patch — which left it the only divisor
      entry in the mod with no delivery mechanism and no feature on the character sheet showing
      what had been earned. It now follows the same split as everything else: the counter holds
      picks, one rank of the effect is granted at each threshold, and the effect's rank is the
      bonus. Guarded by check C7.
  - **Shifter minor form (Human ÷3).** The tabletop bonus is minutes per day, and WOTR does not
    track the form in minutes — but it does meter the aspect as a per-day pool.
    `ShifterAspectResource` (`1b096f34…`) reads base 3 plus one per class level, confirming the
    minor form is resource-limited rather than an at-will toggle, so the bonus carries over onto
    the existing `IncreaseResourceAmountPerRank` as uses instead of minutes. Same pool and same
    effect in play; only the unit differs, and it is the game's unit rather than the book's.

- **Dual heritage moved out of the data and into the gate.** A half-race reaches its own bonuses
  and both parents' — Half-Orc gets Half-Orc/Human/**Orc**, Half-Elf gets Half-Elf/Human/**Elf**.
  That was previously applied by hand, by listing the halves beside Human in each entry's `Races`
  array, and an audit found **11 of 108 entries had missed one or both**: every paladin energy
  resistance, the warpriest Blessing, the cavalier banner and mount-hit-point entries, the shifter
  speed and minor form entries, and the whole Orc line — so a half-orc could not reach the orc
  shifter's claw bonus at all. The rule now lives in `RaceHeritage.Qualifies` behind
  `PrerequisiteRaceAny`, so entry data names the race the tabletop names and no entry can omit it.
  Existing explicit listings are left alone: several are genuine tabletop entries for the half
  race rather than heritage shorthand, and a redundant listing costs nothing. Guarded by check C5.

- **Wave 13: re-checked the "blocked by the engine" list — one was simply wrong.** The chaotic
  caster level bonus (Arcanist, Ganzi ÷2) is now implemented. `SpellDescriptor` does carry the
  value; it is spelled **`Chaos`**, and an earlier pass searched the enum for "Chaotic", found
  nothing and recorded the bonus as impossible. It needed no new component — the existing
  `IncreaseSpellDescriptorCasterLevelPerRank` took it unchanged.
  - Re-checking the whole enum turned up `Ground` as well, which the fidelity notes also claimed
    was absent. See the Acid Spell Damage entry.
  - `Pain`, `Shadow` and `Darkness` really are absent, and there is no subschool concept
    anywhere in the engine — but the shadow spells exist individually, so that family moved from
    "blocked" to "needs a curated list" rather than staying impossible.

- **Dropped the level-up re-anchoring postfix.** It moved the per-class bonus card inside
  `LevelUpState.Selections` so it sat directly behind the favored class pick. It never governed
  where the PICK itself appears — `FeatureGroup.Racial` does that, which is what places it beside
  the background step at level 1 — so all it bought was tidier ordering of the bonus card on
  later level-ups.
  - Against that it reordered `Selections` by removing and re-inserting an entry, while the UI
    remembers a phase's position in that list (`CharGenFeatureSelectorPhaseVM.IndexInLevelupState`).
    A phase counts as complete when its selection is answered **or** when nothing is selectable,
    and the level-up cannot be finished until every phase is complete. A phase pointed at the
    wrong entry, or an entry left without a phase, is therefore exactly how a pick becomes
    silently skippable — which is the reported bug where leaving the choice empty loses the
    increment for good. Cosmetic gain, real risk; removed on the user's call that placement after
    level 1 does not matter.
  - `BonusSelectionGuids`/`BonusSelectionAssetGuids` existed only to drive that postfix and are
    gone with it. The pet-exclusion prefix on the same patch stays — it is load-bearing.

- **The skippable favored class card: fixed with the engine's own flag, not a patch.** Leaving
  the card empty finished the level-up and lost the increment for good. Two wrong guesses came
  first — the re-anchoring postfix, then a Harmony postfix forcing the phase incomplete — before
  the actual mechanism turned up one call deeper.
  - `FeatureSelectionState.CanSelectAnything` opens with `Selection.IsObligatory()`, and
    `BlueprintFeatureSelection.IsObligatory()` is nothing but `return Obligatory` — a plain
    public bool on the blueprint. That is how vanilla marks a selection as one you must answer;
    there is no separate mechanism, and the level-up phase refuses to report itself complete
    while an obligatory card is unanswered.
  - So the fix is `.SetObligatory()` on the per-class bonus card and on the favored class pick.
    The Harmony patch written before this was found is gone, and the mod is back to 10 patches.
  - Declining is still possible — "No Favored Class" remains an option on the pick, and the bonus
    card always offers the universal hit point and skill rank. The choice just has to be made
    rather than skipped past.

## Engine facts this document relies on

Claims about what the engine does **not** have are the ones that rot silently, and getting one
wrong costs a bonus that was never actually impossible: `Chaos` was recorded as missing for
several waves because the search looked for `Chaotic`. So they are asserted here in a form check
C10 can verify against the live assembly rather than left as prose.

```engine-assert
SpellDescriptor absent: Pain, Shadow, Darkness
SpellDescriptor present: Chaos, Ground, Curse, Evil, Good
type absent: SpellSubSchool
```

## Deferred (with reasons)

| Bonus | Reason |
|---|---|
| Kineticist internal buffer | The resource does not exist in WOTR (burn was reworked) |
| Eldritch Scion eldritch pool (÷4) | The resource exists (`EldritchPoolResourse` `17b6158d...`) and would be trivial via `IncreaseResourceAmountPerRank` — but per the user's decision the scion gets only bonus arcana, not the pool |
| Ravener Hunter / Winter Witch / Unlettered Arcanist — separate archetype variants of bonus known spell (own spellbook instead of the base one) | There is no point making separate FCB entries for the archetypes as such: Ravener Hunter is not present in vanilla WOTR (only in the ExpandedContent mod, unverified), Winter Witch is a prestige class (it does not change the base list), and Unlettered Arcanist is already excluded from the base Arcanist entry instead of getting a variant |
| Wizard shadow/darkness known spell (Fetchling); Paladin saves vs shadow/darkness (Drow) | **Not blocked — reclassified.** WOTR has no subschool concept at all (`SpellComponent` carries only `School`, and there is no `SpellSubSchool` type), so "illusion (shadow)" cannot be filtered by tag. But the spells themselves exist as blueprints — `ShadowEvocation`, `ShadowConjuration`, both Greater variants and `Shades` — so the subschool is expressible as an explicit five-spell list, exactly how the bomb and kineticist blast lists already work. Needs a curated list plus, for the paladin, a save-bonus component keyed on it. |
| Rogue sneak attack damage vs outsiders (Tiefling) | `OutsiderType` (`9054d398...`) makes the target check trivial, but our damage component adds to *all* damage against that target, not to sneak attack specifically. Implementing it faithfully needs a hook into the sneak attack damage itself. |
| Wizard arcane school power uses (Drow, Elf, Gnome, Tiefling) | Each school has its own resource (`DivinationSchoolBaseResource`, `EnchantmentSchoolBaseResource`, …), so "select one school power" needs a per-school track like the Thassilonian wizard, not a single resource entry. |
| Cleric/Druid domain power uses (6 races) | Same shape: the choice is among the domains the character already took, so it needs the wrapper pattern used by the favored enemy pick. |
| Insinuator Greed | The archetype is absent from MCE |
| Psychic/Occultist/Investigator/Spiritualist/Summoner — everything | The classes are absent from WOTR and the installed mods |

## Known fidelity gaps

- Both favored class selections are granted through race blueprints, and race features are
  applied at character creation. Characters that already exist in a save therefore keep
  whatever they were given by the arrangement in force when they were made; the race-anchored
  pick, and the half-elf's second slot, apply to newly created characters only.

- Acid Spell Damage (Dwarf/Oread Sorcerer): ZFC used `Acid | Ground`; only the Acid half is
  ported. **The stated reason was wrong** — `SpellDescriptor.Ground` does exist in WOTR, so this
  is an open gap that can be closed rather than an engine limitation. Left alone for now only
  because widening a shipped bonus changes existing characters' numbers.
- The slayer talent wrapper mirrors only the base pool (level 2) out of three.
- Mutagen natural AC (Dwarf Alchemist) recognises only the vanilla set of mutagen/cognatogen
  buffs (31 GUIDs, including True Mutagen) — a mutagen from a third-party mod will not
  activate the condition.
- Wild Shape natural AC triggers on ANY polymorph buff rather than strictly on wild shape:
  in WOTR, druid transformations and polymorph spells use the same native `Polymorph`
  component, and there is no reliable way to tell them apart. In practice this coincides, but
  a druid under, say, Beast Shape from a scroll will also get the bonus.
- Acid Spell Damage (Sorcerer): the original lists only Dwarf, we have Dwarf + Oread — a
  deliberate addition beyond the original (the user's decision), not a gap.
- Companion Damage Reduction grants **DR/cold iron**, where the original and the tabletop text
  say DR/magic. By the level a companion has meaningful ranks of this, essentially everything
  attacking it already bypasses DR/magic, so the bonus was worth nothing in play. Cold iron
  keeps the same shape and numbers while actually mattering. Deliberate deviation (the user's
  decision), not a gap. The blueprint and its GUID are unchanged — it is the same bonus with a
  different bypass material, so existing saves keep working.
- Discrepancies between the original's README and its own `Core.cs` (we follow the code; the
  README is inaccurate): necromancy CL — README says "drow", the code says `dhampir` (we use
  Dhampir); lay on hands and wild shape AC — the README does not mention Half-Elf, the
  code/JSON include it (we include it); Ganzi enchantment — README says "oracle/wizard", the
  code combines wizard+cleric (we use wizard+cleric).
- Wizard bonus known spell does not check the opposition school (the specialist's forbidden
  school): that is a mechanism separate from Thassilonian Specialist (which merely forbids
  preparing spells of the banned school, without replacing the spellbook), so our feature may
  offer a regular specialist wizard a spell from his banned school. Not fixed.
- Inquisitor teamwork feat: the Drow RAW text (Blood of Shadows p.15) is "Gain 1/4 of a
  teamwork feat" (÷4); ZFC ported this as ÷6 and we corrected it to ÷4, following the source.
  In RAW, Half-Elf and Halfling get a different bonus ("+1/4 to the number of times per day the
  inquisitor can change her most recent teamwork feat") — WOTR has no feat-swapping mechanic,
  so they were deliberately given the same entry as Drow (the closest available equivalent)
  rather than a separate, textually exact but unimplementable version.

## Rules for excluding classes from the favored class selection

Do not show: PrestigeClass=true (those get the Favored Prestige Class feat), AnimalClass,
AnimalCompanionClass, MythicCompanionClass, all Mythic*, monster classes, technical classes.
EldritchScionClass is excluded in the original too (it is a magus subclass).
