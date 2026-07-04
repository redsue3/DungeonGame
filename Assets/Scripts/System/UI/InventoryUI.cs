using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 배고픔 상태 + 식료품 인벤토리 패널. DungeonMapPanel 하위에 배치해
// 던전맵/휴식 등에서 열고 닫을 수 있도록 구성한다.
public class InventoryUI : MonoBehaviour
{
    [Header("배고픔 상태")]
    [SerializeField] private TextMeshProUGUI hungerText;
    [SerializeField] private Image           hungerBarFill;

    [Header("식료품 목록")]
    [SerializeField] private Transform  itemParent;
    [SerializeField] private GameObject itemPrefab;   // FoodItemUI + Button 구성

    [Header("패널 루트/닫기")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button     closeBtn;

    private readonly List<GameObject> itemObjects = new List<GameObject>();

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

        hungerText.text = $"배고픔  {p.hunger} / {p.maxHunger}" + (p.IsStarving ? "  (기아 상태!)" : "");
        if (hungerBarFill != null)
            hungerBarFill.fillAmount = (float)p.hunger / p.maxHunger;

        foreach (var obj in itemObjects) Destroy(obj);
        itemObjects.Clear();

        foreach (var (id, count) in p.inventory.GetAll())
        {
            FoodData food = FoodDatabase.Get(id);
            if (food == null) continue;

            GameObject obj = Instantiate(itemPrefab, itemParent);
            itemObjects.Add(obj);

            var ui = obj.GetComponent<FoodItemUI>();
            if (ui != null) ui.Setup(food, count);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() =>
                {
                    if (DungeonManager.Instance.UseFoodItem(id))
                    {
                        Refresh();
                        GetComponentInParent<DungeonMapUI>(true)?.Refresh();
                    }
                });
        }
    }
}
