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
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    void Awake() => Instance = this;

    public void ShowPanel(GameState state)
    {
        HideAll();
        switch (state)
        {
            case GameState.CharacterSelect: characterSelectPanel?.SetActive(true); break;
            case GameState.DungeonMap:      dungeonMapPanel?.SetActive(true);      break;
            case GameState.Battle:          battlePanel?.SetActive(true);           break;
            case GameState.Reward:          rewardPanel?.SetActive(true);           break;
            case GameState.Rest:            restPanel?.SetActive(true);             break;
            case GameState.Shop:            shopPanel?.SetActive(true);             break;
            case GameState.GameOver:        gameOverPanel?.SetActive(true);         break;
            case GameState.Victory:         victoryPanel?.SetActive(true);          break;
        }
        Debug.Log($"[UIManager] 패널 전환 → {state}");
    }

    public void RefreshBattle()
    {
        battlePanel?.GetComponentInChildren<BattleUI>()?.Refresh();
    }

    private void HideAll()
    {
        characterSelectPanel?.SetActive(false);
        dungeonMapPanel?.SetActive(false);
        battlePanel?.SetActive(false);
        rewardPanel?.SetActive(false);
        restPanel?.SetActive(false);
        shopPanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
        victoryPanel?.SetActive(false);
    }
}
