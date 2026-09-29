using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TreeWallMod.Roles.Neutral;

namespace TreeWallMod.Options.Roles.Neutral
{
    public sealed class MarksmanOptions : AbstractOptionGroup<MarksmanRole>
    {
        public override string GroupName => "Marksman";

        [ModdedNumberOption("Kill Cooldown", 10f, 60f, 10f, MiraNumberSuffixes.Seconds)]
        public float KillCd { get; set; } = 15f;

        [ModdedNumberOption("Discover Cooldown", 5f, 30f, 5f, MiraNumberSuffixes.Seconds)]
        public float DiscoverCd { get; set; } = 15f;

        public ModdedNumberOption InitialDiscoverUses { get; } = new("Starting Discover Uses", 0, 0, 15, 1, MiraNumberSuffixes.None, zeroInfinity: true);

        public ModdedNumberOption NewDiscoverKillsRequired { get; } = new("Kills/Guesses Required for new discover", 2, 1, 3, 1, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<MarksmanOptions>.Instance.InitialDiscoverUses != 0
        };

        public ModdedToggleOption CanVent { get; } = new("Can Vent", true);

        public ModdedEnumOption<MarksmanRoleHintEnum> RoleHint { get; } = new("Role hint", MarksmanRoleHintEnum.Subalignment, ["Subalignment", "List Roles", "Doomsayer Hint"]);

        public ModdedNumberOption RoleAmount { get; } = new("Amount of Roles revealed in Role Hint", 7, 2, 7, 1, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<MarksmanOptions>.Instance.RoleHint == MarksmanRoleHintEnum.ListRoles
        };

        public ModdedToggleOption CanDiscoverTwice { get; } = new("Players can be Discovered more than once", false)
        {
            Visible = () => OptionGroupSingleton<MarksmanOptions>.Instance.RoleHint == MarksmanRoleHintEnum.ListRoles
        };

        [ModdedToggleOption("Gets Misguess")]
        public bool MisguessAvailable { get; set; } = true;

        [ModdedNumberOption("Abilities obtained from each guess", 1, 7, 1, MiraNumberSuffixes.None)]
        public float AbilitiesEachGuess { get; set; } = 1;

        [ModdedNumberOption("Ability Cap", 1, 7, 1, MiraNumberSuffixes.None)]
        public float AbilityCap { get; set; } = 7;

        [ModdedNumberOption("Sharpened Blade Reduces Kill cooldown by", 5, 10, 0.5f, MiraNumberSuffixes.Seconds)]
        public float SharpenedBladeKillCdReduction { get; set; } = 7;

        [ModdedNumberOption("Smokebomb uses per round", 0, 5, 1, MiraNumberSuffixes.None)]
        public float SmokebombUses { get; set; } = 2;

        [ModdedNumberOption("Smokebomb Cooldown", 15f, 30f, 5f, MiraNumberSuffixes.Seconds)]
        public float SmokebombCd { get; set; } = 20f;

        [ModdedNumberOption("Smokebomb Duration", 5f, 20f, 1f, MiraNumberSuffixes.Seconds)]
        public float SmokebombDuration { get; set; } = 10f;

        [ModdedNumberOption("Smokebomb Radius", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier)]
        public float SmokebombRadius { get; set; } = 1f;

        [ModdedToggleOption("Smokebomb Moves with Marksman")]
        public bool SmokebombMove { get; set; } = false;

        [ModdedNumberOption("Warp mark cooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
        public float WarpMarkCd { get; set; } = 25f;

        [ModdedNumberOption("Warp Cooldown after Marking", 5f, 20f, 2.5f, MiraNumberSuffixes.Seconds)]
        public float WarpCd { get; set; } = 15f;

        [ModdedNumberOption("Vanish Duration", 10f, 90f, 10f, MiraNumberSuffixes.Seconds)]
        public float VanishDuration { get; set; } = 20f;

        [ModdedNumberOption("Vanish Cooldown", 15f, 75f, 5f, MiraNumberSuffixes.Seconds)]
        public float VanishCooldown { get; set; } = 35f;

        public ModdedNumberOption InitialVanishUses { get; } = new("Starting Vanish Uses", 0, 0, 15, 1, MiraNumberSuffixes.None, zeroInfinity: true);

        public ModdedNumberOption NewVanishKillsRequired { get; } = new("Kills/Guesses Required for new Vanish use", 2, 1, 3, 1, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<MarksmanOptions>.Instance.InitialVanishUses != 0
        };

        [ModdedNumberOption("Dismantle Cooldown", 15f, 60f, 10f, MiraNumberSuffixes.Seconds)]
        public float DismantleCd { get; set; } = 25f;

        [ModdedNumberOption("Dismantle Delay", 1f, 10f, 1f, MiraNumberSuffixes.Seconds)]
        public float DismantleDelay { get; set; } = 3f;
    }

    public enum MarksmanRoleHintEnum
    {
        Subalignment,
        ListRoles,
        DoomHint
    }
}
