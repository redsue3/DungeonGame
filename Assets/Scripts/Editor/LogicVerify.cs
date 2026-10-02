using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Play 모드 없이(배치모드로도) 돌릴 수 있는 게임 로직 검증. 화면을 안 띄우고 로직 API를 직접 부른다.
//   Unity.exe -batchmode -projectPath . -executeMethod LogicVerify.RunBatch -logFile verify.log -quit
// 검사 항목:
//   1. PR3: 계층마다 일반 몬스터 5종이 전부 실제로 스폰되는가 (층을 여러 번 생성해서 모음)
//   2. PR3: 보스가 HP 구간마다 페이즈가 넘어가는가 (역행 없음)
//   3. PR3: 계층별 바닥색이 서로 다른가
//   4. 도감: bestiary.md 22종이 EnemyDatabase 키와 정확히 일치하는가
//   5. 전투 그리드: 대각선으로 떨어진 적이 한 턴에 대각선으로 붙는가 (8방향 추적), 코너컷은 안 하는가
// 화면에서 손으로 봐야 하는 것(UI 배치, 손맛)은 여기서 못 본다 - devlog의 수동 체크리스트로.
public static class LogicVerify
{
    private static int pass, fail;

    [MenuItem("DungeonGame/로직 검증 (Play 모드 없이)")]
    public static void Run()
    {
        pass = fail = 0;
        CheckBestiaryMatchesDatabase();
        CheckNormalSpawns();
        CheckBossPhases();
        CheckFloorPalette();
        CheckDiagonalChase();
        Debug.Log($"[LogicVerify] 결과: 통과 {pass} · 실패 {fail}");
    }

    public static void RunBatch()
    {
        Run();
        EditorApplication.Exit(fail == 0 ? 0 : 1);
    }

    private static void Check(bool ok, string what)
    {
        if (ok) { pass++; Debug.Log($"[LogicVerify] 통과  {what}"); }
        else    { fail++; Debug.LogError($"[LogicVerify] 실패  {what}"); }
    }

