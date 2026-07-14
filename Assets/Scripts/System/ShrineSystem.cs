using UnityEngine;

public enum ShrineBranch { Attack, Utility, Defense }

public class ShrineOption
{
    public ShrineBranch branch;
    public string       branchLabel;
    public Card         card;
}

// 캐릭터 성소 - 방문 시 공격/유틸/방어 3분기 중 하나를 골라 그 자리에서 만들어진 카드를 얻는다.
// 카드 스탯은 고정 데이터가 아니라 방문 시점의 플레이어 상태(계층/힘/체력)로 매번 다시 굴려서 생성된다.
public static class ShrineSystem
{
    private static int Level => DungeonManager.Instance != null ? DungeonManager.Instance.CurrentLayer : 1;

    public static ShrineOption[] GenerateOptions(PlayerCharacter player)
    {
        return new[]
        {
            new ShrineOption { branch = ShrineBranch.Attack,  branchLabel = "공격", card = GenerateAttackCard(player) },
            new ShrineOption { branch = ShrineBranch.Utility, branchLabel = "유틸", card = GenerateUtilityCard() },
            new ShrineOption { branch = ShrineBranch.Defense, branchLabel = "방어", card = GenerateDefenseCard(player) },
        };
    }

    // 공격 분기: 절반 확률로 "회심의 한방"(레벨*힘*임의수치) 또는 "성장형"(0코스트, 쓸수록 강해짐)
    private static Card GenerateAttackCard(PlayerCharacter player)
    {
        if (Random.value < 0.5f)
        {
            int strength = player.strengthStack + 1; // 힘이 0이어도 최소치는 보장
            int factor   = Random.Range(3, 6);
            int dmg      = Level * strength * factor;
            return new Card("shrine_burst", "성소의 회심일격", 2, CardType.Attack, dmg: dmg);
        }
        else
        {
            int baseDmg = Random.Range(2, 4);
            int growth  = Random.Range(1, 3);
            return new Card("shrine_growing", "성소의 성장하는 검", 0, CardType.Attack, dmg: baseDmg, growOnUse: growth);
        }
    }

    // 유틸 분기: 절반 확률로 "다음 공격 강화" 또는 "다음 방어 강화" (둘 다 0코스트)
    private static Card GenerateUtilityCard()
    {
        int amount = Random.Range(3, 7);
        return Random.value < 0.5f
            ? new Card("shrine_focus", "성소의 집중", 0, CardType.Skill, buffNextAttack: amount)
            : new Card("shrine_ward",  "성소의 가호", 0, CardType.Skill, buffNextDefense: amount);
    }

    // 방어 분기: 코스트 2, (레벨+최대체력)*임의수치의 대방벽
    private static Card GenerateDefenseCard(PlayerCharacter player)
    {
        float factor = Random.Range(0.15f, 0.25f);
        int   blk    = Mathf.RoundToInt((Level + player.maxHp) * factor);
        return new Card("shrine_bastion", "성소의 대방벽", 2, CardType.Defense, blk: blk);
    }
}
