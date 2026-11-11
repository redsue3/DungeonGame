using System.Collections.Generic;

public class BattleReward
{
    public List<string> cardChoices = new List<string>();
    public int          gold;
    public string       foodId;    // 획득한 식료품 (없으면 null)
    public string       relicId;   // 획득한 유물 (없으면 null - 엘리트/보스 전투, 보물상자, 미믹에서 나옴)
    public string       title;     // 보상 화면 제목 (null이면 "카드를 1장 선택하세요") - 보물상자·함정 결과를 이 화면으로 보여 줄 때 씀
}
