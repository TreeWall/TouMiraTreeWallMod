using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Patches;
using TreeWallMod.Buttons;
using TreeWallMod.Modules;
using TreeWallMod.Options.Modifiers;
using TreeWallMod.TWAssets;
using UnityEngine;

namespace TreeWallMod.Modifiers.GameModifers
{
    public sealed class HeadlessModifier : UniversalGameModifier, IWikiDiscoverable
    {
        public override ModifierUiConfiguration Configuration => new(
        Colors.HeadlessModifier,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Shy.LoadAsset(),
            "TreeWallMod.Modifier.Universal.Headless", 1.45f));
        public override string IdPart => "Headless";
        public override string ModifierName => "Headless";
        public override LoadableAsset<Sprite>? ModifierIcon => TWModifAssets.HeadlessModifierSprite;

        public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;
        public override Color FreeplayFileColor => Colors.HeadlessModifier;

        public PlayerControl? Killer { get; set; }
        public bool Dead { get; set; } = false;
        public bool Setup { get; set; }

        public bool KillButton { get; set; } = false;
        public HeadlessPlayer HeadlessObject { get; set; }

        public override string GetDescription()
        {
            return MiraLocaleManager.Get($"TreeWallMod.Modifier.{IdPart}.TabDescription");
        }

        public string GetAdvancedDescription()
        {
            return MiraLocaleManager.Get($"TreeWallMod.Modifier.{IdPart}.WikiDescription");
        }

        public override int GetAmountPerGame()
        {
            return (int)OptionGroupSingleton<TWUniversalModifierOptions>.Instance.HeadlessAmount;
        }

        public override int GetAssignmentChance()
        {
            return (int)OptionGroupSingleton<TWUniversalModifierOptions>.Instance.HeadlessChance;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (Dead)
            {
                Player.cosmetics.gameObject.SetActive(false);
                Player.cosmetics.currentBodySprite.BodySprite.color = new Color(1, 1, 1, 0);
            }
        }

        public override void OnDeath(DeathReason reason)
        {
            base.OnDeath(reason);
            Dead = true;

            var body = Helpers.GetBodyById(Player.PlayerId);

            if (body != null)
            {
                body.ClearBody();
            }

            HeadlessObject =  HeadlessPlayer.Spawn(Player, idleAnim: TWAssets.TWAssets.HeadlessIdleAnim.LoadAsset(), walkAnim: TWAssets.TWAssets.HeadlessWalkAnim.LoadAsset());

            Player.gameObject.layer = LayerMask.NameToLayer("Players");

            Player.gameObject.GetComponent<BoxCollider2D>().enabled = true;
            Player.Collider.enabled = true;

            if (Player.AmOwner)
            {
                HudManager.Instance.SetHudActive(false);
                HudManager.Instance.SetHudActive(true);
                HudManagerPatches.ResetZoom();

                HudManager.Instance.ShadowQuad.gameObject.SetActive(true);

                var killButton = CustomButtonSingleton<HeadlessKillButton>.Instance;
                killButton.SetTimer(killButton.Cooldown);

                Player.RpcAddModifier<DisableButtonsModifier>();
            }
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();

            Message("OnDeactivate Called");

            Player.gameObject.GetComponent<BoxCollider2D>().enabled = false;

            Player.gameObject.layer = LayerMask.NameToLayer("Ghost");
            Player.Collider.enabled = false;

            if (Player.AmOwner)
            {
                HudManager.Instance.SetHudActive(false);
                HudManager.Instance.SetHudActive(true);
                HudManagerPatches.ResetZoom();

                HudManager.Instance.ShadowQuad.gameObject.SetActive(false);

                Player.RpcRemoveModifier<DisableButtonsModifier>();
            }

            Player.cosmetics.gameObject.SetActive(true);
            Player.cosmetics.currentBodySprite.BodySprite.color = new Color(1, 1, 1, 1);

            if (HeadlessObject != null)
            {
                HeadlessObject.Destroy();
            }
        }

        public override void OnMeetingStart()
        {
            if (Dead)
            {
                ModifierComponent!.RemoveModifier(this);
            }
        }
    }
}