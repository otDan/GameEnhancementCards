using Sonigon;
using Sonigon.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using static ModdingUtils.Utils.Cards;

namespace GameEnhancementCards.Utils
{
    public static class CardsManager
    {
        private static System.Random random = new System.Random();
        public static List<CustomCard> loadedArt { get; set; }
        
        private static Color firstColor = new Color(1f, 0f, 0.414f, 0.91f);
        private static Color secondColor = new Color(1f, 0f, 0.014f, 1f);

        private static Color triangleColor = new Color(0.90f, 0.90f, 0.90f, 0.75f);
        private static Color textColor = new Color(1f, 1f, 1f, 0.92f);

        private static Dictionary<string, SoundEvent> _soundCache = new Dictionary<string, SoundEvent>();

        static CardsManager()
        {
            loadedArt = new List<CustomCard>();
        }

        //Testing stuff
        /*public static void PlaySound(GameObject gameObject)
        {
            SoundManager.Instance.Play(GetSound("UI_Card_Pick_SE"), gameObject.transform);
        }

        private static SoundEvent GetSound(string name)
        {
            if (!_soundCache.ContainsKey(name))
            {
                var soundEvent = GameObject.Find("/SonigonSoundEventPool").transform.Find(name).gameObject?.GetComponent<InstanceSoundEvent>().soundEvent;
                _soundCache.Add(name, soundEvent);
            }

            return _soundCache[name];
        }*/

        public static void CallPrizeBooth(Player player)
        {
            List<CardInfo> playerCards = player.data.currentCards.Where(p => p.cardName == "Ticket").ToList();
            foreach (CardInfo ticketCard in playerCards)
            {
                var rolledCard = PrizeBoothRoll(player);
                UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Prize Booth rolled card {rolledCard}.");
                ReplacePlayerCard(player, ticketCard, rolledCard);
            }
        }

