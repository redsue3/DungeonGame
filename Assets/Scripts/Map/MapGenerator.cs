using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public static DungeonMap Generate(int layer)
    {
        var map = new DungeonMap(layer);

        // 플레이어: 왼쪽 가운데
        map.SetPlayer(0, DungeonMap.HEIGHT / 2);

        // 보스: 오른쪽 가운데
        map.SetTile(DungeonMap.WIDTH - 1, DungeonMap.HEIGHT / 2, new MapTile
        {
            type     = TileType.Boss,
            enemyIds = new[] { GetBossId(layer) }
        });

        // 배치 가능한 빈 좌표 수집
        var positions = new List<(int x, int y)>();
        for (int x = 1; x < DungeonMap.WIDTH - 1; x++)
        for (int y = 0; y < DungeonMap.HEIGHT;    y++)
            positions.Add((x, y));

        Shuffle(positions);
        int idx = 0;

        // 일반 조우 4개
        for (int i = 0; i < 4 && idx < positions.Count; i++, idx++)
        {
            var (x, y) = positions[idx];
            map.SetTile(x, y, new MapTile
            {
                type     = TileType.NormalEnemy,
                enemyIds = new[] { PickNormal(layer) }
            });
        }

        // 집단 조우 2개 (적 2마리)
        for (int i = 0; i < 2 && idx < positions.Count; i++, idx++)
        {
            var (x, y) = positions[idx];
            map.SetTile(x, y, new MapTile
            {
                type     = TileType.GroupEnemy,
                enemyIds = new[] { PickNormal(layer), PickNormal(layer) }
            });
        }

        // 엘리트 1개
        if (idx < positions.Count)
        {
            var (x, y) = positions[idx++];
            map.SetTile(x, y, new MapTile
            {
                type     = TileType.EliteEnemy,
                enemyIds = new[] { PickElite(layer) }
            });
        }

        // 휴식 2개
        for (int i = 0; i < 2 && idx < positions.Count; i++, idx++)
        {
            var (x, y) = positions[idx];
            map.SetTile(x, y, new MapTile { type = TileType.Rest });
        }

        // 상점 1개
        if (idx < positions.Count)
        {
            var (x, y) = positions[idx];
            map.SetTile(x, y, new MapTile { type = TileType.Shop });
        }

        return map;
    }

    // ───────────────────────────────────────────
    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp    = list[i];
            list[i]  = list[j];
            list[j]  = tmp;
        }
    }

    private static string PickNormal(int layer)
    {
        string[] pool = layer <= 1 ? new[] { "slime", "thief" }
                      : layer == 2 ? new[] { "orc", "poison_spider" }
                      :              new[] { "cursed_warrior", "shadow_witch" };
        return pool[Random.Range(0, pool.Length)];
    }

    private static string PickElite(int layer)
    {
        string[] pool = layer <= 1 ? new[] { "goblin_elite", "skeleton_elite" }
                      : layer == 2 ? new[] { "dark_mage_elite" }
                      :              new[] { "death_knight_elite" };
        return pool[Random.Range(0, pool.Length)];
    }

    private static string GetBossId(int layer) =>
        layer <= 1 ? "giant_slime_boss" :
        layer == 2 ? "orc_warchief_boss" : "demon_lord_boss";
}
