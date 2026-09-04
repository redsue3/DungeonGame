using UnityEngine;

public enum CharacterClass { Warrior, Rogue, Mage, Paladin }

public class PlayerCharacter : Character
{
    public CharacterClass characterClass;

    public int attackBonus;
    public int maxMana;
    public int startHandSize;

    public int strengthStack;
    public int dexterityStack;

    // 성소 유틸 카드의 '다음 카드 예약 보너스' - 해당 타입 카드를 실제로 낼 때 1회 소모됨
    public int pendingAttackBonus;
    public int pendingDefenseBonus;

    // 탐사 코스트 - 맵 탐색 중 카드 사용에 쓰는 자원, 전투용 currentMana(BattleManager)와 완전히 별개.
    public int explorationCost;
    public int maxExplorationCost;
    public int stepsSinceCostRegen;

    // 맵에서 마지막으로 사용한 비공격 카드 - '기습' 1회로 다음 전투 시작 시 적용되고 사라짐 (직전 1장만 유지)
    public Card pendingAmbushCard;

    public int gold;
    public int currentFloor;

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
        maxMana       = data.maxMana;
        startHandSize = data.startHandSize;
        attackBonus   = data.baseAttackBonus;

        maxExplorationCost = data.maxMana;
        explorationCost     = maxExplorationCost;
    }

    public void OnTurnStart()
    {
        ResetBlock();
        ProcessStatusEffects();
    }

    public int GetFinalAttackBonus()  => attackBonus + strengthStack;
    public int GetFinalBlockBonus()   => dexterityStack;

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

    // 맵에서 카드를 쓸 때마다 호출 - 직전에 쓴 카드로 덮어써서 항상 최근 1장만 기억한다.
    public void SetAmbush(Card card) => pendingAmbushCard = card;

    // 전투 시작 시 BattleManager에서 1회 소모
    public Card ConsumeAmbush()
    {
        Card card = pendingAmbushCard;
        pendingAmbushCard = null;
        return card;
    }
}
