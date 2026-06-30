using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerHpText;
    [SerializeField] private TextMeshProUGUI healPreviewText;
    [SerializeField] private Button          restBtn;
    [SerializeField] private Button          leaveBtn;

    void OnEnable()
    {
        restBtn.onClick.AddListener(OnRest);
        leaveBtn.onClick.AddListener(OnLeave);
        Refresh();
    }

    void OnDisable()
    {
        restBtn.onClick.RemoveAllListeners();
        leaveBtn.onClick.RemoveAllListeners();
    }

    private void Refresh()
    {
        var p = DungeonManager.Instance?.Player;
        if (p == null) return;

        int healAmount = Mathf.RoundToInt(p.maxHp * 0.3f);
        int healedHp   = Mathf.Min(p.maxHp, p.currentHp + healAmount);

        playerHpText.text    = $"현재 HP  {p.currentHp} / {p.maxHp}";
        healPreviewText.text = $"휴식하면 HP +{healAmount}  →  {healedHp}";

        restBtn.interactable = p.currentHp < p.maxHp;
    }

    private void OnRest()  => DungeonManager.Instance.UseRestSite();
    private void OnLeave() => DungeonManager.Instance.LeaveShop();   // 그냥 지나치기
}
