// 전투 흐름 확인 - 시간 기반 전투 규칙(2026-11-11)이 코드에서 그대로 동작하는지 하나씩 본다.
// 숫자(MoveTime 등)를 바꿔도 통과하도록 BattleManager의 static 값을 읽어서 비교한다.
using System;
using System.Collections.Generic;
using System.Linq;
using URandom = UnityEngine.Random;

public static class FlowChecks
{
    private static int pass, fail;

    private static void Check(string name, bool ok, string detail = "")
    {
        if (ok) pass++; else fail++;
        Console.WriteLine($"{(ok ? "통과" : "실패")}  {name}{(ok || detail == "" ? "" : "  - " + detail)}");
    }

    private static bool Near(float a, float b) => Math.Abs(a - b) < 1e-4f;

    private static Enemy Dummy(int hp, EnemyIntent intent, int value, float speed = 1f)
    {
        var e = new Enemy("허수아비", hp, 0, 0, 0);
        var phase = new EnemyPhase { hpThreshold = 1f };
        phase.pattern.Add(new EnemyAction(intent, value, "테스트 행동"));
        e.AddPhase(phase);
        e.speed = speed;
        return e;
    }

    private static PlayerCharacter Player(CharacterClass cls, string card, int count = 10)
    {
        var p = new PlayerCharacter(cls);
        for (int i = 0; i < count; i++) p.deck.AddCard(CardDatabase.Create(card));
        return p;
    }

    private class Setup
    {
        public BattleManager  bm;
        public DungeonManager dm;
        public DungeonFloor   floor;
        public RoomInfo       room;
    }

    // 플레이어는 방 한가운데(7,7). 적 좌표는 (dx,dy)로 플레이어 기준.
    private static Setup Start(PlayerCharacter p, params (Enemy e, int dx, int dy)[] enemies)
    {
        URandom.InitState(7);
        var (floor, room) = Arena.MakeRoom(1);
        floor.PlayerX = 7; floor.PlayerY = 7;
        foreach (var (e, dx, dy) in enemies) { e.x = 7 + dx; e.y = 7 + dy; }
        var s = new Setup { dm = new DungeonManager(), bm = new BattleManager(), floor = floor, room = room };
        DungeonManager.Instance = s.dm;
        BattleManager.Instance  = s.bm;
        s.bm.StartBattle(p, enemies.Select(x => x.e).ToList(), floor, room);
        return s;
    }

