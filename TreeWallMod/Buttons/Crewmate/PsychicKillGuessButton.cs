using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using System;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Networking;
using TreeWallMod.Options.Roles.Crewmate;
using TreeWallMod.Roles.Crewmate;
using UnityEngine;

namespace TreeWallMod.Buttons.Crewmate
{
    public sealed class PsychicKillGuessButton : TownOfUsRoleButton<PsychicRole>
    {
        public override string Name => "Guess Killer";
        public override Color TextOutlineColor => new Color32(165, 231, 89, 255);
        public override float Cooldown => Math.Clamp(OptionGroupSingleton<PsychicOptions>.Instance.PsychicGuessCd + MapCooldown, 5f, 120f);
        public override ButtonLocation Location => ButtonLocation.BottomLeft;
        public override LoadableAsset<Sprite> Sprite => TWAssets.TWCrewAssets.PsychicKillGuessSprite;
        public bool CanStillUse = true;

        public override bool UsableInDeath => false;
        public override float EffectDuration => 3.0f;

        private PlayerControl? Victim;

        public override void ClickHandler()
        {
            if (!CanClick())
            {
                return;
            }

            OnClick();
        }

        protected override void OnClick()
        {
            var targetPlayer = PlayerControl.LocalPlayer;
            targetPlayer.NetTransform.Halt();

            if (Minigame.Instance)
            {
                return;
            }

            var player1Menu = CustomPlayerMenu.Create();
            player1Menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
                PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
            player1Menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
                PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

            player1Menu.Begin(
                plr => !plr.Data.Disconnected && (!plr.Data.IsDead || !plr.DiedOtherRound()),
                plr =>
                {
                    player1Menu.ForceClose();

                    if (plr == null || plr.Data.Disconnected || MeetingHud.Instance)
                    {
                        return;
                    }

                    Victim = plr;
                    EffectActive = true;
                    Timer = EffectDuration;
                });
            foreach (var panel in player1Menu.potentialVictims)
            {
                panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
                if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
                {
                    panel.NameText.color = Color.white;
                }
            }
        }

        public override void OnEffectEnd()
        {
            if (Victim == null || Victim.Data.Disconnected || (Victim.TryGetModifier<DisabledModifier>(out var mod) && (!mod.IsConsideredAlive || !mod.CanBeInteractedWith)) || MeetingHud.Instance)
            {
                SetTimer(5f);
                return;
            }

            var player = PlayerControl.LocalPlayer;
            var psychic = player.GetRole<PsychicRole>()!;

            if (psychic.HasKiller(Victim.PlayerId) && !Victim.Data.IsDead)
            {
                try
                {
                    player.RpcSpecialMurder(Victim, MeetingCheck.OutsideMeeting, true, teleportMurderer: false, showKillAnim: true, playKillSound: false, causeOfDeath: "PsychicGuess");
                }
                catch
                {
                    player.RpcSpecialMurder(Victim, MeetingCheck.OutsideMeeting, true, teleportMurderer: false, showKillAnim: false, playKillSound: false, causeOfDeath: "PsychicGuess");
                }
            }
            else if (OptionGroupSingleton<PsychicOptions>.Instance.WrongGuessToggle)
            {
                try
                {
                    player.RpcSpecialMurder(player, MeetingCheck.OutsideMeeting, true, teleportMurderer: false, showKillAnim: true, playKillSound: false, causeOfDeath: "PsychicMisguess");
                }
                catch
                {
                    player.RpcSpecialMurder(player, MeetingCheck.OutsideMeeting, true, teleportMurderer: false, showKillAnim: false, playKillSound: false, causeOfDeath: "PsychicMisguess");
                }
            }
        }
    }
}
