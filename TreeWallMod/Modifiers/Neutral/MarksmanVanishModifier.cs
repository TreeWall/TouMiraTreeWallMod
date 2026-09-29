using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using TreeWallMod.Buttons.Neutral.Marksman;
using TreeWallMod.Options.Roles.Neutral;
using UnityEngine;

namespace TreeWallMod.Modifiers.Neutral
{
    public sealed class MarksmanVanishModifier : ConcealedModifier, IVisualAppearance
    {
        public override string ModifierName => "MarksmanVanished";
        public override float Duration => OptionGroupSingleton<MarksmanOptions>.Instance.VanishDuration;
        public override bool HideOnUi => true;
        public override bool AutoStart => true;
        public override bool VisibleToOthers => false;

        public bool VisualPriority => true;

        public VisualAppearance GetVisualAppearance()
        {
            var playerColor = (PlayerControl.LocalPlayer == Player || (PlayerControl.LocalPlayer.DiedOtherRound() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
                ? new Color(1f, 1f, 1f, 0.1f)
                : new Color(0.1f, 0.2f, 0.1f, 0.025f);

            return new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
            {
                HatId = "hat_NoHat",
                SkinId = "skin_None",
                VisorId = "visor_EmptyVisor",
                PlayerName = string.Empty,
                PetId = "pet_EmptyPet",
                RendererColor = playerColor,
                NameColor = Color.clear,
                ColorBlindTextColor = Color.clear
            };
        }

        public override void OnDeath(DeathReason reason)
        {
            Player.RemoveModifier(this);
        }

        public override void OnMeetingStart()
        {
            Player.RemoveModifier(this);
        }

        public override void OnActivate()
        {
            //CanSwooperVent =
            //    (SwooperVent)OptionGroupSingleton<SwooperOptions>.Instance.CanVent.Value is SwooperVent.Always;
            if (Player.AmOwner)
            {
                TouAudio.PlaySound(TouAudio.SwooperActivateSound);

                var button = CustomButtonSingleton<MarksmanVanishButton>.Instance;
                button.OverrideSprite(LegacyAssets.IsLegacy ? LegacyImpAssets.SwoopSprite.LoadAsset() : TouImpAssets.UnswoopSprite.LoadAsset());
                button.OverrideName("Appear");
            }

            RoleEffectAnimation roleEffectAnimation = UnityEngine.Object.Instantiate<RoleEffectAnimation>(DestroyableSingleton<RoleManager>.Instance.vanish_PoofAnim, Player.gameObject.transform);
            roleEffectAnimation.SetMaterialColor(Player.cosmetics.ColorId);
            roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(true);

            roleEffectAnimation.Play(Player, null, Player.cosmetics.FlipX, RoleEffectAnimation.SoundType.Local, 0f, true, 0f);

            Player.RawSetAppearance(this);
            Player.cosmetics.ToggleNameVisible(false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (VanillaSystemCheckPatches.ShroomSabotageSystem && VanillaSystemCheckPatches.ShroomSabotageSystem.IsActive)
            {
                Player.RawSetAppearance(this);
                Player.cosmetics.ToggleNameVisible(false);
            }
        }

        public override void OnDeactivate()
        {
            RoleEffectAnimation roleEffectAnimation = UnityEngine.Object.Instantiate<RoleEffectAnimation>(DestroyableSingleton<RoleManager>.Instance.vanish_PoofAnim, Player.gameObject.transform);
            roleEffectAnimation.SetMaterialColor(Player.cosmetics.ColorId);
            roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(true);

            roleEffectAnimation.Play(Player, null, Player.cosmetics.FlipX, RoleEffectAnimation.SoundType.Local, 0f, true, 0f);

            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(true);

            if (Player.AmOwner)
            {
                var button = CustomButtonSingleton<MarksmanVanishButton>.Instance;
                button.OverrideSprite(LegacyAssets.IsLegacy ? LegacyImpAssets.SwoopSprite.LoadAsset() : TouImpAssets.SwoopSprite.LoadAsset());
                button.OverrideName("Vanish");
                if (!MeetingHud.Instance)
                {
                    TouAudio.PlaySound(TouAudio.SwooperDeactivateSound);
                }
            }

            if (HudManagerPatches.CamouflageCommsEnabled)
            {
                Player.cosmetics.ToggleNameVisible(false);
            }

            if (VanillaSystemCheckPatches.ShroomSabotageSystem && VanillaSystemCheckPatches.ShroomSabotageSystem.IsActive)
            {
                MushroomMixUp(VanillaSystemCheckPatches.ShroomSabotageSystem, Player);
            }
        }

        public static void MushroomMixUp(MushroomMixupSabotageSystem instance, PlayerControl player)
        {
            if (player != null && !player.Data.IsDead && instance.currentMixups.ContainsKey(player.PlayerId))
            {
                var condensedOutfit = instance.currentMixups[player.PlayerId];
                var playerOutfit = instance.ConvertToPlayerOutfit(condensedOutfit);
                playerOutfit.NamePlateId = player.Data.DefaultOutfit.NamePlateId;

                player.MixUpOutfit(playerOutfit);
            }
        }

        public override bool? CanVent()
        {
            return false;
        }
    }
}
