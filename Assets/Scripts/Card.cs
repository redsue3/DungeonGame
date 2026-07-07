using System.Collections.Generic;

public enum CardType   { Attack, Defense, Skill }
public enum CardRarity { Common, Rare }

public class Card
{
    public string             id;
    public string             cardName;
    public string             description;
    public int                manaCost;
    public CardType           cardType;
    public bool               isAoe;       // 전체 공격 여부
    public CardRarity         rarity;
    public CharacterClass?    classRestriction; // null = 모든 직업 사용 가능, 지정 시 해당 직업 전용

    public int damage;
    public int block;
    public int drawCount;
    public int healAmount;
    public int strengthGain;
    public int poisonApply;
    public int burnApply;
    public int selfDamage;

    // 성소(Shrine) 카드 전용 매커닉
    public int growOnUse;       // 사용할 때마다 이 카드의 damage가 영구적으로 이만큼 증가
    public int buffNextAttack;  // 사용 시 시전자의 '다음 공격 카드' 데미지를 이만큼 예약 증가
    public int buffNextDefense; // 사용 시 시전자의 '다음 방어 카드' 방어막을 이만큼 예약 증가

    public Card(string id, string name, int cost, CardType type,
                int dmg = 0, int blk = 0, int draw = 0, int heal = 0,
                int str = 0, int poison = 0, int burn = 0, int selfDmg = 0, bool aoe = false,
                CardRarity rarity = CardRarity.Common, CharacterClass? classRestriction = null,
                int growOnUse = 0, int buffNextAttack = 0, int buffNextDefense = 0)
    {
        this.id           = id;
        cardName          = name;
        manaCost          = cost;
        cardType          = type;
        damage            = dmg;
        block             = blk;
        drawCount         = draw;
        healAmount        = heal;
        strengthGain      = str;
        poisonApply       = poison;
        burnApply         = burn;
        selfDamage        = selfDmg;
        isAoe             = aoe;
        this.rarity       = rarity;
        this.classRestriction = classRestriction;
        this.growOnUse       = growOnUse;
        this.buffNextAttack  = buffNextAttack;
        this.buffNextDefense = buffNextDefense;
        description       = BuildDescription();
    }

    private string BuildDescription()
    {
        var parts = new List<string>();
        if (damage > 0)          parts.Add($"데미지 {damage}");
        if (block > 0)           parts.Add($"방어막 {block}");
        if (drawCount > 0)       parts.Add($"드로우 {drawCount}");
        if (healAmount > 0)      parts.Add($"회복 {healAmount}");
        if (strengthGain > 0)    parts.Add($"강인함 +{strengthGain}");
        if (poisonApply > 0)     parts.Add($"독 {poisonApply}");
        if (burnApply > 0)       parts.Add($"화상 {burnApply}");
        if (selfDamage > 0)      parts.Add($"자해 {selfDamage}");
        if (growOnUse > 0)       parts.Add($"사용마다 데미지 +{growOnUse}");
        if (buffNextAttack > 0)  parts.Add($"다음 공격 카드 데미지 +{buffNextAttack}");
        if (buffNextDefense > 0) parts.Add($"다음 방어 카드 방어막 +{buffNextDefense}");
        if (isAoe)               parts.Add("[전체]");
        return string.Join(", ", parts);
    }

    public Card Clone() => (Card)MemberwiseClone();
}
