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

        if (panels.TryGetValue(state, out GameObject target))
            target?.SetActive(true);

        Debug.Log($"[UIManager] 패널 전환 → {state}");
    }

    public void RefreshBattle() => battleUI?.Refresh();
}
