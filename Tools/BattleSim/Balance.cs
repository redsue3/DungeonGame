// 승률표 - 층마다 일반 1마리·2마리·집단·엘리트·보스를 직업별로 여러 판 싸워 승률과 잃은 체력을 본다.
// 덱은 snapshots.json의 '층 시작 덱'(보스는 '보스 직전 덱'), 체력·코스트는 가득 찬 상태(만전)에서 시작한다.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using URandom = UnityEngine.Random;

public static class Balance
{
    private enum Kind { Normal1, Normal2, Group, Elite, Boss }

    private static string Label(Kind k) => k switch
    {
        Kind.Normal1 => "일반 1마리",
        Kind.Normal2 => "일반 2마리",
        Kind.Group   => $"집단 {FloorGenerator.GroupSize}마리(각 {GroupScale:0.##}배)",
        Kind.Elite   => "엘리트",
        _            => "보스",
    };

    // 집단 무리원 능력치 배율 - 기본은 게임 값(FloorGenerator.GroupMemberScale), --group-scale로 바꿔 볼 수 있다
    public static float GroupScale = FloorGenerator.GroupMemberScale;

    // 층별 적 구성은 FloorGenerator의 것을 그대로 쓴다 (private이라 리플렉션으로 부름)
    private static string Call(string method, int layer) =>
        (string)typeof(FloorGenerator).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { layer });

    private static List<Enemy> Roll(Kind kind, int layer)
    {
        var list = new List<Enemy>();
        switch (kind)
        {
            case Kind.Normal1: list.Add(EnemyFactory.Create(FloorGenerator.RandomNormalId(layer))); break;
            case Kind.Normal2:
                list.Add(EnemyFactory.Create(FloorGenerator.RandomNormalId(layer)));
                list.Add(EnemyFactory.Create(FloorGenerator.RandomNormalId(layer)));
                break;
            case Kind.Group:
                for (int i = 0; i < FloorGenerator.GroupSize; i++)
                    list.Add(EnemyFactory.Create(FloorGenerator.RandomNormalId(layer), GroupScale));
                break;
            case Kind.Elite: list.Add(EnemyFactory.Create(Call("PickElite", layer))); break;
            case Kind.Boss:  list.Add(EnemyFactory.Create(Call("GetBossId", layer))); break;
        }
        return list;
    }

    private static float Pct(List<float> v, float p)
    {
        if (v.Count == 0) return float.NaN;
        var s = v.OrderBy(x => x).ToList();
        return s[Math.Min(s.Count - 1, (int)(p * s.Count))];
    }

    public static int Run(Options opt)
    {
        string snapPath = SnapshotFile.Find(opt.SnapshotPath);
        SnapshotFile snaps = snapPath != null ? SnapshotFile.Load(snapPath) : null;
        var classes = opt.Class.HasValue ? new[] { opt.Class.Value } : (CharacterClass[])Enum.GetValues(typeof(CharacterClass));
        var layers = opt.Layer.HasValue ? new[] { opt.Layer.Value } : snaps != null ? new[] { 1, 2, 3 } : new[] { 1 };

        Console.WriteLine("# 전투 시뮬레이션 (시간 기반 전투, 실제 게임 코드)");
        Console.WriteLine();
        Console.WriteLine($"시간 규칙: 이동 {BattleManager.MoveTime}, 카드 {BattleManager.CardTime}, 정비 {BattleManager.RecoveryDuration}, 정비 중 이동 {BattleManager.RecoveryMoveTime}" +
                          $" · 직업·상대마다 {opt.Battles}판 · 시드 {opt.Seed}");
        Console.WriteLine(snaps != null
            ? $"덱: {snapPath} ({snaps.GeneratedAt}에 BalanceSim이 만든 표본). 체력·코스트 가득 찬 상태에서 시작."
            : "덱: snapshots.json이 없어 1층 시작 덱만 봅니다. `node Tools/BalanceSim/balance-sim.js`를 먼저 돌리면 2·3층도 볼 수 있습니다.");
        Console.WriteLine("플레이어 규칙: 쓸 만한 카드 중 코스트 대비 가치가 높은 것부터, 사거리 밖이면 다가가고, 쓸 카드가 없으면 턴 종료, 정비는 넘긴다." +
                          (Arena.Kite ? " 원거리 카드가 있으면 붙은 적에게서 먼저 물러난다(--kite)." : ""));
        Console.WriteLine();
        Console.WriteLine("| 층 | 덱 | 상대 | 승률 | 이겼을 때 잃은 체력(중앙값) | 전투 시간(중앙값) | " + string.Join(" | ", classes.Select(c => PlayerDatabase.Get(c).displayName)) + " |");
        Console.WriteLine("|---|---|---|---|---|---|" + string.Concat(classes.Select(_ => "---|")));

        int timeouts = 0, errors0 = UnityEngine.Debug.ErrorCount;
        foreach (int L in layers)
        {
            foreach (Kind kind in Enum.GetValues(typeof(Kind)))
            {
                int check = kind == Kind.Boss ? SnapshotFile.BossCheck(L) : SnapshotFile.StartCheck(L);
                string deckLabel = snaps != null ? (kind == Kind.Boss ? "보스 직전 덱" : "층 시작 덱") : "시작 덱";
                var lost = new List<float>(); var times = new List<float>();
                int wins = 0, n = 0;
                var byClass = new List<string>();

                foreach (CharacterClass cls in classes)
                {
                    URandom.InitState(opt.Seed + L * 1000 + (int)kind * 100 + (int)cls);
                    List<PlayerSnapshot> pool = snaps != null && snaps.ByClass.TryGetValue(cls, out var perCheck) && check < perCheck.Count ? perCheck[check] : null;
                    int cw = 0;
                    for (int i = 0; i < opt.Battles; i++)
                    {
                        PlayerSnapshot snap = pool != null && pool.Count > 0 ? pool[i % pool.Count] : null;
                        PlayerCharacter player = SnapshotFile.BuildPlayer(cls, snap);
                        FightResult r = Arena.Fight(player, Roll(kind, L), L);
                        n++;
                        if (r.TimedOut) timeouts++;
                        if (r.Win) { cw++; wins++; lost.Add(r.HpLost); times.Add(r.Time); }
                    }
                    byClass.Add($"{100.0 * cw / opt.Battles:0}%");
                }

                Console.WriteLine($"| {L} | {deckLabel} | {Label(kind)} | {100.0 * wins / n:0}% | {Pct(lost, 0.5f) * 100:0}% | {Pct(times, 0.5f):0.0} | {string.Join(" | ", byClass)} |");
            }
        }

        Console.WriteLine();
        if (timeouts > 0) Console.WriteLine($"주의: {timeouts}판이 {Arena.MaxActions}번 행동 안에 끝나지 않았습니다 (패배로 셈).");
        int errors = UnityEngine.Debug.ErrorCount - errors0;
        if (errors > 0) Console.WriteLine($"주의: 게임 코드가 오류 로그를 {errors}번 냈습니다. 마지막: {UnityEngine.Debug.LastError}");
        Console.WriteLine("참고: 전투 시간 1 = 속도 1인 적이 한 번 행동하는 시간. 도주는 하지 않는다.");
        return 0;
    }
}
