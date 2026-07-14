using UnityEngine;

public enum CharacterClass { Warrior, Rogue, Mage, Paladin }

public class PlayerCharacter : Character
{
    public CharacterClass characterClass;

    public int attackBonus;
    public int maxCost;
    public int startHandSize;

    // 현재 코스트 - 전투/맵 공용으로 상주한다. 전투 2턴부터는 매 턴 풀 리필,
    // 맵에서는 카드 사용으로 소모하고 이동으로 회복 (MapCardSystem 담당).
    public int currentCost;
    public int stepsSinceCostRegen;

    public int strengthStack;

    // 성소 유틸 카드의 '다음 카드 예약 보너스' - 해당 타입 카드를 실제로 낼 때 1회 소모됨
    public int pendingAttackBonus;
    public int pendingDefenseBonus;

    public int gold;

    public int hunger    = HungerSystem.MaxHunger;
    public int maxHunger = HungerSystem.MaxHunger;
    public int stepsSinceMeal;

    public Deck           deck      = new Deck();
    public RelicInventory relics    = new RelicInventory();
    public Inventory      inventory = new Inventory();

    public bool IsStarving => hunger <= 0;

    public PlayerCharacter(CharacterClass cls) : base("", 0)
    {
        characterClass = cls;
        LoadFromDatabase(cls);
    }

    public void ChangeHunger(int delta) => hunger = Mathf.Clamp(hunger + delta, 0, maxHunger);

    // 체력이 가득 찬 상태가 아닐 때 회복하면 배고픔도 함께 소모된다
    public override void Heal(int amount)
    {
        bool wasDamaged = currentHp < maxHp;
        base.Heal(amount);
        if (wasDamaged) ChangeHunger(-HungerSystem.HealHungerCost(amount));
    }

    private void LoadFromDatabase(CharacterClass cls)
    {
        PlayerData data = PlayerDatabase.Get(cls);
        if (data == null) return;

        characterName = data.displayName;
        maxHp         = data.maxHp;
        currentHp     = data.maxHp;
        maxCost       = data.maxCost;
        currentCost   = data.maxCost;
        startHandSize = data.startHandSize;
        attackBonus   = data.baseAttackBonus;
    }

    // resetBlock=false: 전투 첫 턴 - 맵에서 미리 쳐둔 방어막을 유지한 채 상태이상만 진행
    public void OnTurnStart(bool resetBlock = true)
    {
        if (resetBlock) ResetBlock();
        ProcessStatusEffects();
    }

    public int GetFinalAttackBonus() => attackBonus + strengthStack;

    public int ConsumePendingAttackBonus()
    {
        int value = pendingAttackBonus;
        pendingAttackBonus = 0;
        return value;
    }

    public int ConsumePendingDefenseBonus()
    {
        int value = pendingDefenseBonus;
        pendingDefenseBonus = 0;
        return value;
    }
}
