using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TreeWallMod.TWAssets
{
    public static class TWModifAssets
    {
        private const string ShortPath = "TreeWallMod.Resources.ModifAssets";
        public static LoadableAsset<Sprite> HeadlessModifierSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Headless.png");
        public static LoadableAsset<Sprite> WEEEModifierSprite { get; } = new LoadableResourceAsset($"{ShortPath}.WEEE.png");

        public static LoadableAsset<Sprite> StrechSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Strech.png");
    }
}
