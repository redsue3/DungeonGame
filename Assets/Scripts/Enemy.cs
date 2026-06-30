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

public class Enemy : Character
{
    public int  rewardGoldMin;
    public int  rewardGoldMax;
    public bool isElite;
    public bool isBoss;

    private List<EnemyAction> pattern = new List<EnemyAction>();
    private int patternIndex = 0;

    public Enemy(string name, int hp, int baseAtk, int goldMin, int goldMax)
        : base(name, hp, baseAtk)
    {
        rewardGoldMin = goldMin;
        rewardGoldMax = goldMax;
    }

    public void AddAction(EnemyAction action) => pattern.Add(action);

    public EnemyAction PeekNextAction() => pattern[patternIndex % pattern.Count];

    public EnemyAction GetNextAction()
    {
        EnemyAction action = pattern[patternIndex % pattern.Count];
        patternIndex++;
        return action;
    }

    public void OnTurnStart()
    {
        ProcessStatusEffects();
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
