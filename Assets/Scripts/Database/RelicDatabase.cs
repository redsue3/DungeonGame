using System.Collections.Generic;
using UnityEngine;

public static class RelicDatabase
{
    private static readonly Dictionary<string, RelicData> table = new Dictionary<string, RelicData>
    {
        ["iron_heart"] = new RelicData
        {
            id = "iron_heart", displayName = "철심", rarity = RelicRarity.Common,
            description = "최대 HP +15",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.Passive, effectType = RelicEffectType.BonusMaxHp, value = 15 } }
        },
        ["battle_torc"] = new RelicData
        {
            id = "battle_torc", displayName = "전투 목걸이", rarity = RelicRarity.Common,
            description = "전투 시작 시 힘 +1",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.OnBattleStart, effectType = RelicEffectType.GainStrength, value = 1 } }
        },
        ["defenders_sigil"] = new RelicData
        {
            id = "defenders_sigil", displayName = "수호자의 인장", rarity = RelicRarity.Common,
            description = "전투 시작 시 방어막 +8",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.OnBattleStart, effectType = RelicEffectType.GainBlock, value = 8 } }
        },
        ["serpent_fang"] = new RelicData
        {
            id = "serpent_fang", displayName = "뱀 어금니", rarity = RelicRarity.Common,
            description = "최대 패 수 +1",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.Passive, effectType = RelicEffectType.BonusHandSize, value = 1 } }
        },
        ["war_horn"] = new RelicData
        {
            id = "war_horn", displayName = "전쟁의 뿔피리", rarity = RelicRarity.Uncommon,
            description = "매 턴 시작 시 코스트 +1",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.OnTurnStart, effectType = RelicEffectType.GainCost, value = 1 } }
        },
        ["blood_ring"] = new RelicData
        {
            id = "blood_ring", displayName = "혈약 반지", rarity = RelicRarity.Uncommon,
            description = "적 처치 시 HP 3 회복",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.OnKill, effectType = RelicEffectType.HealHp, value = 3 } }
        },
        ["ancient_tome"] = new RelicData
        {
            id = "ancient_tome", displayName = "고대 서적", rarity = RelicRarity.Rare,
            description = "매 턴 시작 시 카드 1장 추가 드로우",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.OnTurnStart, effectType = RelicEffectType.DrawCard, value = 1 } }
        },
        ["golden_idol"] = new RelicData
        {
            id = "golden_idol", displayName = "황금 우상", rarity = RelicRarity.Rare,
            description = "골드 수입 +25%",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.Passive, effectType = RelicEffectType.BonusGoldPct, value = 25 } }
        },
        ["phoenix_ash"] = new RelicData
        {
            id = "phoenix_ash", displayName = "불사조의 재", rarity = RelicRarity.Rare,
            description = "HP 30% 이하 시 방어막 +10 (전투당 1회)",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.OnHpBelow30, effectType = RelicEffectType.GainBlock, value = 10 } }
        },
        ["demon_crown"] = new RelicData
        {
            id = "demon_crown", displayName = "마왕의 왕관", rarity = RelicRarity.Boss,
            description = "최대 코스트 +1, 전투 시작 시 힘 +2",
            effects = new[]
            {
                new RelicEffect { trigger = RelicTrigger.Passive,       effectType = RelicEffectType.BonusMaxCost,  value = 1 },
                new RelicEffect { trigger = RelicTrigger.OnBattleStart, effectType = RelicEffectType.GainStrength,  value = 2 },
            }
        },
        ["purifying_charm"] = new RelicData
        {
            id = "purifying_charm", displayName = "정화의 부적", rarity = RelicRarity.Uncommon,
            description = "획득 시 덱에서 무작위 카드 1장 제거",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.OnAcquire, effectType = RelicEffectType.RemoveRandomCard, value = 1 } }
        },
        ["hawk_eye"] = new RelicData
        {
            id = "hawk_eye", displayName = "매의 눈", rarity = RelicRarity.Uncommon,
            description = "전투 시작 시 카드 2장 추가 드로우",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.OnBattleStart, effectType = RelicEffectType.DrawCard, value = 2 } }
        },
        ["chaos_prism"] = new RelicData
        {
            id = "chaos_prism", displayName = "혼돈의 프리즘", rarity = RelicRarity.Rare,
            description = "획득 시 덱의 무작위 카드 1장을 다른 카드로 변환",
            effects = new[] { new RelicEffect { trigger = RelicTrigger.OnAcquire, effectType = RelicEffectType.TransformRandomCard, value = 1 } }
        },
    };

    public static RelicData Get(string id)
    {
        if (table.TryGetValue(id, out RelicData data)) return data;
        Debug.LogError($"RelicDatabase: '{id}' 없음");
        return null;
    }

    public static List<RelicData> GetAll() => new List<RelicData>(table.Values);

    // 유물 획득 시점에 호출 - 상시 스탯(Passive) 적용 + 획득 시 1회(OnAcquire) 효과 실행
    public static void ApplyAcquisitionEffects(string relicId, PlayerCharacter player)
    {
        ApplyPassiveStats(relicId, player);

        RelicData data = Get(relicId);
        if (data == null) return;
        foreach (RelicEffect eff in data.effects)
        {
            if (eff.trigger != RelicTrigger.OnAcquire) continue;
            switch (eff.effectType)
            {
                case RelicEffectType.RemoveRandomCard:    RemoveRandomCardFromDeck(player); break;
                case RelicEffectType.TransformRandomCard: TransformRandomCardInDeck(player); break;
            }
        }
    }

    // 상시 스탯형 Passive만 적용 - 세이브 로드 복원 시에도 호출된다 (OnAcquire 1회성 효과는 여기서 재실행 금지)
    public static void ApplyPassiveStats(string relicId, PlayerCharacter player)
    {
        RelicData data = Get(relicId);
        if (data == null) return;
        foreach (RelicEffect eff in data.effects)
        {
            if (eff.trigger != RelicTrigger.Passive) continue;
            switch (eff.effectType)
            {
                case RelicEffectType.BonusMaxHp:
                    player.maxHp     += eff.value;
                    player.currentHp += eff.value;
                    break;
                case RelicEffectType.BonusMaxCost:
                    player.maxCost += eff.value;
                    break;
                case RelicEffectType.BonusHandSize:
                    player.startHandSize += eff.value;
                    break;
                case RelicEffectType.BonusAttack:
                    player.attackBonus += eff.value;
                    break;
                // BonusGoldPct: 골드 계산 시 동적으로 적용 (여기선 처리 안 함)
            }
        }
    }

    // 골드 보너스 배율 계산 (golden_idol 등)
    public static int ApplyGoldBonus(int baseGold, PlayerCharacter player)
    {
        float multiplier = 1f;
        foreach (string id in player.relics.GetAll())
        {
            RelicData data = Get(id);
            if (data == null) continue;
            foreach (RelicEffect eff in data.effects)
            {
                if (eff.trigger == RelicTrigger.Passive && eff.effectType == RelicEffectType.BonusGoldPct)
                    multiplier += eff.value / 100f;
            }
        }
        return UnityEngine.Mathf.RoundToInt(baseGold * multiplier);
    }

    private static void RemoveRandomCardFromDeck(PlayerCharacter player)
    {
        List<Card> all = player.deck.GetAllCards();
        if (all.Count == 0) return;
        Card target = all[UnityEngine.Random.Range(0, all.Count)];
        player.deck.RemoveCard(target);
        Debug.Log($"[유물] 덱에서 제거: {target.cardName}");
    }

    private static void TransformRandomCardInDeck(PlayerCharacter player)
    {
        List<Card> all = player.deck.GetAllCards();
        if (all.Count == 0) return;
        Card target = all[UnityEngine.Random.Range(0, all.Count)];

        int layer = DungeonManager.Instance != null ? DungeonManager.Instance.CurrentLayer : 1;
        List<string> rolled = LootTable.RollCardRewards(layer, 1);
        if (rolled.Count == 0) return;
        Card replacement = CardDatabase.Create(rolled[0]);
        if (replacement == null) return;

        player.deck.RemoveCard(target);
        player.deck.AddCard(replacement);
        Debug.Log($"[유물] 카드 변환: {target.cardName} → {replacement.cardName}");
    }
}
