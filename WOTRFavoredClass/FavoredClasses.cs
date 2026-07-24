using System;
using System.Collections.Generic;
using System.Linq;
using BlueprintCore.Blueprints.Configurators.Classes;
using BlueprintCore.Blueprints.Configurators.Classes.Selection;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.Classes.Selection;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats;

namespace WOTRFavoredClass
{
    internal static class FavoredClasses
    {
        // Fixed root GUIDs of this mod (never change once released).
        internal const string SelectionGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b01";
        private const string HpFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b02";
        private const string MarkerGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b03";
        private const string NoneFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b04";
        private const string SkillFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b05";
        private const string WpCombatPartialGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b06";
        private const string WpCombatSelGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b07";
        // Identity-only blueprint used as the native FeatureSource tag ("granted by")
        // for facts this mod grants outside the level-up selection flow (which would
        // otherwise set source automatically). Never granted as a fact itself.
        internal const string SourceMarkerGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1c";

        private const string WarpriestClassGuid = "30b5e47d47a0e37438cc5a80c96cfb99";
        private const string VanillaWarpriestFeatSel = "303fd456ddb14437946e344bad9a893b";

        // Native talent/hex/arcana/exploit selections mirrored into wrapper reward
        // pools below (same technique as the warpriest combat feat list): our reward
        // selection copies m_AllFeatures from the class's own vanilla selection, so
        // third-party additions (other mods' talents/hexes) are picked up too.
        private const string VanillaRogueTalentSel = "c074a5d615200494b8f2a9c845799d93";
        private const string VanillaWitchHexSel = "9846043cf51251a4897728ed6e24e76f";
        private const string VanillaArcanistExploitSel = "b8bf3d5023f2d8c428fdf6438cecaea7";
        private const string VanillaShamanHexSel = "4223fe18c75d4d14787af196a04e14e7";
        // Slayer talents are split across three level-gated pools (2/6/10); we mirror
        // only the base (level 2) pool, matching the accepted-fidelity-gap precedent
        // set by the warpriest wrapper (see HANDOFF.md known gaps).
        private const string VanillaSlayerTalentSel = "04430ad24988baa4daa0bcd4f1c7d118";
        private const string VanillaWildTalentSel = "5c883ae0cd6d7d5448b7a420f51f8459";
        private const string VanillaMagusArcanaSel = "e9dc4dfc73eaaf94aae27e0ed6cc9ada";
        // Inquisitor-specific teamwork feat pool (TTT-Core FeatTools disambiguates it
        // from the generic cavalier-family pool 90b8828...).
        private const string VanillaInquisitorTeamworkFeatSel = "d87e2f6a9278ac04caeb0f93eff95fcb";
        // MicroscopicContentExpansion's Antipaladin cruelty selection — read from the
        // installed mod's own UserSettings/Blueprints.json, not vanilla data. The
        // mirror loop skips it gracefully if MCE is absent.
        private const string MceAntipaladinCrueltySel = "402fccae3c2147e78da4ff9f6f061461";

        // Races
        private const string HumanRaceGuid = "0a5d473ead98b0646b94495af250fdc4";
        private const string HalfElfRaceGuid = "b3646842ffbd01643ab4dac7479b20b0";
        private const string HalfOrcRaceGuid = "1dc20e195581a804890ddc74218bfd8e";
        private const string ElfRaceGuid = "25a5878d125338244896ebd3238226c8";
        private const string HalflingRaceGuid = "b0c3ef2729c498f47970bb50fa1acd30";
        private const string DwarfRaceGuid = "c4faf439f0e70bd40b5e36ee80d06be7";
        private const string DhampirRaceGuid = "64e8b7d5f1ae91d45bbf1e56a3fdff01";
        private const string KitsuneRaceGuid = "fd188bb7bb0002e49863aec93bfb9d99";
        private const string GnomeRaceGuid = "ef35a22c9a27da345a4528f0d5889157";
        private const string AasimarRaceGuid = "b7f02ba92b363064fb873963bec275ee";
        private const string TieflingRaceGuid = "5c4e42124dc2b4647af6e36cf2590500";
        private const string OreadRaceGuid = "4d4555326b9b7144f93be1ea61337cd7";
        // Races below are added by EbonsContentMod (chargen-facing blueprints with the
        // mod's own GUIDs — NOT the hidden vanilla monster races of the same name).
        // PrerequisiteRaceAny resolves lazily, so these are inert without that mod.
        private const string GoblinRaceGuid = "93fb4931c7b34ec4a023f429e3b16239";
        private const string FetchlingRaceGuid = "29454c0ec53946c48cd34bcad4311ab7";
        private const string HobgoblinRaceGuid = "be0a8e971f8e4ab6975154dade7a2446";
        private const string DrowRaceGuid = "5d357ab2ba684b76b7f13e8f3fe441c4";
        private const string SuliRaceGuid = "d5398269cc1442d7802469cbe7fdf151";
        private const string DuergarRaceGuid = "ac2584f867f24c8499b8c77572dd4a61";

        // Classes
        private const string BarbarianClassGuid = "f7d7eb166b3dd594fb330d085df41853";
        private const string BloodragerClassGuid = "d77e67a814d686842802c9cfd8ef8499";
        private const string MonkClassGuid = "e8f21e5b58e0569468e420ebea456124";
        private const string SlayerClassGuid = "c75e0971973957d4dbad24bc7957e4fb";
        private const string RangerClassGuid = "cda0615668a6df14eb36ba19ee881af6";
        private const string AlchemistClassGuid = "0937bec61c0dabc468428f496580c721";
        private const string WizardClassGuid = "ba34257984f4c41408ce1dc2004e342e";
        private const string SorcererClassGuid = "b3a505fb61437dc4097f43c3f8f9a4cf";
        private const string BardClassGuid = "772c83a25e2268e448e841dcd548235f";
        private const string SkaldClassGuid = "6afa347d804838b48bda16acb0573dc0";
        private const string MagusClassGuid = "45a4607686d96a1498891b3286121780";
        private const string ArcanistClassGuid = "52dbfd8505e22f84fad8d702611f60b7";
        private const string RogueClassGuid = "299aa766dee3cbf4790da4efb8c72484";
        private const string WitchClassGuid = "1b9873f1e7bfe5449bc84d03e9c8e3cc";
        private const string ShamanClassGuid = "145f1d3d360a7ad48bd95d392c81b38e";
        private const string KineticistClassGuid = "42a455d9ec1ad924d889272429eb8391";
        private const string FighterClassGuid = "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string PaladinClassGuid = "bfa11238e7ae3544bbeb4d0b92e897ec";
        private const string InquisitorClassGuid = "f1a70d9e1b0b41e49874e1fa9052a1ce";
        private const string ClericClassGuid = "67819271767a9dd4fbfd4ae700befea0";
        private const string OracleClassGuid = "20ce9bf8af32bee4c8557a045ab499b1";
        private const string DruidClassGuid = "610d836f3a3a9ed42a4349b62f002e96";
        private const string HunterClassGuid = "34ecd1b5e1b90b9498795791b0855239";
        // Third-party classes (Swashbuckler mod, MicroscopicContentExpansion).
        // Class-keyed bonuses only attach if the class is actually in the game's list.
        private const string SwashbucklerClassGuid = "338abf2723c14c1ab0f17cd7e3020444";
        private const string AntipaladinClassGuid = "8939eff25a0a4b77ad1ab6be4c760a6c";

        // Slayer studied-target buffs: same blueprint GUIDs as Kingmaker (Owlcat
        // carried the slayer content over unchanged) — the buff sits on the studied
        // ENEMY with the slayer as caster.
        private const string SlayerStudyTargetBuffGuid = "45548967b714e254aa83f23354f174b0";
        private const string SlayerDefensiveStudyBuffGuid = "cbbff1a2e7a3a5b47b41406701de305b";

