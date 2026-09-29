using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Networking;
using TownOfUs.Utilities;
using TreeWallMod.TWAssets;
using TreeWallMod.Modifiers.Crewmate;
using TreeWallMod.Options.Roles.Crewmate;
using TreeWallMod.Roles.Crewmate;
using UnityEngine;
using TownOfUs.Events.TouEvents;
using Reactor.Utilities;

namespace TreeWallMod.Events.Crewmate
{
	public static class SyringeEvents
	{
		[RegisterEvent]
		public static void AfterMurderEventHandler(AfterMurderEvent @event)
		{
			if (MeetingHud.Instance || 
				@event.Source.HasDied() || 
				(@event.Source.TryGetModifier<DisabledModifier>(out var mod) && (!mod.IsConsideredAlive || !mod.CanBeInteractedWith)))
			{
				return;
			}

			if (!@event.Source.TryGetModifier<SyringeInjectedModifier>(out var injected) || injected.SyringeItems.Count == 0)
			{
				return;
			}

			if (!injected.SyringeItems.Any(x => x.Evil == true))
			{
				return;
			}

			if (@event.Source.IsCrewmate())
			{
				return;
			}

            if (injected.SyringeItems.Any(x => x.Syringe == PlayerControl.LocalPlayer))
            {
				Coroutines.Start(MiscUtils.CoFlash(Colors.Syringe));
            }

            if (@event.Source.AmOwner)
			{
                //@event.Source.RpcRemoveModifier<SyringeInjectedModifier>();
                injected.SyringeItems.Last().Syringe.RpcSpecialMurder(@event.Source, MeetingCheck.OutsideMeeting, ignoreShield: false, createDeadBody: false, teleportMurderer: false,
                    showKillAnim: false, playKillSound: false, causeOfDeath: "Syringe");

                var notif1 = Helpers.CreateAndShowNotification(
					$"You got cured!", Color.white,
					new Vector3(0f, 1f, -20f), spr: TWCrewAssets.SyringeInjectSprite.LoadAsset());
                notif1.AdjustNotification();
            }
		}

		[RegisterEvent]
		public static void StartMeetingEventHandler(StartMeetingEvent @event)
		{
			if (PlayerControl.LocalPlayer.HasModifier<SyringeInjectedModifier>())
			{
				PlayerControl.LocalPlayer.RpcRemoveModifier<SyringeInjectedModifier>();
			}
		}
	}
}
