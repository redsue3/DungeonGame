// 적 스탯 데이터 (순수 데이터만, 로직 없음)
[System.Serializable]
public class EnemyData
{
    public string id;
    public string displayName;
    public int    hp;
    public int    baseAttack;
    public int    rewardGoldMin;
    public int    rewardGoldMax;
    public bool   isElite;
    public bool   isBoss;
    public EnemyActionData[] pattern;
}

[System.Serializable]
public class EnemyActionData
{
    public EnemyIntent intent;
    public int         value;
    public string      description;
}
