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

## Deferred (with reasons)

| Bonus | Reason |
|---|---|
| Kineticist internal buffer | The resource does not exist in WOTR (burn was reworked) |
| Eldritch Scion eldritch pool (÷4) | The resource exists (`EldritchPoolResourse` `17b6158d...`) and would be trivial via `IncreaseResourceAmountPerRank` — but per the user's decision the scion gets only bonus arcana, not the pool |
| Ravener Hunter / Winter Witch / Unlettered Arcanist — separate archetype variants of bonus known spell (own spellbook instead of the base one) | There is no point making separate FCB entries for the archetypes as such: Ravener Hunter is not present in vanilla WOTR (only in the ExpandedContent mod, unverified), Winter Witch is a prestige class (it does not change the base list), and Unlettered Arcanist is already excluded from the base Arcanist entry instead of getting a variant |
| Insinuator Greed | The archetype is absent from MCE |
| Psychic/Occultist/Investigator/Spiritualist/Summoner — everything | The classes are absent from WOTR and the installed mods |

## Known fidelity gaps

- Acid Spell Damage (Dwarf/Oread Sorcerer): ZFC used `Acid | Ground`; the Ground descriptor
  does not exist in WOTR (it is a CotW custom) — only the Acid component was ported.
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
