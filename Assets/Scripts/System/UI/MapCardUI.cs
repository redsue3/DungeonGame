using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 맵 탐색 중 비공격 카드 사용 패널. DungeonMapPanel 하위 오버레이로 배치한다.
// 덱의 카드를 소모하지 않고(덱 순환은 전투 전용) 코스트만 지불하고 효과를 받는다.
public class MapCardUI : MonoBehaviour
{
    [Header("코스트 상태")]
    [SerializeField] private TextMeshProUGUI costText;

    [Header("카드 목록")]
    [SerializeField] private Transform  cardParent;
    [SerializeField] private GameObject cardPrefab;   // CardUI + Button 구성 (전투 손패와 동일 프리팹)

    [Header("패널 루트/닫기")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button     closeBtn;
    [SerializeField] private TextMeshProUGUI emptyHintText;

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

        costText.text = $"코스트  {p.currentCost} / {p.maxCost}   (이동 {MapCardSystem.TilesPerCostRegen}칸마다 +1 회복)";

        foreach (var obj in cardObjects) Destroy(obj);
        cardObjects.Clear();

        // 덱 전체에서 맵 사용 가능한 카드만 나열 (같은 카드가 여러 장이면 각각 표시)
        var usable = p.deck.GetAllCards().FindAll(MapCardSystem.IsUsableOnMap);
        if (emptyHintText != null) emptyHintText.gameObject.SetActive(usable.Count == 0);

        foreach (Card card in usable)
        {
            GameObject obj = Instantiate(cardPrefab, cardParent);
            cardObjects.Add(obj);

            obj.GetComponent<CardUI>()?.Setup(card);

            var btn = obj.GetComponent<Button>();
            if (btn == null) continue;
            btn.interactable = p.currentCost >= card.cost;

            Card captured = card;
            btn.onClick.AddListener(() =>
            {
                if (DungeonManager.Instance.UseMapCard(captured))
                {
                    Refresh();
                    GetComponentInParent<DungeonMapUI>(true)?.Refresh();
                }
            });
        }
    }
}
