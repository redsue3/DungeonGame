// 전투 한 판 - 실제 BattleManager에 방·적·플레이어를 넣고, 사람 대신 단순한 규칙으로 카드를 내고 움직인다.
using System;
using System.Collections.Generic;
using System.Linq;
using URandom = UnityEngine.Random;

public class FightResult
{
    public BattleState State;
    public float       HpLost;     // 전투 시작 최대 HP 대비 잃은 비율 (0~1)
    public float       Time;       // 전투가 끝난 시각 (속도 1인 적의 행동 횟수와 같은 단위)
    public int         Actions;
    public bool        TimedOut;
    public bool Win => State == BattleState.Win;
}

public static class Arena
{
    // 방: 콘텐츠 방(3~5칸) 중 가장 큰 5x5에, DungeonManager.BuildBattleBounds처럼 2칸 여유를 둔 9x9 전투 범위
    public const int HomeX = 5, HomeY = 5, HomeSize = 5;
    public const int MaxActions = 3000;

    public static readonly (int dx, int dy)[] Dirs8 = { (0, 1), (0, -1), (1, 0), (-1, 0), (1, 1), (1, -1), (-1, 1), (-1, -1) };

    public static (DungeonFloor floor, RoomInfo room) MakeRoom(int layer)
    {
        var floor = new DungeonFloor(layer, 15, 15);
        for (int x = 1; x < 14; x++)
            for (int y = 1; y < 14; y++)
                floor.Tiles[x, y] = TileKind.Floor;
        var room = new RoomInfo { id = 1, x = HomeX - 2, y = HomeY - 2, w = HomeSize + 4, h = HomeSize + 4, roomType = TileType.NormalEnemy };
        floor.Rooms.Add(room);
        return (floor, room);
    }

    // 조우 배치: 플레이어는 방 가장자리에서 들어와 첫 적과 맞닿은 채 시작한다(걸어 들어가거나 쫓겨 붙잡힌 상황).
    // 나머지 무리원은 방 안 아무 빈 칸.
    public static void Place(DungeonFloor floor, List<Enemy> enemies)
    {
        int side = URandom.Range(0, 4), k = URandom.Range(0, HomeSize);
        int px = side == 0 ? HomeX : side == 1 ? HomeX + HomeSize - 1 : HomeX + k;
        int py = side == 2 ? HomeY : side == 3 ? HomeY + HomeSize - 1 : HomeY + k;
        floor.PlayerX = px; floor.PlayerY = py;

        var taken = new HashSet<(int, int)> { (px, py) };
        bool InHome(int x, int y) => x >= HomeX && x < HomeX + HomeSize && y >= HomeY && y < HomeY + HomeSize;

        for (int i = 0; i < enemies.Count; i++)
        {
            (int x, int y) cell;
            if (i == 0)
            {
                var near = Dirs8.Select(d => (x: px + d.dx, y: py + d.dy)).Where(c => InHome(c.x, c.y) && !taken.Contains(c)).ToList();
                cell = near[URandom.Range(0, near.Count)];
            }
            else
            {
                var free = new List<(int x, int y)>();
                for (int x = HomeX; x < HomeX + HomeSize; x++)
                    for (int y = HomeY; y < HomeY + HomeSize; y++)
                        if (!taken.Contains((x, y))) free.Add((x, y));
                cell = free[URandom.Range(0, free.Count)];
            }
            taken.Add(cell);
            enemies[i].x = cell.x;
            enemies[i].y = cell.y;
        }
    }

