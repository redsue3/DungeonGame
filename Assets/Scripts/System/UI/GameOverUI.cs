using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private Button          retryBtn;
    [SerializeField] private Button          titleBtn;

    void OnEnable()
    {
        retryBtn.onClick.AddListener(OnRetry);
        titleBtn.onClick.AddListener(OnTitle);

        var p = DungeonManager.Instance?.Player;
        if (p != null)
            summaryText.text = $"{p.characterName}  전사\n골드 {p.gold}  계층 {DungeonManager.Instance.CurrentLayer}";
    }

    void OnDisable()
    {
        retryBtn.onClick.RemoveAllListeners();
        titleBtn.onClick.RemoveAllListeners();
    }

    private void OnRetry()
    {
        SaveSystem.Delete();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTitle()
    {
        SaveSystem.Delete();
        SceneManager.LoadScene(0);
    }
}
