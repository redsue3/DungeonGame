// EnemyDatabase에서 데이터 불러와 Enemy 인스턴스 생성
public static class EnemyFactory
{
    public static Enemy Create(string enemyId)
    {
        EnemyData data = EnemyDatabase.Get(enemyId);
        if (data == null) return null;

        var enemy = new Enemy(data.displayName, data.hp, data.baseAttack,
                              data.rewardGoldMin, data.rewardGoldMax);

        foreach (var phaseData in data.phases)
        {
            var phase = new EnemyPhase
            {
                hpThreshold       = phaseData.hpThreshold,
                transitionMessage = phaseData.transitionMessage,
            };
            foreach (var actionData in phaseData.pattern)
                phase.pattern.Add(new EnemyAction(actionData.intent, actionData.value, actionData.description));

            enemy.AddPhase(phase);
        }

        enemy.isElite = data.isElite;
        enemy.isBoss  = data.isBoss;
        return enemy;
    }
}
