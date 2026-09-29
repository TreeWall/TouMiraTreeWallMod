using AmongUs.Data;
using Assets.CoreScripts;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using System;
using System.Collections.Generic;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TreeWallMod.Modifiers.Crewmate;
using TreeWallMod.Modifiers.Neutral;
using UnityEngine;

namespace TreeWallMod.Modules
{
    public static class TreeWallModRpcs
    {
        [MethodRpc((uint)TreeWallModRpcsEnum.ChangeAnimation)]
        public static void RpcChangeAnimation(this PlayerControl pc, PlayerAnimationClips pac, StoredAnimationClips animation, bool playIdleAnim = false)
        {
            Setter[(int)pac](pc.MyPhysics.Animations.group, LoadedAnimationClips[(int)animation]);
            if (playIdleAnim) pc.MyPhysics.Animations.PlayIdleAnimation();
        }

        static readonly Action<PlayerAnimationGroup, AnimationClip>[] Setter =
        {
            (g, a) => g.IdleAnim = a,
            (g, a) => g.RunAnim = a
        };

        public enum PlayerAnimationClips
        {
            Idle = 0,
            Run
        }

        public enum StoredAnimationClips
        {
            ogIdle = 0,
            ogWalk,
            headlessIdleAnim,
            headlessWalkAnim
        }

        static List<AnimationClip> LoadedAnimationClips = new List<AnimationClip>
        {
            PlayerControl.LocalPlayer.MyPhysics.Animations.group.IdleAnim,
            PlayerControl.LocalPlayer.MyPhysics.Animations.group.RunAnim,
            TWAssets.TWAssets.HeadlessIdleAnim.LoadAsset(),
            TWAssets.TWAssets.HeadlessWalkAnim.LoadAsset()
        };


        [MethodRpc((uint)TreeWallModRpcsEnum.CosmeticControl)]
        public static void RpcCosmeticControl(this PlayerControl pc, bool active)
        {
            pc.cosmetics.gameObject.SetActive(active);
        }

        [MethodRpc((uint)TreeWallModRpcsEnum.RemovePlayerSyringeInject)]
        public static void RpcRemovePlayerSyringeInject(this PlayerControl injected, PlayerControl syringe)
        {
            if (!injected.TryGetModifier<SyringeInjectedModifier>(out var syringeInjectedMod))
            {
                return;
            }

            syringeInjectedMod.RemovePlayer(syringe);
            //Message($"Removed {syringe.name} from {syringeInjectedMod.Player.name}");
        }

        [MethodRpc((uint)TreeWallModRpcsEnum.AddPlayerSyringeInject)]
        public static void RpcAddPlayerSyringeInject(this PlayerControl injected, PlayerControl syringe)
        {
            if (!injected.TryGetModifier<SyringeInjectedModifier>(out var syringeInjectedMod))
            {
                Warning("Doesnt Have modifier!");
                return;
            }

            syringeInjectedMod.AddPlayer(syringe);
            //Message($"Added {syringe.name} to {syringeInjectedMod.Player.name}");
        }

        [MethodRpc((uint)TreeWallModRpcsEnum.MarksmanSuppressedComplete)]
        public static void RpcMarksmanSuppressedComplete(this PlayerControl target, MarksmanSuppressedModifier marksmanSuppressedMod)
        {
            //if (!target.TryGetModifier<MarksmanSuppressedModifier>(out var marksmanSuppressedMod))
            //{
            //	return;
            //}

            var source = marksmanSuppressedMod.Killer;

            var murderResultFlags2 = marksmanSuppressedMod.murderResultFlags | MurderResultFlags.DecisionByHost;

            if (!murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) ||
                !murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost) ||
                murderResultFlags2.HasFlag(MurderResultFlags.FailedError))
            {
                return;
            }

            //GameHistory.UpdatePlayerDeathData(target.PlayerId, MiraLocaleManager.Get($"DiedToMarksman"), 0,
            //    HudManagerHelper.Instance.CurrentRound, (!MeetingHud.Instance && !ExileController.Instance)
            //        ? DeathHandlerOverride.SetTrue
            //        : DeathHandlerOverride.SetFalse,
            //    MiraLocaleManager.Get("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
            //    lockInfo: DeathHandlerOverride.SetTrue,
            //    playerState: StoredPlayerState.Dead);

            GameHistory.UpdatePlayerDeathData(target.PlayerId, MiraLocaleManager.Get($"DiedToMarksman"), 0,
                HudManagerHelper.Instance.CurrentRound, DeathHandlerOverride.SetTrue,
                MiraLocaleManager.Get("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue,
                playerState: StoredPlayerState.Dead);

            DebugAnalytics.Instance.Analytics.Kill(target.Data, source.Data);
            if (source.AmOwner)
            {
                DataManager.Player.Stats.IncrementStat(
                    GameManager.Instance.IsHideAndSeek()
                        ? StatID.HideAndSeek_ImpostorKills
                        : StatID.ImpostorKills);
            }

            UnityTelemetry.Instance.WriteMurder();

            if (target.AmOwner)
            {
                DataManager.Player.Stats.IncrementStat(StatID.TimesMurdered);
                if (Minigame.Instance)
                {
                    try
                    {
                        Minigame.Instance.Close();
                        Minigame.Instance.Close();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                target.RpcSetScanner(false);
            }

            AchievementManager.Instance.OnMurder(
                source.AmOwner,
                target.AmOwner,
                source.CurrentOutfitType == PlayerOutfitType.Shapeshifted,
                source.shapeshiftTargetPlayerId,
                target.PlayerId);

            target.Die(DeathReason.Kill, true);

            var afterMurderEvent = new AfterMurderEvent(source, target, null);
            MiraEventManager.InvokeEvent(afterMurderEvent);

            // Dont FUCKING know why its like this
            if (PlayerControl.LocalPlayer.IsHost())
            {
                marksmanSuppressedMod.VoteArea.Overlay?.gameObject?.SetActive(false);
                marksmanSuppressedMod.VoteArea.XMark?.gameObject?.SetActive(false);
            }
            else
            {
                marksmanSuppressedMod.VoteArea.SetDead(false);
            }

            if (PlayerControl.LocalPlayer.IsHost())
            {
                target.RpcRemoveModifier<MarksmanSuppressedModifier>();
            }
        }
    }
}
