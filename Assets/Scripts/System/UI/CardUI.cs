using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 카드 프리팹에 붙이는 컴포넌트
public class CardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI typeTag;
    [SerializeField] private Image           cardBg;

    private static readonly System.Collections.Generic.Dictionary<CardType, Color> typeColor =
        new System.Collections.Generic.Dictionary<CardType, Color>
    {
        [CardType.Attack]  = new Color(0.85f, 0.25f, 0.25f),
        [CardType.Defense] = new Color(0.25f, 0.45f, 0.85f),
        [CardType.Skill]   = new Color(0.35f, 0.7f, 0.35f),
    };

    public void Setup(Card card)
    {
        cardNameText.text = card.cardName;
        costText.text     = card.cost.ToString();
        descText.text     = card.description;
        typeTag.text      = card.cardType == CardType.Attack ? "공격"
                          : card.cardType == CardType.Defense ? "방어" : "스킬";

        if (cardBg != null && typeColor.TryGetValue(card.cardType, out Color c))
            cardBg.color = c;
    }
}
