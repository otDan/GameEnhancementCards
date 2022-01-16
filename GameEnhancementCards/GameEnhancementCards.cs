using BepInEx;
using UnboundLib;
using UnboundLib.Cards;
using GameEnhancementCards.Cards;
using HarmonyLib;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using Jotunn.Utils;

namespace GameEnhancementCards
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class GameEnhancementCards : BaseUnityPlugin
    {
        private const string ModId = "ot.dan.rounds.GameEnhancementCards";
        private const string ModName = "GameEnhancementCards";
        public const string Version = "0.0.2";
        public const string ModInitials = "GEC";
        public static GameEnhancementCards instance { get; private set; }

        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
            AssetUtils.LoadAssetBundleFromResources("", typeof(GameEnhancementCards).Assembly);
        }
        void Start()
        {
            instance = this;

            //Stealing cards
            CustomCard.BuildCard<Bully>();
            CustomCard.BuildCard<Thief>();
            CustomCard.BuildCard<Mafia>();

            //Market cards
            CustomCard.BuildCard<Stock>();
            CustomCard.BuildCard<Crypto>();
            CustomCard.BuildCard<Nft>();

            //Removing cards
            CustomCard.BuildCard<BadChoice>();
            CustomCard.BuildCard<Negate>();

            //Replacing cards
            CustomCard.BuildCard<Replace>();

            //No cathegory cards
            CustomCard.BuildCard<Rebalance>();
            CustomCard.BuildCard<Disable>();
            CustomCard.BuildCard<DisableLite>();
        }
    }
}

