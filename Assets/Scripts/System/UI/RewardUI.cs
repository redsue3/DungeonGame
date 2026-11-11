using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    [Header("보상 정보")]
    [SerializeField] private TextMeshProUGUI goldRewardText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI foodRewardText;
    [SerializeField] private TextMeshProUGUI relicRewardText;

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

        // 보물상자·함정 결과도 이 화면으로 보여 준다 - 골드가 없으면 골드 줄을 비우고, 제목이 있으면 그걸 쓴다
        goldRewardText.text = reward.gold > 0 ? $"골드 +{reward.gold}" : "";
        titleText.text      = reward.title ?? "카드를 1장 선택하세요";

        if (foodRewardText != null)
        {
            FoodData food = reward.foodId != null ? FoodDatabase.Get(reward.foodId) : null;
            foodRewardText.text = food != null ? $"식료품 획득: {food.displayName} (배고픔 +{food.hungerRestore})" : "";
        }

        if (relicRewardText != null)
        {
            RelicData relic = reward.relicId != null ? RelicDatabase.Get(reward.relicId) : null;
            relicRewardText.text = relic != null ? $"유물 획득: {relic.displayName} — {relic.description}" : "";
        }

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
