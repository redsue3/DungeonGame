using System.Collections.Generic;

// 모든 적 스탯 정의. EnemyFactory가 여기서 불러옴.
// 층마다 일반(잡몹) 5종 + 엘리트 1~2종 + 보스 1종.
// 일반/엘리트는 페이즈 1개(hpThreshold=1f)로 고정 패턴 하나만 반복하고,
// 보스는 페이즈 2~3개를 둬서 HP 구간(100%/66%/33%)마다 패턴이 바뀐다 (Enemy.CheckPhaseTransition 참고).
public static class EnemyDatabase
{
    private static readonly Dictionary<string, EnemyData> table = new Dictionary<string, EnemyData>
    {
        // ─────────────────────────────────────────
        // 1계층 일반 몬스터 5종 (전부 단일 페이즈)
        // ─────────────────────────────────────────
        ["slime"] = new EnemyData
        {
            id = "slime", displayName = "슬라임", hp = 28, baseAttack = 0,
            rewardGoldMin = 6, rewardGoldMax = 10, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 6, description = "돌진" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 6, description = "돌진" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 9, description = "강하게 돌진" },
                }}
            }
        },

        ["thief"] = new EnemyData
        {
            id = "thief", displayName = "도둑", hp = 22, baseAttack = 0,
            rewardGoldMin = 7, rewardGoldMax = 12, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 5,  description = "단도 찌르기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 5,  description = "단도 찌르기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 5,  description = "단도 찌르기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 11, description = "급소 공격" },
                }}
            }
        },

        ["bat_swarm"] = new EnemyData
        {
            id = "bat_swarm", displayName = "박쥐떼", hp = 18, baseAttack = 0,
            rewardGoldMin = 5, rewardGoldMax = 9, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 4, description = "물어뜯기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 4, description = "물어뜯기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 7, description = "연속 급습" },
                }}
            }
        },

        ["goblin_scout"] = new EnemyData
        {
            id = "goblin_scout", displayName = "고블린 정찰병", hp = 24, baseAttack = 0,
            rewardGoldMin = 6, rewardGoldMax = 10, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 5, description = "돌맹이 던지기" },
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 8, description = "몸 사리기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 9, description = "기습 찌르기" },
                }}
            }
        },

        ["skeleton_soldier"] = new EnemyData
        {
            id = "skeleton_soldier", displayName = "해골 병사", hp = 26, baseAttack = 0,
            rewardGoldMin = 7, rewardGoldMax = 11, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 6,  description = "녹슨 검 휘두르기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 6,  description = "녹슨 검 휘두르기" },
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 6,  description = "뼈 부딪히기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 12, description = "필사의 일격" },
                }}
            }
        },

        // ─────────────────────────────────────────
        // 1계층 엘리트 (단일 페이즈, 기존 패턴 유지)
        // ─────────────────────────────────────────
        ["goblin_elite"] = new EnemyData
        {
            id = "goblin_elite", displayName = "고블린 두목", hp = 55, baseAttack = 0,
            rewardGoldMin = 20, rewardGoldMax = 28, isElite = true, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 8,  description = "몽둥이 후려치기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 8,  description = "몽둥이 후려치기" },
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 10, description = "방어 자세" },
                    new EnemyActionData { intent = EnemyIntent.Buff,   value = 3,  description = "흥분 (공격력 UP)" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "분노의 일격" },
                }}
            }
        },

        ["skeleton_elite"] = new EnemyData
        {
            id = "skeleton_elite", displayName = "해골 기사", hp = 60, baseAttack = 0,
            rewardGoldMin = 22, rewardGoldMax = 30, isElite = true, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 12, description = "방패 올리기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 13, description = "창 찌르기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 9,  description = "창 휘두르기" },
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 8,  description = "방패 올리기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 18, description = "강습" },
                }}
            }
        },

        // ─────────────────────────────────────────
        // 1계층 보스 - 3페이즈 (100% / 66% / 33%)
        // ─────────────────────────────────────────
        ["giant_slime_boss"] = new EnemyData
        {
            id = "giant_slime_boss", displayName = "거대 슬라임", hp = 80, baseAttack = 0,
            rewardGoldMin = 35, rewardGoldMax = 45, isElite = false, isBoss = true,
            phases = new[]
            {
                new EnemyPhaseData
                {
                    hpThreshold = 1f, transitionMessage = "물컹거리는 거대한 몸이 다가온다.",
                    pattern = new[]
                    {
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 10, description = "몸통 박치기" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 10, description = "몸통 박치기" },
                        new EnemyActionData { intent = EnemyIntent.Defend, value = 14, description = "끈적끈적 굳기" },
                    }
                },
                new EnemyPhaseData
                {
                    hpThreshold = 0.66f, transitionMessage = "슬라임이 부글부글 끓어오르며 독을 뿜기 시작한다!",
                    pattern = new[]
                    {
                        new EnemyActionData { intent = EnemyIntent.Poison, value = 3,  description = "독액 분비" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 12, description = "몸통 박치기" },
                        new EnemyActionData { intent = EnemyIntent.Poison, value = 3,  description = "독액 분비" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 18, description = "대형 충격파" },
                    }
                },
                new EnemyPhaseData
                {
                    hpThreshold = 0.33f, transitionMessage = "핵이 드러난 슬라임이 폭주한다!",
                    pattern = new[]
                    {
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 20, description = "대형 충격파" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 20, description = "대형 충격파" },
                        new EnemyActionData { intent = EnemyIntent.Defend, value = 10, description = "끈적끈적 굳기" },
                        new EnemyActionData { intent = EnemyIntent.Poison, value = 5,  description = "독액 분비" },
                    }
                },
            }
        },

        // ─────────────────────────────────────────
        // 2계층 일반 몬스터 5종 (전부 단일 페이즈)
        // ─────────────────────────────────────────
        ["orc"] = new EnemyData
        {
            id = "orc", displayName = "오크 전사", hp = 45, baseAttack = 0,
            rewardGoldMin = 10, rewardGoldMax = 16, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 10, description = "도끼 찍기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 10, description = "도끼 찍기" },
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 8,  description = "방어 자세" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 15, description = "대도끼 강타" },
                }}
            }
        },

        ["poison_spider"] = new EnemyData
        {
            id = "poison_spider", displayName = "독 거미", hp = 35, baseAttack = 0,
            rewardGoldMin = 9, rewardGoldMax = 14, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Poison, value = 3, description = "독침 발사" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 7, description = "물기" },
                    new EnemyActionData { intent = EnemyIntent.Poison, value = 2, description = "독침 발사" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 7, description = "물기" },
                }}
            }
        },

        ["orc_grunt"] = new EnemyData
        {
            id = "orc_grunt", displayName = "오크 졸병", hp = 38, baseAttack = 0,
            rewardGoldMin = 9, rewardGoldMax = 14, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 8, description = "몽둥이 휘두르기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 8, description = "몽둥이 휘두르기" },
                    new EnemyActionData { intent = EnemyIntent.Buff,   value = 2, description = "짧은 함성" },
                }}
            }
        },

        ["swamp_viper"] = new EnemyData
        {
            id = "swamp_viper", displayName = "늪지 독사", hp = 32, baseAttack = 0,
            rewardGoldMin = 8, rewardGoldMax = 13, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Poison, value = 4, description = "독니 박기" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 7, description = "휘감기" },
                    new EnemyActionData { intent = EnemyIntent.Poison, value = 3, description = "독니 박기" },
                }}
            }
        },

        ["bandit_raider"] = new EnemyData
        {
            id = "bandit_raider", displayName = "도적 약탈자", hp = 40, baseAttack = 0,
            rewardGoldMin = 10, rewardGoldMax = 15, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 9,  description = "표창 던지기" },
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 10, description = "그림자 은신" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 13, description = "급소 베기" },
                }}
            }
        },

        // ─────────────────────────────────────────
        // 2계층 엘리트 (단일 페이즈, 기존 패턴 유지)
        // ─────────────────────────────────────────
        ["dark_mage_elite"] = new EnemyData
        {
            id = "dark_mage_elite", displayName = "흑마법사", hp = 75, baseAttack = 0,
            rewardGoldMin = 28, rewardGoldMax = 38, isElite = true, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Burn,   value = 3,  description = "화염 저주" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 12, description = "암흑 화살" },
                    new EnemyActionData { intent = EnemyIntent.Buff,   value = 4,  description = "마력 증폭" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 20, description = "암흑 폭발" },
                    new EnemyActionData { intent = EnemyIntent.Poison, value = 4,  description = "독 안개" },
                }}
            }
        },

        // ─────────────────────────────────────────
        // 2계층 보스 - 3페이즈 (100% / 66% / 33%)
        // ─────────────────────────────────────────
        ["orc_warchief_boss"] = new EnemyData
        {
            id = "orc_warchief_boss", displayName = "오크 족장", hp = 120, baseAttack = 0,
            rewardGoldMin = 50, rewardGoldMax = 65, isElite = false, isBoss = true,
            phases = new[]
            {
                new EnemyPhaseData
                {
                    hpThreshold = 1f, transitionMessage = "오크 족장이 전의를 불태운다.",
                    pattern = new[]
                    {
                        new EnemyActionData { intent = EnemyIntent.Buff,   value = 5,  description = "전쟁의 함성" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "도끼 폭풍" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "도끼 폭풍" },
                    }
                },
                new EnemyPhaseData
                {
                    hpThreshold = 0.66f, transitionMessage = "오크 족장의 눈이 핏빛으로 물든다!",
                    pattern = new[]
                    {
                        new EnemyActionData { intent = EnemyIntent.Defend, value = 18, description = "방어막 전개" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 16, description = "도끼 폭풍" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 22, description = "분노의 대도끼" },
                    }
                },
                new EnemyPhaseData
                {
                    hpThreshold = 0.33f, transitionMessage = "오크 족장이 광폭화한다!",
                    pattern = new[]
                    {
                        new EnemyActionData { intent = EnemyIntent.Buff,   value = 8,  description = "전쟁의 함성" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 24, description = "분노의 대도끼" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 24, description = "분노의 대도끼" },
                        new EnemyActionData { intent = EnemyIntent.Defend, value = 14, description = "방어막 전개" },
                    }
                },
            }
        },

        // ─────────────────────────────────────────
        // 3계층 일반 몬스터 5종 (전부 단일 페이즈)
        // ─────────────────────────────────────────
        ["cursed_warrior"] = new EnemyData
        {
            id = "cursed_warrior", displayName = "저주받은 전사", hp = 62, baseAttack = 0,
            rewardGoldMin = 14, rewardGoldMax = 20, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Buff,   value = 3,  description = "저주의 힘" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "저주받은 검" },
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 10, description = "어두운 방어" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 20, description = "파멸의 일격" },
                }}
            }
        },

        ["shadow_witch"] = new EnemyData
        {
            id = "shadow_witch", displayName = "그림자 마녀", hp = 48, baseAttack = 0,
            rewardGoldMin = 13, rewardGoldMax = 19, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Poison, value = 4,  description = "독 안개" },
                    new EnemyActionData { intent = EnemyIntent.Burn,   value = 3,  description = "저주의 화염" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 11, description = "어둠의 화살" },
                    new EnemyActionData { intent = EnemyIntent.Poison, value = 3,  description = "독 안개" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 15, description = "암흑 폭발" },
                }}
            }
        },

        ["bone_golem"] = new EnemyData
        {
            id = "bone_golem", displayName = "뼈 골렘", hp = 70, baseAttack = 0,
            rewardGoldMin = 15, rewardGoldMax = 22, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 13, description = "육중한 강타" },
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 16, description = "뼈 재조립" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 13, description = "육중한 강타" },
                }}
            }
        },

        ["wraith"] = new EnemyData
        {
            id = "wraith", displayName = "원혼", hp = 50, baseAttack = 0,
            rewardGoldMin = 13, rewardGoldMax = 19, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Burn,   value = 4,  description = "저주의 손길" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 10, description = "스산한 일격" },
                    new EnemyActionData { intent = EnemyIntent.Burn,   value = 3,  description = "저주의 손길" },
                }}
            }
        },

        ["fallen_knight"] = new EnemyData
        {
            id = "fallen_knight", displayName = "타락한 기사", hp = 58, baseAttack = 0,
            rewardGoldMin = 14, rewardGoldMax = 20, isElite = false, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "타락한 검격" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "타락한 검격" },
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 12, description = "어둠의 갑주" },
                }}
            }
        },

        // ─────────────────────────────────────────
        // 3계층 엘리트 (단일 페이즈, 기존 패턴 유지)
        // ─────────────────────────────────────────
        ["death_knight_elite"] = new EnemyData
        {
            id = "death_knight_elite", displayName = "죽음의 기사", hp = 100, baseAttack = 0,
            rewardGoldMin = 35, rewardGoldMax = 48, isElite = true, isBoss = false,
            phases = new[]
            {
                new EnemyPhaseData { hpThreshold = 1f, pattern = new[]
                {
                    new EnemyActionData { intent = EnemyIntent.Defend, value = 15, description = "죽음의 갑옷" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 18, description = "혼 흡수" },
                    new EnemyActionData { intent = EnemyIntent.Buff,   value = 5,  description = "죽음의 각성" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 14, description = "혼 흡수" },
                    new EnemyActionData { intent = EnemyIntent.Attack, value = 26, description = "파멸의 강타" },
                }}
            }
        },

        // ─────────────────────────────────────────
        // 3계층 보스 - 3페이즈 (100% / 66% / 33%)
        // ─────────────────────────────────────────
        ["demon_lord_boss"] = new EnemyData
        {
            id = "demon_lord_boss", displayName = "마왕", hp = 200, baseAttack = 0,
            rewardGoldMin = 80, rewardGoldMax = 100, isElite = false, isBoss = true,
            phases = new[]
            {
                new EnemyPhaseData
                {
                    hpThreshold = 1f, transitionMessage = "마왕이 위압적인 기세를 뿜어낸다.",
                    pattern = new[]
                    {
                        new EnemyActionData { intent = EnemyIntent.Buff,   value = 6,  description = "마왕의 위압" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 18, description = "지옥의 화염" },
                        new EnemyActionData { intent = EnemyIntent.Burn,   value = 5,  description = "저주의 낙인" },
                    }
                },
                new EnemyPhaseData
                {
                    hpThreshold = 0.66f, transitionMessage = "마왕이 역병의 기운을 불러낸다!",
                    pattern = new[]
                    {
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 18, description = "지옥의 화염" },
                        new EnemyActionData { intent = EnemyIntent.Poison, value = 5,  description = "역병의 숨결" },
                        new EnemyActionData { intent = EnemyIntent.Defend, value = 20, description = "마왕의 방패" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 20, description = "지옥의 화염" },
                    }
                },
                new EnemyPhaseData
                {
                    hpThreshold = 0.33f, transitionMessage = "마왕이 멸망을 고한다!",
                    pattern = new[]
                    {
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 30, description = "멸망의 일격" },
                        new EnemyActionData { intent = EnemyIntent.Burn,   value = 6,  description = "저주의 낙인" },
                        new EnemyActionData { intent = EnemyIntent.Attack, value = 32, description = "멸망의 일격" },
                        new EnemyActionData { intent = EnemyIntent.Buff,   value = 4,  description = "마왕의 위압" },
                    }
                },
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
