[System.Serializable]
public class PlayerData
{
    public CharacterClass characterClass;
    public string         displayName;
    public int            maxHp;
    public int            maxCost;
    public int            startHandSize;
    public int            baseAttackBonus;
    public int            baseBlockBonus;   // 방어 카드 방어막에 더해지는 값 (음수면 깎임, 최소 0) - 직업의 '방어력'
    public int            manaPerKill;      // 막타(카드로 적을 직접 쓰러뜨림)마다 쌓이는 마나 (마법사만). 런 내내 남고 공격 카드 피해에 더해진다

    // 레벨업 성장치 (LevelSystem). 공격·방어는 소수로 두고 내림 - 0.5면 2레벨마다 +1
    public int            hpPerLevel;       // 레벨업마다 최대 HP(와 현재 HP) 증가
    public float          attackPerLevel;   // 레벨당 공격 보너스 증가
    public float          blockPerLevel;    // 레벨당 방어 보너스 증가

    public string[]       starterDeckCardIds;
}
