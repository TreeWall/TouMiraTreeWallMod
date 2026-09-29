using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using System;
using TownOfUs.Buttons;
using TreeWallMod.TWAssets;
using TreeWallMod.Options.Roles.Neutral;
using TreeWallMod.Roles.Neutral;
using UnityEngine;

namespace TreeWallMod.Buttons.Neutral.Marksman
{
	public sealed class MarksmanMarkWarpButton : TownOfUsRoleButton<MarksmanRole, PlayerControl>
	{
        public override string Name => "Mark";
        public override Color TextOutlineColor => Colors.Marksman;
		public override float Cooldown => Math.Clamp(OptionGroupSingleton<MarksmanOptions>.Instance.WarpMarkCd + MapCooldown, 5f, 120f);
		public override ButtonLocation Location => ButtonLocation.BottomRight;
		public override LoadableAsset<Sprite> Sprite => TWNeutAssets.MarksmanWarpSprite;
        public override bool ShouldPauseInVent => false;

        public override PlayerControl? GetTarget()
		{
			return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
		}

		public override bool IsTargetValid(PlayerControl? target)
		{
			return (base.IsTargetValid(target) && target != null);
		}

        public override bool Enabled(RoleBehaviour? role)
        {
			var marksman = PlayerControl.LocalPlayer.GetRole<MarksmanRole>()!;

            return base.Enabled(role) && marksman.WarpMarking == MarksmanWarpState.Marking && marksman.UnlockedAbilities.Contains(MarksmanAbility.Warp);
        }

		protected override void OnClick()
		{
            var marksman = PlayerControl.LocalPlayer.GetRole<MarksmanRole>()!;

			marksman.WarpMarking = MarksmanWarpState.Warp;

			CustomButtonSingleton<MarksmanWarpButton>.Instance.Timer = CustomButtonSingleton<MarksmanWarpButton>.Instance.Cooldown;

			marksman.WarpMarkedPlayer = Target!;
        }
	}
}
