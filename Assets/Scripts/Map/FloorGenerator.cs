using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 픽셀던전류 로그라이크 방식의 던전 생성기. MapGenerator(슬레이 더 스파이어 분기형 맵)를 대체한다.
// 겹치지 않는 방을 무작위로 흩뿌리고, 방 중심끼리 최소 스패닝 트리(+여분 간선 몇 개)로 이어서
// L자 복도를 깎은 뒤, 시작방/보스방을 정하고 나머지 방에 조우 타입을 배정, 적을 배치한다.
public static class FloorGenerator
{
    public const int Width  = 44;
    public const int Height = 30;
    public const int VisionRadius = 5;
    public const int EnemyDetectRadius = 4;

    private const int RoomCount   = 13; // 시작방 1 + 콘텐츠 방 11 + 보스방 1
    private const int MinRoomSize = 3;
    private const int MaxRoomSize = 5;
    private const int PlacementAttempts = 400;
    private const int ExtraEdges  = 3; // MST 위에 추가로 이어서 루프를 만드는 간선 수

    public static DungeonFloor Generate(int layer)
    {
        var floor = new DungeonFloor(layer, Width, Height);

        PlaceRooms(floor);
        ConnectRooms(floor);
        CarveRooms(floor);

        RoomInfo start = floor.Rooms[0];
        RoomInfo boss  = PickBossRoom(floor.Rooms, start);
        boss.roomType  = TileType.Boss;
        start.roomType = TileType.Empty;

        AssignRoomTypes(floor.Rooms, start, boss, layer);
        SpawnEnemies(floor, layer);

        floor.PlayerX = start.CenterX;
        floor.PlayerY = start.CenterY;
        floor.RevealAround(floor.PlayerX, floor.PlayerY, VisionRadius);

        return floor;
    }

    // ─────────────────────────────────────────
    // 방 배치 - 겹치지 않게 무작위로 흩뿌린다
    // ─────────────────────────────────────────
    private static void PlaceRooms(DungeonFloor floor)
    {
        int nextId = 0;
        int attemptsLeft = RoomCount * PlacementAttempts;

        while (floor.Rooms.Count < RoomCount && attemptsLeft > 0)
        {
            attemptsLeft--;

            int w = Random.Range(MinRoomSize, MaxRoomSize + 1);
            int h = Random.Range(MinRoomSize, MaxRoomSize + 1);
            int x = Random.Range(1, Width - w - 1);
            int y = Random.Range(1, Height - h - 1);

            var candidate = new RoomInfo { id = nextId, x = x, y = y, w = w, h = h };
            if (floor.Rooms.Any(r => Overlaps(r, candidate, padding: 1))) continue;

            floor.Rooms.Add(candidate);
            nextId++;
        }
    }

    private static bool Overlaps(RoomInfo a, RoomInfo b, int padding) =>
        a.x - padding < b.x + b.w && a.x + a.w + padding > b.x &&
        a.y - padding < b.y + b.h && a.y + a.h + padding > b.y;

    // ─────────────────────────────────────────
    // 방 연결 - 중심점끼리 최소 스패닝 트리(Prim) + 여분 간선, L자 복도로 깎기
    // ─────────────────────────────────────────
    private static void ConnectRooms(DungeonFloor floor)
    {
        var rooms = floor.Rooms;
        if (rooms.Count <= 1) return;

        var inTree = new HashSet<int> { 0 };
        var edges  = new List<(int a, int b)>();

        while (inTree.Count < rooms.Count)
        {
            int bestA = -1, bestB = -1;
            int bestDist = int.MaxValue;

            foreach (int a in inTree)
            {
                for (int b = 0; b < rooms.Count; b++)
                {
                    if (inTree.Contains(b)) continue;
                    int dist = ManhattanDist(rooms[a], rooms[b]);
                    if (dist < bestDist) { bestDist = dist; bestA = a; bestB = b; }
                }
            }

            if (bestB < 0) break;
            edges.Add((bestA, bestB));
            inTree.Add(bestB);
        }

        // 루프를 위한 여분 간선 - 무작위 방 쌍 몇 개를 추가로 이어서 외길만 있지 않게 함
        for (int i = 0; i < ExtraEdges && rooms.Count > 2; i++)
        {
            int a = Random.Range(0, rooms.Count);
            int b = Random.Range(0, rooms.Count);
            if (a != b) edges.Add((a, b));
        }

        foreach (var (a, b) in edges)
            CarveCorridor(floor, rooms[a], rooms[b]);
    }

    private static int ManhattanDist(RoomInfo a, RoomInfo b) =>
        Mathf.Abs(a.CenterX - b.CenterX) + Mathf.Abs(a.CenterY - b.CenterY);

