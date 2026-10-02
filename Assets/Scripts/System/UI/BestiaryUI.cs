using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 몬스터 도감 오버레이. DungeonMapPanel 하위에 배치해 상단 '도감' 버튼으로 연다.
// bestiary.md(MonsterLoreDatabase)를 한 마리씩 넘겨 보고, 아직 맞붙은 적 없는 몬스터는 ??? 로 가린다.
// 목록을 프리팹으로 늘어놓지 않고 한 장씩 넘기는 방식이라 런타임에 UI 오브젝트를 만들지 않는다.
public class BestiaryUI : MonoBehaviour
{
    [Header("내용")]
    [SerializeField] private TextMeshProUGUI pageText;     // "3 / 22 · 발견 5"
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI infoText;     // 계층 · 분류 · 등급
    [SerializeField] private TextMeshProUGUI descText;

    [Header("넘기기/닫기")]
    [SerializeField] private Button     prevBtn;
    [SerializeField] private Button     nextBtn;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button     closeBtn;

    private int index;

    void OnEnable()
    {
        prevBtn?.onClick.AddListener(() => Turn(-1));
        nextBtn?.onClick.AddListener(() => Turn(+1));
        closeBtn?.onClick.AddListener(Close);
    }

    void OnDisable()
    {
        prevBtn?.onClick.RemoveAllListeners();
        nextBtn?.onClick.RemoveAllListeners();
        closeBtn?.onClick.RemoveAllListeners();
    }

    public void Open()
    {
        panelRoot?.SetActive(true);
        Refresh();
    }

    public void Close() => panelRoot?.SetActive(false);

    private void Turn(int delta)
    {
        int count = MonsterLoreDatabase.All.Count;
        if (count == 0) return;
        index = (index + delta + count) % count;
        Refresh();
    }

    public void Refresh()
    {
        var all = MonsterLoreDatabase.All;
        if (all.Count == 0)
        {
            pageText.text = "";
            nameText.text = "도감을 불러오지 못했습니다";
            infoText.text = "";
            descText.text = "StreamingAssets/bestiary.md 를 확인하세요.";
            return;
        }

        index = Mathf.Clamp(index, 0, all.Count - 1);
        MonsterLore lore = all[index];

        int found = 0;
        foreach (MonsterLore l in all)
            if (MonsterLoreDatabase.IsDiscovered(l.id)) found++;
        pageText.text = $"{index + 1} / {all.Count}  ·  발견 {found}";

        if (MonsterLoreDatabase.IsDiscovered(lore.id))
        {
            nameText.text = lore.displayName;
            infoText.text = $"{lore.layer}계층  ·  {lore.category}  ·  {lore.grade}";
            descText.text = lore.description;
        }
        else
        {
            // 계층만은 보여준다 - 어디까지 내려가야 만날 수 있는지 힌트.
            nameText.text = "???";
            infoText.text = $"{lore.layer}계층  ·  ???  ·  ???";
            descText.text = "아직 맞닥뜨린 적 없는 몬스터다.";
        }
    }
}
