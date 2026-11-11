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

    // 계단 도착 후 이만큼 걷는 동안 안전 구역이 유지된다 (DungeonManager.TryMove가 한 걸음마다 1씩 줄임)
    public const int SafeTurns = 5;
    // 돌아다니는 엘리트는 플레이어가 이만큼 걸을 때마다 한 칸 움직인다 (추적 중일 땐 매 걸음)
    public const int WanderStepInterval = 2;

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
        SpawnRoamingElite(floor, layer, start, boss);

        // 계단 = 시작 방 중심. 방 크기가 무작위여도 최소 방 크기 기준의 정사각형을 안전 구역으로 잡는다.
        floor.PlayerX = start.CenterX;
        floor.PlayerY = start.CenterY;
        floor.SafeZone = new RoomInfo
        {
            id = -1,
            x  = floor.PlayerX - MinRoomSize / 2,
            y  = floor.PlayerY - MinRoomSize / 2,
            w  = MinRoomSize,
            h  = MinRoomSize,
        };
        floor.SafeTurnsLeft = SafeTurns;
        PlacePickups(floor, start, boss);
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
    // 엘리트 방은 만들지 않는다(2026-11-11) - 엘리트는 SpawnRoamingElite로 층을 돌아다니고, 그 자리는 빈 방으로 둔다
    // (일반 전투 수가 늘지 않도록 Empty를 대신 넣음).
    // ─────────────────────────────────────────
    private static void AssignRoomTypes(List<RoomInfo> rooms, RoomInfo start, RoomInfo boss, int layer)
    {
        var pool = new List<TileType>
        {
            TileType.NormalEnemy, TileType.NormalEnemy, TileType.NormalEnemy, TileType.NormalEnemy,
            TileType.GroupEnemy, TileType.GroupEnemy,
            TileType.Empty,
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
                    // 깊이 내려갈수록 일반 방에 두 마리(각자 원래 능력치)가 같이 있을 확률이 오른다 (2026-11-11)
                    // 한 마리씩은 층마다 플레이어보다 조금 덜 강해지고, 대신 마릿수로 압박한다.
                    if (Random.value < NormalPairChance(layer))
                        SpawnOne(floor, room, PickNormal(layer), ref nextId);
                    break;
                case TileType.GroupEnemy:
                    // 집단 조우: 일반몹 3마리가 각자 1/3 능력치로 나와서, 셋을 합치면 일반몹 1마리 몫이다 (2026-11-10)
                    for (int i = 0; i < GroupSize; i++)
                        SpawnOne(floor, room, PickNormal(layer), ref nextId, GroupMemberScale);
                    break;
                case TileType.EliteEnemy: // 지금은 배정되지 않음 (돌아다니는 엘리트로 대체) - 예전 데이터 호환용으로만 남김
                    SpawnOne(floor, room, PickElite(layer), ref nextId);
                    break;
                case TileType.Boss:
                    SpawnOne(floor, room, GetBossId(layer), ref nextId);
                    break;
            }
        }
    }

    // 층마다 엘리트 1마리가 방 사이를 돌아다닌다. 시작 방에서 먼 쪽 절반의 방에서 출발해 바로 마주치지 않게 하고,
    // roomId는 자기만의 음수 값으로 둬서 지나가던 방의 일반몹과 한 전투로 묶이지 않게 한다.
    private static void SpawnRoamingElite(DungeonFloor floor, int layer, RoomInfo start, RoomInfo boss)
    {
        var candidates = floor.Rooms.Where(r => r != start && r != boss)
                                    .OrderByDescending(r => ManhattanDist(r, start)).ToList();
        if (candidates.Count == 0) return;
        var far = candidates.Take(Mathf.Max(1, candidates.Count / 2)).ToList();
        RoomInfo home = far[Random.Range(0, far.Count)];

        int before = floor.Enemies.Count;
        int nextId = floor.Enemies.Count == 0 ? 0 : floor.Enemies.Max(e => e.id) + 1;
        SpawnOne(floor, home, PickElite(layer), ref nextId);
        if (floor.Enemies.Count == before) return; // 빈 칸이 없으면 생략 (실제로는 일어나지 않음)

        EnemySpawn elite = floor.Enemies[floor.Enemies.Count - 1];
        elite.roomId = RoamingRoomId(elite.id);
        elite.state  = EnemyAiState.Wandering;
        elite.wanderTargetRoomId = PickWanderTarget(floor, home.id);
    }

    public static int RoamingRoomId(int spawnId) => -1000 - spawnId;

    // 보물상자에서 튀어나올 적(미믹) 등 - 그 층 일반몹 하나
    public static string RandomNormalId(int layer) => PickNormal(layer);

    // ─────────────────────────────────────────
    // 보물상자·바닥 보상 배치 (TreasureTable, 2026-11-11)
    // 시작 방·보스 방·안전 구역·적이 선 칸·방 중심(휴식/상점/성소 아이콘 자리)은 피한다.
    // 상자는 콘텐츠 방 안에 방마다 최대 1개, 바닥 보상은 복도를 포함한 아무 빈 바닥에 놓는다.
    // ─────────────────────────────────────────
    private static void PlacePickups(DungeonFloor floor, RoomInfo start, RoomInfo boss)
    {
        bool Free(int x, int y) =>
            floor.IsWalkable(x, y) && floor.EnemyAt(x, y) == null && floor.PickupAt(x, y) == null
            && !start.Contains(x, y) && !boss.Contains(x, y)
            && (floor.SafeZone == null || !floor.SafeZone.Contains(x, y))
            && !floor.Rooms.Any(r => r.CenterX == x && r.CenterY == y);

        var rooms = floor.Rooms.Where(r => r != start && r != boss).ToList();
        Shuffle(rooms);
        int chests = 0;
        foreach (RoomInfo room in rooms)
        {
            if (chests >= TreasureTable.ChestsPerFloor) break;
            var cells = new List<(int x, int y)>();
            for (int x = room.x; x < room.x + room.w; x++)
                for (int y = room.y; y < room.y + room.h; y++)
                    if (Free(x, y)) cells.Add((x, y));
            if (cells.Count == 0) continue;

            var c = cells[Random.Range(0, cells.Count)];
            floor.Pickups.Add(new MapPickup { x = c.x, y = c.y, kind = PickupKind.Chest });
            chests++;
        }

        var tiles = new List<(int x, int y)>();
        for (int x = 0; x < floor.Width; x++)
            for (int y = 0; y < floor.Height; y++)
                if (Free(x, y)) tiles.Add((x, y));
        for (int i = 0; i < TreasureTable.FloorLootPerFloor && tiles.Count > 0; i++)
        {
            int k = Random.Range(0, tiles.Count);
            floor.Pickups.Add(new MapPickup { x = tiles[k].x, y = tiles[k].y, kind = PickupKind.FloorLoot });
            tiles.RemoveAt(k);
        }
    }

    // 다음으로 향할 방 - 시작 방·보스 방·지금 방을 뺀 콘텐츠 방 중 무작위 (EnemyAiSystem도 도착할 때마다 다시 고른다)
    public static int PickWanderTarget(DungeonFloor floor, int currentRoomId)
    {
        RoomInfo start = floor.Rooms[0];
        var options = floor.Rooms.Where(r => r != start && r.roomType != TileType.Boss && r.id != currentRoomId).ToList();
        return options.Count == 0 ? currentRoomId : options[Random.Range(0, options.Count)].id;
    }

    public const int   GroupSize        = 3;
    public const float GroupMemberScale = 1f / GroupSize;

    // 층별로 일반 방에 두 번째 일반몹이 같이 나올 확률 (1층, 2층, 3층). Tools/BalanceSim이 이 배열을 그대로 읽는다.
    private static readonly float[] NormalPairChances = { 0f, 0.30f, 0.60f };
    public static float NormalPairChance(int layer) =>
        NormalPairChances[Mathf.Clamp(layer, 1, NormalPairChances.Length) - 1];

    // 방 안에서 다른 적이 없는 칸을 골라 배치한다 (집단 조우 무리원이 같은 타일에 겹치지 않게)
    private static void SpawnOne(DungeonFloor floor, RoomInfo room, string templateId, ref int nextId, float statScale = 1f)
    {
        var freeCells = new List<(int x, int y)>();
        for (int x = room.x; x < room.x + room.w; x++)
            for (int y = room.y; y < room.y + room.h; y++)
                if (floor.EnemyAt(x, y) == null)
                    freeCells.Add((x, y));

        if (freeCells.Count == 0) return; // 방이 가득 참 (최소 방 크기 3×3 = 9칸 > 방당 최대 적 3마리라 실제로는 발생하지 않음)

        var cell = freeCells[Random.Range(0, freeCells.Count)];
        floor.Enemies.Add(new EnemySpawn
        {
            id = nextId++,
            roomId = room.id,
            enemyTemplateId = templateId,
            x = cell.x,
            y = cell.y,
            statScale = statScale,
        });
    }

    private static string PickNormal(int layer)
    {
        string[] pool = layer <= 1 ? new[] { "slime", "thief", "bat_swarm", "goblin_scout", "skeleton_soldier" }
                      : layer == 2 ? new[] { "orc", "poison_spider", "orc_grunt", "swamp_viper", "bandit_raider" }
                      :              new[] { "cursed_warrior", "shadow_witch", "bone_golem", "wraith", "fallen_knight" };
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
