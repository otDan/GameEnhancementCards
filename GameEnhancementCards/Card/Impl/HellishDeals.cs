﻿using GameEnhancementCards.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace GameEnhancementCards.Cards
{
    class HellishDeals : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            CardController.LoadCard(this);
            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] {GetTitle()} has been setup.");
            //Edits values on card itself, which are then applied to the player in `ApplyCardStats`
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //UnityEngine.Debug.Log($"[{GameEnhancementCards.ModInitials}][Card] {GetTitle()} has been added to player {player.playerID}.");
            //Edits values on player when card is selected
            List<ChanceCard> chanceCards = new List<ChanceCard>();
            chanceCards.Add(new ChanceCard(CardController.Rarity.RARE, 70));
            chanceCards.Add(new ChanceCard(CardController.Rarity.CURSE, 30));
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
            return null;
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
                    amount = "For each",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Rare card",
                    amount = "70%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Curse card",
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
