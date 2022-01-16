using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib.Utils;

namespace GameEnhancementCards.Utils
{
    public class CardsManager
    {
        public static CardsManager instance { get; private set; }


        private System.Random random = new System.Random();

        public CardInfo RandomCard()
        {
            var cards = CardManager.cards.Values.ToArray().Select(card => card.cardInfo).ToArray();
            return RandomCardRoll(cards);
        }

        private CardInfo RandomCardRoll(CardInfo[] cards)
        {
            if (!(cards.Count() > 0))
            {
                return null;
            }

            return cards[random.Next(cards.Count())];
        }
    }
}
