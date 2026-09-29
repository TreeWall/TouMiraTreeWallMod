using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using System.Linq;
using TownOfUs.Buttons;
using TreeWallMod.Modifiers.GameModifers;
using TreeWallMod.Options.Modifiers;

namespace TreeWallMod.Events.Modifiers
{
    public static class HeadlessEvents
    {
        [RegisterEvent(int.MaxValue)]
        public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
        {
            if (!@event.Target.AmOwner || @event.IsCancelled)
            {
                return;
            }

            if (PlayerControl.LocalPlayer.TryGetModifier<HeadlessModifier>(out var headless))
            {
                var buttons = CustomButtonManager.Buttons;

                headless.KillButton =
                    OptionGroupSingleton<TWUniversalModifierOptions>.Instance.HeadlessKillButton.Value &&
                    (buttons.Any(
                        button => button != null && button.Button != null &&
                        button is IKillButton && button.Button.isActiveAndEnabled) ||
                    PlayerControl.LocalPlayer.IsImpostorAligned());
            }
        }

        //[RegisterEvent(int.MaxValue)]
        //public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
        //{
        //	if (@event.IsCancelled || MeetingHud.Instance)
        //          {
        //		//Logger<TreeWallModPlugin>.Instance.LogMessage("Already Cancelled");
        //		return;
        //	}

        //	if (!@event.Target.TryGetModifier<HeadlessModifier>(out var headless)) return;

        //	if (!headless.Die)
        //	{
        //		@event.Cancel();
        //		@event.Source.SetKillTimer(@event.Source.GetKillCooldown());
        //	}

        //	if (!@event.Target.AmOwner || headless.Die)
        //	{
        //		return;
        //	}
        //	Logger<TreeWallModPlugin>.Instance.LogMessage("Cancelled Event");

        //	if (!headless.headlessState)
        //	{
        //		@event.Target.RpcChangeAnimation(PlayerAnimationClips.Run , StoredAnimationClips.headlessWalkAnim);
        //		@event.Target.RpcChangeAnimation(PlayerAnimationClips.Idle, StoredAnimationClips.headlessIdleAnim, true);
        //		@event.Target.RpcCosmeticControl(false);
        //		@event.Target.RemainingEmergencies = 0;

        //		@event.Target.NetTransform.Halt();

        //		//tp killer to the headless person
        //		@event.Source.NetTransform.RpcSnapTo(@event.Target.transform.position);

        //		//play kill animation for headless person
        //		try
        //		{
        //			HudManager.Instance.KillOverlay.ShowKillAnimation(@event.Source.Data, @event.Target.Data);
        //		}
        //		catch (Exception e)
        //		{
        //			Error($"Kill animation failed: {e}");
        //		}

        //		headless.killer = @event.Source;

        //		headless.headlessState = true;
        //		@event.Target.RpcAddModifier<DisableButtonsModifier>();
        //	}
        //}

        //[RegisterEvent]
        //public static void ReportBodyEventHandler(ReportBodyEvent @event)
        //{
        //	if (!PlayerControl.LocalPlayer.TryGetModifier<HeadlessModifier>(out var headless) || !headless.headlessState)
        //	{
        //		return;
        //	}

        //	var pc = PlayerControl.LocalPlayer;
        //	if (pc == null)
        //	{
        //		return;
        //	}

        //	pc.RpcChangeAnimation(PlayerAnimationClips.Run , StoredAnimationClips.ogWalk);
        //	pc.RpcChangeAnimation(PlayerAnimationClips.Idle, StoredAnimationClips.ogIdle, true);
        //	pc.RpcCosmeticControl(true);

        //	try
        //	{
        //		headless.Die = true;
        //		headless.killer.RpcCustomMurder(pc, resetKillTimer: false, createDeadBody: false, playKillSound: false, teleportMurderer: false, showKillAnim: false);
        //		pc.RpcRemoveModifier<HeadlessModifier>();
        //	}
        //	catch
        //	{
        //		Error("Failed to kill or Remove Modifier");
        //		try { pc.RpcRemoveModifier<DisableButtonsModifier>(); }
        //		catch { Error("Failed to Remove DisableButtonModifier"); }
        //	}
        //}
    }
}