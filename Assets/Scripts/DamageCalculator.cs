using UnityEngine;

public static class DamageCalculator
{
    // 기본 데미지 공식
    // 최종 데미지 = (카드 기본 데미지 + 공격력 보너스) * 배율 - 방어막
    public static int Calculate(int baseDamage, int attackBonus, float multiplier = 1f)
    {
        int total = Mathf.RoundToInt((baseDamage + attackBonus) * multiplier);
        return Mathf.Max(0, total);
    }

    // 독 데미지 (방어막 무시)
    public static int CalculatePoison(int poisonStack)
    {
        return poisonStack; // 스택 수 = 데미지
    }

    // 화상 데미지 (방어막 무시)
    public static int CalculateBurn(int burnStack)
    {
        return Mathf.RoundToInt(burnStack * 1.5f);
    }
}
