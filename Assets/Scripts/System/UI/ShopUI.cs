using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("상점 헤더")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI shopTitleText;

    [Header("아이템 목록")]
    [SerializeField] private Transform       itemParent;
    [SerializeField] private GameObject      shopItemPrefab;  // ShopItemUI 달린 프리팹

    [Header("카드 제거 패널 (구매 후 활성화)")]
    [SerializeField] private GameObject      removeCardPanel;
    [SerializeField] private Transform       removeCardList;
    [SerializeField] private GameObject      removeCardEntryPrefab;  // 제거할 카드 선택용
    [SerializeField] private TextMeshProUGUI removeCardHintText;

    [Header("버튼")]
    [SerializeField] private Button          leaveBtn;

    private readonly List<GameObject> itemObjects      = new List<GameObject>();
    private readonly List<GameObject> removeCardObjects = new List<GameObject>();
    private bool removeCardMode = false;

    void OnEnable()
    {
        leaveBtn.onClick.AddListener(OnLeave);
        removeCardMode = false;
        removeCardPanel.SetActive(false);
        Refresh();
    }

    void OnDisable()
    {
        leaveBtn.onClick.RemoveAllListeners();
    }

    private void Refresh()
    {
        var player = DungeonManager.Instance?.Player;
        if (player == null) return;

        goldText.text     = $"보유 골드  {player.gold}";
        shopTitleText.text = "상점";

        BuildItemList();
    }

    private void BuildItemList()
    {
        foreach (var obj in itemObjects) Destroy(obj);
        itemObjects.Clear();

        ShopSystem shop = DungeonManager.Instance?.GetShop();
        if (shop == null) return;

        var player = DungeonManager.Instance.Player;

        for (int i = 0; i < shop.Items.Count; i++)
        {
            int idx = i;
            ShopItem item = shop.Items[i];

            GameObject obj = Instantiate(shopItemPrefab, itemParent);
            itemObjects.Add(obj);

            var ui = obj.GetComponent<ShopItemUI>();
            if (ui != null) ui.Setup(item, player.gold);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = !item.sold && player.gold >= item.price;
                btn.onClick.AddListener(() => OnBuyItem(idx));
            }
        }
    }

    private void OnBuyItem(int index)
    {
        ShopSystem shop = DungeonManager.Instance?.GetShop();
        if (shop == null) return;

        ShopItem item = shop.Items[index];

        bool success = DungeonManager.Instance.BuyShopItem(index);
        if (!success) return;

        if (item.type == ShopItemType.RemoveCard)
        {
            removeCardMode = true;
            removeCardPanel.SetActive(true);
            BuildRemoveCardList();
        }

        Refresh();
    }

    private void BuildRemoveCardList()
    {
        foreach (var obj in removeCardObjects) Destroy(obj);
        removeCardObjects.Clear();

        var player = DungeonManager.Instance?.Player;
        if (player == null) return;

        removeCardHintText.text = "제거할 카드를 선택하세요";

        List<Card> allCards = player.deck.GetAllCards();
        foreach (Card card in allCards)
        {
            string cid = card.id;
            GameObject obj = Instantiate(removeCardEntryPrefab, removeCardList);
            removeCardObjects.Add(obj);

            var ui = obj.GetComponent<CardUI>();
            if (ui != null) ui.Setup(card);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() =>
                {
                    DungeonManager.Instance.RemoveCardFromDeck(cid);
                    removeCardPanel.SetActive(false);
                    removeCardMode = false;
                });
        }
    }

    private void OnLeave()
    {
        if (removeCardMode)
        {
            // 카드 제거 서비스 구매 후 선택 없이 나가면 그냥 취소
            removeCardPanel.SetActive(false);
            removeCardMode = false;
        }
        DungeonManager.Instance.LeaveShop();
    }
}
