using GameEnhancementCards.Utils;
using System.Collections.Generic;
using GameEnhancementCards.Asset;
using UnboundLib.Cards;
using UnityEngine;

namespace GameEnhancementCards.Card.Impl
{
    class PrizeBooth : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            CardController.LoadCard(this);
            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] {GetTitle()} has been setup.");
            //Edits values on card itself, which are then applied to the player in `ApplyCardStats`
            ModdingUtils.Extensions.CardInfoExtension.GetAdditionalData(cardInfo).canBeReassigned = false;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] {GetTitle()} has been added to player {player.playerID}.");
            List<ChanceCard> chanceCards = new List<ChanceCard>
            {
                new ChanceCard(CardController.Rarity.COMMON, 40),
                new ChanceCard(CardController.Rarity.UNCOMMON, 35),
                new ChanceCard(CardController.Rarity.RARE, 25)
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
            return "Prize Booth";
        }
        protected override string GetDescription()
        {
            return "Pick this card to redeem your current tickets.";
        }
        protected override GameObject GetCardArt()
        {
            return AssetManager.PrizeBoothCard;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Common;
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
                    stat = "Common card",
                    amount = "40%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "<color=#00B2FF>Uncommon</color> card",
                    amount = "35%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "<color=#FF00DD>Rare</color> card",
                    amount = "25%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }//,
                // new CardInfoStat()
                // {
                //     positive = false,
                //     stat = "Nothing",
                //     amount = "5%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // }
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
