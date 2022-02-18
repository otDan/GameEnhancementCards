using GameEnhancementCards.Asset;
using GameEnhancementCards.Util;
using System.Collections.Generic;
using UnboundLib.Cards;
using UnityEngine;

namespace GameEnhancementCards.Card.Impl
{
    class HellishDeals : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            CardController.LoadCard(this, true);
            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] {GetTitle()} has been setup.");
            //Edits values on card itself, which are then applied to the player in `ApplyCardStats`
            ModdingUtils.Extensions.CardInfoExtension.GetAdditionalData(cardInfo).canBeReassigned = false;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] {GetTitle()} has been added to player {player.playerID}.");
            //Edits values on player when card is selected
            List<ChanceCard> chanceCards = new List<ChanceCard>
            {
                new ChanceCard(CardController.Rarity.RARE, 70),
                new ChanceCard(CardController.Rarity.CURSE, 30)
            };
            CardController.CallTicketRedeemer(player, chanceCards);
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] {GetTitle()} has been removed from player {player.playerID}.");
            //Run when the card is removed from the player
        }

        protected override string GetTitle()
        {
            return "Hellish Deal";
        }
        protected override string GetDescription()
        {
            return "Pick this card to redeem your current tickets.";
        }
        protected override GameObject GetCardArt()
        {
            return AssetManager.HellishDealsCard;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat()
                {
                    positive = true,
                    stat = "ticket:",
                    amount = "Each",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "<color=#FF00DD>Rare</color> card",
                    amount = "70%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "<color=#B500FF>Curse</color> card",
                    amount = "30%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.TechWhite;
        }
        public override string GetModName()
        {
            return GameEnhancementCards.ModInitials;
        }
    }
}
