using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { PlayerTurn, EnemyTurn, Win, Lose }

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

    private const int CostDrainPenalty = 1;

    public int         CurrentCost  => player != null ? player.currentCost : 0;
    public BattleState CurrentState => state;
    public List<Enemy> GetEnemies() => enemies;

    void Awake() => Instance = this;

    // 집단 조우
    public void StartBattle(PlayerCharacter playerCharacter, List<Enemy> enemyList)
    {
        player  = playerCharacter;
        enemies = new List<Enemy>(enemyList);
        usedOnceRelics.Clear();
        rewardedKills.Clear();
        costRefillPenalty = 0;
        firstPlayerTurn   = true;
        player.deck.ResetForBattle();
        HungerSystem.OnBattleStart(player);
        ApplyRelicTrigger(RelicTrigger.OnBattleStart);
        StartCoroutine(BattleLoop());
    }

    // 개별 조우 편의 오버로드
    public void StartBattle(PlayerCharacter playerCharacter, Enemy enemy)
        => StartBattle(playerCharacter, new List<Enemy> { enemy });

    private IEnumerator BattleLoop()
    {
        state = BattleState.PlayerTurn;

        while (player.IsAlive && enemies.Exists(e => e.IsAlive))
        {
            if (state == BattleState.PlayerTurn)
                yield return StartCoroutine(PlayerTurn());
            else if (state == BattleState.EnemyTurn)
                yield return StartCoroutine(EnemyTurn());
        }

        state = player.IsAlive ? BattleState.Win : BattleState.Lose;
        OnBattleEnd();
    }

    private IEnumerator PlayerTurn()
    {
        playerTurnEnded = false;

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

    private IEnumerator EnemyTurn()
    {
        foreach (Enemy e in enemies.FindAll(e => e.IsAlive))
        {
            e.OnTurnStart();
            e.ResetBlock();
            CheckNewKills(); // 독/화상으로 죽어도 OnKill 유물(혈약 반지 등)이 발동해야 한다

            if (!e.IsAlive) continue;

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
        if (state == BattleState.Win)
        {
            Debug.Log("승리!");
            DungeonManager.Instance?.OnBattleWon(enemies);
        }
        else
        {
            Debug.Log("패배...");
            DungeonManager.Instance?.OnBattleLost();
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
