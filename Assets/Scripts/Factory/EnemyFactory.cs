using UnityEngine;

// EnemyDatabase에서 데이터 불러와 Enemy 인스턴스 생성
public static class EnemyFactory
{
    // statScale: HP·기본 공격력·행동 수치·골드에 곱하는 배율 (집단 조우 무리원은 1/3).
    // 반올림 후 최소 1로 둬서 아주 약한 행동도 0이 되지 않게 한다. EnemyDatabase 원본은 건드리지 않는다.
    public static Enemy Create(string enemyId, float statScale = 1f)
    {
        EnemyData data = EnemyDatabase.Get(enemyId);
        if (data == null) return null;

        bool scaled = !Mathf.Approximately(statScale, 1f);
        int Scale(int v) => scaled ? Mathf.Max(1, Mathf.RoundToInt(v * statScale)) : v;
        int ScaleOrZero(int v) => v <= 0 ? 0 : Scale(v);

        var enemy = new Enemy(data.displayName, Scale(data.hp), ScaleOrZero(data.baseAttack),
                              ScaleOrZero(data.rewardGoldMin), ScaleOrZero(data.rewardGoldMax));

        foreach (var phaseData in data.phases)
        {
            var phase = new EnemyPhase
            {
                hpThreshold       = phaseData.hpThreshold,
                transitionMessage = phaseData.transitionMessage,
            };
            foreach (var actionData in phaseData.pattern)
                phase.pattern.Add(new EnemyAction(actionData.intent, ScaleOrZero(actionData.value), actionData.description));

            enemy.AddPhase(phase);
        }

        enemy.isElite   = data.isElite;
        enemy.isBoss    = data.isBoss;
        enemy.statScale = statScale;
        enemy.speed     = data.speed > 0f ? data.speed : 1f; // 집단 무리원도 속도는 그대로
        return enemy;
    }
}
