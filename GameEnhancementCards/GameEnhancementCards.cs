using BepInEx;
using GameEnhancementCards.Card.Impl;
using GameEnhancementCards.Utils;
using HarmonyLib;
using Jotunn.Utils;
using UnboundLib.Cards;
using UnboundLib.GameModes;

namespace GameEnhancementCards
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.willuwontu.rounds.managers", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class GameEnhancementCards : BaseUnityPlugin
    {
        private const string ModId = "ot.dan.rounds.gameenhancementcards";
        private const string ModName = "Game Enhancement Cards";
        public const string Version = "2.1.1";
        public const string ModInitials = "GEC";
        public static GameEnhancementCards instance { get; private set; }

        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
            AssetUtils.LoadAssetBundleFromResources("", typeof(GameEnhancementCards).Assembly);
            AssetUtils.LoadAssetBundleFromResources("", typeof(GameEnhancementCards).Assembly);
        }

        void Start()
        {
            instance = this;

            // GameModeManager.AddHook(GameModeHooks.HookInitEnd, GameActions.InitEnd);

            GameModeManager.AddHook(GameModeHooks.HookGameStart, GameActions.GameStart);
            GameModeManager.AddHook(GameModeHooks.HookGameEnd, GameActions.GameEnd);

            GameModeManager.AddHook(GameModeHooks.HookPickStart, GameActions.PickStart);
            GameModeManager.AddHook(GameModeHooks.HookPickEnd, GameActions.PickEnd);

            GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, GameActions.PlayerPickStart);

            // Updradable cards (Probably separate card pack)

            // Stealing cards
            CustomCard.BuildCard<Bully>();
            CustomCard.BuildCard<Thief>();
            CustomCard.BuildCard<Mafia>();

            // Market cards needs logic done
            //CustomCard.BuildCard<Stock>();
            //CustomCard.BuildCard<Crypto>();
            //CustomCard.BuildCard<Nft>();

            // Removing cards
            CustomCard.BuildCard<MissClick>();
            //CustomCard.BuildCard<Negate>(cardInfo => CardController.LoadCard(cardInfo)); //Fix needed

            // Replacing cards
            CustomCard.BuildCard<Replace>();
            CustomCard.BuildCard<Spice>();

            // No cathegory cards
            CustomCard.BuildCard<AnotherChance>();
            CustomCard.BuildCard<Agreed>();
            CustomCard.BuildCard<GoodGuy>();
            CustomCard.BuildCard<Rebalance>();
            CustomCard.BuildCard<Ticket>();

            // Ticket redeeming cards
            CustomCard.BuildCard<PrizeBooth>();
            CustomCard.BuildCard<SketchyTrader>();
            CustomCard.BuildCard<HellishDeals>();
            CustomCard.BuildCard<GodOfTickets>();

            //CustomCard.BuildCard<OneMoreGun>(); Adds a card that makes you shoot more stuff but around you so first pick will be one shot in front and one shot in the back
            //CustomCard.BuildCard<ActualHoming>(); Homing that doesn't look at you as target
            // I am the picker (25% to have one more pick each round)
            // My bullets (The bullets you block activate your card effects)

            //Needs rethinking probably bad idea
            //CustomCard.BuildCard<Disable>();
            //CustomCard.BuildCard<DisableLite>();
        }
    }
}