    private static void CarveCorridor(DungeonFloor floor, RoomInfo a, RoomInfo b)
    {
        int ax = a.CenterX, ay = a.CenterY, bx = b.CenterX, by = b.CenterY;

        if (Random.value < 0.5f)
        {
            CarveHorizontal(floor, ax, bx, ay);
            CarveVertical(floor, ay, by, bx);
        }
        else
        {
            CarveVertical(floor, ay, by, ax);
            CarveHorizontal(floor, ax, bx, by);
        }
    }

    private static void CarveHorizontal(DungeonFloor floor, int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
            SetFloor(floor, x, y);
    }

    private static void CarveVertical(DungeonFloor floor, int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
            SetFloor(floor, x, y);
    }

    private static void SetFloor(DungeonFloor floor, int x, int y)
    {
        if (floor.InBounds(x, y)) floor.Tiles[x, y] = TileKind.Floor;
    }

    private static void CarveRooms(DungeonFloor floor)
    {
        foreach (var room in floor.Rooms)
            for (int x = room.x; x < room.x + room.w; x++)
                for (int y = room.y; y < room.y + room.h; y++)
                    floor.Tiles[x, y] = TileKind.Floor;
    }

    // 시작방에서 가장 먼 방을 보스방으로 삼는다 (그리드는 층 개념이 없어져서, 거리로 "가장 깊은 곳"을 대신함).
    private static RoomInfo PickBossRoom(List<RoomInfo> rooms, RoomInfo start) =>
        rooms.Where(r => r != start).OrderByDescending(r => ManhattanDist(r, start)).First();

    // ─────────────────────────────────────────
    // 방 타입 배정 - 성소는 게임 전체(런)에 1개만 나오도록 1층에서만 배정, 모닥불(Rest)은 층당 1개로 축소.
    // 풀 크기(9~10)가 남은 방 개수(11)보다 적어서 남는 방은 아래 foreach의 fallback으로 NormalEnemy가 됨.
    // ─────────────────────────────────────────
    private static void AssignRoomTypes(List<RoomInfo> rooms, RoomInfo start, RoomInfo boss, int layer)
    {
        var pool = new List<TileType>
        {
            TileType.NormalEnemy, TileType.NormalEnemy, TileType.NormalEnemy, TileType.NormalEnemy,
            TileType.GroupEnemy, TileType.GroupEnemy,
            TileType.EliteEnemy,
            TileType.Rest,
            TileType.Shop,
        };
        if (layer == 1) pool.Add(TileType.Shrine);
        Shuffle(pool);

        var remaining = rooms.Where(r => r != start && r != boss).ToList();
        Shuffle(remaining);

        foreach (var room in remaining)
            room.roomType = pool.Count > 0 ? Pop(pool) : TileType.NormalEnemy;
    }

    private static TileType Pop(List<TileType> pool)
    {
        TileType t = pool[pool.Count - 1];
        pool.RemoveAt(pool.Count - 1);
        return t;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ─────────────────────────────────────────
    // 적 배치
    // ─────────────────────────────────────────
    private static void SpawnEnemies(DungeonFloor floor, int layer)
    {
        int nextId = 0;
        foreach (var room in floor.Rooms)
        {
            switch (room.roomType)
            {
                case TileType.NormalEnemy:
                    SpawnOne(floor, room, PickNormal(layer), ref nextId);
                    break;
                case TileType.GroupEnemy:
                    SpawnOne(floor, room, PickNormal(layer), ref nextId);
                    SpawnOne(floor, room, PickNormal(layer), ref nextId);
                    break;
                case TileType.EliteEnemy:
                    SpawnOne(floor, room, PickElite(layer), ref nextId);
                    break;
                case TileType.Boss:
                    SpawnOne(floor, room, GetBossId(layer), ref nextId);
                    break;
            }
        }
    }

    // 방 안에서 다른 적이 없는 칸을 골라 배치한다 (집단 조우 2마리가 같은 타일에 겹치지 않게)
    private static void SpawnOne(DungeonFloor floor, RoomInfo room, string templateId, ref int nextId)
    {
        var freeCells = new List<(int x, int y)>();
        for (int x = room.x; x < room.x + room.w; x++)
            for (int y = room.y; y < room.y + room.h; y++)
                if (floor.EnemyAt(x, y) == null)
                    freeCells.Add((x, y));

        if (freeCells.Count == 0) return; // 방이 가득 참 (최소 방 크기 3×3 > 방당 최대 적 2마리라 실제로는 발생하지 않음)

        var cell = freeCells[Random.Range(0, freeCells.Count)];
        floor.Enemies.Add(new EnemySpawn
        {
            id = nextId++,
            roomId = room.id,
            enemyTemplateId = templateId,
            x = cell.x,
            y = cell.y,
        });
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
