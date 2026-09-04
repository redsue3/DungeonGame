using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 맵 탐색 중 비공격 카드를 쓸 수 있는 '카드' 탭 패널. InventoryUI와 동일하게
// DungeonMapPanel 하위 오버레이로 열고 닫는다. 카드 프리팹(CardUI)을 그대로 재사용한다.
public class CardTabUI : MonoBehaviour
{
    [Header("탐사 코스트 상태")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI ambushText;

    [Header("카드 목록")]
    [SerializeField] private Transform  cardParent;
    [SerializeField] private GameObject cardPrefab; // CardUI + Button 구성 (배틀/보상 등과 공용 프리팹)

    [Header("패널 루트/닫기")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button     closeBtn;

    private readonly List<GameObject> cardObjects = new List<GameObject>();

    void OnEnable()
    {
        closeBtn?.onClick.AddListener(Close);
    }

    void OnDisable()
    {
        closeBtn?.onClick.RemoveAllListeners();
    }

    public void Open()
    {
        panelRoot?.SetActive(true);
        Refresh();
    }

    public void Close() => panelRoot?.SetActive(false);

    public void Refresh()
    {
        var p = DungeonManager.Instance?.Player;
        if (p == null) return;

        costText.text = $"탐사 코스트  {p.explorationCost} / {p.maxExplorationCost}";
        ambushText.text = p.pendingAmbushCard != null
            ? $"기습 예약: [{p.pendingAmbushCard.cardName}] (다음 전투 시작 시 발동)"
            : "기습 예약 없음";

        foreach (var obj in cardObjects) Destroy(obj);
        cardObjects.Clear();

        foreach (Card card in DungeonManager.Instance.GetMapUsableCards())
        {
            GameObject obj = Instantiate(cardPrefab, cardParent);
            cardObjects.Add(obj);

            var ui = obj.GetComponent<CardUI>();
            if (ui != null) ui.Setup(card);

            var btn = obj.GetComponent<Button>();
            if (btn == null) continue;

            btn.interactable = card.manaCost <= p.explorationCost;
            string cardId = card.id;
            btn.onClick.AddListener(() =>
            {
                if (DungeonManager.Instance.UseCardOnMap(cardId))
                {
                    Refresh();
                    GetComponentInParent<DungeonMapUI>(true)?.Refresh();
                }
            });
        }
    }
}