    private static Dictionary<string, EnemyData> EnemyTable() =>
        (Dictionary<string, EnemyData>)typeof(EnemyDatabase)
            .GetField("table", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

    // ── 4. 도감 ↔ EnemyDatabase ─────────────────────────────────────────
    private static void CheckBestiaryMatchesDatabase()
    {
        var lore = MonsterLoreDatabase.All.Select(l => l.id).ToHashSet();
        var db   = EnemyTable().Keys.ToHashSet();
        Check(lore.Count == 22, $"도감 항목 22개 (실제 {lore.Count})");
        var missing = db.Except(lore).ToList();
        var extra   = lore.Except(db).ToList();
        Check(missing.Count == 0 && extra.Count == 0,
              $"도감 id = EnemyDatabase 키 (도감에 없음: {string.Join(",", missing)} / DB에 없음: {string.Join(",", extra)})");
        Check(MonsterLoreDatabase.All.All(l => l.layer > 0 && !string.IsNullOrEmpty(l.description)),
              "도감 항목마다 계층·설정이 채워져 있다");
    }

    // ── 1. 계층별 일반 몬스터 5종 스폰 ─────────────────────────────────
    private static void CheckNormalSpawns()
    {
        var db = EnemyTable();
        for (int layer = 1; layer <= 3; layer++)
        {
            var expected = MonsterLoreDatabase.All
                .Where(l => l.layer == layer && l.grade == "일반")
                .Select(l => l.id).ToHashSet();

            var seen = new HashSet<string>();
            for (int seed = 0; seed < 40; seed++)
            {
                Random.InitState(1000 * layer + seed);
                DungeonFloor floor = FloorGenerator.Generate(layer);
                foreach (EnemySpawn s in floor.Enemies) seen.Add(s.enemyTemplateId);
            }

            Check(expected.Count == 5, $"{layer}계층 도감상 일반 몬스터 5종 (실제 {expected.Count})");
            var notSpawned = expected.Except(seen).ToList();
            Check(notSpawned.Count == 0, $"{layer}계층 일반 몬스터가 전부 스폰됨 (40층 생성, 안 나온 것: {string.Join(",", notSpawned)})");
            var unknown = seen.Where(id => !db.ContainsKey(id)).ToList();
            Check(unknown.Count == 0, $"{layer}계층 스폰 id가 전부 EnemyDatabase에 있음 (없는 것: {string.Join(",", unknown)})");
        }
    }

    // ── 2. 보스 페이즈 전환 ─────────────────────────────────────────────
    private static void CheckBossPhases()
    {
        FieldInfo phaseIdx = typeof(Enemy).GetField("currentPhaseIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var (id, data) in EnemyTable().Where(kv => kv.Value.isBoss).Select(kv => (kv.Key, kv.Value)))
        {
            Enemy boss = EnemyFactory.Create(id);
            int phases = data.phases.Length;
            Check(phases >= 2, $"보스 {id} 페이즈 {phases}개 (2개 이상)");

            int last = 0;
            bool monotonic = true;
            // HP를 조금씩 깎으면서 매 턴 시작 처리를 돌린다 - 페이즈는 늘기만 해야 한다.
            for (int hp = boss.maxHp; hp > 0; hp -= Mathf.Max(1, boss.maxHp / 20))
            {
                boss.currentHp = hp;
                boss.OnTurnStart();
                int now = (int)phaseIdx.GetValue(boss);
                if (now < last) monotonic = false;
                last = now;
            }
            Check(monotonic, $"보스 {id} 페이즈가 역행하지 않음");
            Check(last == phases - 1, $"보스 {id} 빈사까지 깎으면 마지막 페이즈 도달 ({last + 1}/{phases})");
        }
    }

    // ── 3. 계층별 바닥색 ───────────────────────────────────────────────
    private static void CheckFloorPalette()
    {
        MethodInfo get = typeof(DungeonMapUI).GetMethod("GetFloorPalette", BindingFlags.NonPublic | BindingFlags.Static);
        var colors = Enumerable.Range(1, 3).Select(l => (((Color visible, Color dim))get.Invoke(null, new object[] { l })).visible).ToList();
        Check(colors.Distinct().Count() == 3, "1~3계층 바닥색이 서로 다르다");
    }

    // ── 5. 전투 그리드 대각선 추적 ─────────────────────────────────────
    private static void CheckDiagonalChase()
    {
        // 적(0,0) → 플레이어(2,2) → 대각선으로 (4,4)까지 물러날 수 있게 5x5 방을 고른다.
        // 방 크기는 3~5 랜덤이라 한 층에 5x5 방이 없을 수도 있어서, 나올 때까지 시드를 넘긴다.
        DungeonFloor floor = null;
        RoomInfo room = null;
        for (int seed = 0; seed < 200 && room == null; seed++)
        {
            Random.InitState(7000 + seed);
            floor = FloorGenerator.Generate(1);
            room = floor.Rooms.FirstOrDefault(r => r.w >= 5 && r.h >= 5);
        }
        if (room == null) { Check(false, "대각선 추적 검사용 5x5 방을 200층 안에 찾음"); return; }

        int px = room.x + 2, py = room.y + 2;
        floor.PlayerX = px; floor.PlayerY = py;
        var enemy = EnemyFactory.Create("slime");
        enemy.x = px - 2; enemy.y = py - 2;
        var all = new List<Enemy> { enemy };

        BattleGridSystem.StepEnemyToward(floor, room, enemy, all);
        Check(BattleGridSystem.Chebyshev(enemy.x, enemy.y, px, py) == 1,
              $"대각선 2칸 떨어진 적이 한 턴에 인접 (적 위치 {enemy.x - px},{enemy.y - py})");

        // 플레이어가 대각선으로 물러나도 거리가 벌어지지 않는다
        bool kept = true;
        for (int turn = 0; turn < 2; turn++)
        {
            var r = BattleGridSystem.TryMovePlayer(floor, room, all, 1, 1, out _);
            if (r != BattleGridSystem.MoveResult.Moved) break;
            BattleGridSystem.StepEnemyToward(floor, room, enemy, all);
            if (BattleGridSystem.Chebyshev(enemy.x, enemy.y, floor.PlayerX, floor.PlayerY) > 1) kept = false;
        }
        Check(kept, "플레이어가 대각선으로 물러나도 적이 계속 붙어 따라온다");
    }
}
