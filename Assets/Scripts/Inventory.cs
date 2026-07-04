using System.Collections.Generic;

public class Inventory
{
    private readonly Dictionary<string, int> foodCounts = new Dictionary<string, int>();

    public void AddFood(string id, int count = 1)
    {
        if (!foodCounts.ContainsKey(id)) foodCounts[id] = 0;
        foodCounts[id] += count;
    }

    public bool RemoveFood(string id)
    {
        if (!foodCounts.TryGetValue(id, out int count) || count <= 0) return false;
        count--;
        if (count <= 0) foodCounts.Remove(id);
        else foodCounts[id] = count;
        return true;
    }

    public int GetCount(string id) => foodCounts.TryGetValue(id, out int c) ? c : 0;

    public List<(string id, int count)> GetAll()
    {
        var list = new List<(string, int)>();
        foreach (var kv in foodCounts)
            list.Add((kv.Key, kv.Value));
        return list;
    }
}
