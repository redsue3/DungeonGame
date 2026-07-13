using UnityEngine;

// 3단계 - 맵 탐색 중 비공격 카드 사용. 코스트만 소모하고 덱은 건드리지 않는다 (덱 순환은 전투 전용).
// 코스트는 전투와 공유되는 player.currentMana를 쓰고, 맵에서는 이동으로만 자연 회복된다.
public static class MapCardSystem
{
    public const int TilesPerCostRegen = 10; // 이 칸수만큼 이동할 때마다 코스트 1 회복

    // 맵 이동 시 DungeonManager.TryMove에서 호출 - 걸어서 코스트 회복
    public static void OnPlayerMove(PlayerCharacter player)
    {
        if (player.currentMana >= player.maxMana)
        {
            player.stepsSinceCostRegen = 0; // 가득 찬 동안엔 걸음을 적립하지 않는다
            return;
        }

        player.stepsSinceCostRegen++;
        if (player.stepsSinceCostRegen < TilesPerCostRegen) return;

        player.stepsSinceCostRegen = 0;
        player.currentMana++;
        Debug.Log($"[맵카드] 이동으로 코스트 +1 (현재 {player.currentMana}/{player.maxMana})");
    }

    // 맵에서 쓸 수 있는 카드: 공격 요소(데미지/독/화상)가 없고,
    // 전투 밖에서 의미 있는 효과(회복/방어막/강인함/예약 버프)가 하나라도 있는 카드.
    // 드로우 전용 카드는 맵에 손패가 없으므로 제외된다.
    public static bool IsUsableOnMap(Card card)
    {
        bool hasOffense = card.damage > 0 || card.poisonApply > 0 || card.burnApply > 0;
        if (hasOffense) return false;

        return card.healAmount > 0 || card.block > 0 || card.strengthGain > 0
            || card.buffNextAttack > 0 || card.buffNextDefense > 0;
    }

    // 맵에서 카드 사용 - 코스트를 지불하고 시전자 효과만 적용한다.
    // 방어막은 전투 첫 턴까지 유지되고(BattleManager가 첫 턴엔 블록을 초기화하지 않음), 회복은 배고픔을 소모한다.
    public static bool UseCard(PlayerCharacter player, Card card)
    {
        if (!IsUsableOnMap(card)) return false;
        if (player.currentMana < card.manaCost)
        {
            Debug.Log($"[맵카드] 코스트 부족! 필요:{card.manaCost} 현재:{player.currentMana}");
            return false;
        }

        player.currentMana -= card.manaCost;
        ApplyEffect(player, card);
        Debug.Log($"[맵카드] {card.cardName} 사용 | 남은 코스트 {player.currentMana}/{player.maxMana}");
        return true;
    }

    // Deck.ApplyCardEffect의 시전자 효과와 동일한 규칙 (드로우 제외 - 맵에는 손패가 없음)
    private static void ApplyEffect(PlayerCharacter player, Card card)
    {
        if (card.block > 0)
        {
            int bonus = player.GetFinalBlockBonus() + player.ConsumePendingDefenseBonus();
            player.AddBlock(card.block + bonus);
        }
        if (card.healAmount > 0)   player.Heal(card.healAmount);
        if (card.strengthGain > 0) player.strengthStack += card.strengthGain;
        if (card.selfDamage > 0)   player.TakeDamage(card.selfDamage);

        if (card.buffNextAttack > 0)  player.pendingAttackBonus  += card.buffNextAttack;
        if (card.buffNextDefense > 0) player.pendingDefenseBonus += card.buffNextDefense;
    }
}
