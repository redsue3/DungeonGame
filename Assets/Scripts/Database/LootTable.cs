using System.Collections.Generic;
using UnityEngine;

public enum RewardType { Card, Gold, Relic }

[System.Serializable]
public class RewardEntry
{
    public RewardType type;
    public string     id;
    public int        weight;
    public int        goldMin;
    public int        goldMax;
}

public static class LootTable
{
    private static readonly Dictionary<int, List<RewardEntry>> cardPool =
        new Dictionary<int, List<RewardEntry>>
    {
        [1] = new List<RewardEntry>
        {
            new RewardEntry { type = RewardType.Card, id = "heavy_strike",  weight = 10 },
            new RewardEntry { type = RewardType.Card, id = "double_strike", weight = 8  },
            new RewardEntry { type = RewardType.Card, id = "iron_defense",  weight = 10 },
            new RewardEntry { type = RewardType.Card, id = "power_up",      weight = 6  },
            new RewardEntry { type = RewardType.Card, id = "quick_draw",    weight = 8  },
            new RewardEntry { type = RewardType.Card, id = "acid_slime",    weight = 5  },
            new RewardEntry { type = RewardType.Card, id = "holy_blade",    weight = 6  },
        },

        [2] = new List<RewardEntry>
        {
            new RewardEntry { type = RewardType.Card, id = "whirlwind",    weight = 8  },
            new RewardEntry { type = RewardType.Card, id = "poison_blade", weight = 8  },
            new RewardEntry { type = RewardType.Card, id = "flame_burst",  weight = 8  },
            new RewardEntry { type = RewardType.Card, id = "mana_surge",   weight = 6  },
            new RewardEntry { type = RewardType.Card, id = "fortify",      weight = 7  },
            new RewardEntry { type = RewardType.Card, id = "dark_pact",    weight = 5  },
            new RewardEntry { type = RewardType.Card, id = "divine_shield",weight = 6  },
        },

        [3] = new List<RewardEntry>
        {
            new RewardEntry { type = RewardType.Card, id = "soul_slash",    weight = 8  },
            new RewardEntry { type = RewardType.Card, id = "perfect_guard", weight = 8  },
            new RewardEntry { type = RewardType.Card, id = "plague_strike", weight = 7  },
            new RewardEntry { type = RewardType.Card, id = "soul_drain",    weight = 7  },
            new RewardEntry { type = RewardType.Card, id = "chaos_flame",   weight = 5  },
            new RewardEntry { type = RewardType.Card, id = "thunder_storm", weight = 6  },
            new RewardEntry { type = RewardType.Card, id = "blessing",      weight = 6  },
        },
    };

    private static readonly Dictionary<int, (int min, int max)> goldRange =
        new Dictionary<int, (int, int)>
    {
        [1] = (12, 20),
        [2] = (20, 32),
        [3] = (35, 52),
        [4] = (50, 70),
    };

    private static readonly Dictionary<int, string[]> foodPool = new Dictionary<int, string[]>
    {
        [1] = new[] { "apple", "bread", "cheese" },
        [2] = new[] { "bread", "cheese", "dried_meat" },
        [3] = new[] { "dried_meat", "ration", "feast" },
    };

    private const float FoodDropChance  = 0.5f;
    private const float EliteRelicChance = 0.7f;

    public static List<string> RollCardRewards(int layer, int count)
    {
        int resolvedLayer = Mathf.Clamp(layer, 1, cardPool.Count);
        var pool   = cardPool[resolvedLayer];
        var result = new List<string>();
        var used   = new HashSet<string>();

        int totalWeight = 0;
        foreach (var e in pool) totalWeight += e.weight;

        int tries = 0;
        while (result.Count < count && tries < 100)
        {
            tries++;
            int roll = Random.Range(0, totalWeight);
            int acc  = 0;
            foreach (var entry in pool)
            {
                acc += entry.weight;
                if (roll < acc)
                {
                    if (!used.Contains(entry.id))
                    {
                        result.Add(entry.id);
                        used.Add(entry.id);
                    }
                    break;
                }
            }
        }
        return result;
    }

    public static int RollGold(int layer)
    {
        int resolvedLayer = Mathf.Clamp(layer, 1, goldRange.Count);
        var (min, max) = goldRange[resolvedLayer];
        return Random.Range(min, max + 1);
    }

    public static int GetCardRewardCount(int layer) => 1 + layer;

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
