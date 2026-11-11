using System.Collections.Generic;

// 맵 위 적 AI - 시야선(LOS) 감지, BFS 최단 경로 추적, 플레이어 접촉(전투 개시) 판정.
// 플레이어가 한 칸 움직일 때마다 DungeonManager.TryMove가 StepAll을 한 번 호출한다.
public static class EnemyAiSystem
{
    // 모든 적을 한 스텝 진행. 플레이어에게 닿은 적(전투 개시자)이 있으면 그 적을 반환한다.
    // 계단 안전 구역(floor.IsSafeZoneActive)이 살아 있는 동안 플레이어가 그 안에 있으면 감지·추적하지 않고,
    // 어떤 적도 안전 구역 칸으로 들어가지 않는다.
    public static EnemySpawn StepAll(DungeonFloor floor)
    {
        bool playerSafe = floor.IsInSafeZone(floor.PlayerX, floor.PlayerY);

        foreach (EnemySpawn e in floor.Enemies)
        {
            if (e.isDead) continue;

            // 시야선이 뚫려 있고 감지 반경 안이면 Chasing으로 전환 (벽 너머에선 감지하지 않음)
            bool canDetect = e.state == EnemyAiState.Idle || e.state == EnemyAiState.Wandering;
            if (canDetect && !playerSafe &&
                floor.HasLineOfSight(e.x, e.y, floor.PlayerX, floor.PlayerY))
            {
                int dist2 = (e.x - floor.PlayerX) * (e.x - floor.PlayerX) +
                            (e.y - floor.PlayerY) * (e.y - floor.PlayerY);
                if (dist2 <= FloorGenerator.EnemyDetectRadius * FloorGenerator.EnemyDetectRadius)
                    e.state = EnemyAiState.Chasing;
            }

            if (e.state == EnemyAiState.Wandering) { StepWander(floor, e); continue; }
            if (e.state != EnemyAiState.Chasing) continue;
            if (playerSafe) continue; // 안전 구역 안의 플레이어는 쫓지 않고 제자리에서 기다린다
            if (StepTowardPlayer(floor, e)) return e;
        }
        return null;
    }

    // 돌아다니는 엘리트 - WanderStepInterval 걸음마다 목표 방 중심으로 한 칸. 도착하거나 길이 막히면 다음 방을 고른다.
    private static void StepWander(DungeonFloor floor, EnemySpawn e)
    {
        if (++e.wanderStepCounter < FloorGenerator.WanderStepInterval) return;
        e.wanderStepCounter = 0;

        RoomInfo target = floor.Rooms.Find(r => r.id == e.wanderTargetRoomId);
        if (target == null || System.Math.Abs(e.x - target.CenterX) + System.Math.Abs(e.y - target.CenterY) <= 1)
        {
            RoomInfo here = floor.RoomAt(e.x, e.y);
            e.wanderTargetRoomId = FloorGenerator.PickWanderTarget(floor, here != null ? here.id : -1);
            return;
        }

        // 길 찾기는 다른 적을 무시하고 한다 - 방 출구에 가만히 선 일반몹 때문에 갇히지 않도록 (검증에서 약 3% 층에서 갇혔음)
        var path = BfsPath(floor, e.x, e.y, target.CenterX, target.CenterY, ignoreEnemies: true);
        if (path == null || path.Count < 2)
        {
            e.wanderTargetRoomId = FloorGenerator.PickWanderTarget(floor, target.id);
            return;
        }
        var next = path[1];
        if (TryStep(floor, e, next.x - e.x, next.y - e.y)) return;

        // 가는 칸에 가만히 있는(Idle) 일반몹이 서 있으면 자리를 바꿔 지나간다. 보스와 추적 중인 적은 건드리지 않는다.
        EnemySpawn blocker = floor.EnemyAt(next.x, next.y);
        RoomInfo blockerRoom = blocker != null ? floor.Rooms.Find(r => r.id == blocker.roomId) : null;
        if (blocker != null && blocker.state == EnemyAiState.Idle && blockerRoom != null && blockerRoom.roomType != TileType.Boss
            && !floor.IsInSafeZone(next.x, next.y))
        {
            (blocker.x, blocker.y) = (e.x, e.y);
            (e.x, e.y) = (next.x, next.y);
            return;
        }

        // 비켜 줄 수 없는 적(보스, 추적 중인 적)이 막고 있으면 적을 피하는 길로 돌아가고, 그런 길도 없으면 다른 방을 고른다
        var detour = BfsPath(floor, e.x, e.y, target.CenterX, target.CenterY);
        if (detour != null && detour.Count >= 2 && TryStep(floor, e, detour[1].x - e.x, detour[1].y - e.y)) return;
        e.wanderTargetRoomId = FloorGenerator.PickWanderTarget(floor, target.id);
    }

    // BFS 최단 경로로 한 칸 이동. 다음 칸이 플레이어 타일이면 이동 대신 접촉(전투 개시) 신호를 반환.
    private static bool StepTowardPlayer(DungeonFloor floor, EnemySpawn e)
    {
        var path = BfsPath(floor, e.x, e.y, floor.PlayerX, floor.PlayerY);
        if (path == null || path.Count < 2) return false;

        var next = path[1];
        if (next.x == floor.PlayerX && next.y == floor.PlayerY) return true;

        TryStep(floor, e, next.x - e.x, next.y - e.y);
        return false;
    }

    private static List<(int x, int y)> BfsPath(DungeonFloor floor, int sx, int sy, int tx, int ty, bool ignoreEnemies = false)
    {
        var start = (x: sx, y: sy);
        var goal  = (x: tx, y: ty);

        var queue  = new Queue<(int x, int y)>();
        var parent = new Dictionary<(int x, int y), (int x, int y)>();
        queue.Enqueue(start);
        parent[start] = start;

        int[] ddx = { 0, 0, 1, -1 };
        int[] ddy = { 1, -1, 0, 0 };

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur == goal) break;

            for (int i = 0; i < 4; i++)
            {
                var next = (x: cur.x + ddx[i], y: cur.y + ddy[i]);
                if (!floor.IsWalkable(next.x, next.y)) continue;
                if (parent.ContainsKey(next)) continue;
                // 플레이어 칸 자체는 목표로 허용, 다른 적이 막고 있으면 우회
                if (!ignoreEnemies && next != goal && floor.EnemyAt(next.x, next.y) != null) continue;
                // 계단 안전 구역은 살아 있는 동안 벽처럼 돌아간다
                if (floor.IsInSafeZone(next.x, next.y)) continue;
                parent[next] = cur;
                queue.Enqueue(next);
            }
        }

        if (!parent.ContainsKey(goal)) return null;

        var path = new List<(int x, int y)>();
        for (var node = goal; node != start; node = parent[node]) path.Add(node);
        path.Add(start);
        path.Reverse();
        return path;
    }

    private static bool TryStep(DungeonFloor floor, EnemySpawn e, int dx, int dy)
    {
        if (dx == 0 && dy == 0) return false;
        int nx = e.x + dx, ny = e.y + dy;
        if (!floor.IsWalkable(nx, ny)) return false;
        if (floor.EnemyAt(nx, ny) != null) return false;
        if (floor.IsInSafeZone(nx, ny)) return false; // 계단 안전 구역에는 들어가지 않는다
        if (nx == floor.PlayerX && ny == floor.PlayerY) return false; // 접촉 전투는 StepTowardPlayer가 처리

        e.x = nx;
        e.y = ny;
        return true;
    }
}
