using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public CharacterClass characterClass;
    public int   currentHp;
    public int   maxHp;
    public int   gold;
    public int   currentLayer;
    public int   currentStage;
    public int   poisonStack;
    public int   burnStack;
    public int   strengthStack;
    public int   dexterityStack;
    public int   hunger;
    public int   maxHunger;
    public string[] deckCardIds;
    public string[] relicIds;
    public string[] foodIds;
    public int[]    foodCounts;
}

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(PlayerCharacter player, int layer, int stage = 0)
    {
        var data = new SaveData
        {
            characterClass = player.characterClass,
            currentHp      = player.currentHp,
            maxHp          = player.maxHp,
            gold           = player.gold,
            currentLayer   = layer,
            currentStage   = stage,
            poisonStack    = player.poisonStack,
            burnStack      = player.burnStack,
            strengthStack  = player.strengthStack,
            dexterityStack = player.dexterityStack,
            hunger         = player.hunger,
            maxHunger      = player.maxHunger,
            deckCardIds    = ExtractDeckIds(player.deck),
            relicIds       = player.relics.GetAll().ToArray(),
            foodIds        = ExtractFoodIds(player.inventory),
            foodCounts     = ExtractFoodCounts(player.inventory),
        };

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, prettyPrint: true));
        Debug.Log($"[SaveSystem] 저장 완료");
    }

    // LayerManager 기반 흐름에서 호출할 때 쓰는 오버로드
    public static void Save(PlayerCharacter player, LayerManager layer)
        => Save(player, layer.currentLayer, layer.currentStage);

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
        var player = PlayerFactory.Create(data.characterClass);

        player.currentHp     = data.currentHp;
        player.gold          = data.gold;
        player.poisonStack   = data.poisonStack;
        player.burnStack     = data.burnStack;
        player.strengthStack = data.strengthStack;
        player.dexterityStack= data.dexterityStack;
        player.hunger        = data.maxHunger > 0 ? data.hunger    : HungerSystem.MaxHunger;
        player.maxHunger      = data.maxHunger > 0 ? data.maxHunger : HungerSystem.MaxHunger;

        // 덱 복원
        player.deck = new Deck();
        foreach (string cardId in data.deckCardIds)
        {
            var card = CardDatabase.Create(cardId);
            if (card != null) player.deck.AddCard(card);
        }

        // 유물 복원
        foreach (string relicId in data.relicIds)
            player.relics.Add(relicId);

        // 인벤토리(식료품) 복원
        player.inventory = new Inventory();
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

    private static string[] ExtractDeckIds(Deck deck)
    {
        var cards = deck.GetAllCards();
        var ids   = new string[cards.Count];
        for (int i = 0; i < cards.Count; i++)
            ids[i] = cards[i].id;
        return ids;
    }

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
