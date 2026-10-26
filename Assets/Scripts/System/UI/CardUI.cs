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

    private Image cardArt; // Art/Cards/<카드id> 그림이 있을 때만 런타임에 만든다

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

        Sprite art = ArtLibrary.Card(card.id);
        if (art != null && cardArt == null) cardArt = CreateArtImage();
        if (cardArt != null)
        {
            cardArt.sprite  = art;
            cardArt.enabled = art != null;
        }
    }

    // 카드 글자 뒤에 깔리는 그림. 글자가 읽히도록 반투명으로 둔다.
    private Image CreateArtImage()
    {
        var go = new GameObject("Art", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        go.transform.SetSiblingIndex(0);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.preserveAspect = true;
        img.raycastTarget  = false;
        img.color          = new Color(1f, 1f, 1f, 0.45f);
        return img;
    }
}
