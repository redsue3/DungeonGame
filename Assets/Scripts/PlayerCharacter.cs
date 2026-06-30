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

    public Deck           deck   = new Deck();
    public RelicInventory relics = new RelicInventory();

    public PlayerCharacter(CharacterClass cls) : base("", 0)
    {
        characterClass = cls;
        LoadFromDatabase(cls);
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
