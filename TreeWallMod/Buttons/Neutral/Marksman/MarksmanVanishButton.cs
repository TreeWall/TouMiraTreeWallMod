using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using System.Linq;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TreeWallMod.Modifiers.Neutral;
using TreeWallMod.Options.Roles.Neutral;
using TreeWallMod.Roles.Neutral;
using UnityEngine;

namespace TreeWallMod.Buttons.Neutral.Marksman
{
    public sealed class MarksmanVanishButton : TownOfUsRoleButton<MarksmanRole>, ILegacyCapable
    {
        public override string Name => "Vanish";
        public override Color TextOutlineColor => Colors.Marksman;
        public override float Cooldown => Mathf.Clamp(OptionGroupSingleton<MarksmanOptions>.Instance.VanishCooldown + MapCooldown, 5f, 120f);
        public override float EffectDuration => OptionGroupSingleton<MarksmanOptions>.Instance.VanishDuration;
        public override int MaxUses => (int)OptionGroupSingleton<MarksmanOptions>.Instance.InitialVanishUses;
        public override ButtonLocation Location => ButtonLocation.BottomRight;
        public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyImpAssets.SwoopSprite : TouImpAssets.SwoopSprite;
        public override bool ShouldPauseInVent => false;

        public override bool ZeroIsInfinite { get; set; } = true;

        public bool Vanish { get; set; } = true;

        int Kills = 0;
        public void KilledPlayer()
        {
            var marksman = PlayerControl.LocalPlayer.GetRole<MarksmanRole>();

            if (marksman == null || marksman.LockedAbilities.Contains(MarksmanAbility.Vanish))
            {
                return;
            }

            Kills += 1;
            Kills %= OptionGroupSingleton<MarksmanOptions>.Instance.NewVanishKillsRequired;

            //Message($"Killed someone");
            if (Kills == 0 && OptionGroupSingleton<MarksmanOptions>.Instance.InitialVanishUses != 0)
            {
                ++UsesLeft;
                SetUses(UsesLeft);
                //Message($"Added a use as killed");
            }
        }

        public override bool Enabled(RoleBehaviour? role)
        {
            var marksman = PlayerControl.LocalPlayer.GetRole<MarksmanRole>()!;

            return base.Enabled(role) && (marksman.UnlockedAbilities.Contains(MarksmanAbility.Vanish));
        }

        public override void ClickHandler()
        {
            if (!CanUse())
            {
                return;
            }

            OnClick();
            Button?.SetDisabled();
            if (EffectActive)
            {
                Timer = Cooldown;
                EffectActive = false;
            }
            else if (HasEffect)
            {
                EffectActive = true;
                Timer = EffectDuration;
            }
            else
            {
                Timer = Cooldown;
            }
        }

        public override bool CanUse()
        {
            if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance || PlayerControl.LocalPlayer.inVent)
            {
                return false;
            }

            if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
            {
                return false;
            }

            return ((Timer <= 0 && !EffectActive && (!LimitedUses || UsesLeft > 0)) ||
                    (EffectActive && Timer <= EffectDuration - 2f));
        }

        protected override void OnClick()
        {
            if (!EffectActive)
            {
                PlayerControl.LocalPlayer.RpcAddModifier<MarksmanVanishModifier>();
                UsesLeft--;
                if (LimitedUses)
                {
                    Button?.SetUsesRemaining(UsesLeft);
                }
            }
            else
            {
                OnEffectEnd();
            }
        }

        public override void OnEffectEnd()
        {
            if (!PlayerControl.LocalPlayer.HasModifier<MarksmanVanishModifier>())
            {
                return;
            }

            PlayerControl.LocalPlayer.RpcRemoveModifier<MarksmanVanishModifier>();
        }
    }
}
