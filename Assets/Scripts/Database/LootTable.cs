using System.Collections.Generic;
using UnityEngine;

// 카드 드롭 풀의 한 항목. 골드는 처치한 적별 보상(EnemyData.rewardGold)으로,
// 유물은 RollRelicDrop으로 따로 굴리므로 여기엔 카드만 들어간다.
public class RewardEntry
{
    public string id;
    public int    weight;
}

public static class LootTable
{
    // 기본(공용) 카드 10종 - 전 계층 공통으로 등장, 직업 전용/희귀 카드보다 약해서 가중치를 낮게 줌
    // (2026-07-05에 CardDatabase엔 추가됐지만 드롭 풀엔 연결 안 돼 있던 것 - 2026-07-08 연결)
    private static readonly List<RewardEntry> basicCardPool = new List<RewardEntry>
    {
        new RewardEntry { id = "quick_slash",    weight = 4 },
        new RewardEntry { id = "solid_strike",   weight = 4 },
        new RewardEntry { id = "puncture",       weight = 4 },
        new RewardEntry { id = "guard_up",       weight = 4 },
        new RewardEntry { id = "steady_guard",   weight = 4 },
        new RewardEntry { id = "counter_stance", weight = 4 },
        new RewardEntry { id = "focus",          weight = 4 },
        new RewardEntry { id = "adrenaline",     weight = 4 },
        new RewardEntry { id = "second_wind",    weight = 4 },
        new RewardEntry { id = "toughen",        weight = 4 },
    };

    private static readonly Dictionary<int, List<RewardEntry>> cardPool =
        new Dictionary<int, List<RewardEntry>>
    {
        [1] = new List<RewardEntry>
        {
            new RewardEntry { id = "heavy_strike",  weight = 10 },
            new RewardEntry { id = "double_strike", weight = 8  },
            new RewardEntry { id = "iron_defense",  weight = 10 },
            new RewardEntry { id = "power_up",      weight = 6  },
            new RewardEntry { id = "quick_draw",    weight = 8  },
            new RewardEntry { id = "acid_slime",    weight = 5  },
            new RewardEntry { id = "holy_blade",    weight = 6  },
        },

        [2] = new List<RewardEntry>
        {
            new RewardEntry { id = "whirlwind",     weight = 8 },
            new RewardEntry { id = "poison_blade",  weight = 8 },
            new RewardEntry { id = "flame_burst",   weight = 8 },
            new RewardEntry { id = "mana_surge",    weight = 6 },
            new RewardEntry { id = "fortify",       weight = 7 },
            new RewardEntry { id = "dark_pact",     weight = 5 },
            new RewardEntry { id = "divine_shield", weight = 6 },
        },

        [3] = new List<RewardEntry>
        {
            new RewardEntry { id = "soul_slash",    weight = 8 },
            new RewardEntry { id = "perfect_guard", weight = 8 },
            new RewardEntry { id = "plague_strike", weight = 7 },
            new RewardEntry { id = "soul_drain",    weight = 7 },
            new RewardEntry { id = "chaos_flame",   weight = 5 },
            new RewardEntry { id = "thunder_storm", weight = 6 },
            new RewardEntry { id = "blessing",      weight = 6 },
        },
    };

    private static readonly Dictionary<int, string[]> foodPool = new Dictionary<int, string[]>
    {
        [1] = new[] { "apple", "bread", "cheese" },
        [2] = new[] { "bread", "cheese", "dried_meat" },
        [3] = new[] { "dried_meat", "ration", "feast" },
    };

    private const float FoodDropChance   = 0.5f;
    private const float EliteRelicChance = 0.7f;

    static LootTable()
    {
        foreach (var pool in cardPool.Values)
            pool.AddRange(basicCardPool);
    }

    // 가중치 추첨. 뽑힌 항목은 풀 사본에서 제거해서 중복 없이 count장을 채운다.
    public static List<string> RollCardRewards(int layer, int count)
    {
        int resolvedLayer = Mathf.Clamp(layer, 1, cardPool.Count);
        var pool   = new List<RewardEntry>(cardPool[resolvedLayer]);
        var result = new List<string>();

        while (result.Count < count && pool.Count > 0)
        {
            int totalWeight = 0;
            foreach (var entry in pool) totalWeight += entry.weight;

            int roll = Random.Range(0, totalWeight);
            for (int i = 0; i < pool.Count; i++)
            {
                roll -= pool[i].weight;
                if (roll < 0)
                {
                    result.Add(pool[i].id);
                    pool.RemoveAt(i);
                    break;
                }
            }
        }
        return result;
    }

    // 전투 승리 시 카드 보상과 별개로 식료품을 얻을 수도 있다 (없으면 null)
    public static string RollFoodDrop(int layer)
    {
        if (Random.value > FoodDropChance) return null;
        int resolvedLayer = Mathf.Clamp(layer, 1, foodPool.Count);
        string[] pool = foodPool[resolvedLayer];
        return pool[Random.Range(0, pool.Length)];
    }

    // 유물은 일반 전투에서는 나오지 않고 엘리트/보스 전투와 상점에서만 나온다.
    // 보스는 확정 드롭, 엘리트는 확률 드롭. 이미 보유한 유물은 다시 나오지 않는다.
    public static string RollRelicDrop(bool isBoss, PlayerCharacter player)
    {
        if (!isBoss && Random.value > EliteRelicChance) return null;

        List<RelicData> available = RelicDatabase.GetAll().FindAll(r =>
            !player.relics.Has(r.id) && (isBoss || r.rarity != RelicRarity.Boss));
        if (available.Count == 0) return null;

        return available[Random.Range(0, available.Count)].id;
    }
}
