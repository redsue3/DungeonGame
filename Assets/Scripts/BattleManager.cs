using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { PlayerTurn, EnemyTurn, Win, Lose, Fled }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private PlayerCharacter  player;
    private List<Enemy>      enemies = new List<Enemy>();
    private BattleState      state;
    private bool             playerTurnEnded;
    private bool             firstPlayerTurn;   // 전투 첫 턴은 맵에서 들고 온 코스트/방어막을 그대로 쓴다 (리필/블록 초기화 없음)
    private int              costRefillPenalty; // 코스트 소진으로 턴이 자동 종료됐을 때, 다음 턴 리필량에서 깎이는 "리필 틈" 페널티
    private HashSet<string>  usedOnceRelics = new HashSet<string>(); // 전투당 1회만 발동하는 유물(불사조의 재 등)
    private HashSet<Enemy>   rewardedKills  = new HashSet<Enemy>();  // OnKill 유물을 이미 발동시킨 적 (중복 발동 방지)

    // 4단계(전투 그리드 통합) - 전투가 벌어지는 방과 그 방이 속한 층. 이동/도주/사거리 판정에 쓴다.
    private DungeonFloor     floor;
    private RoomInfo         room;
    private bool             hasMovedThisTurn; // 턴당 이동은 1칸만 (카드 코스트와 별개의 자원)

    private const int CostDrainPenalty = 1;

    public int         CurrentCost      => player != null ? player.currentCost : 0;
    public BattleState CurrentState     => state;
    public bool         HasMovedThisTurn => hasMovedThisTurn;
    public RoomInfo    Room             => room;
    public List<Enemy> GetEnemies() => enemies;

    void Awake() => Instance = this;

    // 집단 조우. dungeonFloor/battleRoom - 전투 중 이동/사거리/도주 판정에 쓰는 그리드 컨텍스트.
    public void StartBattle(PlayerCharacter playerCharacter, List<Enemy> enemyList, DungeonFloor dungeonFloor, RoomInfo battleRoom)
    {
        player  = playerCharacter;
        enemies = new List<Enemy>(enemyList);
        floor   = dungeonFloor;
        room    = battleRoom;
        usedOnceRelics.Clear();
        rewardedKills.Clear();
        costRefillPenalty = 0;
        firstPlayerTurn   = true;
        hasMovedThisTurn  = false;
        player.deck.ResetForBattle();
        HungerSystem.OnBattleStart(player);
        ApplyRelicTrigger(RelicTrigger.OnBattleStart);
        StartCoroutine(BattleLoop());
    }

    // 개별 조우 편의 오버로드
    public void StartBattle(PlayerCharacter playerCharacter, Enemy enemy, DungeonFloor dungeonFloor, RoomInfo battleRoom)
        => StartBattle(playerCharacter, new List<Enemy> { enemy }, dungeonFloor, battleRoom);

    private IEnumerator BattleLoop()
    {
        state = BattleState.PlayerTurn;

        // Fled는 TryMovePlayer가 직접 세팅하고 끝내는 상태라 여기서 별도 루프 처리는 안 함 -
        // 조건에서 걸러서 while을 빠져나가게만 하면 된다(안 걸러지면 매 프레임 아무 분기도 안 타는 채로
        // while이 계속 도는 무한루프에 빠져 에디터가 멈춘다).
        while (player.IsAlive && enemies.Exists(e => e.IsAlive) && state != BattleState.Fled)
        {
            if (state == BattleState.PlayerTurn)
                yield return StartCoroutine(PlayerTurn());
            else if (state == BattleState.EnemyTurn)
                yield return StartCoroutine(EnemyTurn());
        }

        if (state != BattleState.Fled)
            state = player.IsAlive ? BattleState.Win : BattleState.Lose;
        OnBattleEnd();
    }

    private IEnumerator PlayerTurn()
    {
        playerTurnEnded  = false;
        hasMovedThisTurn = false;

        if (firstPlayerTurn)
        {
            // 첫 턴: 맵에서 들고 온 코스트/방어막으로 그대로 시작 (풀 리필은 2턴부터 - 맵 카드 사용의 대가)
            firstPlayerTurn = false;
            player.OnTurnStart(resetBlock: false);
        }
        else
        {
            player.OnTurnStart();
            int penalty = costRefillPenalty;
            costRefillPenalty = 0;
            player.currentCost = Mathf.Max(0, player.maxCost - penalty);
            if (penalty > 0) Debug.Log($"[BattleManager] 리필 틈 페널티 적용 — 이번 턴 코스트 -{penalty}");
        }

        CheckHpBelowTriggers();
        if (!player.IsAlive) yield break; // 독/화상으로 턴 시작에 사망 - BattleLoop가 패배 처리

        player.deck.DrawCards(player.startHandSize);
        ApplyRelicTrigger(RelicTrigger.OnTurnStart);

        foreach (Enemy e in enemies.FindAll(e => e.IsAlive))
        {
            EnemyAction next = e.PeekNextAction();
            Debug.Log($"[{e.characterName} 의도] {next.description}  (HP:{e.currentHp}/{e.maxHp})");
        }
        Debug.Log($"=== 플레이어 턴 | 코스트:{player.currentCost}/{player.maxCost} | HP:{player.currentHp}/{player.maxHp} ===");
        LogHand();
        NotifyUI();

        // UI에서 EndPlayerTurn() 호출 또는 승패 확정까지 대기
        yield return new WaitUntil(() => playerTurnEnded || state != BattleState.PlayerTurn);
    }

    private void NotifyUI() => UIManager.Instance?.RefreshBattle();

    // 카드 사용 - UI 버튼에서 호출 (target: 단일 공격 카드가 때릴 적, null이면 첫 번째 생존 적)
    public bool UseCard(int handIndex, Enemy target = null)
    {
        if (state != BattleState.PlayerTurn) return false;

        List<Card> hand = player.deck.GetHand();
        if (handIndex < 0 || handIndex >= hand.Count) return false;

        Card card = hand[handIndex];
        if (player.currentCost < card.cost)
        {
            Debug.Log($"코스트 부족! 필요:{card.cost} 현재:{player.currentCost}");
            return false;
        }

        // 4단계 - 단일 대상 공격/독/화상 카드는 카드의 사거리(체비쇼프 거리) 안에 있어야 닿는다.
        // AoE는 방 전체를 때리므로 사거리 무관.
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
        Debug.Log($"[{card.cardName}] 사용 | 남은 코스트:{player.currentCost}");

        CheckNewKills();
        CheckHpBelowTriggers(); // 자해 카드로 HP 30% 이하가 될 수 있다

        if (enemies.TrueForAll(e => !e.IsAlive))
        {
            state = BattleState.Win;
        }
        else if (!player.IsAlive)
        {
            // 자해 카드로 자기 턴 중에 사망 - 턴 종료를 기다리지 않고 즉시 패배 처리
            state = BattleState.Lose;
        }
        // 이 카드로 코스트를 완전히 소진하면 턴이 자동으로 끝나고, 다음 턴 리필에 "틈"(페널티)이 생긴다.
        // 원래 0인 상태에서 0코스트 카드를 낸 경우는 소진이 아니므로 해당 없음.
        else if (card.cost > 0 && player.currentCost == 0)
        {
            costRefillPenalty = CostDrainPenalty;
            Debug.Log("[BattleManager] 코스트 소진 — 턴 자동 종료 (다음 턴 리필 -1)");
            EndPlayerTurn();
        }

        NotifyUI();
        return true;
    }

    // 턴 종료 - UI 버튼에서 호출
    public void EndPlayerTurn()
    {
        if (state != BattleState.PlayerTurn) return;
        player.deck.DiscardHand();
        state = BattleState.EnemyTurn;
        playerTurnEnded = true;
        NotifyUI();
    }

    // 전투 중 플레이어 이동 - UI(그리드 타일 클릭/WASD)에서 호출. 턴당 1칸만 허용.
    // 방 밖으로 나가면 도주: 그 순간 인접해 있던 적들의 공격 의도가 Attack이면 이탈 공격이 발동한다.
    public bool TryMovePlayer(int dx, int dy)
    {
        if (state != BattleState.PlayerTurn || hasMovedThisTurn) return false;

        var result = BattleGridSystem.TryMovePlayer(floor, room, enemies, dx, dy, out List<Enemy> attackers);
        if (result == BattleGridSystem.MoveResult.Blocked) return false;

        hasMovedThisTurn = true;

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
            state = player.IsAlive ? BattleState.Fled : BattleState.Lose;
        }

        NotifyUI();
        return true;
    }

    private IEnumerator EnemyTurn()
    {
        foreach (Enemy e in enemies.FindAll(e => e.IsAlive))
        {
            e.OnTurnStart();
            e.ResetBlock();
            CheckNewKills(); // 독/화상으로 죽어도 OnKill 유물(혈약 반지 등)이 발동해야 한다

            if (!e.IsAlive) continue;

            // 다음 의도가 플레이어를 직접 노리는 행동(공격/독/화상)인데 인접하지 않았으면,
            // 이번 턴엔 공격 대신 방 범위 안에서 한 칸 접근만 한다 - 의도는 소모되지 않고 다음 턴에 그대로 실행된다.
            // 방어/버프처럼 스스로에게 거는 행동은 거리와 무관하게 그대로 실행.
            EnemyAction next = e.PeekNextAction();
            bool targetsPlayer = next.intent == EnemyIntent.Attack || next.intent == EnemyIntent.Poison || next.intent == EnemyIntent.Burn;
            bool adjacent = BattleGridSystem.Chebyshev(e.x, e.y, floor.PlayerX, floor.PlayerY) <= 1;

            if (targetsPlayer && !adjacent)
            {
                BattleGridSystem.StepEnemyToward(floor, room, e, enemies);
                Debug.Log($"[{e.characterName}] 접근 중... ({e.x},{e.y})");
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            Debug.Log($"=== {e.characterName} 턴 | HP:{e.currentHp}/{e.maxHp} ===");
            yield return new WaitForSeconds(0.4f);

            EnemyAction action = e.GetNextAction();
            e.ExecuteAction(action, player);
            CheckHpBelowTriggers();

            yield return new WaitForSeconds(0.3f);

            if (!player.IsAlive)
            {
                state = BattleState.Lose;
                yield break;
            }
        }

        state = BattleState.PlayerTurn;
    }

    private void OnBattleEnd()
    {
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

    private void LogHand()
    {
        var hand = player.deck.GetHand();
        for (int i = 0; i < hand.Count; i++)
            Debug.Log($"  [{i}] {hand[i].cardName} (코스트:{hand[i].cost}) - {hand[i].description}");
    }

    // 새로 죽은 적 확인 → OnKill 유물 발동 (카드 처치뿐 아니라 독/화상 등 상태이상 처치 포함)
    private void CheckNewKills()
    {
        foreach (Enemy e in enemies)
        {
            if (e.IsAlive || rewardedKills.Contains(e)) continue;
            rewardedKills.Add(e);
            ApplyRelicTrigger(RelicTrigger.OnKill);
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
