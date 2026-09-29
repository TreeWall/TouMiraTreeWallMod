using HarmonyLib;
using MiraAPI.Modifiers;
using TreeWallMod.Modifiers.GameModifers;
using TreeWallMod.Roles.Crewmate;

namespace TreeWallMod.Patches
{
    [HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.GetPlayerSpeedMod))]
    public static class PlayerSpeedPatch
    {
        public static void Postfix(PlayerControl pc, ref float __result)
        {
            if (pc == null || MeetingHud.Instance)
            {
                return;
            }

            var runner = pc.GetRole<RunnerRole>();

            if (runner != null && runner.SpeedActive)
            {
                __result *= runner.SpeedMultiplier;
            }

            if (pc.TryGetModifier<HeadlessModifier>(out var headless) && headless.Dead)
            {
                // Ghost speed is multiplied by 1.2, so to reverse that, multiply by 0.8333
                __result *= 0.8333f;
            }
        }
    }
}