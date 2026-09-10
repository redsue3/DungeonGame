using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Slider          playerHpSlider;
    [SerializeField] private TextMeshProUGUI playerHpText;
    [SerializeField] private TextMeshProUGUI playerBlockText;
    [SerializeField] private TextMeshProUGUI playerCostText;
    [SerializeField] private TextMeshProUGUI playerStatusText;  // 독/화상/힘

    [Header("적 영역")]
    [SerializeField] private Transform       enemyParent;
    [SerializeField] private GameObject      enemyPanelPrefab;  // EnemyPanelUI 달린 프리팹

    [Header("패 영역")]
    [SerializeField] private Transform       handParent;
    [SerializeField] private GameObject      cardPrefab;        // CardUI 달린 프리팹
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private TextMeshProUGUI discardCountText;

    [Header("버튼")]
    [SerializeField] private Button          endTurnBtn;
    [SerializeField] private TextMeshProUGUI turnText;

    [Header("전투 그리드 (4단계 - 이동/도주/키이팅, 2026-09)")]
    [SerializeField] private Transform       battleGridParent;
    [SerializeField] private GameObject      battleTilePrefab;   // 던전맵과 같은 MapTile 프리팹 재사용
    [SerializeField] private TextMeshProUGUI moveHintText;

    private const float BattleTileSize = 48f;

    private readonly List<GameObject> handObjects  = new List<GameObject>();
    private readonly List<GameObject> enemyObjects = new List<GameObject>();
    private readonly Dictionary<(int, int), GameObject> battleTileObjects = new Dictionary<(int, int), GameObject>();
    private RoomInfo lastGridRoom;

    // 단일 공격 카드가 때릴 적. 인덱스가 아니라 참조로 들고 있어야
    // 죽은 적이 목록에서 빠져도 엉뚱한 적을 때리지 않는다.
    private Enemy selectedTarget;

    void OnEnable()
    {
        endTurnBtn.onClick.AddListener(OnEndTurn);
        selectedTarget = null; // 새 전투 - 이전 전투의 적 참조를 버린다
        lastGridRoom = null;   // 새 전투 - 방이 바뀌었을 수 있으니 그리드도 강제로 다시 그린다
        Refresh();
    }

    void OnDisable()
    {
        endTurnBtn.onClick.RemoveAllListeners();
    }

    void Update()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.CurrentState != BattleState.PlayerTurn) return;

        if (Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W)) TryMove(0, 1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S)) TryMove(0, -1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A)) TryMove(-1, 0);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) TryMove(1, 0);
    }

    // BattleManager에서 매 변경마다 호출.
    // RefreshEnemies가 selectedTarget을 확정시켜야 RefreshHand의 사거리 판정이 정확해서 순서가 중요하다.
    public void Refresh()
    {
        RefreshPlayerStatus();
        RefreshEnemies();
        RefreshHand();
        RefreshGrid();
    }

    private void RefreshPlayerStatus()
    {
        var player = DungeonManager.Instance?.Player;
        if (player == null) return;

        playerNameText.text  = player.characterName;
        playerHpSlider.value = (float)player.currentHp / player.maxHp;
        playerHpText.text    = $"{player.currentHp} / {player.maxHp}";
        playerBlockText.text = player.block > 0 ? $"방어 {player.block}" : "";

        int currentCost = BattleManager.Instance != null
            ? BattleManager.Instance.CurrentCost : player.maxCost;
        playerCostText.text = BuildCostPips(currentCost, player.maxCost);

        var sb = new System.Text.StringBuilder();
        if (player.poisonStack   > 0) sb.Append($"독 {player.poisonStack}  ");
        if (player.burnStack     > 0) sb.Append($"화상 {player.burnStack}  ");
        if (player.strengthStack > 0) sb.Append($"힘 +{player.strengthStack}");
        playerStatusText.text = sb.ToString().TrimEnd();
    }

    private void RefreshHand()
    {
        foreach (var obj in handObjects) Destroy(obj);
        handObjects.Clear();

        var player = DungeonManager.Instance?.Player;
        if (player == null) return;

        List<Card> hand = player.deck.GetHand();

        for (int i = 0; i < hand.Count; i++)
        {
            int idx   = i;
            Card card = hand[i];

            GameObject obj = Instantiate(cardPrefab, handParent);
            handObjects.Add(obj);

            var ui = obj.GetComponent<CardUI>();
            if (ui != null) ui.Setup(card);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
            {
                int currentCost = BattleManager.Instance?.CurrentCost ?? 0;
                btn.interactable = card.cost <= currentCost && InRangeOfTarget(card);
                btn.onClick.AddListener(() =>
                {
                    BattleManager.Instance?.UseCard(idx, selectedTarget);
                    Refresh();
                });
            }
        }

        deckCountText.text    = $"덱 {player.deck.DrawCount}";
        discardCountText.text = $"묘지 {player.deck.DiscardCount}";
    }

    private void RefreshEnemies()
    {
        foreach (var obj in enemyObjects) Destroy(obj);
        enemyObjects.Clear();

        var enemies = BattleManager.Instance?.GetEnemies();
        if (enemies == null) return;

        // 아직 타겟이 없거나 선택했던 적이 죽었으면 첫 번째 생존 적으로
        if (selectedTarget == null || !selectedTarget.IsAlive)
            selectedTarget = enemies.Find(e => e.IsAlive);

        foreach (Enemy enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            Enemy captured = enemy;
            GameObject obj = Instantiate(enemyPanelPrefab, enemyParent);
            enemyObjects.Add(obj);

            var ui = obj.GetComponent<EnemyPanelUI>();
            if (ui != null) ui.Setup(enemy, enemy == selectedTarget);

            var btn = obj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() =>
                {
                    selectedTarget = captured;
                    RefreshEnemies();
                    RefreshHand(); // 타겟이 바뀌면 사거리 판정도 다시 해야 카드 활성화 상태가 즉시 맞는다
                });
        }
    }

    private void OnEndTurn()
    {
        BattleManager.Instance?.EndPlayerTurn();
        Refresh();
    }

    // AoE/비공격 카드는 사거리 제약이 없다. 단일 대상 공격/독/화상 카드만 선택한 적까지의 거리를 확인.
    private bool InRangeOfTarget(Card card)
    {
        bool hasOffense = card.damage > 0 || card.poisonApply > 0 || card.burnApply > 0;
        if (!hasOffense || card.isAoe) return true;

        var floor = DungeonManager.Instance?.CurrentFloor;
        if (floor == null || selectedTarget == null) return true;

        return BattleGridSystem.Chebyshev(floor.PlayerX, floor.PlayerY, selectedTarget.x, selectedTarget.y) <= card.attackRange;
    }

    private void TryMove(int dx, int dy)
    {
        if (BattleManager.Instance != null && BattleManager.Instance.TryMovePlayer(dx, dy))
            Refresh();
    }

    // ─────────────────────────────────────────
    // 전투 그리드 - 방 범위(+1칸 여유, 도주 출구까지 보이게)를 고정 배치로 그린다.
    // 던전맵(DungeonMapUI)의 카메라 추종 방식과 달리 방이 작아서 스크롤이 필요 없다.
    // ─────────────────────────────────────────
    private void RefreshGrid()
    {
        var floor = DungeonManager.Instance?.CurrentFloor;
        var bm    = BattleManager.Instance;
        if (floor == null || bm == null || bm.Room == null || battleGridParent == null) return;

        RoomInfo room = bm.Room;
        if (room != lastGridRoom)
        {
            RebuildGridTiles(floor, room);
            lastGridRoom = room;
        }

        RepaintGridTiles(floor, bm);
    }

    private void RebuildGridTiles(DungeonFloor floor, RoomInfo room)
    {
        foreach (var obj in battleTileObjects.Values) Destroy(obj);
        battleTileObjects.Clear();
        if (battleTilePrefab == null) return;

        // 방 안쪽 + 바깥 1칸(복도 진입부, 도주 시 실제로 밟는 칸)까지 걸어다닐 수 있는 칸만 그린다.
        for (int x = room.x - 1; x <= room.x + room.w; x++)
        {
            for (int y = room.y - 1; y <= room.y + room.h; y++)
            {
                if (!floor.IsWalkable(x, y)) continue;

                GameObject obj = Instantiate(battleTilePrefab, battleGridParent);
                var rt = (RectTransform)obj.transform;
                rt.sizeDelta = new Vector2(BattleTileSize, BattleTileSize);
                rt.anchoredPosition = new Vector2((x - room.x) * BattleTileSize, (y - room.y) * BattleTileSize);
                battleTileObjects[(x, y)] = obj;
            }
        }
    }

    private void RepaintGridTiles(DungeonFloor floor, BattleManager bm)
    {
        var enemies = bm.GetEnemies();
        bool canAct = bm.CurrentState == BattleState.PlayerTurn && !bm.HasMovedThisTurn;

        foreach (var kv in battleTileObjects)
        {
            int x = kv.Key.Item1, y = kv.Key.Item2;
            GameObject obj = kv.Value;

            var img = obj.GetComponent<Image>();
            var lbl = obj.GetComponentInChildren<TextMeshProUGUI>();
            var btn = obj.GetComponent<Button>();

            bool  isPlayer = x == floor.PlayerX && y == floor.PlayerY;
            Enemy occupant = isPlayer ? null : enemies.Find(e => e.IsAlive && e.x == x && e.y == y);

            if (isPlayer)
            {
                img.color = new Color(0.2f, 0.6f, 1f);
                lbl.text  = "★";
            }
            else if (occupant != null)
            {
                img.color = occupant == selectedTarget ? new Color(1f, 0.3f, 0.25f) : new Color(0.7f, 0.35f, 0.3f);
                lbl.text  = "적";
            }
            else
            {
                img.color = new Color(0.3f, 0.3f, 0.35f);
                lbl.text  = "";
            }

            int  ddx = x - floor.PlayerX, ddy = y - floor.PlayerY;
            // BattleGridSystem.CanStepTo와 같은 기준(대각선 코너컷 포함)을 써야 밝게 보이는 칸과
            // 실제로 클릭했을 때 이동되는 칸이 일치한다 - 안 그러면 눌리는데 반응 없는 죽은 클릭이 생긴다.
            bool isAdjacent = !isPlayer && BattleGridSystem.CanStepTo(floor, floor.PlayerX, floor.PlayerY, ddx, ddy);

            btn.onClick.RemoveAllListeners();
            if (occupant != null)
            {
                // 적이 서 있는 칸 클릭 - 카드 타겟 선택 (인접 여부와 무관, 사거리는 카드 사용 시점에 따로 체크)
                Enemy captured = occupant;
                btn.interactable = true;
                btn.onClick.AddListener(() => { selectedTarget = captured; Refresh(); });
            }
            else if (canAct && isAdjacent)
            {
                btn.interactable = true;
                btn.onClick.AddListener(() => TryMove(ddx, ddy));
            }
            else
            {
                btn.interactable = false;
            }
        }

        if (moveHintText != null)
            moveHintText.text = bm.HasMovedThisTurn ? "이동 완료 (다음 턴에 다시 이동 가능)"
                                                     : "인접 칸 클릭 또는 WASD로 이동 - 방 밖으로 나가면 도주";
    }

    private string BuildCostPips(int current, int max)
    {
        var sb = new System.Text.StringBuilder("코스트 ");
        int pips = Mathf.Max(max, current); // 유물로 최대치를 넘겨 받은 코스트도 초과분까지 표시
        for (int i = 0; i < pips; i++)
            sb.Append(i < current ? "◆" : "◇");
        return sb.ToString();
    }
}
