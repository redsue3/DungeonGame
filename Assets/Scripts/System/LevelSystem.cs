using UnityEngine;

// 레벨 시스템 (2026-11-11)
// 막타(카드로 적을 직접 쓰러뜨림)마다 경험치를 얻고, 레벨이 오르면 직업별 성장치(PlayerData의 *PerLevel)만큼
// 최대 HP·공격 보너스·방어 보너스가 오른다. 독·화상으로 쓰러진 적은 경험치를 주지 않는다(마법사 마나와 같은 규칙).
// Tools/BalanceSim이 아래 상수와 XpToNext 식을 그대로 읽는다 - 형식을 바꾸면 시뮬레이터 파서도 같이 고칠 것.
public static class LevelSystem
{
    public const int MaxLevel = 10;

    // 적 한 마리를 쓰러뜨렸을 때 경험치 (집단 무리원처럼 능력치 배율이 있으면 같은 배율, 최소 1)
    public const int XpNormal = 3;
    public const int XpElite  = 10;
    public const int XpBoss   = 20;

    // 지금 레벨에서 다음 레벨까지 필요한 경험치: 10, 15, 20, 25 ...
    public static int XpToNext(int level) => 10 + 5 * (level - 1);

    public static int XpFor(Enemy e)
    {
        int baseXp = e.isBoss ? XpBoss : e.isElite ? XpElite : XpNormal;
        return Mathf.Max(1, Mathf.RoundToInt(baseXp * e.statScale));
    }
}
