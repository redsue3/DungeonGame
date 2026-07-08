using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 전투 중 적 1명을 표시하는 패널 프리팹에 붙임
public class EnemyPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Slider          hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI blockText;
    [SerializeField] private TextMeshProUGUI intentText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image           selectionOutline;  // 타겟 선택 강조

    public void Setup(Enemy enemy, bool isSelected)
    {
        nameText.text    = enemy.characterName + (enemy.isBoss ? "  [보스]" : enemy.isElite ? "  [엘리트]" : "");
        hpSlider.value   = (float)enemy.currentHp / enemy.maxHp;
        hpText.text      = $"{enemy.currentHp} / {enemy.maxHp}";
        blockText.text   = enemy.block > 0 ? $"방어 {enemy.block}" : "";

        EnemyAction next = enemy.PeekNextAction();
        intentText.text  = next != null ? $"→ {IntentIcon(next.intent)} {next.description}" : "";

        var sb = new System.Text.StringBuilder();
        if (enemy.poisonStack > 0) sb.Append($"독 {enemy.poisonStack}  ");
        if (enemy.burnStack   > 0) sb.Append($"화상 {enemy.burnStack}");
        statusText.text = sb.ToString().TrimEnd();

        if (selectionOutline != null)
            selectionOutline.enabled = isSelected;
    }

    private string IntentIcon(EnemyIntent intent) => intent switch
    {
        EnemyIntent.Attack => "[공격]",
        EnemyIntent.Defend => "[방어]",
        EnemyIntent.Buff   => "[버프]",
        EnemyIntent.Poison => "[독]",
        EnemyIntent.Burn   => "[화상]",
        _                  => "[?]"
    };
}
