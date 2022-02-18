using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using GameEnhancementCards.Mono;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;
using WillsWackyManagers.Utils;
using static ModdingUtils.Utils.Cards;

namespace GameEnhancementCards.Util
{
    public static class CardController
    {
        private static readonly System.Random Random = new System.Random();

        private static Color _firstColor = new Color(1f, 0f, 0.414f, 0.91f);
        private static Color _secondColor = new Color(1f, 0f, 0.014f, 1f);

        private static Color _triangleColor = new Color(0.90f, 0.90f, 0.90f, 0.75f);
        private static Color _textColor = new Color(1f, 1f, 1f, 0.92f);

        private static readonly HashSet<CardCategory> NoPickCategories = new HashSet<CardCategory>();

        static CardController()
        {
            NoPickCategories.Add(RerollManager.instance.NoFlip);
            NoPickCategories.Add(CustomCardCategories.instance.CardCategory("CardManipulation"));
            NoPickCategories.Add(CustomCardCategories.instance.CardCategory("NoRemove"));
        }

        public static void CallGodOfTickets(Player player)
        {
            Unbound.Instance.StartCoroutine(HandleGodOfTickets(player));
        }

        private static IEnumerator HandleGodOfTickets(Player player)
        {
            yield return Unbound.Instance.StartCoroutine(CardController.CleanseCursesOfPlayers());
            List<ChanceCard> chanceCards = new List<ChanceCard> { new ChanceCard(Rarity.RARE, 100) };
            CallTicketRedeemer(player, chanceCards);
        }

        private static IEnumerator CleanseCursesOfPlayers()
        {
            yield return WaitFor.Frames(25);
            foreach (Player player in PlayerManager.instance.players)
            {
                CleanseCursesOfPlayer(player);
                yield return WaitFor.Frames(25);
            }
        }

        private static void CleanseCursesOfPlayer(Player player)
        {
            List<CardInfo> playerCurses = GetPlayerCurses(player);
            ModdingUtils.Utils.Cards.instance.RemoveCardsFromPlayer(player, playerCurses.ToArray(), SelectionType.Newest);
        }

        private static List<CardInfo> GetPlayerCurses(Player player)
        {
            return player.data.currentCards.Where(card => card.categories.Contains(CurseManager.instance.curseCategory)).ToList();
        }

        public static void CallAgreed(Player player)
        {
            var agreedCardsList = GameActions.FinishedPickCards;
            agreedCardsList.RemoveAll(HasBlacklistedCategories);

            var agreedCards = agreedCardsList.ToArray();
            int amount = agreedCards.Length;
            var floatArray = Enumerable.Repeat(2f, amount).ToArray();
            ModdingUtils.Utils.Cards.instance.AddCardsToPlayer(player, agreedCards, false, Enumerable.Repeat("", amount).ToArray(), floatArray, floatArray, true);
        }

        public static void CallTicketRedeemer(Player player, List<ChanceCard> percentageCards)
        {
            var playerTickets = player.data.currentCards
                .Select((value, index) => new { value, index })
                .Where(a => string.Equals(a.value.cardName, "Ticket"))
                .Select(a => a.index).ToList();

            Unbound.Instance.StartCoroutine(HandleTicketRedeemer(player, percentageCards, playerTickets));
        }

        public static IEnumerator HandleTicketRedeemer(Player player, List<ChanceCard> percentageCards, List<int> playerTickets)
        {
            yield return WaitFor.Frames(25);
            
            List<CardInfo> rolledCards = new List<CardInfo>();
            List<int> toRemoveTickets = new List<int>();
            foreach (var ticket in playerTickets)
            {
                CardInfo rolledCard = RollWithPercentages(player, percentageCards);
                rolledCards.Add(rolledCard);
                if (rolledCard == null)
                {
                    toRemoveTickets.Add(ticket);
                }
            }
            yield return ReplacePlayerCards(player, playerTickets, rolledCards);
            if (toRemoveTickets.Count > 0)
            {
                yield return ModdingUtils.Utils.Cards.instance.RemoveCardsFromPlayer(player, toRemoveTickets.ToArray());
                //UnityEngine.Debug.Log($"Removed tickets {toRemoveTickets.Count}");
            }
            
        }

