using System.Collections.Generic;

public enum CardType { Attack, Defense, Skill }

public class Card
{
    public string   id;
    public string   cardName;
    public string   description;
    public int      cost;
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

    // 성소(Shrine) 카드 전용 매커닉
    public int growOnUse;       // 사용할 때마다 이 카드의 damage가 영구적으로 이만큼 증가
    public int buffNextAttack;  // 사용 시 시전자의 '다음 공격 카드' 데미지를 이만큼 예약 증가
    public int buffNextDefense; // 사용 시 시전자의 '다음 방어 카드' 방어막을 이만큼 예약 증가

    public Card(string id, string name, int cost, CardType type,
                int dmg = 0, int blk = 0, int draw = 0, int heal = 0,
                int str = 0, int poison = 0, int burn = 0, int selfDmg = 0, bool aoe = false,
                int growOnUse = 0, int buffNextAttack = 0, int buffNextDefense = 0)
    {
        this.id      = id;
        cardName     = name;
        this.cost    = cost;
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
        this.growOnUse       = growOnUse;
        this.buffNextAttack  = buffNextAttack;
        this.buffNextDefense = buffNextDefense;
        RebuildDescription();
    }

    // 시전자 자신에게 적용되는 효과 (방어막/회복/강인함/자해/예약 버프).
    // 전투(Deck.ApplyCardEffect)와 맵 탐색(MapCardSystem.UseCard)이 같은 규칙을 공유한다.
    // 드로우는 덱 순환이 필요해서 Deck 쪽에서만 처리한다.
    public void ApplyCasterEffects(Character caster)
    {
        var player = caster as PlayerCharacter;

        if (block > 0)
        {
            int bonus = player != null ? player.ConsumePendingDefenseBonus() : 0;
            caster.AddBlock(block + bonus);
        }
        if (healAmount > 0)                      caster.Heal(healAmount);
        if (strengthGain > 0 && player != null)  player.strengthStack += strengthGain;
        if (selfDamage > 0)                      caster.TakeDamage(selfDamage);

        if (player != null)
        {
            if (buffNextAttack > 0)  player.pendingAttackBonus  += buffNextAttack;
            if (buffNextDefense > 0) player.pendingDefenseBonus += buffNextDefense;
        }
    }

    // 성장형(growOnUse) 카드처럼 스탯이 나중에 변하는 카드는 이걸 다시 불러 설명을 갱신한다
    public void RebuildDescription()
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
        description = string.Join(", ", parts);
    }

    public Card Clone() => (Card)MemberwiseClone();
}
