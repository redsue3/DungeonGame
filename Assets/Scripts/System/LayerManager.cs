using System.Collections.Generic;
using UnityEngine;

public enum StageType { Normal, Elite, Boss, Rest, Shop }

public class StageInfo
{
    public int       stageNumber;
    public StageType type;
    public string    enemyId;
    public bool      eliteGuaranteed;
}

public class LayerManager : MonoBehaviour
{
    public static LayerManager Instance;

    public int currentLayer { get; private set; } = 1;
    public int currentStage { get; private set; } = 1;

    private bool eliteSpawnedThisLayer = false;

    private static readonly Dictionary<int, string[]> normalPool = new Dictionary<int, string[]>
    {
        [1] = new[] { "slime", "thief" },
        [2] = new[] { "orc", "poison_spider" },
        [3] = new[] { "cursed_warrior", "shadow_witch" },
    };

    private static readonly Dictionary<int, string[]> elitePool = new Dictionary<int, string[]>
    {
        [1] = new[] { "goblin_elite", "skeleton_elite" },
        [2] = new[] { "dark_mage_elite" },
        [3] = new[] { "death_knight_elite" },
    };

    private static readonly Dictionary<int, string> bossTable = new Dictionary<int, string>
    {
        [1] = "giant_slime_boss",
        [2] = "orc_warchief_boss",
        [3] = "demon_lord_boss",
    };

    void Awake() => Instance = this;

    public StageInfo GetCurrentStage() => BuildStageInfo(currentLayer, currentStage);

    public StageInfo Advance()
    {
        currentStage++;
        if (currentStage > 5)
        {
            currentStage = 1;
            currentLayer++;
            eliteSpawnedThisLayer = false;
        }
        return GetCurrentStage();
    }

    private StageInfo BuildStageInfo(int layer, int stage)
    {
        // 스테이지 5 = 보스
        if (stage == 5)
        {
            return new StageInfo
            {
                stageNumber = stage,
                type        = StageType.Boss,
                enemyId     = GetBossId(layer),
            };
        }

        // 스테이지 3 = 휴식 or 상점 (50/50)
        if (stage == 3)
        {
            StageType restOrShop = Random.value < 0.5f ? StageType.Rest : StageType.Shop;
            return new StageInfo { stageNumber = stage, type = restOrShop };
        }

        // 스테이지 4: 엘리트 미등장 시 강제 등장
        if (stage == 4 && !eliteSpawnedThisLayer)
        {
            eliteSpawnedThisLayer = true;
            return new StageInfo
            {
                stageNumber     = stage,
                type            = StageType.Elite,
                enemyId         = GetEliteId(layer),
                eliteGuaranteed = true,
            };
        }

        // 스테이지 2~4: 35% 확률로 엘리트
        if (stage >= 2 && !eliteSpawnedThisLayer && Random.value < 0.35f)
        {
            eliteSpawnedThisLayer = true;
            return new StageInfo
            {
                stageNumber = stage,
                type        = StageType.Elite,
                enemyId     = GetEliteId(layer),
            };
        }

        return new StageInfo
        {
            stageNumber = stage,
            type        = StageType.Normal,
            enemyId     = GetNormalId(layer),
        };
    }

    private string GetNormalId(int layer)
    {
        int l = Mathf.Clamp(layer, 1, normalPool.Count);
        string[] pool = normalPool[l];
        return pool[Random.Range(0, pool.Length)];
    }

    private string GetEliteId(int layer)
    {
        int l = Mathf.Clamp(layer, 1, elitePool.Count);
        string[] pool = elitePool[l];
        return pool[Random.Range(0, pool.Length)];
    }

    private string GetBossId(int layer)
    {
        if (bossTable.TryGetValue(layer, out string id)) return id;
        // 정의된 보스 없으면 마지막 계층 보스 재사용
        int maxLayer = 0;
        foreach (int key in bossTable.Keys)
            if (key > maxLayer) maxLayer = key;
        return bossTable[maxLayer];
    }

    public BattleReward GenerateReward(StageType stageType)
    {
        int cardCount = LootTable.GetCardRewardCount(currentLayer);
        if (stageType == StageType.Boss)  cardCount += 1;
        if (stageType == StageType.Elite) cardCount = Mathf.Max(cardCount, 2);

        return new BattleReward
        {
            cardChoices = LootTable.RollCardRewards(currentLayer, cardCount),
            gold        = LootTable.RollGold(currentLayer),
            foodId      = LootTable.RollFoodDrop(currentLayer),
        };
    }
}

public class BattleReward
{
    public List<string> cardChoices;
    public int          gold;
    public string       foodId;    // 획득한 식료품 (없으면 null)
    public string       relicId;   // 획득한 유물 (없으면 null, 엘리트/보스 전투에서만 나옴)
}
