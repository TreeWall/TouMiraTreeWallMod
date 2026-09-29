using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System.Linq;
using TownOfUs.Buttons;
using TreeWallMod.Modifiers.Neutral;
using TreeWallMod.Options.Roles.Neutral;
using TreeWallMod.Roles.Neutral;
using TreeWallMod.TWAssets;
using UnityEngine;

namespace TreeWallMod.Buttons.Neutral.Marksman
{
    public sealed class MarksmanDismantleButton : TownOfUsRoleButton<MarksmanRole, PlayerControl>
    {
        public override string Name => "Dismantle";
        public override Color TextOutlineColor => Colors.Marksman;
        public override float Cooldown => Mathf.Clamp(OptionGroupSingleton<MarksmanOptions>.Instance.DismantleCd + MapCooldown, 5f, 120f);
        public override float EffectDuration => OptionGroupSingleton<MarksmanOptions>.Instance.DismantleDelay;
        public override int MaxUses => 1;
        public override ButtonLocation Location => ButtonLocation.BottomLeft;
        public override LoadableAsset<Sprite> Sprite => TWNeutAssets.MarksmanDismantleSprite;
        public override bool ShouldPauseInVent => false;
        public override bool UsableFirstRound => false;

        public override bool UsableInDeath => false;
        PlayerControl? finalTarget = null;

        public override PlayerControl? GetTarget()
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
        }

        public override bool Enabled(RoleBehaviour? role)
        {
            var marksman = PlayerControl.LocalPlayer.GetRole<MarksmanRole>()!;
            var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied());

            return base.Enabled(role) && marksman.UnlockedAbilities.Contains(MarksmanAbility.Dismantle) && alivePlayers > 5;
        }

        protected override void OnClick()
        {
            finalTarget = Target;
        }

        public override void OnEffectEnd()
        {
            if (finalTarget == null || finalTarget.HasDied())
            {
                Error("Marksman Dismantle: Target is null or 'Died'");
                return;
            }

            var notif1 = Helpers.CreateAndShowNotification(
                $"Dismantled {finalTarget.name}",
                Color.white, new Vector3(0f, 1f, -20f), spr: TWRoleIcons.Marksman.LoadAsset());
            notif1.AdjustNotification();

            finalTarget.RpcAddModifier<MarksmanDismantledModifier>(PlayerControl.LocalPlayer);
        }
    }
}
