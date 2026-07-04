using System.Collections.Generic;
using UnityEngine;

public enum ShopItemType { Card, Relic, RemoveCard, Food }

public class ShopItem
{
    public ShopItemType type;
    public string       id;
    public int          price;
    public bool         sold;
}

public class ShopSystem
{
    public List<ShopItem> Items { get; private set; } = new List<ShopItem>();

    private static readonly Dictionary<RelicRarity, int> relicPrices = new Dictionary<RelicRarity, int>
    {
        [RelicRarity.Common]   = 80,
        [RelicRarity.Uncommon] = 130,
        [RelicRarity.Rare]     = 200,
        [RelicRarity.Boss]     = 300,
    };

    public void Generate(int layer, PlayerCharacter player)
    {
        Items.Clear();

        // 카드 3장
        List<string> cardIds = LootTable.RollCardRewards(layer, 3);
        foreach (string id in cardIds)
            Items.Add(new ShopItem { type = ShopItemType.Card, id = id, price = GetCardPrice(layer) });

        // 유물 1개 (보유하지 않은 것 중 랜덤)
        string relicId = RollRandomRelic(player);
        if (relicId != null)
        {
            RelicData relicData = RelicDatabase.Get(relicId);
            int price = relicData != null && relicPrices.TryGetValue(relicData.rarity, out int p) ? p : 150;
            Items.Add(new ShopItem { type = ShopItemType.Relic, id = relicId, price = price });
        }

        // 식료품 2개
        List<string> foodIds = RollRandomFoods(2);
        foreach (string id in foodIds)
        {
            FoodData food = FoodDatabase.Get(id);
            if (food != null)
                Items.Add(new ShopItem { type = ShopItemType.Food, id = id, price = food.price });
        }

        // 카드 제거 서비스
        Items.Add(new ShopItem { type = ShopItemType.RemoveCard, id = "", price = 75 });
    }

    public bool TryBuy(int index, PlayerCharacter player)
    {
        if (index < 0 || index >= Items.Count) return false;
        ShopItem item = Items[index];
        if (item.sold)
        {
            Debug.Log("[상점] 이미 판매된 아이템");
            return false;
        }
        if (player.gold < item.price)
        {
            Debug.Log($"[상점] 골드 부족! 필요:{item.price} 보유:{player.gold}");
            return false;
        }

        player.gold -= item.price;
        item.sold    = true;

        switch (item.type)
        {
            case ShopItemType.Card:
                Card card = CardDatabase.Create(item.id);
                if (card != null)
                {
                    player.deck.AddCard(card);
                    Debug.Log($"[상점] 카드 구매: {card.cardName}");
                }
                break;

            case ShopItemType.Relic:
                player.relics.Add(item.id);
                RelicDatabase.ApplyPassiveEffects(item.id, player);
                Debug.Log($"[상점] 유물 구매: {item.id}");
                break;

            case ShopItemType.RemoveCard:
                Debug.Log("[상점] 카드 제거 서비스 구매 — UI에서 제거할 카드 선택 후 DungeonManager.RemoveCardFromDeck() 호출");
                break;

            case ShopItemType.Food:
                player.inventory.AddFood(item.id);
                Debug.Log($"[상점] 식료품 구매: {FoodDatabase.Get(item.id)?.displayName}");
                break;
        }
        return true;
    }

    // 카드 제거 서비스 사용 시 호출
    public bool RemoveCard(string cardId, PlayerCharacter player)
    {
        bool removed = player.deck.RemoveCard(cardId);
        if (removed) Debug.Log($"[상점] 카드 제거: {cardId}");
        return removed;
    }

    private int GetCardPrice(int layer) => 50 + layer * 20 + Random.Range(-10, 15);

    private string RollRandomRelic(PlayerCharacter player)
    {
        List<RelicData> all       = RelicDatabase.GetAll();
        List<RelicData> available = all.FindAll(r => !player.relics.Has(r.id));
        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)].id;
    }

    private List<string> RollRandomFoods(int count)
    {
        List<FoodData>  all    = FoodDatabase.GetAll();
        List<string>    result = new List<string>();
        for (int i = 0; i < count && all.Count > 0; i++)
            result.Add(all[Random.Range(0, all.Count)].id);
        return result;
    }
}
