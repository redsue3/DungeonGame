using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Slider          playerHpSlider;
    [SerializeField] private TextMeshProUGUI playerHpText;
    [SerializeField] private TextMeshProUGUI playerBlockText;
    [SerializeField] private TextMeshProUGUI playerCostText;
    [SerializeField] private TextMeshProUGUI playerStatusText;  // 독/화상/힘

    [Header("적 영역")]
    [SerializeField] private Transform       enemyParent;
    [SerializeField] private GameObject      enemyPanelPrefab;  // EnemyPanelUI 달린 프리팹

    [Header("패 영역")]
    [SerializeField] private Transform       handParent;
    [SerializeField] private GameObject      cardPrefab;        // CardUI 달린 프리팹
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private TextMeshProUGUI discardCountText;

    [Header("버튼")]
    [SerializeField] private Button          endTurnBtn;
    [SerializeField] private TextMeshProUGUI turnText;

    private readonly List<GameObject> handObjects  = new List<GameObject>();
    private readonly List<GameObject> enemyObjects = new List<GameObject>();

    // 단일 공격 카드가 때릴 적. 인덱스가 아니라 참조로 들고 있어야
    // 죽은 적이 목록에서 빠져도 엉뚱한 적을 때리지 않는다.
    private Enemy selectedTarget;

    void OnEnable()
    {
        endTurnBtn.onClick.AddListener(OnEndTurn);
        selectedTarget = null; // 새 전투 - 이전 전투의 적 참조를 버린다
        Refresh();
    }

    void OnDisable()
    {
        endTurnBtn.onClick.RemoveAllListeners();
    }

    // BattleManager에서 매 변경마다 호출
    public void Refresh()
    {
        RefreshPlayerStatus();
        RefreshHand();
        RefreshEnemies();
    }

    private void RefreshPlayerStatus()
    {
        var player = DungeonManager.Instance?.Player;
        if (player == null) return;

        playerNameText.text  = player.characterName;
        playerHpSlider.value = (float)player.currentHp / player.maxHp;
        playerHpText.text    = $"{player.currentHp} / {player.maxHp}";
        playerBlockText.text = player.block > 0 ? $"방어 {player.block}" : "";

        int currentCost = BattleManager.Instance != null
            ? BattleManager.Instance.CurrentCost : player.maxCost;
        playerCostText.text = BuildCostPips(currentCost, player.maxCost);

        var sb = new System.Text.StringBuilder();
        if (player.poisonStack   > 0) sb.Append($"독 {player.poisonStack}  ");
        if (player.burnStack     > 0) sb.Append($"화상 {player.burnStack}  ");
        if (player.strengthStack > 0) sb.Append($"힘 +{player.strengthStack}");
        playerStatusText.text = sb.ToString().TrimEnd();
    }

    private void RefreshHand()
    {
        foreach (var obj in handObjects) Destroy(obj);
        handObjects.Clear();

        var player = DungeonManager.Instance?.Player;
        if (player == null) return;

        List<Card> hand = player.deck.GetHand();

        for (int i = 0; i < hand.Count; i++)
        {
            int idx   = i;
            Card card = hand[i];

            GameObject obj = Instantiate(cardPrefab, handParent);
            handObjects.Add(obj);

            var ui = obj.GetComponent<CardUI>();
            if (ui != null) ui.Setup(card);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
            {
                int currentCost = BattleManager.Instance?.CurrentCost ?? 0;
                btn.interactable = card.cost <= currentCost;
                btn.onClick.AddListener(() =>
                {
                    BattleManager.Instance?.UseCard(idx, selectedTarget);
                    Refresh();
                });
            }
        }

        deckCountText.text    = $"덱 {player.deck.DrawCount}";
        discardCountText.text = $"묘지 {player.deck.DiscardCount}";
    }

    private void RefreshEnemies()
    {
        foreach (var obj in enemyObjects) Destroy(obj);
        enemyObjects.Clear();

        var enemies = BattleManager.Instance?.GetEnemies();
        if (enemies == null) return;

        // 아직 타겟이 없거나 선택했던 적이 죽었으면 첫 번째 생존 적으로
        if (selectedTarget == null || !selectedTarget.IsAlive)
            selectedTarget = enemies.Find(e => e.IsAlive);

        foreach (Enemy enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            Enemy captured = enemy;
            GameObject obj = Instantiate(enemyPanelPrefab, enemyParent);
            enemyObjects.Add(obj);

            var ui = obj.GetComponent<EnemyPanelUI>();
            if (ui != null) ui.Setup(enemy, enemy == selectedTarget);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() =>
                {
                    selectedTarget = captured;
                    RefreshEnemies();
                });
        }
    }

    private void OnEndTurn()
    {
        BattleManager.Instance?.EndPlayerTurn();
        Refresh();
    }

    private string BuildCostPips(int current, int max)
    {
        var sb = new System.Text.StringBuilder("코스트 ");
        int pips = Mathf.Max(max, current); // 유물로 최대치를 넘겨 받은 코스트도 초과분까지 표시
        for (int i = 0; i < pips; i++)
            sb.Append(i < current ? "◆" : "◇");
        return sb.ToString();
    }
}
