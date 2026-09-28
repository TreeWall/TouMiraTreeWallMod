using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TreeWallMod.Assets
{
    public static class NeutAssets
    {
        private const string ShortPath = "TreeWallMod.Resources.NeutButtons";
        public static LoadableAsset<Sprite> MarksmanDiscover { get; } = new LoadableResourceAsset($"{ShortPath}.MarksmanDiscover.png");
        public static LoadableAsset<Sprite> MarksmanSharpenedBlade { get; } = new LoadableResourceAsset($"{ShortPath}.SharpenedBlade.png");
        public static LoadableAsset<Sprite> MarksmanWarp { get; } = new LoadableResourceAsset($"{ShortPath}.MarksmanWarp.png");
        public static LoadableAsset<Sprite> MarksmanSuppressed { get; } = new LoadableResourceAsset($"{ShortPath}.MarksmanSuppressed.png");
        public static LoadableAsset<Sprite> MarksmanSmokeBomb { get; } = new LoadableResourceAsset($"{ShortPath}.MarksmanSmokebomb.png");
        public static LoadableAsset<Sprite> MarksmanDismantle { get; } = new LoadableResourceAsset($"{ShortPath}.MarksmanDismantle.png");
    }
}