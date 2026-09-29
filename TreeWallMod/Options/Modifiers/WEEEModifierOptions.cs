using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using UnityEngine;

namespace TreeWallMod.Options.Modifiers
{
    public sealed class WEEEModifierOptions : AbstractOptionGroup
    {
        public override string GroupName => "WEEE modifier";
        public override MenuCategory ParentMenu => MenuCategory.Modifiers;
        public override Color GroupColor => Colors.WEEEModifer;

        public ModdedEnumOption<WEEEStrechEnum> StrechType { get; } =
            new("Strech Type", WEEEStrechEnum.Random,
                [
                    "Random",
                    "Horizontally",
                    "Vertically",
                    "Both",
                    "Squish",
                ]);

        public ModdedNumberOption Cooldown { get; } = new("Cooldown", 20f, 5f, 60f, 5f, MiraAPI.Utilities.MiraNumberSuffixes.Seconds);

        public ModdedNumberOption Duration { get; } = new("Duration", 10f, 2.5f, 20f, 1f, MiraAPI.Utilities.MiraNumberSuffixes.Seconds);

        public ModdedNumberOption Scale { get; } = new("Scale", 3f, 2f, 22f, 5f, MiraAPI.Utilities.MiraNumberSuffixes.Multiplier);

        public ModdedNumberOption Cycles { get; } = new("Scale Cycles", 5, 1, 50, 10, MiraAPI.Utilities.MiraNumberSuffixes.None);

        public ModdedToggleOption Rotate { get; } = new("Rotate", false);

        public ModdedNumberOption RotCycles { get; } = new("Rotation Cycles", 2, 1, 50, 10, MiraAPI.Utilities.MiraNumberSuffixes.None);
    }

    public enum WEEEStrechEnum
    {
        Random = 0,
        inX = 1,
        inY = 2,
        Both = 3,
        Squish = 4
    }
}
