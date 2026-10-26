using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Linq;

// 임시 자동 플레이테스트 - 4단계(전투 그리드 통합) 검증용. 확인 후 삭제할 것.
// UI 클릭 대신 DungeonManager/BattleManager API를 직접 호출해서 게임 로직을 그대로 몰아붙인다.
// 각 단계에 실시간 워치독(Stopwatch)을 걸어서 무한루프(특히 도주 시 BattleLoop) 발생 시 타임아웃으로 잡아낸다.
public static class PlaytestRunner
{
    // Play 모드 진입 시 도메인 리로드로 static 구독(delayCall 등)이 전부 지워진다 -
    // 그래서 "실행 요청됨"은 리로드를 견디는 SessionState에 남기고, 리로드 후 다시 구독한다.
    private const string PendingKey = "PlaytestRunner.Pending";

    [MenuItem("DungeonGame/__Playtest 실행 (임시)")]
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
        SessionState.SetBool(PendingKey, true);
        EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    private static void Register() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

    private static void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PendingKey, false)) return;
        SessionState.EraseBool(PendingKey);
        new GameObject("__PlaytestDriver").AddComponent<PlaytestDriver>();
    }
}

public class PlaytestDriver : MonoBehaviour
{
    private static bool failed;
    private static string failReason = "";

    void Awake() => Application.logMessageReceived += OnLog;

    void OnLog(string condition, string stackTrace, LogType type)
    {
        // 새 Library에서 에디터 검색 인덱싱(UnityEditor.Search)이 내부 예외를 내는 경우가 있다 - 게임 로직과 무관하므로 무시
        if (stackTrace != null && stackTrace.Contains("UnityEditor.Search")) return;
        if (type == LogType.Exception || type == LogType.Error)
        {
            failed = true;
            failReason = condition;
        }
    }

    void Start() => StartCoroutine(MainSequence());

    private IEnumerator MainSequence()
    {
        yield return Step("맵 로밍 → 전투 조우 + 일반 전투 진행", Step_RoamAndFightToEnd);
        if (Bail()) yield break;

        yield return Step("재시작 → 전투 재진입", Step_RoamUntilBattle);
        if (Bail()) yield break;

        yield return Step("도주 시나리오 (무한루프 워치독)", Step_FleeScenario);
        if (Bail()) yield break;

        yield return Step("도주 후 추격 재조우", Step_CorridorCatchScenario);
        if (Bail()) yield break;

        yield return Step("보스 처치 → 계단으로 다음 계층", Step_BossStairsScenario);
        if (Bail()) yield break;

        yield return Step("전투 중 이동 취소", Step_UndoMoveScenario);
        if (Bail()) yield break;

        Debug.Log("[PLAYTEST] ALL_TESTS_PASSED");
        Quit(0);
    }

