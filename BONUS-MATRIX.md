# Матрица favored class бонусов для WOTR

Ванильные играбельные расы WOTR: Human, Elf, Half-Elf, Dwarf, Gnome, Halfling, Half-Orc,
Aasimar, Tiefling, Oread, Dhampir, Kitsune (12).

## Расы из Ebon's Content Mod (установлен)

«Ушедшие из KM-версии» расы НЕ потеряны: EbonsContentMod добавляет их как полноценные
чарген-расы со СВОИМИ GUID (не путать со скрытыми ванильными монстро-блюпринтами тех же
имён — Goblin/Fetchling/Duergar существуют в данных дважды; для `PrerequisiteRaceAny`
нужен GUID мода Ebon'а, именно он попадает в `unit.Progression.Race`).

| Раса | GUID (EbonsContentMod) | Использована в бонусах |
|---|---|---|
| Goblin | `93fb4931c7b34ec4a023f429e3b16239` | bardic performance, alchemist fire res |
| Fetchling | `29454c0ec53946c48cd34bcad4311ab7` | cold/elec res, negative dmg, studied dodge, companion DR, arcane pool |
| Hobgoblin | `be0a8e971f8e4ab6975154dade7a2446` | bombs, grapple/trip, negative dmg, FE atk/dmg, concentration |
| Drow | `5d357ab2ba684b76b7f13e8f3fe441c4` | disarm, concentration, teamwork feat, cruelty |
| Suli | `d5398269cc1442d7802469cbe7fdf151` | 4× energy res (ranger), arcane pool |
| Duergar | `ac2584f867f24c8499b8c77572dd4a61` | judgment |
| Ganzi | `14be0c2967a842febd853380ad785ce5` | Oracle bonus known spell (enchantment school) |

Прочие расы Ebon'а (Sylph, Undine, Svirfneblin, Samsaran, Strix, Ifrit, Changeling, Kuru,
Vishkanya, Shabti, Android, Skinwalker, Orc, Rougarou, Nagaji, Mongrel, Ascending Succubus)
в ZFC отсутствовали — бонусы для них были бы гомбрю, в v1 не делаем.

## Сторонние классы (установлены)

| Класс | GUID | Источник |
|---|---|---|
| Swashbuckler | `338abf2723c14c1ab0f17cd7e3020444` | Swashbuckler mod (panache `ac63bfcf...`, charmed life `e6ad4ad4...`) |
| Antipaladin | `8939eff25a0a4b77ad1ab6be4c760a6c` | MicroscopicContentExpansion (cruelty selection `402fccae...` из его Blueprints.json) |

## Статус портирования (актуален на срез Wave 5)

Полный леджер 91 бонуса оригинала — опубликованный артефакт «Favored Class Bonus Ledger».
Сводка по волнам:

- **Wave 1 (universal)**: HP, skill rank — готово (v0.1.0).
- **Wave 2 (расовые PnP)**: speed (barb/bloodrager/monk), dodge vs FE, natural AC,
  necro CL, ench DC + Magical Tail (kitsune), warpriest combat feat — готово (v0.3–0.4 + Wave 5 monk).
- **Wave 3 (resource pools)**: rage, bloodrage, bardic/skald performance, bombs, ki,
  arcane pool, arcane reservoir — готово. + Wave 5: judgment (Duergar), panache,
  charmed life (Swashbuckler).
- **Wave 4 (обёртки 1/6)**: rogue talent, witch hex, arcanist exploit, shaman hex,
  slayer talent, wild talent, magus arcana — готово. + Wave 5: teamwork feat (Inquisitor;
  Drow ÷4, RAW Blood of Shadows p.15 — исправлено с ÷6, унаследованного от ZFC; Half-Elf/
  Halfling добавлены позже как замена их собственного нереализуемого механизма, см. фиделити-гэпы),
  cruelty (Antipaladin/Drow, ÷4).
- **Wave 5**: concentration ×4 (нативные RuleCheckConcentration/AddBonusConcentration
  СУЩЕСТВУЮТ в WOTR), CMB grapple/trip + disarm (нативный CMBBonusForManeuver),
  kineticist blast dmg ×2 (GUID-ы блайстов идентичны KM), acid/fire/negative spell dmg,
  FE attack/damage, energy resistance ×7, studied target dodge (буфф тот же GUID, что в KM),
  companion DR + saves (MasterFeatureRank + грант питомцу через IPartyHandler).
