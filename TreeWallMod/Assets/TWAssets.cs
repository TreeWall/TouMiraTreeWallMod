using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace TreeWallMod.TWAssets
{
    public static class TWAssets
    {
        private const string ShortPath = "TreeWallMod.Resources";

        public static readonly AssetBundle Bundle = AssetBundleManager.Load("rfsbundle");

        public static readonly LoadableBundleAsset<AnimationClip> HeadlessWalkAnim = new LoadableBundleAsset<AnimationClip>("walk.anim", Bundle);
        public static readonly LoadableBundleAsset<AnimationClip> HeadlessIdleAnim = new LoadableBundleAsset<AnimationClip>("idle_0.anim", Bundle);
        public static readonly LoadableBundleAsset<AnimationClip> VanishPoofAnim = new LoadableBundleAsset<AnimationClip>("Vanish_Poof.anim", Bundle);
        public static readonly LoadableBundleAsset<AnimationClip> VanishChargeAnim = new LoadableBundleAsset<AnimationClip>("Vanish_ChargeLoop.anim", Bundle);

        public static LoadableAudioResourceAsset FartKillSound1 { get; } = new LoadableAudioResourceAsset($"{ShortPath}.Audio.FartKill1.wav");
        public static LoadableAudioResourceAsset FartKillSound2 { get; } = new LoadableAudioResourceAsset($"{ShortPath}.Audio.FartKill2.wav");
        public static LoadableAudioResourceAsset FartKillSound3 { get; } = new LoadableAudioResourceAsset($"{ShortPath}.Audio.FartKill3.wav");

        public static LoadableAsset<Sprite> Cloud_1 { get; } = new LoadableResourceAsset($"{ShortPath}.Cloud_1.png");
        public static LoadableAsset<Sprite> Cloud_2 { get; } = new LoadableResourceAsset($"{ShortPath}.Cloud_2.png");
        public static LoadableAsset<Sprite> Cloud_3 { get; } = new LoadableResourceAsset($"{ShortPath}.Cloud_3.png");
        public static LoadableAsset<Sprite> Cloud_4 { get; } = new LoadableResourceAsset($"{ShortPath}.Cloud_4.png");
    }
}