    // 보스 방 적에게 직접 접촉해 전투 → 적 HP를 0으로 만들어 승리 → 계층이 그대로인지, 계단이 열렸는지 →
    // 계단 칸을 밟으면 2계층으로 내려가는지 확인
    private IEnumerator Step_BossStairsScenario()
    {
        var dm = DungeonManager.Instance;
        dm.StartRun(CharacterClass.Warrior);
        yield return null;

        var floor = dm.CurrentFloor;
        var bossRoom = floor.Rooms.FirstOrDefault(r => r.roomType == TileType.Boss);
        if (bossRoom == null) { Fail("보스 방이 없음"); yield break; }
        var boss = floor.Enemies.FirstOrDefault(e => e.roomId == bossRoom.id && !e.isDead);
        if (boss == null) { Fail("보스 방에 적이 없음"); yield break; }

        // 보스 옆 빈 칸으로 순간이동 후 보스 칸으로 걸어 들어가 조우
        var from = Dirs.Select(d => (x: boss.x - d.dx, y: boss.y - d.dy, d))
                       .FirstOrDefault(p => floor.IsWalkable(p.x, p.y) && floor.EnemyAt(p.x, p.y) == null);
        if (from.d.dx == 0 && from.d.dy == 0) { Fail("보스 옆 빈 칸을 못 찾음"); yield break; }
        floor.PlayerX = from.x; floor.PlayerY = from.y;
        dm.TryMove(from.d.dx, from.d.dy);
        yield return null;
        if (dm.CurrentState != GameState.Battle) { Fail($"보스 조우 실패 (state={dm.CurrentState})"); yield break; }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (BattleManager.Instance.CurrentState != BattleState.PlayerTurn)
        {
            if (sw.Elapsed.TotalSeconds > 10) { Fail("보스전 플레이어 턴 대기 타임아웃"); yield break; }
            yield return null;
        }
        foreach (var e in BattleManager.Instance.GetEnemies()) e.currentHp = 0;
        BattleManager.Instance.EndPlayerTurn();

        while (dm.CurrentState == GameState.Battle)
        {
            if (sw.Elapsed.TotalSeconds > 10) { Fail("보스전 종료 대기 타임아웃"); yield break; }
            yield return null;
        }
        if (dm.CurrentState != GameState.Reward) { Fail($"보스 처치 후 보상 화면이 아님 (state={dm.CurrentState})"); yield break; }
        if (dm.CurrentLayer != 1) { Fail($"보스 처치 직후 계층이 바뀜 ({dm.CurrentLayer}) - 계단을 밟기 전엔 그대로여야 함"); yield break; }
        if (floor.Stairs == null) { Fail("보스 처치 후 계단이 열리지 않음"); yield break; }
        Debug.Log("[PLAYTEST] 보스 처치 후 1계층 유지 + 계단 열림 확인");

        dm.SkipCardReward();
        yield return null;
        if (dm.CurrentState != GameState.DungeonMap) { Fail($"보상 후 맵으로 안 돌아옴 (state={dm.CurrentState})"); yield break; }

        int sx = bossRoom.CenterX, sy = bossRoom.CenterY;
        var near = Dirs.Select(d => (x: sx - d.dx, y: sy - d.dy, d)).FirstOrDefault(p => floor.IsWalkable(p.x, p.y));
        if (near.d.dx == 0 && near.d.dy == 0) { Fail("계단 옆 빈 칸을 못 찾음"); yield break; }
        floor.PlayerX = near.x; floor.PlayerY = near.y;
        dm.TryMove(near.d.dx, near.d.dy);
        yield return null;

        // 밟기만 해서는 내려가지 않고, 확인(Enter/클릭 = DescendStairs)해야 내려간다
        if (!dm.IsOnStairs) { Fail($"계단 칸으로 이동하지 못함 (pos={floor.PlayerX},{floor.PlayerY}, state={dm.CurrentState})"); yield break; }
        if (dm.CurrentLayer != 1) { Fail("계단을 밟기만 했는데 확인 없이 내려감"); yield break; }
        dm.DescendStairs();
        yield return null;

        if (dm.CurrentLayer != 2) { Fail($"계단에서 확인했는데 2계층으로 안 내려감 (layer={dm.CurrentLayer}, state={dm.CurrentState})"); yield break; }
        if (dm.CurrentFloor.Stairs != null) { Fail("내려간 새 층에 계단이 이미 있음"); yield break; }
        if (dm.CurrentFloor == floor) { Fail("계층 번호만 바뀌고 층이 새로 생성되지 않음"); yield break; }
        Debug.Log($"[PLAYTEST] 계단으로 2계층 진입 확인 (state={dm.CurrentState})");
    }

