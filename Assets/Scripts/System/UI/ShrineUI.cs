using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 성소 패널 - 공격/유틸/방어 3분기 카드를 미리보기로 보여주고 하나를 골라 덱에 추가한다.
public class ShrineUI : MonoBehaviour
{
    [Header("타이틀")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("분기 슬롯 (공격/유틸/방어 순서)")]
    [SerializeField] private Transform  attackSlot;
    [SerializeField] private Transform  utilitySlot;
    [SerializeField] private Transform  defenseSlot;
    [SerializeField] private GameObject cardChoicePrefab; // CardUI + Button

    private readonly GameObject[] slotObjects = new GameObject[3];

    void OnEnable() => Refresh();

    void OnDisable()
    {
        foreach (var obj in slotObjects)
            if (obj != null) Destroy(obj);
    }

    private void Refresh()
    {
        var player  = DungeonManager.Instance?.Player;
        var options = DungeonManager.Instance?.GetPendingShrineOptions();
        if (player == null || options == null || options.Length < 3) return;

        titleText.text = $"{player.characterName}의 성소 접촉 — 카드 제작";

        Transform[] slots = { attackSlot, utilitySlot, defenseSlot };
        for (int i = 0; i < slots.Length; i++)
        {
            if (slotObjects[i] != null) Destroy(slotObjects[i]);

            int idx = i;
            GameObject obj = Instantiate(cardChoicePrefab, slots[i]);
            slotObjects[i] = obj;

            var ui = obj.GetComponent<CardUI>();
            if (ui != null) ui.Setup(options[i].card);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => DungeonManager.Instance.ChooseShrineCard(idx));
        }
    }
}
