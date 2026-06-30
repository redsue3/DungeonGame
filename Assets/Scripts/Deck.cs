using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    private List<Card> drawPile    = new List<Card>();
    private List<Card> hand        = new List<Card>();
    private List<Card> discardPile = new List<Card>();

    public int HandCount    => hand.Count;
    public int DrawCount    => drawPile.Count;
    public int DiscardCount => discardPile.Count;

    public void AddCard(Card card) => drawPile.Add(card);

    public bool RemoveCard(string cardId)
    {
        Card found = drawPile.Find(c => c.id == cardId);
        if (found != null) { drawPile.Remove(found); return true; }
        found = discardPile.Find(c => c.id == cardId);
        if (found != null) { discardPile.Remove(found); return true; }
        return false;
    }

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

    // 단일 타겟 편의 오버로드 (Unity BattleManager 하위 호환)
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
        foreach (Character target in targets)
        {
            if (card.damage > 0)
                target.TakeDamage(DamageCalculator.Calculate(card.damage, attackBonus));
            if (card.poisonApply > 0)
                target.poisonStack += card.poisonApply;
            if (card.burnApply > 0)
                target.burnStack += card.burnApply;
        }

        // 시전자 효과
        if (card.block > 0)
        {
            int bonus = (caster is PlayerCharacter pc) ? pc.GetFinalBlockBonus() : 0;
            caster.AddBlock(card.block + bonus);
        }
        if (card.drawCount > 0)    DrawCards(card.drawCount);
        if (card.healAmount > 0)   caster.Heal(card.healAmount);
        if (card.strengthGain > 0 && caster is PlayerCharacter pcs)
            pcs.strengthStack += card.strengthGain;
        if (card.selfDamage > 0)   caster.TakeDamage(card.selfDamage);
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

    public List<Card> GetHand()     => hand;
    public List<Card> GetDrawPile() => drawPile;

    public List<Card> GetAllCards()
    {
        var all = new List<Card>(drawPile);
        all.AddRange(hand);
        all.AddRange(discardPile);
        return all;
    }
}