    // 전투에서 한 칸 이동 → 카드 내기 전 이동 취소 → 원위치 + 이동권 복구 확인. 카드를 낸 뒤에는 취소 불가 확인.
    private IEnumerator Step_UndoMoveScenario()
    {
        DungeonManager.Instance.StartRun(CharacterClass.Warrior);
        yield return null;
        yield return WaitForBattleEncounter(777);
        if (failed) yield break;

        var bm = BattleManager.Instance;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (bm.CurrentState != BattleState.PlayerTurn)
        {
            if (sw.Elapsed.TotalSeconds > 10) { Fail("플레이어 턴 대기 타임아웃"); yield break; }
            yield return null;
        }

        var floor = DungeonManager.Instance.CurrentFloor;
        int ox = floor.PlayerX, oy = floor.PlayerY;
        bool moved = false;
        foreach (var d in Dirs)
        {
            if (!bm.Room.Contains(ox + d.dx, oy + d.dy)) continue; // 도주 방향은 제외
            if (bm.TryMovePlayer(d.dx, d.dy)) { moved = true; break; }
        }
        if (!moved) { Debug.Log("[PLAYTEST] 참고: 방 안에서 움직일 칸이 없어 이동 취소 검사 스킵"); yield break; }

        if (!bm.CanUndoMove) { Fail("이동 직후 이동 취소가 불가능함"); yield break; }
        if (!bm.UndoMove()) { Fail("UndoMove 실패"); yield break; }
        if (floor.PlayerX != ox || floor.PlayerY != oy) { Fail("이동 취소 후 원위치가 아님"); yield break; }
        if (bm.HasMovedThisTurn) { Fail("이동 취소 후 이동권이 복구되지 않음"); yield break; }
        Debug.Log("[PLAYTEST] 이동 취소 → 원위치 + 이동권 복구 확인");

        // 다시 이동한 뒤 카드를 내면 취소 불가
        foreach (var d in Dirs)
        {
            if (!bm.Room.Contains(ox + d.dx, oy + d.dy)) continue;
            if (bm.TryMovePlayer(d.dx, d.dy)) break;
        }
        var hand = DungeonManager.Instance.Player.deck.GetHand();
        bool used = false;
        for (int i = 0; i < hand.Count && !used; i++)
        {
            // 방어처럼 사거리가 필요 없는 카드로 확인
            bool offense = hand[i].damage > 0 || hand[i].poisonApply > 0 || hand[i].burnApply > 0;
            if (!offense && hand[i].cost <= bm.CurrentCost) used = bm.UseCard(i);
        }
        if (!used) { Debug.Log("[PLAYTEST] 참고: 낼 수 있는 비공격 카드가 없어 '카드 후 취소 불가' 검사 스킵"); yield break; }
        if (bm.CurrentState == BattleState.PlayerTurn && bm.CanUndoMove) { Fail("카드를 낸 뒤에도 이동 취소가 가능함"); yield break; }
        Debug.Log("[PLAYTEST] 카드 사용 후 이동 취소 불가 확인");
    }

    private IEnumerator Step(string name, System.Func<IEnumerator> body)
    {
        Debug.Log($"[PLAYTEST] STEP 시작: {name}");
        yield return body();
        Debug.Log($"[PLAYTEST] STEP 종료: {name}");
    }

    private bool Bail()
    {
        if (!failed) return false;
        Debug.LogError("[PLAYTEST] FAILED: " + failReason);
        Quit(1);
        return true;
    }

    private void Fail(string reason)
    {
        failed = true;
        failReason = reason;
    }

    private void Quit(int code) => EditorApplication.Exit(code);

    private static readonly (int dx, int dy)[] Dirs = { (0, 1), (0, -1), (1, 0), (-1, 0) };

    private IEnumerator Step_RoamAndFightToEnd()
    {
        DungeonManager.Instance.StartRun(CharacterClass.Warrior);
        yield return null;

        yield return WaitForBattleEncounter(12345);
        if (failed) yield break;

        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        int actions = 0;
        while (DungeonManager.Instance.CurrentState == GameState.Battle)
        {
            if (sw.Elapsed.TotalSeconds > 40) { Fail("전투 진행 40초 타임아웃 (교착/무한루프 의심)"); yield break; }

            // 카운터는 플레이어 행동에만 센다 - 배치모드는 프레임 제한이 없어 적 턴 대기(WaitForSeconds) 중 프레임 수가 폭증한다
            if (BattleManager.Instance.CurrentState == BattleState.PlayerTurn)
            {
                if (actions++ > 500) { Fail("전투 500액션 초과 - 종료되지 않음"); yield break; }
                var hand = DungeonManager.Instance.Player.deck.GetHand();
                bool used = false;
                for (int i = 0; i < hand.Count; i++)
                {
                    if (hand[i].cost <= BattleManager.Instance.CurrentCost)
                    {
                        used = BattleManager.Instance.UseCard(i);
                        if (used) break;
                    }
                }
                if (!used) BattleManager.Instance.EndPlayerTurn();
            }
            yield return null;
        }
        Debug.Log($"[PLAYTEST] 전투 종료 state={DungeonManager.Instance.CurrentState} actions={actions}");
    }

    private IEnumerator Step_RoamUntilBattle()
    {
        DungeonManager.Instance.StartRun(CharacterClass.Warrior);
        yield return null;
        yield return WaitForBattleEncounter(54321);
    }

