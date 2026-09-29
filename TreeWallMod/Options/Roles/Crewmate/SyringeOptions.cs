using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TreeWallMod.Roles.Crewmate;

namespace TreeWallMod.Options.Roles.Crewmate
{
    public sealed class SyringeOptions : AbstractOptionGroup<SyringeRole>
    {
        public override string GroupName => "Syringe";

        [ModdedNumberOption("Inject cooldown", 45f, 120f, 5f, MiraNumberSuffixes.Seconds)]
        public float InjectCd { get; set; } = 60f;

        [ModdedToggleOption("Crew turned Traitor becomes assasin")]
        public bool InjectedCrewmateBecomesAssasin { get; set; } = false;

        public ModdedNumberOption CrewmateChangeTraitor { get; set; } = new ModdedNumberOption(
            "Chance for Injected Crewmate to become Traitor",
            25f, 0f, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            ChangedEvent = newA =>
            {
                if (newA + OptionGroupSingleton<SyringeOptions>.Instance.CrewmateDeath.Value > 100f)
                {
                    OptionGroupSingleton<SyringeOptions>.Instance.CrewmateDeath.SetValue(100 - newA);
                }
            }
        };

        public ModdedNumberOption CrewmateDeath { get; set; } = new ModdedNumberOption(
            "Chance for Injected Crewmate to DIE",
            25f, 0f, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            ChangedEvent = newB =>
            {
                if (newB + OptionGroupSingleton<SyringeOptions>.Instance.CrewmateChangeTraitor.Value > 100f)
                {
                    OptionGroupSingleton<SyringeOptions>.Instance.CrewmateChangeTraitor.SetValue(100 - newB);
                }
            }
        };

        public ModdedNumberOption MinCrewForTraitor { get; set; } = new ModdedNumberOption(
            "Minimum Crewmate required for injected Crewmate to be turned to Traitor",
            4, 0, 15, 1, MiraNumberSuffixes.None);

        [ModdedEnumOption("After syringe injected crew fails to turn Traitor", typeof(SyringeTraitorFail), ["Nothing Happens", "Both Syringe And Target Die"])]
        public SyringeTraitorFail syringeTraitorFail { get; set; } = SyringeTraitorFail.BothSyringeAndTargetDie;
    }

    public enum SyringeTraitorFail
    {
        NothingHappens,
        BothSyringeAndTargetDie
    }
}
