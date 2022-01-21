using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib.GameModes;

namespace GameEnhancementCards.Utils
{
    public static class GameActions
    {
        private static Dictionary<Player, List<CardInfo>> usedRoundCards = new Dictionary<Player, List<CardInfo>>();
        public static Dictionary<Player, List<CardInfo>> lastRoundCards { get; private set; }
        private static bool firstPick = true;

        static GameActions()
        {
            lastRoundCards = new Dictionary<Player, List<CardInfo>>();
        }

        internal static IEnumerator PickEnd(IGameModeHandler gameModeHandler)
        {
            if (firstPick) 
            {
                foreach (Player player in PlayerManager.instance.players)
                {
                    if (!lastRoundCards.ContainsKey(player))
                    {
                        lastRoundCards[player] = new List<CardInfo>();
                        
                    }
                    if (!usedRoundCards.ContainsKey(player))
                    {
                        usedRoundCards[player] = new List<CardInfo>();

                    }

                    lastRoundCards[player].AddRange(player.data.currentCards);
                }
                firstPick = false;
            }
            else
            {
                foreach (Player player in PlayerManager.instance.players)
                {
                    if (!lastRoundCards.ContainsKey(player))
                    {
                        lastRoundCards[player] = new List<CardInfo>();

                    }
                    if (!usedRoundCards.ContainsKey(player))
                    {
                        usedRoundCards[player] = new List<CardInfo>();

                    }

                    usedRoundCards[player].AddRange(player.data.currentCards.Where(x => lastRoundCards[player].Contains(x)));
                    lastRoundCards.Clear();
                    lastRoundCards[player].AddRange(player.data.currentCards.Where(x => !usedRoundCards[player].Contains(x)));
                }
            }

            yield break;
        }

        internal static IEnumerator GameStart(IGameModeHandler gameModeHandler)
        {
            foreach (var player in PlayerManager.instance.players)
            {

            }

            yield break;
        }

        internal static IEnumerator GameEnd(IGameModeHandler gameModeHandler)
        {
            lastRoundCards = new Dictionary<Player, List<CardInfo>>();
            firstPick = true;
            yield break;
        }
    }
}
