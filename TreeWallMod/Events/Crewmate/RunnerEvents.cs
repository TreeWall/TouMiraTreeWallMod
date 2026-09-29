using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using System.Linq;
using TreeWallMod.Buttons.Crewmate;
using TreeWallMod.Modules;
using TreeWallMod.Options.Roles.Crewmate;
using TreeWallMod.Roles.Crewmate;
using TreeWallMod.TWAssets;
using UnityEngine;

namespace TreeWallMod.Events.Crewmate
{
    public static class RunnerEvents
    {
        [RegisterEvent]
        public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
        {
            var runner = @event.Target.GetRole<RunnerRole>();
            if (runner == null)
            {
                return;
            }

            if (!MeetingHud.Instance &&
                runner.RunnerMoves.Count > 0 && runner.RunnerMoves.MaxBy(move => move.Order).Moving &&
                (!OptionGroupSingleton<RunnerOptions>.Instance.CaffeineSave || runner.SpeedActive) &&
                (!OptionGroupSingleton<RunnerOptions>.Instance.CommsEnabled || !TWHelpers.CommsActive()))
            {
                @event.Cancel();

                if (@event.Source.AmOwner)
                {
                    var notif1 = Helpers.CreateAndShowNotification(
                        "Target was a Runner, cannot kill them while they are running",
                        Color.white, new Vector3(0f, 1f, -20f), spr: TWCrewAssets.RunnerCaffeineSprite.LoadAsset());

                    notif1.AdjustNotification();
                }

            }
        }

        [RegisterEvent]
        public static void CompleteTaskEvent(CompleteTaskEvent @event)
        {
            if (@event.Player.AmOwner && @event.Player.Data.Role is RunnerRole &&
                OptionGroupSingleton<RunnerOptions>.Instance.CaffeineUses != 0 && OptionGroupSingleton<RunnerOptions>.Instance.TaskUses)
            {
                var button = CustomButtonSingleton<RunnerCaffeineButton>.Instance;
                ++button.UsesLeft;
                button.SetUses(button.UsesLeft);
            }
        }
    }
}