    public static int Run()
    {
        Console.WriteLine("# 전투 흐름 확인 (시간 기반 전투)");
        Console.WriteLine($"시간 규칙: 이동 {BattleManager.MoveTime}, 카드 {BattleManager.CardTime}, 정비 {BattleManager.RecoveryDuration}, 정비 중 이동 {BattleManager.RecoveryMoveTime}");
        Console.WriteLine();
        int errors0 = UnityEngine.Debug.ErrorCount;

        // 1. 공격 턴 이동
        {
            var p = Player(CharacterClass.Warrior, "strike");
            var s = Start(p, (Dummy(100, EnemyIntent.Attack, 5), 3, 0));
            bool moved = s.bm.TryMovePlayer(-1, 0);
            Check("공격 턴에 한 칸 움직이면 MoveTime만큼 시간이 흐른다", moved && Near(s.bm.BattleTime, BattleManager.MoveTime) && s.bm.Phase == PlayerPhase.Attack,
                  $"시간 {s.bm.BattleTime}");
        }

        // 2. 카드 한 장
        {
            var p = Player(CharacterClass.Warrior, "strike");
            var e = Dummy(100, EnemyIntent.Defend, 1);
            var s = Start(p, (e, 1, 0));
            bool used = s.bm.UseCard(0, e);
            Check("카드 한 장을 쓰면 CardTime만큼 시간이 흐른다", used && Near(s.bm.BattleTime, BattleManager.CardTime), $"시간 {s.bm.BattleTime}");
        }

        // 3·4. 적 속도
        {
            var p = Player(CharacterClass.Warrior, "strike");
            var slow = Dummy(100, EnemyIntent.Defend, 4, 1f);
            var fast = Dummy(100, EnemyIntent.Defend, 4, 1.5f);
            var s = Start(p, (slow, 3, 0), (fast, -3, 0));
            float t1 = 1f / 1.5f;
            int moves = 0;
            while (s.bm.BattleTime + 1e-4f < 1f && moves < 20) { s.bm.TryMovePlayer(0, moves % 2 == 0 ? 1 : -1); moves++; }
            Check("속도 1인 적은 시간 1에 첫 행동을 한다", slow.block == 4 && Near(slow.nextActTime, 2f), $"방어막 {slow.block}, 다음 행동 {slow.nextActTime}");
            Check("속도 1.5인 적은 1/1.5마다 행동한다", Near(fast.nextActTime, t1 * 2f), $"다음 행동 {fast.nextActTime} (기대 {t1 * 2f})");
        }

        // 5~9. 코스트 소진 → 정비 → 공격 턴
        {
            var p = Player(CharacterClass.Warrior, "strike");
            var e = Dummy(300, EnemyIntent.Defend, 1);
            var s = Start(p, (e, 1, 0));
            for (int i = 0; i < p.maxCost; i++) s.bm.UseCard(0, e);
            Check("코스트를 다 쓰면 정비로 넘어간다", s.bm.Phase == PlayerPhase.Recovery, $"단계 {s.bm.Phase}");
            Check("정비에 들어가면 버린 카드가 아니라 남은 덱에서 손패를 채운다",
                  p.deck.GetHand().Count == p.startHandSize && p.deck.DiscardCount == p.maxCost,
                  $"손패 {p.deck.GetHand().Count}, 버린 카드 {p.deck.DiscardCount}");
            Check("정비 중에는 카드를 쓸 수 없다", !s.bm.UseCard(0, e));

            float before = s.bm.BattleTime;
            bool moved = s.bm.TryMovePlayer(0, 1);
            Check("정비 중 이동은 RecoveryMoveTime만큼 걸린다", moved && Near(s.bm.BattleTime - before, BattleManager.RecoveryMoveTime),
                  $"걸린 시간 {s.bm.BattleTime - before}");
            Check("정비가 끝나면 공격 턴으로 돌아오고 코스트가 가득 찬다",
                  s.bm.Phase == PlayerPhase.Attack && p.currentCost == p.maxCost, $"단계 {s.bm.Phase}, 코스트 {p.currentCost}/{p.maxCost}");
        }

        // 10. 턴 종료 → 정비 넘기기
        {
            var p = Player(CharacterClass.Warrior, "strike");
            var s = Start(p, (Dummy(100, EnemyIntent.Defend, 1), 3, 0));
            s.bm.EndPlayerTurn();
            bool inRecovery = s.bm.Phase == PlayerPhase.Recovery;
            float t = s.bm.BattleTime;
            s.bm.EndPlayerTurn();
            Check("턴 종료로 정비에 들어가고, 한 번 더 누르면 남은 정비 시간을 넘긴다",
                  inRecovery && s.bm.Phase == PlayerPhase.Attack && Near(s.bm.BattleTime, t + BattleManager.RecoveryDuration),
                  $"정비 들어감 {inRecovery}, 단계 {s.bm.Phase}, 시간 {s.bm.BattleTime}");
        }

        // 11. 같은 시각이면 적이 먼저 - 공격 턴에 쳐 둔 방어막이 그 공격을 막는다
        {
            var p = Player(CharacterClass.Warrior, "defend");
            var e = Dummy(100, EnemyIntent.Attack, 5);
            var s = Start(p, (e, 1, 0));
            int hp0 = p.currentHp;
            // 정비가 끝나는 시각을 적의 첫 행동(시간 1)에 맞춘다: 카드 n장 + 정비 = 1
            int cards = (int)Math.Round((1f - BattleManager.RecoveryDuration) / BattleManager.CardTime);
            bool aligned = cards >= 1 && cards < p.maxCost && Near(cards * BattleManager.CardTime + BattleManager.RecoveryDuration, 1f);
            if (aligned)
            {
                for (int i = 0; i < cards; i++) s.bm.UseCard(0);
                int block = p.block;
                s.bm.EndPlayerTurn(); // 정비 시작
                s.bm.EndPlayerTurn(); // 정비 넘기기 → 시간 1에 적 공격과 정비 종료가 겹친다
                Check("정비 종료와 적 행동이 같은 시각이면 적이 먼저 행동한다(방어막이 막아 줌)",
                      p.currentHp == hp0 && block >= 5 && p.block == 0 && s.bm.Phase == PlayerPhase.Attack,
                      $"HP {hp0}→{p.currentHp}, 방어막 {block}→{p.block}");
            }
            else Console.WriteLine("건너뜀  정비 종료와 적 행동이 겹치는 경우 (지금 시간 숫자로는 카드 장수로 맞출 수 없음)");
        }

        // 12. 떨어진 적은 다가온다
        {
            var p = Player(CharacterClass.Warrior, "strike");
            var e = Dummy(100, EnemyIntent.Attack, 5);
            var s = Start(p, (e, 3, 0));
            int hp0 = p.currentHp;
            s.bm.EndPlayerTurn(); s.bm.EndPlayerTurn(); // 정비 들어갔다가 넘김
            while (s.bm.BattleTime + 1e-4f < 1f) s.bm.EndPlayerTurn();
            int dist = BattleGridSystem.Chebyshev(e.x, e.y, s.floor.PlayerX, s.floor.PlayerY);
            Check("떨어져 있는 적은 행동 대신 한 칸 다가온다", dist == 2 && p.currentHp == hp0, $"거리 {dist}, HP {hp0}→{p.currentHp}");
        }

        // 13. 승패는 한 번만
        {
            var p = Player(CharacterClass.Warrior, "strike");
            var e = Dummy(3, EnemyIntent.Attack, 5);
            var s = Start(p, (e, 1, 0));
            s.bm.UseCard(0, e);
            s.bm.UseCard(0, e); s.bm.EndPlayerTurn(); s.bm.TryMovePlayer(0, 1);
            Check("이기면 전투가 한 번만 끝난다(이후 입력은 무시)", s.bm.CurrentState == BattleState.Win && s.dm.EndCalls == 1,
                  $"상태 {s.bm.CurrentState}, 끝 처리 {s.dm.EndCalls}번");
        }

        // 14. 카드 막타 → 마나·경험치
        {
            var p = Player(CharacterClass.Mage, "fireball");
            var e = Dummy(3, EnemyIntent.Attack, 5);
            var s = Start(p, (e, 2, 0));
            s.bm.UseCard(0, e);
            Check("카드로 막타를 치면 마법사 마나와 경험치가 오른다", s.bm.CurrentState == BattleState.Win && p.manaStack == p.manaPerKill && p.xp == LevelSystem.XpNormal,
                  $"마나 {p.manaStack}, 경험치 {p.xp}");
        }

        // 15. 독으로 쓰러뜨리면 없음
        {
            var p = Player(CharacterClass.Mage, "mana_shield");
            var e = Dummy(3, EnemyIntent.Defend, 1);
            e.poisonStack = 10;
            var s = Start(p, (e, 1, 0));
            for (int i = 0; i < 10 && s.bm.CurrentState != BattleState.Win; i++) s.bm.EndPlayerTurn();
            Check("독·화상으로 쓰러지면 마나·경험치가 없다", s.bm.CurrentState == BattleState.Win && p.manaStack == 0 && p.xp == 0,
                  $"상태 {s.bm.CurrentState}, 마나 {p.manaStack}, 경험치 {p.xp}");
        }

        // 16. 힘은 전투가 끝나면 0
        {
            var p = Player(CharacterClass.Warrior, "strike");
            p.strengthStack = 5;
            var e = Dummy(3, EnemyIntent.Attack, 5);
            var s = Start(p, (e, 1, 0));
            s.bm.UseCard(0, e);
            Check("전투가 끝나면 힘이 0으로 돌아간다", s.bm.CurrentState == BattleState.Win && p.strengthStack == 0, $"힘 {p.strengthStack}");
        }

        // 17. 첫 공격 턴은 맵에서 들고 온 코스트·방어막
        {
            var p = Player(CharacterClass.Warrior, "strike");
            p.currentCost = 1; p.block = 6;
            var s = Start(p, (Dummy(100, EnemyIntent.Attack, 5), 3, 0));
            Check("첫 공격 턴은 맵에서 들고 온 코스트와 방어막을 그대로 쓴다", p.currentCost == 1 && p.block == 6 && s.bm.Phase == PlayerPhase.Attack,
                  $"코스트 {p.currentCost}, 방어막 {p.block}");
        }

        // 18. 도주
        {
            var p = Player(CharacterClass.Warrior, "strike");
            var e = Dummy(100, EnemyIntent.Attack, 5);
            var (floor, room) = Arena.MakeRoom(1);
            floor.PlayerX = room.x; floor.PlayerY = 7;
            e.x = room.x + 1; e.y = 7;
            var dm = new DungeonManager(); DungeonManager.Instance = dm;
            var bm = new BattleManager(); BattleManager.Instance = bm;
            bm.StartBattle(p, new List<Enemy> { e }, floor, room);
            int hp0 = p.currentHp;
            bm.TryMovePlayer(-1, 0);
            Check("방 밖으로 나가면 도주하고, 붙어 있던 적이 이탈 공격을 한다", bm.CurrentState == BattleState.Fled && p.currentHp == hp0 - 5 && dm.EndCalls == 1,
                  $"상태 {bm.CurrentState}, HP {hp0}→{p.currentHp}");
        }

        int errors = UnityEngine.Debug.ErrorCount - errors0;
        Console.WriteLine();
        Console.WriteLine($"{pass}개 통과, {fail}개 실패{(errors > 0 ? $", 게임 코드 오류 로그 {errors}번" : "")}");
        return fail == 0 && errors == 0 ? 0 : 1;
    }
}
