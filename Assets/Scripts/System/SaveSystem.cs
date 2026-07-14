using System.IO;
using UnityEngine;

// 주의: SaveData/CardSnapshot의 필드명이 그대로 save.json의 키가 된다 (JsonUtility).
// 코드가 mana → cost로 리네이밍된 뒤에도 예전 세이브를 읽을 수 있도록 JSON 키는 옛 이름을 유지한다.
[System.Serializable]
public class SaveData
{
    public CharacterClass characterClass;
    public int   currentHp;
    public int   gold;
    public int   currentLayer;
    public int   poisonStack;
    public int   burnStack;
    public int   strengthStack;
    public int   hunger;
    public int   maxHunger;
    public int   currentMana = -1; // 현재 코스트(currentCost). -1 = 필드가 없던 구버전 세이브 표시 → 로드 시 최대 코스트로 복원
    public CardSnapshot[] deckCards;
    public string[] relicIds;
    public string[] foodIds;
    public int[]    foodCounts;
}

// 카드 전체 스탯 스냅샷 - 성소에서 만들어진 카드처럼 CardDatabase에 없는(그때그때 생성된)
// 카드도 세이브/로드 시 사라지지 않고 그대로 복원되도록 모든 필드를 담아 저장한다.
[System.Serializable]
public class CardSnapshot
{
    public string   id;
    public string   cardName;
    public int      manaCost; // 카드 코스트(cost) - JSON 키 하위호환 때문에 옛 이름 유지
    public CardType cardType;
    public bool     isAoe;
    public int      damage;
    public int      block;
    public int      drawCount;
    public int      healAmount;
    public int      strengthGain;
    public int      poisonApply;
    public int      burnApply;
    public int      selfDamage;
    public int      growOnUse;
    public int      buffNextAttack;
    public int      buffNextDefense;
}

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(PlayerCharacter player, int layer)
    {
        var data = new SaveData
        {
            characterClass = player.characterClass,
            currentHp      = player.currentHp,
            gold           = player.gold,
            currentLayer   = layer,
            poisonStack    = player.poisonStack,
            burnStack      = player.burnStack,
            strengthStack  = player.strengthStack,
            hunger         = player.hunger,
            maxHunger      = player.maxHunger,
            currentMana    = player.currentCost,
            deckCards      = ExtractDeckSnapshots(player.deck),
            relicIds       = player.relics.GetAll().ToArray(),
            foodIds        = ExtractFoodIds(player.inventory),
            foodCounts     = ExtractFoodCounts(player.inventory),
        };

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, prettyPrint: true));
        Debug.Log($"[SaveSystem] 저장 완료");
    }

    public static SaveData Load()
    {
        if (!HasSave()) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
    }

    public static void Delete()
    {
        if (HasSave()) File.Delete(SavePath);
    }

    public static bool HasSave() => File.Exists(SavePath);

    // 세이브 데이터로 플레이어 복원
    public static PlayerCharacter RestorePlayer(SaveData data)
    {
        // PlayerFactory 대신 직접 생성 - 스타터 덱/시작 식료품은 어차피 세이브 내용으로 대체된다
        var player = new PlayerCharacter(data.characterClass);

        // 유물부터 복원 - 상시 스탯(Passive)을 다시 적용해야 최대 HP/최대 코스트/패 수가 저장 당시와 같아진다.
        // 획득 시 1회짜리 OnAcquire 효과(정화의 부적/혼돈의 프리즘의 덱 조작)는 재실행하면 안 되므로 호출하지 않는다.
        if (data.relicIds != null)
        {
            foreach (string relicId in data.relicIds)
            {
                player.relics.Add(relicId);
                RelicDatabase.ApplyPassiveStats(relicId, player);
            }
        }

        player.currentHp     = Mathf.Min(data.currentHp, player.maxHp);
        player.gold          = data.gold;
        player.poisonStack   = data.poisonStack;
        player.burnStack     = data.burnStack;
        player.strengthStack = data.strengthStack;
        player.hunger        = data.maxHunger > 0 ? data.hunger    : HungerSystem.MaxHunger;
        player.maxHunger     = data.maxHunger > 0 ? data.maxHunger : HungerSystem.MaxHunger;
        player.currentCost   = data.currentMana >= 0 ? Mathf.Min(data.currentMana, player.maxCost) : player.maxCost;

        // 덱 복원 - CardDatabase에 등록된 카드는 최신 밸런스로, 성소 카드처럼
        // 등록되지 않은 카드는 저장된 스냅샷 그대로 복원한다.
        if (data.deckCards != null)
        {
            foreach (CardSnapshot snap in data.deckCards)
            {
                if (snap == null) continue;
                Card card = CardDatabase.Exists(snap.id) ? CardDatabase.Create(snap.id) : CardFromSnapshot(snap);
                if (card != null) player.deck.AddCard(card);
            }
        }

        // 인벤토리(식료품) 복원
        if (data.foodIds != null)
        {
            for (int i = 0; i < data.foodIds.Length; i++)
            {
                int count = (data.foodCounts != null && i < data.foodCounts.Length) ? data.foodCounts[i] : 1;
                player.inventory.AddFood(data.foodIds[i], count);
            }
        }

        return player;
    }

    private static CardSnapshot[] ExtractDeckSnapshots(Deck deck)
    {
        var cards = deck.GetAllCards();
        var snaps = new CardSnapshot[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            Card c = cards[i];
            snaps[i] = new CardSnapshot
            {
                id = c.id, cardName = c.cardName, manaCost = c.cost, cardType = c.cardType, isAoe = c.isAoe,
                damage = c.damage, block = c.block, drawCount = c.drawCount, healAmount = c.healAmount,
                strengthGain = c.strengthGain, poisonApply = c.poisonApply, burnApply = c.burnApply, selfDamage = c.selfDamage,
                growOnUse = c.growOnUse, buffNextAttack = c.buffNextAttack, buffNextDefense = c.buffNextDefense,
            };
        }
        return snaps;
    }

    private static Card CardFromSnapshot(CardSnapshot s) => new Card(
        s.id, s.cardName, s.manaCost, s.cardType,
        dmg: s.damage, blk: s.block, draw: s.drawCount, heal: s.healAmount,
        str: s.strengthGain, poison: s.poisonApply, burn: s.burnApply, selfDmg: s.selfDamage, aoe: s.isAoe,
        growOnUse: s.growOnUse, buffNextAttack: s.buffNextAttack, buffNextDefense: s.buffNextDefense);

    private static string[] ExtractFoodIds(Inventory inventory)
    {
        var all = inventory.GetAll();
        var ids = new string[all.Count];
        for (int i = 0; i < all.Count; i++)
            ids[i] = all[i].id;
        return ids;
    }

    private static int[] ExtractFoodCounts(Inventory inventory)
    {
        var all    = inventory.GetAll();
        var counts = new int[all.Count];
        for (int i = 0; i < all.Count; i++)
            counts[i] = all[i].count;
        return counts;
    }
}
