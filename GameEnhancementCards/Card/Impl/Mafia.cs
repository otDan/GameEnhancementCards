using GameEnhancementCards.Asset;
using GameEnhancementCards.Util;
using UnboundLib.Cards;
using UnityEngine;

namespace GameEnhancementCards.Card.Impl
{
    class Mafia : CustomCard
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
            CardController.StealPlayersCardOfRarity(player, CardInfo.Rarity.Rare, CardController.PlayerAmount.ONE);
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] {GetTitle()} has been removed from player {player.playerID}.");
            //Run when the card is removed from the player
        }

        protected override string GetTitle()
        {
            return "Mafia";
        }
        protected override string GetDescription()
        {
            return "Steal a random rare card from a random player.";
        }
        protected override GameObject GetCardArt()
        {
            return AssetManager.MafiaCard;
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
                    positive = false,
                    stat = "stolen:",
                    amount = "No card",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Ticket cards",
                    amount = "+3",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                // new CardInfoStat()
                // {
                //     positive = true,
                //     stat = "opponent:",
                //     amount = "<color=#00B2FF>Stolen",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                // new CardInfoStat()
                // {
                //     positive = true,
                //     stat = "Ticket cards",
                //     amount = "+3",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
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
