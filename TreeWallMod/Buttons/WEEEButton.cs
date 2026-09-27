using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Buttons;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Options.Modifiers.Assailant;
using TreeWallMod.Modifiers.GameModifiers;
using TreeWallMod.Options.Modifiers;
using UnityEngine;

namespace TreeWallMod.Buttons
{
    public sealed class WEEEButton : TownOfUsButton
    {
        public override string Name => "Strech";
        public override float Cooldown => Math.Clamp(OptionGroupSingleton<WEEEModifierOptions>.Instance.Cooldown.Value + MapCooldown, 2.5f, 120f);
        public override LoadableAsset<Sprite> Sprite => TouAssets.OverclockSprite; // placeholder
        public override float EffectDuration => OptionGroupSingleton<WEEEModifierOptions>.Instance.Duration.Value;
        public override ButtonLocation Location => ButtonLocation.BottomLeft;

        public override bool Enabled(RoleBehaviour? role)
        {
            return PlayerControl.LocalPlayer &&
                   PlayerControl.LocalPlayer.HasModifier<WEEEModifier>() &&
                   !PlayerControl.LocalPlayer.Data.IsDead;
        }

        protected override void OnClick()
        {
            var opts = OptionGroupSingleton<WEEEModifierOptions>.Instance;

            WEEEModifier.RpcWEEEStrech(PlayerControl.LocalPlayer, opts.StrechType, opts.Duration.Value, (int)opts.Scale, opts.Cycles, opts.Rotate, opts.RotCycles);
        }
    }
}
