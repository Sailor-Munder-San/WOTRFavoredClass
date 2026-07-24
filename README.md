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

This is an initial public release. A large majority of the original mod's ~91 favored class
bonuses are ported (universal HP/skill bonuses, racial PnP bonuses, resource pool bonuses,
"1/6 of a new talent" wrapper bonuses, concentration, combat maneuvers, energy resistance,
spell/ability damage, favored-enemy bonuses, and animal companion bonuses). A smaller set is
deliberately deferred (bonus known spells, Lay on Hands, a few archetype-specific variants)
because they need either a dedicated subsystem or hands-on in-game verification before
shipping. See **[BONUS-MATRIX.md](BONUS-MATRIX.md)** for the full per-bonus breakdown,
including exactly which races/GUIDs are used and why specific bonuses are deferred (that
document is in Russian — the primary development language for this project — but the table
structure is self-explanatory).

## Installation

Requires [Unity Mod Manager](https://www.nexusmods.com/site/mods/21) and
[DragonLibrary](https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/163) (for
[BlueprintCore](#reference-material--credits)). Drop the `WOTRFavoredClass` folder into your
`Wrath of the Righteous/Mods` directory.

A handful of racial bonuses only activate if certain other mods are also installed — the mod
degrades gracefully without them (those specific bonus options simply won't be offered):

- [TabletopTweaks-Core](https://github.com/Vek17/TabletopTweaks-Core) / TabletopTweaks-Base
- [EbonsContentMod](https://www.nexusmods.com/pathfinderwrathoftherighteous) (adds Goblin,
  Drow, Hobgoblin, Fetchling, Suli, Duergar, Ganzi as playable races)
- [Swashbuckler](https://github.com/novumvita/SwashbucklerWOTR) (Panache / Charmed Life bonuses)
- MicroscopicContentExpansion (Antipaladin cruelty bonus)

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