    public static FightResult Fight(PlayerCharacter player, List<Enemy> enemies, int layer)
    {
        var (floor, room) = MakeRoom(layer);
        Place(floor, enemies);

        var dm = new DungeonManager { CurrentLayer = layer };
        DungeonManager.Instance = dm;
        var bm = new BattleManager();
        BattleManager.Instance = bm;

        int hp0 = player.currentHp, max0 = player.maxHp;
        bm.StartBattle(player, enemies, floor, room);

        int actions = 0;
        while (IsRunning(bm) && actions < MaxActions)
        {
            actions++;
            Act(bm, player, floor, room);
        }

        return new FightResult
        {
            State    = bm.CurrentState,
            HpLost   = Math.Max(0f, (float)(hp0 - player.currentHp) / max0),
            Time     = bm.BattleTime,
            Actions  = actions,
            TimedOut = IsRunning(bm),
        };
    }

    public static bool IsRunning(BattleManager bm) => bm.CurrentState == BattleState.PlayerTurn || bm.CurrentState == BattleState.EnemyTurn;

    // 이번에 들어올 피해 추정 - 2칸 안의 적이 다음 행동으로 때리거나 독·화상을 거는 양
    public static int Threat(PlayerCharacter p, DungeonFloor floor, List<Enemy> alive)
    {
        int sum = 0;
        foreach (Enemy e in alive)
        {
            if (BattleGridSystem.Chebyshev(e.x, e.y, floor.PlayerX, floor.PlayerY) > 2) continue;
            EnemyAction a = e.PeekNextAction();
            if (a.intent == EnemyIntent.Attack) sum += DamageCalculator.Calculate(a.value, e.attackPower);
            else if (a.intent == EnemyIntent.Poison) sum += (int)(a.value * 1.5f);
            else if (a.intent == EnemyIntent.Burn) sum += a.value * 2;
        }
        return sum;
    }

    // 플레이어 대신 한 번 행동한다.
    //   공격 턴: 쓸 만한 카드 중 코스트 대비 가치가 가장 높은 것 → 없고 사거리 밖이라 못 쓴 공격 카드가 있으면 적에게 한 칸 → 아니면 턴 종료
    //   정비 중: 정비 넘기기 (정비 중 이동은 2배 느려서 기다리는 쪽이 낫다)
    //   --kite: 원거리 카드(사거리 2 이상)가 손에 있는데 적이 붙어 있으면 먼저 한 칸 물러난다 (정비 중에도)
    public static bool Kite;

    public static void Act(BattleManager bm, PlayerCharacter p, DungeonFloor floor, RoomInfo room)
    {
        if (!bm.CanAct) { bm.EndPlayerTurn(); return; }

        var alive = bm.GetEnemies().Where(e => e.IsAlive).ToList();
        if (Kite && p.deck.GetHand().Any(c => c.attackRange >= 2 && (c.damage > 0 || c.poisonApply > 0 || c.burnApply > 0))
                 && alive.Any(e => BattleGridSystem.Chebyshev(e.x, e.y, floor.PlayerX, floor.PlayerY) <= 1)
                 && StepAway(bm, floor, room, alive)) return;

        if (bm.Phase == PlayerPhase.Recovery) { bm.EndPlayerTurn(); return; }

        List<Card> hand = p.deck.GetHand();
        int incoming = Threat(p, floor, alive);
        int atkBonus = p.GetFinalAttackBonus() + p.pendingAttackBonus;

        int best = -1; Enemy bestTarget = null; double bestScore = 0;
        bool outOfRange = false;
        for (int i = 0; i < hand.Count; i++)
        {
            Card c = hand[i];
            if (c.cost > p.currentCost) continue;

            bool offense = c.damage > 0 || c.poisonApply > 0 || c.burnApply > 0;
            Enemy target = null;
            int targets = 1;
            if (offense)
            {
                if (c.isAoe) targets = alive.Count;
                else
                {
                    var inRange = alive.Where(e => BattleGridSystem.Chebyshev(e.x, e.y, floor.PlayerX, floor.PlayerY) <= c.attackRange).ToList();
                    if (inRange.Count == 0) { outOfRange = true; continue; }
                    // 막타를 노린다: 이 카드로 쓰러뜨릴 수 있는 적 중 HP가 가장 높은 적, 없으면 HP가 가장 낮은 적
                    int hit = DamageCalculator.Calculate(c.damage, atkBonus);
                    var killable = inRange.Where(e => c.damage > 0 && e.currentHp + e.block <= hit).ToList();
                    target = killable.Count > 0 ? killable.OrderByDescending(e => e.currentHp).First()
                                                : inRange.OrderBy(e => e.currentHp + e.block).First();
                }
            }

            double v = 0;
            if (c.damage > 0) v += (c.damage + atkBonus) * targets;
            v += c.poisonApply * 2.5 * targets + c.burnApply * 3.0 * targets;
            if (c.block > 0) v += Math.Min(Math.Max(0, c.block + p.pendingDefenseBonus + p.blockBonus), Math.Max(0, incoming - p.block));
            v += Math.Min(c.healAmount, p.maxHp - p.currentHp);
            v += c.drawCount * 3 + c.strengthGain * 4 + c.buffNextAttack * 0.8 + c.buffNextDefense * 0.4 - c.selfDamage * 0.7;
            if (c.selfDamage >= p.currentHp) v = -1;

            double score = c.cost == 0 ? v * 100 : v / c.cost;
            if (v > 0.5 && score > bestScore) { bestScore = score; best = i; bestTarget = target; }
        }

        if (best >= 0 && bm.UseCard(best, bestTarget)) return;

        if (outOfRange && StepTowardNearest(bm, floor, room, alive)) return;

        bm.EndPlayerTurn();
    }

