using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("상태별 패널 (Inspector에서 연결)")]
    public GameObject characterSelectPanel;
    public GameObject dungeonMapPanel;
    public GameObject battlePanel;
    public GameObject rewardPanel;
    public GameObject restPanel;
    public GameObject shopPanel;
    public GameObject shrinePanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    private Dictionary<GameState, GameObject> panels;
    private BattleUI battleUI;

    // 화면별 배경 그림 파일 이름 (Assets/Resources/Art/Backgrounds/<이름>.png, 계층별은 <이름>_<계층>.png)
    private static readonly Dictionary<GameState, string> backgroundNames = new Dictionary<GameState, string>
    {
        [GameState.CharacterSelect] = "title",
        [GameState.DungeonMap]      = "map",
        [GameState.Battle]          = "battle",
        [GameState.Reward]          = "reward",
        [GameState.Rest]            = "rest",
        [GameState.Shop]            = "shop",
        [GameState.Shrine]          = "shrine",
        [GameState.GameOver]        = "gameover",
        [GameState.Victory]         = "victory",
    };
    private readonly Dictionary<GameObject, Color> originalPanelColors = new Dictionary<GameObject, Color>();

    // 배경 그림 위의 글자가 묻히지 않게 그림을 살짝 어둡게 깐다
    private static readonly Color BackgroundTint = new Color(0.7f, 0.7f, 0.7f, 1f);

    void Awake()
    {
        Instance = this;

        panels = new Dictionary<GameState, GameObject>
        {
            [GameState.CharacterSelect] = characterSelectPanel,
            [GameState.DungeonMap]      = dungeonMapPanel,
            [GameState.Battle]          = battlePanel,
            [GameState.Reward]          = rewardPanel,
            [GameState.Rest]            = restPanel,
            [GameState.Shop]            = shopPanel,
            [GameState.Shrine]          = shrinePanel,
            [GameState.GameOver]        = gameOverPanel,
            [GameState.Victory]         = victoryPanel,
        };

        battleUI = battlePanel != null ? battlePanel.GetComponentInChildren<BattleUI>(includeInactive: true) : null;
    }

    public void ShowPanel(GameState state)
    {
        foreach (GameObject panel in panels.Values)
            panel?.SetActive(false);

        if (panels.TryGetValue(state, out GameObject target) && target != null)
        {
            target.SetActive(true);
            ApplyBackground(state, target);
        }

        Debug.Log($"[UIManager] 패널 전환 → {state}");
    }

    // Art/Backgrounds에 이 화면 그림이 있으면 패널 바탕 Image에 깔고, 없으면 원래 단색으로 둔다
    private void ApplyBackground(GameState state, GameObject panel)
    {
        var img = panel.GetComponent<UnityEngine.UI.Image>();
        if (img == null || !backgroundNames.TryGetValue(state, out string name)) return;

        if (!originalPanelColors.ContainsKey(panel)) originalPanelColors[panel] = img.color;

        int layer = DungeonManager.Instance != null ? DungeonManager.Instance.CurrentLayer : 1;
        Sprite bg = ArtLibrary.Background(name, layer);
        img.sprite = bg;
        img.color  = bg != null ? BackgroundTint : originalPanelColors[panel];
    }

    public void RefreshBattle() => battleUI?.Refresh();
}
