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

namespace GameEnhancementCards.Utils
{
    public static class CardController
    {
        private static readonly System.Random Random = new System.Random();

        private static Color firstColor = new Color(1f, 0f, 0.414f, 0.91f);
        private static Color secondColor = new Color(1f, 0f, 0.014f, 1f);

        private static Color triangleColor = new Color(0.90f, 0.90f, 0.90f, 0.75f);
        private static Color textColor = new Color(1f, 1f, 1f, 0.92f);

        public static void CallGodOfTickets(Player player)
        {
            Unbound.Instance.StartCoroutine(HandleGodOfTickets(player));
        }

        private static IEnumerator HandleGodOfTickets(Player player)
        {
            yield return Unbound.Instance.StartCoroutine(CardController.CleanseCursesOfPlayers());
            List<ChanceCard> chanceCards = new List<ChanceCard>();
            chanceCards.Add(new ChanceCard(CardController.Rarity.RARE, 100));
            CardController.CallTicketRedeemer(player, chanceCards);
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
            List<CardInfo> curses = new List<CardInfo>();
            foreach (var card in player.data.currentCards)
            {
                if (card.categories.Contains(CurseManager.instance.curseCategory))
                {
                    curses.Add(card);
                }
            }

            return curses;
        }

        public static void CallAgreed(Player player)
        {
            var agreedCards = GameActions.finishedPickCards.ToArray();
            int amount = agreedCards.Length;
            var floatArray = Enumerable.Repeat<float>(2f, amount).ToArray();
            ModdingUtils.Utils.Cards.instance.AddCardsToPlayer(player, agreedCards, false, Enumerable.Repeat("", amount).ToArray(), floatArray, floatArray, true);
        }

        public static void CallTicketRedeemer(Player player, List<ChanceCard> percentageCards)
        {
            var playerTickets = player.data.currentCards.Select((value, index) => new {value, index})
                .Where(a => string.Equals(a.value.cardName, "Ticket"))
                .Select(a => a.index).ToList();

            Unbound.Instance.StartCoroutine(HandleTicketRedeemer(player, percentageCards, playerTickets));
        }

