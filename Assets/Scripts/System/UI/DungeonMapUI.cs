using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonMapUI : MonoBehaviour
{
    [Header("맵 그리드")]
    [SerializeField] private GameObject tilePrefab;   // Button + Image + Text 구성
    [SerializeField] private Transform  gridParent;   // 자유 배치 컨테이너 (레이아웃 그룹 없음)
    [SerializeField] private TextMeshProUGUI layerText;

    [Header("플레이어 상태 (상단)")]
    [SerializeField] private TextMeshProUGUI playerHpText;
    [SerializeField] private TextMeshProUGUI playerGoldText;
    [SerializeField] private TextMeshProUGUI playerRelicsText;
    [SerializeField] private TextMeshProUGUI playerHungerText;

    [Header("인벤토리")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Button      inventoryBtn;

    private const float NodeSize = 64f;
    private const float MarginX  = 70f;
    private const float MarginY  = 70f;

    private readonly List<GameObject> tileObjects = new List<GameObject>();
    private readonly List<GameObject> lineObjects = new List<GameObject>();

    // 타일 타입별 아이콘 문자 (폰트 아이콘 없이 텍스트로 표현)
    private static readonly Dictionary<TileType, string> tileIcon = new Dictionary<TileType, string>
    {
        [TileType.NormalEnemy] = "⚔",
        [TileType.GroupEnemy]  = "⚔⚔",
        [TileType.EliteEnemy]  = "☠",
        [TileType.Boss]        = "👑",
        [TileType.Rest]        = "🔥",
        [TileType.Shop]        = "🛒",
        [TileType.Shrine]      = "⛩",
    };

    private static readonly Dictionary<TileType, Color> tileColor = new Dictionary<TileType, Color>
    {
        [TileType.NormalEnemy] = new Color(0.9f, 0.4f, 0.4f),
        [TileType.GroupEnemy]  = new Color(0.85f, 0.3f, 0.3f),
        [TileType.EliteEnemy]  = new Color(0.7f, 0.2f, 0.8f),
        [TileType.Boss]        = new Color(1f, 0.2f, 0.2f),
        [TileType.Rest]        = new Color(0.3f, 0.75f, 0.4f),
        [TileType.Shop]        = new Color(1f, 0.8f, 0.2f),
        [TileType.Shrine]      = new Color(0.6f, 0.5f, 1f),
    };

    void OnEnable()
    {
        inventoryBtn?.onClick.AddListener(() => inventoryUI?.Open());
        Refresh();
    }

    void OnDisable()
    {
        inventoryBtn?.onClick.RemoveAllListeners();
    }

    public void Refresh()
    {
        DungeonMap map = DungeonManager.Instance?.CurrentMap;
        if (map == null) return;

        layerText.text      = $"{map.Layer}계층";
        UpdatePlayerStatus();
        RebuildMap(map);
    }

    private void UpdatePlayerStatus()
    {
        var p = DungeonManager.Instance?.Player;
        if (p == null) return;

        playerHpText.text    = $"HP  {p.currentHp} / {p.maxHp}";
        playerGoldText.text  = $"골드  {p.gold}";
        if (playerHungerText != null)
            playerHungerText.text = $"배고픔  {p.hunger} / {p.maxHunger}" + (p.IsStarving ? " ⚠" : "");

        var relicNames = new System.Text.StringBuilder();
        foreach (string id in p.relics.GetAll())
        {
            RelicData r = RelicDatabase.Get(id);
            if (r != null) relicNames.Append($"[{r.displayName}] ");
        }
        playerRelicsText.text = relicNames.Length > 0 ? relicNames.ToString().TrimEnd() : "유물 없음";
    }

    // 층(floor)/가로위치(x)로 배치된 분기형 맵을 그린다.
    // 예전 9x7 그리드와 달리 실제로 존재하는 노드와, 그 노드끼리를 잇는 선만 그린다 -
    // 화면에 보이는 방은 전부 진짜 갈 수 있는 곳이고, 그중 지금 위치에서 갈 수 있는 곳만
    // 밝게 표시되고 클릭이 된다.
    private void RebuildMap(DungeonMap map)
    {
        foreach (var obj in lineObjects) Destroy(obj);
        lineObjects.Clear();
        foreach (var obj in tileObjects) Destroy(obj);
        tileObjects.Clear();

        var rt = (RectTransform)gridParent;
        float w = rt.rect.width;
        float h = rt.rect.height;
        float usableW = Mathf.Max(1f, w - MarginX * 2f);
        float usableH = Mathf.Max(1f, h - MarginY * 2f);

        Vector2 PosOf(MapNode n) => new Vector2(
            MarginX + n.x * usableW - w / 2f,
            MarginY + (float)n.floor / (map.FloorCount - 1) * usableH - h / 2f);

        var reachableIds = new HashSet<int>(map.ReachableNodes().Select(n => n.id));

        // 연결선을 버튼보다 먼저 그려서 항상 아래에 깔리게 함
        foreach (var node in map.Nodes)
        {
            Vector2 from = PosOf(node);
            bool outgoingFromCurrent = node.id == map.CurrentNodeId;
            foreach (int nextId in node.next)
            {
                MapNode target = map.GetNode(nextId);
                lineObjects.Add(CreateLine(from, PosOf(target), outgoingFromCurrent));
            }
        }

        // 아직 입장 전이면 화면 아래에 가상의 "시작" 지점과, 0층으로 가는 선을 보여준다
        if (map.CurrentNodeId < 0)
        {
            Vector2 startPos = new Vector2(0f, -h / 2f - MarginY * 0.6f);
            foreach (var node in map.NodesOnFloor(0))
                lineObjects.Add(CreateLine(startPos, PosOf(node), true));

            GameObject startMarker = Instantiate(tilePrefab, gridParent);
            tileObjects.Add(startMarker);
            var srt = (RectTransform)startMarker.transform;
            srt.anchoredPosition = startPos;
            srt.sizeDelta = new Vector2(NodeSize * 0.8f, NodeSize * 0.8f);
            startMarker.GetComponent<Image>().color = new Color(0.4f, 0.8f, 1f);
            startMarker.GetComponentInChildren<TextMeshProUGUI>().text = "시작";
            startMarker.GetComponent<Button>().interactable = false;
        }

        foreach (var node in map.Nodes)
        {
            GameObject obj = Instantiate(tilePrefab, gridParent);
            tileObjects.Add(obj);

            var nodeRt = (RectTransform)obj.transform;
            nodeRt.anchoredPosition = PosOf(node);
            nodeRt.sizeDelta = new Vector2(NodeSize, NodeSize);

            var img = obj.GetComponent<Image>();
            var lbl = obj.GetComponentInChildren<TextMeshProUGUI>();
            var btn = obj.GetComponent<Button>();

            bool isCurrent   = node.id == map.CurrentNodeId;
            bool isReachable = reachableIds.Contains(node.id);

            if (isCurrent)
            {
                img.color = new Color(0.2f, 0.6f, 1f);
                lbl.text  = "★";
            }
            else if (node.isCleared)
            {
                img.color = new Color(0.2f, 0.2f, 0.25f);
                lbl.text  = "✓";
            }
            else
            {
                Color baseColor = tileColor.TryGetValue(node.type, out Color c) ? c : Color.gray;
                img.color = isReachable ? baseColor : baseColor * new Color(0.45f, 0.45f, 0.45f, 0.6f);
                lbl.text  = tileIcon.TryGetValue(node.type, out string icon) ? icon : "?";
            }

            btn.interactable = isReachable && !isCurrent;
            if (btn.interactable)
            {
                int nodeId = node.id;
                btn.onClick.AddListener(() => DungeonManager.Instance.MoveToNode(nodeId));
            }
        }
    }

    private GameObject CreateLine(Vector2 from, Vector2 to, bool active)
    {
        GameObject line = new GameObject("Line", typeof(RectTransform));
        line.transform.SetParent(gridParent, false);

        var img = line.AddComponent<Image>();
        img.color = active ? new Color(1f, 0.85f, 0.3f, 0.8f) : new Color(1f, 1f, 1f, 0.15f);
        img.raycastTarget = false;

        Vector2 dir = to - from;
        float length = dir.magnitude;

        var rt = (RectTransform)line.transform;
        rt.sizeDelta        = new Vector2(length, active ? 5f : 3f);
        rt.anchoredPosition  = (from + to) / 2f;
        rt.localRotation     = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        return line;
    }
}
