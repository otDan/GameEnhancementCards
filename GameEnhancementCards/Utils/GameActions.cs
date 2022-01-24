using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib.Cards;
using UnboundLib.GameModes;

namespace GameEnhancementCards.Utils
{
    public static class GameActions
    {
        private static Dictionary<int, List<CardInfo>> usedRoundCards = new Dictionary<int, List<CardInfo>>();
        public static Dictionary<int, List<CardInfo>> lastRoundCards { get; private set; }
        private static bool firstPick = true;

        static GameActions()
        {
            lastRoundCards = new Dictionary<int, List<CardInfo>>();
        }

        internal static IEnumerator PickEnd(IGameModeHandler gameModeHandler)
        {
            try {
                CardsManager.loadedArt = new List<CustomCard>();

                if (firstPick)
                {
                    foreach (Player player in PlayerManager.instance.players)
                    {
                        if (!lastRoundCards.ContainsKey(player.playerID))
                        {
                            lastRoundCards[player.playerID] = new List<CardInfo>(player.data.currentCards);
                        }
                        if (!usedRoundCards.ContainsKey(player.playerID))
                        {
                            usedRoundCards[player.playerID] = new List<CardInfo>();
                        }
                    }
                    firstPick = false;
                }
                else
                {
                    foreach (Player player in PlayerManager.instance.players)
                    {
                        if (!lastRoundCards.ContainsKey(player.playerID))
                        {
                            lastRoundCards[player.playerID] = new List<CardInfo>();
                        }
                        if (!usedRoundCards.ContainsKey(player.playerID))
                        {
                            usedRoundCards[player.playerID] = new List<CardInfo>();
                        }

                        usedRoundCards[player.playerID].AddRange(player.data.currentCards.Where(x => lastRoundCards[player.playerID].Contains(x)));
                        foreach (var usedCard in usedRoundCards[player.playerID])
                        {
                            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] {player.playerID} Used card {usedCard}.");
                        }

                        List<int> keys = new List<int>(lastRoundCards.Keys);
                        foreach (int key in keys)
                        {
                            lastRoundCards[key] = new List<CardInfo>();
                        }

                        lastRoundCards[player.playerID].AddRange(player.data.currentCards.Where(x => !usedRoundCards[player.playerID].Contains(x)));
                        foreach (var lastCard in lastRoundCards[player.playerID])
                        {
                            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] {player.playerID} Last card {lastCard}.");
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Exception happened {exception}.");
            }

            yield break;
        }

        internal static IEnumerator GameEnd(IGameModeHandler gameModeHandler)
        {
            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Game ended.");
            lastRoundCards = new Dictionary<int, List<CardInfo>>();
            firstPick = true;
            yield break;
        }

        internal static IEnumerator GameStart(IGameModeHandler gameModeHandler)
        {
            try
            {
                CardsManager.loadedArt = new List<CustomCard>();
            }
            catch (Exception exception)
            {
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Exception happened {exception}.");
            }

            yield break;
        }

        internal static IEnumerator InitEnd(IGameModeHandler gameModeHandler)
        {
            try
            {
                CardsManager.loadedArt = new List<CustomCard>();
            }
            catch (Exception exception)
            {
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Exception happened {exception}.");
            }
            yield break;
        }
    }
}
