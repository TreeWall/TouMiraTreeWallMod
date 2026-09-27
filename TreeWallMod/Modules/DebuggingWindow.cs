using Reactor.Utilities.Attributes;
using System;
using TreeWallMod.Modifiers.GameModifiers;
using TreeWallMod.Options.Modifiers;
using UnityEngine;
using static TreeWallMod.Modules.Debugging;

namespace TreeWallMod.Modules
{
    [RegisterInIl2Cpp]
    public class DebuggingWindow(IntPtr cppPtr) : MonoBehaviour(cppPtr)
    {
        private bool _showWindow = true;
        private Rect _windowRect = new Rect(20, 20, 350, 380);

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _showWindow = !_showWindow;
            }
        }

        public void OnGUI()
        {
            if (!_showWindow) return;

            // Draw an immediate mode GUI window
            _windowRect = GUI.Window(
                0,
                _windowRect,
                (GUI.WindowFunction)DrawWindowContents,
                "Mod Testing Debugger"
            );
        }

        public void DrawWindowContents(int windowID)
        {
            GUILayout.Label("Mod Status: Active");

            if (GUILayout.Button("Camera"))
            {
                Message(DebugCamera());
            }
            if (GUILayout.Button("KIllButton"))
            {
                Message(DebugKillButton());
            }
            if (GUILayout.Button("SpawnHeadless"))
            {
                if (TutorialManager.InstanceExists)
                {
                    HeadlessPlayer.Spawn(PlayerControl.LocalPlayer, idleAnim: Assets.Assets.HeadlessIdleAnim.LoadAsset(), walkAnim: Assets.Assets.HeadlessWalkAnim.LoadAsset());
                }
            }
            if (GUILayout.Button("AddStrechComponent") && TutorialManager.InstanceExists)
            {
                PlayerControl.LocalPlayer.gameObject.AddComponent<DebugWrapper>();
            }

        }
    }

    [RegisterInIl2Cpp]
    public class DebugWrapper : MonoBehaviour
    {
        public void Strech(PlayerControl Player, WEEEStrechEnum strechMode, float duration = 10f, float scale = 2.5f, int cycles = 5, bool rotate = false, int rotCycles = 7)
        {
            DebugStrech(Player, strechMode, duration, scale, cycles, rotate, rotCycles);
        }
    }
}