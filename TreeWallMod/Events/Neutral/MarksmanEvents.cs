using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using System.Linq;
using TreeWallMod.Buttons.Neutral.Marksman;
using TreeWallMod.Modifiers.Neutral;
using TreeWallMod.Modules;
using TreeWallMod.Options.Roles.Neutral;
using TreeWallMod.Roles.Neutral;

namespace TreeWallMod.Events.Neutral
{
    public static class MarksmanEvents
    {
        [RegisterEvent]
        public static void AfterMurderEventHandler(AfterMurderEvent @event)
        {
            Message($"{@event.Target.name} got killed by {@event.Source.name}");

            if (!PlayerControl.LocalPlayer.IsRole<MarksmanRole>())
            {
                return;
            }

            var discoverButton = CustomButtonSingleton<MarksmanDiscoverButton>.Instance;
            var vanishButton = CustomButtonSingleton<MarksmanVanishButton>.Instance;
            if ((discoverButton.FirstTarget != null) && @event.Target == discoverButton.FirstTarget && !MeetingHud.Instance)
            {
                ++discoverButton.UsesLeft;
                if ((int)OptionGroupSingleton<MarksmanOptions>.Instance.InitialDiscoverUses != 0)
                {
                    discoverButton.SetUses(discoverButton.UsesLeft);
                }
                discoverButton.FirstTarget = null;
            }

            if ((discoverButton.SecondTarget != null) && @event.Target == discoverButton.SecondTarget && !MeetingHud.Instance)
            {
                ++discoverButton.UsesLeft;
                if ((int)OptionGroupSingleton<MarksmanOptions>.Instance.InitialDiscoverUses != 0)
                {
                    discoverButton.SetUses(discoverButton.UsesLeft);
                }
                discoverButton.SecondTarget = null;
            }

            if (@event.Source.AmOwner)
            {
                discoverButton.KilledPlayer();
                vanishButton.KilledPlayer();
            }
        }

        [RegisterEvent]
        public static void StartMeetingEventHandler(StartMeetingEvent @event)
        {
            var marksman = PlayerControl.LocalPlayer.GetRole<MarksmanRole>();
            if (marksman != null)
            {
                marksman.WarpMarkedPlayer = null;
                marksman.WarpMarking = MarksmanWarpState.Marking;

                var smokebombButton = CustomButtonSingleton<MarksmanSmokebombButton>.Instance;
                smokebombButton.SetUses((int)OptionGroupSingleton<MarksmanOptions>.Instance.SmokebombUses);
            }
        }

        [RegisterEvent]
        public static void AfterMarksmanSuppresedModifierPlayerMurderHandler(AfterMurderEvent @event)
        {
            if (@event.Target.AmOwner && MeetingHud.Instance
                && @event.Target.TryGetModifier<MarksmanSuppressedModifier>(out var marksmanSuppressedModifier) &&
                marksmanSuppressedModifier.Killer != @event.Source)
            {
                PlayerControl.LocalPlayer.RpcRemoveModifier<MarksmanSuppressedModifier>();
            }
        }

        [RegisterEvent]
        public static void MarksmanSuppresedModifierVotingCompleteEventHandler(VotingCompleteEvent @event)
        {
            if (!PlayerControl.LocalPlayer.IsHost())
            {
                return;
            }

            var suppressedPlayers = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => !x.Data.IsDead && x.HasModifier<MarksmanSuppressedModifier>());

            //Message($"{suppressedPlayers.Count()} players are to be Suppressed");
            foreach (var plr in suppressedPlayers)
            {
                Message($"Deciding {plr.name}'s fate");
                if (!plr.TryGetModifier<MarksmanSuppressedModifier>(out var marksmanSuppressedMod))
                {
                    Message("Doesnt have marksman mod");
                    continue;
                }

                plr.RpcMarksmanSuppressedComplete(marksmanSuppressedMod);
            }
        }
    }
}
