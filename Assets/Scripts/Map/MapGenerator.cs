using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 슬레이 더 스파이어 스타일의 분기형 던전 맵 생성기.
// 층(floor)마다 여러 방(노드)을 두고 경로 여러 개를 랜덤 워크로 그어 서로 연결한 뒤,
// 고립된 노드가 없도록 보정하고, 각 노드에 조우 타입을 배정한다.
// 예전처럼 네모난 그리드에 무작위로 흩뿌리면 플레이어 주변이 대부분 빈 칸이라
// 클릭해도 반응이 없는 칸이 태반이었는데, 이제는 실제로 갈 수 있는 방만 존재하고
// 전부 선으로 이어져 있어서 어디로 갈 수 있는지 항상 명확하다.
public static class MapGenerator
{
    // 보스 층을 제외한 인코운터 층별 방 개수
    private static readonly int[] FloorNodeCounts = { 2, 3, 3, 2, 1 };
    private const int PathCount = 6; // 층 사이를 잇는 경로 수 (많을수록 갈래/합류가 많아짐)

    public static DungeonMap Generate(int layer)
    {
        int floorCount = FloorNodeCounts.Length + 1; // 인코운터 층들 + 보스 층
        var map = new DungeonMap(layer, floorCount);
        var floors = new List<MapNode>[floorCount];
        int nextId = 0;

        for (int f = 0; f < FloorNodeCounts.Length; f++)
        {
            int count = FloorNodeCounts[f];
            floors[f] = new List<MapNode>();
            for (int i = 0; i < count; i++)
            {
                var node = new MapNode
                {
                    id    = nextId++,
                    floor = f,
                    x     = count == 1 ? 0.5f : (float)i / (count - 1),
                };
                floors[f].Add(node);
                map.Nodes.Add(node);
            }
        }

        // 보스 층 (항상 노드 1개, 가운데 배치)
        int bossFloor = FloorNodeCounts.Length;
        var bossNode = new MapNode
        {
            id       = nextId++,
            floor    = bossFloor,
            x        = 0.5f,
            type     = TileType.Boss,
            enemyIds = new[] { GetBossId(layer) },
        };
        floors[bossFloor] = new List<MapNode> { bossNode };
        map.Nodes.Add(bossNode);

        AssignRoomTypes(floors);
        ConnectFloors(floors);

        foreach (var node in map.Nodes)
        {
            switch (node.type)
            {
                case TileType.NormalEnemy: node.enemyIds = new[] { PickNormal(layer) }; break;
                case TileType.GroupEnemy:  node.enemyIds = new[] { PickNormal(layer), PickNormal(layer) }; break;
                case TileType.EliteEnemy:  node.enemyIds = new[] { PickElite(layer) }; break;
            }
        }

        return map;
    }

    // ─────────────────────────────────────────
    // 방 타입 배정 (예전 MapGenerator와 동일한 전체 비율을 층에 나눠 배치)
    // ─────────────────────────────────────────
    private static void AssignRoomTypes(List<MapNode>[] floors)
    {
        int lastEncounterFloor = FloorNodeCounts.Length - 1;

        var pool = new List<TileType>
        {
            TileType.NormalEnemy, TileType.NormalEnemy, TileType.NormalEnemy, TileType.NormalEnemy,
            TileType.GroupEnemy, TileType.GroupEnemy,
            TileType.EliteEnemy,
            TileType.Rest, TileType.Rest,
            TileType.Shop,
            TileType.Shrine,
        };
        Shuffle(pool);

        // 0층: 전투(단일/집단)만 등장 - 시작하자마자 엘리트/휴식/상점/성소를 만나지 않게
        foreach (var node in floors[0])
            node.type = TakeFrom(pool, TileType.NormalEnemy, TileType.GroupEnemy);

        // 보스 직전 층: 휴식 보장 (보스 앞에서 숨 돌릴 곳)
        foreach (var node in floors[lastEncounterFloor])
            node.type = TakeFrom(pool, TileType.Rest);

        // 남은 중간 층들에 나머지 풀을 무작위로 분배
        var remainingNodes = new List<MapNode>();
        for (int f = 1; f < lastEncounterFloor; f++)
            remainingNodes.AddRange(floors[f]);
        Shuffle(remainingNodes);

        foreach (var node in remainingNodes)
            node.type = pool.Count > 0 ? Pop(pool) : TileType.NormalEnemy;
    }

    private static TileType TakeFrom(List<TileType> pool, params TileType[] candidates)
    {
        foreach (var c in candidates)
        {
            int idx = pool.IndexOf(c);
            if (idx >= 0) { pool.RemoveAt(idx); return c; }
        }
        return candidates[0];
    }

    private static TileType Pop(List<TileType> pool)
    {
        TileType t = pool[pool.Count - 1];
        pool.RemoveAt(pool.Count - 1);
        return t;
    }

    // ─────────────────────────────────────────
    // 경로 연결 - 여러 경로를 랜덤 워크로 그은 뒤, 고립된 노드가 없도록 보정
    // ─────────────────────────────────────────
    private static void ConnectFloors(List<MapNode>[] floors)
    {
        int lastEncounterFloor = FloorNodeCounts.Length - 1;
        int bossFloor          = FloorNodeCounts.Length;

        for (int p = 0; p < PathCount; p++)
        {
            int cur = Random.Range(0, floors[0].Count);
            for (int f = 0; f < lastEncounterFloor; f++)
            {
                int nextCount = floors[f + 1].Count;
                int step = Random.Range(-1, 2); // -1, 0, 1
                int next = Mathf.Clamp(cur + step, 0, nextCount - 1);
                AddEdge(floors[f][cur], floors[f + 1][next]);
                cur = next;
            }
        }

        // 들어오는 경로가 없는 노드 보정 (0층은 시작점이라 제외)
        for (int f = 1; f <= lastEncounterFloor; f++)
        {
            foreach (var node in floors[f])
            {
                bool hasIncoming = floors[f - 1].Any(n => n.next.Contains(node.id));
                if (!hasIncoming)
                    AddEdge(ClosestByX(floors[f - 1], node), node);
            }
        }

        // 나가는 경로가 없는 노드 보정 (보스로의 연결은 아래에서 별도 처리)
        for (int f = 0; f < lastEncounterFloor; f++)
        {
            foreach (var node in floors[f])
            {
                if (node.next.Count == 0)
                    AddEdge(node, ClosestByX(floors[f + 1], node));
            }
        }

        // 보스 직전 층은 전부 보스로 연결
        MapNode boss = floors[bossFloor][0];
        foreach (var node in floors[lastEncounterFloor])
            AddEdge(node, boss);
    }

    private static void AddEdge(MapNode from, MapNode to)
    {
        if (!from.next.Contains(to.id)) from.next.Add(to.id);
    }

    // 가로 위치(x)가 가장 가까운 노드를 찾는다 (연결선이 너무 꼬이지 않게)
    private static MapNode ClosestByX(List<MapNode> candidates, MapNode reference)
    {
        MapNode best = candidates[0];
        float bestDist = Mathf.Abs(best.x - reference.x);
        foreach (var c in candidates)
        {
            float dist = Mathf.Abs(c.x - reference.x);
            if (dist < bestDist) { best = c; bestDist = dist; }
        }
        return best;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
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
