using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.GameModes;

namespace GameEnhancementCards.Util
{
    public static class GameActions
    {
        // private static Dictionary<int, List<CardInfo>> usedRoundCards = new Dictionary<int, List<CardInfo>>();
        // public static Dictionary<int, List<CardInfo>> lastRoundCards { get; private set; }
        private static bool _firstPick = true;
        private static int _lastPicker = -1;
        private static List<CardInfo> _prePickCards;
        public static List<CardInfo> FinishedPickCards { get; private set; }

        static GameActions()
        {
            // lastRoundCards = new Dictionary<int, List<CardInfo>>();
        }

        internal static IEnumerator PlayerPickStart(IGameModeHandler gameModeHandler)
        {
            try
            {
                Unbound.Instance.StartCoroutine(PickerWait());
            }
            catch (Exception)
            {
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Exception {exception}.");
            }
            yield break;
        }

        private static IEnumerator PickerWait()
        {
            while (CardChoice.instance.pickrID == -1)
            {
                yield return WaitFor.Frames(1);
            }

            int choicePlayer = CardChoice.instance.pickrID;
            // UnityEngine.Debug.Log($"LastPicker: {lastPicker} choicePlayer: {choicePlayer}");

            if (_lastPicker != -1)
            {
                // printCards(prePickCards, "prepick");
                FinishedPickCards = PlayerManager.instance.players[_lastPicker].data.currentCards.ToList();
                FinishedPickCards.RemoveAll(card => _prePickCards.Contains(card));
                // printCards(finishedPickCards, "finished");
                // printCards(PlayerManager.instance.players[lastPicker].data.currentCards.ToList(), "player");
                // UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] {lastPicker} Finished picking.number of prepick {prePickCards.Count} number of picked cards {finishedPickCards.Count} total number of cards {PlayerManager.instance.players[lastPicker].data.currentCards.Count}");
            }

            if (_lastPicker == choicePlayer) yield break;

            _lastPicker = choicePlayer;
            _prePickCards = PlayerManager.instance.players[choicePlayer].data.currentCards.ToList();
            // UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] {choicePlayer} Started picking, number of cards {prePickCards.Count}");


            // prePickCards = new List<CardInfo>()
            // Unbound.Instance.StartCoroutine(PickerFinish(choicePlayer));
        }

        // private static IEnumerator PickerFinish(int playerID)
        // {
        //     while (CardChoice.instance.pickrID == playerID)
        //     {
        //         yield return WaitFor.Frames(1);
        //     }
        //     // int choicePlayer = CardChoice.instance.pickrID;
        // }

        // private static void printCards(List<CardInfo> cards, String type)
        // {
        //     cards.ForEach(card =>
        //     {
        //         UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] {type} Card in list: {card.cardName}");
        //     });
        // }

        internal static IEnumerator PickStart(IGameModeHandler gameModeHandler)
        {
            try
            {
                // Get cards of all players
            }
            catch (Exception)
            {
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Exception {exception}.");
            }
            yield break;
        }

        internal static IEnumerator PickEnd(IGameModeHandler gameModeHandler)
        {
            try
            {
                // CardController.loadedArt = new List<CustomCard>();

                // if (firstPick)
                // {
                //     foreach (Player player in PlayerManager.instance.players)
                //     {
                //         if (!lastRoundCards.ContainsKey(player.playerID))
                //         {
                //             lastRoundCards[player.playerID] = new List<CardInfo>(player.data.currentCards);
                //         }
                //         if (!usedRoundCards.ContainsKey(player.playerID))
                //         {
                //             usedRoundCards[player.playerID] = new List<CardInfo>();
                //         }
                //     }
                //     firstPick = false;
                // }
                // else
                // {
                //     foreach (Player player in PlayerManager.instance.players)
                //     {
                //         if (!lastRoundCards.ContainsKey(player.playerID))
                //         {
                //             lastRoundCards[player.playerID] = new List<CardInfo>();
                //         }
                //         if (!usedRoundCards.ContainsKey(player.playerID))
                //         {
                //             usedRoundCards[player.playerID] = new List<CardInfo>();
                //         }
                //
                //         usedRoundCards[player.playerID].AddRange(player.data.currentCards.Where(x => lastRoundCards[player.playerID].Contains(x)));
                //         foreach (var usedCard in usedRoundCards[player.playerID])
                //         {
                //             //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] {player.playerID} Used card {usedCard}.");
                //         }
                //
                //         List<int> keys = new List<int>(lastRoundCards.Keys);
                //         foreach (int key in keys)
                //         {
                //             lastRoundCards[key] = new List<CardInfo>();
                //         }
                //
                //         lastRoundCards[player.playerID].AddRange(player.data.currentCards.Where(x => !usedRoundCards[player.playerID].Contains(x)));
                //         foreach (var lastCard in lastRoundCards[player.playerID])
                //         {
                //             //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] {player.playerID} Last card {lastCard}.");
                //         }
                //     }
                // }
            }
            catch (Exception exception)
            {
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Exception happened {exception}.");
            }

            yield break;
        }

        internal static IEnumerator GameEnd(IGameModeHandler gameModeHandler)
        {
            try
            {
                _lastPicker = -1;
                _prePickCards = new List<CardInfo>();
                FinishedPickCards = new List<CardInfo>();
            }
            catch (Exception exception)
            {
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Exception happened {exception}.");
            }

            yield break;
        }

        internal static IEnumerator GameStart(IGameModeHandler gameModeHandler)
        {
            try
            {
                _lastPicker = -1;
                _prePickCards = new List<CardInfo>();
                FinishedPickCards = new List<CardInfo>();
            }
            catch (Exception exception)
            {
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Exception happened {exception}.");
            }

            yield break;
        }
        //
        // internal static IEnumerator InitEnd(IGameModeHandler gameModeHandler)
        // {
        //     try
        //     {
        //         // CardController.loadedArt = new List<CustomCard>();
        //     }
        //     catch (Exception exception)
        //     {
        //         //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Exception happened {exception}.");
        //     }
        //     yield break;
        // }
    }
}
