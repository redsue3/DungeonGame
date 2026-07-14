using System.Collections.Generic;

// 맵 위 적 AI - 시야선(LOS) 감지, BFS 최단 경로 추적, 플레이어 접촉(전투 개시) 판정.
// 플레이어가 한 칸 움직일 때마다 DungeonManager.TryMove가 StepAll을 한 번 호출한다.
public static class EnemyAiSystem
{
    // 모든 적을 한 스텝 진행. 플레이어에게 닿은 적(전투 개시자)이 있으면 그 적을 반환한다.
    public static EnemySpawn StepAll(DungeonFloor floor)
    {
        foreach (EnemySpawn e in floor.Enemies)
        {
            if (e.isDead) continue;

            // 시야선이 뚫려 있고 감지 반경 안이면 Chasing으로 전환 (벽 너머에선 감지하지 않음)
            if (e.state == EnemyAiState.Idle &&
                floor.HasLineOfSight(e.x, e.y, floor.PlayerX, floor.PlayerY))
            {
                int dist2 = (e.x - floor.PlayerX) * (e.x - floor.PlayerX) +
                            (e.y - floor.PlayerY) * (e.y - floor.PlayerY);
                if (dist2 <= FloorGenerator.EnemyDetectRadius * FloorGenerator.EnemyDetectRadius)
                    e.state = EnemyAiState.Chasing;
            }

            if (e.state != EnemyAiState.Chasing) continue;
            if (StepTowardPlayer(floor, e)) return e;
        }
        return null;
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

    private static List<(int x, int y)> BfsPath(DungeonFloor floor, int sx, int sy, int tx, int ty)
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
                if (next != goal && floor.EnemyAt(next.x, next.y) != null) continue;
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
        if (nx == floor.PlayerX && ny == floor.PlayerY) return false; // 접촉 전투는 StepTowardPlayer가 처리

        e.x = nx;
        e.y = ny;
        return true;
    }
}
