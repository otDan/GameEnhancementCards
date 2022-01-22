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
        public static List<CardInfo> loadedArt { get; private set; }
        private static Color firstColor = new Color(1f, 0f, 0.314f, 0.94f);
        private static Color secondColor = new Color(1f, 1f, 1f, 0.85f);
        private static Color textColor = new Color(1f, 1f, 1f, 0.92f);

        private static Dictionary<string, SoundEvent> _soundCache = new Dictionary<string, SoundEvent>();

        static CardsManager()
        {
            loadedArt = new List<CardInfo>();
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

        public static GameObject FindObjectInChilds(this GameObject gameObject, string gameObjectName)
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

        public static List<GameObject> FindObjectsInChilds(this GameObject gameObject, string gameObjectName)
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

        public static void GivePlayersCard(CardInfo card)
        {
            foreach (Player player in PlayerManager.instance.players)
            {
                GivePlayerCard(player, card);
            }
        }

        public static void GivePlayerCard(Player player, CardInfo card)
        {
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, card, false, "", 2f, 2f, true);
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
            CardInfo stolenCard = GetRandomElements<CardInfo>(fromPlayerCards, fromPlayerCards.Count()).First<CardInfo>();
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

        public static void LoadCard(CardInfo cardInfo)
        {
            if (!loadedArt.Contains(cardInfo))
            {
                List<GameObject> ballObjects = FindObjectsInChilds(cardInfo.cardBase, "SmallBall");
                List<GameObject> triangleObjects = FindObjectsInChilds(cardInfo.cardBase, "Triangle");
                foreach (GameObject triangleObject in triangleObjects)
                {
                    var triangleImageObjects = triangleObject.gameObject.GetComponentsInChildren(typeof(Image), true).Where(x => x.gameObject.transform.parent.name == "Triangle").ToList();
                    foreach (Image image in triangleImageObjects)
                    {
                        image.color = secondColor;
                    }
                }
                var faceObject = FindObjectInChilds(cardInfo.cardBase, "Face");
                GameObject.Destroy(faceObject);

                var backObject = FindObjectInChilds(cardInfo.cardBase, "Back");
                GameObject textObject = new GameObject("BackText");
                textObject.AddComponent<TextMeshProUGUI>();
                var backText = textObject.GetComponent<TextMeshProUGUI>();
                backText.text = "GEC";
                backText.fontSize = 12;
                backText.transform.SetParent(backObject.transform);

                var imageObjects = backObject.gameObject.GetComponentsInChildren<Image>(true);

                foreach (Image image in imageObjects)
                {
                    image.color = firstColor;
                }

                var cardVisuals = cardInfo.cardBase.GetComponentInChildren<CardVisuals>();
                cardVisuals.chillColor = firstColor;

                cardVisuals.toggleSelectionAction = delegate (bool boolean)
                {
                    Unbound.Instance.ExecuteAfterFrames(1, () =>
                    {
                        cardVisuals.defaultColor = firstColor;
                        TextMeshProUGUI cardTextObject = (TextMeshProUGUI) cardInfo.cardBase.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponentInChildren(typeof(TextMeshProUGUI), true);
                        cardTextObject.color = textColor;

                        foreach (GameObject triangleObject in triangleObjects)
                        {
                            var triangleImageObjects = triangleObject.gameObject.GetComponentsInChildren(typeof(Image), true).Where(x => x.gameObject.transform.parent.name == "Triangle").ToList();
                            foreach (Image image in triangleImageObjects)
                            {
                                image.color = secondColor;
                            }
                        }

                        foreach (GameObject ballObject in ballObjects)
                        {
                            var ballImageObjects = ballObject.gameObject.GetComponentsInChildren(typeof(ProceduralImage), true).Where(x => x.gameObject.transform.name.Contains("SmallBall")).ToList();
                            foreach (ProceduralImage image in ballImageObjects)
                            {

                                image.color = secondColor;
                            }
                        }

                        /*textObject = new GameObject("BackText"); Rotations are fucked while adding no clue right now
                        textObject.AddComponent<TextMeshProUGUI>();
                        backText = textObject.GetComponent<TextMeshProUGUI>();
                        backText.text = "GEC";
                        backText.fontSize = 12;
                        backText.transform.SetParent(faceObject.transform);*/
                    }
                    );
                };
                loadedArt.Add(cardInfo);
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

    public enum PlayerAmount
    {
        ONE,
        HALF,
        ALL
    }
}
