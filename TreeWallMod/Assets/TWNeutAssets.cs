using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TreeWallMod.TWAssets
{
    public static class TWNeutAssets
    {
        private const string ShortPath = "TreeWallMod.Resources.NeutButtons";
        public static LoadableAsset<Sprite> MarksmanDiscoverSprite { get; } = new LoadableResourceAsset($"{ShortPath}.MarksmanDiscover.png");
        public static LoadableAsset<Sprite> MarksmanSharpenedBladeSprite { get; } = new LoadableResourceAsset($"{ShortPath}.SharpenedBlade.png");
        public static LoadableAsset<Sprite> MarksmanWarpSprite { get; } = new LoadableResourceAsset($"{ShortPath}.MarksmanWarp.png");
        public static LoadableAsset<Sprite> MarksmanSuppressedSprite { get; } = new LoadableResourceAsset($"{ShortPath}.MarksmanSuppressed.png");
        public static LoadableAsset<Sprite> MarksmanSmokeBombSprite { get; } = new LoadableResourceAsset($"{ShortPath}.MarksmanSmokebomb.png");
        public static LoadableAsset<Sprite> MarksmanDismantleSprite { get; } = new LoadableResourceAsset($"{ShortPath}.MarksmanDismantle.png");
    }
}