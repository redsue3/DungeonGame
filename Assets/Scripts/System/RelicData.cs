public enum RelicRarity    { Common, Uncommon, Rare, Boss }
public enum RelicTrigger   { Passive, OnBattleStart, OnTurnStart, OnKill, OnHpBelow30 }
public enum RelicEffectType
{
    BonusMaxHp,          // Passive: 최대 HP 증가 (현재 HP도 동량 증가)
    BonusMaxMana,        // Passive: 최대 마나 증가
    BonusHandSize,       // Passive: 시작 패 수 증가
    BonusAttack,         // Passive: 공격 보너스 증가
    BonusGoldPct,        // Passive: 골드 수입 % 증가 (동적 적용)
    GainBlock,           // Active: 방어막 획득
    GainMana,            // Active: 현재 마나 증가
    DrawCard,            // Active: 카드 드로우
    GainStrength,        // Active: 힘 증가
    HealHp,              // Active: HP 회복
    RemoveRandomCard,    // Passive(획득 시 1회): 덱에서 무작위 카드 제거
    TransformRandomCard, // Passive(획득 시 1회): 덱의 무작위 카드를 다른 카드로 교체
}

[System.Serializable]
public class RelicEffect
{
    public RelicTrigger    trigger;
    public RelicEffectType effectType;
    public int             value;
}

[System.Serializable]
public class RelicData
{
    public string        id;
    public string        displayName;
    public string        description;
    public RelicRarity   rarity;
    public RelicEffect[] effects;
}
