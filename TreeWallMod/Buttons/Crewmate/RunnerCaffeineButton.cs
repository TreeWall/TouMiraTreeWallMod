using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using System;
using TownOfUs.Buttons;
using TreeWallMod.Options.Roles.Crewmate;
using TreeWallMod.Roles.Crewmate;
using TreeWallMod.TWAssets;
using UnityEngine;

namespace TreeWallMod.Buttons.Crewmate
{
    public sealed class RunnerCaffeineButton : TownOfUsRoleButton<RunnerRole>
    {
        public override string Name => "Caffeine";
        public override BaseKeybind Keybind => Keybinds.PrimaryAction;
        public override Color TextOutlineColor => Colors.Runner;
        public override float Cooldown => Math.Clamp(OptionGroupSingleton<RunnerOptions>.Instance.CaffeineCooldown + MapCooldown, 5f, 120f);
        public override float EffectDuration => OptionGroupSingleton<RunnerOptions>.Instance.CaffeineDuration.Value;
        public override int MaxUses => (int)OptionGroupSingleton<RunnerOptions>.Instance.CaffeineUses;
        public override LoadableAsset<Sprite> Sprite => TWCrewAssets.RunnerCaffeineSprite;
        public override bool ZeroIsInfinite { get; set; } = true;
        public bool CanStillUse = true;

        protected override void OnClick()
        {
            var runner = PlayerControl.LocalPlayer.GetRole<RunnerRole>()!;

            float speed = 0;

            if (OptionGroupSingleton<RunnerOptions>.Instance.CaffeineStack &&
                runner.SpeedMultiplier + OptionGroupSingleton<RunnerOptions>.Instance.SpeedMultiplier - 1 > OptionGroupSingleton<RunnerOptions>.Instance.SpeedLimit.Value &&
                OptionGroupSingleton<RunnerOptions>.Instance.SpeedLimit.Value != 0)
            {
                speed = runner.SpeedMultiplier;
            }
            else
            {
                speed = (runner.SpeedMultiplier == 1 ? 0 : runner.SpeedMultiplier) + OptionGroupSingleton<RunnerOptions>.Instance.SpeedMultiplier;
            }

            RunnerRole.RpcSetRunnerSpeed(runner.Player, speed, true);

            Message($"Current Multiplier: {runner.SpeedMultiplier}, Active: {runner.SpeedActive}");
        }

        public override void OnEffectEnd()
        {
            var runner = PlayerControl.LocalPlayer.GetRole<RunnerRole>()!;

            float speed = OptionGroupSingleton<RunnerOptions>.Instance.CaffeineStack ? runner.SpeedMultiplier : 1f;

            RunnerRole.RpcSetRunnerSpeed(runner.Player, speed, false);
        }

    }
}