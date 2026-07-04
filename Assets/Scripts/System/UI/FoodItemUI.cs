using UnityEngine;
using TMPro;

// 인벤토리 목록의 식료품 한 줄을 표시하는 프리팹에 붙임
public class FoodItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI countText;

    public void Setup(FoodData food, int count)
    {
        nameText.text  = food.displayName;
        descText.text  = food.description;
        countText.text = $"x{count}";
    }
}