    // 가장 가까운 적과의 거리가 가장 멀어지는 칸으로 한 칸 (방 밖으로는 나가지 않는다)
    private static bool StepAway(BattleManager bm, DungeonFloor floor, RoomInfo room, List<Enemy> alive)
    {
        int MinDist(int x, int y) => alive.Min(e => BattleGridSystem.Chebyshev(e.x, e.y, x, y));
        int cur = MinDist(floor.PlayerX, floor.PlayerY);
        var options = Dirs8
            .Select(d => (d.dx, d.dy, nx: floor.PlayerX + d.dx, ny: floor.PlayerY + d.dy))
            .Where(o => room.Contains(o.nx, o.ny) && BattleGridSystem.CanStepTo(floor, floor.PlayerX, floor.PlayerY, o.dx, o.dy)
                        && !alive.Any(e => e.x == o.nx && e.y == o.ny))
            .OrderByDescending(o => MinDist(o.nx, o.ny))
            // 같은 거리면 방 가운데 쪽(구석에 몰리지 않게)
            .ThenBy(o => BattleGridSystem.Chebyshev(o.nx, o.ny, room.CenterX, room.CenterY));
        foreach (var o in options)
        {
            if (MinDist(o.nx, o.ny) <= cur) break;
            if (bm.TryMovePlayer(o.dx, o.dy)) return true;
        }
        return false;
    }

    private static bool StepTowardNearest(BattleManager bm, DungeonFloor floor, RoomInfo room, List<Enemy> alive)
    {
        if (alive.Count == 0) return false;
        Enemy near = alive.OrderBy(e => BattleGridSystem.Chebyshev(e.x, e.y, floor.PlayerX, floor.PlayerY)).First();
        int cur = BattleGridSystem.Chebyshev(near.x, near.y, floor.PlayerX, floor.PlayerY);
        foreach (var d in Dirs8.OrderBy(d => BattleGridSystem.Chebyshev(near.x, near.y, floor.PlayerX + d.dx, floor.PlayerY + d.dy)))
        {
            int nx = floor.PlayerX + d.dx, ny = floor.PlayerY + d.dy;
            if (BattleGridSystem.Chebyshev(near.x, near.y, nx, ny) >= cur) break;
            if (!room.Contains(nx, ny)) continue; // 방 밖으로 나가면 도주가 되니 피한다
            if (bm.TryMovePlayer(d.dx, d.dy)) return true;
        }
        return false;
    }
}
