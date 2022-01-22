using BepInEx;
using UnboundLib;
using UnboundLib.Cards;
using GameEnhancementCards.Cards;
using HarmonyLib;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using Jotunn.Utils;
using GameEnhancementCards.Utils;
using System.Collections;
using UnboundLib.GameModes;

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
        public const string Version = "1.1.3";
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

            GameModeManager.AddHook(GameModeHooks.HookGameStart, GameActions.GameStart);
            GameModeManager.AddHook(GameModeHooks.HookGameEnd, GameActions.GameEnd);
            GameModeManager.AddHook(GameModeHooks.HookPickEnd, GameActions.PickEnd);

            //Updradable cards (Probably separate card pack)

            //Stealing cards
            CustomCard.BuildCard<Bully>();
            CustomCard.BuildCard<Thief>();
            CustomCard.BuildCard<Mafia>();

            //Market cards needs logic done
            //CustomCard.BuildCard<Stock>();
            //CustomCard.BuildCard<Crypto>();
            //CustomCard.BuildCard<Nft>();

            //Removing cards
            //CustomCard.BuildCard<MissClick>(); Needs a good stat improvement to have it balanced
            //CustomCard.BuildCard<Negate>(cardInfo => CardsManager.LoadCard(cardInfo)); //Fix needed

            //Replacing cards
            CustomCard.BuildCard<Replace>();
            CustomCard.BuildCard<Spice>();

            //No cathegory cards
            CustomCard.BuildCard<AnotherChance>();
            //CustomCard.BuildCard<Agreed>(); TODO
            //CustomCard.BuildCard<GoodGuy>(); TODO
            CustomCard.BuildCard<Rebalance>();

            //CustomCard.BuildCard<OneMoreGun>(); Adds a card that makes you shoot more stuff but around you so first pick will be one shot in front and one shot in the back
            //CustomCard.BuildCard<ActualHoming>(); Homing that doesn't look at you as target

            //Needs rethinking probably bad idea
            //CustomCard.BuildCard<Disable>();
            //CustomCard.BuildCard<DisableLite>();
        }
    }
}

