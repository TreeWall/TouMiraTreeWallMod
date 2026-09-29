using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using System;
using TownOfUs.Buttons;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Alliance;
using TreeWallMod.Modifiers.GameModifers;
using UnityEngine;

namespace TreeWallMod.Buttons
{
    // role does not matter, only needs to have the headless modifier and dead
    public sealed class HeadlessKillButton : TownOfUsKillRoleButton<CrewmateRole, PlayerControl>, IDiseaseableButton, IKillButton
    {
        public override string Name => "Kill";
        public override BaseKeybind Keybind => Keybinds.PrimaryAction;
        public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
        public override float Cooldown => Math.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + MapCooldown, 0.5f, 120f);

        public override bool UsableInDeath => true;

        public void SetDiseasedTimer(float multiplier)
        {
            SetTimer(Cooldown * multiplier);
        }

        public override bool CanUse()
        {

            if (TimeLordRewindSystem.IsRewinding)
            {
                return false;
            }

            if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
            {
                return false;
            }

            if (!PlayerControl.LocalPlayer.CanMove)
            {
                return false;
            }

            var newTarget = GetTarget();
            if (newTarget != Target)
            {
                SetOutline(false);
            }

            Target = IsTargetValid(newTarget) ? newTarget : null;
            SetOutline(true);

            return PlayerControl.LocalPlayer.moveable && Target != null && !EffectActive;
        }

        public override PlayerControl? GetTarget()
        {
            if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
            {
                return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
            }

            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
        }

        public override bool Enabled(RoleBehaviour? role)
        {
            if (PlayerControl.LocalPlayer.TryGetModifier<HeadlessModifier>(out var headless) && headless.Dead && headless.KillButton)
            {
                return true;
            }

            return false;
        }

        protected override void OnClick()
        {
            if (Target == null)
            {
                Error("Headless Behead: Target is null");
                return;
            }

            PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
        }
    }
}