        public static IEnumerable<Tuple<T, int>> FindDuplicates<T>(IEnumerable<T> data)
        {
            var hashSet = new HashSet<T>();
            int index = 0;
            foreach (var item in data)
            {
                if (hashSet.Contains(item))
                {
                    yield return Tuple.Create(item, index);
                }
                else
                {
                    hashSet.Add(item);
                }
                index++;
            }
        }

        private static CardInfo RollWithPercentages(Player player, List<ChanceCard> percentageCards)
        {
            int totalPercentage = percentageCards.Sum(t => t.Chance);
            int randomNumber = Random.Next(0, totalPercentage) + 1;

            int accumulatedProbability = 0;
            foreach (var card in percentageCards)
            {
                accumulatedProbability += card.Chance;
                if (randomNumber > accumulatedProbability) continue;
                CardInfo cardInfo = card.GetCardInfo(player);
                return cardInfo;
            }

            return null;
        }

        // public static void CallNegate()
        // {
        //     UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Negate card called.");
        //     foreach (Player player in PlayerManager.instance.players)
        //     {
        //         var lastCards = GameActions.lastRoundCards[player.playerID];
        //         UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Negate card called for player {player.playerID} for {lastCards.Count} cards {lastCards}.");
        //         foreach (CardInfo card in lastCards)
        //         {
        //             UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Negate card removed {card.cardName}.");
        //         }
        //         ModdingUtils.Utils.Cards.instance.RemoveCardsFromPlayer(player, lastCards.ToArray(), SelectionType.Newest);
        //     }
        // }

        public static void CallRebalance()
        {
            Unbound.Instance.StartCoroutine(HandleRebalace());
        }

        public static IEnumerator HandleRebalace()
        {
            var players = PlayerManager.instance.players;
            List<CardInfo> cardsToBalance = new List<CardInfo>();
            Dictionary<Player, List<CardInfo>> playerCardsMap = new Dictionary<Player, List<CardInfo>>();

            foreach (Player player in players)
            {
                playerCardsMap[player] = new List<CardInfo>();
                cardsToBalance.AddRange(player.data.currentCards);
                cardsToBalance.RemoveAll(HasBlacklistedCategories);
                ModdingUtils.Utils.Cards.instance.RemoveAllCardsFromPlayer(player, true);
            }

            cardsToBalance = GetRandomElements(cardsToBalance, cardsToBalance.Count);

            int playerIndex = 0;

            List<CardInfo> notAllowedCards = new List<CardInfo>();
            foreach (CardInfo card in cardsToBalance)
            {
                Player player = players.ElementAt(playerIndex);
                playerCardsMap[player].Add(card);

                if (ModdingUtils.Utils.Cards.instance.PlayerIsAllowedCard(player, card))
                {
                    ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, card, false, "", 2f, 2f, true);
                    yield return WaitFor.Frames(25);
                }
                else
                {
                    notAllowedCards.Add(card);
                }

                if (playerIndex < (players.Count - 1))
                {
                    playerIndex++;
                    continue;
                }
                playerIndex = 0;
            }

