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
    [SerializeField] private TextMeshProUGUI playerManaText;
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

    private int selectedTargetIndex = 0;

    void OnEnable()
    {
        endTurnBtn.onClick.AddListener(OnEndTurn);
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

        int currentMana = BattleManager.Instance != null
            ? BattleManager.Instance.CurrentMana : player.maxMana;
        playerManaText.text  = BuildManaPips(currentMana, player.maxMana);

        var sb = new System.Text.StringBuilder();
        if (player.poisonStack  > 0) sb.Append($"독 {player.poisonStack}  ");
        if (player.burnStack    > 0) sb.Append($"화상 {player.burnStack}  ");
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
            int idx  = i;
            Card card = hand[i];

            GameObject obj = Instantiate(cardPrefab, handParent);
            handObjects.Add(obj);

            var ui = obj.GetComponent<CardUI>();
            if (ui != null) ui.Setup(card);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
            {
                int currentMana = BattleManager.Instance?.CurrentMana ?? 0;
                bool usesMana = player.characterClass == CharacterClass.Mage;
                btn.interactable = !usesMana || card.manaCost <= currentMana;
                btn.onClick.AddListener(() =>
                {
                    BattleManager.Instance?.UseCard(idx, selectedTargetIndex);
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

        for (int i = 0; i < enemies.Count; i++)
        {
            if (!enemies[i].IsAlive) continue;

            int idx = i;
            GameObject obj = Instantiate(enemyPanelPrefab, enemyParent);
            enemyObjects.Add(obj);

            var ui = obj.GetComponent<EnemyPanelUI>();
            if (ui != null) ui.Setup(enemies[i], idx == selectedTargetIndex);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() =>
                {
                    selectedTargetIndex = idx;
                    RefreshEnemies();
                });
        }
    }

    private void OnEndTurn()
    {
        BattleManager.Instance?.EndPlayerTurn();
        Refresh();
    }

    private string BuildManaPips(int current, int max)
    {
        var sb = new System.Text.StringBuilder("마나 ");
        for (int i = 0; i < max; i++)
            sb.Append(i < current ? "◆" : "◇");
        return sb.ToString();
    }
}
