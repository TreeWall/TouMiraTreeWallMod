using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using System.Linq;
using TownOfUs.Networking;
using UnityEngine;

namespace TreeWallMod.Modifiers.Neutral
{
    public sealed class MarksmanSuppressedModifier(PlayerControl killer) : BaseModifier
    {
        public override string ModifierName => "MarksmanSuppressed";
        public override bool HideOnUi => true;

        public SpriteRenderer BmOverlay;
        public PlayerVoteArea VoteArea;
        public bool Voted;
        public bool SetAlive { get; set; } = false;

        public PlayerControl Killer { get; } = killer;

        public MurderResultFlags murderResultFlags { get; set; } = MurderResultFlags.Succeeded;

        public override void OnActivate()
        {
            base.OnActivate();

            var meetingInstance = MeetingHud.Instance;
            if (!meetingInstance)
            {
                if (Player.AmOwner)
                {
                    Killer.RpcMeetingMurder(Player, MeetingAnimation.None, causeOfDeath: "Marksman");
                }

                ModifierComponent!.RemoveModifier(this);
                return;
            }

            VoteArea = meetingInstance.playerStates.FirstOrDefault(x => x.PlayerId == Player.PlayerId)!;
            if (!VoteArea)
            {
                if (Player.AmOwner)
                {
                    Killer.RpcMeetingMurder(Player, MeetingAnimation.None, causeOfDeath: "Marksman");
                }

                ModifierComponent!.RemoveModifier(this);
                return;
            }

            var beforeMurderEvent = new BeforeMurderEvent(Killer, Player, MeetingCheck.ForMeeting);
            MiraEventManager.InvokeEvent(beforeMurderEvent);

            if (!MeetingHud.Instance && !ExileController.Instance)
            {
                beforeMurderEvent.Cancel();
            }

            if (beforeMurderEvent.IsCancelled)
            {
                murderResultFlags = MurderResultFlags.FailedError;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            var meetingInstance = MeetingHud.Instance;

            if (!meetingInstance)
            {
                return;
            }

            if (!VoteArea || VoteArea.DidVote || Voted)
            {
                return;
            }

            if (!Player.AmOwner)
            {
                return;
            }

            //Message("Skipping");
            VoteArea.SetVote(252);
            meetingInstance.Confirm(252);
            if (VoteArea.DidVote)
            {
                Voted = true;
            }
        }

        //     public override void OnDeath(DeathReason reason)
        //     {
        //ModifierComponent!.RemoveModifier(this);
        //     }
    }
}
