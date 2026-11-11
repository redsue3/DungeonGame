using UnityEngine;

// 맵에 놓이는 보물상자·바닥 보상의 개수와 확률 (2026-11-11 사용자와 확정 - 테스트 플레이 뒤 다시 조정할 수 있음)
// 숫자는 전부 여기 모아 둔다. Tools/BalanceSim이 이 파일의 상수와 가중치를 그대로 읽으니 형식을 바꾸면 파서도 같이 고칠 것.
public enum ChestOutcome { Gold, Card, Food, Relic, Mimic, Trap }
public enum PickupKind { Chest, FloorLoot }

public static class TreasureTable
{
    // 층마다 놓이는 개수
    public const int ChestsPerFloor    = 2;
    public const int FloorLootPerFloor = 3;

    // 보물상자를 열었을 때 결과 가중치 (합 100 = 퍼센트로 읽으면 됨)
    public const int WeightGold  = 40;   // 골드: 층 x ChestGoldMin~Max
    public const int WeightCard  = 22;   // 그 층 카드 3장 중 1장 고르기
    public const int WeightFood  = 15;   // 그 층 음식 1개
    public const int WeightRelic = 8;    // 보스 유물을 뺀 무작위 유물
    public const int WeightMimic = 10;   // 상자 속에 숨어 있던 그 층 일반몹과 전투 - 이기면 MimicRelicChance로 유물 추가
    public const int WeightTrap  = 5;    // 함정: 절반은 HP 손실(층 x TrapHpPerLayer, 최소 1은 남김), 절반은 배고픔 손실

    public const int   ChestGoldMinPerLayer = 15;
    public const int   ChestGoldMaxPerLayer = 25;
    public const float MimicRelicChance     = 0.5f;
    public const int   TrapHpPerLayer       = 4;
    public const int   TrapHungerLoss       = 10;

    // 바닥 보상: 골드(층 x LootGoldMin~Max) 아니면 음식
    public const int LootGoldChancePercent = 70;
    public const int LootGoldMinPerLayer   = 3;
    public const int LootGoldMaxPerLayer   = 6;

    public static ChestOutcome RollChest()
    {
        int total = WeightGold + WeightCard + WeightFood + WeightRelic + WeightMimic + WeightTrap;
        int roll  = Random.Range(0, total);
        if ((roll -= WeightGold)  < 0) return ChestOutcome.Gold;
        if ((roll -= WeightCard)  < 0) return ChestOutcome.Card;
        if ((roll -= WeightFood)  < 0) return ChestOutcome.Food;
        if ((roll -= WeightRelic) < 0) return ChestOutcome.Relic;
        if ((roll -= WeightMimic) < 0) return ChestOutcome.Mimic;
        return ChestOutcome.Trap;
    }

    public static int RollChestGold(int layer) => layer * Random.Range(ChestGoldMinPerLayer, ChestGoldMaxPerLayer + 1);
    public static int RollLootGold(int layer)  => layer * Random.Range(LootGoldMinPerLayer, LootGoldMaxPerLayer + 1);
}
