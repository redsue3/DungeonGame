using UnityEngine;

public class Character
{
    public string characterName;
    public int maxHp;
    public int currentHp;
    public int block;
    public int attackPower;

    public int poisonStack;
    public int burnStack;

    public bool IsAlive => currentHp > 0;

    public Character(string name, int hp, int atk = 0)
    {
        characterName = name;
        maxHp         = hp;
        currentHp     = hp;
        attackPower   = atk;
    }

    public void TakeDamage(int amount)
    {
        int absorbed  = Mathf.Min(block, amount);
        block        -= absorbed;
        int remaining = amount - absorbed;
        currentHp     = Mathf.Max(0, currentHp - remaining);
        Debug.Log($"{characterName}: 방어막 {absorbed} 흡수, 실제 데미지 {remaining} → HP {currentHp}/{maxHp}");
    }

    public void AddBlock(int amount)
    {
        block += amount;
        Debug.Log($"{characterName}: 방어막 +{amount} (현재 {block})");
    }

    public void ResetBlock() => block = 0;

    public virtual void Heal(int amount)
    {
        currentHp = Mathf.Min(maxHp, currentHp + amount);
        Debug.Log($"{characterName}: 체력 +{amount} → HP {currentHp}/{maxHp}");
    }

    public void ProcessStatusEffects()
    {
        if (poisonStack > 0)
        {
            int dmg   = DamageCalculator.CalculatePoison(poisonStack);
            currentHp = Mathf.Max(0, currentHp - dmg);
            poisonStack = Mathf.Max(0, poisonStack - 1);
            Debug.Log($"[독] {characterName}: {dmg} 데미지 (남은: {poisonStack})");
        }
        if (burnStack > 0)
        {
            int dmg   = DamageCalculator.CalculateBurn(burnStack);
            currentHp = Mathf.Max(0, currentHp - dmg);
            burnStack = Mathf.Max(0, burnStack - 1);
            Debug.Log($"[화상] {characterName}: {dmg} 데미지 (남은: {burnStack})");
        }
    }
}
