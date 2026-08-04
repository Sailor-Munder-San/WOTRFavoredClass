# WOTRFavoredClass

A port of Holic75's **[ZFavoredClass](https://github.com/Holic75/KingmakerFavoredClass)** mod
— the tabletop Pathfinder favored class bonus system — from *Pathfinder: Kingmaker* to
*Pathfinder: Wrath of the Righteous*.

Each character begins play with a favored class. Whenever they gain a level in that class,
they receive +1 hit point or +1 skill rank as usual — but many race/class combinations also
unlock an alternate racial favored class bonus (extra rage rounds, elemental resistance,
bonus talents, combat maneuver bonuses, and more), exactly as written in the Pathfinder
Roleplaying Game rules.

## Status

Current release: **v0.1.5** — 89 racial favored class bonuses across every class below, plus
the universal +1 HP / +1/2 skill rank bonus everyone gets. The favored class is chosen at the
race step of character creation, and half-elves get **Multitalented**: two favored classes,
either of which pays out whenever they take a level in it.

| Class | Racial favored class bonuses |
|---|---|
| **Alchemist** | Natural Armor (Dwarf); Bombs (Gnome, Hobgoblin); Fire Resistance (Goblin); Known Formula (Elf, Human, Halfling, Half-Elf, Half-Orc, Aasimar, Tiefling); Bomb Damage (Half-Orc, Tiefling) |
| **Antipaladin** | Cruelty (Drow) |
| **Arcanist** | Arcane Reservoir (Elf, Half-Elf); Arcanist Exploit (Halfling); Concentration (Half-Orc); Reservoir Regen (Gnome); Known Spell (Human, Half-Elf, Half-Orc, Aasimar, Tiefling) |
| **Barbarian** | Speed (Elf, Half-Elf); Rage Rounds (Dwarf, Half-Orc); Cold/Electricity Resistance (Fetchling) |
| **Bard** | Bardic Performance (Half-Elf, Half-Orc, Gnome, Goblin); Known Spell (Human, Half-Elf, Half-Orc, Aasimar, Tiefling) |
| **Bloodrager** | Speed (Elf, Half-Elf); Bloodrage Rounds (Dwarf, Half-Orc, Human, Half-Elf, Aasimar, Tiefling); Concentration (Drow) |
| **Cleric** | Negative Energy Spell Damage (Hobgoblin, Fetchling); Channel Energy (Half-Elf); Harm Undead (Aasimar) |
| **Druid** | Companion DR (Gnome, Fetchling, Svirfneblin); Companion Saves (Halfling); Wild Shape Natural Armor (Elf, Half-Orc, Half-Elf); Companion HP (Goblin, Half-Orc); Acid/Cold/Electricity/Fire Resistance (Gnome) |
| **Fighter** | Disarm (Drow) |
| **Hunter** | Companion DR (Gnome, Fetchling, Svirfneblin); Companion Saves (Halfling); Companion HP (Goblin, Half-Orc) |
| **Inquisitor** | Concentration (Hobgoblin); Judgment (Duergar); Teamwork Feat (Drow, Half-Elf, Halfling); Known Spell (Elf, Human, Half-Elf, Half-Orc, Aasimar, Tiefling) |
| **Kineticist** | Wild Talent (Human, Half-Elf, Half-Orc, Aasimar, Tiefling); Earth Blast Damage (Dwarf); Elemental Blast Damage (Elf, Half-Elf) |
| **Magus** | Arcane Pool (Human, Half-Elf, Half-Orc, Aasimar, Tiefling, Suli, Fetchling); Magus Arcana (Elf, Halfling, Half-Elf); Eldritch Scion Arcana (Elf, Halfling, Half-Elf); Fire Spell Damage (Half-Orc) |
| **Monk** | Ki Pool (Human, Half-Elf, Half-Orc, Aasimar, Tiefling); Grapple/Trip (Hobgoblin); Speed (Elf, Half-Elf) |
| **Oracle** | Negative Energy Spell Damage (Dhampir); Known Spell (Elf, Human, Half-Elf, Half-Orc, Aasimar, Tiefling); Known Spell – Enchantment (Ganzi) |
| **Paladin** | Concentration (Dwarf); Lay on Hands (Elf, Gnome, Halfling, Half-Elf); Lay on Hands Self-Healing (Tiefling); Acid/Cold/Electricity/Fire Resistance (Human) |
| **Ranger** | Dodge vs Favored Enemies (Halfling); Favored Enemy Bonus (Hobgoblin); Acid/Cold/Fire/Electricity Resistance (Suli); Companion DR (Gnome, Fetchling, Svirfneblin); Companion Natural Armor (Oread) |
| **Rogue** | Rogue Talent (Human, Half-Elf, Half-Orc, Aasimar, Tiefling, Changeling, Kitsune, Samsaran) |
| **Shaman** | Hex (Gnome); Known Spell (Half-Elf, Human, Half-Orc, Aasimar, Tiefling); Known Spell – Enchantment (Kitsune) |
| **Skald** | Raging Song (Half-Elf, Half-Orc); Known Spell (Human, Half-Elf, Half-Orc, Aasimar, Tiefling); Concentration (Gnome) |
| **Slayer** | Talent (Human, Gnome, Half-Elf, Half-Orc, Aasimar, Tiefling); Dodge vs Studied Targets (Halfling, Fetchling) |
| **Sorcerer** | Enchantment Spell DC (Kitsune); Acid Spell Damage (Dwarf, Oread); Fire Spell Damage (Half-Orc); Cold/Electricity Resistance (Fetchling); Known Spell (Human, Half-Elf, Half-Orc, Aasimar, Tiefling); Known Spell – Fire (Goblin); Good-descriptor Caster Level (Aasimar); Known Spell – Curse/Evil (Drow) |
| **Swashbuckler** | Panache (Elf, Human, Half-Elf, Half-Orc, Tiefling, Aasimar, Kitsune); Charmed Life (Gnome, Halfling, Half-Elf) |
| **Warpriest** | Combat Feat (Human, Half-Elf, Half-Orc, Aasimar, Tiefling); Channel Energy (Half-Elf); Blessings (Dwarf, Elf, Nagaji); Fervor (Drow) |
| **Witch** | Hex (Gnome); Patron Spells Caster Level (Halfling); Known Spell (Human, Half-Orc, Half-Elf, Elf, Aasimar, Tiefling, Goblin) |
| **Wizard** | Necromancy Caster Level (Dhampir); Known Spell, 8 Thassilonian school branches (Human, Half-Elf, Half-Orc, Aasimar, Tiefling) |
| **Kitsune** (any favored class) | Magical Tail |

