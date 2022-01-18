using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Utils;
using UnityEngine;

namespace GameEnhancementCards.Utils
{
    public static class CardsManager
    {
        private static System.Random random;

        static CardsManager()
        {
            random = new System.Random();
        }

        public static void HandleRebalace()
        {
            var players = PlayerManager.instance.players;
            List<CardInfo> cardsToBalance = new List<CardInfo>();

            foreach (Player player in players)
            {
                cardsToBalance.AddRange(player.data.currentCards);
                ModdingUtils.Utils.Cards.instance.RemoveAllCardsFromPlayer(player, true);
            }

            int cardsPerPlayer = (cardsToBalance.Count - 1) / (players.Count - 1);
            cardsToBalance = GetRandomElements<CardInfo>(cardsToBalance, (cardsToBalance.Count - 1));

            int playerIndex = 0;
            foreach (CardInfo card in cardsToBalance)
            {
                Player player = players.ElementAt(playerIndex);
                Unbound.Instance.ExecuteAfterFrames(10, () =>
                {
                    ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, card, false, "", 2f, 2f, true);
                }
                );
                
                if (playerIndex < (players.Count - 1))
                {
                    playerIndex++;
                    continue;
                }
                playerIndex = 0;
            }
        }

        public static void RandomizePlayersCardAtPosition(CardPosition cardPosition)
        {
            foreach (Player player in PlayerManager.instance.players)
            {
                RandomizePlayerCardAtPosition(player, cardPosition);
            }
        }

        public static void RandomizePlayerCardAtPosition(Player player, CardPosition cardPosition)
        {
            List<CardInfo> playerCards = player.data.currentCards;
            CardInfo oldCard = PlayerCardAtPosition(player, playerCards, cardPosition);
            if (oldCard != null)
            {
                var randomCard = RandomCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, oldCard);
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] Replacing card {removeCard.cardName} from player {player.playerID} with {randomCard.cardName}.");
                Unbound.Instance.ExecuteAfterFrames(10, () =>
                {
                    Unbound.Instance.StartCoroutine(ModdingUtils.Utils.Cards.instance.ReplaceCard(player, playerCards.IndexOf(oldCard), randomCard, "", 2f, 2f, true));
                }
                );
            }
        }

        public static void RemovePlayersCardAtPosition(CardPosition cardPosition)
        {
            foreach (Player player in PlayerManager.instance.players)
            {
                RemovePlayerCardAtPosition(player, cardPosition);
            }
        }

        public static void RemovePlayerCardAtPosition(Player player, CardPosition cardPosition)
        {
            List<CardInfo> playerCards = player.data.currentCards;
            CardInfo removeCard = PlayerCardAtPosition(player, playerCards, cardPosition);
            if (removeCard != null)
            {
                Unbound.Instance.ExecuteAfterFrames(10, () =>
                {
                    ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(player, playerCards.IndexOf(removeCard));
                }
                );
            }
        }

        public static void StealPlayersCardAtPosition(Player player, PlayerAmount playerAmount, CardPosition cardPosition)
        {
            List<Player> players = new List<Player>(PlayerManager.instance.players);
            players.Remove(player);
            players.OrderBy(order => random.Next());

            switch (playerAmount)
            {
                case PlayerAmount.ONE:
                    {
                        players = GetRandomElements<Player>(players, 1);
                        break;
                    }
                case PlayerAmount.HALF:
                    {
                        players = GetRandomElements<Player>(players, (players.Count - 1) / 2);
                        break;
                    }
                case PlayerAmount.ALL:
                    {
                        players = GetRandomElements<Player>(players, players.Count - 1);
                        break;
                    }
            }

            foreach (Player fromPlayer in players)
            {
                StealPlayerCardAtPosition(player, fromPlayer, cardPosition);
            }
        }

        public static void StealPlayerCardAtPosition(Player player, Player fromPlayer, CardPosition cardPosition)
        {
            List<CardInfo> fromPlayerCards = fromPlayer.data.currentCards;
            CardInfo stolenCard = PlayerCardAtPosition(player, fromPlayerCards, cardPosition);
            if (stolenCard != null)
            {
                Unbound.Instance.ExecuteAfterFrames(10, () =>
                {
                    ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(fromPlayer, fromPlayerCards.IndexOf(stolenCard));
                    ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, stolenCard, false, "", 2f, 2f, true);
                }
                );
            }
        }

        private static List<T> GetRandomElements<T>(this IEnumerable<T> list, int elementsCount)
        {
            return list.OrderBy(arg => Guid.NewGuid()).Take(elementsCount).ToList();
        }

        private static CardInfo PlayerCardAtPosition(Player player, List<CardInfo> playerCards, CardPosition cardPosition)
        {
            if (playerCards.Count < 1)
                return null;
            switch (cardPosition)
            {
                case CardPosition.FIRST:
                    {
                        return playerCards.ElementAt(0);
                    }
                case CardPosition.LAST:
                    {
                        return playerCards.ElementAt(playerCards.Count - 1);
                    }
                case CardPosition.RANDOM:
                    {
                        int randomCard = random.Next(0, playerCards.Count - 1);
                        return playerCards.ElementAt(randomCard);
                    }
            }

            return null;
        }

        private static CardInfo RandomCard(Player checkPlayer, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats, CardInfo checkCardInfo)
        {
            Func<CardInfo, Player, Gun, GunAmmo, CharacterData, HealthHandler, Gravity, Block, CharacterStatModifiers, bool> condition = (cardInfo, player, g, ga, d, h, gr, b, s) =>
            {
                var result = true;
                if (CardRarity(cardInfo) != CardRarity(checkCardInfo))
                {
                    result = false;
                }

                return result;
            };

            return ModdingUtils.Utils.Cards.instance.GetRandomCardWithCondition(checkPlayer, gun, gunAmmo, data, health, gravity, block, characterStats, condition);
        }

        private static CardInfo.Rarity CardRarity(CardInfo cardInfo)
        {
            switch (cardInfo.rarity)
            {
                case CardInfo.Rarity.Rare:
                    return CardInfo.Rarity.Rare;
                case CardInfo.Rarity.Uncommon:
                    return CardInfo.Rarity.Uncommon;
                default:
                    return CardInfo.Rarity.Common;
            }
        }

        private static CardInfo RandomCardRoll(CardInfo[] cards)
        {
            if (!(cards.Count() > 0))
            {
                return null;
            }

            return cards[random.Next(cards.Count())];
        }
    }

    public enum CardPosition
    {
        LAST,
        FIRST,
        RANDOM
    }

    public enum PlayerAmount
    {
        ONE,
        HALF,
        ALL
    }
}
