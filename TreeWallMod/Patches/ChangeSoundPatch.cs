using HarmonyLib;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TreeWallMod.Patches
{
    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
    public static class ChangeSoundPatch
    {
        readonly struct SoundSwapInfo(Func<LoadableAudioResourceAsset> audioClip, Func<bool> isEnabled, Func<bool> loop, Func<float> volume)
        {
            public Func<bool> IsEnabled { get; } = isEnabled;
            public Func<bool> Loop { get; } = loop;
            public Func<float> Volume { get; } = volume;
            public Func<LoadableAudioResourceAsset> AudioClip { get; } = audioClip;
        }

        static readonly Dictionary<string, SoundSwapInfo> SoundSwapList = new();

        public static void RegisterSwap(string clipName, LoadableAudioResourceAsset audioClip, Func<bool>? isEnabled = null, Func<bool>? loop = null, Func<float>? volume = null)
        {
            RegisterSwap(clipName, () => audioClip, isEnabled, loop, volume);
        }

        public static void RegisterSwap(string clipName, Func<LoadableAudioResourceAsset> audioClips, Func<bool>? isEnabled = null, Func<bool>? loop = null, Func<float>? volume = null)
        {
            isEnabled ??= () => true;
            loop ??= () => false;
            volume ??= () => 1f;

            SoundSwapList[clipName] = new SoundSwapInfo(audioClips, isEnabled, loop, volume);
        }

        static bool Prefix(ref AudioClip clip, ref bool loop, ref float volume, ref AudioSource __result)
        {
            if (clip == null)
            {
                return true;
            }

            if (SoundSwapList.TryGetValue(clip.name, out var soundSwapInfo) && soundSwapInfo.IsEnabled())
            {
                var temp = SoundSwapList[clip.name];

                clip = temp.AudioClip().LoadAsset();
                loop = temp.Loop();
                volume = temp.Volume();
            }

            return true;
        }

    }
}
