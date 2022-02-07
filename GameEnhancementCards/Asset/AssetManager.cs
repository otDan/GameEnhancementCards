using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameEnhancementCards.Asset
{
    public static class AssetManager
    {
        private static readonly AssetBundle Bundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("gec_assets", typeof(GameEnhancementCards).Assembly);

        public static GameObject BullyCard = Bundle.LoadAsset<GameObject>("C_Bully");
    }
}
