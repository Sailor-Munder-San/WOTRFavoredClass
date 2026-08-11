using System;
using System.Collections.Generic;
using System.Linq;
using BlueprintCore.Blueprints.Configurators.Classes;
using BlueprintCore.Blueprints.Configurators.Classes.Selection;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.Classes.Selection;
using BlueprintCore.Blueprints.CustomConfigurators.Classes.Spells;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;

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
        // What a stat breakdown should call a modifier that came from this mod. Without it the
        // breakdown shows the individual feature's own name ("Bonus Hit Point"), which says
        // nothing about where the bonus came from.
        internal const string BonusSourceLabel = "Favored Class Bonus";
        // Half-elf Multitalented: a SECOND favored class selection, carrying the same
        // options as the first. Attached to the half-elf race rather than to a progression.
        internal const string MultitalentedGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bbd";

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
        private const string GanziRaceGuid = "14be0c2967a842febd853380ad785ce5";
        private const string SvirfneblinRaceGuid = "ee7b945a0cf04c9fa6cb2c29aff3f4a8";
        private const string SamsaranRaceGuid = "69ad3e90baf7442c9df956170c7206f0";
        private const string ChangelingRaceGuid = "bc9b8d879d104455895c98e31f8d8503";
        private const string NagajiRaceGuid = "4e6e156b707f47c5993ac9262ca19a56";
        private const string OrcRaceGuid = "7088a348ef0646dabdb3900fb187fb21";

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
        private const string CavalierClassGuid = "3adc3439f98cb534ba98df59838f02c7";
        private const string ShifterClassGuid = "a406d6ebea5c46bba3160246be03e96f";
        // Third-party classes (Swashbuckler mod, MicroscopicContentExpansion).
        // Class-keyed bonuses only attach if the class is actually in the game's list.
        private const string SwashbucklerClassGuid = "338abf2723c14c1ab0f17cd7e3020444";
        private const string AntipaladinClassGuid = "8939eff25a0a4b77ad1ab6be4c760a6c";

        // Slayer studied-target buffs: same blueprint GUIDs as Kingmaker (Owlcat
        // carried the slayer content over unchanged) — the buff sits on the studied
        // ENEMY with the slayer as caster.
        private const string SlayerStudyTargetBuffGuid = "45548967b714e254aa83f23354f174b0";
        private const string SlayerDefensiveStudyBuffGuid = "cbbff1a2e7a3a5b47b41406701de305b";

        // Cavalier challenge: the vanilla challenge ability puts this buff on the challenged
        // foe with the cavalier as its caster, which is what the damage bonus keys off.
        private const string CavalierChallengeTargetBuffGuid = "4f0218323ad379248b69de8a9501159f";
        private const string CavalierBannerBuffGuid = "4d3b79e464282af4897c1d860bf9e9b3";
        private const string CavalierBannerGreaterBuffGuid = "2f4f532386870824d8f586ae18666f11";

        // Cavalier mount and shifter pet-side counters (same MasterFeatureRank shape as the
        // druid/hunter companion bonuses: the feature lives on the pet, its magnitude reads
        // the master's counter rank).
        private const string CavalierMountHPPetGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1be0";
        private const string CavalierMountSpeedPetGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1be2";
        private const string CavalierMountHPCounterGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bdf";
        private const string CavalierMountSpeedCounterGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1be1";
        // Shifter aspect — the "minor form". Confirmed from the live blueprint to be a real
        // per-day pool (base 3, +1 per class level), not an at-will toggle, which is what makes
        // the tabletop "minutes per day" bonus portable at all. See the shifter entry below.
        private const string ShifterAspectResourceGuid = "1b096f343ea54ae0a4e3b6cf404bf62d";

        // The buffs the seven shifter claw modals apply — one per level tier. These identify
        // "the shifter claws ability is switched on", which the weapon blueprint cannot: only
        // ShifterClaw1d10x3 is shifter-specific, so the lower tiers reuse the generic claw
        // weapons that animal forms also carry. ShifterClawVisualBuff is deliberately excluded:
        // it is cosmetic and says nothing about whether the claws are actually in use.
        private static readonly string[] ShifterClawBuffGuids =
        {
            "02070af90de345c6a82a8cf469a65080", // Level 1
            "1bb67316c37e400888e0489ee8d64067", // Level 3
            "c9441167a3b84fb48729e55f29a9df64", // Level 7
            "13243d59d212463d9ab3f36e646aa40c", // Level 11
            "6e31c78ce801444aad398248b66a22b8", // Level 13
            "cb51194e75ca45bc9fedf9a09c50b827", // Level 17
            "494d127890c3498fb3dbf3a53dcb4fe6", // Level 19
        };

        private const string ChallengeDmgEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bdd";
        private const string ChallengeAoODmgEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bed";
        private const string DefensiveInstinctEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1be9";
        private const string ShifterClawDmgEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bee";

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

        // Wave 6 (bonus known spells): custom filtered/combined spell lists for the
        // two race-specific variants, built once at Install() by reading native
        // class lists' SpellsByLevel and filtering in plain C# (same technique real
        // WOTR mods use — PrestigePlus GraveSpellList.cs, EbonsContentMod
        // FaithMagic.cs — rather than ZFC's Kingmaker-only Common.combineSpellLists).
        private const string GanziOracleSpellListGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b71";
        private const string GoblinSorcererSpellListGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b72";
        private const string ShamanKnownSpellListGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1ba6";

        // Per-level parametrized-feature guids, one literal array per FCB entry
        // (independently-generated — NOT derived via MergeIds/XOR: every guid in this
        // whole file already shares the same "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bXX" prefix,
        // so XOR-ing two of them together cancels that shared prefix to zero and
        // collapses to "prefix + xor-of-last-bytes", which collided with an existing
        // constant in testing — MergeIds is only safe against a high-entropy foreign
        // guid, e.g. a vanilla class guid, which is what its other use in this file does).
        private static readonly string[] AlchemistKnownSpellLevelGuids = { "3200f165d71a4bc2a15d8d777e8bbd1e", "19788ba42ade4fe88b725ee5e31942b1", "783b922e08454720848d9c30d6e2b14e", "9acedbd416f1419c8f14c90b4b7f82dd", "189c87c652a248729477179933b5ff23" };
        private static readonly string[] BardKnownSpellLevelGuids = { "4f7cca11374a493fb4a2adf453d8ab4c", "748a09904a7446fbb0a68a3641528080", "b3daa07db6c54d478b16f376452b5e81", "4ce55f4787774566983e5c5ace5c88b0", "ddf0451d094442f8a24d73cc266fce5f" };
        private static readonly string[] InquisitorKnownSpellLevelGuids = { "894f9e6e33bd453c8fe2d447faf12f8f", "7a086526a0864b7082d9df9ddac41e8a", "8d1c79853e52449e9685d554522cbf91", "4ec953c82f814dc9bfb9a5a9569ab716", "cfb7cac98bad4ecfa7a4b799e534c8c3" };
        private static readonly string[] OracleKnownSpellLevelGuids = { "2bc5c1b5c9a443b99b1eeed5268335b1", "b521fc4dcff043edbf87a5de4801e4e3", "08857e8be02d47d2898c72dcf53ffd1e", "45a33c1babc948a5a6ebbb50e296b350", "b21d44f47f9249ecaafe511a0094fa9b", "45c9270e995241c693535d7466bdc650", "cfd3b9413e3f4dde8d64ab0ec340381b", "387e4f0d029a4bed827af0309247f158" };
        private static readonly string[] OracleGanziKnownSpellLevelGuids = { "84348713728546f9856b26d2de153724", "868bb7bdea7940468000b18dba0c0684", "0ceeee8727ad4f24a29730ee84fa3a68", "d68140a8df214020a67365bb14d7b2d8", "7d65ea731ebb45aebe5aea93bcf7c315", "4eb23edec6384f60bb751c0c9ac6d64d", "074c5af22f69433ba4e2c07164f07d1e", "e7fd4090f3fc492c9835541d375ba913" };
        private static readonly string[] ShamanKnownSpellLevelGuids = { "eddff04ca85c40d4b5acb908ab026967", "dec6e71a3c63469f89859ab84afd73a1", "4ffb2c7710bb4cf893b6e5adef345373", "4d090fbd852048bc996a88e73efc096c", "dfd8f818e2ca46f8822cfe04507b1fa3", "983a9d09140a446d93d24c2f94b2c5a6", "72284da4cda246c3a33eadcf936dd796", "85ac20256261461f931b1da395e1f30b" };
        private static readonly string[] SorcererKnownSpellLevelGuids = { "7a6d3230f4de44f39823444dadf92fd1", "4a915b785b0c4b37bbfdc4ae21c11e1b", "b7f2de97ec034a9fab84fb2c658b48e0", "3ec44340abc74ee79f9d0ab6c04977ba", "be815ad8f7284f778c76b16bfe66ab4e", "12ade9261527452a903e6d978e050021", "31ccd1827d7c42ecb48ac625d7e59ec3", "afe2794b95284068a99c42a6ab225db0" };
        private static readonly string[] SorcererGoblinKnownSpellLevelGuids = { "d8c3a01f517645d0bd2d958e92ddf434", "778c78300e254e1987df4b7056f8da79", "46df8360fa554ff88d383741d181ddfc", "1b74b4920688488dbad663f75c4ff0b9", "cc47112c43d34b708ddd9f88d4c8b35d", "e706f7c22d8f4855a065501a4888dfa2", "170b39b3a1934a8e8f36e641c6d58d03", "2581f00e8f404ec19cbe5e38625fdcfa" };
        private static readonly string[] WizardKnownSpellLevelGuids = { "3dc6c03a61ac42b19a4d84c6979542b1", "5fa4469c45f84e52b364725160f9ffe8", "8a07aa06e2854824b94fc515aa925c3e", "47b3337f98bd4d6f978cf487a13f9862", "7cbeebfcc4d3407180660af2603d0179", "cd579f88d3ac440eac8c2eef096a8b9c", "c1304c60351242cfae46a1c23faa85e1", "78fd958f33d84d6699c3e903f0a235f6" };
        private static readonly string[] WizardKnownSpellAbjurationLevelGuids = { "533d9f94b52f44b4b9590412d31841e2", "179f3ed5982d424fa063831e217b7531", "c2b6462b0ba447709f8cec571c411dfe", "d72d04ad6a0541f288213efe4db3f8b4", "4dad98ec4fb742dc9982cd23ca58aff1", "02941f60767744238660ead7590941b1", "ed41bf16fa3c4440914d6d1f96c605aa", "cc7ceee0110d496fa2395b35b1e72b77" };
        private static readonly string[] WizardKnownSpellConjurationLevelGuids = { "7697165f855f4d4da12fd37e3538abba", "a8a5d0d65dd0411da8336de64433206d", "c43c1007338746c4a7a1a9efcfc46a7e", "f7629ce104b74775860695f9a2cc158d", "936f78b358e54b6f9b8b8b205c7be061", "daf71e0ab04449539680c70e69c67568", "5802c3ced97146409e8bf853d44514aa", "04d8c2ee1b8144e8a36076c36cef2212" };
        private static readonly string[] WizardKnownSpellEnchantmentLevelGuids = { "cc1fe866f7104c8e801d8b9d60c755d4", "274cc4b2a2974848aab09af1a5b20a86", "eb07cc98807b45db857abe3a76f9c031", "ccc01a688792479f9a08af44d843501b", "f3f27c0ddad4493a914937e12dcd4067", "78b401e903e7473db50ea6c68faa7c3b", "304dd179a0064533925d15810da4d075", "8e5331807fb442d4b1d8840c78c0c576" };
        private static readonly string[] WizardKnownSpellEvocationLevelGuids = { "f28da48860b745e0bf652fdbafea146b", "9a3e6b4d131d41928f4aef64a7f9b5cf", "3f5c1c2b185e4fa59b6dbe7bd5d073bf", "f7f6c4cb8f1144a395110e03f0811ab5", "110e9ee9d37f4475b9484b77355e94bd", "b27f769435834cfbab24bb7d6a79feca", "7a5e571174084c81b6c901705be8aebd", "8bd46c22f0574bab8ff08a9b82547bd4" };
        private static readonly string[] WizardKnownSpellIllusionLevelGuids = { "20a0ba2985104a218ea99bb3ca7a73f5", "4fd41cfc019a4d1e8170c3134c68511f", "23d790fd24104ec5afb87290853d64ad", "55b80529416e4c0c93086eca3ee9a17e", "d6fe8267c3994b05b6bf2e61a7855e3d", "44e9a6f9531c42e681e766aa0dea3bad", "5909904af47b4a0782263e84fa880798", "b6e5e0b502a04c8084e8063f4f5bad6b" };
        private static readonly string[] WizardKnownSpellNecromancyLevelGuids = { "a846ec451d46405db2c238526e9d5178", "b83226c9cd304203a9bdef5ff48772fa", "fe18ecb540154c8e9fc38b0393dd41b4", "ec18431dfcdf4228b36cb29312c518fe", "19dc672017384077aca1b3def09a73bb", "fb96c05105174011bf0f011b28248ffb", "1f6d5f97ab1941a4b574dfdd3eb4a0b4", "bb85e715f68e491b9759065cf8e772e8" };
        private static readonly string[] WizardKnownSpellTransmutationLevelGuids = { "d2809720b22a49158c0380adbc6fb699", "3e66d89eaccb42ada6bc4ae47fd525cf", "4ff049011d8c44208eafb1e3236d0930", "5abae039ba2c4fd2b51a02f61600c86e", "c2c5c9a222e14dc28f08a8cdde4de790", "af622a9b560147539b0e43aac6aca2c2", "46a7d0ef0cba4682852a9425a61fa630", "1b261c13a91a4a809461005c279cb392" };
        private static readonly string[] WitchKnownSpellLevelGuids = { "b370345bd9f14eebb0cc85ae99db3433", "5aa9b7ef502a478cb1bc460112b99a45", "59a5cb6ed72d4c0491979442fd0eb45f", "02b5cf3bf2d74a99b4d563d0cff14055", "031674c2fcb34c53a9d0cab8c834715a", "67f7df263fce4cb5851c4f59a3d204cf", "e5c5095f0cbf4e0f9ae07362f8e2c54f", "7a1609f6094341b0862bd5ea0e7c7026" };
        private static readonly string[] SkaldKnownSpellLevelGuids = { "6b3d01b9d6f747d181f4f8926d3cf034", "f8c2a82dd13c4cb0998373868937a0ae", "98f37c8bc4f0463eb39374fb0e3a4e92", "af9495fad428431c9e1356e6cf3206a9", "e7efa501b4954e8aa4e3e2233c152ad2" };
        private static readonly string[] ArcanistKnownSpellLevelGuids = { "aaec1142c8324464ab2655c643bb1661", "9ee845b331c44b0f991fd01316a6b313", "0864101e4a1f4a8db3b0dc1dbfc0cd88", "06e1f21852cc40c18e57c70ba67eed52", "0a8d94843fde4d67939d39e064a75cfc", "435876994e614e42b00131a4cc3c8cce", "9b7121153b794dc18a87e30589c38529", "16776b1733bb43d4a8d3a7620220d622" };

        // Wizard's Thassilonian Specialist archetype (7 school variants) replaces the
        // wizard's spellbook with a school-restricted one — confirmed via BlueprintCore's
        // native FeatureReplaceSpellbookRefs/SpellListRefs. Rather than 7 separate FCB
        // entries, the single Wizard "bonus known spell" reward selection carries one
        // extra track per school (own level-guid array, gated on the matching archetype
        // feature) alongside the generic track (gated OFF whenever any of the 7 apply).
        private static readonly (string School, string[] LevelGuids, string ArchetypeFeatureGuid, string SpellListGuid)[] ThassilonianTracks =
        {
            ("Abjuration", WizardKnownSpellAbjurationLevelGuids, "15c681d5a76c1a742abe2760376ddf6d", "280dd5167ccafe449a33fbe93c7a875e"),
            ("Conjuration", WizardKnownSpellConjurationLevelGuids, "1a258cd8e93461a4ab011c73a2c43dac", "5b154578f228c174bac546b6c29886ce"),
            ("Enchantment", WizardKnownSpellEnchantmentLevelGuids, "e1ebc61a71c55054991863a5f6f6d2c2", "ac551db78c1baa34eb8edca088be13cb"),
            ("Evocation", WizardKnownSpellEvocationLevelGuids, "5e33543285d1c3d49b55282cf466bef3", "17c0bfe5b7c8ac3449da655cdcaed4e7"),
            ("Illusion", WizardKnownSpellIllusionLevelGuids, "aa271e69902044b47a8e62c4e58a9dcb", "c311aed33deb7a346ab715baef4a0572"),
            ("Necromancy", WizardKnownSpellNecromancyLevelGuids, "fb343ede45ca1a84496c91c190a847ff", "5c08349132cb6b04181797f58ccf38ae"),
            ("Transmutation", WizardKnownSpellTransmutationLevelGuids, "dd163630abbdace4e85284c55d269867", "f3a8f76b1d030a64084355ba3eea369a"),
        };

        // Arcanist archetypes confirmed (via BlueprintCore SpellbookRefs/SpellListRefs)
        // to replace the arcanist's own spellbook with a dedicated one — excluded from
        // the plain Arcanist "bonus known spell" FCB entirely (per-user ruling; unlike
        // the Thassilonian case, these don't get their own dedicated FCB variant).
        private static readonly string[] ArcanistKnownSpellExcludedArchetypes =
        {
            "44f3ba33839a87f48a66b2b9b2f7c69b", // Unlettered Arcanist
            "26185cfb81b34e778ad370407300de9a", // Nature Mage
            "5c77110cd0414e7eb4c2e485659c9a46", // Magic Deceiver
        };

        // Wave 6: "bonus known spell" wrapper triples (Feature/Progress/Reward).
        private const string AlchemistKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b82";
        private const string AlchemistKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b83";
        private const string AlchemistKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b84";
        private const string BardKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b85";
        private const string BardKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b86";
        private const string BardKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b87";
        private const string InquisitorKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b88";
        private const string InquisitorKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b89";
        private const string InquisitorKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b8a";
        private const string OracleKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b8b";
        private const string OracleKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b8c";
        private const string OracleKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b8d";
        private const string OracleGanziKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b8e";
        private const string OracleGanziKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b8f";
        private const string OracleGanziKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b90";
        private const string ShamanKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b91";
        private const string ShamanKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b92";
        private const string ShamanKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b93";
        private const string SorcererKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b94";
        private const string SorcererKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b95";
        private const string SorcererKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b96";
        private const string SorcererGoblinKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b97";
        private const string SorcererGoblinKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b98";
        private const string SorcererGoblinKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b99";
        private const string WizardKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b9a";
        private const string WizardKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b9b";
        private const string WizardKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b9c";
        private const string WitchKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b9d";
        private const string WitchKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b9e";
        private const string WitchKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b9f";
        private const string SkaldKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1ba0";
        private const string SkaldKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1ba1";
        private const string SkaldKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1ba2";
        private const string ArcanistKnownSpellFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1ba3";
        private const string ArcanistKnownSpellProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1ba4";
        private const string ArcanistKnownSpellRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1ba5";

        // Wave 7: lay on hands, conditional natural armor, eldritch scion arcana,
        // channel energy / fervor pools.
        private const string LayOnHandsEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1ba7";
        private const string LayOnHandsSelfEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1ba8";
        private const string WildShapeACEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bab";
        private const string ChannelEnergyEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bb0";
        private const string FervorEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bb2";
        private const string HarmUndeadEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bb4";

        // Wave 8: corrected favored enemy bonus, arcane reservoir regen, patron spell CL.
        private const string FavoredEnemyPickFeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bb6";
        private const string FavoredEnemyPickProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bb7";
        private const string FavoredEnemyPickRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bb8";
        private const string ReservoirRegenEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bb9";
        private const string PatronCLEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bbb";
        // XOR seed for deriving one reward feature per favored enemy type. Safe use of
        // MergeIds: it is xored against the VANILLA favored-enemy feature guid, which
        // is high-entropy and shares no prefix with ours.
        private const string FavoredEnemyPickSeed = "3eb3fba584b8425b95fc4b643f5c1cd0";

        // Separate seeds per pool: cleric and druid share several domains, so one seed would
        // generate the same pick guid for the same domain in two pools and collide.
        private const string ClericPowerUseSeed = "9c1de3f0a5b8471e8ad2f6c4e7b09d31";
        private const string DruidPowerUseSeed = "4f7a2b6c8d0e4319b5c7e1a3f9d26804";
        private const string WizardPowerUseSeed = "b8e04c17d2a3496f871e5b0c9a4d3f26";

        // Vanilla selections the pools are derived from.
        private const string ClericPowerUseCounterGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b73";
        private const string ClericPowerUseProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b74";
        private const string ClericPowerUseRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b75";
        private const string DruidPowerUseCounterGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b76";
        private const string DruidPowerUseProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b77";
        private const string DruidPowerUseRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b78";
        private const string WizardPowerUseCounterGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b79";
        private const string WizardPowerUseProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b7a";
        private const string WizardPowerUseRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b7b";

        private const string ClericDomainsSelectionGuid = "48525e5da45c9c243a343fc6545dbdb9";
        private const string ClericSecondDomainsSelectionGuid = "43281c3d7fe18cc4d91928395837cd1e";
        // The separatist replaces the SECOND domain with one taken from a deity other than his
        // own, through a selection of its own — and every domain it offers is a separate
        // blueprint with its own resource (AirDomainBaseResourceSeparatist and so on), because
        // the archetype runs those powers at one level lower. Reading only the two standard
        // selections therefore left a separatist's second domain out of the pool entirely.
        private const string ClericSeparatistSecondDomainsSelectionGuid = "42b781e4375d499383b2602d90661283";
        // The Extra Domain feat. Its options are the ordinary domain features, so this usually
        // adds nothing (the pool dedupes by feature); it is listed so that a domain reachable
        // ONLY through the feat is not missed.
        private const string ExtraDomainSelectionGuid = "213a8480d22206b45acbfa0619ca5aaf";
        private const string DruidDomainSelectionGuid = "5edfe84c93823d04f8c40ca2b4e0f039";
        private const string WizardSchoolSelectionGuid = "8d4637639441f1041bee496f20af7fa3";
        private const string WizardSpecialistSchoolSelectionGuid = "5f838049069f1ac4d804ce0862ab5110";
        // Same shape as the separatist, on the wizard side: the Thassilonian specialist picks his
        // school from a selection of his own rather than from the standard specialist card.
        private const string WizardThassilonianSchoolSelectionGuid = "f431178ec0e2b4946a34ab504bb46285";
        private const string VanillaFavoriteEnemySel = "16cc2c937ea8d714193017780e7d4fc6";
        private const string InstantEnemyBuffGuid = "82574f7d14a28e64fab8867fbaa17715";

        // Channel energy / fervor favored class bonuses scale the MAGNITUDE of the
        // healing or damage, not the number of uses per day (original README:
        // "+1 bonus to channel energy healing or damage/3 levels"). Channel energy only
        // ever deals damage in its harm-undead mode, so a damage bonus on the same
        // ability list is exactly the aasimar cleric's "harm undead" bonus.
        private static readonly string[] ChannelEnergyAbilityGuids =
        {
            "f5fc9a1a2a3c1a946a31b320d1dd31b2", // ChannelEnergy (cleric base; variants match via Parent)
            "b5cf6b80e65ea724d99dc9f4f8874fc3", // WarpriestChannelEnergy
            "6bcaf7636388f2a40bce263372735eef", // WarpriestShieldbearerChannelEnergy
        };
        private static readonly string[] FervorAbilityGuids =
        {
            "051eaf10f7fe97f49aaf87bdc68580bd", // WarpriestFervorPositiveAbility
            "608a63ea6eec40bd8598c76965bf439c", // WarpriestFervorPositiveAbilityCast
            "5542b984ed4e7a74eac305d3c2413e1d", // WarpriestFervorPositiveAbilitySelf
            "44972136cbe45e441bd4a65dde725a3f", // WarpriestFervorNegativeAbility
            "df91f64952884c56a163b4c511462e86", // WarpriestFervorNegativeAbilityCast
            "b1f39dcf9fbcc8f49a7b5761bbfc27f6", // WarpriestFervorNegativeAbilitySelf
        };

        // Paladin lay on hands — same three ability blueprints as Kingmaker (Owlcat
        // carried them over unchanged), covering the touch-other, self, and
        // self-or-troth casts. The harm-undead use is the same ability, so the damage
        // half of "whether using it to heal or harm" needs no separate list.
        private static readonly string[] LayOnHandsAbilityGuids =
        {
            "caae1dc6fcf7b37408686971ee27db13", // LayOnHandsOthers
            "8d6073201e5395d458b8251386d72df1", // LayOnHandsSelf
            "8337cea04c8afd1428aad69defbfc365", // LayOnHandsSelfOrTroth
        };

        // Alchemist mutagen/cognatogen buffs — the natural armor favored class bonus
        // applies only while one of these is active (RAW: "when using the character's
        // mutagen"). Vanilla set only; a mutagen added by another mod would not be
        // recognised, matching how the blast lists above are scoped.
        private static readonly string[] MutagenBuffGuids =
        {
            "b84abc3531ed5674284ef0ba4aafcd3b", "f2be3d538b5d75c409289d35399723c4", "bd48322a4e258b8418106dcc6459e024",
            "83ed8d5c1e4ed9045874494c0fe2b682", "a42c49fcb081bd1469679e4f515732c8", "84ae955af09809b4ea31a2c719c68377",
            "d0a5cedfd497f3b4f9581b6066d9043b", "84c42fea967a2a8499ceeaef3a6416b8", "a8e7ca242395c3b49af5a3dbc9dee683",
            "204a74affae72d54984fb533704caf72", "3b7cf6307d3e61545a977c9f4156e12e", "8d4357118c75a5746802a3582a937376",
            "bf73a2b70b6fac54e891431cf6c7d8eb", "9c3761b9f48f69849ad78873c5a12147", "0d51a2ff0a6ce85458309affbc00b933",
            "20e740104092b5e49bfb167f1670a9de", "6871149a90e278f479aa171ee8bb563e", "32f2bc843effd9b45a0952a3cffbbe9f",
            "1c2fdba3b33dacd41afd5b74d84c7332", "34fde71198d30094aa133546e8cf8733", "b60f8b93d3d1d26439c1bb48fd461a3a",
            "61271a59038390c488c313f7a0aee6ea", "bc0890817bb28fe4a86094fe57cd40fb", "60eb20b9d1077ed4f8f8a9df5490a208",
            "8de52f7aa6052a0498875e0d834330af", "ac7753d72b0b7264982c2b6670fa2a2e", "a5a6f915d13fd994fb109473032d7440",
            "608dd115b3b0fba4ab511f448bc798f8", "98a46e8da1dca9f47b41b9d71d579628", "232fe914c22744c4ea3e050901bda424",
            "3fb9e9a6408589343bc8bfc3fd1610e5", // TrueMutagenBuff
        };

        // The buff that carries the paladin's divine bond with her weapon, and therefore its
        // duration. DivineHunter is the archetype variant of the same bond.
        private static readonly string[] WeaponBondDurationBuffGuids =
        {
            "bf570774501886f47b395a4bfe75eeb2", // WeaponBondDurationBuff
            "30b2f6ad2bcfa2045948fc9ec7f572b5", // DivineHunterBondDurationBuff
        };

        // The bloodrager's rage. The bloodline buffs are separate and ride on top of this one,
        // so the base buff alone answers "is this character bloodraging right now".
        private static readonly string[] BloodrageBuffGuids =
        {
            "5eac31e457999334b98f98b60fc73b2f", // BloodragerStandartRageBuff
        };

        // Creature types in WOTR are features on the unit rather than an enum on the blueprint.
        private const string OutsiderTypeFeatureGuid = "9054d3988d491d944ac144e27b6bc318";

        // Eldritch Scion is an ARCHETYPE of Magus in WOTR (the same-named character
        // class blueprint is the hidden spellbook helper, excluded from favored
        // classes). Its arcana pool is a separate, Charisma-based selection, so the
        // scion gets its own bonus-arcana entry and is excluded from both the regular
        // magus arcana entry and the arcane pool entry (a scion has an eldritch pool
        // instead, resource 17b6158d363e4844fa073483eb2655f8).
        private const string EldritchScionArchetypeGuid = "d078b2ef073f2814c9e338a789d97b73";
        private const string VanillaEldritchMagusArcanaSel = "d4b54d9db4932454ab2899f931c2042c";
        private const string EldritchArcanaRewardGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1baf";

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
        private const string BlessingResourceGuid = "d128a6332e4ea7c4a9862b9fdb358cca";

        // Alchemist bombs. Discovery bombs are separate root abilities rather than children of
        // the standard bomb, so each one has to be listed for the damage bonus to reach it.
        private static readonly string[] BombAbilityGuids =
        {
            "5fa0111ac60ed194db82d3110a9d0352", // BombStandart
            "fd101fbc4aacf5d48b76a65e3aa5db6d", // AcidBomb
            "bd05918a568c41e49aed7b9526ba596b", // BlindingBomb
            "f80896af0e10d7c4f9454cf1ce50ada4", // DispellingBomb
            "2b76e3bd89b4fa0419853a69fec0785f", // ExplosiveBomb
            "557898e059f5ff644848b0a4df087391", // ForceBomb
            "addf00b42747e1b47917b852073ddcd9", // FrostBomb
            "b94ee802dc1574b4fb71215a4a6f11dc", // HolyBomb
            "9aef2eb14fba66d47bef9442311e346e", // ShockBomb
            "526aa6319e9174e4ab2026e0f299b011", // TanglefootBomb
        };

        // Wave 10 additions.
        private const string BombDmgEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bbf";
        private const string CompanionHPPetGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc1";
        private const string CompanionNaturalACPetGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc3";
        private const string BlessingEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bce";
        private const string GoodCLEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bd0";
        private const string DrowSorcererSpellListGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bd4";
        private const string KitsuneShamanSpellListGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bd8";

        private static readonly string[] SorcererDrowKnownSpellLevelGuids = { "a20ec3e437f14938ab28b24f8c6ce12b", "1718f32392914ecca7a43696191909cf", "4558051ef64d4379bfbba5428f686caf", "3f3eea73bf0b4d3fa95a5b42196d7e94", "87390c49a59040fb9f8d8d859bb81028", "e3b7b0bb93bf428493cdee76a76ff556", "07f4154139694c9783566f4e428d88e0", "18ffd105db444471b914e490cbe4ee35" };
        private static readonly string[] ShamanKitsuneKnownSpellLevelGuids = { "8c572b34dfe846e1ad5617666a492c9f", "5207b87415b340508fa28cc040cb86af", "5a257a479b4e44a3b2bf69d2c935a863", "f5accb652916420eb607d8aba453bbb3", "38fada954d0141b8b4a15bb6db5d2eb2", "d78c7777c8024922a5c238529a73e186", "7148be42a7dd458d8106c279a4515bfa", "83259926f18545f581e53cd6ccacd682" };
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

        // Vanilla progression every character gets, with no class restriction — its level 1
        // fires exactly once, during chargen. Holds the favored class selection.
        private const string BasicFeatsProgressionGuid = "5b72dd2ca2cb73b49903806ee8986325";

        // Guids of all per-class bonus selections — used by the level-up queue patch
        // to glue each bonus card right behind its favored-class pick card.
        // BlueprintGuid twins of the identifiers the runtime patches compare against. Comparing
        // BlueprintGuid directly avoids the 32-character string that AssetGuid.ToString()
        // allocates on every check, which matters because the progression gate runs over every
        // feature of every level entry for every unit that levels — including whole areas of
        // NPCs auto-levelling on load. Install fills the set; the two singles are parsed once.
        internal static readonly BlueprintGuid SelectionAssetGuid = BlueprintGuid.Parse(SelectionGuid);
        internal static readonly BlueprintGuid MultitalentedAssetGuid = BlueprintGuid.Parse(MultitalentedGuid);

        // Every blueprint guid this mod creates — used by the clean-uninstall strip.
        internal static readonly HashSet<string> AllModGuids = new();
        // Same set as BlueprintGuid structs for allocation-free checks on hot paths
        // (the player-faction gate runs on every fact grant in the game).
        internal static readonly HashSet<BlueprintGuid> AllModBlueprintGuids = new();

        // Vanilla scaling knobs the mod raises rather than duplicates. Each entry pairs a
        // ContextRankConfig instance from a vanilla blueprint with the effect feature whose rank
        // gets added to it, so ContextRankConfig_GetValue_RaisePatch identifies its targets with
        // a reference comparison and nothing more. Nothing is written to those blueprints.
        internal sealed class RaisedRankConfig
        {
            public Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig Config;
            public string EffectGuid;
            public string Label;   // diagnostics only
        }

        internal static readonly List<RaisedRankConfig> RaisedRankConfigs = new();

        // Both of the banner buff's effects (SavingThrowBonusAgainstDescriptor and
        // ChargeAttackBonus) take their magnitude from one ContextRankConfig, so raising what it
        // returns raises the banner bonus itself — literally what "+1/4 to the cavalier's banner
        // bonus" asks for.
        internal const string BannerBonusCounterGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf3";

        // The banner bonus follows the same shape as every other divisor entry: the counter
        // accumulates picks, and one rank of this effect feature is granted at each threshold, so
        // its rank IS the earned bonus. The patch reads this rather than dividing the counter
        // itself — which keeps the arithmetic in one place and, more importantly, gives the
        // player a feature on the character sheet showing what the banner actually gained.
        internal const string BannerBonusEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf4";

        // Warpriest sacred weapon: same arrangement, a different vanilla knob. The damage tier is
        // chosen by a Conditional on WarpriestSacredWeaponBuffBase reading a shared value that
        // comes from that buff's ClassLevel ContextRankConfig, so raising the rank makes the
        // game's own conditional apply the higher tier buff.
        internal const string SacredWeaponLevelCounterGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1c01";
        internal const string SacredWeaponLevelEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1c02";
        private const string WarpriestSacredWeaponBuffBaseGuid = "2d133dca34668ef4e98805cb428edebe";

        // Paladin auras. The bonus lives on the effect buff the aura puts on allies, as
        // SavingThrowBonusAgainstDescriptor with ModifierDescriptor.Morale and a flat Value of 4.
        // There is no ContextRankConfig to raise, but the component's second slot — the Bonus
        // ContextValue — is unused, so the mod supplies one. See PatchPaladinAuras.
        internal const string AuraBonusCounterGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1c03";
        internal const string AuraBonusEffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1c04";
        private const string AuraOfCourageEffectBuffGuid = "1044ac71f6200f84bbfbcfa2bcb3bd66";
        private const string AuraOfResolveEffectBuffGuid = "d8f2f84899d6e1e4d83859e18f697ae3";

        internal enum BonusDisplayKind { Flat, Feet, SkillRanks, Feats, HitPoints }

        // Accumulative bonus features -> how to render the earned whole bonus in
        // character sheet tooltips.
        internal static readonly Dictionary<BlueprintGuid, (int Divisor, BonusDisplayKind Kind)> BonusDisplays = new();

        // Counter feature -> (divisor, effect feature granted one rank per threshold).
        internal static readonly Dictionary<BlueprintGuid, (int Divisor, string EffectGuid)> EffectGrants = new();

        // Wrapper reward selection -> its pick-counter feature: a reward pick also
        // counts as a pick, so the counter always shows the true number of picks.
        internal static readonly Dictionary<BlueprintGuid, string> RewardPickCounters = new();

        // Per-class favored class progression -> the class it belongs to. The character
        // sheet groups progressions with GetClassProgressions(cls), which matches on
        // Feature.GetSourceClass(); that reads the fact's source and casts it to a
        // character class. A pick made from our selection would otherwise be sourced to
        // whatever granted the selection, so the entry has to be re-sourced to its class
        // for the sheet to file it under that class (see the SelectFeature patch).
        internal static readonly Dictionary<BlueprintGuid, string> FavoredClassProgressionClass = new();

        // Witch patron progression -> the spells that patron grants, read out of the
        // patron progressions' own AddKnownSpell components at install time.
        // AnyPatronSpell is the union, used as a cheap first-pass reject on the cast
        // path. Static lookup tables built from immutable blueprint data — never
        // serialized, same as the dictionaries above.
        internal static readonly Dictionary<BlueprintProgressionReference, HashSet<BlueprintGuid>> PatronSpells = new();
        internal static readonly HashSet<BlueprintGuid> AnyPatronSpell = new();

        // Witch patron progressions (WOTR ships 15).
        private static readonly string[] WitchPatronProgressionGuids =
        {
            "08518b2a62446c74b9ae08ee73664047", // Agility
            "b9c4e782706099f42a2ebc901acf492d", // Ancestors
            "f4f3d8395db347938237c1bc77820781", // Dark Pact
            "3a4214e3c2eab3c40bc491d7abea7045", // Deception
            "facb0ed7d8e52b04cacf351bea430ce9", // Devotion
            "67e85f52b1f020847aaa738d8999d4cd", // Elements
            "8fe0a14c90d3ea94a833d087b8a09bb9", // Endurance
            "a3e4ef40ad99f4d47af15cc5f16afc97", // Healing
            "eafc47304da734a4d922ae663d82f1e5", // Insanity
            "cad7c2fdabeb9574f95f4b9ffee20afe", // Mercy
            "f48bfffe3618c274dbd42dfff8d0df56", // Shadow
            "850ac8a2bc65d814db9f3fea871c18bb", // Strength
            "23ea5ade326f80b488164f75580c03af", // Transformation
            "e98d8d9f907c1814aa7376d6cdaac012", // Winter
        };

        // Walks each patron progression's level entries, collecting every spell it
        // grants via AddKnownSpell. This is what makes the halfling witch's "+1/4
        // caster level for patron spells" bonus possible without hard-coding spell
        // lists: the patron itself is the source of truth, so patrons added or
        // changed by other mods are picked up too.
        private static void BuildPatronSpellMap()
        {
            PatronSpells.Clear();
            AnyPatronSpell.Clear();
            foreach (var progGuid in WitchPatronProgressionGuids)
            {
                var progression = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>(BlueprintGuid.Parse(progGuid));
                if (progression?.LevelEntries == null) continue;
                var spells = new HashSet<BlueprintGuid>();
                foreach (var entry in progression.LevelEntries)
                {
                    foreach (var featureRef in entry.m_Features)
                    {
                        var feature = featureRef?.Get();
                        if (feature?.ComponentsArray == null) continue;
                        foreach (var component in feature.ComponentsArray)
                        {
                            if (component is Kingmaker.UnitLogic.FactLogic.AddKnownSpell known
                                && known.m_Spell != null && !known.m_Spell.deserializedGuid.Equals(BlueprintGuid.Empty))
                            {
                                spells.Add(known.m_Spell.deserializedGuid);
                            }
                        }
                    }
                }
                if (spells.Count == 0) continue;
                PatronSpells[BlueprintTool.GetRef<BlueprintProgressionReference>(progGuid)] = spells;
                foreach (var s in spells) AnyPatronSpell.Add(s);
            }
            Main.Log($"Patron spell map: {PatronSpells.Count} patrons, {AnyPatronSpell.Count} distinct patron spells.");
        }
        private static readonly HashSet<string> ExcludedClasses = new()
        {
            "f5b8c63b141b2f44cbb8c2d7579c34f5", // EldritchScionClass — magus subclass, excluded in the original too
            // Pet / summon / monster / technical classes. The flag test below already
            // catches these, but they are listed explicitly because they are the ones that
            // actually turn up on a player-controlled unit (an animal companion levels a
            // real class), and a wrong flag on any of them would be silent.
            "4cd1757a0eea7694ba5c933729a53920", // AnimalClass
            "26b10d4340839004f960f9816f6109fe", // AnimalCompanionClass
            "530b6a79cb691c24ba99e1577b4beb6d", // MythicCompanionClass
            "e40e01860956b8b4d80059d4437996f5", // AberrationClass
            "fd66bdea5c33e5f458e929022322e6bf", // ConstructClass
            "c91a49b104e94b7ab806bf6120f98f05", // DLC3_UniqueConstructClass
            "01a754e7c1b7c5946ba895a5ff0faffc", // DragonClass
            "f2e6e760ead99fb48ade27c7e9d4ac94", // FeyClass
            "6ab4526f94d2e3e439af0599a29b6675", // HumanoidClass
            "b9e97f47cb86f2d45a0784a096ff8037", // MagicalBeastClass
            "8a3c86893f383214da070e9c84c1e95b", // MonstrousHumanoidClass
            "9a20b40b57f4e684fa20d17c0edfd5ba", // NymphClass
            "92ab5f2fe00631b44810deffcc1a97fd", // OutsiderClass
            "9393cc36ea29d084bab7433e3a28d40b", // PlantClass
            "fb7a0be8af1d405e8387648ad8513c9c", // PrototypeClass
            "19a2d9e58d916d04db4cd7ad2c7a3ee2", // UndeadClass
            "d1a15612d1a96334d94edf5f1d3b8d29", // VerminClass
            "b2d9af52cf680744eb0cdc3f3034395f", // WarriorClass
            "96a850e939904ca3ac8431d55318e7c6", // BardClass_Penta (technical duplicate)
            "b82f1fbd191e1f2498266ca41f05027f", // FakeLegendClass
        };

        // A class may be a favored class only if a player could pick it at chargen. The
        // flag test mirrors what the game's own class-selection screen uses
        // (CharGenClassPhaseVM filters on HideInUI), so classes added by other mods are
        // judged by the same rule instead of needing to be listed here.
        private static bool IsFavoredClassCandidate(BlueprintCharacterClass cls)
        {
            if (cls == null) return false;
            if (cls.PrestigeClass) return false;   // these get the Favored Prestige Class feat instead
            if (cls.IsMythic) return false;
            if (cls.HideInUI) return false;
            return !ExcludedClasses.Contains(cls.AssetGuid.ToString());
        }

        public static void Install()
        {
            // Every registry is reset together, not just the guid list. RaisedRankConfigs is the
            // one that actually matters: it holds references to component instances belonging to
            // vanilla blueprints, so a second Install after the blueprints were reloaded would
            // otherwise leave stale objects in the list alongside the live ones.
            AllModGuids.Clear();
            AllModBlueprintGuids.Clear();
            RaisedRankConfigs.Clear();
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

            // The reward taken on each level of the favored class, as opposed to the choice
            // of the class itself.
            var bonusSelectionName = LocalizationTool.CreateString("ZFCW.BonusSelection.Name",
                "Favored Class Bonus", tagEncyclopediaEntries: false);
            var bonusSelectionDesc = LocalizationTool.CreateString("ZFCW.BonusSelection.Desc",
                "Whenever a character gains a level in his favored class, he receives either +1 hit point or +1 skill rank. " +
                "Members of some races may instead select an alternate racial bonus.",
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
                if (!IsFavoredClassCandidate(cls)) continue;
                var clsGuid = cls.AssetGuid.ToString();

                var bonusSelGuid = MergeIds(clsGuid, BonusSelectionSeed);
                var progGuid = MergeIds(clsGuid, ProgressionSeed);

                var bonusItems = new List<string> { HpFeatureGuid, SkillFeatureGuid };
                if (classExtras.TryGetValue(clsGuid, out var extras)) bonusItems.AddRange(extras);
                bonusItems.AddRange(GlobalBonusExtras);
                // Named apart from the favored class pick itself: this card is the per-level
                // reward, and two steps both called "Favored Class" read as the same one
                // asked twice.
                FeatureSelectionConfigurator.New($"ZFCWFavoredClass{cls.name}BonusSelection", bonusSelGuid)
                    .SetDisplayName(bonusSelectionName)
                    .SetDescription(bonusSelectionDesc)
                    .SetIsClassFeature(true)
                    // The native "you must answer this" flag. FeatureSelectionState
                    // .CanSelectAnything tests IsObligatory() first, and the level-up phase
                    // refuses to report itself complete while an obligatory card is unanswered.
                    // Without it the card could be left empty and the increment lost for good.
                    // Safe to force: this card always offers the universal +1 hit point and
                    // +1/2 skill rank, neither of which has a prerequisite.
                    .SetObligatory()
                    .AddToAllFeatures(bonusItems.Select(g => (Blueprint<BlueprintFeatureReference>)g).ToArray())
                    .Configure();
                AllModGuids.Add(bonusSelGuid);
                AllModGuids.Add(progGuid);

                // "Inquisitor" alone reads as a duplicate of the class's own progression
                // entry — append " Favored Class" so the two are visually distinct. Read
                // the class name through the SAME resolution path the game itself uses
                // for cls.LocalizedName (LocalizationManager.CurrentPack.GetText on its
                // key), not the raw cls.Name field: mod classes (e.g. Swashbuckler) may
                // not have cls.Name populated at install time, which is what previously
                // produced "null" labels. Fall back to the bare class name if this
                // specific class's string genuinely isn't registered yet, rather than
                // ever baking in an empty/null result.
                // CurrentPack can still be null this early — the startup coroutine only waits
                // for the localization pack after the blueprint load finishes — so resolve
                // defensively and fall back to the class's own name.
                var resolvedClassName = Kingmaker.Localization.LocalizationManager.CurrentPack
                    ?.GetText(cls.LocalizedName.m_Key, reportUnknown: false);
                if (string.IsNullOrEmpty(resolvedClassName))
                {
                    // Falling back to cls.LocalizedName rendered correctly but WITHOUT the
                    // suffix, so the Swashbuckler row read plain "Swashbuckler" among a column
                    // of "X Favored Class" — a mod class whose string is not in the pack this
                    // early. The blueprint name is always present, so derive from it instead and
                    // keep the suffix; trailing "Class" is dropped because the internal names
                    // read "ClericClass" while the mod ones read "Swashbuckler".
                    resolvedClassName = cls.name ?? "";
                    if (resolvedClassName.EndsWith("Class") && resolvedClassName.Length > 5)
                    {
                        resolvedClassName = resolvedClassName.Substring(0, resolvedClassName.Length - 5);
                    }
                }
                var progressionName = !string.IsNullOrEmpty(resolvedClassName)
                    ? LocalizationTool.CreateString($"ZFCW.FC.{clsGuid}", $"{resolvedClassName} Favored Class", tagEncyclopediaEntries: false)
                    : cls.LocalizedName;

                ProgressionConfigurator.New($"ZFCWFavoredClass{cls.name}Progression", progGuid)
                    .SetDisplayName(progressionName)
                    .SetDescription(selectionDesc)
                    .SetIsClassFeature(true)
                    .SetClasses(clsGuid)
                    // No marker prerequisite: the selection is offered once, at character
                    // level 1, so nothing can re-offer it — and gating on the marker would
                    // block the half-elf's SECOND pick (Multitalented), which draws from
                    // this same item list. Taking one class twice is already impossible: a
                    // progression is not an IFeatureSelection, so MeetsPrerequisites rejects
                    // it once its rank reaches Ranks (1).
                    .AddToLevelEntry(1, bonusSelGuid, MarkerGuid)
                    .AddToLevelEntries(bonusSelGuid, from: 2)
                    .Configure();

                progressionGuids.Add(progGuid);
                FavoredClassProgressionClass[BlueprintGuid.Parse(progGuid)] = clsGuid;
            }

            // Explicit opt-out. Two ranks so a half-elf, who picks twice, can decline both
            // slots; the marker is still granted as the "has chosen" flag.
            FeatureConfigurator.New("ZFCWNoFavoredClass", NoneFeatureGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.None.Name", "No Favored Class", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.None.Desc",
                    "You have no favored class and receive no favored class bonuses.", tagEncyclopediaEntries: false))
                .SetIsClassFeature(true)
                .SetRanks(2)
                .AddFacts(new() { MarkerGuid })
                .Configure();

            FeatureSelectionConfigurator.New("ZFCWFavoredClassSelection", SelectionGuid)
                .SetDisplayName(selectionName)
                .SetDescription(selectionDesc)
                .SetIsClassFeature(true)
                // Same reasoning; "No Favored Class" is always among the options, so declining
                // stays possible — it just has to be said out loud rather than skipped.
                .SetObligatory()
                // Chargen sorts a selection into a phase by its feature group:
                // CharGenFeatureSelectorPhaseVM.GetFeaturePriority maps Racial and the
                // various heritage groups to the RaceFeatures phase and everything else to
                // Features, which sits after ability scores and skills. Without a group the
                // pick was stranded at the far end of chargen, while Multitalented — which
                // carries Racial — sat right after the racial heritage step. Same group here
                // puts them together, in m_Features order: heritage, favored class, then
                // Multitalented.
                .SetGroup(Kingmaker.Blueprints.Classes.FeatureGroup.Racial)
                .AddToAllFeatures(progressionGuids.Select(g => (Blueprint<BlueprintFeatureReference>)g)
                    .Append(NoneFeatureGuid).ToArray())
                .Configure();

            // Attach the selection to the BASIC FEAT progression's level-1 entry — not to a
            // class, and not to the races.
            //
            // Per class was wrong outright: a class's level 1 is reached again whenever the
            // character multiclasses, so the pick card came back at, say, Fighter 1 of a
            // level-8 wizard, with every option already spent and nothing selectable on it.
            // This progression carries no class restriction, so its level tracks character
            // level and the entry fires exactly once.
            //
            // Where the card APPEARS in chargen is decided by the feature group, not by what
            // holds the selection: CharGenFeatureSelectorPhaseVM.GetFeaturePriority maps
            // FeatureGroup.Racial and the heritage groups to the RaceFeatures phase. The
            // background selection is the proof — it sits in that same phase with its own
            // group and is not a race feature either. So the group above is what puts this
            // right after the racial heritage step; hanging it off the races was never needed
            // for that, and cost two things:
            //   - the race screen listed "Favored Class" as though it were a racial trait of
            //     every race, before anything had been chosen;
            //   - SelectRace.Apply handed the selection to every NPC of a playable race.
            // A favored class is not a racial trait, so it should not read as one — it shows
            // up on the character once it has actually been chosen.
            var selection = BlueprintTool.Get<BlueprintFeatureSelection>(SelectionGuid);
            var selectionRef = selection.ToReference<BlueprintFeatureBaseReference>();
            int attached = 0;
            var basicFeats = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>(
                BlueprintGuid.Parse(BasicFeatsProgressionGuid));
            var basicFeatsL1 = basicFeats?.LevelEntries?.FirstOrDefault(e => e.Level == 1);
            if (basicFeatsL1 == null)
            {
                Main.Log("ERROR: BasicFeatsProgression level 1 entry not found — favored class selection NOT attached.");
            }
            else if (!basicFeatsL1.m_Features.Any(r => r.Guid.ToString() == SelectionGuid))
            {
                basicFeatsL1.m_Features.Add(selectionRef);
                attached++;
            }

            // Half-elf "Multitalented": a second, independent favored class pick offering
            // the same options as the first. It hangs off the RACE rather than a
            // progression — races grant their features during chargen, which is exactly
            // when the second choice is made, and it keeps the option away from every
            // other race without needing a race prerequisite. Same approach as the
            // original mod, which copies its favored class selection onto the half-elf.
            //
            // Picking the same class twice is impossible on its own: a progression stops
            // qualifying once its rank reaches Ranks (1), so the class taken in the first
            // slot is simply not offered in the second.
            FeatureSelectionConfigurator.New("ZFCWMultitalented", MultitalentedGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.Multitalented.Name", "Multitalented", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.Multitalented.Desc",
                    "Half-elves choose two favored classes at 1st level and gain 1 additional hit point or skill point " +
                    "whenever they take a level in either one of those classes.",
                    tagEncyclopediaEntries: false))
                .SetIsClassFeature(true)
                .SetGroup(Kingmaker.Blueprints.Classes.FeatureGroup.Racial)
                .AddToAllFeatures(progressionGuids.Select(g => (Blueprint<BlueprintFeatureReference>)g)
                    .Append(NoneFeatureGuid).ToArray())
                .Configure();
            AllModGuids.Add(MultitalentedGuid);
            AllModBlueprintGuids.Add(BlueprintGuid.Parse(MultitalentedGuid));

            var halfElf = ResourcesLibrary.TryGetBlueprint<BlueprintRace>(BlueprintGuid.Parse(HalfElfRaceGuid));
            if (halfElf == null)
            {
                Main.Log("Half-elf race blueprint not found — Multitalented NOT attached.");
            }
            else if (!halfElf.m_Features.Any(r => r.Guid.ToString() == MultitalentedGuid))
            {
                halfElf.m_Features = halfElf.m_Features
                    .Append(BlueprintTool.Get<BlueprintFeatureSelection>(MultitalentedGuid)
                        .ToReference<BlueprintFeatureBaseReference>())
                    .ToArray();
                Main.Log("Multitalented attached to the half-elf race.");
            }

            foreach (var g in AllModGuids)
            {
                AllModBlueprintGuids.Add(BlueprintGuid.Parse(g));
            }

            BonusDisplays[BlueprintGuid.Parse(HpFeatureGuid)] = (1, BonusDisplayKind.HitPoints);
            BonusDisplays[BlueprintGuid.Parse(SkillFeatureGuid)] = (2, BonusDisplayKind.SkillRanks);
            // Every other entry is populated automatically from its RacialBonusDef inside the
            // defs-processing loop below (see the "Tooltip display" comment there) — this
            // retired counter is the one exception, since it no longer has a def at all (its
            // RacialBonusDef was replaced by FavoredEnemyPick; only the hidden stub survives,
            // for old-save compatibility).
            BonusDisplays[BlueprintGuid.Parse("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b55")] = (4, BonusDisplayKind.Flat);

            CacheRaisedRankConfigs();
            PatchPaladinAuras();

            Main.Log($"Favored class system installed: {progressionGuids.Count} class progressions, selection attached to {attached} basic-feat L1 entry (expected 1).");
        }

        // Finds the rank configs the mod raises, so the patch can recognise them by reference.
        // Nothing is written to these blueprints — the components stay exactly as the game
        // shipped them, and a character without the bonus is unaffected because the patch exits
        // on a zero effect rank.
        private static void CacheRaisedRankConfigs()
        {
            void Collect(string buffGuid, string effectGuid, string label)
            {
                var buff = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>(
                    BlueprintGuid.Parse(buffGuid));
                if (buff == null) return;
                foreach (var component in buff.ComponentsArray)
                {
                    if (component is Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig config
                        && config.Type == Kingmaker.Enums.AbilityRankType.Default)
                    {
                        RaisedRankConfigs.Add(new RaisedRankConfig
                        {
                            Config = config,
                            EffectGuid = effectGuid,
                            Label = label,
                        });
                    }
                }
            }

            Collect(CavalierBannerBuffGuid, BannerBonusEffectGuid, "banner");
            Collect(CavalierBannerGreaterBuffGuid, BannerBonusEffectGuid, "banner");
            Collect(WarpriestSacredWeaponBuffBaseGuid, SacredWeaponLevelEffectGuid, "sacred weapon");

            Main.Log($"Vanilla rank configs raised by this mod: {RaisedRankConfigs.Count} (expected 2 — banner, sacred weapon).");
        }

        // The one place the mod writes to a vanilla blueprint.
        //
        // "Add +1/4 to the bonus the paladin grants her allies with her aura of courage and aura
        // of resolve." The bonus sits on the effect buff the aura applies to allies, as
        // SavingThrowBonusAgainstDescriptor with ModifierDescriptor.Morale and Value = 4. Adding a
        // second component beside it would not do: morale bonuses of the same descriptor do not
        // stack, and an untyped one alongside would be a different bonus rather than a bigger
        // aura. There is also no ContextRankConfig here to raise the way the banner's is.
        //
        // What the component does have is a second, unused slot: it computes
        // Bonus.Calculate(Fact.MaybeContext) + Value * Fact.GetRank(), and Bonus is an empty
        // ContextValue. So the mod points Bonus at a rank and supplies the ContextRankConfig that
        // defines it. The vanilla component then emits 4 + earned, with its own Morale descriptor
        // — one bonus, simply larger.
        //
        // Safe for characters without the bonus: the rank config counts ranks of a feature they
        // do not have, so it yields 0 and the aura stays at 4. Safe for saves: both components
        // added here are vanilla types, so no mod $type is written. Safe on uninstall: blueprints
        // are rebuilt from the game's pack every launch, so the edit simply stops happening.
        private static void PatchPaladinAuras()
        {
            int patched = 0;
            foreach (var buffGuid in new[] { AuraOfCourageEffectBuffGuid, AuraOfResolveEffectBuffGuid })
            {
                var buff = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>(
                    BlueprintGuid.Parse(buffGuid));
                if (buff == null)
                {
                    Main.Log($"WARNING: paladin aura buff {buffGuid} not found; the aura bonus will not apply.");
                    continue;
                }

                var save = buff.ComponentsArray
                    .OfType<Kingmaker.Designers.Mechanics.Facts.SavingThrowBonusAgainstDescriptor>()
                    .FirstOrDefault();
                if (save == null)
                {
                    Main.Log($"WARNING: {buff.name} has no SavingThrowBonusAgainstDescriptor; the aura bonus will not apply.");
                    continue;
                }

                // Guard against a second install pass re-adding the rank config.
                if (save.Bonus != null
                    && save.Bonus.ValueType == Kingmaker.UnitLogic.Mechanics.ContextValueType.Rank)
                {
                    patched++;
                    continue;
                }

                var rank = new Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig();
                rank.m_Type = Kingmaker.Enums.AbilityRankType.Default;
                rank.m_BaseValueType = Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.FeatureRank;
                rank.m_Feature = BlueprintTool.GetRef<BlueprintFeatureReference>(AuraBonusEffectGuid);
                rank.m_Progression = Kingmaker.UnitLogic.Mechanics.Components.ContextRankProgression.AsIs;
                rank.m_UseMax = true;
                rank.m_Max = 5;

                buff.ComponentsArray = buff.ComponentsArray.Append(rank).ToArray();
                save.Bonus = ContextValues.Rank();
                patched++;
            }
            Main.Log($"Paladin aura buffs wired for the aura bonus: {patched} (expected 2).");
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
            public string[] ExcludeArchetypes; // hide the whole entry for characters with these archetypes
            public string RequireArchetype;    // show the entry ONLY for this archetype
            public BonusDisplayKind DisplayKind = BonusDisplayKind.Flat; // tooltip wording for the earned bonus
            public bool SkipBonusDisplay;      // opt out when the real progression isn't floor(rank/Divisor)
            public string Folder;              // Key of the BonusFolderDef this nests inside (null = listed directly)
        }

        // A container card: instead of N sibling entries in the bonus list, one entry that
        // unfolds into them, the same native nesting wrapper mode already relies on.
        //
        // Worth doing ONLY where a single character actually faces a choice among the
        // children — otherwise the folder is one extra click to reach the sole available
        // option. Energy resistance is the only family that qualifies: a Suli ranger, a
        // Gnome druid and a Human paladin each choose among four energies, and a Fetchling
        // barbarian or sorcerer between two. Every other multi-entry family was checked and
        // does not qualify — bonus known spells, companion bonuses and Lay on Hands have
        // disjoint race sets per class (no character is offered two), and the magus arcana
        // pair is mutually exclusive by archetype.
        //
        // Children keep their own GUIDs, ranks and components untouched; only their place in
        // the list changes. That keeps existing saves valid — the facts a character already
        // holds are the same blueprints as before.
        private sealed class BonusFolderDef
        {
            public string Key;
            public string FolderGuid;
            public string DisplayName;
            public string Description;
            public int Ranks = 20;             // one pick per class level, so 20 is the ceiling
            public string[] Races;
            public string[] Classes;
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
                    // Same blueprint as before (rank still means "earned whole bonuses"),
                    // but the bonus is now correctly gated on an active mutagen —
                    // previously it applied unconditionally, a documented fidelity gap.
                    Key = "NaturalACEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b17", Ranks = 5,
                    DisplayName = "Mutagen Natural Armor Bonus",
                    Description = "+1 natural armor bonus to Armor Class per rank while the character's mutagen is active.",
                    Components = f => f.AddComponent<NaturalACWhileTransformedPerRank>(c =>
                        c.m_Buffs = MutagenBuffGuids.Select(g =>
                            BlueprintTool.GetRef<Kingmaker.Blueprints.BlueprintBuffReference>(g)).ToArray()),
                },
                new()
                {
                    Key = "WildShapeACEffect", Guid = WildShapeACEffectGuid, Ranks = 6,
                    DisplayName = "Wild Shape Natural Armor Bonus",
                    Description = "+1 natural armor bonus to Armor Class per rank while using wild shape.",
                    Components = f => f.AddComponent<NaturalACWhileTransformedPerRank>(c => c.AnyPolymorph = true),
                },
                new()
                {
                    Key = "LayOnHandsEffect", Guid = LayOnHandsEffectGuid, Ranks = 10,
                    DisplayName = "Lay on Hands Bonus",
                    Description = "+1 hit point per rank to the paladin's lay on hands ability, whether using it to heal or harm.",
                    Components = f => f
                        .AddComponent<HealBonusForAbilitiesPerRank>(c =>
                            c.m_Abilities = LayOnHandsAbilityGuids.Select(g =>
                                BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray())
                        .AddComponent<AbilityDamageBonusPerRank>(c =>
                            c.m_Abilities = LayOnHandsAbilityGuids.Select(g =>
                                BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray()),
                },
                // (No LayOnHandsSelfEffect here: that bonus has no divisor, so a separate
                // effect feature would just be a same-named, same-rank twin of the counter
                // in Special Abilities. Its component now sits on the counter itself, the
                // way every other divisor-1 entry does — see the LayOnHandsSelf def below;
                // the old effect blueprint is retired to a hidden stub further down.)
                new()
                {
                    Key = "ChannelEnergyEffect", Guid = ChannelEnergyEffectGuid, Ranks = 7,
                    DisplayName = "Channel Energy Bonus",
                    Description = "+1 point per rank to the healing or damage done by channel energy.",
                    Components = f => f
                        .AddComponent<HealBonusForAbilitiesPerRank>(c =>
                            c.m_Abilities = ChannelEnergyAbilityGuids.Select(g =>
                                BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray())
                        .AddComponent<AbilityDamageBonusPerRank>(c =>
                            c.m_Abilities = ChannelEnergyAbilityGuids.Select(g =>
                                BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray()),
                },
                new()
                {
                    Key = "HarmUndeadEffect", Guid = HarmUndeadEffectGuid, Ranks = 10,
                    DisplayName = "Harm Undead Bonus",
                    Description = "+1 point per rank to the damage done by channel energy for the purpose of harming undead.",
                    Components = f => f.AddComponent<AbilityDamageBonusPerRank>(c =>
                        c.m_Abilities = ChannelEnergyAbilityGuids.Select(g =>
                            BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray()),
                },
                new()
                {
                    Key = "FervorEffect", Guid = FervorEffectGuid, Ranks = 10,
                    DisplayName = "Fervor Bonus",
                    Description = "+1 point per rank to the healing or damage done by fervor.",
                    Components = f => f
                        .AddComponent<HealBonusForAbilitiesPerRank>(c =>
                            c.m_Abilities = FervorAbilityGuids.Select(g =>
                                BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray())
                        .AddComponent<AbilityDamageBonusPerRank>(c =>
                            c.m_Abilities = FervorAbilityGuids.Select(g =>
                                BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray()),
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
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(AlchemistBombsResourceGuid)),
                },
                new()
                {
                    Key = "KiPoolEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b28", Ranks = 20,
                    DisplayName = "Bonus Ki Pool",
                    Description = "+1 ki point per rank.",
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(KiPowerResourceGuid)),
                },
                new()
                {
                    Key = "ArcanePoolEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2a", Ranks = 20,
                    DisplayName = "Bonus Arcane Pool",
                    Description = "+1 arcane pool point per rank.",
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
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
                    // Retired: replaced by the per-enemy pick below, which matches the
                    // original's "a single existing favored enemy" wording. Kept
                    // registered (rule: retired blueprints become hidden stubs) so
                    // saves made before the rework still load.
                    Key = "FavoredEnemyAtkDmgEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b56", Ranks = 5,
                    DisplayName = "Favored Enemy Attack and Damage Bonus",
                    Description = "+1 to attack and damage rolls against favored enemies per rank.",
                    Components = f => f
                        .AddComponent<AttackBonusAgainstFavoredEnemyPerRank>()
                        .AddComponent<DamageBonusAgainstFavoredEnemyPerRank>(),
                },
                new()
                {
                    Key = "ReservoirRegenEffect", Guid = ReservoirRegenEffectGuid, Ranks = 4,
                    DisplayName = "Bonus Arcane Reservoir Restored",
                    Description = "+1 point per rank restored to the arcanist's arcane reservoir after resting.",
                    Components = f => f.AddComponent<RestoreResourceOnRestPerRank>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(ArcanistArcaneReservoirResourceGuid)),
                },
                new()
                {
                    Key = "PatronCLEffect", Guid = PatronCLEffectGuid, Ranks = 5,
                    DisplayName = "Patron Spells Caster Level",
                    Description = "+1 caster level per rank when casting spells from the witch's patron spell list.",
                    Components = f => f.AddComponent<PatronSpellCasterLevelPerRank>(),
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
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(PanacheResourceGuid)),
                },
                new()
                {
                    Key = "CharmedLifeEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b63", Ranks = 5,
                    DisplayName = "Bonus Charmed Life",
                    Description = "+1 use per day of charmed life per rank.",
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(CharmedLifeResourceGuid)),
                },
                new()
                {
                    Key = "BombDmgEffect", Guid = BombDmgEffectGuid, Ranks = 10,
                    DisplayName = "Bomb Damage",
                    Description = "+1 point of damage per rank to the alchemist's bombs.",
                    Components = f => f.AddComponent<AbilityDamageBonusPerRank>(c =>
                        c.m_Abilities = BombAbilityGuids.Select(g =>
                            BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray()),
                },
                new()
                {
                    Key = "BlessingEffect", Guid = BlessingEffectGuid, Ranks = 7,
                    DisplayName = "Bonus Blessings",
                    Description = "+1 use per day of the warpriest's blessings per rank.",
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(BlessingResourceGuid)),
                },
                new()
                {
                    Key = "ChaosCLEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b7f", Ranks = 10,
                    DisplayName = "Chaotic Spell Caster Level",
                    Description = "+1 caster level per rank when casting spells with the chaotic descriptor.",
                    // The enum value is spelled Chaos, not Chaotic — which is why an earlier pass
                    // searched for "Chaotic", found nothing, and wrote this bonus off as blocked.
                    Components = f => f.AddComponent<IncreaseSpellDescriptorCasterLevelPerRank>(c =>
                        c.Descriptors = Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Chaos),
                },
                new()
                {
                    Key = "GoodCLEffect", Guid = GoodCLEffectGuid, Ranks = 5,
                    DisplayName = "Good Spell Caster Level",
                    Description = "+1 caster level per rank when casting spells with the good descriptor.",
                    Components = f => f.AddComponent<IncreaseSpellDescriptorCasterLevelPerRank>(c =>
                        c.Descriptors = Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Good),
                },
                new()
                {
                    Key = "ChallengeDmgEffect", Guid = ChallengeDmgEffectGuid, Ranks = 10,
                    DisplayName = "Damage Against Challenged Targets",
                    Description = "+1 damage per rank against the target of the cavalier's challenge.",
                    Components = f => f.AddComponent<DamageBonusAgainstCasterBuffTargetPerRank>(c =>
                        c.m_Buffs = new[]
                        {
                            BlueprintTool.GetRef<Kingmaker.Blueprints.BlueprintBuffReference>(CavalierChallengeTargetBuffGuid),
                        }),
                },
                new()
                {
                    Key = "BannerBonusEffect", Guid = BannerBonusEffectGuid, Ranks = 5,
                    DisplayName = "Banner Bonus",
                    Description = "+1 to the cavalier's banner bonus per rank.",
                    // No component: the banner's own ContextRankConfig is what carries the bonus,
                    // and ContextRankConfig_GetValue_RaisePatch adds this feature's rank to it.
                    // The feature earns its place by making the bonus visible on the sheet, the
                    // same as every other divisor entry's effect.
                },
                new()
                {
                    Key = "SacredWeaponLevelEffect", Guid = SacredWeaponLevelEffectGuid, Ranks = 5,
                    DisplayName = "Sacred Weapon Damage Level",
                    Description = "+1 to the warpriest's effective level for sacred weapon damage per rank.",
                    // Same arrangement as the banner: no component here, the rank is added to the
                    // vanilla ContextRankConfig that chooses the damage tier.
                },
                new()
                {
                    Key = "AuraBonusEffect", Guid = AuraBonusEffectGuid, Ranks = 5,
                    DisplayName = "Aura of Courage and Resolve Bonus",
                    Description = "+1 to the bonus the paladin's aura of courage and aura of resolve grant her allies, per rank.",
                    // No component: PatchPaladinAuras points the vanilla component's unused Bonus
                    // slot at a rank config counting ranks of this feature.
                },
                new()
                {
                    Key = "BloodrageDodgeEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bfb", Ranks = 5,
                    DisplayName = "Dodge Bonus Against Large Creatures While Bloodraging",
                    Description = "+1 dodge bonus to Armor Class per rank against creatures of size Large or larger, while bloodraging.",
                    Components = f => f.AddComponent<ACBonusAgainstLargerCreaturesPerRank>(c =>
                    {
                        c.MinimumSize = Kingmaker.Enums.Size.Large;
                        c.m_RequiredOwnerBuffs = BloodrageBuffGuids
                            .Select(g => BlueprintTool.GetRef<Kingmaker.Blueprints.BlueprintBuffReference>(g))
                            .ToArray();
                    }),
                },
                new()
                {
                    Key = "FlankedDamageEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bfc", Ranks = 6,
                    DisplayName = "Damage Against Flanked Opponents",
                    Description = "+1 damage per rank on weapon attacks against an opponent that is flanked or denied its Dexterity bonus to Armor Class.",
                    Components = f => f.AddComponent<DamageBonusAgainstFlankedTargetPerRank>(),
                },
                new()
                {
                    Key = "OutsiderSpellPenEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bfd", Ranks = 10,
                    DisplayName = "Spell Penetration Against Outsiders",
                    Description = "+1 per rank on caster level checks made to overcome the spell resistance of outsiders.",
                    Components = f => f.AddComponent<SpellPenetrationBonusAgainstFactOwnerPerRank>(c =>
                        c.m_RequiredTargetFact = BlueprintTool.GetRef<BlueprintUnitFactReference>(OutsiderTypeFeatureGuid)),
                },
                new()
                {
                    Key = "SneakBeforeActEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bfe", Ranks = 10,
                    DisplayName = "Sneak Attack Damage Before the Target Acts",
                    Description = "+1 per rank on sneak attack damage rolls during the surprise round or before the target has acted in combat.",
                    Components = f => f.AddComponent<SneakAttackBonusBeforeTargetActsPerRank>(),
                },
                new()
                {
                    Key = "MutagenDurationEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bff", Ranks = 20,
                    DisplayName = "Mutagen Duration",
                    Description = "+10 minutes per rank to the duration of the alchemist's mutagens.",
                    Components = f => f.AddComponent<ExtendBuffDurationPerRank>(c =>
                    {
                        c.SecondsPerRank = 600;
                        c.m_Buffs = MutagenBuffGuids
                            .Select(g => BlueprintTool.GetRef<Kingmaker.Blueprints.BlueprintBuffReference>(g))
                            .ToArray();
                    }),
                },
                new()
                {
                    Key = "DivineBondDurationEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1c00", Ranks = 20,
                    DisplayName = "Divine Bond Duration",
                    Description = "+1/2 minute per rank to the duration of the paladin's divine bond with her weapon.",
                    Components = f => f.AddComponent<ExtendBuffDurationPerRank>(c =>
                    {
                        c.SecondsPerRank = 30;
                        c.m_Buffs = WeaponBondDurationBuffGuids
                            .Select(g => BlueprintTool.GetRef<Kingmaker.Blueprints.BlueprintBuffReference>(g))
                            .ToArray();
                    }),
                },
                new()
                {
                    Key = "ChallengeAoODmgEffect", Guid = ChallengeAoODmgEffectGuid, Ranks = 10,
                    DisplayName = "Attack of Opportunity Damage Against Challenged Targets",
                    Description = "+1 damage per rank on attacks of opportunity against the target of the cavalier's challenge.",
                    Components = f => f.AddComponent<DamageBonusAgainstCasterBuffTargetPerRank>(c =>
                    {
                        c.m_Buffs = new[]
                        {
                            BlueprintTool.GetRef<Kingmaker.Blueprints.BlueprintBuffReference>(CavalierChallengeTargetBuffGuid),
                        };
                        c.OnlyAttackOfOpportunity = true;
                    }),
                },
                new()
                {
                    Key = "ShifterAspectEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf1", Ranks = 7,
                    DisplayName = "Bonus Shifter Aspect",
                    Description = "+1 use per day of shifter aspect per rank.",
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(ShifterAspectResourceGuid)),
                },
                new()
                {
                    Key = "PickDmgEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b7c", Ranks = 5,
                    DisplayName = "Pick Damage",
                    Description = "+1 damage per rank on attacks made with a light or heavy pick.",
                    Components = f => f.AddComponent<WeaponCategoryDamageBonusPerRank>(c =>
                        c.Categories = new[]
                        {
                            Kingmaker.Enums.WeaponCategory.LightPick,
                            Kingmaker.Enums.WeaponCategory.HeavyPick,
                        }),
                },
                new()
                {
                    Key = "ShifterClawDmgEffect", Guid = ShifterClawDmgEffectGuid, Ranks = 4,
                    DisplayName = "Claw Damage",
                    Description = "+1 damage per rank on attacks made with the shifter claws ability.",
                    Components = f => f.AddComponent<WeaponCategoryDamageBonusPerRank>(c =>
                    {
                        c.Categories = new[] { Kingmaker.Enums.WeaponCategory.Claw };
                        c.m_RequiredOwnerBuffs = ShifterClawBuffGuids
                            .Select(g => BlueprintTool.GetRef<Kingmaker.Blueprints.BlueprintBuffReference>(g))
                            .ToArray();
                    }),
                },
                new()
                {
                    Key = "DefensiveInstinctEffect", Guid = DefensiveInstinctEffectGuid, Ranks = 5,
                    DisplayName = "Dodge Bonus Against Large Creatures",
                    Description = "+1 dodge bonus to Armor Class per rank against creatures of size Large or larger.",
                    Components = f => f.AddComponent<ACBonusAgainstLargerCreaturesPerRank>(c =>
                        c.MinimumSize = Kingmaker.Enums.Size.Large),
                },
                new()
                {
                    Key = "JudgmentEffect", Guid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b65", Ranks = 3,
                    DisplayName = "Bonus Judgment",
                    Description = "+1 use per day of judgment per rank.",
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
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
                    "This animal companion has DR/cold iron granted by its master's favored class bonus (maximum DR 10/cold iron).", tagEncyclopediaEntries: false))
                .SetIsClassFeature(true)
                // Cold iron rather than the original's magic: by the point a companion has
                // this, practically everything attacking it already bypasses DR/magic, so the
                // bonus was worth nothing in play. Deliberate deviation, see BONUS-MATRIX.md.
                .AddDamageResistancePhysical(
                    bypassedByMaterial: true,
                    material: Kingmaker.Enums.Damage.PhysicalDamageMaterial.ColdIron,
                    value: ContextValues.Rank())
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

            FeatureConfigurator.New("ZFCWCompanionHPFeature", CompanionHPPetGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.CompanionHPFeature.Name", "Companion Hit Points", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.CompanionHPFeature.Desc",
                    "This animal companion has bonus hit points granted by its master's favored class bonus.", tagEncyclopediaEntries: false))
                .SetIsClassFeature(true)
                .AddContextStatBonus(StatType.HitPoints, ContextValues.Rank())
                .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc0", useMaster: true, max: 20))
                .Configure();
            AllModGuids.Add(CompanionHPPetGuid);

            // Cavalier mount. Mechanically identical to the druid/hunter companion bonuses —
            // a mount is an animal companion, so it is a pet like any other and the same
            // MasterFeatureRank plumbing applies. Separate blueprints rather than reusing the
            // companion ones so the two counters stay independent: a character could in
            // principle hold both, and each must scale from its own picks.
            FeatureConfigurator.New("ZFCWCavalierMountHPFeature", CavalierMountHPPetGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.CavalierMountHPFeature.Name", "Mount Hit Points", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.CavalierMountHPFeature.Desc",
                    "This mount has bonus hit points granted by its rider's favored class bonus.", tagEncyclopediaEntries: false))
                .SetIsClassFeature(true)
                .AddContextStatBonus(StatType.HitPoints, ContextValues.Rank())
                .AddContextRankConfig(ContextRankConfigs.FeatureRank(CavalierMountHPCounterGuid, useMaster: true, max: 20))
                .Configure();
            AllModGuids.Add(CavalierMountHPPetGuid);

            FeatureConfigurator.New("ZFCWCavalierMountSpeedFeature", CavalierMountSpeedPetGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.CavalierMountSpeedFeature.Name", "Mount Speed", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.CavalierMountSpeedFeature.Desc",
                    "This mount has a bonus to its base speed granted by its rider's favored class bonus.", tagEncyclopediaEntries: false))
                .SetIsClassFeature(true)
                // Div-step 5 then multiplied by 5, so five picks are worth +5 feet and four are
                // worth nothing — the tabletop wording, and the same arithmetic the
                // character-side SpeedEffect performs (Multiplier = 5 over a floor(rank/5) rank).
                .AddContextStatBonus(StatType.Speed, ContextValues.Rank(), multiplier: 5)
                .AddContextRankConfig(ContextRankConfigs.FeatureRank(CavalierMountSpeedCounterGuid, useMaster: true, max: 20).WithDivStepProgression(5))
                .Configure();
            AllModGuids.Add(CavalierMountSpeedPetGuid);

            FeatureConfigurator.New("ZFCWCompanionNaturalACFeature", CompanionNaturalACPetGuid)
                .SetDisplayName(LocalizationTool.CreateString("ZFCW.CompanionNaturalACFeature.Name", "Companion Natural Armor", tagEncyclopediaEntries: false))
                .SetDescription(LocalizationTool.CreateString("ZFCW.CompanionNaturalACFeature.Desc",
                    "This animal companion has a natural armor bonus granted by its master's favored class bonus.", tagEncyclopediaEntries: false))
                .SetIsClassFeature(true)
                .AddContextStatBonus(StatType.AC, ContextValues.Rank(), Kingmaker.Enums.ModifierDescriptor.NaturalArmor)
                .AddContextRankConfig(ContextRankConfigs.FeatureRank("3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc2", useMaster: true, max: 20).WithDivStepProgression(4))
                .Configure();
            AllModGuids.Add(CompanionNaturalACPetGuid);

            // Wave 6 (bonus known spells): custom race-specific spell lists, built
            // once here from already-loaded native class lists before the defs below
            // reference them by guid.
            BuildFilteredSpellList("ZFCWGanziOracleSpellList", GanziOracleSpellListGuid, 8,
                spell => spell.School == SpellSchool.Enchantment,
                BlueprintTool.Get<BlueprintCharacterClass>(WizardClassGuid)?.Spellbook?.SpellList,
                BlueprintTool.Get<BlueprintCharacterClass>(ClericClassGuid)?.Spellbook?.SpellList);
            BuildFilteredSpellList("ZFCWGoblinSorcererSpellList", GoblinSorcererSpellListGuid, 8,
                spell => (spell.SpellDescriptor & SpellDescriptor.Fire) != 0,
                BlueprintTool.Get<BlueprintCharacterClass>(WizardClassGuid)?.Spellbook?.SpellList);
            {
                var shamanOwnGuids = new HashSet<string>();
                var shamanOwnList = BlueprintTool.Get<BlueprintCharacterClass>(ShamanClassGuid)?.Spellbook?.SpellList;
                if (shamanOwnList?.SpellsByLevel != null)
                {
                    foreach (var lvl in shamanOwnList.SpellsByLevel)
                    foreach (var s in lvl.Spells)
                        if (s != null) shamanOwnGuids.Add(s.AssetGuid.ToString());
                }
                BuildFilteredSpellList("ZFCWShamanKnownSpellList", ShamanKnownSpellListGuid, 8,
                    spell => !shamanOwnGuids.Contains(spell.AssetGuid.ToString()),
                    BlueprintTool.Get<BlueprintCharacterClass>(ClericClassGuid)?.Spellbook?.SpellList);
            }

            // Drow sorcerer: the tabletop wording is "curse, evil, or pain". WOTR's
            // SpellDescriptor has Curse and Evil but no Pain, so the filter covers the two
            // that exist — see BONUS-MATRIX.md.
            BuildFilteredSpellList("ZFCWDrowSorcererSpellList", DrowSorcererSpellListGuid, 8,
                spell => (spell.SpellDescriptor & (SpellDescriptor.Curse | SpellDescriptor.Evil)) != 0,
                BlueprintTool.Get<BlueprintCharacterClass>(SorcererClassGuid)?.Spellbook?.SpellList);

            // Kitsune shaman: enchantment spells from the wizard list that the shaman does not
            // already have, mirroring the Ganzi oracle build below.
            {
                var shamanOwn = new HashSet<string>();
                var shamanList = BlueprintTool.Get<BlueprintCharacterClass>(ShamanClassGuid)?.Spellbook?.SpellList;
                if (shamanList?.SpellsByLevel != null)
                {
                    foreach (var lvl in shamanList.SpellsByLevel)
                    foreach (var s in lvl.Spells)
                        if (s != null) shamanOwn.Add(s.AssetGuid.ToString());
                }
                BuildFilteredSpellList("ZFCWKitsuneShamanSpellList", KitsuneShamanSpellListGuid, 8,
                    spell => spell.School == SpellSchool.Enchantment
                             && !shamanOwn.Contains(spell.AssetGuid.ToString()),
                    BlueprintTool.Get<BlueprintCharacterClass>(WizardClassGuid)?.Spellbook?.SpellList);
            }

            // Wizard "bonus known spell" is one FCB entry whose reward selection
            // carries a generic track (full wizard list, hidden once any Thassilonian
            // Specialist school is taken) plus one track per school (own restricted
            // spell list, visible only with that specific specialization) — the
            // player only ever sees the one matching their build, per-option
            // prerequisites do the narrowing, no separate FCB choices needed.
            var wizardRewardFeatures = BuildKnownSpellRewardFeatures(
                "WizardKnownSpell", WizardKnownSpellLevelGuids, WizardClassGuid, 8,
                ClassSpellListGuid(WizardClassGuid),
                extraPrereq: c =>
                {
                    foreach (var track in ThassilonianTracks) c.AddPrerequisiteNoFeature(track.ArchetypeFeatureGuid);
                });
            foreach (var track in ThassilonianTracks)
            {
                wizardRewardFeatures.AddRange(BuildKnownSpellRewardFeatures(
                    $"WizardKnownSpell{track.School}", track.LevelGuids,
                    WizardClassGuid, 8, track.SpellListGuid,
                    extraPrereq: c => c.AddPrerequisiteFeature(track.ArchetypeFeatureGuid)));
            }

            var folders = new List<BonusFolderDef>
            {
                new()
                {
                    Key = "FetchlingEnergyRes", FolderGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bd9",
                    DisplayName = "Energy Resistance",
                    Description = "Add +1 to the character's cold or electricity resistance (maximum +10 each).",
                    Races = new[] { FetchlingRaceGuid },
                    Classes = new[] { BarbarianClassGuid, SorcererClassGuid },
                },
                new()
                {
                    Key = "SuliEnergyRes", FolderGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bda",
                    DisplayName = "Energy Resistance",
                    Description = "Add +1 to one of the ranger's energy resistances — acid, cold, electricity or fire (maximum +10 each).",
                    Races = new[] { SuliRaceGuid },
                    Classes = new[] { RangerClassGuid },
                },
                new()
                {
                    Key = "DruidEnergyRes", FolderGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bdb",
                    DisplayName = "Energy Resistance",
                    Description = "Add +1 to one of the druid's energy resistances — acid, cold, electricity or fire (maximum +10 each).",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { DruidClassGuid },
                },
                new()
                {
                    Key = "PaladinEnergyRes", FolderGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bdc",
                    DisplayName = "Energy Resistance",
                    Description = "Add +1 to one of the paladin's energy resistances — acid, cold, electricity or fire (maximum +10 each).",
                    Races = new[] { HumanRaceGuid },
                    Classes = new[] { PaladinClassGuid },
                },
                new()
                {
                    Key = "ShifterEnergyRes", FolderGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1be8",
                    DisplayName = "Energy Resistance",
                    Description = "Gain energy resistance 1 against acid, cold, electricity or fire. Each time the shifter selects this reward, increase that energy resistance by 1, to a maximum of energy resistance 10.",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { ShifterClassGuid },
                },
            };

            var defs = new List<RacialBonusDef>
            {
                new()
                {
                    Key = "WpFeat", FeatureGuid = WpCombatPartialGuid,
                    Divisor = 6, Ranks = 18, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Combat Feat (+1/6)",
                    Description = "Gain 1/6 of a new bonus combat feat.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { WarpriestClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b14",
                    RewardSelectionGuid = WpCombatSelGuid,
                    RewardFeatures = null, // feat list mirrored from vanilla after the loop
                },
                new()
                {
                    Key = "SpeedBarbarian", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b08",
                    Divisor = 5, Ranks = 20, DisplayKind = BonusDisplayKind.Feet,
                    DisplayName = "Bonus Speed (+1 ft.)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1a",
                    Description = "Add +1 to the barbarian's base speed. In combat this option has no effect unless the barbarian has selected it five times (or another increment of five). " +
                        "This bonus stacks with the barbarian's fast movement feature and applies under the same conditions as that feature.",
                    Races = new[] { ElfRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { BarbarianClassGuid },
                },
                new()
                {
                    Key = "SpeedBloodrager", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b11",
                    Divisor = 5, Ranks = 20, DisplayKind = BonusDisplayKind.Feet,
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
                    Divisor = 6, Ranks = 18, DisplayKind = BonusDisplayKind.Feats,
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
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
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
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
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
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
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
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
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
                    // A scion has an eldritch pool, not an arcane pool — this entry
                    // would be inert for it.
                    ExcludeArchetypes = new[] { EldritchScionArchetypeGuid },
                },
                new()
                {
                    Key = "ArcaneReservoir", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2b",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Bonus Arcane Reservoir",
                    Description = "Add +1 point to the arcanist's arcane reservoir.",
                    Races = new[] { ElfRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { ArcanistClassGuid },
                    Components = f => f.AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
                        c.m_Resource = BlueprintTool.GetRef<BlueprintAbilityResourceReference>(ArcanistArcaneReservoirResourceGuid)),
                },
                new()
                {
                    Key = "RogueTalent", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2d",
                    Divisor = 6, Ranks = 18, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Rogue Talent (+1/6)",
                    Description = "Gain 1/6 of a new rogue talent.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid,
                        ChangelingRaceGuid, KitsuneRaceGuid, SamsaranRaceGuid },
                    Classes = new[] { RogueClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b2e",
                    RewardSelectionGuid = RogueTalentRewardGuid,
                    RewardFeatures = null, // mirrored from vanilla after the loop
                },
                new()
                {
                    Key = "WitchHex", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b30",
                    Divisor = 6, Ranks = 18, DisplayKind = BonusDisplayKind.Feats,
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
                    Divisor = 6, Ranks = 18, DisplayKind = BonusDisplayKind.Feats,
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
                    Divisor = 6, Ranks = 18, DisplayKind = BonusDisplayKind.Feats,
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
                    Divisor = 6, Ranks = 18, DisplayKind = BonusDisplayKind.Feats,
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
                    Divisor = 6, Ranks = 18, DisplayKind = BonusDisplayKind.Feats,
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
                    Divisor = 6, Ranks = 18, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Magus Arcana (+1/6)",
                    Description = "Gain 1/6 of a new magus arcana.",
                    Races = new[] { ElfRaceGuid, HalflingRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { MagusClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b40",
                    RewardSelectionGuid = MagusArcanaRewardGuid,
                    RewardFeatures = null,
                    // The scion's arcana are a separate, Charisma-based pool — it gets
                    // its own entry below instead.
                    ExcludeArchetypes = new[] { EldritchScionArchetypeGuid },
                },
                new()
                {
                    Key = "EldritchArcana", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bad",
                    Divisor = 6, Ranks = 18, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Eldritch Scion Arcana (+1/6)",
                    Description = "Gain 1/6 of a new magus arcana.",
                    Races = new[] { ElfRaceGuid, HalflingRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { MagusClassGuid },
                    RequireArchetype = EldritchScionArchetypeGuid,
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bae",
                    RewardSelectionGuid = EldritchArcanaRewardGuid,
                    RewardFeatures = null, // mirrored from the eldritch (Charisma-based) arcana pool
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
                    // Corrected shape: the original lets you add +1/4 to ONE of your
                    // already-chosen favored enemies (max +1 each), not a blanket bonus
                    // against all of them. Each reward is one +1 against one enemy type.
                    Key = "FavoredEnemyPick", FeatureGuid = FavoredEnemyPickFeatureGuid,
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Favored Enemy Bonus (+1/4)",
                    Description = "Add +1/4 to a single existing favored enemy bonus (maximum bonus +1 per favored enemy).",
                    Races = new[] { HobgoblinRaceGuid },
                    Classes = new[] { RangerClassGuid },
                    ProgressGuid = FavoredEnemyPickProgressGuid,
                    RewardSelectionGuid = FavoredEnemyPickRewardGuid,
                    RewardFeatures = null, // one feature per favored enemy type, generated after the loop
                },
                new()
                {
                    Key = "ReservoirRegen", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bba",
                    Divisor = 6, Ranks = 18,
                    DisplayName = "Bonus Arcane Reservoir Restored (+1/6)", EffectGuid = ReservoirRegenEffectGuid,
                    Description = "Add 1/6 to the number of points the arcanist gains in her arcane reservoir each day.",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { ArcanistClassGuid },
                },
                new()
                {
                    Key = "PatronCL", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bbc",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Patron Spells Caster Level (+1/4)", EffectGuid = PatronCLEffectGuid,
                    Description = "Add +1/4 to the witch's caster level when casting spells from her patron spell list.",
                    Races = new[] { HalflingRaceGuid },
                    Classes = new[] { WitchClassGuid },
                },
                new()
                {
                    Key = "AlchFireRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b57",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Fire Resistance",
                    Description = "Add +1 to the alchemist's fire resistance (maximum +10).",
                    Races = new[] { GoblinRaceGuid },
                    Classes = new[] { AlchemistClassGuid },
                    // Value is a flat 1, NOT ContextValues.Rank(). This component scales itself:
                    // AddDamageResistanceEnergy.CalculateValue is `Fact.GetRank() * Value`, unlike
                    // AddDamageResistancePhysical, AddContextStatBonus and AddCMBBonusForManeuver,
                    // which all take the value as-is and so do need an explicit rank. Passing the
                    // rank here made every energy resistance rank SQUARED — correct at 1 pick,
                    // which is why it survived from Wave 5 to v0.1.5 unnoticed; 4 picks read 16.
                    // Every entry below follows this same shape.
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Fire, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "ColdRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b58",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Cold Resistance",
                    Description = "Add +1 to the character's cold resistance (maximum +10).",
                    Races = new[] { FetchlingRaceGuid },
                    Classes = new[] { BarbarianClassGuid, SorcererClassGuid },
                    Folder = "FetchlingEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Cold, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "ElecRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b59",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Electricity Resistance",
                    Description = "Add +1 to the character's electricity resistance (maximum +10).",
                    Races = new[] { FetchlingRaceGuid },
                    Classes = new[] { BarbarianClassGuid, SorcererClassGuid },
                    Folder = "FetchlingEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Electricity, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "SuliAcidRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5a",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Acid Resistance",
                    Description = "Add +1 to the ranger's acid resistance (maximum +10).",
                    Races = new[] { SuliRaceGuid },
                    Classes = new[] { RangerClassGuid },
                    Folder = "SuliEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Acid, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "SuliColdRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5b",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Cold Resistance",
                    Description = "Add +1 to the ranger's cold resistance (maximum +10).",
                    Races = new[] { SuliRaceGuid },
                    Classes = new[] { RangerClassGuid },
                    Folder = "SuliEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Cold, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "SuliFireRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5c",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Fire Resistance",
                    Description = "Add +1 to the ranger's fire resistance (maximum +10).",
                    Races = new[] { SuliRaceGuid },
                    Classes = new[] { RangerClassGuid },
                    Folder = "SuliEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Fire, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "SuliElecRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b5d",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Electricity Resistance",
                    Description = "Add +1 to the ranger's electricity resistance (maximum +10).",
                    Races = new[] { SuliRaceGuid },
                    Classes = new[] { RangerClassGuid },
                    Folder = "SuliEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Electricity, value: ContextValues.Constant(1)),
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
                    Races = new[] { ElfRaceGuid, HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, TieflingRaceGuid, AasimarRaceGuid,
                        KitsuneRaceGuid },
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
                    Divisor = 1, Ranks = 19, SkipBonusDisplay = true, // real curve is 1, +1/2... not floor(rank/Divisor)
                    DisplayName = "Companion Damage Reduction",
                    Description = "The character's animal companion gains DR 1/cold iron. Each additional time this bonus is selected, the DR increases by 1/2 (maximum DR 10/cold iron).",
                    Races = new[] { GnomeRaceGuid, FetchlingRaceGuid, SvirfneblinRaceGuid },
                    Classes = new[] { HunterClassGuid, RangerClassGuid, DruidClassGuid },
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
                    Divisor = 4, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Teamwork Feat (+1/4)",
                    // RAW (Blood of Shadows p.15) is "Gain 1/4 of a teamwork feat" for
                    // Drow. Half-Elf/Halfling's own RAW text ("+1/4 to the number of
                    // times per day the inquisitor can change her most recent teamwork
                    // feat") has no equivalent mechanic in WOTR, so they share this same
                    // entry as the closest available substitute instead of being ported
                    // literally — see BONUS-MATRIX.md.
                    Description = "Gain 1/4 of a new teamwork feat.",
                    Races = new[] { DrowRaceGuid, HalfElfRaceGuid, HalflingRaceGuid },
                    Classes = new[] { InquisitorClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6b",
                    RewardSelectionGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6c",
                    RewardFeatures = null, // mirrored from the inquisitor teamwork pool
                },
                new()
                {
                    Key = "SpeedMonk", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6d",
                    Divisor = 5, Ranks = 20, DisplayKind = BonusDisplayKind.Feet,
                    DisplayName = "Bonus Speed (+1 ft.)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1a",
                    Description = "Add +1 to the monk's base speed. In combat this option has no effect unless the monk has selected it five times (or another increment of five). " +
                        "This bonus stacks with the monk's fast movement feature and applies under the same conditions as that feature.",
                    Races = new[] { ElfRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { MonkClassGuid },
                },
                new()
                {
                    Key = "Cruelty", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6e",
                    Divisor = 4, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Cruelty (+1/4)",
                    Description = "Gain 1/4 of a new cruelty.",
                    Races = new[] { DrowRaceGuid },
                    Classes = new[] { AntipaladinClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b6f",
                    RewardSelectionGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b70",
                    RewardFeatures = null, // mirrored from MCE's cruelty selection
                },
                // Wave 6: bonus known spells. All 1/2-increment wrapper entries —
                // reward features are freshly-authored parametrized spell picks
                // (BuildKnownSpellRewardFeatures), not mirrored vanilla selections.
                new()
                {
                    Key = "AlchemistKnownSpell", FeatureGuid = AlchemistKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Formula (+1/2)",
                    Description = "Add 1/2 formula known to the alchemist's formula book. This formula must be at least 1 level below the highest formula level the alchemist can create.",
                    Races = new[] { ElfRaceGuid, HumanRaceGuid, HalflingRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { AlchemistClassGuid },
                    ProgressGuid = AlchemistKnownSpellProgressGuid,
                    RewardSelectionGuid = AlchemistKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("AlchemistKnownSpell", AlchemistKnownSpellLevelGuids,
                        AlchemistClassGuid, 5, ClassSpellListGuid(AlchemistClassGuid), levelNoun: "Formula").ToArray(),
                },
                new()
                {
                    Key = "BardKnownSpell", FeatureGuid = BardKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell (+1/2)",
                    Description = "Add 1/2 spell known to the bard's spell list. This spell must be at least 1 level below the highest spell level the bard can cast.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { BardClassGuid },
                    ProgressGuid = BardKnownSpellProgressGuid,
                    RewardSelectionGuid = BardKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("BardKnownSpell", BardKnownSpellLevelGuids,
                        BardClassGuid, 5, ClassSpellListGuid(BardClassGuid)).ToArray(),
                },
                new()
                {
                    Key = "InquisitorKnownSpell", FeatureGuid = InquisitorKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell (+1/2)",
                    Description = "Add 1/2 spell known to the inquisitor's spell list. This spell must be at least 1 level below the highest spell level the inquisitor can cast.",
                    Races = new[] { ElfRaceGuid, HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { InquisitorClassGuid },
                    ProgressGuid = InquisitorKnownSpellProgressGuid,
                    RewardSelectionGuid = InquisitorKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("InquisitorKnownSpell", InquisitorKnownSpellLevelGuids,
                        InquisitorClassGuid, 5, ClassSpellListGuid(InquisitorClassGuid)).ToArray(),
                },
                new()
                {
                    Key = "OracleKnownSpell", FeatureGuid = OracleKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell (+1/2)",
                    Description = "Add 1/2 spell known to the oracle's spell list. This spell must be at least 1 level below the highest spell level the oracle can cast.",
                    Races = new[] { ElfRaceGuid, HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { OracleClassGuid },
                    ProgressGuid = OracleKnownSpellProgressGuid,
                    RewardSelectionGuid = OracleKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("OracleKnownSpell", OracleKnownSpellLevelGuids,
                        OracleClassGuid, 8, ClassSpellListGuid(OracleClassGuid)).ToArray(),
                },
                new()
                {
                    Key = "OracleGanziKnownSpell", FeatureGuid = OracleGanziKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell — Enchantment (+1/2)",
                    Description = "Add 1/2 spell known of the enchantment school from the cleric or wizard spell list. This spell must be at least 1 level below the highest spell level the oracle can cast.",
                    Races = new[] { GanziRaceGuid },
                    Classes = new[] { OracleClassGuid },
                    ProgressGuid = OracleGanziKnownSpellProgressGuid,
                    RewardSelectionGuid = OracleGanziKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("OracleGanziKnownSpell", OracleGanziKnownSpellLevelGuids,
                        OracleClassGuid, 8, GanziOracleSpellListGuid).ToArray(),
                },
                new()
                {
                    Key = "ShamanKnownSpell", FeatureGuid = ShamanKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell (+1/2)",
                    Description = "Add 1/2 spell known from the cleric spell list (excluding spells already on the shaman spell list). This spell must be at least 1 level below the highest spell level the shaman can cast.",
                    Races = new[] { HalfElfRaceGuid, HumanRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { ShamanClassGuid },
                    ProgressGuid = ShamanKnownSpellProgressGuid,
                    RewardSelectionGuid = ShamanKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("ShamanKnownSpell", ShamanKnownSpellLevelGuids,
                        ShamanClassGuid, 8, ShamanKnownSpellListGuid).ToArray(),
                },
                new()
                {
                    Key = "SorcererKnownSpell", FeatureGuid = SorcererKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell (+1/2)",
                    Description = "Add 1/2 spell known to the sorcerer's spell list. This spell must be at least 1 level below the highest spell level the sorcerer can cast.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { SorcererClassGuid },
                    ProgressGuid = SorcererKnownSpellProgressGuid,
                    RewardSelectionGuid = SorcererKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("SorcererKnownSpell", SorcererKnownSpellLevelGuids,
                        SorcererClassGuid, 8, ClassSpellListGuid(SorcererClassGuid)).ToArray(),
                },
                new()
                {
                    Key = "SorcererGoblinKnownSpell", FeatureGuid = SorcererGoblinKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell — Fire (+1/2)",
                    Description = "Add 1/2 spell known from the sorcerer spell list. This spell must be at least 1 level below the highest spell level the sorcerer can cast, and must have the fire descriptor.",
                    Races = new[] { GoblinRaceGuid },
                    Classes = new[] { SorcererClassGuid },
                    ProgressGuid = SorcererGoblinKnownSpellProgressGuid,
                    RewardSelectionGuid = SorcererGoblinKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("SorcererGoblinKnownSpell", SorcererGoblinKnownSpellLevelGuids,
                        SorcererClassGuid, 8, GoblinSorcererSpellListGuid).ToArray(),
                },
                new()
                {
                    Key = "WizardKnownSpell", FeatureGuid = WizardKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell (+1/2)",
                    Description = "Add 1/2 spell known to the wizard's spellbook. This spell must be at least 1 level below the highest spell level the wizard can cast. " +
                        "A Thassilonian Specialist instead learns this spell from the spell list of his bound school.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { WizardClassGuid },
                    ProgressGuid = WizardKnownSpellProgressGuid,
                    RewardSelectionGuid = WizardKnownSpellRewardGuid,
                    RewardFeatures = wizardRewardFeatures.ToArray(),
                },
                new()
                {
                    Key = "WitchKnownSpell", FeatureGuid = WitchKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell (+1/2)",
                    Description = "Add 1/2 spell known to the witch's spell list. This spell must be at least 1 level below the highest spell level the witch can cast.",
                    // Changeling and Orc are the "add one spell to the witch's familiar" line.
                    // Familiars are not controllable in WOTR and hold no spell list of their own,
                    // so the bonus is the same bonus known spell, and gets the same entry.
                    Races = new[] { HumanRaceGuid, HalfOrcRaceGuid, HalfElfRaceGuid, ElfRaceGuid, AasimarRaceGuid, TieflingRaceGuid, GoblinRaceGuid, ChangelingRaceGuid, OrcRaceGuid },
                    Classes = new[] { WitchClassGuid },
                    ProgressGuid = WitchKnownSpellProgressGuid,
                    RewardSelectionGuid = WitchKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("WitchKnownSpell", WitchKnownSpellLevelGuids,
                        WitchClassGuid, 8, ClassSpellListGuid(WitchClassGuid)).ToArray(),
                },
                new()
                {
                    Key = "SkaldKnownSpell", FeatureGuid = SkaldKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell (+1/2)",
                    Description = "Add 1/2 spell known to the skald's spell list. This spell must be at least 1 level below the highest spell level the skald can cast.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { SkaldClassGuid },
                    ProgressGuid = SkaldKnownSpellProgressGuid,
                    RewardSelectionGuid = SkaldKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("SkaldKnownSpell", SkaldKnownSpellLevelGuids,
                        SkaldClassGuid, 5, ClassSpellListGuid(SkaldClassGuid)).ToArray(),
                },
                new()
                {
                    Key = "ArcanistKnownSpell", FeatureGuid = ArcanistKnownSpellFeatureGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell (+1/2)",
                    Description = "Add 1/2 spell known to the arcanist's spell list. This spell must be at least 1 level below the highest spell level the arcanist can cast.",
                    Races = new[] { HumanRaceGuid, HalfElfRaceGuid, HalfOrcRaceGuid, AasimarRaceGuid, TieflingRaceGuid },
                    Classes = new[] { ArcanistClassGuid },
                    ProgressGuid = ArcanistKnownSpellProgressGuid,
                    RewardSelectionGuid = ArcanistKnownSpellRewardGuid,
                    RewardFeatures = BuildKnownSpellRewardFeatures("ArcanistKnownSpell", ArcanistKnownSpellLevelGuids,
                        ArcanistClassGuid, 8, ClassSpellListGuid(ArcanistClassGuid)).ToArray(),
                    ExcludeArchetypes = ArcanistKnownSpellExcludedArchetypes,
                },
                // Wave 7: lay on hands, conditional natural armor, channel energy,
                // fervor. (The eldritch scion arcana entry sits next to the regular
                // magus arcana entry above.)
                new()
                {
                    Key = "LayOnHands", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1ba9",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Lay on Hands Bonus (+1/2)", EffectGuid = LayOnHandsEffectGuid,
                    Description = "Add +1/2 hit point to the paladin's lay on hands ability (whether using it to heal or harm).",
                    Races = new[] { ElfRaceGuid, GnomeRaceGuid, HalflingRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { PaladinClassGuid },
                },
                new()
                {
                    Key = "LayOnHandsSelf", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1baa",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Lay on Hands Self-Healing Bonus",
                    Description = "Add +1 to the amount of damage the paladin heals with lay on hands, but only when the paladin uses that ability on herself.",
                    Races = new[] { TieflingRaceGuid },
                    Classes = new[] { PaladinClassGuid },
                    // No divisor, so the effect rides the counter directly instead of a
                    // separate feature — one entry in Special Abilities, not two identical ones.
                    Components = f => f.AddComponent<HealBonusForAbilitiesPerRank>(c =>
                    {
                        c.m_Abilities = LayOnHandsAbilityGuids.Select(g =>
                            BlueprintTool.GetRef<BlueprintAbilityReference>(g)).ToArray();
                        c.SelfOnly = true;
                    }),
                },
                new()
                {
                    Key = "WildShapeAC", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bac",
                    Divisor = 3, Ranks = 18,
                    DisplayName = "Wild Shape Natural Armor Bonus (+1/3)", EffectGuid = WildShapeACEffectGuid,
                    Description = "Add +1/3 to the druid's natural armor bonus when using wild shape.",
                    Races = new[] { ElfRaceGuid, HalfOrcRaceGuid, HalfElfRaceGuid },
                    Classes = new[] { DruidClassGuid },
                },
                new()
                {
                    Key = "ChannelEnergy", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bb1",
                    Divisor = 3, Ranks = 18,
                    DisplayName = "Channel Energy Bonus (+1/3)", EffectGuid = ChannelEnergyEffectGuid,
                    Description = "Add +1/3 point to the amount of damage healed or dealt by the character's channel energy ability.",
                    Races = new[] { HalfElfRaceGuid },
                    Classes = new[] { ClericClassGuid, WarpriestClassGuid },
                },
                new()
                {
                    Key = "HarmUndead", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bb5",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Harm Undead Bonus (+1/2)", EffectGuid = HarmUndeadEffectGuid,
                    Description = "Add +1/2 point to the amount of damage dealt by the cleric's channel energy ability for the purpose of harming undead.",
                    Races = new[] { AasimarRaceGuid },
                    Classes = new[] { ClericClassGuid },
                },
                // Wave 10.
                new()
                {
                    Key = "BombDmg", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bbe",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Bomb Damage (+1/2)", EffectGuid = BombDmgEffectGuid,
                    Description = "Add +1/2 to the alchemist's bomb damage.",
                    Races = new[] { HalfOrcRaceGuid, TieflingRaceGuid },
                    Classes = new[] { AlchemistClassGuid },
                },
                new()
                {
                    Key = "CompanionHP", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc0",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Companion Hit Points",
                    Description = "Add +1 hit point to the character's animal companion.",
                    Races = new[] { GoblinRaceGuid, HalfOrcRaceGuid },
                    Classes = new[] { DruidClassGuid, HunterClassGuid },
                    Components = f => f.AddComponent<GrantFeatureToPetsWhileActive>(c =>
                        c.m_Feature = BlueprintTool.GetRef<BlueprintUnitFactReference>(CompanionHPPetGuid)),
                },
                new()
                {
                    Key = "CompanionNaturalAC", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc2",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Companion Natural Armor (+1/4)",
                    Description = "Add +1/4 to the natural armor bonus of the ranger's animal companion.",
                    Races = new[] { OreadRaceGuid },
                    Classes = new[] { RangerClassGuid },
                    Components = f => f.AddComponent<GrantFeatureToPetsWhileActive>(c =>
                        c.m_Feature = BlueprintTool.GetRef<BlueprintUnitFactReference>(CompanionNaturalACPetGuid)),
                },
                new()
                {
                    Key = "DruidAcidRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc4",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Acid Resistance",
                    Description = "Add +1 to the druid's acid resistance (maximum +10).",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { DruidClassGuid },
                    Folder = "DruidEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Acid, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "DruidColdRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc5",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Cold Resistance",
                    Description = "Add +1 to the druid's cold resistance (maximum +10).",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { DruidClassGuid },
                    Folder = "DruidEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Cold, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "DruidElecRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc6",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Electricity Resistance",
                    Description = "Add +1 to the druid's electricity resistance (maximum +10).",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { DruidClassGuid },
                    Folder = "DruidEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Electricity, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "DruidFireRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc7",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Fire Resistance",
                    Description = "Add +1 to the druid's fire resistance (maximum +10).",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { DruidClassGuid },
                    Folder = "DruidEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Fire, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "PaladinAcidRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc8",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Acid Resistance",
                    Description = "Add +1 to the paladin's acid resistance (maximum +10).",
                    Races = new[] { HumanRaceGuid },
                    Classes = new[] { PaladinClassGuid },
                    Folder = "PaladinEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Acid, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "PaladinColdRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bc9",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Cold Resistance",
                    Description = "Add +1 to the paladin's cold resistance (maximum +10).",
                    Races = new[] { HumanRaceGuid },
                    Classes = new[] { PaladinClassGuid },
                    Folder = "PaladinEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Cold, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "PaladinElecRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bca",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Electricity Resistance",
                    Description = "Add +1 to the paladin's electricity resistance (maximum +10).",
                    Races = new[] { HumanRaceGuid },
                    Classes = new[] { PaladinClassGuid },
                    Folder = "PaladinEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Electricity, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "PaladinFireRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bcb",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Fire Resistance",
                    Description = "Add +1 to the paladin's fire resistance (maximum +10).",
                    Races = new[] { HumanRaceGuid },
                    Classes = new[] { PaladinClassGuid },
                    Folder = "PaladinEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Fire, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "ConcSkald", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bcc",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Concentration Bonus",
                    Description = "Add a +1 bonus on concentration checks when casting spells.",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { SkaldClassGuid },
                    Components = f => f.AddComponent<ConcentrationBonusPerRank>(),
                },
                new()
                {
                    Key = "Blessing", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bcd",
                    Divisor = 3, Ranks = 18,
                    DisplayName = "Bonus Blessings (+1/3)", EffectGuid = BlessingEffectGuid,
                    Description = "Add 1/3 to the number of times per day the warpriest can use blessings.",
                    Races = new[] { DwarfRaceGuid, ElfRaceGuid, NagajiRaceGuid },
                    Classes = new[] { WarpriestClassGuid },
                },
                new()
                {
                    Key = "ChaosCL", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b7e",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Chaotic Spell Caster Level (+1/2)",
                    EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b7f",
                    Description = "Add +1/2 to the arcanist's effective caster level when casting spells with the chaotic descriptor.",
                    Races = new[] { GanziRaceGuid },
                    Classes = new[] { ArcanistClassGuid },
                },
                new()
                {
                    Key = "GoodCL", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bcf",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Good Spell Caster Level (+1/4)", EffectGuid = GoodCLEffectGuid,
                    Description = "Add +1/4 to the sorcerer's caster level when casting spells with the good descriptor.",
                    Races = new[] { AasimarRaceGuid },
                    Classes = new[] { SorcererClassGuid },
                },
                new()
                {
                    Key = "SorcererDrowKnownSpell", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bd1",
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell — Curse or Evil (+1/2)",
                    Description = "Add 1/2 spell known from the sorcerer spell list. This spell must have the curse or evil descriptor, and be at least 1 level below the highest spell level the sorcerer can cast.",
                    Races = new[] { DrowRaceGuid },
                    Classes = new[] { SorcererClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bd2",
                    RewardSelectionGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bd3",
                    RewardFeatures = BuildKnownSpellRewardFeatures("SorcererDrowKnownSpell", SorcererDrowKnownSpellLevelGuids,
                        SorcererClassGuid, 8, DrowSorcererSpellListGuid).ToArray(),
                },
                new()
                {
                    Key = "ShamanKitsuneKnownSpell", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bd5",
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Known Spell — Enchantment (+1/2)",
                    Description = "Add 1/2 enchantment spell known from the wizard spell list that is not on the shaman spell list. This spell must be at least 1 level below the highest spell level the shaman can cast.",
                    Races = new[] { KitsuneRaceGuid },
                    Classes = new[] { ShamanClassGuid },
                    ProgressGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bd6",
                    RewardSelectionGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bd7",
                    RewardFeatures = BuildKnownSpellRewardFeatures("ShamanKitsuneKnownSpell", ShamanKitsuneKnownSpellLevelGuids,
                        ShamanClassGuid, 8, KitsuneShamanSpellListGuid).ToArray(),
                },
                new()
                {
                    Key = "PreciseStrikePick", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b7d",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Precise Strike Damage with Picks (+1/4)",
                    EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b7c",
                    // Precise strike's extra damage IS the swashbuckler's class level, so raising
                    // that level for this purpose is simply more damage on the attack — the same
                    // reasoning as the cavalier's challenge entries.
                    Description = "Add +1/4 to the swashbuckler's effective class level to determine the extra damage she deals because of the precise strike deed when wielding a light pick or a heavy pick.",
                    Races = new[] { DwarfRaceGuid },
                    Classes = new[] { SwashbucklerClassGuid },
                },

                // Wave 12. "Select one power granted at 1st level that is normally usable
                // 3 + modifier times per day; add +1/2 to its uses." One reward feature per
                // domain or school, generated after the loop from the game's own selections.
                new()
                {
                    Key = "ClericPowerUse", FeatureGuid = ClericPowerUseCounterGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Domain Power Use (+1/2)",
                    Description = "Select one domain power granted at 1st level that is normally usable a number of times per day equal to 3 + the cleric's Wisdom modifier. Add +1/2 to the number of uses per day of that domain power.",
                    Races = new[] { DwarfRaceGuid, ElfRaceGuid, HalflingRaceGuid, HalfOrcRaceGuid },
                    Classes = new[] { ClericClassGuid },
                    ProgressGuid = ClericPowerUseProgressGuid,
                    RewardSelectionGuid = ClericPowerUseRewardGuid,
                    RewardFeatures = null, // one per domain, generated after the loop
                },
                new()
                {
                    Key = "DruidPowerUse", FeatureGuid = DruidPowerUseCounterGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus Domain Power Use (+1/2)",
                    Description = "Select one domain power granted at 1st level that is normally usable a number of times per day equal to 3 + the druid's Wisdom modifier. Add +1/2 to the number of uses per day of that domain power.",
                    Races = new[] { DwarfRaceGuid },
                    Classes = new[] { DruidClassGuid },
                    ProgressGuid = DruidPowerUseProgressGuid,
                    RewardSelectionGuid = DruidPowerUseRewardGuid,
                    RewardFeatures = null,
                },
                new()
                {
                    Key = "WizardPowerUse", FeatureGuid = WizardPowerUseCounterGuid,
                    Divisor = 2, Ranks = 20, DisplayKind = BonusDisplayKind.Feats,
                    DisplayName = "Bonus School Power Use (+1/2)",
                    Description = "Select one arcane school power at 1st level that is normally usable a number of times per day equal to 3 + the wizard's Intelligence modifier. Add +1/2 to the number of uses per day of that arcane school power.",
                    Races = new[] { DrowRaceGuid, ElfRaceGuid, GnomeRaceGuid },
                    Classes = new[] { WizardClassGuid },
                    ProgressGuid = WizardPowerUseProgressGuid,
                    RewardSelectionGuid = WizardPowerUseRewardGuid,
                    RewardFeatures = null,
                },

                // Wave 14. The pointwise tabletop lines that had no mechanism yet. Each one is a
                // single race/class pair with its own condition; what they share is that the
                // condition turned out to be expressible with a vanilla test, so none of them
                // needed a new mechanism, only a new component.
                new()
                {
                    Key = "BloodrageDodge", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf5",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Dodge Bonus Against Larger Creatures (+1/4)",
                    EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bfb",
                    Description = "Gain a +1/4 dodge bonus to Armor Class while bloodraging against creatures at least one size category larger than the bloodrager.",
                    Races = new[] { HalflingRaceGuid },
                    Classes = new[] { BloodragerClassGuid },
                },
                new()
                {
                    Key = "MutagenDuration", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf9",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Mutagen Duration (+10 minutes)",
                    EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bff",
                    Description = "Add +10 minutes to the duration of the alchemist's mutagens.",
                    Races = new[] { DhampirRaceGuid },
                    Classes = new[] { AlchemistClassGuid },
                },
                new()
                {
                    Key = "OutsiderSpellPen", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf7",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Spell Penetration Against Outsiders (+1/2)",
                    EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bfd",
                    // The tabletop line reads "+1", which as a favored class bonus would reach
                    // +20 and dwarf every comparable entry; halved on the user's decision, the
                    // same call made for the other flat-looking lines.
                    Description = "Add a +1/2 bonus on caster level checks made to overcome the spell resistance of outsiders.",
                    Races = new[] { HumanRaceGuid, TieflingRaceGuid },
                    Classes = new[] { ClericClassGuid },
                },
                new()
                {
                    Key = "FlankedDamage", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf6",
                    Divisor = 3, Ranks = 20,
                    DisplayName = "Damage Against Flanked Opponents (+1/3)",
                    EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bfc",
                    Description = "Add 1/3 to damage rolls the fighter makes with weapon attacks against an opponent that he is flanking or that is denied its Dexterity bonus to Armor Class.",
                    Races = new[] { KitsuneRaceGuid },
                    Classes = new[] { FighterClassGuid },
                },
                new()
                {
                    Key = "DivineBondDuration", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bfa",
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Divine Bond Duration (+1/2 minute)",
                    EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1c00",
                    Description = "Add 1/2 minute to the duration of the paladin's divine bond with her weapon.",
                    Races = new[] { NagajiRaceGuid },
                    Classes = new[] { PaladinClassGuid },
                },
                new()
                {
                    Key = "AuraBonus", FeatureGuid = AuraBonusCounterGuid,
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Aura of Courage and Resolve Bonus (+1/4)",
                    EffectGuid = AuraBonusEffectGuid,
                    Description = "Add +1/4 to the bonus the paladin grants her allies with her aura of courage and aura of resolve special abilities.",
                    Races = new[] { FetchlingRaceGuid, OreadRaceGuid },
                    Classes = new[] { PaladinClassGuid },
                },
                new()
                {
                    Key = "SneakBeforeAct", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf8",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Sneak Attack Damage Before the Target Acts (+1/2)",
                    EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bfe",
                    // As with the cleric entry above, the tabletop "+1" is halved.
                    Description = "Add a +1/2 bonus on the rogue's sneak attack damage rolls during the surprise round or before the target has acted in combat.",
                    Races = new[] { GoblinRaceGuid },
                    Classes = new[] { RogueClassGuid },
                },
                new()
                {
                    Key = "SacredWeaponLevel", FeatureGuid = SacredWeaponLevelCounterGuid,
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Sacred Weapon Damage Level (+1/4)",
                    EffectGuid = SacredWeaponLevelEffectGuid,
                    Description = "Add 1/4 to the warpriest's effective level when determining the damage of his sacred weapon.",
                    Races = new[] { HalflingRaceGuid },
                    Classes = new[] { WarpriestClassGuid },
                },

                // Wave 11. Cavalier and shifter — two classes that had a favored class
                // progression but no racial bonuses at all, so the only options were the
                // universal hit point and skill rank.
                new()
                {
                    Key = "ChallengeDmgAasimar", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bde",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Damage Against Challenged Targets (+1/4)", EffectGuid = ChallengeDmgEffectGuid,
                    Description = "Add +1/4 to the cavalier's bonus on damage against targets of his challenge.",
                    Races = new[] { AasimarRaceGuid },
                    Classes = new[] { CavalierClassGuid },
                },
                new()
                {
                    Key = "ChallengeDmgDwarf", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bea",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Damage Against Challenged Targets (+1/2)", EffectGuid = ChallengeDmgEffectGuid,
                    Description = "Add +1/2 to the cavalier's bonus to damage against targets of his challenge.",
                    Races = new[] { DwarfRaceGuid },
                    Classes = new[] { CavalierClassGuid },
                },
                new()
                {
                    Key = "CavalierMountHP", FeatureGuid = CavalierMountHPCounterGuid,
                    Divisor = 1, Ranks = 20,
                    DisplayName = "Mount Hit Points",
                    Description = "Add +1 hit point to the cavalier's mount. If the cavalier ever replaces his mount, the new mount gains these bonus hit points.",
                    Races = new[] { ElfRaceGuid, GoblinRaceGuid, HalfOrcRaceGuid },
                    Classes = new[] { CavalierClassGuid },
                    Components = f => f.AddComponent<GrantFeatureToPetsWhileActive>(c =>
                        c.m_Feature = BlueprintTool.GetRef<BlueprintUnitFactReference>(CavalierMountHPPetGuid)),
                },
                new()
                {
                    Key = "CavalierMountSpeed", FeatureGuid = CavalierMountSpeedCounterGuid,
                    Divisor = 5, Ranks = 20, DisplayKind = BonusDisplayKind.Feet,
                    DisplayName = "Mount Speed (+1 ft.)",
                    Description = "Add +1 to the cavalier's mounted base speed. In combat this has no effect unless the cavalier has selected this reward five times (or another increment of five). " +
                        "If the cavalier ever replaces his mount, the new mount gains this bonus to its speed.",
                    Races = new[] { GnomeRaceGuid, HalfElfRaceGuid, NagajiRaceGuid },
                    Classes = new[] { CavalierClassGuid },
                    Components = f => f.AddComponent<GrantFeatureToPetsWhileActive>(c =>
                        c.m_Feature = BlueprintTool.GetRef<BlueprintUnitFactReference>(CavalierMountSpeedPetGuid)),
                },
                new()
                {
                    Key = "CavalierBanner", FeatureGuid = BannerBonusCounterGuid,
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Banner Bonus (+1/4)", EffectGuid = BannerBonusEffectGuid,
                    // The counter holds picks and the effect feature holds earned bonuses, as
                    // everywhere else. What differs is only how the effect is consumed: rather
                    // than carrying a component, its rank is read by
                    // ContextRankConfig_GetValue_RaisePatch and added to the value the banner
                    // already computes, so the bonus flows through the game's own scaling.
                    Description = "Add +1/4 to the cavalier's banner bonus.",
                    Races = new[] { HumanRaceGuid, KitsuneRaceGuid },
                    Classes = new[] { CavalierClassGuid },
                },
                new()
                {
                    Key = "ChallengeAoODmgHalfling", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bef",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Attack of Opportunity Damage Against Challenged Targets (+1/2)",
                    EffectGuid = ChallengeAoODmgEffectGuid,
                    Description = "Add +1/2 to the cavalier's effective class level for the purposes of determining the damage he deals when making an attack of opportunity against a challenged foe.",
                    Races = new[] { HalflingRaceGuid },
                    Classes = new[] { CavalierClassGuid },
                },
                new()
                {
                    Key = "ShifterMinorForm", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf2",
                    Divisor = 3, Ranks = 20,
                    DisplayName = "Bonus Shifter Aspect (+1/3)",
                    EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf1",
                    // The tabletop bonus is measured in minutes ("add 1/3 to the number of minutes
                    // the shifter can assume her minor form each day"). WOTR does not track the
                    // form in minutes at all — it meters the aspect as a per-day resource
                    // (ShifterAspectResource, base 3 plus one per class level), so the bonus is
                    // carried over as uses rather than minutes. Same pool, same "more minor form
                    // per day" effect; the unit is the game's, not the book's.
                    Description = "Add +1/3 to the number of times per day the shifter can assume her minor form.",
                    Races = new[] { HumanRaceGuid },
                    Classes = new[] { ShifterClassGuid },
                },
                new()
                {
                    Key = "ShifterClawDmg", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bf0",
                    Divisor = 5, Ranks = 20,
                    DisplayName = "Claw Damage (+1/5)", EffectGuid = ShifterClawDmgEffectGuid,
                    Description = "Add +1/5 to the damage dealt when using the shifter claws ability.",
                    Races = new[] { OrcRaceGuid },
                    Classes = new[] { ShifterClassGuid },
                },
                new()
                {
                    Key = "SpeedShifter", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1beb",
                    Divisor = 5, Ranks = 20, DisplayKind = BonusDisplayKind.Feet,
                    DisplayName = "Bonus Speed (+1 ft.)", EffectGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1b1a",
                    Description = "Add +1 to the shifter's base speed. In combat this option has no effect unless the shifter has selected it five times (or another increment of five).",
                    Races = new[] { ElfRaceGuid },
                    Classes = new[] { ShifterClassGuid },
                },
                new()
                {
                    Key = "ShifterDefensiveInstinct", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bec",
                    Divisor = 4, Ranks = 20,
                    DisplayName = "Dodge Bonus Against Large Creatures (+1/4)", EffectGuid = DefensiveInstinctEffectGuid,
                    Description = "Increase the Armor Class bonus from defensive instinct by 1/4 against creatures of size Large or larger.",
                    Races = new[] { HalflingRaceGuid },
                    Classes = new[] { ShifterClassGuid },
                },
                new()
                {
                    Key = "ShifterAcidRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1be4",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Acid Resistance",
                    Description = "Gain +1 acid resistance (maximum +10).",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { ShifterClassGuid },
                    Folder = "ShifterEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Acid, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "ShifterColdRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1be5",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Cold Resistance",
                    Description = "Gain +1 cold resistance (maximum +10).",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { ShifterClassGuid },
                    Folder = "ShifterEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Cold, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "ShifterElecRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1be6",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Electricity Resistance",
                    Description = "Gain +1 electricity resistance (maximum +10).",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { ShifterClassGuid },
                    Folder = "ShifterEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Electricity, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "ShifterFireRes", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1be7",
                    Divisor = 1, Ranks = 10,
                    DisplayName = "Fire Resistance",
                    Description = "Gain +1 fire resistance (maximum +10).",
                    Races = new[] { GnomeRaceGuid },
                    Classes = new[] { ShifterClassGuid },
                    Folder = "ShifterEnergyRes",
                    Components = f => f
                        .AddDamageResistanceEnergy(type: Kingmaker.Enums.Damage.DamageEnergyType.Fire, value: ContextValues.Constant(1)),
                },
                new()
                {
                    Key = "Fervor", FeatureGuid = "3a1b6cf1d0f34d5e9b7a2c8e4f6a1bb3",
                    Divisor = 2, Ranks = 20,
                    DisplayName = "Fervor Bonus (+1/2)", EffectGuid = FervorEffectGuid,
                    Description = "Add +1/2 point to the amount of damage healed or dealt by the warpriest's fervor ability.",
                    Races = new[] { DrowRaceGuid },
                    Classes = new[] { WarpriestClassGuid },
                },
            };

            var extras = new Dictionary<string, List<string>>();
            var globalExtras = new List<string>();
            var folderChildren = new Dictionary<string, List<string>>();
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
                    if (def.ExcludeArchetypes != null)
                    {
                        foreach (var archetypeGuid in def.ExcludeArchetypes)
                        {
                            conf = conf.AddPrerequisiteNoArchetype(archetypeGuid, def.Classes[0]);
                        }
                    }
                    if (def.RequireArchetype != null)
                    {
                        conf = conf.AddPrerequisiteArchetypeLevel(def.RequireArchetype, def.Classes[0], level: 1);
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
                            // Gate only, no tooltip line: the progress readout belongs on the
                            // outer entry (which carries PrerequisiteRankProgressDisplay), not
                            // on the choices nested inside it — the cycle counts picks since
                            // the last reward (x/Divisor-1), so printing it here contradicts
                            // the outer x/Divisor readout. Native HideInUI is display-only:
                            // TooltipTemplateLevelUp.AddPrerequisites filters on it, while
                            // BlueprintFeature.MeetsPrerequisites ignores it and still checks.
                            c.HideInUI = true;
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
                            c.HideInUI = true; // same as the progress feature above
                        });
                    if (def.RewardFeatures != null)
                    {
                        rewardConf = rewardConf.AddToAllFeatures(
                            def.RewardFeatures.Select(g => (Blueprint<BlueprintFeatureReference>)g).ToArray());
                    }
                    rewardConf.Configure();
                    AllModGuids.Add(def.RewardSelectionGuid);

                    var outerConf = FeatureSelectionConfigurator.New($"ZFCW{def.Key}Bonus", def.FeatureGuid)
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
                        .AddToAllFeatures(def.ProgressGuid, def.RewardSelectionGuid);
                    if (def.ExcludeArchetypes != null)
                    {
                        foreach (var archetypeGuid in def.ExcludeArchetypes)
                        {
                            outerConf = outerConf.AddPrerequisiteNoArchetype(archetypeGuid, def.Classes[0]);
                        }
                    }
                    if (def.RequireArchetype != null)
                    {
                        outerConf = outerConf.AddPrerequisiteArchetypeLevel(def.RequireArchetype, def.Classes[0], level: 1);
                    }
                    outerConf.Configure();
                }
                AllModGuids.Add(def.FeatureGuid);
                // Tooltip display derives straight from this same def — wrapper mode shows
                // progress on the nested ProgressGuid feature (the FeatureGuid is just the
                // outer selection wrapper), plain mode shows it on FeatureGuid directly. This
                // keeps BonusDisplays impossible to desync from def.Divisor (a hand-maintained
                // second copy previously went stale here: TeamworkFeat's divisor was corrected
                // from 6 to 4 for RAW but the old manual table still said 6).
                if (!def.SkipBonusDisplay)
                {
                    var displayKey = def.RewardSelectionGuid == null ? def.FeatureGuid : def.ProgressGuid;
                    BonusDisplays[BlueprintGuid.Parse(displayKey)] = (def.Divisor, def.DisplayKind);
                }
                if (def.EffectGuid != null)
                {
                    EffectGrants[BlueprintGuid.Parse(def.FeatureGuid)] = (def.Divisor, def.EffectGuid);
                }
                if (def.RewardSelectionGuid != null)
                {
                    RewardPickCounters[BlueprintGuid.Parse(def.RewardSelectionGuid)] = def.ProgressGuid;
                }

                // A foldered entry is not listed in the bonus selection itself — its folder is,
                // and the entry becomes one of the folder's children. The folder claims the
                // slot of its FIRST child so the bonus list keeps the order declared here.
                if (def.Folder != null)
                {
                    if (!folderChildren.TryGetValue(def.Folder, out var kids))
                    {
                        folderChildren[def.Folder] = kids = new List<string>();
                    }
                    kids.Add(def.FeatureGuid);
                    if (kids.Count > 1) continue;   // the slot is already taken by the first child

                    var folder = folders.FirstOrDefault(f => f.Key == def.Folder);
                    if (folder == null)
                    {
                        Main.Log($"ERROR: entry '{def.Key}' names folder '{def.Folder}', which does not exist.");
                        continue;
                    }
                    foreach (var cls in folder.Classes)
                    {
                        if (!extras.TryGetValue(cls, out var l)) extras[cls] = l = new List<string>();
                        l.Add(folder.FolderGuid);
                    }
                    continue;
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

            // Built after the loop, so every child blueprint already exists. The folder carries
            // no mechanics of its own and stays out of BonusDisplays: each child keeps its own
            // rank, components and tooltip readout exactly as it had them when listed directly.
            foreach (var folder in folders)
            {
                if (!folderChildren.TryGetValue(folder.Key, out var children) || children.Count == 0)
                {
                    Main.Log($"Bonus folder '{folder.Key}' has no entries — not created.");
                    continue;
                }
                FeatureSelectionConfigurator.New($"ZFCW{folder.Key}Folder", folder.FolderGuid)
                    .SetDisplayName(LocalizationTool.CreateString($"ZFCW.{folder.Key}.Name", folder.DisplayName, tagEncyclopediaEntries: false))
                    .SetDescription(LocalizationTool.CreateString($"ZFCW.{folder.Key}.Desc", folder.Description, tagEncyclopediaEntries: false))
                    .SetRanks(folder.Ranks)
                    .SetIsClassFeature(true)
                    .AddComponent<PrerequisiteRaceAny>(c => c.m_Races =
                        folder.Races.Select(g => BlueprintTool.GetRef<BlueprintRaceReference>(g)).ToArray())
                    .AddToAllFeatures(children.Select(g => (Blueprint<BlueprintFeatureReference>)g).ToArray())
                    .Configure();
                AllModGuids.Add(folder.FolderGuid);
            }

            // Retired blueprints from the two-entry (partial/full) design: kept
            // registered so existing test saves load, hidden and granted nowhere.
            foreach (var (retiredGuid, idx) in new[]
                     {
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0a", 1), // old Dodge partial
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0c", 2), // old NaturalAC partial
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b0e", 3), // old NecroCL partial
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b10", 4), // old EnchDC partial
                         ("3a1b6cf1d0f34d5e9b7a2c8e4f6a1b55", 5), // old blanket FavoredEnemyAtkDmg counter
                         // Divisor-1 bonus: its component moved onto the LayOnHandsSelf counter,
                         // so this twin is no longer granted. Old saves keep the fact as an inert
                         // hidden stub and get the same bonus from the counter instead.
                         (LayOnHandsSelfEffectGuid, 6),
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
                         (EldritchArcanaRewardGuid, VanillaEldritchMagusArcanaSel),
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

            BuildFavoredEnemyPickPool();
            // Every selection through which the class can end up holding a domain or school, not
            // just the standard one. An archetype that swaps the choice out for its own — the
            // separatist cleric, the Thassilonian wizard — hands the character domain features
            // that are separate blueprints with separate resources, so a missing selection here
            // is a silently absent option in the reward card rather than an error. The pool
            // dedupes by feature, so listing a selection whose options are the ordinary ones
            // costs nothing.
            BuildPowerUsePickPool(ClericPowerUseRewardGuid, ClericPowerUseSeed, "Cleric domain",
                new[]
                {
                    ClericDomainsSelectionGuid,
                    ClericSecondDomainsSelectionGuid,
                    ClericSeparatistSecondDomainsSelectionGuid,
                    ExtraDomainSelectionGuid,
                });
            BuildPowerUsePickPool(DruidPowerUseRewardGuid, DruidPowerUseSeed, "Druid domain",
                new[] { DruidDomainSelectionGuid });
            BuildPowerUsePickPool(WizardPowerUseRewardGuid, WizardPowerUseSeed, "Wizard school",
                new[]
                {
                    // WizardSchoolSelectionGuid is deliberately absent. Despite the name, that
                    // blueprint holds the OPPOSITION schools — its options are OppositionSchool*,
                    // carrying AddOppositionSchool — which grant no powers and never could. It
                    // contributed 0 of 8 for as long as it was listed.
                    WizardSpecialistSchoolSelectionGuid,
                    WizardThassilonianSchoolSelectionGuid,
                });
            BuildPatronSpellMap();

            GlobalBonusExtras = globalExtras;
            return extras;
        }

        // Fills the hobgoblin ranger reward selection with one feature per favored
        // enemy type: +1 attack and damage against exactly that enemy, selectable only
        // once you already have that favored enemy. Mirrors the original mod, which
        // derives the same list from the vanilla favored enemy selection instead of
        // enumerating creature types by hand — so third-party favored enemies are
        // covered automatically.
        // "Select one domain power granted at 1st level that is normally usable a number of times
        // per day equal to 3 + the caster's ability modifier; add +1/2 to its uses per day."
        //
        // Same shape as the favored enemy pick: one reward feature per domain (or school) the
        // character could have taken, each gated on actually having it, each raising that one
        // power's own resource. The list is DERIVED from the game rather than hard-coded — there
        // are dozens of domain resources once Separatist and Greater variants are counted, and a
        // hand-written list would silently miss domains added by other mods.
        //
        // The tabletop wording is the filter: a resource qualifies only if its maximum is
        // "3 + a stat modifier", which is exactly BaseValue == 3 with IncreasedByStat set. That
        // excludes the Greater powers (a different pool) and anything level-scaled.
        private static void BuildPowerUsePickPool(string rewardGuid, string seed, string label, string[] selectionGuids)
        {
            var picks = new List<Blueprint<BlueprintFeatureReference>>();
            var seen = new HashSet<BlueprintGuid>();
            var perSelection = new List<string>();
            foreach (var selGuid in selectionGuids)
            {
                var sel = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>(BlueprintGuid.Parse(selGuid));
                if (sel == null)
                {
                    perSelection.Add($"{selGuid}=NOT FOUND");
                    continue;
                }
                int before = picks.Count;
                foreach (var feature in EnumerateSelectableFeatures(sel, new HashSet<BlueprintGuid>(), 0))
                {
                    if (!seen.Add(feature.AssetGuid)) continue;
                    var resource = FindPowerResource(feature);
                    if (resource == null) continue;

                    var pickGuid = MergeIds(feature.AssetGuid.ToString(), seed);
                    FeatureConfigurator.New($"ZFCWPowerUse{feature.name}", pickGuid)
                        .SetDisplayName(LocalizationTool.CreateString($"ZFCW.PowerUse.{feature.name}.Name",
                            $"Bonus Uses ({feature.Name})", tagEncyclopediaEntries: false))
                        .SetDescription(LocalizationTool.CreateString($"ZFCW.PowerUse.{feature.name}.Desc",
                            "Add one use per day of this power.", tagEncyclopediaEntries: false))
                        .SetIcon(feature.Icon)
                        .SetIsClassFeature(true)
                        .SetRanks(10)
                        // Only offered for a domain or school the character actually took.
                        .AddPrerequisiteFeature(feature.AssetGuid.ToString())
                        .AddComponent<Kingmaker.UnitLogic.FactLogic.IncreaseResourceAmount>(c =>
                            c.m_Resource = resource.ToReference<BlueprintAbilityResourceReference>())
                        .Configure();
                    AllModGuids.Add(pickGuid);
                    AllModBlueprintGuids.Add(BlueprintGuid.Parse(pickGuid));
                    picks.Add(pickGuid);
                }
                perSelection.Add($"{sel.name}={picks.Count - before}");
            }

            FeatureSelectionConfigurator.For(rewardGuid)
                .AddToAllFeatures(picks.ToArray())
                .Configure();
            // Per selection, not just the total. A selection contributing 0 is how an archetype
            // that swaps the domain or school choice for its own goes missing — the card still
            // opens, it just never offers that character's actual domain. A total alone hides it.
            Main.Log($"{label} power-use pick pool: {picks.Count} powers [{string.Join(", ", perSelection)}].");

            // Not diagnostics but a real failure worth one line: an empty pool means the card
            // unfolds onto a selection with nothing in it, so the bonus can be taken once and
            // never again. Silence here would make that look like a content decision.
            if (picks.Count == 0)
            {
                Main.Log($"WARNING: {label} power-use pool is EMPTY; the reward card will offer nothing.");
            }
        }

        // Flattens a selection into the features a character can actually end up with. An option
        // may itself be a selection — which is why the wizard pool built ZERO entries while the
        // cleric and druid ones filled up: WizardSchoolSelection offers *kinds* of school, and
        // the schools themselves sit one level further in. Treating an option as a leaf therefore
        // found a wrapper with no resource anywhere in its own tree and moved on.
        private static IEnumerable<BlueprintFeature> EnumerateSelectableFeatures(
            BlueprintFeature feature, HashSet<BlueprintGuid> visited, int depth)
        {
            if (feature == null || depth > 3 || !visited.Add(feature.AssetGuid)) yield break;
            if (feature is BlueprintFeatureSelection selection && selection.m_AllFeatures != null)
            {
                foreach (var childRef in selection.m_AllFeatures)
                {
                    foreach (var nested in EnumerateSelectableFeatures(childRef?.Get(), visited, depth + 1))
                    {
                        yield return nested;
                    }
                }
                yield break;
            }
            yield return feature;
        }

        // Walks a domain or school for the pool its 1st-level power spends.
        //
        // Three different components can name that pool, which is why the first version of this
        // found 123 cleric domains and 16 druid domains but ZERO wizard schools: it only looked
        // for AbilityResourceLogic on an ability reached through AddFacts, and the schools do not
        // advertise themselves that way. AddAbilityResources is the most direct signal — a
        // feature saying "I grant this pool" — and activatable powers use their own logic
        // component rather than the ability one.
        //
        // The walk is recursive because the nesting depth is not uniform: some powers hang
        // directly off the domain, others sit a feature or two down. Depth is capped and visited
        // blueprints are remembered, so a cycle cannot run away.
        private static BlueprintAbilityResource FindPowerResource(BlueprintScriptableObject root)
        {
            return FindResource(root, new HashSet<BlueprintGuid>(), 0);
        }

        private static BlueprintAbilityResource FindResource(
            BlueprintScriptableObject bp, HashSet<BlueprintGuid> visited, int depth)
        {
            if (bp == null || depth > 4 || !visited.Add(bp.AssetGuid)) return null;

            foreach (var component in bp.ComponentsArray)
            {
                BlueprintAbilityResource resource = null;
                switch (component)
                {
                    case Kingmaker.Designers.Mechanics.Facts.AddAbilityResources addRes:
                        resource = addRes.m_Resource?.Get();
                        break;
                    case Kingmaker.UnitLogic.Abilities.Components.AbilityResourceLogic abilityLogic:
                        resource = abilityLogic.m_RequiredResource?.Get();
                        break;
                    case Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic activatable:
                        resource = activatable.m_RequiredResource?.Get();
                        break;
                }
                if (IsPerDayPowerResource(resource)) return resource;

                if (component is Kingmaker.UnitLogic.FactLogic.AddFacts addFacts && addFacts.m_Facts != null)
                {
                    foreach (var factRef in addFacts.m_Facts)
                    {
                        var found = FindResource(factRef?.Get(), visited, depth + 1);
                        if (found != null) return found;
                    }
                }
            }

            if (bp is BlueprintProgression progression && progression.LevelEntries != null)
            {
                foreach (var entry in progression.LevelEntries)
                {
                    if (entry?.Features == null) continue;
                    foreach (var f in entry.Features)
                    {
                        var found = FindResource(f, visited, depth + 1);
                        if (found != null) return found;
                    }
                }
            }
            return null;
        }

        // The tabletop wording is the filter: "normally usable a number of times per day equal to
        // 3 + the ability modifier". What matters is the SHAPE — a small fixed base plus a stat
        // modifier, with no level scaling — which is what separates a 1st-level domain or school
        // power from the Greater powers and from anything that grows with level.
        //
        // The base is 3 for an ordinary cleric, wizard or druid, and **2 for the separatist**,
        // whose domain powers run at one cleric level lower; Owlcat wrote that straight into the
        // resource (AirDomainBaseResourceSeparatist is 2 + Wisdom). Requiring exactly 3 therefore
        // rejected 73 of the separatist's 75 domains while accepting the two that happen to share
        // a resource with the standard line — which is how the archetype came to have a reward
        // card that never listed the domain it had actually taken.
        //
        // Two values rather than a range, deliberately: these are the two shapes observed in the
        // game's data, and the pool's diagnostic now prints the amount of any resource it rejects,
        // so a third shape shows up as a log line to be looked at rather than as silence.
        private static bool IsPerDayPowerResource(BlueprintAbilityResource resource)
        {
            if (resource == null) return false;
            var amount = resource.m_MaxAmount;
            if (!amount.IncreasedByStat) return false;
            if (amount.IncreasedByLevel || amount.IncreasedByLevelStartPlusDivStep) return false;
            return amount.BaseValue == 3 || amount.BaseValue == 2;
        }

        private static void BuildFavoredEnemyPickPool()
        {
            var sourceSel = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>(
                BlueprintGuid.Parse(VanillaFavoriteEnemySel));
            if (sourceSel?.m_AllFeatures == null)
            {
                Main.Log("Favored enemy selection missing — favored enemy pick pool left empty.");
                return;
            }

            var picks = new List<Blueprint<BlueprintFeatureReference>>();
            foreach (var enemyRef in sourceSel.m_AllFeatures)
            {
                var enemyFeature = enemyRef?.Get();
                var favored = enemyFeature?.GetComponent<Kingmaker.UnitLogic.FactLogic.FavoredEnemy>();
                if (favored?.m_CheckedFacts == null || favored.m_CheckedFacts.Length == 0) continue;

                var pickGuid = MergeIds(enemyFeature.AssetGuid.ToString(), FavoredEnemyPickSeed);
                var checkedFact = favored.m_CheckedFacts[0];
                var conf = FeatureConfigurator.New($"ZFCWFavoredEnemyPick{enemyFeature.name}", pickGuid)
                    .SetDisplayName(LocalizationTool.CreateString($"ZFCW.FEPick.{enemyFeature.name}.Name",
                        $"Favored Enemy Bonus ({enemyFeature.Name})", tagEncyclopediaEntries: false))
                    .SetDescription(LocalizationTool.CreateString($"ZFCW.FEPick.{enemyFeature.name}.Desc",
                        "Add +1 to this favored enemy bonus.", tagEncyclopediaEntries: false))
                    .SetIcon(enemyFeature.Icon)
                    .SetIsClassFeature(true)
                    // Only offered for a favored enemy the ranger actually has.
                    .AddPrerequisiteFeature(enemyFeature.AssetGuid.ToString())
                    .AddComponent<Kingmaker.Designers.Mechanics.Facts.AttackBonusAgainstFactOwner>(c =>
                    {
                        c.m_CheckedFact = checkedFact;
                        c.AttackBonus = 1;
                        c.Descriptor = Kingmaker.Enums.ModifierDescriptor.UntypedStackable;
                    })
                    .AddComponent<Kingmaker.Designers.Mechanics.Facts.DamageBonusAgainstFactOwner>(c =>
                    {
                        c.m_CheckedFact = checkedFact;
                        c.DamageBonus = 1;
                        c.Descriptor = Kingmaker.Enums.ModifierDescriptor.UntypedStackable;
                    });
                conf.Configure();
                AllModGuids.Add(pickGuid);
                AllModBlueprintGuids.Add(BlueprintGuid.Parse(pickGuid));
                picks.Add(pickGuid);
            }

            FeatureSelectionConfigurator.For(FavoredEnemyPickRewardGuid)
                .AddToAllFeatures(picks.ToArray())
                // Instant Enemy makes any target count as a favored enemy; the original
                // grants the same bonus against it, so a ranger's picks are not dead
                // weight when using it.
                .AddComponent<Kingmaker.Designers.Mechanics.Facts.AttackBonusAgainstFactOwner>(c =>
                {
                    c.m_CheckedFact = BlueprintTool.GetRef<BlueprintUnitFactReference>(InstantEnemyBuffGuid);
                    c.AttackBonus = 1;
                    c.Descriptor = Kingmaker.Enums.ModifierDescriptor.UntypedStackable;
                })
                .AddComponent<Kingmaker.Designers.Mechanics.Facts.DamageBonusAgainstFactOwner>(c =>
                {
                    c.m_CheckedFact = BlueprintTool.GetRef<BlueprintUnitFactReference>(InstantEnemyBuffGuid);
                    c.DamageBonus = 1;
                    c.Descriptor = Kingmaker.Enums.ModifierDescriptor.UntypedStackable;
                })
                .Configure();
            Main.Log($"Favored enemy pick pool: {picks.Count} enemy types.");
        }

        // Resolves a class's own currently-configured spell list guid (mirrors ZFC's
        // own "wizard.Spellbook.SpellList" access) rather than a separately-looked-up
        // named ref, so it stays correct even if some other mod ever swaps the base
        // class's list.
        private static string ClassSpellListGuid(string classGuid) =>
            BlueprintTool.Get<BlueprintCharacterClass>(classGuid)?.Spellbook?.SpellList?.AssetGuid.ToString();

        // Builds a custom spell list by filtering/combining already-loaded native
        // class lists — same technique real WOTR mods use (PrestigePlus's
        // GraveSpellList.cs, EbonsContentMod's FaithMagic.cs): read each source
        // list's SpellsByLevel, keep spells matching predicate, dedupe by guid, write
        // into a fresh list. Used for the race-specific "bonus known spell" variants
        // (Ganzi/Oracle, Goblin/Sorcerer, Shaman) that ZFC built via Kingmaker-only
        // CotW helpers we don't have access to.
        private static void BuildFilteredSpellList(string name, string guid, int maxLevel,
            Func<BlueprintAbility, bool> predicate, params BlueprintSpellList[] sources)
        {
            var levels = new SpellLevelList[maxLevel + 1];
            for (int i = 0; i <= maxLevel; i++) levels[i] = new SpellLevelList(i);
            var spellList = SpellListConfigurator.New(name, guid)
                .AddToSpellsByLevel(levels)
                .SetFilterByMaxLevel(maxLevel)
                .Configure();

            var seen = new HashSet<string>();
            foreach (var source in sources)
            {
                if (source?.SpellsByLevel == null) continue;
                for (int i = 0; i <= maxLevel && i < source.SpellsByLevel.Length; i++)
                {
                    foreach (var spell in source.SpellsByLevel[i].Spells)
                    {
                        if (spell == null || !predicate(spell)) continue;
                        if (!seen.Add(spell.AssetGuid.ToString())) continue;
                        spellList.SpellsByLevel[i].m_Spells.Add(spell.ToReference<BlueprintAbilityReference>());
                    }
                }
            }
            AllModGuids.Add(guid);
        }

        // Builds N parametrized "learn a bonus known spell of level i" features
        // (levels 1..maxLevel) — the wrapper reward selection's AllFeatures for a
        // "bonus known spell" entry. Gated so a level-i slot only unlocks once the
        // character can already cast level (i+1) spells, matching ZFC's "at least 1
        // level below the highest spell level you can cast." levelGuids[level-1] is
        // this entry's own fixed, independently-generated guid for that level (see the
        // guid-array fields above). extraPrereq lets the Wizard Thassilonian Specialist
        // tracks add archetype-specific gating on top.
        private static List<string> BuildKnownSpellRewardFeatures(
            string keyPrefix, string[] levelGuids, string classGuid, int maxLevel, string spellListGuid,
            string levelNoun = "Spell", Action<ParametrizedFeatureConfigurator> extraPrereq = null)
        {
            var guids = new List<string>();
            for (int level = 1; level <= maxLevel; level++)
            {
                var guid = levelGuids[level - 1];
                var levelName = LocalizationTool.CreateString($"ZFCW.{keyPrefix}L{level}.Name",
                    $"Learn a Bonus Level {level} {levelNoun}", tagEncyclopediaEntries: false);
                var levelDesc = LocalizationTool.CreateString($"ZFCW.{keyPrefix}L{level}.Desc",
                    $"Learn one additional level {level} {levelNoun.ToLowerInvariant()}. This {levelNoun.ToLowerInvariant()} must be at least 1 level below the highest level the character can cast.",
                    tagEncyclopediaEntries: false);
                var conf = ParametrizedFeatureConfigurator.New($"ZFCW{keyPrefix}L{level}", guid)
                    .SetDisplayName(levelName)
                    .SetDescription(levelDesc)
                    .SetIsClassFeature(true)
                    .SetHideNotAvailibleInUI(true)
                    .SetRanks(20)
                    .SetParameterType(FeatureParameterType.LearnSpell)
                    .SetSpellcasterClass(classGuid)
                    .SetSpellList(spellListGuid)
                    .SetSpecificSpellLevel(true)
                    .SetSpellLevel(level)
                    .SetSpellLevelPenalty(0)
                    .AddLearnSpellParametrized(specificSpellLevel: true, spellcasterClass: classGuid,
                        spellLevel: level, spellLevelPenalty: 0, spellList: spellListGuid)
                    .AddPrerequisiteClassSpellLevel(classGuid, requiredSpellLevel: level + 1);
                extraPrereq?.Invoke(conf);
                conf.Configure();
                AllModGuids.Add(guid);
                guids.Add(guid);
            }
            return guids;
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
