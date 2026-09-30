using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeWallMod.Options.Roles.Crewmate;
using TreeWallMod.Roles.Crewmate;

namespace TreeWallMod.Patches
{
	[HarmonyPatch]
	public static class PsychicVitalsDisablePatch
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
		public static bool UseButtonClickPatch(UseButton __instance)
		{
            if (PlayerControl.LocalPlayer.Data.Role is not PsychicRole || __instance.currentTarget == null) return true;
            var icon = __instance.currentTarget.UseIcon;
            if (icon is ImageNames.VitalsButton)
            {
                return OptionGroupSingleton<PsychicOptions>.Instance.UseVitals;
            }
            return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.Use))]
		public static bool UseConsoleClickPatch(SystemConsole __instance)
		{
			if (PlayerControl.LocalPlayer.Data.Role is not PsychicRole) return true;
			var icon = __instance.UseIcon;
			if (icon is ImageNames.VitalsButton)
			{
				return OptionGroupSingleton<PsychicOptions>.Instance.UseVitals;
			}
			return true;
		}
	}
}
