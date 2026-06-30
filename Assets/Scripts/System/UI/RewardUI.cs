using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    [Header("보상 정보")]
    [SerializeField] private TextMeshProUGUI goldRewardText;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("카드 선택")]
    [SerializeField] private Transform       cardChoiceParent;
    [SerializeField] private GameObject      cardChoicePrefab;  // CardUI + Button

    [Header("버튼")]
    [SerializeField] private Button          skipBtn;

    private readonly List<GameObject> choiceObjects = new List<GameObject>();

    void OnEnable()
    {
        skipBtn.onClick.AddListener(OnSkip);
        Refresh();
    }

    void OnDisable()
    {
        skipBtn.onClick.RemoveAllListeners();
    }

    private void Refresh()
    {
        BattleReward reward = DungeonManager.Instance?.GetPendingReward();
        if (reward == null) return;

        goldRewardText.text = $"골드 +{reward.gold}";
        titleText.text      = "카드를 1장 선택하세요";

        foreach (var obj in choiceObjects) Destroy(obj);
        choiceObjects.Clear();

        foreach (string cardId in reward.cardChoices)
        {
            string id  = cardId;
            Card card  = CardDatabase.Create(id);
            if (card == null) continue;

            GameObject obj = Instantiate(cardChoicePrefab, cardChoiceParent);
            choiceObjects.Add(obj);

            var ui = obj.GetComponent<CardUI>();
            if (ui != null) ui.Setup(card);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => DungeonManager.Instance.ChooseCard(id));
        }
    }

    private void OnSkip() => DungeonManager.Instance.SkipCardReward();
}
