using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerTurn = 플레이어 입력을 받는 중(공격 턴이든 정비 중이든), EnemyTurn = 시간이 흐르며 적이 움직이는 중(입력 막음)
public enum BattleState { PlayerTurn, EnemyTurn, Win, Lose, Fled }

// 플레이어 쪽 단계 (2026-11-11 시간 기반 전투)
//   Attack  : 공격 턴 - 이동 속도 정상, 카드 사용 가능, 코스트 최대치
//   Recovery: 정비 중 - 이동이 느려지고(시간 2배), 카드 사용 불가. 들어갈 때 남은 덱에서 손패를 채운다
public enum PlayerPhase { Attack, Recovery }

// 픽셀 던전식 시간 기반 전투 + 덱.
// 전투는 '시간'으로 흐른다. 플레이어가 이동하거나 카드를 쓰면 그만큼 시간이 지나고, 적은 자기 속도(Enemy.speed)에 맞춰
// 그 사이사이에 행동한다. 플레이어가 아무것도 안 하면 시간도 멈춰 있다(턴제 로그라이크와 같은 감각).
// 플레이어는 공격 턴과 정비 시간을 번갈아 갖는다:
//   공격 턴 → (턴 종료 버튼 또는 코스트 소진) → 정비: 이동 느려짐 + 카드 사용 불가 + 남은 덱에서 손패 채우기
//   정비가 RecoveryDuration만큼 지나면 → 다시 공격 턴: 이동 속도 복귀 + 공격 가능 + 코스트 복구
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    // ── 시간 규칙 (플레이 해 보고 조정할 숫자들) ──────────────────────
    // 속도 1인 적은 시간 1마다 한 번 행동한다. 그 사이 공격 턴엔 2칸 이동 또는 카드 4장, 정비 중엔 1칸 이동.
    // 2026-11-11 후보 6개를 실제 전투 코드로 돌려(1층 시작 덱, 직업 4종) 예전 턴 방식에 맞춘 난이도와 가장 가까운 값을 골랐다:
    //   일반 1마리 승률 100%·체력 17% 손실, 2마리 73%, 엘리트 86%, 보스 76% (예전: 100%·18%, 65%, 76%, 72%)
    // 카드 1장 = 시간 1로 두면 카드마다 적이 한 번씩 때려서 일반몹 1마리에게도 승률이 34%로 떨어졌다.
    public static float MoveTime         = 0.5f;  // 공격 턴에 한 칸 이동
    public static float RecoveryMoveTime = 1f;    // 정비 중 한 칸 이동 (이속 감소 - 공격 턴의 2배 느림)
    public static float CardTime         = 0.25f; // 카드 한 장 사용
    public static float RecoveryDuration = 0.5f;  // 정비 시간 길이 ('잠시' 덱 사용 불가)

    private PlayerCharacter  player;
    private List<Enemy>      enemies = new List<Enemy>();
    private BattleState      state;
    private bool             battleOver;
    private HashSet<string>  usedOnceRelics = new HashSet<string>(); // 전투당 1회만 발동하는 유물(불사조의 재 등)
    private HashSet<Enemy>   rewardedKills  = new HashSet<Enemy>();  // OnKill 유물을 이미 발동시킨 적 (중복 발동 방지)

    // 4단계(전투 그리드 통합) - 전투가 벌어지는 방과 그 방이 속한 층. 이동/도주/사거리 판정에 쓴다.
    private DungeonFloor     floor;
    private RoomInfo         room;

    // 시간
    private float            battleTime;
    private float            recoveryEndsAt;
    private bool             busy;              // 시간이 흐르는 중(적 행동 연출) - 이 동안엔 입력을 받지 않는다

    public int         CurrentCost  => player != null ? player.currentCost : 0;
    public BattleState CurrentState => state;
    public PlayerPhase Phase        { get; private set; }
    public bool        CanAct       => state == BattleState.PlayerTurn && !busy && !battleOver;
    public float       BattleTime   => battleTime;
    public float       RecoveryLeft => Phase == PlayerPhase.Recovery ? Mathf.Max(0f, recoveryEndsAt - battleTime) : 0f;
    public float       CurrentMoveTime => Phase == PlayerPhase.Attack ? MoveTime : RecoveryMoveTime;
    public RoomInfo    Room         => room;
    public List<Enemy> GetEnemies() => enemies;
    public float       TimeUntilAct(Enemy e) => Mathf.Max(0f, e.nextActTime - battleTime);

    void Awake() => Instance = this;

    // 집단 조우. dungeonFloor/battleRoom - 전투 중 이동/사거리/도주 판정에 쓰는 그리드 컨텍스트.
    public void StartBattle(PlayerCharacter playerCharacter, List<Enemy> enemyList, DungeonFloor dungeonFloor, RoomInfo battleRoom)
    {
        StopAllCoroutines();
        player  = playerCharacter;
        enemies = new List<Enemy>(enemyList);
        floor   = dungeonFloor;
        room    = battleRoom;
        usedOnceRelics.Clear();
        rewardedKills.Clear();
        battleOver = false;
        busy       = false;
        battleTime = 0f;

        // 적은 플레이어가 먼저 한 번 움직인 뒤(자기 속도만큼 시간이 지난 뒤)부터 행동한다
        foreach (Enemy e in enemies) e.nextActTime = 1f / Mathf.Max(0.1f, e.speed);

        player.deck.ResetForBattle();
        HungerSystem.OnBattleStart(player);
        ApplyRelicTrigger(RelicTrigger.OnBattleStart);

        // 첫 공격 턴: 맵에서 들고 온 코스트/방어막을 그대로 쓴다 (리필·방어막 초기화 없음), 손패만 뽑는다
        Phase = PlayerPhase.Attack;
        player.OnTurnStart(resetBlock: false);
        CheckHpBelowTriggers();
        player.deck.DrawCards(player.startHandSize);
        ApplyRelicTrigger(RelicTrigger.OnTurnStart);
        state = BattleState.PlayerTurn;

        LogStatus("전투 시작");
        if (!player.IsAlive) { Finish(BattleState.Lose); return; }
        NotifyUI();
    }

    // 개별 조우 편의 오버로드
    public void StartBattle(PlayerCharacter playerCharacter, Enemy enemy, DungeonFloor dungeonFloor, RoomInfo battleRoom)
        => StartBattle(playerCharacter, new List<Enemy> { enemy }, dungeonFloor, battleRoom);

    private void NotifyUI() => UIManager.Instance?.RefreshBattle();

    // ─────────────────────────────────────────
    // 플레이어 행동 - 전부 시간을 쓴다
    // ─────────────────────────────────────────

    // 카드 사용 - 공격 턴에만. (target: 단일 공격 카드가 때릴 적, null이면 첫 번째 생존 적)
    public bool UseCard(int handIndex, Enemy target = null)
    {
        if (!CanAct || Phase != PlayerPhase.Attack) return false;

        List<Card> hand = player.deck.GetHand();
        if (handIndex < 0 || handIndex >= hand.Count) return false;

        Card card = hand[handIndex];
        if (player.currentCost < card.cost)
        {
            Debug.Log($"코스트 부족! 필요:{card.cost} 현재:{player.currentCost}");
            return false;
        }

        // 단일 대상 공격/독/화상 카드는 카드의 사거리(체비쇼프 거리) 안에 있어야 닿는다. AoE는 방 전체.
        bool hasOffense = card.damage > 0 || card.poisonApply > 0 || card.burnApply > 0;
        if (hasOffense && !card.isAoe)
        {
            Enemy resolved = target != null && target.IsAlive ? target : enemies.Find(e => e.IsAlive);
            if (resolved != null && BattleGridSystem.Chebyshev(floor.PlayerX, floor.PlayerY, resolved.x, resolved.y) > card.attackRange)
            {
                Debug.Log($"[{card.cardName}] 사거리 밖! (사거리 {card.attackRange})");
                return false;
            }
        }

        player.currentCost -= card.cost;
        player.deck.PlayCard(handIndex, BuildTargets(card, target), player, player.GetFinalAttackBonus());
        Debug.Log($"[{card.cardName}] 사용 | 남은 코스트:{player.currentCost} | 시간 {battleTime:0.0}");

        CheckNewKills(lastHitByCard: true); // 카드로 직접 쓰러뜨림 = 막타 → 마법사 마나·경험치
        CheckHpBelowTriggers();             // 자해 카드로 HP 30% 이하가 될 수 있다

        if (enemies.TrueForAll(e => !e.IsAlive)) { Finish(BattleState.Win);  return true; }
        if (!player.IsAlive)                     { Finish(BattleState.Lose); return true; }

        // 코스트를 다 쓰면 이 카드 시간이 지난 뒤 자동으로 정비에 들어간다
        bool drained = card.cost > 0 && player.currentCost <= 0;
        StartCoroutine(SpendTime(CardTime, enterRecoveryAfter: drained));
        return true;
    }

    // 턴 종료 버튼 - 공격 턴이면 정비에 들어가고, 정비 중이면 남은 정비 시간을 그대로 흘려보낸다(정비 넘기기)
    public void EndPlayerTurn()
    {
        if (!CanAct) return;

        if (Phase == PlayerPhase.Attack)
        {
            EnterRecovery();
            NotifyUI();
        }
        else
        {
            StartCoroutine(SpendTime(Mathf.Max(0.01f, recoveryEndsAt - battleTime), enterRecoveryAfter: false));
        }
    }

    // 전투 중 플레이어 이동 - UI(그리드 타일 클릭/WASD)에서 호출. 한 칸마다 시간을 쓴다(정비 중엔 2배).
    // 방 밖으로 나가면 도주: 그 순간 인접해 있던 적들의 공격 의도가 Attack이면 이탈 공격이 발동한다.
    public bool TryMovePlayer(int dx, int dy)
    {
        if (!CanAct) return false;

        var result = BattleGridSystem.TryMovePlayer(floor, room, enemies, dx, dy, out List<Enemy> attackers);
        if (result == BattleGridSystem.MoveResult.Blocked) return false;

        if (result == BattleGridSystem.MoveResult.Fled)
        {
            foreach (Enemy attacker in attackers)
            {
                EnemyAction action = attacker.PeekNextAction();
                if (action.intent != EnemyIntent.Attack) continue;
                Debug.Log($"[{attacker.characterName}] 이탈 공격!");
                attacker.ExecuteAction(action, player);
            }
            CheckHpBelowTriggers();
            Finish(player.IsAlive ? BattleState.Fled : BattleState.Lose);
            return true;
        }

        StartCoroutine(SpendTime(CurrentMoveTime, enterRecoveryAfter: false));
        return true;
    }

    // ─────────────────────────────────────────
    // 시간 흐름
    // ─────────────────────────────────────────

    // 플레이어 행동이 dt만큼 시간을 쓴다 - 그 사이에 행동할 차례가 된 적과 정비 종료를 시간 순서대로 처리한다.
    private IEnumerator SpendTime(float dt, bool enterRecoveryAfter)
    {
        busy  = true;
        state = BattleState.EnemyTurn;
        NotifyUI();

        float target = battleTime + dt;
        while (!battleOver)
        {
            Enemy next = NextEnemyToAct();
            float enemyAt    = next != null ? next.nextActTime : float.MaxValue;
            float recoveryAt = Phase == PlayerPhase.Recovery ? recoveryEndsAt : float.MaxValue;

            // 정비 종료와 적 행동이 같은 시각이면 적이 먼저 - 정비 중에 쳐 둔 방어막이 그 공격까지는 막아 준다
            if (recoveryAt < enemyAt && recoveryAt <= target)
            {
                battleTime = recoveryAt;
                StartAttackTurn();
                if (battleOver) yield break;
                NotifyUI();
                continue;
            }
            if (enemyAt > target) break;

            battleTime = enemyAt;
            yield return StartCoroutine(EnemyAct(next));
        }
        if (battleOver) yield break;

        battleTime = target;
        if (Phase == PlayerPhase.Recovery && battleTime >= recoveryEndsAt) StartAttackTurn();
        if (battleOver) yield break;
        if (enterRecoveryAfter && Phase == PlayerPhase.Attack) EnterRecovery();

        busy  = false;
        state = BattleState.PlayerTurn;
        NotifyUI();
    }

    private Enemy NextEnemyToAct()
    {
        Enemy best = null;
        foreach (Enemy e in enemies)
            if (e.IsAlive && (best == null || e.nextActTime < best.nextActTime)) best = e;
        return best;
    }

    // 정비 시작 - 이동이 느려지고 카드를 못 쓴다. 버린 카드 더미가 아니라 남은 덱(뽑을 카드)에서 손패를 채운다.
    // (뽑을 카드가 다 떨어졌을 때만 Deck.DrawCards가 버린 카드를 섞어 넣는다)
    private void EnterRecovery()
    {
        Phase          = PlayerPhase.Recovery;
        recoveryEndsAt = battleTime + RecoveryDuration;
        int toDraw = Mathf.Max(0, player.startHandSize - player.deck.GetHand().Count);
        player.deck.DrawCards(toDraw);
        LogStatus($"정비 시작 (+{toDraw}장, {RecoveryDuration:0.#} 동안 카드 사용 불가)");
    }

    // 공격 턴 시작 - 이동 속도 복귀, 공격 가능, 코스트 복구. 방어막 초기화·독/화상은 여기서 한 번 진행된다.
    private void StartAttackTurn()
    {
        Phase = PlayerPhase.Attack;
        player.OnTurnStart();
        CheckHpBelowTriggers();
        if (!player.IsAlive) { Finish(BattleState.Lose); return; }

        player.currentCost = player.maxCost;
        ApplyRelicTrigger(RelicTrigger.OnTurnStart);
        LogStatus("공격 턴");
    }

    // 적 한 번의 행동 - 상태이상 진행 후, 의도가 플레이어를 노리는데 붙어 있지 않으면 한 칸 접근(의도는 남김),
    // 붙어 있거나 자기에게 거는 행동이면 실행. 다음 행동은 1/속도 만큼 뒤.
    private IEnumerator EnemyAct(Enemy e)
    {
        e.nextActTime += 1f / Mathf.Max(0.1f, e.speed);

        e.OnTurnStart();
        e.ResetBlock();
        CheckNewKills(); // 독/화상으로 죽어도 OnKill 유물(혈약 반지 등)이 발동해야 한다
        if (!e.IsAlive)
        {
            if (enemies.TrueForAll(x => !x.IsAlive)) Finish(BattleState.Win);
            NotifyUI();
            yield break;
        }

        EnemyAction next = e.PeekNextAction();
        bool targetsPlayer = next.intent == EnemyIntent.Attack || next.intent == EnemyIntent.Poison || next.intent == EnemyIntent.Burn;
        bool adjacent = BattleGridSystem.Chebyshev(e.x, e.y, floor.PlayerX, floor.PlayerY) <= 1;

        if (targetsPlayer && !adjacent)
        {
            BattleGridSystem.StepEnemyToward(floor, room, e, enemies);
            Debug.Log($"[{e.characterName}] 접근 중... ({e.x},{e.y}) 시간 {battleTime:0.0}");
            NotifyUI();
            yield return new WaitForSeconds(0.25f);
            yield break;
        }

        NotifyUI();
        yield return new WaitForSeconds(0.3f);

        EnemyAction action = e.GetNextAction();
        e.ExecuteAction(action, player);
        CheckHpBelowTriggers();
        NotifyUI();
        yield return new WaitForSeconds(0.25f);

        if (!player.IsAlive) Finish(BattleState.Lose);
    }

    // 전투 끝 - 한 번만
    private void Finish(BattleState result)
    {
        if (battleOver) return;
        battleOver = true;
        busy       = false;
        state      = result;
        OnBattleEnd();
    }

    private void LogStatus(string what) =>
        Debug.Log($"=== {what} | 시간 {battleTime:0.0} | 코스트 {player.currentCost}/{player.maxCost} | HP {player.currentHp}/{player.maxHp} | 손패 {player.deck.GetHand().Count} ===");

    private void OnBattleEnd()
    {
        // 힘은 한 전투 동안만 유지된다. 맵에서 미리 올린 힘은 바로 다음 전투 한 번에만 들어간다.
        // 초기화하지 않으면 걸어서 회복한 코스트로 맵에서 담금질(힘 +2)을 반복해 힘이 끝없이 쌓이고,
        // 전투 목걸이·마왕의 왕관(전투 시작 시 힘)도 전투마다 누적됐다. (2026-11-10 밸런스 작업)
        if (player != null) player.strengthStack = 0;

        switch (state)
        {
            case BattleState.Win:
                Debug.Log("승리!");
                DungeonManager.Instance?.OnBattleWon(enemies);
                break;
            case BattleState.Fled:
                Debug.Log("도주 성공");
                DungeonManager.Instance?.OnBattleFled(floor, enemies);
                break;
            default:
                Debug.Log("패배...");
                DungeonManager.Instance?.OnBattleLost();
                break;
        }
    }

    // AoE → 살아있는 모든 적 / 단일 → 지정한 적 (죽었거나 없으면 첫 번째 생존 적)
    private List<Character> BuildTargets(Card card, Enemy target)
    {
        bool hasOffense = card.damage > 0 || card.poisonApply > 0 || card.burnApply > 0;
        if (!hasOffense) return new List<Character>();

        var alive = enemies.FindAll(e => e.IsAlive);
        if (card.isAoe) return alive.ConvertAll(e => (Character)e);

        Enemy resolved = target != null && target.IsAlive ? target
                       : alive.Count > 0                  ? alive[0] : null;
        return resolved != null ? new List<Character> { resolved } : new List<Character>();
    }

    // 새로 죽은 적 확인 → OnKill 유물 발동 (카드 처치뿐 아니라 독/화상 등 상태이상 처치 포함)
    // lastHitByCard: 플레이어가 카드로 직접 마지막 일격을 넣었을 때만 true - 마법사 마나·경험치는 이때만 쌓인다(독·화상 처치 제외).
    private void CheckNewKills(bool lastHitByCard = false)
    {
        foreach (Enemy e in enemies)
        {
            if (e.IsAlive || rewardedKills.Contains(e)) continue;
            rewardedKills.Add(e);
            ApplyRelicTrigger(RelicTrigger.OnKill);
            if (lastHitByCard)
            {
                player.GainManaFromLastHit();
                player.GainXp(LevelSystem.XpFor(e)); // 막타 경험치 → 레벨업 시 HP·공격·방어 상승 (LevelSystem)
            }
        }
    }

    private void ApplyRelicTrigger(RelicTrigger trigger)
    {
        foreach (string id in player.relics.GetAll())
        {
            RelicData relic = RelicDatabase.Get(id);
            if (relic == null) continue;
            foreach (RelicEffect eff in relic.effects)
            {
                if (eff.trigger != trigger) continue;
                ExecuteRelicEffect(eff);
                Debug.Log($"[유물] {relic.displayName}: {relic.description}");
            }
        }
    }

    private void ExecuteRelicEffect(RelicEffect eff)
    {
        switch (eff.effectType)
        {
            case RelicEffectType.GainBlock:    player.AddBlock(eff.value);           break;
            case RelicEffectType.GainCost:     player.currentCost += eff.value;      break;
            case RelicEffectType.DrawCard:     player.deck.DrawCards(eff.value);     break;
            case RelicEffectType.GainStrength: player.strengthStack += eff.value;    break;
            case RelicEffectType.HealHp:       player.Heal(eff.value);               break;
        }
    }

    // HP 30% 이하 트리거 - 플레이어 HP가 깎이는 모든 경로(적 공격/자해/독/화상) 뒤에 호출된다
    private void CheckHpBelowTriggers()
    {
        if ((float)player.currentHp / player.maxHp > 0.3f) return;

        foreach (string id in player.relics.GetAll())
        {
            if (usedOnceRelics.Contains(id)) continue;
            RelicData relic = RelicDatabase.Get(id);
            if (relic == null) continue;
            foreach (RelicEffect eff in relic.effects)
            {
                if (eff.trigger != RelicTrigger.OnHpBelow30) continue;
                ExecuteRelicEffect(eff);
                usedOnceRelics.Add(id);
                Debug.Log($"[유물] {relic.displayName} 발동 (HP 30% 이하)");
            }
        }
    }
}