        public static IEnumerator HandleTicketRedeemer(Player player, List<ChanceCard> percentageCards, List<int> playerTickets)
        {
            yield return WaitFor.Frames(25);

            var playerCards = player.data.currentCards;
            List<CardInfo> rolledCards = new List<CardInfo>();

            for (int i = 0; i < playerTickets.Count; i++)
            {
                rolledCards.Add(RollWithPercentages(player, percentageCards));
            }

            yield return ReplacePlayerCards(player, playerTickets, rolledCards);
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
            int totalPercentage = 0;
            for (int i = 0; i < percentageCards.Count; i++)
            {
                totalPercentage += percentageCards[i].chance;
            }

            int randomNumber = Random.Next(0, totalPercentage) + 1;

            int accumulatedProbability = 0;
            for (int i = 0; i < percentageCards.Count; i++)
            {
                accumulatedProbability += percentageCards[i].chance;
                if (randomNumber <= accumulatedProbability)
                    return percentageCards[i].GetCardInfo(player);
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
                ModdingUtils.Utils.Cards.instance.RemoveAllCardsFromPlayer(player, true);
            }

            cardsToBalance = GetRandomElements<CardInfo>(cardsToBalance, cardsToBalance.Count);

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
                foreach (Player player in players)
                {
                    if (!assigned)
                    {
                        if (ModdingUtils.Utils.Cards.instance.PlayerIsAllowedCard(player, card))
                        {
                            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, card, false, "", 2f, 2f, true);
                            assigned = true;
                            yield return WaitFor.Frames(25);
                        }
                    }
                }
            }
        }

        public static void ReplacePlayerCard(Player player, CardInfo oldCard, CardInfo newCard)
        {
            List<CardInfo> playerCards = player.data.currentCards;
            if (oldCard != null)
            {
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
        }

        public static IEnumerator ReplacePlayerCards(Player player, List<int> playerTicketIndexes, List<CardInfo> newCards)
        {
            playerTicketIndexes.ForEach(index =>
            {
                UnityEngine.Debug.Log("Index in list: " + index);
            });
            
            yield return Unbound.Instance.StartCoroutine(ModdingUtils.Utils.Cards.instance.ReplaceCards(player, playerTicketIndexes.ToArray(), newCards.ToArray(), null));
        }

        public static void GivePlayersCard(Player callPlayer, PlayerType playerType, Rarity rarity)
        {
            List<Player> players = new List<Player>();
            switch (playerType)
            {
                case (PlayerType.TEAM):
                    {
                        foreach (Player player in PlayerManager.instance.players)
                        {
                            if (callPlayer.teamID.Equals(player.teamID))
                                players.Add(player);
                        }
                        break;
                    }
                case (PlayerType.ENEMY):
                    {
                        foreach (Player player in PlayerManager.instance.players)
                        {
                            if (!callPlayer.teamID.Equals(player.teamID))
                                players.Add(player);
                        }
                        break;
                    }
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
            UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Giving Random card {randomCard} to player {player.playerID}.");
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, randomCard, false, "", 2f, 2f, true);
        }

        public static void GivePlayersCard(String cardName)
        {
            foreach (Player player in PlayerManager.instance.players)
            {
                GivePlayerCard(player, cardName);
            }
        }

        public static void GivePlayerCard(Player player, String cardName)
        {
            var cardFromName = SearchCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, cardName);
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, cardFromName, false, "", 2f, 2f, true);
        }

        public static void GivePlayerCard(Player player, String cardName, int amount)
        {
            Unbound.Instance.ExecuteAfterFrames(25, () =>
            {
                var cardFromName = SearchCard(player, player.data.weaponHandler.gun,
                    player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data,
                    player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats,
                    cardName);
                CardInfo[] cardsToGive = Enumerable.Repeat(cardFromName, amount).ToArray();
                var floatArray = Enumerable.Repeat<float>(2f, amount).ToArray();
                ModdingUtils.Utils.Cards.instance.AddCardsToPlayer(player, cardsToGive, false,
                    Enumerable.Repeat<String>("", amount).ToArray(), floatArray, floatArray, true);
            });
        }

        public static IEnumerator RandomizePlayersCardAtPosition(CardPosition cardPosition)
        {
            foreach (Player player in PlayerManager.instance.players)
            {
                yield return RandomizePlayerCardAtPosition(player, cardPosition);
            }
        }

        public static IEnumerator RandomizePlayerCardAtPosition(Player player, CardPosition cardPosition)
        {
            List<CardInfo> playerCards = player.data.currentCards;
            CardInfo oldCard = PlayerCardAtPosition(player, playerCards, cardPosition);
            if (oldCard != null)
            {
                var randomCard = RandomCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, CardRarity(oldCard, false));
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] Replacing card {removeCard.cardName} from player {player.playerID} with {randomCard.cardName}.");
                yield return WaitFor.Frames(25);
                yield return Unbound.Instance.StartCoroutine(ModdingUtils.Utils.Cards.instance.ReplaceCard(player, playerCards.IndexOf(oldCard), randomCard, "", 2f, 2f, true));
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

        public static void StealPlayersCardOfRarity(Player player, CardInfo.Rarity rarity, PlayerAmount playerAmount)
        {
            List<Player> players = new List<Player>(PlayerManager.instance.players);
            players.Remove(player);
            players.OrderBy(order => Random.Next());

            switch (playerAmount)
            {
                case PlayerAmount.ONE:
                    {
                        players = GetRandomElements<Player>(players, 1);
                        break;
                    }
                case PlayerAmount.HALF:
                    {
                        players = GetRandomElements<Player>(players, players.Count / 2);
                        break;
                    }
                case PlayerAmount.ALL:
                    {
                        players = GetRandomElements<Player>(players, players.Count);
                        break;
                    }
            }

            foreach (Player fromPlayer in players)
            {
                StealPlayerCardOfRarity(player, fromPlayer, rarity);
            }
        }

        public static void StealPlayerCardOfRarity(Player player, Player fromPlayer, CardInfo.Rarity rarity)
        {
            List<CardInfo> fromPlayerCards = fromPlayer.data.currentCards.Where(x => x.rarity.Equals(rarity) && (x.cardName != "Bully" || x.cardName != "Thief" | x.cardName != "Mafia")).ToList();
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
                                    UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] Stealing card {stolenCard.cardName} from player {fromPlayer.playerID} at {cardIndex} with card {fromPlayer.data.currentCards[cardIndex].cardName} and adding it to {player.playerID}.");
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
            int amount = 0;
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
            }
            GivePlayerCard(player, "Ticket", amount);
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
                        int randomCard = Random.Next(0, playerCards.Count - 1);
                        return playerCards.ElementAt(randomCard);
                    }
            }
            return null;
        }

        public static CardInfo RandomCard(Player checkPlayer, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats, Rarity rarity)
        {
            Func<CardInfo, Player, Gun, GunAmmo, CharacterData, HealthHandler, Gravity, Block, CharacterStatModifiers, bool> condition = (cardInfo, player, g, ga, d, h, gr, b, s) =>
            {
                if (CardRarity(cardInfo, false) != rarity)
                {
                    return false;
                }
                if (cardInfo.cardName == "Ticket")
                {
                    return false;
                }
                if (cardInfo.cardName == "Table Flip")
                {
                    return false;
                }
                if (cardInfo.cardName == "Reroll")
                {
                    return false;
                }
                if (cardInfo.cardName == "Spice")
                {
                    return false;
                }
                if (cardInfo.cardName == "Rebalance")
                {
                    return false;
                }
                return true;
            };

            return ModdingUtils.Utils.Cards.instance.GetRandomCardWithCondition(checkPlayer, gun, gunAmmo, data, health, gravity, block, characterStats, condition);
        }

        private static CardInfo SearchCard(Player checkPlayer, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats, String cardName)
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
            foreach (Transform item in children)
            {
                if (item.name == gameObjectName)
                {
                    return item.gameObject;
                }
            }

            return null;
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

        private static CardInfo RandomCardRoll(CardInfo[] cards)
        {
            if (!(cards.Length > 0))
            {
                return null;
            }

            return cards[Random.Next(cards.Length)];
        }

        public static void LoadCard(CustomCard customCard)
        {
            if (customCard.gameObject.GetComponent<CustomCardHandler>() == null)
            {
                // var cardVisuals = customCard.cardInfo.GetComponent<CardVisuals>();
                // if (cardVisuals != null)
                // {
                //     cardVisuals.toggleSelectionAction = toggle =>
                //     {
                //         UnityEngine.Debug.Log($"Toggled {toggle}");
                //     };
                // }

                customCard.gameObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.AddComponent<CustomCardHandler>();

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

        public static void LoadCard(CustomCard customCard, bool legendary)
        {
            if (customCard.gameObject.GetComponent<CustomCardHandler>() == null)
            {
                // var cardVisuals = customCard.cardInfo.GetComponent<CardVisuals>();
                // if (cardVisuals != null)
                // {
                //     cardVisuals.toggleSelectionAction = toggle =>
                //     {
                //         UnityEngine.Debug.Log($"Toggled {toggle}");
                //     };
                // }

                var cardFront = customCard.gameObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                cardFront.AddComponent<CustomCardHandler>();
                cardFront.AddComponent<Legendary>();

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
    }

    static class WaitFor
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

    public class ChanceCard
    {
        public CardController.Rarity type { get; }
        public int chance { get; }

        public ChanceCard(CardController.Rarity type, int chance)
        {
            this.type = type;
            this.chance = chance;
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
            }

            return null;
        }
    }
}
