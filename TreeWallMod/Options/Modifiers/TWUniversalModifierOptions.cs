using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using System;
using TreeWallMod.Modifiers.GameModifiers;
using UnityEngine;

namespace TreeWallMod.Options.Modifiers
{
    public sealed class TWUniversalModifierOptions : AbstractOptionGroup
    {
        public override string GroupName => "Headless Modifiers";
        public override MenuCategory ParentMenu => MenuCategory.Modifiers;
        public override Color GroupColor => Color.gray;

        public ModdedNumberOption HeadlessAmount { get; } =
            new("Headless Amount", 0, 0, 15, 1, MiraNumberSuffixes.None);

        public ModdedNumberOption HeadlessChance { get; } =
            new("Headless Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
            {
                Visible = () => OptionGroupSingleton<TWUniversalModifierOptions>.Instance.HeadlessAmount > 0
            };

        public ModdedToggleOption HeadlessKillButton { get; } =
            new("Headless killer gets Kill button", true)
            {
                Visible = () => OptionGroupSingleton<TWUniversalModifierOptions>.Instance.HeadlessAmount > 0
            };

        public ModdedNumberOption WEEEAmount { get; } =
            new("WEEE Amount", 0, 0, 15, 1, MiraNumberSuffixes.None);

        public ModdedNumberOption WEEEChance { get; } =
            new("WEEE Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
            {
                Visible = () => OptionGroupSingleton<TWUniversalModifierOptions>.Instance.WEEEAmount > 0
            };
    }
}
