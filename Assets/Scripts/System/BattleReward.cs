using System.Collections.Generic;

public class BattleReward
{
    public List<string> cardChoices;
    public int          gold;
    public string       foodId;    // 획득한 식료품 (없으면 null)
    public string       relicId;   // 획득한 유물 (없으면 null, 엘리트/보스 전투에서만 나옴)
}
