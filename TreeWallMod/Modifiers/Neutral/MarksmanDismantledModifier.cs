using MiraAPI.Utilities;
using System;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using UnityEngine;

namespace TreeWallMod.Modifiers.Neutral
{
    public sealed class MarksmanDismantledModifier(PlayerControl marksmanId) : DisabledModifier, IDisposable
    {
        public override string ModifierName => "Dismantled";
        public override bool CanUseAbilities => false;
        public override bool CanReport => false;
        public override bool HideOnUi => true;
        public override bool AutoStart => true;
        public override bool CanBeInteractedWith => true;
        public override bool IsConsideredAlive => true;
        public PlayerControl MarksmanId { get; } = marksmanId;
        public override float Duration => 1000000f;

        private ScreenFlash? flash;

        public void Dispose()
        {
            flash?.Dispose();
        }

        public override void OnActivate()
        {
            base.OnActivate();

            flash = new ScreenFlash();

            if (Player.AmOwner)
            {
                Player.moveable = false;
                Player.NetTransform.Halt();

                flash.SetColour(new Color(0f, 0f, 0f, 1f));
                flash.SetActive(true);

                var notif1 = Helpers.CreateAndShowNotification(
                    $"A Marksman Dismantled you, just sit for a round and hope no one kills you",
                    Color.white,
                    spr: TWAssets.TWRoleIcons.Marksman.LoadAsset());

                notif1.AdjustNotification();
                notif1.transform.localPosition = new Vector3(0f, 1f, -150f);
            }
        }

        public override void OnDeath(DeathReason reason)
        {
            ModifierComponent!.RemoveModifier(this);
        }

        public override void OnMeetingStart()
        {
            ModifierComponent!.RemoveModifier(this);
        }

        public override void OnDeactivate()
        {
            if (Player.AmOwner)
            {
                Player.moveable = true;

                flash?.SetActive(false);
                flash?.Destroy();
            }
        }
    }
}