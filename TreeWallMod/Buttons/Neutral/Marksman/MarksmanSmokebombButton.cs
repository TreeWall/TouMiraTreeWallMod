using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System.Collections.Generic;
using System.Linq;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TreeWallMod.Assets;
using TreeWallMod.Modifiers.Neutral;
using TreeWallMod.Options.Roles.Neutral;
using TreeWallMod.Roles.Neutral;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace TreeWallMod.Buttons.Neutral.Marksman
{
    public sealed class MarksmanSmokebombButton : TownOfUsRoleButton<MarksmanRole>
    {
        public override string Name => "Smokebomb";
        public override Color TextOutlineColor => Colors.Marksman;
        public override float Cooldown => Mathf.Clamp(OptionGroupSingleton<MarksmanOptions>.Instance.SmokebombCd + MapCooldown, 5f, 120f);
        public override float EffectDuration => OptionGroupSingleton<MarksmanOptions>.Instance.SmokebombDuration;
        public override int MaxUses => (int)OptionGroupSingleton<MarksmanOptions>.Instance.SmokebombUses;
        public override ButtonLocation Location => ButtonLocation.BottomLeft;
        public override LoadableAsset<Sprite> Sprite => NeutAssets.MarksmanSmokeBomb;
        public override bool ShouldPauseInVent => true;
        public override bool ZeroIsInfinite { get; set; } = true;

        public override bool UsableInDeath => false;

        public override bool Enabled(RoleBehaviour? role)
        {
            var marksman = PlayerControl.LocalPlayer.GetRole<MarksmanRole>()!;

            return base.Enabled(role) && marksman.UnlockedAbilities.Contains(MarksmanAbility.SmokeBomb);
        }

        protected override void OnClick()
        {
            if (OptionGroupSingleton<MarksmanOptions>.Instance.SmokebombMove)
            {
                var allPlayers = PlayerControl.AllPlayerControls.ToArray().ToList();

                foreach (var player in allPlayers)
                {
                    player.RpcAddModifier<MarksmanMovingSmokeModifier>(PlayerControl.LocalPlayer);
                }
            }
            else
            {
                var smokedPlayers = Helpers.GetClosestPlayers(PlayerControl.LocalPlayer, OptionGroupSingleton<MarksmanOptions>.Instance.SmokebombRadius * ShipStatus.Instance.MaxLightRadius);

                foreach (var player in smokedPlayers)
                {
                    player.RpcAddModifier<MarksmanSmokedModifier>(PlayerControl.LocalPlayer);
                }

                PlayerControl.LocalPlayer.RpcAddModifier<MarksmanSmokedModifier>(PlayerControl.LocalPlayer);
            }

            var notif1 = Helpers.CreateAndShowNotification(
                $"Smokebomb Activated",
                Color.white, new Vector3(0f, 1f, -150f),
                spr: RoleIcons.Marksman.LoadAsset());

            notif1.AdjustNotification();
        }
    }
}
