using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 상점 아이템 1줄을 표시하는 프리팹에 붙임
public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI tagText;
    [SerializeField] private Image           soldOverlay;

    public void Setup(ShopItem item, int playerGold)
    {
        switch (item.type)
        {
            case ShopItemType.Card:
                Card card = CardDatabase.Create(item.id);
                itemNameText.text = card != null ? card.cardName : item.id;
                itemDescText.text = card != null ? card.description : "";
                tagText.text      = "카드";
                break;

            case ShopItemType.Relic:
                RelicData relic = RelicDatabase.Get(item.id);
                itemNameText.text = relic != null ? relic.displayName : item.id;
                itemDescText.text = relic != null ? relic.description  : "";
                tagText.text      = $"유물  [{(relic != null ? relic.rarity.ToString() : "")}]";
                break;

            case ShopItemType.RemoveCard:
                itemNameText.text = "카드 제거";
                itemDescText.text = "덱에서 카드 1장을 영구 제거합니다";
                tagText.text      = "서비스";
                break;

            case ShopItemType.Food:
                FoodData food = FoodDatabase.Get(item.id);
                itemNameText.text = food != null ? food.displayName : item.id;
                itemDescText.text = food != null ? food.description : "";
                tagText.text      = "식료품";
                break;
        }

        priceText.text  = $"{item.price} G";
        priceText.color = playerGold >= item.price ? Color.white : new Color(1f, 0.4f, 0.4f);

        if (soldOverlay != null)
            soldOverlay.gameObject.SetActive(item.sold);
    }
}
