using UnityEngine;

// 탐사 코스트 시스템 - 맵 탐색 중 카드 사용에 쓰는 자원의 걸음 수 기반 회복을 담당.
// HungerSystem과 동일한 패턴(걸음 수 누적 → 일정 칸마다 소모/회복)을 재사용하되,
// 전투용 BattleManager.currentMana와는 완전히 별개의 자원이다.
public static class ExplorationCostSystem
{
    public const int TilesPerCostTick = 20; // 이 칸수만큼 이동할 때마다 탐사 코스트 1 회복

    // 맵 이동 시 DungeonManager.TryMove에서 호출
    public static void OnPlayerMove(PlayerCharacter player)
    {
        if (player.explorationCost >= player.maxExplorationCost) return;

        player.stepsSinceCostRegen++;
        if (player.stepsSinceCostRegen < TilesPerCostTick) return;

        player.stepsSinceCostRegen = 0;
        player.explorationCost = Mathf.Min(player.maxExplorationCost, player.explorationCost + 1);
        Debug.Log($"[탐사 코스트] 회복 +1 (현재 {player.explorationCost}/{player.maxExplorationCost})");
    }
}