    private IEnumerator WaitForBattleEncounter(int seed)
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        var rng = new System.Random(seed);
        int guard = 0;
        while (DungeonManager.Instance.CurrentState != GameState.Battle)
        {
            if (sw.Elapsed.TotalSeconds > 25) { Fail("맵 로밍 25초 타임아웃 (인피니트 루프 의심)"); yield break; }
            if (guard++ > 5000) { Fail("맵 로밍 5000스텝 내에 전투 조우 실패"); yield break; }

            // 전투가 아닌 방(성소/휴식/상점/보상)에 들어가면 바로 나와서 계속 돌아다닌다
            var dm = DungeonManager.Instance;
            switch (dm.CurrentState)
            {
                case GameState.Shrine: dm.ChooseShrineCard(0); yield return null; continue;
                case GameState.Rest:   dm.LeaveRestSite();     yield return null; continue;
                case GameState.Shop:   dm.LeaveShop();         yield return null; continue;
                case GameState.Reward: dm.SkipCardReward();    yield return null; continue;
                case GameState.GameOver:
                case GameState.Victory:
                    Fail($"맵 로밍 중 예상치 못한 상태: {dm.CurrentState}"); yield break;
            }

            var d = Dirs[rng.Next(4)];
            DungeonManager.Instance.TryMove(d.dx, d.dy);
            yield return null;
        }
        Debug.Log($"[PLAYTEST] 전투 조우 성공 (steps={guard})");
    }

    private IEnumerator Step_FleeScenario()
    {
        if (DungeonManager.Instance.CurrentState != GameState.Battle)
        {
            Fail("도주 테스트 시작 시 전투 상태가 아님");
            yield break;
        }

        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        int turns = 0;
        while (DungeonManager.Instance.CurrentState == GameState.Battle)
        {
            if (sw.Elapsed.TotalSeconds > 40) { Fail("도주 시나리오 40초 타임아웃 (BattleLoop 무한루프 의심)"); yield break; }

            if (BattleManager.Instance.CurrentState == BattleState.PlayerTurn)
            {
                if (turns++ > 80) { Fail("전투 80턴 내 도주/종료 실패"); yield break; }
                var floor = DungeonManager.Instance.CurrentFloor;
                var room = BattleManager.Instance.Room;
                var ordered = Dirs.OrderByDescending(d =>
                    BattleGridSystem.Chebyshev(floor.PlayerX + d.dx, floor.PlayerY + d.dy, room.CenterX, room.CenterY));

                bool moved = false;
                foreach (var d in ordered)
                {
                    if (BattleManager.Instance.TryMovePlayer(d.dx, d.dy)) { moved = true; break; }
                }
                if (!moved) BattleManager.Instance.EndPlayerTurn();
            }
            yield return null;
        }

        var finalState = DungeonManager.Instance.CurrentState;
        Debug.Log($"[PLAYTEST] 도주 테스트 종료 state={finalState} turns={turns}");
        if (finalState != GameState.DungeonMap)
            Debug.Log("[PLAYTEST] 참고: 도주 대신 승리/패배로 전투가 끝났음 (버그 아님, 방이 좁거나 적이 계속 인접했을 수 있음)");
    }

    private IEnumerator Step_CorridorCatchScenario()
    {
        if (DungeonManager.Instance.CurrentState != GameState.DungeonMap)
        {
            Debug.Log("[PLAYTEST] 이전 단계가 도주로 끝나지 않아 스킵");
            yield break;
        }

        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        int waits = 0;
        while (DungeonManager.Instance.CurrentState == GameState.DungeonMap)
        {
            if (sw.Elapsed.TotalSeconds > 20) break;   // 참고용 - 못 잡혀도 실패 아님
            if (waits++ > 300) break;
            DungeonManager.Instance.TryMove(0, 0); // 제자리 대기 - 적 AI만 진행시켜 추격을 유도
            yield return null;
        }

        if (DungeonManager.Instance.CurrentState == GameState.Battle)
            Debug.Log($"[PLAYTEST] 추격 적 재조우로 전투 시작 확인 (waits={waits})");
        else
            Debug.Log($"[PLAYTEST] 제한 시간/횟수 내 추격 재조우 미발생 (waits={waits}, 참고용 - 실패 아님)");
    }
}
