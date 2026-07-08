using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 픽셀던전 스타일 그리드 던전 렌더러. 예전 노드그래프(층/x 자유배치 + 연결선) 방식을 대체한다.
// 바닥 타일만 실체 오브젝트로 만들고(벽은 그냥 빈 공간), 플레이어를 항상 화면 중앙에 두고
// 나머지 타일/적 마커를 플레이어 기준 상대 좌표로 배치해서 카메라 추종 효과를 낸다.
public class DungeonMapUI : MonoBehaviour
{
    [Header("맵 그리드")]
    [SerializeField] private GameObject tilePrefab;   // Button + Image + Text 구성
    [SerializeField] private Transform  gridParent;   // 중앙 기준 자유 배치 컨테이너
    [SerializeField] private TextMeshProUGUI layerText;

    [Header("플레이어 상태 (상단)")]
    [SerializeField] private TextMeshProUGUI playerHpText;
    [SerializeField] private TextMeshProUGUI playerGoldText;
    [SerializeField] private TextMeshProUGUI playerRelicsText;
    [SerializeField] private TextMeshProUGUI playerHungerText;

    [Header("인벤토리")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Button      inventoryBtn;

    private const float MinTileSize = 14f;
    private const float MaxTileSize = 48f;
    private const float DefaultTileSize = 26f;
    private const string TileSizePrefKey = "DungeonMap.TileSize";

    private float TileSize;

    // 타일 타입별 아이콘/색 (방 중심 칸에만 표시)
    private static readonly Dictionary<TileType, string> roomIcon = new Dictionary<TileType, string>
    {
        [TileType.NormalEnemy] = "적",
        [TileType.GroupEnemy]  = "적×2",
        [TileType.EliteEnemy]  = "정예",
        [TileType.Boss]        = "보스",
        [TileType.Rest]        = "휴식",
        [TileType.Shop]        = "상점",
        [TileType.Shrine]      = "성소",
    };

    private static readonly Dictionary<TileType, Color> roomColor = new Dictionary<TileType, Color>
    {
        [TileType.NormalEnemy] = new Color(0.9f, 0.4f, 0.4f),
        [TileType.GroupEnemy]  = new Color(0.85f, 0.3f, 0.3f),
        [TileType.EliteEnemy]  = new Color(0.7f, 0.2f, 0.8f),
        [TileType.Boss]        = new Color(1f, 0.2f, 0.2f),
        [TileType.Rest]        = new Color(0.3f, 0.75f, 0.4f),
        [TileType.Shop]        = new Color(1f, 0.8f, 0.2f),
        [TileType.Shrine]      = new Color(0.6f, 0.5f, 1f),
    };

    private static readonly Color FloorVisibleColor = new Color(0.32f, 0.32f, 0.38f);
    private static readonly Color FloorDimColor      = new Color(0.16f, 0.16f, 0.2f);
    private static readonly Color PlayerColor        = new Color(0.2f, 0.6f, 1f);
    private static readonly Color EnemyIdleColor     = new Color(0.6f, 0.55f, 0.2f);
    private static readonly Color EnemyChaseColor    = new Color(1f, 0.25f, 0.2f);

    private DungeonFloor lastFloor;
    private readonly Dictionary<(int, int), GameObject> floorTileObjects = new Dictionary<(int, int), GameObject>();
    private GameObject playerMarker;
    private readonly Dictionary<int, GameObject> enemyMarkers = new Dictionary<int, GameObject>();

    void OnEnable()
    {
        TileSize = PlayerPrefs.GetFloat(TileSizePrefKey, DefaultTileSize);
        inventoryBtn?.onClick.AddListener(() => inventoryUI?.Open());
        Refresh();
    }

    void OnDisable()
    {
        inventoryBtn?.onClick.RemoveAllListeners();
    }

    void Update()
    {
        if (DungeonManager.Instance == null || DungeonManager.Instance.CurrentState != GameState.DungeonMap) return;

        if (Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W)) Move(0, 1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S)) Move(0, -1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A)) Move(-1, 0);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Move(1, 0);

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0f) Zoom(scroll);
    }

    // 마우스 휠로 타일 크기 조절 (기억해뒀다가 다음에 켤 때도 유지)
    private void Zoom(float delta)
    {
        TileSize = Mathf.Clamp(TileSize + delta * 3f, MinTileSize, MaxTileSize);
        PlayerPrefs.SetFloat(TileSizePrefKey, TileSize);

        DungeonFloor floor = DungeonManager.Instance?.CurrentFloor;
        if (floor != null) RepaintGrid(floor);
    }

    private void Move(int dx, int dy)
    {
        if (DungeonManager.Instance.TryMove(dx, dy)) Refresh();
    }

    public void Refresh()
    {
        DungeonFloor floor = DungeonManager.Instance?.CurrentFloor;
        if (floor == null) return;

        layerText.text = $"{floor.Layer}계층";
        UpdatePlayerStatus();

        if (floor != lastFloor)
        {
            RebuildGridObjects(floor);
            lastFloor = floor;
        }

        RepaintGrid(floor);
    }

    private void UpdatePlayerStatus()
    {
        var p = DungeonManager.Instance?.Player;
        if (p == null) return;

        playerHpText.text    = $"HP  {p.currentHp} / {p.maxHp}";
        playerGoldText.text  = $"골드  {p.gold}";
        if (playerHungerText != null)
            playerHungerText.text = $"배고픔  {p.hunger} / {p.maxHunger}" + (p.IsStarving ? " (위험!)" : "");

        var relicNames = new System.Text.StringBuilder();
        foreach (string id in p.relics.GetAll())
        {
            RelicData r = RelicDatabase.Get(id);
            if (r != null) relicNames.Append($"[{r.displayName}] ");
        }
        playerRelicsText.text = relicNames.Length > 0 ? relicNames.ToString().TrimEnd() : "유물 없음";
    }

    // 바닥 타일 오브젝트를 새 층 크기에 맞게 새로 만든다 (층이 바뀔 때만 호출 - 매 이동마다 다시 만들지 않음).
    private void RebuildGridObjects(DungeonFloor floor)
    {
        foreach (var obj in floorTileObjects.Values) Destroy(obj);
        floorTileObjects.Clear();
        foreach (var obj in enemyMarkers.Values) Destroy(obj);
        enemyMarkers.Clear();
        if (playerMarker != null) Destroy(playerMarker);

        for (int x = 0; x < floor.Width; x++)
        {
            for (int y = 0; y < floor.Height; y++)
            {
                if (floor.Tiles[x, y] != TileKind.Floor) continue;

                GameObject obj = Instantiate(tilePrefab, gridParent);
                var rt = (RectTransform)obj.transform;
                rt.sizeDelta = new Vector2(TileSize, TileSize);
                floorTileObjects[(x, y)] = obj;
            }
        }

        playerMarker = Instantiate(tilePrefab, gridParent);
        var prt = (RectTransform)playerMarker.transform;
        prt.sizeDelta = new Vector2(TileSize, TileSize);
        playerMarker.GetComponent<Button>().interactable = false;
        playerMarker.GetComponent<Image>().color = PlayerColor;
        playerMarker.GetComponentInChildren<TextMeshProUGUI>().text = "★";
    }

    // 플레이어를 중심(0,0)에 고정하고 나머지는 상대 좌표로 그린다 (카메라 추종 효과).
    private void RepaintGrid(DungeonFloor floor)
    {
        int px = floor.PlayerX, py = floor.PlayerY;
        var playerRt = (RectTransform)playerMarker.transform;
        playerRt.anchoredPosition = Vector2.zero;
        playerRt.sizeDelta = new Vector2(TileSize, TileSize);

        foreach (var kv in floorTileObjects)
        {
            int x = kv.Key.Item1, y = kv.Key.Item2;
            GameObject obj = kv.Value;

            if (!floor.Visited[x, y]) { obj.SetActive(false); continue; }
            obj.SetActive(true);

            var rt  = (RectTransform)obj.transform;
            rt.sizeDelta = new Vector2(TileSize, TileSize);
            rt.anchoredPosition = new Vector2((x - px) * TileSize, (y - py) * TileSize);

            bool visible = floor.Visible[x, y];
            var img = obj.GetComponent<Image>();
            var lbl = obj.GetComponentInChildren<TextMeshProUGUI>();
            var btn = obj.GetComponent<Button>();

            RoomInfo room = floor.RoomAt(x, y);
            bool showRoomIcon = room != null && !room.isCleared && room.CenterX == x && room.CenterY == y
                                 && roomIcon.TryGetValue(room.roomType, out string icon);

            if (showRoomIcon)
            {
                img.color = visible ? roomColor[room.roomType] : roomColor[room.roomType] * new Color(0.5f, 0.5f, 0.5f, 1f);
                lbl.text  = roomIcon[room.roomType];
            }
            else
            {
                img.color = visible ? FloorVisibleColor : FloorDimColor;
                lbl.text  = "";
            }

            bool isAdjacent = System.Math.Abs(x - px) + System.Math.Abs(y - py) == 1 ||
                               (System.Math.Abs(x - px) == 1 && System.Math.Abs(y - py) == 1);
            btn.interactable = visible && isAdjacent;
            btn.onClick.RemoveAllListeners();
            if (btn.interactable)
            {
                int ddx = x - px, ddy = y - py;
                btn.onClick.AddListener(() => Move(ddx, ddy));
            }
        }

        RepaintEnemies(floor, px, py);
    }

    private void RepaintEnemies(DungeonFloor floor, int px, int py)
    {
        var seen = new HashSet<int>();

        foreach (EnemySpawn e in floor.Enemies)
        {
            if (e.isDead) continue;
            if (!floor.Visible[e.x, e.y]) continue;
            seen.Add(e.id);

            if (!enemyMarkers.TryGetValue(e.id, out GameObject obj))
            {
                obj = Instantiate(tilePrefab, gridParent);
                obj.GetComponent<Button>().interactable = false;
                enemyMarkers[e.id] = obj;
            }

            var ert = (RectTransform)obj.transform;
            ert.sizeDelta = new Vector2(TileSize * 0.8f, TileSize * 0.8f);
            ert.anchoredPosition = new Vector2((e.x - px) * TileSize, (e.y - py) * TileSize);
            obj.GetComponent<Image>().color = e.state == EnemyAiState.Chasing ? EnemyChaseColor : EnemyIdleColor;
            obj.GetComponentInChildren<TextMeshProUGUI>().text = e.state == EnemyAiState.Chasing ? "!" : "?";
        }

        var toRemove = new List<int>();
        foreach (var kv in enemyMarkers)
        {
            if (seen.Contains(kv.Key)) continue;
            Destroy(kv.Value);
            toRemove.Add(kv.Key);
        }
        foreach (int id in toRemove) enemyMarkers.Remove(id);
    }
}
