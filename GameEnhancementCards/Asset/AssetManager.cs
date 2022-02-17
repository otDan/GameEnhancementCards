using UnityEngine;

namespace GameEnhancementCards.Asset
{
    public static class AssetManager
    {
        private static readonly AssetBundle Bundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("gec_assets", typeof(GameEnhancementCards).Assembly);
        private static readonly AssetBundle PlaceholderBundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("otdanassets", typeof(GameEnhancementCards).Assembly);

        public static GameObject BullyCard = Bundle.LoadAsset<GameObject>("C_Bully");
        public static GameObject ThiefCard = Bundle.LoadAsset<GameObject>("C_Thief");
        public static GameObject TicketCard = Bundle.LoadAsset<GameObject>("C_Ticket");

        //Placeholder assets

        public static GameObject AgreedCard = PlaceholderBundle.LoadAsset<GameObject>("C_AGREED");
        public static GameObject AnotherChanceCard = PlaceholderBundle.LoadAsset<GameObject>("C_ANOTHERCHANCE");
        public static GameObject GodOfTicketsCard = PlaceholderBundle.LoadAsset<GameObject>("C_GODOFTICKETS");
        public static GameObject GoodGuyCard = PlaceholderBundle.LoadAsset<GameObject>("C_GOODGUY");
        public static GameObject HellishDealsCard = PlaceholderBundle.LoadAsset<GameObject>("C_HELLISHDEAL");
        public static GameObject MafiaCard = PlaceholderBundle.LoadAsset<GameObject>("C_MAFIA");
        public static GameObject MissClickCard = PlaceholderBundle.LoadAsset<GameObject>("C_MISSCLICK");
        public static GameObject PrizeBoothCard = PlaceholderBundle.LoadAsset<GameObject>("C_PRIZEBOOTH");
        public static GameObject RebalanceCard = PlaceholderBundle.LoadAsset<GameObject>("C_REBALANCE");
        public static GameObject ReplaceCard = PlaceholderBundle.LoadAsset<GameObject>("C_REPLACE");
        public static GameObject SketchyTraderCard = PlaceholderBundle.LoadAsset<GameObject>("C_SKETCHYTRADER");
        public static GameObject SpiceCard = PlaceholderBundle.LoadAsset<GameObject>("C_SPICE");
    }
}
