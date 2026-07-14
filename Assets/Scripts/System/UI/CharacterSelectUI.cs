using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("직업 버튼")]
    [SerializeField] private Button warriorBtn;
    [SerializeField] private Button rogueBtn;
    [SerializeField] private Button mageBtn;
    [SerializeField] private Button paladinBtn;

    [Header("직업 정보 표시")]
    [SerializeField] private TextMeshProUGUI classNameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI handSizeText;
    [SerializeField] private TextMeshProUGUI attackBonusText;
    [SerializeField] private TextMeshProUGUI starterCardsText;

    [Header("확인 버튼")]
    [SerializeField] private Button confirmBtn;
    [SerializeField] private TextMeshProUGUI confirmBtnText;

    [Header("이어하기 버튼")]
    [SerializeField] private Button continueBtn;

    private CharacterClass selectedClass = CharacterClass.Warrior;

    void OnEnable()
    {
        warriorBtn.onClick.AddListener(() => SelectClass(CharacterClass.Warrior));
        rogueBtn.onClick.AddListener(()   => SelectClass(CharacterClass.Rogue));
        mageBtn.onClick.AddListener(()    => SelectClass(CharacterClass.Mage));
        paladinBtn.onClick.AddListener(() => SelectClass(CharacterClass.Paladin));
        confirmBtn.onClick.AddListener(OnConfirm);

        if (continueBtn != null)
        {
            continueBtn.onClick.AddListener(OnContinue);
            continueBtn.gameObject.SetActive(SaveSystem.HasSave());
        }

        SelectClass(CharacterClass.Warrior);
    }

    void OnDisable()
    {
        warriorBtn.onClick.RemoveAllListeners();
        rogueBtn.onClick.RemoveAllListeners();
        mageBtn.onClick.RemoveAllListeners();
        paladinBtn.onClick.RemoveAllListeners();
        confirmBtn.onClick.RemoveAllListeners();
        continueBtn?.onClick.RemoveAllListeners();
    }

    private void SelectClass(CharacterClass cls)
    {
        selectedClass = cls;

        PlayerData data = PlayerDatabase.Get(cls);
        if (data == null) return;

        classNameText.text    = data.displayName;
        hpText.text           = $"HP  {data.maxHp}";
        costText.text         = $"코스트  {data.maxCost}";
        handSizeText.text     = $"시작 패  {data.startHandSize}장";
        attackBonusText.text  = $"공격 보너스  +{data.baseAttackBonus}";
        confirmBtnText.text   = $"{data.displayName} 선택";

        // 스타터 카드 목록
        var sb = new System.Text.StringBuilder("스타터 덱: ");
        foreach (string id in data.starterDeckCardIds)
        {
            Card c = CardDatabase.Create(id);
            if (c != null) sb.Append($"{c.cardName}  ");
        }
        starterCardsText.text = sb.ToString().TrimEnd();

        // 선택된 버튼 강조
        SetButtonHighlight(warriorBtn, cls == CharacterClass.Warrior);
        SetButtonHighlight(rogueBtn,   cls == CharacterClass.Rogue);
        SetButtonHighlight(mageBtn,    cls == CharacterClass.Mage);
        SetButtonHighlight(paladinBtn, cls == CharacterClass.Paladin);
    }

    private void OnConfirm() => DungeonManager.Instance.StartRun(selectedClass);

    private void OnContinue() => DungeonManager.Instance.LoadRun();

    private void SetButtonHighlight(Button btn, bool selected)
    {
        var img = btn.GetComponent<Image>();
        if (img != null)
            img.color = selected ? new Color(1f, 0.85f, 0.2f) : Color.white;
    }
}
