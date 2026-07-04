using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonMapUI : MonoBehaviour
{
    [Header("맵 그리드")]
    [SerializeField] private GameObject tilePrefab;   // Button + Image + Text 구성
    [SerializeField] private Transform  gridParent;   // Grid Layout Group 달린 부모
    [SerializeField] private TextMeshProUGUI layerText;

    [Header("플레이어 상태 (상단)")]
    [SerializeField] private TextMeshProUGUI playerHpText;
    [SerializeField] private TextMeshProUGUI playerGoldText;
    [SerializeField] private TextMeshProUGUI playerRelicsText;
    [SerializeField] private TextMeshProUGUI playerHungerText;

    [Header("인벤토리")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Button      inventoryBtn;

    private readonly List<GameObject> tileObjects = new List<GameObject>();

    // 타일 타입별 아이콘 문자 (폰트 아이콘 없이 텍스트로 표현)
    private static readonly Dictionary<TileType, string> tileIcon = new Dictionary<TileType, string>
    {
        [TileType.NormalEnemy] = "⚔",
        [TileType.GroupEnemy]  = "⚔⚔",
        [TileType.EliteEnemy]  = "☠",
        [TileType.Boss]        = "👑",
        [TileType.Rest]        = "🔥",
        [TileType.Shop]        = "🛒",
        [TileType.Empty]       = "·",
    };

    private static readonly Dictionary<TileType, Color> tileColor = new Dictionary<TileType, Color>
    {
        [TileType.NormalEnemy] = new Color(0.9f, 0.4f, 0.4f),
        [TileType.GroupEnemy]  = new Color(0.85f, 0.3f, 0.3f),
        [TileType.EliteEnemy]  = new Color(0.7f, 0.2f, 0.8f),
        [TileType.Boss]        = new Color(1f, 0.2f, 0.2f),
        [TileType.Rest]        = new Color(0.3f, 0.75f, 0.4f),
        [TileType.Shop]        = new Color(1f, 0.8f, 0.2f),
        [TileType.Empty]       = new Color(0.3f, 0.3f, 0.35f),
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
        RebuildGrid(map);
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

    private void RebuildGrid(DungeonMap map)
    {
        foreach (var obj in tileObjects) Destroy(obj);
        tileObjects.Clear();

        // GridLayoutGroup 은 x(col) 기준으로 WIDTH 열 설정해둬야 함
        for (int y = DungeonMap.HEIGHT - 1; y >= 0; y--)
        {
            for (int x = 0; x < DungeonMap.WIDTH; x++)
            {
                MapTile tile = map[x, y];
                bool isPlayer = (x == map.PlayerX && y == map.PlayerY);

                GameObject obj = Instantiate(tilePrefab, gridParent);
                tileObjects.Add(obj);

                var img = obj.GetComponent<Image>();
                var lbl = obj.GetComponentInChildren<TextMeshProUGUI>();
                var btn = obj.GetComponent<Button>();

                if (isPlayer)
                {
                    img.color = new Color(0.2f, 0.6f, 1f);
                    lbl.text  = "★";
                }
                else if (tile.isCleared)
                {
                    img.color = new Color(0.2f, 0.2f, 0.25f);
                    lbl.text  = "✓";
                }
                else
                {
                    img.color = tileColor.TryGetValue(tile.type, out Color c) ? c : Color.gray;
                    lbl.text  = tileIcon.TryGetValue(tile.type, out string icon) ? icon : "?";
                }

                // 이동 버튼: 현재 플레이어 위치 인접 타일만 활성화
                int cx = x, cy = y;
                int dx = cx - map.PlayerX;
                int dy = cy - map.PlayerY;
                bool adjacent = !isPlayer && Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 1
                                && !tile.isCleared && tile.type != TileType.Empty;
                btn.interactable = adjacent;

                if (adjacent)
                    btn.onClick.AddListener(() => DungeonManager.Instance.MovePlayer(dx, dy));
            }
        }
    }
}