- **Wave 6 (текущая, bonus known spells)**: 12 записей ÷2, механизм —
  `BlueprintParametrizedFeature`+`LearnSpellParametrized` (нативно, как у ZFC) внутри
  того же wrapper-паттерна (ProgressGuid+RewardSelectionGuid), новый компонент не
  понадобился. Alchemist/Bard/Inquisitor/Oracle/Shaman/Sorcerer/Witch/Skald/Arcanist —
  обычный список класса; Oracle/Ganzi (enchantment) и Sorcerer/Goblin (fire) — кастомные
  фильтрованные списки, построены на лету из нативных Wizard/Cleric листов
  (`BuildFilteredSpellList`, тот же приём, что у PrestigePlus/EbonsContentMod).
  Wizard — одна запись с generic-веткой + 7 school-веток (Thassilonian Specialist,
  gated по `FeatureReplaceSpellbookRefs.ThassilonianXFeature`), игрок видит один выбор,
  какая из 8 веток доступна решают prerequisites. Arcanist исключает Unlettered
  Arcanist/Nature Mage/Magic Deceiver (`AddPrerequisiteNoArchetype`, все три подтверждены
  иметь свой Spellbook/SpellList через нативные SpellbookRefs/SpellListRefs). Ravener
  Hunter — не исключён (подтверждено пользователем, архетипа нет в ванильном WOTR).

- **Wave 7**: Lay on Hands ×2 (Paladin), Wild Shape natural AC (Druid), условие мутагена
  для Dwarf Alchemist natural AC, Eldritch Scion arcana, Channel Energy, Harm Undead, Fervor.
  Сверено с README и `Custom/*.json` оригинального мода
  (github.com/Holic75/KingmakerFavoredClass) — эти JSON-ы лежат в нашем клоне
  `reference/ZFavoredClass-source/ZFavoredClass/Custom/` и содержат точные
  divisor/классы/расы для «сторонних» бонусов, которых нет в `Core.cs`.
  Channel Energy и Fervor изначально были реализованы неверно (как +использования
  в день) — по факту это бонус к ВЕЛИЧИНЕ лечения/урона, исправлено.
  Два новых компонента: `HealBonusForAbilitiesPerRank` (нативный
  `RuleHealDamage.AdditionalBonus.Add` — плоская прибавка к лечению, фильтр по
  ability-блюпринту; `SelfOnly` сравнивает `evt.Target` с владельцем, что точнее
  ZFC-шного хардкода self-абилки) и `NaturalACWhileTransformedPerRank` (AC только
  пока активна нужная форма). Урон lay on hands («to heal **or harm**») закрыт
  переиспользованием уже существующего `AbilityDamageBonusPerRank` — новый код не
  понадобился. Wild shape определяется по наличию нативного компонента
  `Kingmaker.UnitLogic.Buffs.Polymorph` на активном баффе, а не по списку форм, —
  поэтому работает и с формами из других модов. Eldritch Scion в WOTR — **архетип**
  магуса (`d078b2ef...`; одноимённый класс `f5b8c63b...` — скрытый спеллбук-хелпер и
  остаётся в `ExcludedClasses`), поэтому запись привязана к классу Magus с
  `PrerequisiteArchetypeLevel`, зеркалит харизменный `EldritchMagusArcanaSelection`
  (`d4b54d9b...`), а обычные magus arcana и arcane pool получили
  `PrerequisiteNoArchetype` на scion'а.

- **Wave 8**: исправлен Favored Enemy (Hobgoblin Ranger) — теперь это, как в оригинале,
  выбор ОДНОГО уже взятого избранного врага (+1 к нему, максимум +1 на врага), а не
  бонус против всех сразу; пул наград строится из ванильного `FavoriteEnemySelection`
  (`16cc2c93...`), по фиче на тип врага, с `PrerequisiteFeature` на соответствующего
  избранного врага + бонус против Instant Enemy, как в оригинале. Старый счётчик
  `...b55` переведён в скрытую заглушку. Добавлены Arcane Reservoir regen
  (Gnome Arcanist ÷6) и Patron Spells CL (Halfling Witch ÷4) — оба ранее числились
  неисполнимыми, см. ниже.
  - Regen: у `BlueprintAbilityResource` действительно нет поля «сколько восстановить»,
    но восстановление на отдыхе идёт через событие `IUnitRestHandler` (им же пользуется
    нативный `AddRestTrigger`), и `UnitAbilityResourceCollection.Restore(bp, amount)`
    публичен. Новый `RestoreResourceOnRestPerRank` просто доливает ресурс сверх того,
    что восстановил класс. `Restore` клампится максимумом, поэтому если у арканиста
    резервуар всё-таки восстанавливается полностью — бонус безвреден (просто ничего
    не даёт), а не ломается.
  - Patron CL: карта патрон→заклинания строится на Install обходом 15 патронных
    `BlueprintProgression` и их компонентов `AddKnownSpell` — то есть источником
    истины является сам патрон, а не захардкоженный список (патроны из других модов
    подхватятся сами). На горячем пути каста сначала один хеш-лукап по объединённому
    множеству всех патронных спеллов (почти всегда — мгновенный выход), и только для
    патронного спелла проверяется, чей именно это патрон.

