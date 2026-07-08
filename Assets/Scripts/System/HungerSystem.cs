using UnityEngine;

// 배고픔 시스템 - 이동/전투/치유에 따른 배고픔 소모와 기아 페널티를 담당
public static class HungerSystem
{
    public const int MaxHunger          = 100;
    public const int TilesPerHungerTick = 60;  // 이 칸수만큼 이동할 때마다 배고픔 소모 (그리드 이동은 한 칸이 예전 노드 이동보다 훨씬 잘게 쪼개져서 20배로 늘림)
    public const int HungerLossPerTick  = 6;
    public const int BattleHungerCost   = 10;  // 전투 진입 시 소모
    public const int StarvationDamage   = 5;   // 배고픔 0 상태에서 이동 시 HP 피해

    // 치유량이 클수록 배고픔도 더 소모 (완전 회복이 아닐 때만 호출됨)
    public static int HealHungerCost(int healAmount) => Mathf.Max(1, healAmount / 3);

    // 맵 이동 시 DungeonManager.MovePlayer에서 호출
    public static void OnPlayerMove(PlayerCharacter player)
    {
        player.stepsSinceMeal++;
        if (player.stepsSinceMeal < TilesPerHungerTick) return;

        player.stepsSinceMeal = 0;

        if (player.hunger <= 0)
        {
            player.TakeDamage(StarvationDamage);
            Debug.Log($"[배고픔] 기아 상태! HP -{StarvationDamage}");
        }
        else
        {
            player.ChangeHunger(-HungerLossPerTick);
            Debug.Log($"[배고픔] 이동으로 배고픔 -{HungerLossPerTick} (현재 {player.hunger}/{player.maxHunger})");
        }
    }

    // 전투 시작 시 BattleManager.StartBattle에서 호출
    public static void OnBattleStart(PlayerCharacter player)
    {
        player.ChangeHunger(-BattleHungerCost);
        Debug.Log($"[배고픔] 전투로 배고픔 -{BattleHungerCost} (현재 {player.hunger}/{player.maxHunger})");
    }
}