What remains unported is what the game cannot express: classes absent from WOTR and its mods
(Psychic, Occultist, Investigator, Spiritualist, Summoner), the kineticist internal buffer
(burn was reworked), and a couple of archetype-specific spellbook variants. A few bonuses whose
tabletop wording has no mechanical equivalent in the game were given the closest available
substitute instead, called out explicitly where it applies.

See **[BONUS-MATRIX.md](BONUS-MATRIX.md)** for the full per-bonus breakdown, including exactly
which races/GUIDs are used, which bonuses are deferred and why, and the known fidelity gaps
where this port knowingly differs from the tabletop rules or from the original mod.

## Installation

Requires [Unity Mod Manager](https://www.nexusmods.com/site/mods/21) and
[DragonLibrary](https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/163) (for
[BlueprintCore](#reference-material--credits)). Drop the `WOTRFavoredClass` folder into your
`Wrath of the Righteous/Mods` directory.

A handful of racial bonuses only activate if certain other mods are also installed — the mod
degrades gracefully without them (those specific bonus options simply won't be offered):

- [TabletopTweaks-Core](https://github.com/Vek17/TabletopTweaks-Core) / TabletopTweaks-Base
- [EbonsContentMod](https://www.nexusmods.com/pathfinderwrathoftherighteous) (adds Goblin,
  Drow, Hobgoblin, Fetchling, Suli, Duergar, Ganzi, Svirfneblin, Samsaran, Changeling, Nagaji
  as playable races)
- [Swashbuckler](https://github.com/novumvita/SwashbucklerWOTR) (Panache / Charmed Life bonuses)
- [MicroscopicContentExpansion](https://github.com/alterasc/MicroscopicContentExpansion) (Antipaladin cruelty bonus)

## Uninstalling cleanly

The mod's in-game menu (Unity Mod Manager overlay) has a **"Strip all Favored Class data from
loaded save"** button. Load your save, press it, then save again — saves made after that no
longer reference this mod and load fine without it.

## Reference material & credits

- **[Holic75/KingmakerFavoredClass](https://github.com/Holic75/KingmakerFavoredClass)** — the
  original *Pathfinder: Kingmaker* mod this is a port of. Its `Core.cs` is the source of truth
  for every bonus's PnP wording, divisor, and race/class list.
- **[WittleWolfie/WW-Blueprint-Core](https://github.com/WittleWolfie/WW-Blueprint-Core)**
  (BlueprintCore) — the blueprint-authoring library this mod is built on, and the source of
  every vanilla WOTR class/race/resource/ability GUID referenced in the code.
- **[Vek17/TabletopTweaks-Core](https://github.com/Vek17/TabletopTweaks-Core)** — reference
  implementation for several native-interface components (rank-scaled resource bonuses,
  `RuleCalculateDamage` hooks, concentration bonus provider, companion-targeting patterns).
- **[YLMstring/Prestige-Plus](https://github.com/YLMstring/Prestige-Plus)** — reference
  implementation for combat-maneuver bonuses (`CMBBonusForManeuver`) and favored-enemy
  attack/damage bonus patterns (`RuleCalculateAttackBonus` / `RuleAttackWithWeapon`).
- **[novumvita/SwashbucklerWOTR](https://github.com/novumvita/SwashbucklerWOTR)** — source of
  the Panache and Charmed Life resource GUIDs used by the corresponding favored class bonuses.

## Development notes

This port was developed with the assistance of [Claude](https://www.anthropic.com/claude)
(Anthropic) — including decompiling and cross-referencing the reference mods above against
the WOTR game assembly to identify native engine mechanisms for each bonus, writing the
Harmony-free, stateless components the project's save-hygiene rules require, and drafting
this documentation.
