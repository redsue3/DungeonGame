using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
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
            summaryText.text = $"마왕 격파!\n{p.characterName}  |  골드 {p.gold}  |  HP {p.currentHp}/{p.maxHp}";
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
