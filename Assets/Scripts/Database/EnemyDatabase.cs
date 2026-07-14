using System.Collections.Generic;

// 모든 적 스탯 정의. EnemyFactory가 여기서 불러옴.
public static class EnemyDatabase
{
    private static readonly Dictionary<string, EnemyData> table = new Dictionary<string, EnemyData>
    {
        // ─────────────────────────────────────────
        // 1계층 일반 몬스터 (기믹 없음, 단순 공격)
        // ─────────────────────────────────────────
        ["slime"] = new EnemyData
        {
            id           = "slime",
            displayName  = "슬라임",
            hp           = 28,
            baseAttack   = 0,
            rewardGoldMin = 6,
            rewardGoldMax = 10,
            isElite      = false,
            isBoss       = false,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Attack, value = 6,  description = "돌진" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 6,  description = "돌진" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 9,  description = "강하게 돌진" },
            }
        },

        ["thief"] = new EnemyData
        {
            id           = "thief",
            displayName  = "도둑",
            hp           = 22,
            baseAttack   = 0,
            rewardGoldMin = 7,
            rewardGoldMax = 12,
            isElite      = false,
            isBoss       = false,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Attack, value = 5,  description = "단도 찌르기" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 5,  description = "단도 찌르기" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 5,  description = "단도 찌르기" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 11, description = "급소 공격" },
            }
        },

        // ─────────────────────────────────────────
        // 1계층 엘리트
        // ─────────────────────────────────────────
        ["goblin_elite"] = new EnemyData
        {
            id           = "goblin_elite",
            displayName  = "고블린 두목",
            hp           = 55,
            baseAttack   = 0,
            rewardGoldMin = 20,
            rewardGoldMax = 28,
            isElite      = true,
            isBoss       = false,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Attack, value = 8,  description = "몽둥이 후려치기" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 8,  description = "몽둥이 후려치기" },
                new EnemyActionData { intent = EnemyIntent.Defend, value = 10, description = "방어 자세" },
                new EnemyActionData { intent = EnemyIntent.Buff,   value = 3,  description = "흥분 (공격력 UP)" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "분노의 일격" },
            }
        },

        ["skeleton_elite"] = new EnemyData
        {
            id           = "skeleton_elite",
            displayName  = "해골 기사",
            hp           = 60,
            baseAttack   = 0,
            rewardGoldMin = 22,
            rewardGoldMax = 30,
            isElite      = true,
            isBoss       = false,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Defend, value = 12, description = "방패 올리기" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 13, description = "창 찌르기" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 9,  description = "창 휘두르기" },
                new EnemyActionData { intent = EnemyIntent.Defend, value = 8,  description = "방패 올리기" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 18, description = "강습" },
            }
        },

        // ─────────────────────────────────────────
        // 1계층 보스
        // ─────────────────────────────────────────
        ["giant_slime_boss"] = new EnemyData
        {
            id           = "giant_slime_boss",
            displayName  = "거대 슬라임",
            hp           = 80,
            baseAttack   = 0,
            rewardGoldMin = 35,
            rewardGoldMax = 45,
            isElite      = false,
            isBoss       = true,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Attack, value = 10, description = "몸통 박치기" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 10, description = "몸통 박치기" },
                new EnemyActionData { intent = EnemyIntent.Defend, value = 14, description = "끈적끈적 굳기" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 18, description = "대형 충격파" },
                new EnemyActionData { intent = EnemyIntent.Poison, value = 3,  description = "독액 분비" },
            }
        },

        // ─────────────────────────────────────────
        // 2계층 일반 몬스터
        // ─────────────────────────────────────────
        ["orc"] = new EnemyData
        {
            id           = "orc",
            displayName  = "오크 전사",
            hp           = 45,
            baseAttack   = 0,
            rewardGoldMin = 10,
            rewardGoldMax = 16,
            isElite      = false,
            isBoss       = false,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Attack, value = 10, description = "도끼 찍기" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 10, description = "도끼 찍기" },
                new EnemyActionData { intent = EnemyIntent.Defend, value = 8,  description = "방어 자세" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 15, description = "대도끼 강타" },
            }
        },

        ["poison_spider"] = new EnemyData
        {
            id           = "poison_spider",
            displayName  = "독 거미",
            hp           = 35,
            baseAttack   = 0,
            rewardGoldMin = 9,
            rewardGoldMax = 14,
            isElite      = false,
            isBoss       = false,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Poison, value = 3, description = "독침 발사" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 7, description = "물기" },
                new EnemyActionData { intent = EnemyIntent.Poison, value = 2, description = "독침 발사" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 7, description = "물기" },
            }
        },

        // ─────────────────────────────────────────
        // 2계층 엘리트
        // ─────────────────────────────────────────
        ["dark_mage_elite"] = new EnemyData
        {
            id           = "dark_mage_elite",
            displayName  = "흑마법사",
            hp           = 75,
            baseAttack   = 0,
            rewardGoldMin = 28,
            rewardGoldMax = 38,
            isElite      = true,
            isBoss       = false,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Burn,   value = 3,  description = "화염 저주" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 12, description = "암흑 화살" },
                new EnemyActionData { intent = EnemyIntent.Buff,   value = 4,  description = "마력 증폭" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 20, description = "암흑 폭발" },
                new EnemyActionData { intent = EnemyIntent.Poison, value = 4,  description = "독 안개" },
            }
        },

        // ─────────────────────────────────────────
        // 2계층 보스
        // ─────────────────────────────────────────
        ["orc_warchief_boss"] = new EnemyData
        {
            id           = "orc_warchief_boss",
            displayName  = "오크 족장",
            hp           = 120,
            baseAttack   = 0,
            rewardGoldMin = 50,
            rewardGoldMax = 65,
            isElite      = false,
            isBoss       = true,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Buff,   value = 5,  description = "전쟁의 함성" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "도끼 폭풍" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "도끼 폭풍" },
                new EnemyActionData { intent = EnemyIntent.Defend, value = 18, description = "방어막 전개" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 22, description = "분노의 대도끼" },
            }
        },

        // ─────────────────────────────────────────
        // 3계층 일반 몬스터
        // ─────────────────────────────────────────
        ["cursed_warrior"] = new EnemyData
        {
            id           = "cursed_warrior",
            displayName  = "저주받은 전사",
            hp           = 62,
            baseAttack   = 0,
            rewardGoldMin = 14,
            rewardGoldMax = 20,
            isElite      = false,
            isBoss       = false,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Buff,   value = 3,  description = "저주의 힘" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "저주받은 검" },
                new EnemyActionData { intent = EnemyIntent.Defend, value = 10, description = "어두운 방어" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 20, description = "파멸의 일격" },
            }
        },

        ["shadow_witch"] = new EnemyData
        {
            id           = "shadow_witch",
            displayName  = "그림자 마녀",
            hp           = 48,
            baseAttack   = 0,
            rewardGoldMin = 13,
            rewardGoldMax = 19,
            isElite      = false,
            isBoss       = false,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Poison, value = 4, description = "독 안개" },
                new EnemyActionData { intent = EnemyIntent.Burn,   value = 3, description = "저주의 화염" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 11, description = "어둠의 화살" },
                new EnemyActionData { intent = EnemyIntent.Poison, value = 3, description = "독 안개" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 15, description = "암흑 폭발" },
            }
        },

        // ─────────────────────────────────────────
        // 3계층 엘리트
        // ─────────────────────────────────────────
        ["death_knight_elite"] = new EnemyData
        {
            id           = "death_knight_elite",
            displayName  = "죽음의 기사",
            hp           = 100,
            baseAttack   = 0,
            rewardGoldMin = 35,
            rewardGoldMax = 48,
            isElite      = true,
            isBoss       = false,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Defend, value = 15, description = "죽음의 갑옷" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 18, description = "혼 흡수" },
                new EnemyActionData { intent = EnemyIntent.Buff,   value = 5,  description = "죽음의 각성" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "혼 흡수" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 26, description = "파멸의 강타" },
            }
        },

        // ─────────────────────────────────────────
        // 3계층 보스
        // ─────────────────────────────────────────
        ["demon_lord_boss"] = new EnemyData
        {
            id           = "demon_lord_boss",
            displayName  = "마왕",
            hp           = 200,
            baseAttack   = 0,
            rewardGoldMin = 80,
            rewardGoldMax = 100,
            isElite      = false,
            isBoss       = true,
            pattern      = new[]
            {
                new EnemyActionData { intent = EnemyIntent.Buff,   value = 6,  description = "마왕의 위압" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 18, description = "지옥의 화염" },
                new EnemyActionData { intent = EnemyIntent.Burn,   value = 5,  description = "저주의 낙인" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 18, description = "지옥의 화염" },
                new EnemyActionData { intent = EnemyIntent.Poison, value = 5,  description = "역병의 숨결" },
                new EnemyActionData { intent = EnemyIntent.Defend, value = 20, description = "마왕의 방패" },
                new EnemyActionData { intent = EnemyIntent.Attack, value = 30, description = "멸망의 일격" },
            }
        },
    };

    public static EnemyData Get(string id)
    {
        if (table.TryGetValue(id, out EnemyData data)) return data;
        UnityEngine.Debug.LogError($"EnemyDatabase: '{id}' 없음");
        return null;
    }
}
