using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TreeWallMod.Roles.Crewmate;

namespace TreeWallMod.Options.Roles.Crewmate
{
    public sealed class RunnerOptions : AbstractOptionGroup<RunnerRole>
    {
        public override string GroupName => "Runner";

        [ModdedNumberOption("Caffeine Cooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
        public float CaffeineCooldown { get; set; } = 20f;

        public ModdedNumberOption CaffeineDuration { get; } = new ModdedNumberOption("Caffeine Duration", 7f, 5f, 25f, 1f, MiraNumberSuffixes.Seconds);

        [ModdedNumberOption("Caffeine Speed Multiplier", 1f, 5f, 0.2f, MiraNumberSuffixes.Multiplier)]
        public float SpeedMultiplier { get; set; } = 2f;

        [ModdedToggleOption("Stack Caffeine")]
        public bool CaffeineStack { get; set; } = false;

        public ModdedNumberOption SpeedLimit { get; } = new ModdedNumberOption("Caffeine Speed Limit", 20f, 0f, 100f, 5f, MiraNumberSuffixes.Multiplier, zeroInfinity: true)
        {
            Visible = () => OptionGroupSingleton<RunnerOptions>.Instance.CaffeineStack
        };

        [ModdedNumberOption("Starting Uses", 0, 16, 2, MiraNumberSuffixes.None, zeroInfinity: true)]
        public float CaffeineUses { get; set; } = 4;

        public ModdedToggleOption TaskUses { get; } = new("Tasks increase uses", true)
        {
            Visible = () => OptionGroupSingleton<RunnerOptions>.Instance.CaffeineUses != 0
        };

        [ModdedToggleOption("Has to be Caffeinated to be saved from getting killed")]
        public bool CaffeineSave { get; set; } = true;

        [ModdedToggleOption("Comms disable immunity")]
        public bool CommsEnabled { get; set; } = true;

        [ModdedToggleOption("Comms disable speed")]
        public bool CommsSpeed { get; set; } = false;

    }
}