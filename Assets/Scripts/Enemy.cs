using System.Collections.Generic;
using UnityEngine;

public enum EnemyIntent { Attack, Defend, Buff, Poison, Burn }

public class EnemyAction
{
    public EnemyIntent intent;
    public int         value;
    public string      description;

    public EnemyAction(EnemyIntent i, int v, string desc)
    {
        intent      = i;
        value       = v;
        description = desc;
    }
}

// 보스의 페이즈 하나 - 자기 패턴과 자기 진행 인덱스를 따로 들고 있어서,
// 페이즈가 바뀌면 새 패턴을 처음부터 다시 돈다.
public class EnemyPhase
{
    public float  hpThreshold;
    public string transitionMessage;
    public List<EnemyAction> pattern = new List<EnemyAction>();
}

public class Enemy : Character
{
    public int  rewardGoldMin;
    public int  rewardGoldMax;
    public bool isElite;
    public bool isBoss;

    // 4단계(전투 그리드 통합) - 전투 중 이 적의 방 안 좌표. DungeonManager.Engage()가
    // originating EnemySpawn에서 초기값을 복사하고, spawnId로 도주 시 위치를 되돌려준다.
    public int x, y;
    public int spawnId;

    private List<EnemyPhase> phases = new List<EnemyPhase>();
    private int currentPhaseIndex = 0;
    private int patternIndex = 0;

    public Enemy(string name, int hp, int baseAtk, int goldMin, int goldMax)
        : base(name, hp, baseAtk)
    {
        rewardGoldMin = goldMin;
        rewardGoldMax = goldMax;
    }

    public void AddPhase(EnemyPhase phase) => phases.Add(phase);

    private EnemyPhase CurrentPhase => phases[currentPhaseIndex];

    public EnemyAction PeekNextAction() => CurrentPhase.pattern[patternIndex % CurrentPhase.pattern.Count];

    public EnemyAction GetNextAction()
    {
        EnemyAction action = CurrentPhase.pattern[patternIndex % CurrentPhase.pattern.Count];
        patternIndex++;
        return action;
    }

    public void OnTurnStart()
    {
        ProcessStatusEffects();
        CheckPhaseTransition();
    }

    // HP 비율이 다음 페이즈의 진입 조건 이하로 떨어졌으면 전환한다 (역행 없음 - 회복해도 이전 페이즈로 안 돌아감).
    // 한 턴에 여러 단계를 건너뛸 수도 있고(while), 페이즈가 바뀔 때마다 패턴 인덱스를 0으로 리셋해서 새 패턴을 처음부터 돈다.
    private void CheckPhaseTransition()
    {
        while (currentPhaseIndex + 1 < phases.Count &&
               (float)currentHp / maxHp <= phases[currentPhaseIndex + 1].hpThreshold)
        {
            currentPhaseIndex++;
            patternIndex = 0;
            if (!string.IsNullOrEmpty(CurrentPhase.transitionMessage))
                Debug.Log($"[페이즈 전환] {characterName}: {CurrentPhase.transitionMessage}");
        }
    }

    public void ExecuteAction(EnemyAction action, PlayerCharacter target)
    {
        switch (action.intent)
        {
            case EnemyIntent.Attack:
                int dmg = DamageCalculator.Calculate(action.value, attackPower);
                Debug.Log($"{characterName}: {action.description} ({dmg} 데미지)");
                target.TakeDamage(dmg);
                break;

            case EnemyIntent.Defend:
                AddBlock(action.value);
                Debug.Log($"{characterName}: {action.description} (방어막 {action.value})");
                break;

            case EnemyIntent.Buff:
                attackPower += action.value;
                Debug.Log($"{characterName}: {action.description} (공격력 +{action.value}, 누적 {attackPower})");
                break;

            case EnemyIntent.Poison:
                target.poisonStack += action.value;
                Debug.Log($"{characterName}: {action.description} (독 {action.value} 스택)");
                break;

            case EnemyIntent.Burn:
                target.burnStack += action.value;
                Debug.Log($"{characterName}: {action.description} (화상 {action.value} 스택)");
                break;
        }
    }

    public int RollGoldReward() => Random.Range(rewardGoldMin, rewardGoldMax + 1);
}
