using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using System;
using TownOfUs.Buttons;
using TownOfUs.Options.Modifiers.Alliance;
using TreeWallMod.Modifiers.Neutral;
using TreeWallMod.Options.Roles.Neutral;
using TreeWallMod.Roles.Neutral;
using UnityEngine;

namespace TreeWallMod.Buttons.Neutral.Marksman
{
    public sealed class MarksmanKillButton : TownOfUsKillRoleButton<MarksmanRole, PlayerControl>, IDiseaseableButton, IKillButton
    {
        public override string Name => "Kill";
        public override BaseKeybind Keybind => Keybinds.PrimaryAction;
        public override Color TextOutlineColor => Colors.Marksman;
        public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;

        public override float Cooldown
        {
            get
            {
                var player = PlayerControl.LocalPlayer;
                var role = player.GetRole<MarksmanRole>();

                if (role == null)
                {
                    return Math.Clamp(OptionGroupSingleton<MarksmanOptions>.Instance.KillCd + MapCooldown, 0.5f, 120f);
                }

                return Math.Clamp(
                    OptionGroupSingleton<MarksmanOptions>.Instance.KillCd
                    + MapCooldown
                    - (role.UnlockedAbilities.Contains(MarksmanAbility.SharpenedBlade) ? OptionGroupSingleton<MarksmanOptions>.Instance.SharpenedBladeKillCdReduction : 0), 0.5f, 120f);
            }
        }

        public void SetDiseasedTimer(float multiplier)
        {
            SetTimer(Cooldown * multiplier);
        }

        public override bool CanUse()
        {
            return (base.CanUse() && !PlayerControl.LocalPlayer.HasModifier<MarksmanVanishModifier>());
        }

        protected override void OnClick()
        {
            if (Target == null)
            {
                Error("Marksman Shoot: Target is null");
                return;
            }

            PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
        }

        public override PlayerControl? GetTarget()
        {
            if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
            {
                return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
            }

            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
        }
    }
}
