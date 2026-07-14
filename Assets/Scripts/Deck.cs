using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    private List<Card> drawPile    = new List<Card>();
    private List<Card> hand        = new List<Card>();
    private List<Card> discardPile = new List<Card>();

    public int DrawCount    => drawPile.Count;
    public int DiscardCount => discardPile.Count;

    public void AddCard(Card card) => drawPile.Add(card);

    // 인스턴스 기준 제거 - 같은 id 카드가 여러 장(성소 성장형처럼 스탯이 다른 사본 포함)이어도 정확히 그 카드만 지운다
    public bool RemoveCard(Card card) =>
        drawPile.Remove(card) || hand.Remove(card) || discardPile.Remove(card);

    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (drawPile.Count == 0) ReshuffleDiscard();
            if (drawPile.Count == 0) break;
            int idx = Random.Range(0, drawPile.Count);
            hand.Add(drawPile[idx]);
            drawPile.RemoveAt(idx);
        }
    }

    // 단일 타겟 편의 오버로드
    public bool PlayCard(int handIndex, Character target, Character caster, int attackBonus = 0)
    {
        return PlayCard(handIndex, new List<Character> { target }, caster, attackBonus);
    }

    // 다중 타겟 (AoE 및 집단전 지원)
    public bool PlayCard(int handIndex, List<Character> targets, Character caster, int attackBonus = 0)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return false;
        Card card = hand[handIndex];
        ApplyCardEffect(card, targets, caster, attackBonus);
        hand.RemoveAt(handIndex);
        discardPile.Add(card);
        return true;
    }

    private void ApplyCardEffect(Card card, List<Character> targets, Character caster, int attackBonus)
    {
        // 타겟별 효과 (데미지/독/화상)
        if (card.damage > 0)
        {
            int finalAttackBonus = attackBonus;
            if (caster is PlayerCharacter attacker)
                finalAttackBonus += attacker.ConsumePendingAttackBonus();

            foreach (Character target in targets)
                target.TakeDamage(DamageCalculator.Calculate(card.damage, finalAttackBonus));

            if (card.growOnUse > 0) // 성소의 성장형 카드 - 사용할수록 영구 강화
            {
                card.damage += card.growOnUse;
                card.RebuildDescription();
            }
        }
        foreach (Character target in targets)
        {
            if (card.poisonApply > 0) target.poisonStack += card.poisonApply;
            if (card.burnApply > 0)   target.burnStack   += card.burnApply;
        }

        // 시전자 효과 - 맵 탐색(MapCardSystem)과 공유하는 규칙. 드로우만 덱이 있어야 해서 여기서 처리.
        card.ApplyCasterEffects(caster);
        if (card.drawCount > 0) DrawCards(card.drawCount);
    }

    public void DiscardHand()
    {
        discardPile.AddRange(hand);
        hand.Clear();
    }

    public void ResetForBattle()
    {
        drawPile.AddRange(hand);
        drawPile.AddRange(discardPile);
        hand.Clear();
        discardPile.Clear();
    }

    private void ReshuffleDiscard()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Debug.Log("덱을 다시 섞었습니다.");
    }

    public List<Card> GetHand() => hand;

    public List<Card> GetAllCards()
    {
        var all = new List<Card>(drawPile);
        all.AddRange(hand);
        all.AddRange(discardPile);
        return all;
    }
}
