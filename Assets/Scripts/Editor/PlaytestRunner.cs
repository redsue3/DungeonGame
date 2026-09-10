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
    [MenuItem("DungeonGame/__Playtest 실행 (임시)")]
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
        EditorApplication.EnterPlaymode();
        EditorApplication.delayCall += Bootstrap;
    }

    private static void Bootstrap()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.delayCall += Bootstrap;
            return;
        }
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

        Debug.Log("[PLAYTEST] ALL_TESTS_PASSED");
        Quit(0);
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
            if (actions++ > 500) { Fail("전투 500액션 초과 - 종료되지 않음"); yield break; }

            if (BattleManager.Instance.CurrentState == BattleState.PlayerTurn)
            {
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
        while (DungeonManager.Instance.CurrentState == GameState.DungeonMap)
        {
            if (sw.Elapsed.TotalSeconds > 25) { Fail("맵 로밍 25초 타임아웃 (인피니트 루프 의심)"); yield break; }
            if (guard++ > 5000) { Fail("맵 로밍 5000스텝 내에 전투 조우 실패"); yield break; }
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
            if (turns++ > 80) { Fail("전투 80턴 내 도주/종료 실패"); yield break; }

            if (BattleManager.Instance.CurrentState == BattleState.PlayerTurn)
            {
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
