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
            baseBlockBonus     = 2,
            manaPerKill        = 0,
            hpPerLevel         = 6,      // 레벨업 성장 - 전사: 체력·방어 위주
            attackPerLevel     = 0.5f,
            blockPerLevel      = 0.5f,
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
            maxHp              = 55,
            maxCost            = 3,
            startHandSize      = 6,
            baseAttackBonus    = 6,
            baseBlockBonus     = -1,
            manaPerKill        = 0,
            hpPerLevel         = 3,      // 레벨업 성장 - 도적: 공격 위주
            attackPerLevel     = 1f,
            blockPerLevel      = 0f,
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
            maxHp              = 45,
            maxCost            = 4,
            startHandSize      = 5,
            baseAttackBonus    = 0,
            baseBlockBonus     = 0,
            manaPerKill        = 1,
            hpPerLevel         = 2,      // 레벨업 성장 - 마법사: 체력만 조금, 성장은 마나로
            attackPerLevel     = 0f,
            blockPerLevel      = 0f,
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
            maxHp              = 72,
            maxCost            = 3,
            startHandSize      = 5,
            baseAttackBonus    = 2,
            baseBlockBonus     = 1,
            manaPerKill        = 0,
            hpPerLevel         = 5,      // 레벨업 성장 - 성기사: 전사보다 체력·방어가 조금 덜 오름
            attackPerLevel     = 0.5f,
            blockPerLevel      = 0.34f,
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