        // Kineticist blast base abilities (GUIDs identical to Kingmaker's; variant
        // forms are child abilities matched via Parent by AbilityDamageBonusPerRank).
        private static readonly string[] EarthBlastBaseGuids =
        {
            "b93e1f0540a4fa3478a6b47ae3816f32", // sandstorm
            "e2610c88664e07343b4f3fb6336f210c", // mud
            "6276881783962284ea93298c1fe54c48", // metal
            "8c25f52fce5113a4491229fd1265fc3c", // magma
            "e53f34fb268a7964caf1566afb82dadd", // earth
        };
        private static readonly string[] AllBlastBaseGuids =
        {
            "83d5873f306ac954cad95b6aeeeb2d8c", // fire
            "d663a8d40be1e57478f34d6477a67270", // water
            "b813ceb82d97eed4486ddd86d3f7771b", // thunderstorm
            "3baf01649a92ae640927b0f633db7c11", // steam
            "b93e1f0540a4fa3478a6b47ae3816f32", // sandstorm
            "9afdc3eeca49c594aa7bf00e8e9803ac", // plasma
            "e2610c88664e07343b4f3fb6336f210c", // mud
            "6276881783962284ea93298c1fe54c48", // metal
            "8c25f52fce5113a4491229fd1265fc3c", // magma
            "403bcf42f08ca70498432cf62abee434", // ice
            "45eb571be891c4c4581b6fcddda72bcd", // electric
            "e53f34fb268a7964caf1566afb82dadd", // earth
            "7980e876b0749fc47ac49b9552e259c1", // cold
            "4e2e066dd4dc8de4d8281ed5b3f4acb6", // charged water
            "d29186edb20be6449b23660b39435398", // blue flame
            "16617b8c20688e4438a803effeeee8a6", // blizzard
            "0ab1552e2ebdacf44bb7b20f5393366d", // air
        };

        // Wrapper reward selections (this mod's own blueprints — mirror the vanilla
        // selections above at Install() time).
        private const string RogueTalentRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2f";
        private const string WitchHexRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b32";
        private const string ArcanistExploitRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b35";
        private const string ShamanHexRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b38";
        private const string SlayerTalentRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b3b";
        private const string KineticistWildTalentRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b3e";
        private const string MagusArcanaRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b41";

        // Ability resources (Wave 3 — resource-pool favored class bonuses).
        private const string RageResourceGuid = "24353fcf8096ea54684a72bf58dedbc9";
        private const string BloodragerRageResourceGuid = "4aec9ec9d9cd5e24a95da90e56c72e37";
        private const string BardicPerformanceResourceGuid = "e190ba276831b5c4fa28737e5e49e6a6";
        private const string RagingSongResourceGuid = "4a2302c4ec2cfb042bba67d825babfec";
        private const string AlchemistBombsResourceGuid = "1633025edc9d53f4691481b48248edd7";
        private const string KiPowerResourceGuid = "9d9c90a9a1f52d04799294bf91c80a82";
        private const string ArcanePoolResourceGuid = "effc3e386331f864e9e06d19dc218b37";
        private const string ArcanistArcaneReservoirResourceGuid = "cac948cbbe79b55459459dd6a8fe44ce";
        private const string JudgmentResourceGuid = "394088e9e54ccd64698c7bd87534027f";
        // Swashbuckler mod resources (from its own source; unresolvable refs are
        // simply never matched by IncreaseResourceAmountPerRank if the mod is absent).
        private const string PanacheResourceGuid = "ac63bfcfec3143dca5ce04617a3bc854";
        private const string CharmedLifeResourceGuid = "e6ad4ad4c14c46a9b18af8d9d82a3b33";

        // Companion-side features (granted to the player's pets, value = master rank).
        private const string CompanionDRPetGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b67";
        private const string CompanionSavesPetGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b69";
        // Seeds for deterministic per-class child GUIDs (xor with class guid).
        private const string ProgressionSeed = "602ea6032c324258a183588f84522ea1";
        private const string BonusSelectionSeed = "f431abc7ab7b4771a58fff7ee2af8a01";

        private const string BasicFeatsProgressionGuid = "5b72dd2ca2cb73b49903806ee8986325";

        // Guids of all per-class bonus selections — used by the level-up queue patch
        // to glue each bonus card right behind its favored-class pick card.
        internal static readonly HashSet<string> BonusSelectionGuids = new();

        // Every blueprint guid this mod creates — used by the clean-uninstall strip.
        internal static readonly HashSet<string> AllModGuids = new();
        // Same set as BlueprintGuid structs for allocation-free checks on hot paths
        // (the player-faction gate runs on every fact grant in the game).
        internal static readonly HashSet<BlueprintGuid> AllModBlueprintGuids = new();

        internal enum BonusDisplayKind { Flat, Feet, SkillRanks, Feats, HitPoints }

        // Accumulative bonus features -> how to render the earned whole bonus in
        // character sheet tooltips.
        internal static readonly Dictionary<BlueprintGuid, (int Divisor, BonusDisplayKind Kind)> BonusDisplays = new();

        // Counter feature -> (divisor, effect feature granted one rank per threshold).
        internal static readonly Dictionary<BlueprintGuid, (int Divisor, string EffectGuid)> EffectGrants = new();

        // Wrapper reward selection -> its pick-counter feature: a reward pick also
        // counts as a pick, so the counter always shows the true number of picks.
        internal static readonly Dictionary<BlueprintGuid, string> RewardPickCounters = new();
        private static readonly HashSet<string> ExcludedClasses = new()
        {
            "f5b8c63b141b2f44cbb8c2d7579c34f5", // EldritchScionClass — magus subclass, excluded in the original too
        };

        public static void Install()
        {
            AllModGuids.Clear();
            AllModGuids.Add(SelectionGuid);
            AllModGuids.Add(HpFeatureGuid);
            AllModGuids.Add(SkillFeatureGuid);
            AllModGuids.Add(MarkerGuid);
            AllModGuids.Add(NoneFeatureGuid);
            AllModGuids.Add(SourceMarkerGuid);

            // Native "granted by" source tag — never granted as a fact, only referenced
            // via Feature.SetSource so the character sheet attributes our facts to
            // "Favored Class" the same way the engine attributes class features to
            // their class, instead of us spelling it out in description text.
            ProgressionConfigurator.New("ZFCWSourceMarker", SourceMarkerGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.SourceMarker.Name", "Favored Class", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.SourceMarker.Desc", "", tagEncyclopediaEntries: false))
                .Configure();

            FeatureConfigurator.New("ZFCWFavoredClassBonusHitPoint", HpFeatureGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.HP.Name", "Bonus Hit Point", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.HP.Desc", "Gain +1 hit point.", tagEncyclopediaEntries: false))
                .SetRanks(20)
                .SetIsClassFeature(true)
                .AddContextStatBonus(StatType.HitPoints, ContextValues.Rank())
                .AddContextRankConfig(ContextRankConfigs.FeatureRank(HpFeatureGuid))
                .Configure();

