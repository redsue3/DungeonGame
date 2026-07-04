using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { PlayerTurn, EnemyTurn, Win, Lose }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private PlayerCharacter  player;
    private List<Enemy>      enemies = new List<Enemy>();
    private int              currentMana;
    private BattleState      state;
    private HashSet<string>  usedOnceRelics = new HashSet<string>();

    public int         CurrentMana  => currentMana;
    public BattleState CurrentState => state;
    public List<Enemy> GetEnemies() => enemies;

    void Awake() => Instance = this;

    // 집단 조우
    public void StartBattle(PlayerCharacter playerCharacter, List<Enemy> enemyList)
    {
        player  = playerCharacter;
        enemies = new List<Enemy>(enemyList);
        usedOnceRelics.Clear();
        player.deck.ResetForBattle();
        HungerSystem.OnBattleStart(player);
        ApplyRelicTrigger(RelicTrigger.OnBattleStart);
        StartCoroutine(BattleLoop());
    }

    // 개별 조우 편의 오버로드
    public void StartBattle(PlayerCharacter playerCharacter, Enemy enemy)
        => StartBattle(playerCharacter, new List<Enemy> { enemy });

    private bool playerTurnEnded = false;

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
        player.OnTurnStart();
        currentMana = player.maxMana;
        player.deck.DrawCards(player.startHandSize);
        ApplyRelicTrigger(RelicTrigger.OnTurnStart);

        foreach (Enemy e in enemies.FindAll(e => e.IsAlive))
        {
            EnemyAction next = e.PeekNextAction();
            Debug.Log($"[{e.characterName} 의도] {next.description}  (HP:{e.currentHp}/{e.maxHp})");
        }
        Debug.Log($"=== 플레이어 턴 | 마나:{currentMana}/{player.maxMana} | HP:{player.currentHp}/{player.maxHp} ===");
        LogHand();
        NotifyUI();

        // UI에서 EndPlayerTurn() 호출 때까지 대기
        yield return new WaitUntil(() => playerTurnEnded || state == BattleState.Win);
    }

    private void NotifyUI() => UIManager.Instance?.RefreshBattle();

    // 카드 사용 - UI 버튼에서 호출 (targetIndex: 적이 여러 명일 때 선택)
    public bool UseCard(int handIndex, int targetIndex = 0)
    {
        if (state != BattleState.PlayerTurn) return false;

        List<Card> hand = player.deck.GetHand();
        if (handIndex < 0 || handIndex >= hand.Count) return false;

        Card card = hand[handIndex];
        if (currentMana < card.manaCost)
        {
            Debug.Log($"마나 부족! 필요:{card.manaCost} 현재:{currentMana}");
            return false;
        }

        currentMana -= card.manaCost;

        List<Character> targets = BuildTargets(card, targetIndex);
        player.deck.PlayCard(handIndex, targets, player, player.GetFinalAttackBonus());

        Debug.Log($"[{card.cardName}] 사용 | 남은 마나:{currentMana}");

        // 처치 판정 → OnKill 유물 발동
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy e = enemies[i];
            string killKey = "killed_" + i;
            if (!e.IsAlive && !usedOnceRelics.Contains(killKey))
            {
                usedOnceRelics.Add(killKey);
                ApplyRelicTrigger(RelicTrigger.OnKill);
            }
        }

        if (enemies.TrueForAll(e => !e.IsAlive)) state = BattleState.Win;
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
        var aliveEnemies = enemies.FindAll(e => e.IsAlive);

        foreach (Enemy e in aliveEnemies)
        {
            e.OnTurnStart();
            e.ResetBlock();

            if (!e.IsAlive) continue; // 상태이상으로 사망 가능

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
            DungeonManager.Instance?.OnBattleWon();
        }
        else
        {
            Debug.Log("패배...");
            DungeonManager.Instance?.OnBattleLost();
        }
    }

    // AoE → 살아있는 모든 적 / 단일 → targetIndex 번째 적
    private List<Character> BuildTargets(Card card, int targetIndex)
    {
        bool hasOffense = card.damage > 0 || card.poisonApply > 0 || card.burnApply > 0;
        if (!hasOffense) return new List<Character>();

        var alive = enemies.FindAll(e => e.IsAlive);
        if (card.isAoe) return alive.ConvertAll(e => (Character)e);

        int idx = (targetIndex >= 0 && targetIndex < alive.Count) ? targetIndex : 0;
        return new List<Character> { alive[idx] };
    }

    private void LogHand()
    {
        var hand = player.deck.GetHand();
        for (int i = 0; i < hand.Count; i++)
            Debug.Log($"  [{i}] {hand[i].cardName} (마나:{hand[i].manaCost}) - {hand[i].description}");
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
            case RelicEffectType.GainMana:     currentMana += eff.value;             break;
            case RelicEffectType.DrawCard:     player.deck.DrawCards(eff.value);     break;
            case RelicEffectType.GainStrength: player.strengthStack += eff.value;    break;
            case RelicEffectType.HealHp:       player.Heal(eff.value);               break;
        }
    }

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
