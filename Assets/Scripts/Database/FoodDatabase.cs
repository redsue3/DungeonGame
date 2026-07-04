using System.Collections.Generic;
using UnityEngine;

public static class FoodDatabase
{
    private static readonly Dictionary<string, FoodData> table = new Dictionary<string, FoodData>
    {
        ["apple"]      = new FoodData { id = "apple",      displayName = "사과",       description = "배고픔 +12",           hungerRestore = 12,  price = 8  },
        ["bread"]      = new FoodData { id = "bread",      displayName = "빵",         description = "배고픔 +20",           hungerRestore = 20,  price = 15 },
        ["cheese"]     = new FoodData { id = "cheese",     displayName = "치즈",       description = "배고픔 +18",           hungerRestore = 18,  price = 14 },
        ["dried_meat"] = new FoodData { id = "dried_meat", displayName = "육포",       description = "배고픔 +35",           hungerRestore = 35,  price = 28 },
        ["ration"]     = new FoodData { id = "ration",     displayName = "비상 식량",   description = "배고픔 +50",           hungerRestore = 50,  price = 40 },
        ["feast"]      = new FoodData { id = "feast",      displayName = "진수성찬",   description = "배고픔 +100 (완전 회복)", hungerRestore = 100, price = 70 },
    };

    public static FoodData Get(string id) =>
        table.TryGetValue(id, out FoodData data) ? data : null;

    public static bool Exists(string id) => table.ContainsKey(id);

    public static List<FoodData> GetAll() => new List<FoodData>(table.Values);
}
