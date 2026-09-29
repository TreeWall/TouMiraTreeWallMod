using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using System;
using System.Linq;
using TownOfUs;
using TreeWallMod.Modifiers.Neutral;
using static TreeWallMod.Modules.Debugging;

namespace TreeWallMod.Patches
{
    [HarmonyPatch]
    public static class ChatPatches
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        public static bool FPrefix(ChatController __instance)
        {
            // this patch only works in dev builds
            if (!TreeWallModPlugin.IsDevBuild)
            {
                return true;
            }

            var text = __instance.freeChatField.Text.ToLower(TownOfUsPlugin.Culture);
            var textRegular = __instance.freeChatField.Text.WithoutRichText();

            var systemName = $"<color=#8BFDFD>{MiraLocaleManager.Get("SystemChatTitle")}</color>";

            if (textRegular.StartsWith("/modifier ", StringComparison.OrdinalIgnoreCase))
            {
                string targetName = textRegular.Substring(10).Trim();
                var target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.Data?.PlayerName.Equals(targetName, StringComparison.OrdinalIgnoreCase) == true);

                if (target == null)
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        $"<color=#FF0000>Player \"{targetName}\" not found.</color>");
                    ClearChat(__instance);
                    return false;
                }

                string outputString = "";

                foreach (var modifier in target.GetModifiers<BaseModifier>())
                    outputString += $"{modifier.ModifierName} ";

                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        $"{outputString}");
                ClearChat(__instance);

                return false;
            }
            else if (textRegular.StartsWith("/debug", StringComparison.OrdinalIgnoreCase))
            {
                string output = "";

                if (textRegular.Contains("Camera"))
                {
                    output = DebugCamera();
                }
                if (textRegular.Contains("KillButton"))
                {
                    output = DebugKillButton();
                }

                Message(output);

                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        $"{output}");

                ClearChat(__instance);
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.UpdateChatMode))]
        public static void UpdateChatModePostfix(ChatController __instance)
        {
            if (PlayerControl.LocalPlayer.HasModifier<MarksmanSuppressedModifier>() && MeetingHud.Instance)
            {
                __instance.freeChatField.SetVisible(false);
                __instance.quickChatField.SetVisible(false);
            }

            if (PlayerControl.LocalPlayer.HasModifier<MarksmanSuppressedModifier>() && !MeetingHud.Instance && PlayerControl.LocalPlayer.Data.IsDead)
            {
                __instance.freeChatField.SetVisible(true);
                __instance.quickChatField.SetVisible(true);
            }
        }

        private static void ClearChat(ChatController chat)
        {
            chat.freeChatField.Clear();
            chat.quickChatMenu.Clear();
            chat.quickChatField.Clear();
            chat.UpdateChatMode();
        }
    }
}
