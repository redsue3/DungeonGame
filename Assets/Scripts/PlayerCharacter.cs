using UnityEngine;

public enum CharacterClass { Warrior, Rogue, Mage, Paladin }

public class PlayerCharacter : Character
{
    public CharacterClass characterClass;

    public int attackBonus;
    public int blockBonus;   // 직업 방어력 - 방어 카드 방어막에 더해진다 (Card.ApplyCasterEffects)
    public int maxCost;
    public int startHandSize;

    // 마법사의 마나 - 막타(카드로 적을 직접 쓰러뜨림)마다 manaPerKill씩 영구히 쌓이고(런이 끝날 때까지 유지),
    // 마나 3마다 공격 카드 피해 +1 (ManaPerBonusDamage). 1층에서는 약하고 깊이 내려갈수록 강해지는 성장형 직업을 만든다.
    public int manaPerKill;
    public int manaStack;
    public bool UsesMana => manaPerKill > 0;

    // 레벨 (LevelSystem) - 막타 경험치로 오르고, 직업별 성장치만큼 최대 HP·공격·방어가 오른다
    public int level = 1;
    public int xp;               // 현재 레벨 안에서 쌓인 경험치 (레벨업하면 필요량만큼 빠진다)
    public int XpToNext => LevelSystem.XpToNext(level);
    private int   baseAttackBonusValue, baseBlockBonusValue, hpPerLevel;
    private float attackPerLevel, blockPerLevel;

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
        blockBonus    = data.baseBlockBonus;
        manaPerKill   = data.manaPerKill;

        baseAttackBonusValue = data.baseAttackBonus;
        baseBlockBonusValue  = data.baseBlockBonus;
        hpPerLevel           = data.hpPerLevel;
        attackPerLevel       = data.attackPerLevel;
        blockPerLevel        = data.blockPerLevel;
    }

    // 경험치를 더하고 오른 레벨 수를 돌려준다 (BattleManager.CheckNewKills에서 카드 막타일 때)
    public int GainXp(int amount)
    {
        int levelsGained = 0;
        xp += amount;
        while (level < LevelSystem.MaxLevel && xp >= XpToNext)
        {
            xp -= XpToNext;
            LevelUp();
            levelsGained++;
        }
        if (level >= LevelSystem.MaxLevel) xp = 0;
        return levelsGained;
    }

    // 세이브 복원용 - 저장된 레벨까지 성장치를 다시 적용한다 (유물 상시 효과를 먼저 적용한 뒤 호출)
    public void RestoreLevel(int savedLevel, int savedXp)
    {
        while (level < Mathf.Min(savedLevel, LevelSystem.MaxLevel)) LevelUp();
        xp = Mathf.Max(0, savedXp);
    }

    // 최대 HP는 유물(철심)도 건드리므로 누적으로 더하고, 공격·방어 보너스는 기본값 + 성장치 x (레벨-1)을 내림해서 다시 계산
    private void LevelUp()
    {
        level++;
        maxHp     += hpPerLevel;
        currentHp += hpPerLevel;
        attackBonus = baseAttackBonusValue + Mathf.FloorToInt(attackPerLevel * (level - 1) + 0.001f);
        blockBonus  = baseBlockBonusValue  + Mathf.FloorToInt(blockPerLevel  * (level - 1) + 0.001f);
        Debug.Log($"[레벨업] {characterName} Lv {level} - 최대 HP {maxHp}, 공격 +{attackBonus}, 방어 {blockBonus}");
    }

    // resetBlock=false: 전투 첫 턴 - 맵에서 미리 쳐둔 방어막을 유지한 채 상태이상만 진행
    public void OnTurnStart(bool resetBlock = true)
    {
        if (resetBlock) ResetBlock();
        ProcessStatusEffects();
    }

    // 마나는 ManaPerBonusDamage마다 공격 카드 피해 +1 (Tools/BalanceSim이 이 상수를 읽는다)
    public const int ManaPerBonusDamage = 3;
    public int ManaDamageBonus => manaStack / ManaPerBonusDamage;

    public int GetFinalAttackBonus() => attackBonus + strengthStack + ManaDamageBonus;

    // BattleManager.CheckNewKills에서 카드 막타일 때 호출 - 마나를 쓰는 직업(마법사)만 쌓인다
    public void GainManaFromLastHit()
    {
        if (UsesMana) manaStack += manaPerKill;
    }

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
