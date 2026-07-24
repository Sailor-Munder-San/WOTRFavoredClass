# Favored Class Bonus Matrix for WOTR

Vanilla playable WOTR races: Human, Elf, Half-Elf, Dwarf, Gnome, Halfling, Half-Orc,
Aasimar, Tiefling, Oread, Dhampir, Kitsune (12).

## Races added by Ebon's Content Mod (installed)

The races that "left with the Kingmaker version" are NOT actually lost: EbonsContentMod adds
them back as full chargen-facing races with their OWN blueprint GUIDs (not to be confused with
the hidden vanilla monster blueprints of the same name — Goblin/Fetchling/Duergar exist twice
in the game's data; `PrerequisiteRaceAny` needs Ebon's GUID, since that's the one that actually
ends up in `unit.Progression.Race` for a player character).

| Race | GUID (EbonsContentMod) | Used by |
|---|---|---|
| Goblin | `93fb4931c7b34ec4a023f429e3b16239` | bardic performance, alchemist fire resistance |
| Fetchling | `29454c0ec53946c48cd34bcad4311ab7` | cold/electricity resistance, negative energy damage, studied-target dodge, companion DR, arcane pool |
| Hobgoblin | `be0a8e971f8e4ab6975154dade7a2446` | bombs, grapple/trip, negative energy damage, favored enemy attack/damage, concentration |
| Drow | `5d357ab2ba684b76b7f13e8f3fe441c4` | disarm, concentration, teamwork feat, cruelty |
| Suli | `d5398269cc1442d7802469cbe7fdf151` | 4× energy resistance (ranger), arcane pool |
| Duergar | `ac2584f867f24c8499b8c77572dd4a61` | judgment |
| Ganzi | `14be0c2967a842febd853380ad785ce5` | (reserved: Oracle enchantment spell known) |

Ebon's other races (Sylph, Undine, Svirfneblin, Samsaran, Strix, Ifrit, Changeling, Kuru,
Vishkanya, Shabti, Android, Skinwalker, Orc, Rougarou, Nagaji, Mongrel, Ascending Succubus)
had no bonus in the original ZFC mod — favored class bonuses for them would be homebrew, not
a port, so they're out of scope for v1.

## Third-party classes (installed)

| Class | GUID | Source |
|---|---|---|
| Swashbuckler | `338abf2723c14c1ab0f17cd7e3020444` | Swashbuckler mod (panache `ac63bfcf...`, charmed life `e6ad4ad4...`) |
| Antipaladin | `8939eff25a0a4b77ad1ab6be4c760a6c` | MicroscopicContentExpansion (cruelty selection `402fccae...`, from its own Blueprints.json) |

## Porting status

The full ledger of the original mod's 91 bonuses was tracked as a separate reference table
during development. Summary by wave:

- **Wave 1 (universal)**: HP, skill rank — done.
- **Wave 2 (literal PnP racial)**: speed (barbarian/bloodrager/monk), dodge vs. favored
  enemies, natural armor, necromancy caster level, enchantment DC + Magical Tail (kitsune),
  warpriest bonus combat feat — done.
- **Wave 3 (resource pools)**: rage, bloodrage, bardic/skald performance, bombs, ki pool,
  arcane pool, arcane reservoir, judgment (Duergar), panache, charmed life (Swashbuckler) — done.
- **Wave 4 ("1/6 of a new X" wrappers)**: rogue talent, witch hex, arcanist exploit,
  shaman hex, slayer talent, wild talent, magus arcana, teamwork feat (Inquisitor/Drow),
  cruelty (Antipaladin/Drow, ÷4) — done.
- **Wave 5**: concentration ×4 (the native RuleCheckConcentration/AddBonusConcentration
  mechanism does exist in WOTR), CMB grapple/trip + disarm (native CMBBonusForManeuver),
  kineticist blast damage ×2 (blast ability GUIDs are identical to Kingmaker's),
  acid/fire/negative-energy spell damage, favored-enemy attack/damage, energy resistance ×7,
  studied-target dodge (same buff GUID as Kingmaker), companion DR + saves (MasterFeatureRank
  config + granting the fact to the pet via IPartyHandler) — done.

## Deferred (with reasons)

| Bonus | Reason |
|---|---|
| Bonus known spells (17 rows, all ÷2) | ZFC built this on a `CreateExtraSpellSelection` factory layered over spellbooks — a separate subsystem that needs its own design pass |
| Lay on Hands ×2 (Paladin) | Requires mutating 3 vanilla abilities' internal components (ContextActionHealTarget/DealDamage BonusValue); the ability GUIDs are identical to Kingmaker's (`caae1dc6...`, `8d607320...`, `8337cea0...`), but their internal structure needs in-game verification before committing to a patch |
| Wild Shape natural AC (Druid), mutagen condition (Dwarf Alchemist fix) | Needs a general "bonus only while buff X is active" mechanism — the research into which wild-shape/mutagen buffs to check isn't finished |
| Patron spells caster level (Witch/Halfling) | Needs a way to identify a patron spell at cast time — not yet researched |
| Arcane reservoir regen (Arcanist/Gnome) | No hook found for regen rate (as opposed to max amount) |
| Kineticist internal buffer | The resource doesn't exist in WOTR (burn was reworked) |
| Eldritch Scion pool/arcana | Eldritch Scion is its own subclass in WOTR (excluded from favored-class selection, same as the original mod); supporting it is a separate design decision |
| Ravener Hunter / Winter Witch / Unlettered Arcanist bonus spell known | These archetypes don't exist in WOTR (Winter Witch here is a prestige class) |
| Insinuator Greed | The archetype doesn't exist in MicroscopicContentExpansion |
| Everything for Psychic/Occultist/Investigator/Spiritualist/Summoner | None of these classes exist in WOTR or its installed mods |

## Known fidelity gaps

- Acid spell damage (Dwarf/Oread Sorcerer): the original used `Acid | Ground`; WOTR has no
  `Ground` spell descriptor (that was a Call of the Wild-only addition in Kingmaker) — only the
  Acid half is ported.
- The Slayer talent wrapper only mirrors the base (level 2) pool out of three level-gated pools.
- Dwarf alchemist natural armor is unconditional; in the original it only applies while a
  mutagen/cognatogen is active.
- Warpriest bonus combat feat: the Aasimar and Tiefling races aren't wired up yet.

## Rules for excluding classes from the favored-class selection

Not shown: PrestigeClass=true (they get the Favored Prestige Class feat instead), AnimalClass,
AnimalCompanionClass, MythicCompanionClass, all Mythic* classes, monster classes, and technical
classes. EldritchScionClass is excluded, matching the original mod (it's a Magus subclass).
