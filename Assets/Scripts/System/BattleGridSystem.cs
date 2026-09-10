using System.Collections.Generic;
using System.Linq;

// 4단계(2026-09) - 전투를 그리드 위로 통합. 던전맵 추적용 EnemyAiSystem과 역할이 갈려서 분리했다:
// 이쪽은 "전투 중" 딱 그 방(RoomInfo) 범위 안에서만 적이 접근하고, 플레이어는 방 밖으로 나가면
// 도주로 취급된다 - 맵 전체를 넘나드는 EnemyAiSystem의 BFS와는 경계 조건이 달라 재사용하지 않았다.
public static class BattleGridSystem
{
    public enum MoveResult { Blocked, Moved, Fled }

    public static int Chebyshev(int x1, int y1, int x2, int y2) =>
        System.Math.Max(System.Math.Abs(x1 - x2), System.Math.Abs(y1 - y2));

    // (fx,fy)→(fx+dx,fy+dy) 한 칸 이동이 막혀 있지 않은지(목적지 통행 가능 + 대각선 코너컷 방지).
    // TryMovePlayer와 BattleUI의 인접 타일 하이라이트가 같은 기준을 쓰도록 여기 하나로 뽑아뒀다 -
    // 따로 판정하면 "눌리는데 반응 없는 칸"처럼 UI와 실제 이동 가능 여부가 어긋난다.
    public static bool CanStepTo(DungeonFloor floor, int fx, int fy, int dx, int dy)
    {
        int nx = fx + dx, ny = fy + dy;
        if (!floor.IsWalkable(nx, ny)) return false;

        // 대각선 이동은 양쪽 직교 칸이 전부 걸을 수 있어야 허용 - 안 그러면 벽 모서리를
        // 두 칸 다 막힌 채로 대각선으로만 뚫고 지나갈 수 있다(4방향 전용인 WASD/던전맵과 불일치).
        if (dx != 0 && dy != 0)
            return floor.IsWalkable(fx + dx, fy) && floor.IsWalkable(fx, fy + dy);

        return true;
    }

    // 플레이어 전투 중 이동. 방 안이면 그냥 이동, 방 경계 밖으로 나가면 도주 성공 -
    // 그 순간 인접해 있던 살아있는 적들은 opportunityAttackers로 반환되어 호출자가 이탈 공격을 처리한다.
    public static MoveResult TryMovePlayer(DungeonFloor floor, RoomInfo room, List<Enemy> enemies,
                                            int dx, int dy, out List<Enemy> opportunityAttackers)
    {
        opportunityAttackers = new List<Enemy>();

        if (!CanStepTo(floor, floor.PlayerX, floor.PlayerY, dx, dy)) return MoveResult.Blocked;

        int nx = floor.PlayerX + dx, ny = floor.PlayerY + dy;
        if (enemies.Any(e => e.IsAlive && e.x == nx && e.y == ny)) return MoveResult.Blocked;

        bool leavingRoom = !room.Contains(nx, ny);
        if (leavingRoom)
        {
            opportunityAttackers = enemies
                .Where(e => e.IsAlive && Chebyshev(e.x, e.y, floor.PlayerX, floor.PlayerY) <= 1)
                .ToList();
        }

        floor.PlayerX = nx;
        floor.PlayerY = ny;
        return leavingRoom ? MoveResult.Fled : MoveResult.Moved;
    }

    // 적 한 마리를 방 범위 안에서 플레이어 쪽으로 한 칸 접근시킨다.
    // 이미 인접해 있으면 움직이지 않고 true. 접근한 뒤 인접해졌으면 true(이번 턴은 접근만, 공격은 다음 턴).
    public static bool StepEnemyToward(DungeonFloor floor, RoomInfo room, Enemy enemy, List<Enemy> allEnemies)
    {
        if (Chebyshev(enemy.x, enemy.y, floor.PlayerX, floor.PlayerY) <= 1) return true;

        var path = BfsPath(floor, room, enemy.x, enemy.y, floor.PlayerX, floor.PlayerY, allEnemies);
        if (path == null || path.Count < 2) return false;

        var next = path[1];
        enemy.x = next.x;
        enemy.y = next.y;
        return false; // 이번 턴엔 접근만 함 (다음 턴에 인접 판정에서 공격 가능해짐)
    }

    // 방 범위 안(+겹치지 않은 칸)으로 제한한 4방향 BFS 최단 경로.
    private static List<(int x, int y)> BfsPath(DungeonFloor floor, RoomInfo room, int sx, int sy, int tx, int ty,
                                                  List<Enemy> allEnemies)
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
                if (!room.Contains(next.x, next.y)) continue; // 전투 중엔 적이 방 밖으로 나가서 접근하지 않는다
                if (parent.ContainsKey(next)) continue;
                if (next != goal && allEnemies.Any(e => e.IsAlive && e.x == next.x && e.y == next.y)) continue;
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
}
