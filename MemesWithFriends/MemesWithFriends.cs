using BepInEx;
using UnboundLib;
using UnboundLib.Cards;
using MemesWithFriends.Cards;
using HarmonyLib;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;

namespace MemesWithFriends
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class MemesWithFriends : BaseUnityPlugin
    {
        private const string ModId = "ot.dan.rounds.MemesWithFriends";
        private const string ModName = "MemesWithFriends";
        public const string Version = "0.0.0"; // What version are we on (major.minor.patch)?

        public static MemesWithFriends instance { get; private set; }
        public const string ModInitials = "MWF";

        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            CustomCard.BuildCard < Template>();
        }
    }
}

