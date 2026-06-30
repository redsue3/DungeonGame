using System.Collections.Generic;

public class RelicInventory
{
    private readonly List<string> relicIds = new List<string>();

    public bool Has(string id)     => relicIds.Contains(id);
    public void Add(string id)     { if (!Has(id)) relicIds.Add(id); }
    public void Remove(string id)  => relicIds.Remove(id);
    public List<string> GetAll()   => new List<string>(relicIds);
    public int Count               => relicIds.Count;
}
