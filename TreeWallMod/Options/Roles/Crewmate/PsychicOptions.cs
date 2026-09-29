using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TreeWallMod.Roles.Crewmate;

namespace TreeWallMod.Options.Roles.Crewmate
{
    public sealed class PsychicOptions : AbstractOptionGroup<PsychicRole>
    {
        public override string GroupName => "Psychic";

        [ModdedNumberOption("Psychic killer guess cd", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
        public float PsychicGuessCd { get; set; } = 30f;

        [ModdedToggleOption("Wrong guess kills pyschic")]
        public bool WrongGuessToggle { get; set; } = true;

    }
}