            FeatureConfigurator.New("ZFCWFavoredClassBonusSkillRank", SkillFeatureGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.Skill.Name", "Bonus Skill Rank (+1/2)", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.Skill.Desc",
                    "Gain +1/2 skill rank. Every two ranks grant one additional skill point, received at the following level-up.",
                    tagEncyclopediaEntries: false))
                .SetRanks(20)
                .SetIsClassFeature(true)
                .AddComponent<AddSkillPointPerFavoredRank>()
                .AddComponent<PrerequisiteRankProgressDisplay>(c =>
                {
                    c.m_Fact = BlueprintTool.GetRef<BlueprintUnitFactReference>(SkillFeatureGuid);
                    c.Step = 2;
                })
                .Configure();

            var classExtras = BuildRacialBonuses();

            var selectionName = LocalizationTool.CreateString("ZFCW.Selection.Name", "Favored Class", tagEncyclopediaEntries: false);
            var selectionDesc = LocalizationTool.CreateString("ZFCW.Selection.Desc",
                "Each character begins play with a single favored class of his choosing — typically, this is the same class as the one he chooses at 1st level. " +
                "Whenever a character gains a level in his favored class, he receives either +1 hit point or +1 skill rank. " +
                "Members of some races may instead select an alternate racial bonus. " +
                "The choice of favored class cannot be changed once the character is created. Prestige classes can never be a favored class.",
                tagEncyclopediaEntries: false);

            // Hidden marker: granted together with any favored class pick; blocks the
            // selection from re-appearing when multiclassing into another class at its level 1.
            FeatureConfigurator.New("ZFCWFavoredClassChosenMarker", MarkerGuid)
                .SetDisplayName(selectionName)
                .SetDescription(selectionDesc)
                .SetIsClassFeature(true)
                .SetHideInUI(true)
                .SetHideInCharacterSheetAndLevelUp(true)
                .Configure();

            var progressionGuids = new List<string>();
            foreach (var cls in BlueprintRoot.Instance.Progression.CharacterClasses)
            {
                if (cls == null || cls.PrestigeClass) continue;
                var clsGuid = cls.AssetGuid.ToString();
                if (ExcludedClasses.Contains(clsGuid)) continue;

                var bonusSelGuid = MergeIds(clsGuid, BonusSelectionSeed);
                var progGuid = MergeIds(clsGuid, ProgressionSeed);

                var bonusItems = new List<string> { HpFeatureGuid, SkillFeatureGuid };
                if (classExtras.TryGetValue(clsGuid, out var extras)) bonusItems.AddRange(extras);
                bonusItems.AddRange(GlobalBonusExtras);
                FeatureSelectionConfigurator.New($"ZFCWFavoredClass{cls.name}BonusSelection", bonusSelGuid)
                    .SetDisplayName(selectionName)
                    .SetDescription(selectionDesc)
                    .SetIsClassFeature(true)
                    .AddToAllFeatures(bonusItems.Select(g => (Blueprint<BlueprintFeatureReference>)g).ToArray())
                    .Configure();
                BonusSelectionGuids.Add(bonusSelGuid);
                AllModGuids.Add(bonusSelGuid);
                AllModGuids.Add(progGuid);

                // Reuse the class's own LocalizedString: mod classes (e.g. Swashbuckler)
                // may not have their localization resolved yet at install time, so baking
                // cls.Name into a new string produces "null" labels.
                ProgressionConfigurator.New($"ZFCWFavoredClass{cls.name}Progression", progGuid)
                    .SetDisplayName(cls.LocalizedName)
                    .SetDescription(selectionDesc)
                    .SetIsClassFeature(true)
                    .SetClasses(clsGuid)
                    .AddPrerequisiteNoFeature(MarkerGuid)
                    .AddToLevelEntry(1, bonusSelGuid, MarkerGuid)
                    .AddToLevelEntries(bonusSelGuid, from: 2)
                    .Configure();

                progressionGuids.Add(progGuid);
            }

            // Explicit opt-out: grants nothing but the marker, so the choice never
            // re-appears on later multiclassing either.
            FeatureConfigurator.New("ZFCWNoFavoredClass", NoneFeatureGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.None.Name", "No Favored Class", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.None.Desc",
                    "You have no favored class and receive no favored class bonuses.", tagEncyclopediaEntries: false))
                .SetIsClassFeature(true)
                .AddPrerequisiteNoFeature(MarkerGuid)
                .AddFacts(new() { MarkerGuid })
                .Configure();

            FeatureSelectionConfigurator.New("ZFCWFavoredClassSelection", SelectionGuid)
                .SetDisplayName(selectionName)
                .SetDescription(selectionDesc)
                .SetIsClassFeature(true)
                .AddToAllFeatures(progressionGuids.Select(g => (Blueprint<BlueprintFeatureReference>)g)
                    .Append(NoneFeatureGuid).ToArray())
                .Configure();

            // Attach the selection to the END of each class's own level-1 progression
            // entry: class selections are processed last in chargen, so the favored
            // class pick and its bonus pick appear together as the final two cards.
            var selection = BlueprintTool.Get<BlueprintFeatureSelection>(SelectionGuid);
            var selectionRef = selection.ToReference<BlueprintFeatureBaseReference>();
            int attached = 0;
            foreach (var cls in BlueprintRoot.Instance.Progression.CharacterClasses)
            {
                if (cls == null || cls.PrestigeClass) continue;
                if (ExcludedClasses.Contains(cls.AssetGuid.ToString())) continue;
                var entry = cls.Progression?.LevelEntries?.FirstOrDefault(e => e.Level == 1);
                if (entry == null) continue;
                if (entry.m_Features.Any(r => r.Guid.ToString() == SelectionGuid)) continue;
                entry.m_Features.Add(selectionRef);
                attached++;
            }

            foreach (var g in AllModGuids)
            {
                AllModBlueprintGuids.Add(BlueprintGuid.Parse(g));
            }

            BonusDisplays[BlueprintGuid.Parse(HpFeatureGuid)] = (1, BonusDisplayKind.HitPoints);
            BonusDisplays[BlueprintGuid.Parse(SkillFeatureGuid)] = (2, BonusDisplayKind.SkillRanks);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b08")] = (5, BonusDisplayKind.Feet);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b11")] = (5, BonusDisplayKind.Feet);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b09")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0b")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0d")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0f")] = (4, BonusDisplayKind.Flat);
            // Wrapper pick counters are the nested progress features.
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b14")] = (6, BonusDisplayKind.Feats);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b15")] = (6, BonusDisplayKind.Feats);
            // Resource-pool bonuses (Wave 3).
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1d")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1f")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b21")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b23")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b25")] = (2, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b27")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b29")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2b")] = (1, BonusDisplayKind.Flat);
            // "1/6 of a new X" wrapper selections (Wave 4). Keyed on each entry's
            // ProgressGuid, not FeatureGuid — the ProgressGuid feature is the one
            // carrying PrerequisiteRankProgressDisplay and visible in Special
            // Abilities, matching WpFeat/MagicalTail/TeamworkFeat/Cruelty above.
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2e")] = (6, BonusDisplayKind.Feats);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b31")] = (6, BonusDisplayKind.Feats);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b34")] = (6, BonusDisplayKind.Feats);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b37")] = (6, BonusDisplayKind.Feats);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b3a")] = (6, BonusDisplayKind.Feats);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b3d")] = (6, BonusDisplayKind.Feats);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b40")] = (6, BonusDisplayKind.Feats);
            // Wave 5: concentration, maneuvers, damage, resistance, studied target,
            // third-party pools, companion saves, teamwork/cruelty wrappers, monk speed.
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b42")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b43")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b44")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b45")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b46")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b48")] = (3, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4a")] = (3, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4c")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4e")] = (2, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b50")] = (2, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b52")] = (2, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b54")] = (2, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b55")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b57")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b58")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b59")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5a")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5b")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5c")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5d")] = (1, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5e")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b60")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b62")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b64")] = (6, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b68")] = (4, BonusDisplayKind.Flat);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6b")] = (6, BonusDisplayKind.Feats);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6d")] = (5, BonusDisplayKind.Feet);
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6f")] = (4, BonusDisplayKind.Feats);

            Main.Log($"Favored class system installed: {progressionGuids.Count} class progressions, selection attached to {attached} class L1 entries.");
        }

        private sealed class RacialBonusDef
        {
            public string Key;                 // localization/blueprint name stem
            public string FeatureGuid;         // the single visible entry in the bonus list
            public int Divisor = 1;            // effect/reward increment (1 = every pick)
            public int Ranks = 20;
            public string DisplayName;
            public string Description;
            public string[] Races;
            public string[] Classes;           // null => available for every favored class
            public string ProgressGuid;        // wrapper mode: nested progress feature
            public string RewardSelectionGuid; // wrapper mode: nested reward selection at each threshold
            public string[] RewardFeatures;    // items of the reward selection (null = filled manually)
            public string EffectGuid;          // separate effect feature ranked up at each threshold
            public Action<FeatureConfigurator> Components;   // components that stay on the counter itself
        }

        private sealed class EffectDef
        {
            public string Key;
            public string Guid;
            public int Ranks;
            public string DisplayName;
            public string Description;
            public Action<FeatureConfigurator> Components;
        }

        private static Dictionary<string, List<string>> BuildRacialBonuses()
        {
            // Effect features: visible in the character sheet with rank = number of
            // earned whole bonuses; carry the actual mechanical components.
            var effects = new List<EffectDef>
            {
                new()
                {
                    Key = "DodgeEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b16", Ranks = 5,
                    DisplayName = "Dodge Bonus Against Favored Enemies",
                    Description = "+1 dodge bonus to Armor Class against the character's favored enemies per rank.",
                    Components = f => f.AddComponent<ACBonusAgainstFavoredEnemyPerRank>(),
                },
                new()
                {
                    Key = "NaturalACEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b17", Ranks = 5,
                    DisplayName = "Natural Armor Bonus",
                    Description = "+1 natural armor bonus to Armor Class per rank.",
                    Components = f => f
                        .AddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, ContextValues.Rank(), Kingmaker.Enums.ModifierDescriptor.NaturalArmor)
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b17")),
                },
                new()
                {
                    Key = "NecroCLEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b18", Ranks = 5,
                    DisplayName = "Necromancy Caster Level",
                    Description = "+1 caster level for spells of the necromancy school per rank.",
                    Components = f => f.AddComponent<IncreaseSpellSchoolCasterLevelPerRank>(c =>
                        c.School = Kingmaker.Blueprints.Classes.Spells.SpellSchool.Necromancy),
                },
                new()
                {
                    Key = "EnchDCEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b19", Ranks = 5,
                    DisplayName = "Enchantment Spell DC",
                    Description = "+1 to the DC of enchantment spells per rank.",
                    Components = f => f.AddComponent<IncreaseSpellSchoolDCPerRank>(c =>
                        c.School = Kingmaker.Blueprints.Classes.Spells.SpellSchool.Enchantment),
                },
                new()
                {
                    Key = "SpeedEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1a", Ranks = 4,
                    DisplayName = "Bonus Speed",
                    Description = "+5 feet to base speed per rank.",
                    Components = f => f
                        .AddComponent<Kingmaker.UnitLogic.FactLogic.AddContextStatBonus>(c =>
                        {
                            c.Stat = Kingmaker.EntitySystem.Stats.StatType.Speed;
                            c.Value = ContextValues.Rank();
                            c.Multiplier = 5;
                        })
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1a")),
                },
                new()
                {
                    Key = "SkillEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1b", Ranks = 10,
                    DisplayName = "Bonus Skill Ranks",
                    Description = "+1 skill rank per rank.",
                    Components = null, // the skill points themselves are granted by the counter's component
                },
                new()
                {
                    Key = "BombsEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b26", Ranks = 20,
                    DisplayName = "Bonus Bombs",
                    Description = "+1 bomb per day per rank.",
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(AlchemistBombsResourceGuid)),
                },
                new()
                {
                    Key = "KiPoolEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b28", Ranks = 20,
                    DisplayName = "Bonus Ki Pool",
                    Description = "+1 ki point per rank.",
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(KiPowerResourceGuid)),
                },
                new()
                {
                    Key = "ArcanePoolEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2a", Ranks = 20,
                    DisplayName = "Bonus Arcane Pool",
                    Description = "+1 arcane pool point per rank.",
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(ArcanePoolResourceGuid)),
                },
                new()
                {
                    Key = "CMBGrappleTripEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b47", Ranks = 5,
                    DisplayName = "Grapple and Trip Bonus",
                    Description = "+1 to combat maneuver checks made to grapple or trip per rank.",
                    Components = f => f
                        .AddCMBBonusForManeuver(checkFact: false,
                            descriptor: Kingmaker.Enums.ModifierDescriptor.UntypedStackable,
                            maneuvers: new[] { Kingmaker.RuleSystem.Rules.CombatManeuver.Grapple, Kingmaker.RuleSystem.Rules.CombatManeuver.Trip },
                            value: ContextValues.Rank())
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b47")),
                },
                new()
                {
                    Key = "CMBDisarmEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b49", Ranks = 6,
                    DisplayName = "Disarm Bonus",
                    Description = "+1 to combat maneuver checks made to disarm per rank.",
                    Components = f => f
                        .AddCMBBonusForManeuver(checkFact: false,
                            descriptor: Kingmaker.Enums.ModifierDescriptor.UntypedStackable,
                            maneuvers: new[] { Kingmaker.RuleSystem.Rules.CombatManeuver.Disarm },
                            value: ContextValues.Rank())
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b49")),
                },
                new()
                {
                    Key = "EarthBlastDmgEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4b", Ranks = 6,
                    DisplayName = "Earth Blast Damage",
                    Description = "+1 damage with earth element blasts per rank.",
                    Components = f => f.AddComponent<AbilityDamageBonusPerRank>(c =>
                        c.m_Abilities = EarthBlastBaseGuids.Select(g =>
                            BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray()),
                },
                new()
                {
                    Key = "AllBlastDmgEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4d", Ranks = 5,
                    DisplayName = "Elemental Blast Damage",
                    Description = "+1 damage with elemental blasts per rank.",
                    Components = f => f.AddComponent<AbilityDamageBonusPerRank>(c =>
                        c.m_Abilities = AllBlastBaseGuids.Select(g =>
                            BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray()),
                },
                new()
                {
                    Key = "AcidDmgEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4f", Ranks = 10,
                    DisplayName = "Acid Spell Damage",
                    Description = "+1 damage with spells that deal acid damage per rank.",
                    Components = f => f.AddComponent<SpellDescriptorDamageBonusPerRank>(c =>
                        c.Descriptors = Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Acid),
                },
                new()
                {
                    Key = "FireDmgEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b51", Ranks = 10,
                    DisplayName = "Fire Spell Damage",
                    Description = "+1 damage with spells that deal fire damage per rank.",
                    Components = f => f.AddComponent<SpellDescriptorDamageBonusPerRank>(c =>
                        c.Descriptors = Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Fire),
                },
                new()
                {
                    Key = "NegativeDmgEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b53", Ranks = 10,
                    DisplayName = "Negative Energy Spell Damage",
                    Description = "+1 damage with spells that deal negative energy damage per rank.",
                    Components = f => f.AddComponent<EnergyTypeDamageBonusPerRank>(c =>
                        c.EnergyType = Kingmaker.Enums.Damage.DamageEnergyType.NegativeEnergy),
                },
                new()
                {
                    Key = "FavoredEnemyAtkDmgEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b56", Ranks = 5,
                    DisplayName = "Favored Enemy Attack and Damage Bonus",
                    Description = "+1 to attack and damage rolls against favored enemies per rank.",
                    Components = f => f
                        .AddComponent<AttackBonusAgainstFavoredEnemyPerRank>()
                        .AddComponent<DamageBonusAgainstFavoredEnemyPerRank>(),
                },
                new()
                {
                    Key = "StudiedDodgeEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5f", Ranks = 5,
                    DisplayName = "Dodge Bonus Against Studied Targets",
                    Description = "+1 dodge bonus to Armor Class against the character's studied targets per rank.",
                    Components = f => f.AddComponent<ACBonusAgainstCasterBuffPerRank>(c =>
                        c.m_Buffs = new[]
                        {
                            BlueprintTool.GetRef<Kingmaker.Blueprints.BlueprintBuffReference>(SlayerStudyTargetBuffGuid),
                            BlueprintTool.GetRef<Kingmaker.Blueprints.BlueprintBuffReference>(SlayerDefensiveStudyBuffGuid),
                        }),
                },
                new()
                {
                    Key = "PanacheEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b61", Ranks = 5,
                    DisplayName = "Bonus Panache",
                    Description = "+1 panache point per rank.",
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(PanacheResourceGuid)),
                },
                new()
                {
                    Key = "CharmedLifeEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b63", Ranks = 5,
                    DisplayName = "Bonus Charmed Life",
                    Description = "+1 use per day of charmed life per rank.",
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(CharmedLifeResourceGuid)),
                },
                new()
                {
                    Key = "JudgmentEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b65", Ranks = 3,
                    DisplayName = "Bonus Judgment",
                    Description = "+1 use per day of judgment per rank.",
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(JudgmentResourceGuid)),
                },
            };
            foreach (var e in effects)
            {
                var conf = FeatureConfigurator.New($"ZFCW{e.Key}", e.Guid)
                    .SetDisplayName(LocalizationTool.CreateString($"ZFCW.{e.Key}.Name", e.DisplayName, tagEncyclopediaEntries: false))
                    .SetDescription(LocalizationTool.CreateString($"ZFCW.{e.Key}.Desc", e.Description, tagEncyclopediaEntries: false))
                    .SetRanks(e.Ranks)
                    .SetIsClassFeature(true);
                e.Components?.Invoke(conf);
                conf.Configure();
                AllModGuids.Add(e.Guid);
            }
            EffectGrants[BlueprintGuid.Parse(SkillFeatureGuid)] = (2, "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1b");

            // Companion-side features: kept on the player's animal companion by the
            // GrantFeatureToPetsWhileActive component on the owner's counter. Their
            // magnitude reads the MASTER's counter rank (native MasterFeatureRank),
            // so they are self-scaling and inert if the master somehow lacks ranks.
            FeatureConfigurator.New("ZFCWCompanionDRFeature", CompanionDRPetGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.CompanionDRFeature.Name", "Companion Damage Reduction", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.CompanionDRFeature.Desc",
                    "This animal companion has DR/magic granted by its master's favored class bonus (maximum DR 10/magic).", tagEncyclopediaEntries: false))
                .SetIsClassFeature(true)
                .AddDamageResistancePhysical(bypassedByMagic: true, value: ContextValues.Rank())
                .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b66", useMaster: true, max: 19).WithOnePlusDiv2Progression())
                .Configure();
            AllModGuids.Add(CompanionDRPetGuid);

            FeatureConfigurator.New("ZFCWCompanionSavesFeature", CompanionSavesPetGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.CompanionSavesFeature.Name", "Companion Saving Throws", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.CompanionSavesFeature.Desc",
                    "This animal companion has a luck bonus on all saving throws granted by its master's favored class bonus.", tagEncyclopediaEntries: false))
                .SetIsClassFeature(true)
                .AddContextStatBonus(StatType.SaveFortitude, ContextValues.Rank(), Kingmaker.Enums.ModifierDescriptor.Luck)
                .AddContextStatBonus(StatType.SaveReflex, ContextValues.Rank(), Kingmaker.Enums.ModifierDescriptor.Luck)
                .AddContextStatBonus(StatType.SaveWill, ContextValues.Rank(), Kingmaker.Enums.ModifierDescriptor.Luck)
                .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b68", useMaster: true, max: 20).WithDivStepProgression(4))
                .Configure();
            AllModGuids.Add(CompanionSavesPetGuid);

            var defs = new List<RacialBonusDef>
            {
                new()
                {
                    Key = "WpFeat", FeatureGuid = WpCombatPartialGuid,
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Combat Feat (+1/6)",
                    Description = "Gain 1/6 of a new bonus combat feat.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid },
                    Classes = new[] { WarpriestClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b14",
                    RewardSelectionGuid = WpCombatSelGuid,
                    RewardFeatures = null, // feat list mirrored from vanilla after the loop
                },
                new()
                {
                    Key = "SpeedBarbarian", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b08",
                    Divisor = 5, Ranks = 20,
                    DisplayName = "Bonus Speed (+1 ft.)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1a",
                    Description = "Add +1 to the barbarian's base speed. In combat this option has no effect unless the barbarian has selected it five times (or another increment of five). " +
                        "This bonus stacks with the barbarian's fast movement feature and applies under the same conditions as that feature.",
                    Races = new[] { ElfRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { BarbarianClassGuid },
                },
                new()
                {
                    Key = "SpeedBloodrager", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b11",
                    Divisor = 5, Ranks = 20,
                    DisplayName = "Bonus Speed (+1 ft.)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1a",
                    Description = "Add +1 to the bloodrager's base speed. In combat this option has no effect unless the bloodrager has selected it five times (or another increment of five). " +
                        "This bonus stacks with the bloodrager's fast movement feature and applies under the same conditions as that feature.",
                    Races = new[] { ElfRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { BloodragerClassGuid },
                },
                new()
                {
                    Key = "Dodge", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b09",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Dodge Bonus Against Favored Enemies (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b16",
                    Description = "Add a +1/4 dodge bonus to Armor Class against the ranger's favored enemies.",
                    Races = new[] { HalflingRaceGuid },
                    Classes = new[] { RangerClassGuid },
                },
                new()
                {
                    Key = "MagicalTail", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b13",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Magical Tail (+1/6)",
                    Description = "Gain 1/6 of a new Magical Tail feat.",
                    Races = new[] { KitsuneRaceGuid },
                    Classes = null, // any kitsune character, upon gaining a level in her favored class
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b15",
                    RewardSelectionGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b12",
                    RewardFeatures = new[]
                    {
                        "febb8fe9a2d142fb80c1be6b0b539d9d", // MagicalTail
                        "5114829572da5a04f896a8c5b67be413", "c032f65c0bd9f6048a927fb07fc0195d",
                        "d5050e13742d9b64da20921aaf7c2b2a", "342b6aed6b2eaab4786de243f0bcbcb8",
                        "044cd84818c36854abf61064ade542a1", "053e37697a0d20547b06c3dbd8b71702",
                        "041f91c25586d48469dce6b4575053f6", "df186ef345849d149bdbf4ddb45aee35",
                    },
                },
                new()
                {
                    Key = "NaturalAC", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0b",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Natural Armor Bonus (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b17",
                    Description = "Gain a +1/4 natural armor bonus to Armor Class.",
                    Races = new[] { DwarfRaceGuid },
                    Classes = new[] { AlchemistClassGuid },
                },
                new()
                {
                    Key = "NecroCL", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0d",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Necromancy Caster Level (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b18",
                    Description = "Add +1/4 to the character's caster level when casting spells of the necromancy school.",
                    Races = new[] { DhampirRaceGuid },
                    Classes = new[] { WizardClassGuid },
                },
                new()
                {
                    Key = "EnchDC", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0f",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Enchantment Spell DC (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b19",
                    Description = "Add +1/4 to the DC of enchantment spells.",
                    Races = new[] { KitsuneRaceGuid },
                    Classes = new[] { SorcererClassGuid },
                },
                new()
                {
                    Key = "RageRounds", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1d",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Bonus Rage Rounds",
                    Description = "Add +1 to the number of rounds per day the barbarian can rage.",
                    Races = new[] { DwarfRaceGuid, HalfOrcRaceGuid },
                    Classes = new[] { BarbarianClassGuid },
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(RageResourceGuid)),
                },
                new()
                {
                    Key = "BloodrageRounds", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1f",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Bonus Bloodrage Rounds",
                    Description = "Add +1 to the number of rounds per day the bloodrager can bloodrage.",
                    Races = new[] { DwarfRaceGuid, HalfOrcRaceGuid, HumanRaceGuid, HalfElfRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { BloodragerClassGuid },
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(BloodragerRageResourceGuid)),
                },
                new()
                {
                    Key = "BardicPerformance", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b21",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Bonus Bardic Performance",
                    Description = "Add +1 to the number of rounds per day the bard can use bardic performance.",
                    Races = new[] { HalfElfRaceGuid, HalfOrcRaceGuid, GnomeRaceGuid, GoblinRaceGuid },
                    Classes = new[] { BardClassGuid },
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(BardicPerformanceResourceGuid)),
                },
                new()
                {
                    Key = "SkaldPerformance", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b23",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Bonus Raging Song",
                    Description = "Add +1 to the number of rounds per day the skald can use raging song.",
                    Races = new[] { HalfElfRaceGuid, HalfOrcRaceGuid },
                    Classes = new[] { SkaldClassGuid },
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(RagingSongResourceGuid)),
                },
                new()
                {
                    Key = "Bombs", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b25",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Bonus Bombs (+1/2)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b26",
                    Description = "Add +1/2 to the number of bombs per day the alchemist can create.",
                    Races = new[] { GnomeRaceGuid, HobgoblinRaceGuid },
                    Classes = new[] { AlchemistClassGuid },
                },
                new()
                {
                    Key = "KiPool", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b27",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Bonus Ki Pool (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b28",
                    Description = "Add +1/4 point to the monk's ki pool.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { MonkClassGuid },
                },
                new()
                {
                    Key = "ArcanePool", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b29",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Bonus Arcane Pool (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2a",
                    Description = "Add +1/4 point to the magus's arcane pool.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid, SuliRaceGuid, FetchlingRaceGuid },
                    Classes = new[] { MagusClassGuid },
                },
                new()
                {
                    Key = "ArcaneReservoir", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2b",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Bonus Arcane Reservoir",
                    Description = "Add +1 point to the arcanist's arcane reservoir.",
                    Races = new[] { ElfRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { ArcanistClassGuid },
                    Components = f => f.AddComponent<IncreaseResourceAmountPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(ArcanistArcaneReservoirResourceGuid)),
                },
                new()
                {
                    Key = "RogueTalent", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2d",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Rogue Talent (+1/6)",
                    Description = "Gain 1/6 of a new rogue talent.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { RogueClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2e",
                    RewardSelectionGuid = RogueTalentRewardGuid,
                    RewardFeatures = null, // mirrored from vanilla after the loop
                },
                new()
                {
                    Key = "WitchHex", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b30",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Witch Hex (+1/6)",
                    Description = "Gain 1/6 of a new witch hex.",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { WitchClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b31",
                    RewardSelectionGuid = WitchHexRewardGuid,
                    RewardFeatures = null,
                },
                new()
                {
                    Key = "ArcanistExploit", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b33",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Arcanist Exploit (+1/6)",
                    Description = "Gain 1/6 of a new arcanist exploit.",
                    Races = new[] { HalflingRaceGuid },
                    Classes = new[] { ArcanistClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b34",
                    RewardSelectionGuid = ArcanistExploitRewardGuid,
                    RewardFeatures = null,
                },
                new()
                {
                    Key = "ShamanHex", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b36",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Shaman Hex (+1/6)",
                    Description = "Gain 1/6 of a new shaman hex.",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { ShamanClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b37",
                    RewardSelectionGuid = ShamanHexRewardGuid,
                    RewardFeatures = null,
                },
                new()
                {
                    Key = "SlayerTalent", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b39",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Slayer Talent (+1/6)",
                    Description = "Gain 1/6 of a new slayer talent.",
                    Races = new[] { HumanRaceGuid, GnomeRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { SlayerClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b3a",
                    RewardSelectionGuid = SlayerTalentRewardGuid,
                    RewardFeatures = null, // mirrors the base (level 2) talent pool only — see VanillaSlayerTalentSel note
                },
                new()
                {
                    Key = "KineticistWildTalent", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b3c",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Wild Talent (+1/6)",
                    Description = "Gain 1/6 of a new wild talent.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { KineticistClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b3d",
                    RewardSelectionGuid = KineticistWildTalentRewardGuid,
                    RewardFeatures = null,
                },
                new()
                {
                    Key = "MagusArcana", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b3f",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Magus Arcana (+1/6)",
                    Description = "Gain 1/6 of a new magus arcana.",
                    Races = new[] { ElfRaceGuid, HalflingRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { MagusClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b40",
                    RewardSelectionGuid = MagusArcanaRewardGuid,
                    RewardFeatures = null,
                },
                // Wave 5: concentration, combat maneuvers, spell/blast damage, favored
                // enemy attack/damage, energy resistance, studied target, third-party
                // resource pools, companion bonuses, teamwork feats, cruelties.
                new()
                {
                    Key = "ConcPaladin", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b42",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Concentration Bonus",
                    Description = "Add a +1 bonus on concentration checks when casting spells.",
                    Races = new[] { DwarfRaceGuid },
                    Classes = new[] { PaladinClassGuid },
                    Components = f => f.AddComponent<ConcentrationBonusPerRank>(),
                },
                new()
                {
                    Key = "ConcArcanist", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b43",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Concentration Bonus",
                    Description = "Add a +1 bonus on concentration checks when casting spells.",
                    Races = new[] { HalfOrcRaceGuid },
                    Classes = new[] { ArcanistClassGuid },
                    Components = f => f.AddComponent<ConcentrationBonusPerRank>(),
                },
                new()
                {
                    Key = "ConcInquisitor", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b44",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Concentration Bonus",
                    Description = "Add a +1 bonus on concentration checks when casting spells.",
                    Races = new[] { HobgoblinRaceGuid },
                    Classes = new[] { InquisitorClassGuid },
                    Components = f => f.AddComponent<ConcentrationBonusPerRank>(),
                },
                new()
                {
                    Key = "ConcBloodrager", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b45",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Concentration Bonus",
                    Description = "Add a +1 bonus on concentration checks when casting spells.",
                    Races = new[] { DrowRaceGuid },
                    Classes = new[] { BloodragerClassGuid },
                    Components = f => f.AddComponent<ConcentrationBonusPerRank>(),
                },
                new()
                {
                    Key = "CMBGrappleTrip", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b46",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Grapple and Trip Bonus (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b47",
                    Description = "Add +1/4 to the monk's combat maneuver checks made to grapple or trip.",
                    Races = new[] { HobgoblinRaceGuid },
                    Classes = new[] { MonkClassGuid },
                },
                new()
                {
                    Key = "CMBDisarm", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b48",
                    Divisor = 3, Ranks = 18,
                    DisplayName = "Disarm Bonus (+1/3)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b49",
                    Description = "Add +1/3 to the fighter's combat maneuver checks made to disarm.",
                    Races = new[] { DrowRaceGuid },
                    Classes = new[] { FighterClassGuid },
                },
                new()
                {
                    Key = "EarthBlastDmg", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4a",
                    Divisor = 3, Ranks = 18,
                    DisplayName = "Earth Blast Damage (+1/3)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4b",
                    Description = "Add +1/3 point of damage to the kineticist's earth element blasts.",
                    Races = new[] { DwarfRaceGuid },
                    Classes = new[] { KineticistClassGuid },
                },
                new()
                {
                    Key = "AllBlastDmg", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4c",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Elemental Blast Damage (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4d",
                    Description = "Add +1/4 point of damage to the kineticist's elemental blasts.",
                    Races = new[] { ElfRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { KineticistClassGuid },
                },
                new()
                {
                    Key = "AcidDmg", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4e",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Acid Spell Damage (+1/2)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b4f",
                    Description = "Add +1/2 point of acid damage to spells that deal acid damage.",
                    Races = new[] { DwarfRaceGuid, OreadRaceGuid },
                    Classes = new[] { SorcererClassGuid },
                },
                new()
                {
                    Key = "FireDmg", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b50",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Fire Spell Damage (+1/2)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b51",
                    Description = "Add +1/2 point of fire damage to spells that deal fire damage.",
                    Races = new[] { HalfOrcRaceGuid },
                    Classes = new[] { SorcererClassGuid, MagusClassGuid },
                },
                new()
                {
                    Key = "NegativeDmgCleric", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b52",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Negative Energy Spell Damage (+1/2)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b53",
                    Description = "Add +1/2 point of negative energy damage to spells that deal negative energy damage.",
                    Races = new[] { HobgoblinRaceGuid, FetchlingRaceGuid },
                    Classes = new[] { ClericClassGuid },
                },
                new()
                {
                    Key = "NegativeDmgOracle", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b54",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Negative Energy Spell Damage (+1/2)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b53",
                    Description = "Add +1/2 point of negative energy damage to spells that deal negative energy damage.",
                    Races = new[] { DhampirRaceGuid },
                    Classes = new[] { OracleClassGuid },
                },
                new()
                {
                    Key = "FavoredEnemyAtkDmg", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b55",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Favored Enemy Bonus (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b56",
                    Description = "Add +1/4 to attack and damage rolls against the ranger's favored enemies.",
                    Races = new[] { HobgoblinRaceGuid },
                    Classes = new[] { RangerClassGuid },
                },
                new()
                {
                    Key = "AlchFireRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b57",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Fire Resistance",
                    Description = "Add +1 to the alchemist's fire resistance (maximum +10).",
                    Races = new[] { GoblinRaceGuid },
                    Classes = new[] { AlchemistClassGuid },
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Fire, value: ContextValues.Rank())
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b57")),
                },
                new()
                {
                    Key = "ColdRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b58",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Cold Resistance",
                    Description = "Add +1 to the character's cold resistance (maximum +10).",
                    Races = new[] { FetchlingRaceGuid },
                    Classes = new[] { BarbarianClassGuid, SorcererClassGuid },
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Cold, value: ContextValues.Rank())
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b58")),
                },
                new()
                {
                    Key = "ElecRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b59",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Electricity Resistance",
                    Description = "Add +1 to the character's electricity resistance (maximum +10).",
                    Races = new[] { FetchlingRaceGuid },
                    Classes = new[] { BarbarianClassGuid, SorcererClassGuid },
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Electricity, value: ContextValues.Rank())
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b59")),
                },
                new()
                {
                    Key = "SuliAcidRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5a",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Acid Resistance",
                    Description = "Add +1 to the ranger's acid resistance (maximum +10).",
                    Races = new[] { SuliRaceGuid },
                    Classes = new[] { RangerClassGuid },
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Acid, value: ContextValues.Rank())
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5a")),
                },
                new()
                {
                    Key = "SuliColdRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5b",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Cold Resistance",
                    Description = "Add +1 to the ranger's cold resistance (maximum +10).",
                    Races = new[] { SuliRaceGuid },
                    Classes = new[] { RangerClassGuid },
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Cold, value: ContextValues.Rank())
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5b")),
                },
                new()
                {
                    Key = "SuliFireRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5c",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Fire Resistance",
                    Description = "Add +1 to the ranger's fire resistance (maximum +10).",
                    Races = new[] { SuliRaceGuid },
                    Classes = new[] { RangerClassGuid },
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Fire, value: ContextValues.Rank())
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5c")),
                },
                new()
                {
                    Key = "SuliElecRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5d",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Electricity Resistance",
                    Description = "Add +1 to the ranger's electricity resistance (maximum +10).",
                    Races = new[] { SuliRaceGuid },
                    Classes = new[] { RangerClassGuid },
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Electricity, value: ContextValues.Rank())
                        .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5d")),
                },
                new()
                {
                    Key = "StudiedDodge", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5e",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Dodge Bonus Against Studied Targets (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5f",
                    Description = "Add a +1/4 dodge bonus to Armor Class against the slayer's studied targets.",
                    Races = new[] { HalflingRaceGuid, FetchlingRaceGuid },
                    Classes = new[] { SlayerClassGuid },
                },
                new()
                {
                    Key = "Panache", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b60",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Bonus Panache (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b61",
                    Description = "Add +1/4 point of panache to the swashbuckler's panache pool.",
                    Races = new[] { ElfRaceGuid, HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, TieflingRaceGuid, AasimarRaceGuid },
                    Classes = new[] { SwashbucklerClassGuid },
                },
                new()
                {
                    Key = "CharmedLife", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b62",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Bonus Charmed Life (+1/4)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b63",
                    Description = "Add +1/4 to the number of times per day the swashbuckler can use charmed life.",
                    Races = new[] { GnomeRaceGuid, HalflingRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { SwashbucklerClassGuid },
                },
                new()
                {
                    Key = "Judgment", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b64",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Judgment (+1/6)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b65",
                    Description = "Add +1/6 to the number of times per day the inquisitor can use the judgment class feature.",
                    Races = new[] { DuergarRaceGuid },
                    Classes = new[] { InquisitorClassGuid },
                },
                new()
                {
                    Key = "CompanionDR", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b66",
                    Divisor = 1, Ranks = 19,
                    DisplayName = "Companion Damage Reduction",
                    Description = "The character's animal companion gains DR 1/magic. Each additional time this bonus is selected, the DR increases by 1/2 (maximum DR 10/magic).",
                    Races = new[] { GnomeRaceGuid, FetchlingRaceGuid },
                    Classes = new[] { HunterClassGuid, RangerClassGuid },
                    Components = f => f.AddComponent<GrantFeatureToPetsWhileActive>(c =>
                        c.m_Feature = BlueprintTool.GetRef<BlueprintUnitFactReference>(CompanionDRPetGuid)),
                },
                new()
                {
                    Key = "CompanionSaves", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b68",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Companion Saving Throws (+1/4)",
                    Description = "Add +1/4 luck bonus on the saving throws of the character's animal companion.",
                    Races = new[] { HalflingRaceGuid },
                    Classes = new[] { HunterClassGuid, DruidClassGuid },
                    Components = f => f.AddComponent<GrantFeatureToPetsWhileActive>(c =>
                        c.m_Feature = BlueprintTool.GetRef<BlueprintUnitFactReference>(CompanionSavesPetGuid)),
                },
                new()
                {
                    Key = "TeamworkFeat", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6a",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Teamwork Feat (+1/6)",
                    Description = "Gain 1/6 of a new teamwork feat.",
                    Races = new[] { DrowRaceGuid },
                    Classes = new[] { InquisitorClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6b",
                    RewardSelectionGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6c",
                    RewardFeatures = null, // mirrored from the inquisitor teamwork pool
                },
                new()
                {
                    Key = "SpeedMonk", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6d",
                    Divisor = 5, Ranks = 20,
                    DisplayName = "Bonus Speed (+1 ft.)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1a",
                    Description = "Add +1 to the monk's base speed. In combat this option has no effect unless the monk has selected it five times (or another increment of five). " +
                        "This bonus stacks with the monk's fast movement feature and applies under the same conditions as that feature.",
                    Races = new[] { ElfRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { MonkClassGuid },
                },
                new()
                {
                    Key = "Cruelty", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6e",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Bonus Cruelty (+1/4)",
                    Description = "Gain 1/4 of a new cruelty.",
                    Races = new[] { DrowRaceGuid },
                    Classes = new[] { AntipaladinClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6f",
                    RewardSelectionGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b70",
                    RewardFeatures = null, // mirrored from MCE's cruelty selection
                },
            };

            var extras = new Dictionary<string, List<string>>();
            var globalExtras = new List<string>();
            foreach (var def in defs)
            {
                var races = def.Races.Select(g => BlueprintTool.GetRef<BlueprintRaceReference>(g)).ToArray();
                var name = LocalizationTool.CreateString($"ZFCW.{def.Key}.Name", def.DisplayName, tagEncyclopediaEntries: false);
                var desc = LocalizationTool.CreateString($"ZFCW.{def.Key}.Desc", def.Description, tagEncyclopediaEntries: false);

                if (def.RewardSelectionGuid == null)
                {
                    // Plain accumulative feature: effect scales as floor(rank / divisor).
                    var conf = FeatureConfigurator.New($"ZFCW{def.Key}Bonus", def.FeatureGuid)
                        .SetDisplayName(name)
                        .SetDescription(desc)
                        .SetRanks(def.Ranks)
                        .SetIsClassFeature(true)
                        .AddComponent<PrerequisiteRaceAny>(c => c.m_Races = races);
                    if (def.Divisor > 1)
                    {
                        conf = conf.AddComponent<PrerequisiteRankProgressDisplay>(c =>
                        {
                            c.m_Fact = BlueprintTool.GetRef<BlueprintUnitFactReference>(def.FeatureGuid);
                            c.Step = def.Divisor;
                        });
                    }
                    def.Components?.Invoke(conf);
                    conf.Configure();
                }
                else
                {
                    // Wrapper mode: the visible entry is itself a selection — the native
                    // way the game unfolds a choice inside a picked feature. Inside it:
                    // a progress feature (available between thresholds) and the reward
                    // selection (available on every divisor-th pick), gated by the
                    // rank-cycle prerequisite pair.
                    var progressRef = BlueprintTool.GetRef<BlueprintUnitFactReference>(def.ProgressGuid);
                    var rewardRef = BlueprintTool.GetRef<BlueprintUnitFactReference>(def.RewardSelectionGuid);

                    FeatureConfigurator.New($"ZFCW{def.Key}Progress", def.ProgressGuid)
                        .SetDisplayName(name)
                        .SetDescription(desc)
                        .SetRanks(def.Ranks)
                        .SetIsClassFeature(true)
                        .AddComponent<PrerequisiteFactRankCycle>(c =>
                        {
                            c.m_Partial = progressRef;
                            c.m_Full = rewardRef;
                            c.Divisor = def.Divisor;
                            c.Not = true;
                        })
                        .Configure();
                    AllModGuids.Add(def.ProgressGuid);

                    var rewardConf = FeatureSelectionConfigurator.New($"ZFCW{def.Key}Reward", def.RewardSelectionGuid)
                        .SetDisplayName(LocalizationTool.CreateString($"ZFCW.{def.Key}.Reward.Name",
                            def.DisplayName.Replace(" (+1/" + def.Divisor + ")", ""), tagEncyclopediaEntries: false))
                        .SetDescription(desc)
                        .SetRanks(Math.Max(1, def.Ranks / def.Divisor))
                        .SetIsClassFeature(true)
                        .AddComponent<PrerequisiteFactRankCycle>(c =>
                        {
                            c.m_Partial = progressRef;
                            c.m_Full = rewardRef;
                            c.Divisor = def.Divisor;
                            c.Not = false;
                        });
                    if (def.RewardFeatures != null)
                    {
                        rewardConf = rewardConf.AddToAllFeatures(
                            def.RewardFeatures.Select(g => (Blueprint<BlueprintFeatureReference>)g).ToArray());
                    }
                    rewardConf.Configure();
                    AllModGuids.Add(def.RewardSelectionGuid);

                    FeatureSelectionConfigurator.New($"ZFCW{def.Key}Bonus", def.FeatureGuid)
                        .SetDisplayName(name)
                        .SetDescription(desc)
                        .SetRanks(def.Ranks)
                        .SetIsClassFeature(true)
                        .AddComponent<PrerequisiteRaceAny>(c => c.m_Races = races)
                        .AddComponent<PrerequisiteRankProgressDisplay>(c =>
                        {
                            c.m_Fact = BlueprintTool.GetRef<BlueprintUnitFactReference>(def.FeatureGuid);
                            c.Step = def.Divisor;
                        })
                        .AddToAllFeatures(def.ProgressGuid, def.RewardSelectionGuid)
                        .Configure();
                }
                AllModGuids.Add(def.FeatureGuid);
                if (def.EffectGuid != null)
                {
                    EffectGrants[BlueprintGuid.Parse(def.FeatureGuid)] = (def.Divisor, def.EffectGuid);
                }
                if (def.RewardSelectionGuid != null)
                {
                    RewardPickCounters[BlueprintGuid.Parse(def.RewardSelectionGuid)] = def.ProgressGuid;
                }

                if (def.Classes == null)
                {
                    globalExtras.Add(def.FeatureGuid);
                }
                else
                {
                    foreach (var cls in def.Classes)
                    {
                        if (!extras.TryGetValue(cls, out var list)) extras[cls] = list = new List<string>();
                        list.Add(def.FeatureGuid);
                    }
                }
            }

            // Retired blueprints from the two-entry (partial/full) design: kept
            // registered so existing test saves load, hidden and granted nowhere.
            foreach (var (retiredGuid, idx) in new[]
                     {
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0a", 1), // old Dodge partial
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0c", 2), // old NaturalAC partial
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0e", 3), // old NecroCL partial
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b10", 4), // old EnchDC partial
                     })
            {
                FeatureConfigurator.New($"ZFCWRetiredPartial{idx}", retiredGuid)
                    .SetDisplayName(LocalizationTool.CreateString($"ZFCW.Retired{idx}.Name", "Favored Class Bonus (retired)", tagEncyclopediaEntries: false))
                    .SetDescription(LocalizationTool.CreateString($"ZFCW.Retired{idx}.Desc", "This bonus has been merged into a single option.", tagEncyclopediaEntries: false))
                    .SetHideInUI(true)
                    .SetHideInCharacterSheetAndLevelUp(true)
                    .SetRanks(20)
                    .Configure();
                AllModGuids.Add(retiredGuid);
            }

            // Wrapper reward pools mirror their source class selection (including
            // feats/talents/hexes/arcana added by other mods by this point). Sources
            // owned by optional third-party mods (MCE cruelties) may be absent — skip
            // those gracefully, leaving our reward selection empty.
            foreach (var (ourGuid, sourceGuid) in new[]
                     {
                         (WpCombatSelGuid, VanillaWarpriestFeatSel),
                         (RogueTalentRewardGuid, VanillaRogueTalentSel),
                         (WitchHexRewardGuid, VanillaWitchHexSel),
                         (ArcanistExploitRewardGuid, VanillaArcanistExploitSel),
                         (ShamanHexRewardGuid, VanillaShamanHexSel),
                         (SlayerTalentRewardGuid, VanillaSlayerTalentSel),
                         (KineticistWildTalentRewardGuid, VanillaWildTalentSel),
                         (MagusArcanaRewardGuid, VanillaMagusArcanaSel),
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6c", VanillaInquisitorTeamworkFeatSel),
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b70", MceAntipaladinCrueltySel),
                     })
            {
                var sourceSel = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>(BlueprintGuid.Parse(sourceGuid));
                if (sourceSel == null)
                {
                    Main.Log($"Reward pool source missing (optional mod not installed?), skipping mirror: {sourceGuid}");
                    continue;
                }
                var ourSel = BlueprintTool.Get<BlueprintFeatureSelection>(ourGuid);
                ourSel.m_AllFeatures = sourceSel.m_AllFeatures.ToArray();
                ourSel.Group = sourceSel.Group;
            }

            GlobalBonusExtras = globalExtras;
            return extras;
        }

        private static List<string> GlobalBonusExtras = new();

        private static ProgressionConfigurator AddToLevelEntries(this ProgressionConfigurator conf, string feature, int from = 1)
        {
            for (int lvl = from; lvl <= 20; lvl++)
            {
                conf = conf.AddToLevelEntry(lvl, feature);
            }
            return conf;
        }

        private static string MergeIds(string guidA, string guidB)
        {
            var a = Guid.Parse(guidA).ToByteArray();
            var b = Guid.Parse(guidB).ToByteArray();
            var merged = new byte[16];
            for (int i = 0; i < 16; i++) merged[i] = (byte)(a[i] ^ b[i]);
            return new Guid(merged).ToString("N");
        }
    }
}
