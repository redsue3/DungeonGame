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
    public EnemyPhaseData[] phases;
}

// 페이즈 하나 - 특정 HP 비율 이하로 내려가면 이 페이즈의 패턴으로 전환된다.
// 일반/엘리트 몹은 보통 페이즈 1개(hpThreshold=1f, 처음부터 끝까지 같은 패턴 하나만 반복),
// 보스는 2~3개를 둬서 HP가 줄어들수록 더 위협적인 패턴으로 바뀌게 한다.
[System.Serializable]
public class EnemyPhaseData
{
    public float  hpThreshold;       // 이 비율(현재HP/최대HP) 이하로 내려가면 전환. 배열의 첫 페이즈는 1f로 둬서 시작부터 적용.
    public string transitionMessage; // 전환 시 로그에 남길 대사 (선택, 없으면 null)
    public EnemyActionData[] pattern;
}

[System.Serializable]
public class EnemyActionData
{
    public EnemyIntent intent;
    public int         value;
    public string      description;
}
