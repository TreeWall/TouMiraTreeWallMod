using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System.Linq;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TreeWallMod.Modifiers.Crewmate;
using TreeWallMod.Options.Roles.Crewmate;
using TreeWallMod.Roles.Crewmate;
using TreeWallMod.TWAssets;
using UnityEngine;

namespace TreeWallMod.Buttons.Crewmate
{
    public sealed class SyringeInjectButton : TownOfUsRoleButton<SyringeRole, PlayerControl>
    {
        public override string Name => "Inject";
        public override BaseKeybind Keybind => Keybinds.PrimaryAction;
        public override Color TextOutlineColor => TreeWallMod.Colors.Syringe;
        public override float Cooldown => OptionGroupSingleton<SyringeOptions>.Instance.InjectCd;
        public override float InitialCooldown => OptionGroupSingleton<SyringeOptions>.Instance.InjectCd;
        public override float EffectDuration => 5f;
        public override LoadableAsset<Sprite> Sprite => TWCrewAssets.SyringeInjectSprite;

        private PlayerControl? Victim;

        public override PlayerControl? GetTarget()
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
        }

        public override bool IsTargetValid(PlayerControl? target)
        {
            return (
                base.IsTargetValid(target) &&
                target != null &&
                !(target.TryGetModifier<SyringeInjectedModifier>(out var injected) && injected.SyringeItems.Any(x => x.Syringe == PlayerControl.LocalPlayer)));
        }

        protected override void OnClick()
        {
            Victim = Target;

            //if (Target == null)
            //{
            //    Error("Inject: Target is null");
            //    return;
            //}

            //var notif1 = Helpers.CreateAndShowNotification(
            //    $"Injected {Target.name}!", Color.white,
            //    new Vector3(0f, 1f, -20f), spr: TWCrewAssets.SyringeInjectSprite.LoadAsset());
            //notif1.AdjustNotification();

            //SyringeInjectedModifier.UpdateSyringe(Target, PlayerControl.LocalPlayer);
        }

        public override void OnEffectEnd()
        {
            if (Victim == null || Victim.HasDied() || !IsTargetValid(Victim) || MeetingHud.Instance)
            {
                SetTimer(5f);
                return;
            }

            var notif1 = Helpers.CreateAndShowNotification(
                $"Injected {Victim.name}!", Color.white,
                new Vector3(0f, 1f, -20f), spr: TWCrewAssets.SyringeInjectSprite.LoadAsset());
            notif1.AdjustNotification();

            SyringeInjectedModifier.UpdateSyringe(Victim, PlayerControl.LocalPlayer);
        }
    }
}
