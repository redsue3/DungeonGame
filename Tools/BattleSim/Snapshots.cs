// 덱 표본 - Tools/BalanceSim(node)이 런을 600판씩 굴려 만든 '층 시작'·'보스 직전' 시점의 플레이어 상태(덱·레벨·유물·마나).
// 덱이 어떻게 자라는지(보상 카드·유물·레벨)는 BalanceSim이 맡고, 그 덱으로 싸우는 전투는 여기서 실제 코드로 돌린다.
// snapshots.json은 `node Tools/BalanceSim/balance-sim.js`를 돌릴 때마다 새로 쓰인다.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class PlayerSnapshot
{
    public int          level = 1;
    public int          xp;
    public int          mana;
    public List<string> relics = new List<string>();
    public List<JsonElement> deck = new List<JsonElement>(); // "strike" 또는 카드 데이터에 없는 카드(성소 카드 등)의 전체 수치
}

public class SnapshotFile
{
    public string   Path;
    public string   GeneratedAt;
    public string[] Checks;
    // 직업 → 체크포인트(0: 1층 시작, 1: 1층 보스 직전, 2: 2층 시작 ...) → 표본들
    public Dictionary<CharacterClass, List<List<PlayerSnapshot>>> ByClass = new Dictionary<CharacterClass, List<List<PlayerSnapshot>>>();

    public static int StartCheck(int layer) => (layer - 1) * 2;
    public static int BossCheck(int layer)  => (layer - 1) * 2 + 1;

    public static SnapshotFile Load(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;
        var file = new SnapshotFile { Path = path };
        if (root.TryGetProperty("generatedAt", out var g)) file.GeneratedAt = g.GetString();
        var checks = new List<string>();
        foreach (var c in root.GetProperty("checks").EnumerateArray()) checks.Add(c.GetString());
        file.Checks = checks.ToArray();

        foreach (var cls in root.GetProperty("classes").EnumerateObject())
        {
            if (!Enum.TryParse(cls.Name, out CharacterClass cc)) continue;
            var perCheck = new List<List<PlayerSnapshot>>();
            foreach (var list in cls.Value.EnumerateArray())
            {
                var snaps = new List<PlayerSnapshot>();
                foreach (var s in list.EnumerateArray())
                {
                    var snap = new PlayerSnapshot
                    {
                        level = s.GetProperty("level").GetInt32(),
                        xp    = s.GetProperty("xp").GetInt32(),
                        mana  = s.GetProperty("mana").GetInt32(),
                    };
                    foreach (var r in s.GetProperty("relics").EnumerateArray()) snap.relics.Add(r.GetString());
                    foreach (var c in s.GetProperty("deck").EnumerateArray()) snap.deck.Add(c.Clone());
                    snaps.Add(snap);
                }
                perCheck.Add(snaps);
            }
            file.ByClass[cc] = perCheck;
        }
        return file;
    }

    // 지정한 경로 → 현재 폴더부터 위로 올라가며 Tools/BattleSim/snapshots.json → 실행 파일 위치부터 위로
    public static string Find(string explicitPath)
    {
        if (!string.IsNullOrEmpty(explicitPath)) return File.Exists(explicitPath) ? explicitPath : null;
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string p1 = System.IO.Path.Combine(dir.FullName, "snapshots.json");
                string p2 = System.IO.Path.Combine(dir.FullName, "Tools", "BattleSim", "snapshots.json");
                if (File.Exists(p2)) return p2;
                if (File.Exists(p1) && dir.Name == "BattleSim") return p1;
            }
        }
        return null;
    }

    // 표본으로 플레이어를 만든다. 세이브 불러오기와 같은 순서: 유물 상시 효과 → 레벨 복원 → 마나. 체력·코스트는 가득.
    public static PlayerCharacter BuildPlayer(CharacterClass cls, PlayerSnapshot snap)
    {
        var p = new PlayerCharacter(cls);
        if (snap == null)
        {
            foreach (string id in PlayerDatabase.Get(cls).starterDeckCardIds) p.deck.AddCard(CardDatabase.Create(id));
        }
        else
        {
            foreach (var entry in snap.deck)
            {
                Card card = MakeCard(entry);
                if (card != null) p.deck.AddCard(card);
            }
            foreach (string id in snap.relics)
            {
                if (RelicDatabase.Get(id) == null) continue;
                p.relics.Add(id);
                RelicDatabase.ApplyPassiveStats(id, p);
            }
            p.RestoreLevel(snap.level, snap.xp);
            p.manaStack = snap.mana;
        }
        p.currentHp   = p.maxHp;
        p.currentCost = p.maxCost;
        return p;
    }

    private static Card MakeCard(JsonElement e)
    {
        if (e.ValueKind == JsonValueKind.String) return CardDatabase.Create(e.GetString());

        string id = Str(e, "id");
        int I(string k) => e.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;
        bool B(string k) => e.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.True;

        // 카드 데이터에 있는 카드인데 수치만 다른 경우(성장형 카드가 자란 상태 등)
        if (CardDatabase.Exists(id))
        {
            Card c = CardDatabase.Create(id);
            if (e.TryGetProperty("dmg", out _)) c.damage = I("dmg");
            c.RebuildDescription();
            return c;
        }

        // 성소 카드처럼 그때그때 만들어지는 카드
        CardType type = Enum.TryParse(Str(e, "type"), out CardType t) ? t : CardType.Skill;
        int range = I("range");
        return new Card(id, Str(e, "name") ?? id, I("cost"), type,
            dmg: I("dmg"), blk: I("blk"), draw: I("draw"), heal: I("heal"), str: I("str"),
            poison: I("poison"), burn: I("burn"), selfDmg: I("selfDmg"), aoe: B("aoe"),
            growOnUse: I("grow"), buffNextAttack: I("buffAtk"), buffNextDefense: I("buffDef"), range: range > 0 ? range : 1);
    }

    private static string Str(JsonElement e, string k) => e.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}
