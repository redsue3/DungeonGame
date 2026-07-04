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
    }

    public void OnTurnStart()
    {
        ResetBlock();
        ProcessStatusEffects();
    }

    public int GetFinalAttackBonus()  => attackBonus + strengthStack;
    public int GetFinalBlockBonus()   => dexterityStack;
}
