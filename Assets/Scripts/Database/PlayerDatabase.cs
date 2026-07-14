using System.Collections.Generic;

public static class PlayerDatabase
{
    private static readonly Dictionary<CharacterClass, PlayerData> table =
        new Dictionary<CharacterClass, PlayerData>
    {
        [CharacterClass.Warrior] = new PlayerData
        {
            characterClass     = CharacterClass.Warrior,
            displayName        = "전사",
            maxHp              = 80,
            maxCost            = 3,
            startHandSize      = 5,
            baseAttackBonus    = 2,
            starterDeckCardIds = new[]
            {
                "strike","strike","strike","strike","strike",
                "defend","defend","defend","defend",
                "heavy_strike"
            }
        },

        [CharacterClass.Rogue] = new PlayerData
        {
            characterClass     = CharacterClass.Rogue,
            displayName        = "도적",
            maxHp              = 65,
            maxCost            = 3,
            startHandSize      = 6,
            baseAttackBonus    = 0,
            starterDeckCardIds = new[]
            {
                "dagger","dagger","dagger","dagger","dagger","dagger",
                "evade","evade","evade",
                "combo"
            }
        },

        [CharacterClass.Mage] = new PlayerData
        {
            characterClass     = CharacterClass.Mage,
            displayName        = "마법사",
            maxHp              = 55,
            maxCost            = 4,
            startHandSize      = 5,
            baseAttackBonus    = 0,
            starterDeckCardIds = new[]
            {
                "fireball","fireball","fireball","fireball",
                "mana_shield","mana_shield","mana_shield",
                "lightning",
                "draw_spell"
            }
        },

        [CharacterClass.Paladin] = new PlayerData
        {
            characterClass     = CharacterClass.Paladin,
            displayName        = "성기사",
            maxHp              = 70,
            maxCost            = 3,
            startHandSize      = 5,
            baseAttackBonus    = 1,
            starterDeckCardIds = new[]
            {
                "holy_strike","holy_strike","holy_strike","holy_strike",
                "shield_bash","shield_bash","shield_bash",
                "sacred_heal","sacred_heal",
                "judgement"
            }
        },
    };

    public static PlayerData Get(CharacterClass cls)
    {
        if (table.TryGetValue(cls, out PlayerData data)) return data;
        UnityEngine.Debug.LogError($"PlayerDatabase: '{cls}' 없음");
        return null;
    }
}