## Отложено (с причинами)

| Бонус | Причина |
|---|---|
| Kineticist internal buffer | Ресурса в WOTR не существует (burn переработан) |
| Eldritch Scion eldritch pool (÷4) | Ресурс есть (`EldritchPoolResourse` `17b6158d...`), реализуется тривиально через `IncreaseResourceAmountPerRank` — но по решению пользователя scion получает только бонусные arcana, не пул |
| Ravener Hunter / Winter Witch / Unlettered Arcanist — отдельные архетип-варианты bonus known spell (свой спеллбук вместо базового) | Архетипов-как-таковых для отдельной FCB-записи нет смысла делать: Ravener Hunter не найден в ванильном WOTR (только в моде ExpandedContent, не проверен), Winter Witch — престиж-класс (не меняет базовый список), Unlettered Arcanist уже исключён из базовой Arcanist-записи вместо отдельного варианта |
| Insinuator Greed | Архетипа нет в MCE |
| Psychic/Occultist/Investigator/Spiritualist/Summoner всё | Классов нет в WOTR + модах |

## Известные фиделити-гэпы

- Acid Spell Damage (Dwarf/Oread Sorcerer): в ZFC был `Acid | Ground`; дескриптора
  Ground в WOTR нет (это кастом CotW) — портирован только Acid-компонент.
- Slayer talent wrapper зеркалит только базовый пул (level 2) из трёх.
- Мутагенный natural AC (Dwarf Alchemist) распознаёт только ванильный набор баффов
  мутагенов/когнатогенов (31 GUID, включая True Mutagen) — мутаген из стороннего мода
  условие не активирует.
- Wild Shape natural AC срабатывает на ЛЮБОЙ полиморф-бафф, а не строго на wild shape:
  в WOTR друидские превращения и полиморф-заклинания используют один и тот же
  нативный компонент `Polymorph`, надёжно различить их нечем. На практике совпадает,
  но друид под, скажем, Beast Shape из свитка тоже получит бонус.
- Channel Energy FCB привязан к ванильному `ChannelEnergyResource` (`5e2bba3e...`).
  Для клирика это точно тот ресурс; **для варприста нужно проверить в игре** — если
  варпристовский channel energy использует другой ресурс, запись для него будет
  просто неактивной (не сломается, но и не сработает).
- Acid Spell Damage (Sorcerer): в оригинале раса только Dwarf, у нас Dwarf + Oread —
  осознанное добавление сверх оригинала (решение пользователя), не гэп.
- Расхождения README оригинала с его же `Core.cs` (мы следуем коду, README неточен):
  necromancy CL — README «drow», код `dhampir` (у нас Dhampir); lay on hands и wild
  shape AC — README не упоминает Half-Elf, код/JSON его включают (у нас включён);
  Ganzi enchantment — README «oracle/wizard», код комбинирует wizard+cleric (у нас
  wizard+cleric).
- Wizard bonus known spell не проверяет opposition school (запрещённую школу
  специализации): это отдельный от Thassilonian Specialist механизм (тот просто
  запрещает готовить спеллы забаненной школы, не подменяет спеллбук), наша фича может
  предложить спелл из забаненной школы обычному специалисту-визарду. Не исправлено.
- Inquisitor teamwork feat: Drow RAW-текст (Blood of Shadows p.15) — "Gain 1/4 of a
  teamwork feat" (÷4); ZFC портировал это как ÷6, мы исправили на ÷4 вслед за источником.
  Half-Elf/Halfling в RAW получают другой бонус ("+1/4 к числу раз в день, когда инквизитор
  может поменять свой последний teamwork feat") — этой механики смены фита в WOTR нет,
  поэтому им намеренно дана та же запись, что Drow (ближайший доступный эквивалент), а не
  отдельная, текстуально точная, но нереализуемая версия.

## Правила исключения классов из favored-селекшена

Не показывать: PrestigeClass=true (им — Favored Prestige Class фит), AnimalClass,
AnimalCompanionClass, MythicCompanionClass, все Mythic*, монстро-классы, technical.
EldritchScionClass — исключён и в оригинале (это подкласс магуса).