            foreach (CardInfo card in notAllowedCards)
            {
                bool assigned = false;
                players = GetRandomElements(players, players.Count);
                foreach (var player in players.Where(player => !assigned).Where(player => ModdingUtils.Utils.Cards.instance.PlayerIsAllowedCard(player, card)))
                {
                    ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, card, false, "", 2f, 2f, true);
                    assigned = true;
                    yield return WaitFor.Frames(25);
                }
            }
        }

        public static void ReplacePlayerCard(Player player, CardInfo oldCard, CardInfo newCard)
        {
            List<CardInfo> playerCards = player.data.currentCards;
            if (oldCard == null) return;

            if (newCard != null)
            {
                Unbound.Instance.ExecuteAfterFrames(25, () =>
                    {
                        // UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Replacing {oldCard} with {newCard}.");
                        Unbound.Instance.StartCoroutine(ModdingUtils.Utils.Cards.instance.ReplaceCard(player, playerCards.IndexOf(oldCard), newCard, "", 2f, 2f, true));
                    }
                );
            }
            else
            {
                // UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Removing {oldCard}.");
                ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(player, oldCard, SelectionType.Random);
            }
        }

        public static IEnumerator ReplacePlayerCards(Player player, List<int> playerTicketIndexes, List<CardInfo> newCards)
        {
            yield return Unbound.Instance.StartCoroutine(ModdingUtils.Utils.Cards.instance.ReplaceCards(player, playerTicketIndexes.ToArray(), newCards.ToArray(), null));
        }

        public static void GivePlayersCard(Player callPlayer, PlayerType playerType, Rarity rarity)
        {
            List<Player> players = new List<Player>();
            switch (playerType)
            {
                case (PlayerType.TEAM):
                    {
                        players.AddRange(PlayerManager.instance.players.Where(player => callPlayer.teamID.Equals(player.teamID)));
                        break;
                    }
                case (PlayerType.ENEMY):
                    {
                        players.AddRange(PlayerManager.instance.players.Where(player => !callPlayer.teamID.Equals(player.teamID)));
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
            }

            foreach (Player player in players)
            {
                GivePlayerCard(player, rarity);
            }
        }

        public static void GivePlayersCard(Rarity rarity)
        {
            foreach (Player player in PlayerManager.instance.players)
            {
                GivePlayerCard(player, rarity);
            }
        }

        public static void GivePlayerCard(Player player, Rarity rarity)
        {
            var randomCard = RandomCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, rarity);
            // UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Giving Random card {randomCard} to player {player.playerID}.");
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, randomCard, false, "", 2f, 2f, true);
        }

        public static void GivePlayersCard(string cardName)
        {
            foreach (Player player in PlayerManager.instance.players)
            {
                GivePlayerCard(player, cardName);
            }
        }

        public static void GivePlayerCard(Player player, string cardName)
        {
            var cardFromName = SearchCard(cardName);
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, cardFromName, false, "", 2f, 2f, true);
        }

        public static void GivePlayerCard(Player player, string cardName, int amount)
        {
            Unbound.Instance.ExecuteAfterFrames(25, () =>
            {
                var cardFromName = SearchCard(cardName);
                CardInfo[] cardsToGive = Enumerable.Repeat(cardFromName, amount).ToArray();
                var floatArray = Enumerable.Repeat(2f, amount).ToArray();
                ModdingUtils.Utils.Cards.instance.AddCardsToPlayer(player, cardsToGive, false,
                    Enumerable.Repeat("", amount).ToArray(), floatArray, floatArray, true);
            });
        }

        public static void CallReplace(Player player)
        {
            Unbound.Instance.StartCoroutine(RandomizePlayerCardAtPosition(player, CardPosition.LAST));
        }

        public static void CallSpice()
        {
            Unbound.Instance.StartCoroutine(RandomizePlayersCardAtPosition(CardPosition.RANDOM));
        }

        public static IEnumerator RandomizePlayersCardAtPosition(CardPosition cardPosition)
        {
            return PlayerManager.instance.players.Select(player => RandomizePlayerCardAtPosition(player, cardPosition)).GetEnumerator();
        }

        public static IEnumerator RandomizePlayerCardAtPosition(Player player, CardPosition cardPosition)
        {
            List<CardInfo> playerCards = player.data.currentCards;
            CardInfo oldCard = PlayerCardAtPosition(playerCards, cardPosition);
            if (oldCard is null) yield break;

            var randomCard = RandomCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, CardRarity(oldCard, false));
            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] Replacing card {removeCard.cardName} from player {player.playerID} with {randomCard.cardName}.");
            yield return WaitFor.Frames(25);
            yield return Unbound.Instance.StartCoroutine(ModdingUtils.Utils.Cards.instance.ReplaceCard(player, playerCards.IndexOf(oldCard), randomCard, "", 2f, 2f, true));
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
            CardInfo removeCard = PlayerCardAtPosition(playerCards, cardPosition);
            if (removeCard != null)
            {
                Unbound.Instance.ExecuteAfterFrames(10, () =>
                {
                    ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(player, playerCards.IndexOf(removeCard));
                }
                );
            }
        }

        public static void StealPlayersCardOfRarity(Player player, CardInfo.Rarity rarity, PlayerAmount playerAmount)
        {
            List<Player> players = new List<Player>(PlayerManager.instance.players);
            players.Remove(player);
            var ordered = players.OrderBy(order => Random.Next());

            switch (playerAmount)
            {
                case PlayerAmount.ONE:
                    {
                        players = GetRandomElements(ordered, 1);
                        break;
                    }
                case PlayerAmount.HALF:
                    {
                        players = GetRandomElements(ordered, players.Count / 2);
                        break;
                    }
                case PlayerAmount.ALL:
                    {
                        players = GetRandomElements(ordered, players.Count);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerAmount), playerAmount, null);
            }

            foreach (Player fromPlayer in players)
            {
                StealPlayerCardOfRarity(player, fromPlayer, rarity);
            }
        }

        public static bool HasBlacklistedCategories(CardInfo cardInfo)
        {
            return cardInfo.categories
                .Any(cardCategory => NoPickCategories
                    .Any(checkCategory => cardCategory == checkCategory));
        }

        public static void StealPlayerCardOfRarity(Player player, Player fromPlayer, CardInfo.Rarity rarity)
        {
            List<CardInfo> fromPlayerCards = fromPlayer.data.currentCards.Where(x => x.rarity.Equals(rarity) && !HasBlacklistedCategories(x)).ToList();
            if (fromPlayerCards.Count > 0)
            {
                var randomizedCards = GetRandomElements(fromPlayerCards, fromPlayerCards.Count());
                if (randomizedCards.Count > 0)
                {
                    CardInfo stolenCard = randomizedCards.First();
                    if (stolenCard != null)
                    {
                        Unbound.Instance.ExecuteAfterFrames(25, () =>
                            {
                                var cardIndex = fromPlayer.data.currentCards.IndexOf(stolenCard);
                                if (cardIndex >= 0)
                                {
                                    //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] Stealing card {stolenCard.cardName} from player {fromPlayer.playerID} at {cardIndex} with card {fromPlayer.data.currentCards[cardIndex].cardName} and adding it to {player.playerID}.");
                                    ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(fromPlayer, cardIndex);
                                    Unbound.Instance.ExecuteAfterFrames(25, () =>
                                    {
                                        ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, stolenCard, false, "",
                                            2f,
                                            2f, true);
                                    });
                                    return;
                                }
                                GiveConsolationPrize(player, rarity);
                            }
                        );
                    }
                    else
                    {
                        GiveConsolationPrize(player, rarity);
                    }
                }
                else
                {
                    GiveConsolationPrize(player, rarity);
                }
            }
            else
            {
                GiveConsolationPrize(player, rarity);
            }
        }

        public static void GiveConsolationPrize(Player player, CardInfo.Rarity rarity)
        {
            int amount;
            switch (rarity)
            {
                case (CardInfo.Rarity.Common):
                    {
                        amount = 1;
                        break;
                    }
                case (CardInfo.Rarity.Uncommon):
                    {
                        amount = 2;
                        break;
                    }
                case (CardInfo.Rarity.Rare):
                    {
                        amount = 3;
                        break;
                    }
                default:
                    amount = 0;
                    break;
            }
            GivePlayerCard(player, "Ticket", amount);
        }

        private static List<T> GetRandomElements<T>(this IEnumerable<T> list, int elementsCount)
        {
            return list.OrderBy(arg => Guid.NewGuid()).Take(elementsCount).ToList();
        }

        private static CardInfo PlayerCardAtPosition(IReadOnlyCollection<CardInfo> playerCards, CardPosition cardPosition)
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
                        int randomCard = Random.Next(0, playerCards.Count - 1);
                        return playerCards.ElementAt(randomCard);
                    }
            }
            return null;
        }

        public static CardInfo RandomCard(Player checkPlayer, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats, Rarity rarity)
        {
            bool SearchConditions(CardInfo cardInfo, Player player, Gun g, GunAmmo ga, CharacterData d, HealthHandler h, Gravity gr, Block b, CharacterStatModifiers s)
            {
                if (CardRarity(cardInfo, false) != rarity)
                {
                    return false;
                }

                return !HasBlacklistedCategories(cardInfo);
            }

            return ModdingUtils.Utils.Cards.instance.GetRandomCardWithCondition(checkPlayer, gun, gunAmmo, data, health, gravity, block, characterStats, SearchConditions);
        }

        private static CardInfo SearchCard(string cardName)
        {
            var foundCard = ModdingUtils.Utils.Cards.all.Where(card => card.cardName == cardName).ToArray()[0];
            return foundCard;
        }

        public enum Rarity
        {
            NONE,

            COMMON,
            UNCOMMON,
            RARE,

            LEGENDARY,

            COMMON_CURSE,
            UNCOMMON_CURSE,
            RARE_CURSE,
            CURSE
        }

        public enum CardPosition
        {
            LAST,
            FIRST,
            RANDOM
        }

        public enum PlayerType
        {
            TEAM,
            ENEMY
        }

        public enum PlayerAmount
        {
            ONE,
            HALF,
            ALL
        }

        public static Rarity CardRarity(CardInfo cardInfo, bool specific)
        {
            Rarity rarity;

            if (cardInfo.categories.Contains(CurseManager.instance.curseCategory))
            {
                if (specific)
                {
                    switch (cardInfo.rarity)
                    {
                        case CardInfo.Rarity.Rare:
                            rarity = Rarity.RARE_CURSE;
                            break;
                        case CardInfo.Rarity.Uncommon:
                            rarity = Rarity.UNCOMMON_CURSE;
                            break;
                        case CardInfo.Rarity.Common:
                        default:
                            rarity = Rarity.COMMON_CURSE;
                            break;
                    }
                }
                else
                {
                    rarity = Rarity.CURSE;
                }
            }
            else
            {
                switch (cardInfo.rarity)
                {
                    case CardInfo.Rarity.Rare:
                        rarity = Rarity.RARE;
                        break;
                    case CardInfo.Rarity.Uncommon:
                        rarity = Rarity.UNCOMMON;
                        break;
                    case CardInfo.Rarity.Common:
                    default:
                        rarity = Rarity.COMMON;
                        break;
                }
            }

            return rarity;
        }

        public static GameObject FindObjectInChildren(GameObject gameObject, string gameObjectName)
        {
            Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
            return (from item in children where item.name == gameObjectName select item.gameObject).FirstOrDefault();
        }

        public static List<GameObject> FindObjectsInChildren(GameObject gameObject, string gameObjectName, bool precise)
        {
            List<GameObject> returnObjects = new List<GameObject>();
            Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform item in children)
            {
                if (precise)
                {
                    if (item.gameObject.name.Equals(gameObjectName))
                    {
                        returnObjects.Add(item.gameObject);
                    }
                }
                else
                {
                    if (item.gameObject.name.Contains(gameObjectName))
                    {
                        returnObjects.Add(item.gameObject);
                    }
                }
            }

            return returnObjects;
        }

        private static CardInfo RandomCardRoll(IReadOnlyList<CardInfo> cards)
        {
            return !(cards.Count > 0) ? null : cards[Random.Next(cards.Count)];
        }

        public static void LoadCard(CustomCard customCard)
        {
            LoadCard(customCard, Rarity.NONE, false);
        }

        public static void LoadCard(CustomCard customCard, bool manipulation)
        {
            LoadCard(customCard, Rarity.NONE, manipulation);
        }

        public static void LoadCard(CustomCard customCard, Rarity rarity)
        {
            LoadCard(customCard, rarity, false);
        }

        public static void LoadCard(CustomCard customCard, Rarity rarity, bool manipulation)
        {
            if (customCard.gameObject.GetComponent<CustomCardHandler>() != null) return;

            GameObject cardFront = customCard.gameObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
            cardFront.AddComponent<CustomCardHandler>();

            if (rarity == Rarity.LEGENDARY)
            {
                cardFront.AddComponent<Legendary>();
            }

            if (manipulation)
            {
                customCard.cardInfo.categories = NoPickCategories.ToArray();
            }

            var faceObject = FindObjectInChildren(customCard.gameObject, "Face");
            UnityEngine.Object.Destroy(faceObject);

            var frontObject = FindObjectInChildren(customCard.gameObject, "Back");
            var textObject = new GameObject("BackText");
            textObject.transform.SetParent(frontObject.transform);
            textObject.transform.localPosition = new Vector3(0, 0, 0);
            textObject.transform.localScale = new Vector3(1, 1, 1);

            textObject.AddComponent<RectTransform>();
            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMax = new Vector2(0, 0);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.localRotation = new Quaternion(0, 1, 0, 0);

            textObject.AddComponent<TextMeshProUGUI>();
            var backText = textObject.GetComponent<TextMeshProUGUI>();
            backText.text = "GEC";
            backText.fontSize = 175;
            backText.alignment = TextAlignmentOptions.Center;
        }
    }

    internal static class WaitFor
    {
        public static IEnumerator Frames(int frameCount)
        {
            if (frameCount <= 0)
            {
                throw new ArgumentOutOfRangeException("frameCount", "Cannot wait for less that 1 frame");
            }

            while (frameCount > 0)
            {
                frameCount--;
                yield return null;
            }
        }
    }

    public class ChanceCard
    {
        public CardController.Rarity type { get; }
        public int Chance { get; }

        public ChanceCard(CardController.Rarity type, int chance)
        {
            this.type = type;
            this.Chance = chance;
        }

        public CardInfo GetCardInfo(Player player)
        {
            switch (type)
            {
                case (CardController.Rarity.COMMON):
                    {
                        return CardController.RandomCard(player, player.data.weaponHandler.gun,
                            player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data,
                            player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats,
                            CardController.Rarity.COMMON);
                    }
                case (CardController.Rarity.UNCOMMON):
                    {
                        return CardController.RandomCard(player, player.data.weaponHandler.gun,
                            player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data,
                            player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats,
                            CardController.Rarity.UNCOMMON);
                    }
                case (CardController.Rarity.RARE):
                    {
                        return CardController.RandomCard(player, player.data.weaponHandler.gun,
                            player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data,
                            player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats,
                            CardController.Rarity.RARE);
                    }
                case (CardController.Rarity.CURSE):
                    {
                        return CardController.RandomCard(player, player.data.weaponHandler.gun,
                            player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data,
                            player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats,
                            CardController.Rarity.CURSE);
                    }
                default:
                case CardController.Rarity.NONE:
                    break;
                case CardController.Rarity.LEGENDARY:
                    break;
                case CardController.Rarity.COMMON_CURSE:
                    break;
                case CardController.Rarity.UNCOMMON_CURSE:
                    break;
                case CardController.Rarity.RARE_CURSE:
                    break;
            }
            return null;
        }
    }
}
