using System.Collections.Generic;

public enum CardType { Attack, Defense, Skill }

public class Card
{
    public string   id;
    public string   cardName;
    public string   description;
    public int      manaCost;
    public CardType cardType;
    public bool     isAoe;       // 전체 공격 여부

    public int damage;
    public int block;
    public int drawCount;
    public int healAmount;
    public int strengthGain;
    public int poisonApply;
    public int burnApply;
    public int selfDamage;

    public Card(string id, string name, int cost, CardType type,
                int dmg = 0, int blk = 0, int draw = 0, int heal = 0,
                int str = 0, int poison = 0, int burn = 0, int selfDmg = 0, bool aoe = false)
    {
        this.id      = id;
        cardName     = name;
        manaCost     = cost;
        cardType     = type;
        damage       = dmg;
        block        = blk;
        drawCount    = draw;
        healAmount   = heal;
        strengthGain = str;
        poisonApply  = poison;
        burnApply    = burn;
        selfDamage   = selfDmg;
        isAoe        = aoe;
        description  = BuildDescription();
    }

    private string BuildDescription()
    {
        var parts = new List<string>();
        if (damage > 0)       parts.Add($"데미지 {damage}");
        if (block > 0)        parts.Add($"방어막 {block}");
        if (drawCount > 0)    parts.Add($"드로우 {drawCount}");
        if (healAmount > 0)   parts.Add($"회복 {healAmount}");
        if (strengthGain > 0) parts.Add($"강인함 +{strengthGain}");
        if (poisonApply > 0)  parts.Add($"독 {poisonApply}");
        if (burnApply > 0)    parts.Add($"화상 {burnApply}");
        if (selfDamage > 0)   parts.Add($"자해 {selfDamage}");
        if (isAoe)            parts.Add("[전체]");
        return string.Join(", ", parts);
    }

    public Card Clone() => (Card)MemberwiseClone();
}
