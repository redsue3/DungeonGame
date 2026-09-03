using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 몬스터 하나의 도감 설정 (읽기 전용 텍스트 데이터, 게임플레이 로직 없음).
public class MonsterLore
{
    public string id;
    public string displayName;
    public int    layer;
    public string category;    // 자연발생 / 침략 세력 / 실패한 모험가 / 곳간의 구조물·관문·파수꾼·주인
    public string grade;       // 일반 / 엘리트 / 보스
    public string description;
}

// Assets/StreamingAssets/bestiary.md를 파싱해서 몬스터 id로 조회 가능하게 만든다.
// id는 EnemyDatabase.cs 딕셔너리 키와 1:1 대응 (bestiary.md/worldbook.md 설계 그대로).
// bestiary.md는 devlog.md/worldbook.md와 달리 런타임에 실제로 읽어야 해서 Assets 밖(repo 루트)에
// 둘 수 없다 - Unity는 StreamingAssets 폴더만 빌드에 원본 그대로 포함한다.
public static class MonsterLoreDatabase
{
    private const string FileName = "bestiary.md";

    private static Dictionary<string, MonsterLore> table;

    public static MonsterLore Get(string id)
    {
        EnsureLoaded();
        if (table.TryGetValue(id, out MonsterLore lore)) return lore;
        Debug.LogError($"MonsterLoreDatabase: '{id}' 없음");
        return null;
    }

    public static bool TryGet(string id, out MonsterLore lore)
    {
        EnsureLoaded();
        return table.TryGetValue(id, out lore);
    }

    private static void EnsureLoaded()
    {
        if (table != null) return;
        table = new Dictionary<string, MonsterLore>();

        string path = Path.Combine(Application.streamingAssetsPath, FileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"MonsterLoreDatabase: 파일 없음 - {path}");
            return;
        }

        Parse(File.ReadAllText(path), table);
    }

    // bestiary.md 형식 고정 파싱 (worldbook.md에서 설명하는 그 형식):
    // ### 표시이름 (id)
    // - 계층: N
    // - 분류: X
    // - 등급: Y
    // - 설정: 텍스트
    private static void Parse(string text, Dictionary<string, MonsterLore> into)
    {
        MonsterLore current = null;
        foreach (string rawLine in text.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r').Trim();

            if (line.StartsWith("### "))
            {
                current = ParseHeader(line);
                if (current != null) into[current.id] = current;
                continue;
            }
            if (current == null) continue;

            if (TryParseField(line, "계층", out string layerStr) && int.TryParse(layerStr, out int layer))
                current.layer = layer;
            else if (TryParseField(line, "분류", out string category))
                current.category = category;
            else if (TryParseField(line, "등급", out string grade))
                current.grade = grade;
            else if (TryParseField(line, "설정", out string description))
                current.description = description;
        }
    }

    // "### 슬라임 (slime)" → displayName="슬라임", id="slime"
    private static MonsterLore ParseHeader(string line)
    {
        string content = line.Substring(4).Trim(); // "### " 제거
        int open  = content.LastIndexOf('(');
        int close = content.LastIndexOf(')');
        if (open < 0 || close <= open)
        {
            Debug.LogWarning($"MonsterLoreDatabase: 헤더 형식 오류 - \"{line}\"");
            return null;
        }

        return new MonsterLore
        {
            displayName = content.Substring(0, open).Trim(),
            id          = content.Substring(open + 1, close - open - 1).Trim(),
        };
    }

    private static bool TryParseField(string line, string key, out string value)
    {
        string prefix = $"- {key}:";
        if (line.StartsWith(prefix))
        {
            value = line.Substring(prefix.Length).Trim();
            return true;
        }
        value = null;
        return false;
    }
}