        private static CardInfo PrizeBoothRoll(Player player)
        {
            int randomNumber = random.Next(0, 100);
            UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Rolled {randomNumber}.");

            if (randomNumber >= 0 && randomNumber < 45)
            {
                return RandomCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, CardInfo.Rarity.Common);
            }
            else if (randomNumber >= 45 && randomNumber < 80)
            {
                return RandomCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, CardInfo.Rarity.Uncommon);
            }
            else if (randomNumber >= 80 && randomNumber < 95)
            {
                return RandomCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, CardInfo.Rarity.Rare);
            }

            return null;
        }

        public static void CallNegate()
        {
            UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Negate card called.");
            foreach (Player player in PlayerManager.instance.players)
            {
                var lastCards = GameActions.lastRoundCards[player.playerID];
                UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Negate card called for player {player.playerID} for {lastCards.Count} cards {lastCards}.");
                foreach (CardInfo card in lastCards)
                {
                    UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Negate card removed {card.cardName}.");
                }
                ModdingUtils.Utils.Cards.instance.RemoveCardsFromPlayer(player, lastCards.ToArray(), SelectionType.Newest);
            }
        }

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
                    yield return WaitFor.Frames(10);
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
                players = GetRandomElements<Player>(players, players.Count);
                foreach (Player player in players)
                {
                    if (!assigned)
                    {
                        if (ModdingUtils.Utils.Cards.instance.PlayerIsAllowedCard(player, card))
                        {
                            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, card, false, "", 2f, 2f, true);
                            assigned = true;
                            yield return WaitFor.Frames(10);
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
                    Unbound.Instance.ExecuteAfterFrames(10, () =>
                    {
                        UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Replacing {oldCard} with {newCard}.");
                        Unbound.Instance.StartCoroutine(ModdingUtils.Utils.Cards.instance.ReplaceCard(player, playerCards.IndexOf(oldCard), newCard, "", 2f, 2f, true));
                    }
                    );
                }
                else
                {
                    UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Removing {oldCard}.");
                    ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(player, playerCards.IndexOf(oldCard));
                }
            }
        }

        public static void GivePlayersCard(Player callPlayer, PlayerType playerType, CardInfo.Rarity rarity)
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

        public static void GivePlayersCard(CardInfo.Rarity rarity)
        {
            foreach (Player player in PlayerManager.instance.players)
            {
                GivePlayerCard(player, rarity);
            }
        }

        public static void GivePlayerCard(Player player, CardInfo.Rarity rarity)
        {
            var randomCard = RandomCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, rarity);
            UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Giving random card {randomCard} to player {player.playerID}.");
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
            var cardFromName = SearchCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, cardName);
            CardInfo[] cardsToGive = Enumerable.Repeat(cardFromName, amount).ToArray();
            var floatArray = Enumerable.Repeat<float>(2f, amount).ToArray();
            ModdingUtils.Utils.Cards.instance.AddCardsToPlayer(player, cardsToGive, false, Enumerable.Repeat<String>("", amount).ToArray(), floatArray, floatArray, true);
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
                var randomCard = RandomCard(player, player.data.weaponHandler.gun, player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>(), player.data, player.data.healthHandler, player.GetComponent<Gravity>(), player.data.block, player.data.stats, oldCard.rarity);
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

        public static void StealPlayersCardOfRarity(Player player, CardInfo.Rarity rarity, PlayerAmount playerAmount)
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
            List<CardInfo> fromPlayerCards = fromPlayer.data.currentCards.Where(x => x.rarity.Equals(rarity)).ToList();
            var randomizedCards = GetRandomElements<CardInfo>(fromPlayerCards, fromPlayerCards.Count());
            if (randomizedCards.Count > 0)
            {
                CardInfo stolenCard = randomizedCards.First<CardInfo>();
                if (stolenCard != null)
                {
                    Unbound.Instance.ExecuteAfterFrames(10, () =>
                    {
                        var cardIndex = fromPlayerCards.IndexOf(stolenCard);
                        if (cardIndex >= 0)
                        {
                            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] Stealing card {stolenCard.cardName} from player {fromPlayer.playerID} at {cardIndex} and adding it to {player.playerID}.");
                            ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(fromPlayer, cardIndex);
                            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, stolenCard, false, "", 2f, 2f, true);
                            return;
                        }
                        else
                        {
                            GiveConsolationPrize(player, rarity);
                        }
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
                        int randomCard = random.Next(0, playerCards.Count - 1);
                        return playerCards.ElementAt(randomCard);
                    }
            }

            return null;
        }

        private static CardInfo RandomCard(Player checkPlayer, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats, CardInfo.Rarity rarity)
        {
            Func<CardInfo, Player, Gun, GunAmmo, CharacterData, HealthHandler, Gravity, Block, CharacterStatModifiers, bool> condition = (cardInfo, player, g, ga, d, h, gr, b, s) =>
            {
                if (CardRarity(cardInfo) != rarity)
                {
                    return false;
                }
                if (cardInfo.cardName == "Ticket")
                {
                    return false;
                }
                return true;
            };

            return ModdingUtils.Utils.Cards.instance.GetRandomCardWithCondition(checkPlayer, gun, gunAmmo, data, health, gravity, block, characterStats, condition);
        }

        private static CardInfo SearchCard(Player checkPlayer, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats, String cardName)
        {
            Func<CardInfo, Player, Gun, GunAmmo, CharacterData, HealthHandler, Gravity, Block, CharacterStatModifiers, bool> condition = (cardInfo, player, g, ga, d, h, gr, b, s) =>
            {
                if (cardInfo.cardName != cardName)
                {
                    return false;
                }
                return true;
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

        private static GameObject FindObjectInChilds(this GameObject gameObject, string gameObjectName)
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

        private static List<GameObject> FindObjectsInChilds(this GameObject gameObject, string gameObjectName)
        {
            List<GameObject> returnObjects = new List<GameObject>();
            Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform item in children)
            {
                if (item.name.Contains(gameObjectName))
                {
                    returnObjects.Add(gameObject);
                }
            }

            return returnObjects;
        }

        private static CardInfo RandomCardRoll(CardInfo[] cards)
        {
            if (!(cards.Count() > 0))
            {
                return null;
            }

            return cards[random.Next(cards.Count())];
        }

        public static void LoadCard(CustomCard customCard)
        {
            if (!loadedArt.Contains(customCard))
            {
                var faceObject = FindObjectInChilds(customCard.gameObject, "Face");
                GameObject.Destroy(faceObject);

                var frontObject = FindObjectInChilds(customCard.gameObject, "Back");
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

                loadedArt.Add(customCard);
            }
        }

        private static float timeLeft;
        private static Color targetColor;
        private static Color currentColor;

        public static void Update()
        {
            if (timeLeft <= Time.deltaTime)
            {
                CardColorChange(targetColor);
                timeLeft = 1f;

                if (targetColor == firstColor)
                {
                    targetColor = secondColor;
                    return;
                }
                targetColor = firstColor;    
            }
            else
            {
                CardColorChange(Color.Lerp(currentColor, targetColor, Time.deltaTime / timeLeft));
                timeLeft -= Time.deltaTime;
            }
        }

        private static void CardColorChange(Color newColor)
        {
            try
            {
                foreach (var card in CustomCardList())
                {
                    if (card != null)
                    {
                        TextMeshProUGUI cardTextObject = (TextMeshProUGUI) card.gameObject.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponentInChildren(typeof(TextMeshProUGUI), true);
                        cardTextObject.color = textColor;

                        var images = card.gameObject.GetComponentsInChildren(typeof(Image), true).Where(x => x.gameObject.transform.parent.name == "Front" || x.gameObject.transform.parent.name == "Back" || x.gameObject.transform.parent.name.Contains("EdgePart")).ToList();
                        foreach (Image image in images)
                        {
                            image.color = newColor;
                        }

                        List<GameObject> ballObjects = FindObjectsInChilds(card.gameObject, "SmallBall");
                        foreach (GameObject ballObject in ballObjects)
                        {
                            var ballImageObjects = ballObject.gameObject.GetComponentsInChildren(typeof(ProceduralImage), true).Where(x => x.gameObject.transform.name.Contains("SmallBall")).ToList();
                            foreach (ProceduralImage image in ballImageObjects)
                            {

                                image.color = triangleColor;
                            }
                        }
                    }

                }
            }
            catch (Exception exception)
            {
                //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}] Exception {exception}.");
            }
            currentColor = newColor;
        }

        public static IEnumerable<CustomCard> CustomCardList()
        {
            // Iterating the elements of my_list
            foreach (var items in loadedArt)
            {
                // Returning the element after every iteration
                yield return items;
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
}
