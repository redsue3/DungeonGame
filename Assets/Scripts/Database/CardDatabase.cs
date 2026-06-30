using System.Collections.Generic;
using UnityEngine;

public static class CardDatabase
{
    private static readonly Dictionary<string, Card> table = new Dictionary<string, Card>
    {
        // 전사 스타터
        ["strike"]        = new Card("strike",        "전사의 일격",   1, CardType.Attack,  dmg:  6),
        ["defend"]        = new Card("defend",        "방어",         1, CardType.Defense, blk:  5),
        ["heavy_strike"]  = new Card("heavy_strike",  "분노의 일격",   2, CardType.Attack,  dmg: 14),

        // 도적 스타터
        ["dagger"]        = new Card("dagger",        "단도",         1, CardType.Attack,  dmg: 4),
        ["evade"]         = new Card("evade",         "회피",         1, CardType.Defense, blk: 4),
        ["combo"]         = new Card("combo",         "연격",         2, CardType.Attack,  dmg: 5, draw: 1),

        // 마법사 스타터
        ["fireball"]      = new Card("fireball",      "화염구",       1, CardType.Attack,  dmg:  7),
        ["mana_shield"]   = new Card("mana_shield",   "마력 방벽",    1, CardType.Defense, blk:  4),
        ["lightning"]     = new Card("lightning",     "번개",         2, CardType.Attack,  dmg: 18),
        ["draw_spell"]    = new Card("draw_spell",    "드로우 마법",  1, CardType.Skill,   draw: 2),

        // 성기사 스타터
        ["holy_strike"]   = new Card("holy_strike",   "성스러운 일격", 1, CardType.Attack,  dmg: 5),
        ["shield_bash"]   = new Card("shield_bash",   "방패 강타",    1, CardType.Defense, blk: 6),
        ["sacred_heal"]   = new Card("sacred_heal",   "신성한 치유",  2, CardType.Skill,   heal: 8),
        ["judgement"]     = new Card("judgement",     "심판",         2, CardType.Attack,  dmg: 14),

        // 1계층 드롭
        ["double_strike"] = new Card("double_strike", "연속 강타",    2, CardType.Attack,  dmg: 10),
        ["iron_defense"]  = new Card("iron_defense",  "철벽 방어",    2, CardType.Defense, blk: 12),
        ["power_up"]      = new Card("power_up",      "힘 강화",      1, CardType.Skill,   str: 2),
        ["quick_draw"]    = new Card("quick_draw",    "신속 드로우",  1, CardType.Skill,   draw: 3),
        ["acid_slime"]    = new Card("acid_slime",    "산성 슬라임",  1, CardType.Attack,  dmg: 3, poison: 2),
        ["holy_blade"]    = new Card("holy_blade",    "성검",         2, CardType.Attack,  dmg: 10, heal: 4),

        // 2계층 드롭
        ["whirlwind"]     = new Card("whirlwind",     "회오리 베기",  2, CardType.Attack,  dmg: 8,  aoe: true),  // AoE
        ["poison_blade"]  = new Card("poison_blade",  "독 단검",      1, CardType.Attack,  dmg: 4, poison: 3),
        ["flame_burst"]   = new Card("flame_burst",   "화염 폭발",    2, CardType.Attack,  dmg: 10, burn: 2, aoe: true), // AoE
        ["mana_surge"]    = new Card("mana_surge",    "마나 폭주",    1, CardType.Skill,   draw: 2),
        ["fortify"]       = new Card("fortify",       "요새화",       2, CardType.Defense, blk: 14),
        ["dark_pact"]     = new Card("dark_pact",     "어둠의 계약",  1, CardType.Skill,   draw: 3, selfDmg: 3),
        ["divine_shield"] = new Card("divine_shield", "신성한 방패",  2, CardType.Defense, blk: 10, heal: 3),

        // 3계층 드롭
        ["soul_slash"]    = new Card("soul_slash",    "영혼 베기",    2, CardType.Attack,  dmg: 18),
        ["perfect_guard"] = new Card("perfect_guard", "완벽한 가드",  2, CardType.Defense, blk: 18),
        ["plague_strike"] = new Card("plague_strike", "역병 일격",    1, CardType.Attack,  dmg: 5, poison: 5),
        ["soul_drain"]    = new Card("soul_drain",    "영혼 흡수",    2, CardType.Attack,  dmg: 10, heal: 6),
        ["chaos_flame"]   = new Card("chaos_flame",   "혼돈 화염",    0, CardType.Attack,  dmg: 8, burn: 3, selfDmg: 4),
        ["thunder_storm"] = new Card("thunder_storm", "뇌우",         3, CardType.Attack,  dmg: 12, aoe: true),
        ["blessing"]      = new Card("blessing",      "축복",         1, CardType.Skill,   draw: 2, str: 1),
    };

    public static Card Create(string id)
    {
        if (table.TryGetValue(id, out Card card)) return card.Clone();
        Debug.LogError($"CardDatabase: '{id}' 없음");
        return null;
    }

    public static bool Exists(string id) => table.ContainsKey(id);
}
