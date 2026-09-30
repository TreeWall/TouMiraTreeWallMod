using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TreeWallMod.Modules;
using TreeWallMod.Options;
using UnityEngine;

namespace TreeWallMod
{
    [BepInAutoPlugin("com.treewall.mod", "TreeWallMod")]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    [BepInDependency(MiraApiPlugin.Id)]
    [BepInDependency("auavengers.tou.mira", BepInDependency.DependencyFlags.SoftDependency)]
    [ReactorModFlags(ModFlags.RequireOnAllClients)]
    public partial class TreeWallModPlugin : BasePlugin, IMiraPlugin
    {
        public string OptionsTitleText => "TreeWall Mod";
        public static bool IsDevBuild =>
#if DEBUG
    true;
#else
    false;
#endif

        public Harmony Harmony { get; } = new(Id);
        public ConfigFile GetConfigFile() => Config;

        public override void Load()
        {
            Harmony.PatchAll();

            MiraLocaleManager.Register(Id);
            ReactorCredits.Register<TreeWallModPlugin>(ReactorCredits.AlwaysShow);

            if (IsDevBuild)
            {
                GameObject guiObject = new GameObject("ModDebugGuiObject");
                UnityEngine.Object.DontDestroyOnLoad(guiObject);
                guiObject.hideFlags = HideFlags.HideAndDontSave;

                guiObject.AddComponent<DebuggingWindow>();
            }

            Patches.ChangeSoundPatch.RegisterSwap("impostor_kill", () =>
            {
                int rand = UnityEngine.Random.RandomRangeInt(0, 3);

                switch (rand)
                {
                    case 0:
                        return TWAssets.TWAssets.FartKillSound1;

                    case 1:
                        return TWAssets.TWAssets.FartKillSound2;

                    case 2:
                        return TWAssets.TWAssets.FartKillSound3;

                    default:
                        return TWAssets.TWAssets.FartKillSound2;
                }
            },
            isEnabled: () => OptionGroupSingleton<TWGeneralOptions>.Instance.FartKill);
        }
    }
}
