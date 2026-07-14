public enum RelicRarity    { Common, Uncommon, Rare, Boss }

// Passive   = 상시 스탯. 획득 시 적용되고, 세이브 로드 복원 시에도 다시 적용된다.
// OnAcquire = 획득 시 딱 1회 실행되는 효과 (덱 조작 등). 로드 시 재실행하면 안 된다.
public enum RelicTrigger   { Passive, OnAcquire, OnBattleStart, OnTurnStart, OnKill, OnHpBelow30 }
public enum RelicEffectType
{
    BonusMaxHp,          // Passive: 최대 HP 증가 (현재 HP도 동량 증가)
    BonusMaxCost,        // Passive: 최대 코스트 증가
    BonusHandSize,       // Passive: 시작 패 수 증가
    BonusAttack,         // Passive: 공격 보너스 증가
    BonusGoldPct,        // Passive: 골드 수입 % 증가 (동적 적용)
    GainBlock,           // Active: 방어막 획득
    GainCost,            // Active: 현재 코스트 증가
    DrawCard,            // Active: 카드 드로우
    GainStrength,        // Active: 힘 증가
    HealHp,              // Active: HP 회복
    RemoveRandomCard,    // OnAcquire: 덱에서 무작위 카드 제거
    TransformRandomCard, // OnAcquire: 덱의 무작위 카드를 다른 카드로 교체
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
