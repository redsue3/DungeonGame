using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

// 씬을 처음부터 완전히 구성하는 에디터 툴.
// 패널 껍데기만 만드는 게 아니라 버튼/텍스트/슬라이더/그리드/프리팹까지 전부 코드로 만들고
// 각 UI 스크립트의 SerializeField 를 전부 연결해서, 메뉴 실행 후 바로 Play 할 수 있게 한다.
public static class SceneSetup
{
    private const string PrefabDir = "Assets/Prefabs/UI";

    private static readonly Color PanelBg   = new Color(0.08f, 0.08f, 0.11f);
    private static readonly Color HeaderBg  = new Color(0.14f, 0.14f, 0.19f);
    private static readonly Color EntryBg   = new Color(0.20f, 0.20f, 0.26f);
    private static readonly Color BtnBlue   = new Color(0.25f, 0.45f, 0.80f);
    private static readonly Color BtnGreen  = new Color(0.30f, 0.65f, 0.35f);
    private static readonly Color BtnRed    = new Color(0.75f, 0.30f, 0.30f);
    private static readonly Color BtnYellow = new Color(0.75f, 0.65f, 0.20f);
    private static readonly Color TextWhite = Color.white;
    private static readonly Color TextDim   = new Color(0.75f, 0.75f, 0.80f);

    [MenuItem("DungeonGame/씬 자동 세팅")]
    public static void BuildScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EnsureTMPEssentials();

        // ── 카메라 ──
        Camera cam = new GameObject("Main Camera").AddComponent<Camera>();
        cam.gameObject.AddComponent<AudioListener>();
        cam.gameObject.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        cam.transform.position = new Vector3(0, 0, -10);

        // ── 매니저 ──
        GameObject managers = new GameObject("Managers");
        managers.AddComponent<DungeonManager>();
        managers.AddComponent<BattleManager>();
        managers.AddComponent<LayerManager>();

        // ── Canvas ──
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        UIManager uiManager = canvasGO.AddComponent<UIManager>();

        // ── EventSystem ──
        GameObject esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── 재사용 프리팹 ──
        GameObject cardPrefab       = BuildCardPrefab();
        GameObject enemyPanelPrefab = BuildEnemyPanelPrefab();
        GameObject shopItemPrefab   = BuildShopItemPrefab();
        GameObject tilePrefab       = BuildMapTilePrefab();
        GameObject foodItemPrefab   = BuildFoodItemPrefab();

        // ── 패널 ──
        GameObject characterSelectPanel = BuildCharacterSelectPanel(canvasGO.transform);
        GameObject dungeonMapPanel      = BuildDungeonMapPanel(canvasGO.transform, tilePrefab, foodItemPrefab);
        GameObject battlePanel          = BuildBattlePanel(canvasGO.transform, cardPrefab, enemyPanelPrefab);
        GameObject rewardPanel          = BuildRewardPanel(canvasGO.transform, cardPrefab);
        GameObject restPanel            = BuildRestPanel(canvasGO.transform);
        GameObject shopPanel            = BuildShopPanel(canvasGO.transform, shopItemPrefab, cardPrefab);
        GameObject shrinePanel          = BuildShrinePanel(canvasGO.transform, cardPrefab);
        GameObject gameOverPanel        = BuildGameOverPanel(canvasGO.transform);
        GameObject victoryPanel         = BuildVictoryPanel(canvasGO.transform);

        foreach (var p in new[] { characterSelectPanel, dungeonMapPanel, battlePanel, rewardPanel, restPanel, shopPanel, shrinePanel, gameOverPanel, victoryPanel })
            p.SetActive(false);
        characterSelectPanel.SetActive(true);

        // ── UIManager 연결 ──
        Bind(uiManager, "characterSelectPanel", characterSelectPanel);
        Bind(uiManager, "dungeonMapPanel", dungeonMapPanel);
        Bind(uiManager, "battlePanel", battlePanel);
        Bind(uiManager, "rewardPanel", rewardPanel);
        Bind(uiManager, "restPanel", restPanel);
        Bind(uiManager, "shopPanel", shopPanel);
        Bind(uiManager, "shrinePanel", shrinePanel);
        Bind(uiManager, "gameOverPanel", gameOverPanel);
        Bind(uiManager, "victoryPanel", victoryPanel);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/GameScene.unity");
        Debug.Log("✅ 완전히 연결된 씬 세팅 완료! 바로 Play 해서 플레이할 수 있습니다.");
    }

    // ─────────────────────────────────────────────────────────
    // TMP 필수 리소스 확인
    // ─────────────────────────────────────────────────────────
    private static void EnsureTMPEssentials()
    {
        bool hasFont = AssetDatabase.FindAssets("t:TMP_FontAsset").Length > 0;
        if (hasFont) return;

        try
        {
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"TMP Essential Resources 자동 임포트 실패: {e.Message}\n" +
                              "글자가 안 보이면 Window > TextMeshPro > Import TMP Essential Resources 를 수동 실행하세요.");
        }
    }

    // ─────────────────────────────────────────────────────────
    // 프리팹 빌더
    // ─────────────────────────────────────────────────────────
    private static GameObject BuildMapTilePrefab()
    {
        GameObject go = NewGO("MapTile");
        go.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f);
        go.AddComponent<Button>();
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 70; le.preferredHeight = 70;

        Text(go.transform, "Label", "?", 26, TextAlignmentOptions.Center, Color.white);

        return SaveTemplateAsPrefab(go, $"{PrefabDir}/MapTile.prefab");
    }

    private static GameObject BuildCardPrefab()
    {
        GameObject go = NewGO("CardEntry");
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 200; le.preferredHeight = 280;

        Image bg = go.AddComponent<Image>();
        bg.color = EntryBg;
        go.AddComponent<Button>();

        var manaBg = Img(go.transform, "ManaBg", new Color(0.15f, 0.35f, 0.8f));
        Anchor(manaBg.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(8, -44), new Vector2(44, -8));

        var manaText = Text(go.transform, "ManaCost", "1", 24, TextAlignmentOptions.Center, Color.white);
        Anchor(manaText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(8, -44), new Vector2(44, -8));

        var nameText = Text(go.transform, "Name", "카드", 20, TextAlignmentOptions.Center, Color.white);
        Anchor(nameText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(50, -40), new Vector2(-8, -8));

        var typeTag = Text(go.transform, "TypeTag", "공격", 15, TextAlignmentOptions.Center, TextDim);
        Anchor(typeTag.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -70), new Vector2(-8, -46));

        var descText = Text(go.transform, "Desc", "설명", 16, TextAlignmentOptions.Center, Color.white);
        Anchor(descText.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(8, 8), new Vector2(-8, -74));
        descText.textWrappingMode = TextWrappingModes.Normal;

        var ui = go.AddComponent<CardUI>();
        Bind(ui, "cardNameText", nameText);
        Bind(ui, "manaCostText", manaText);
        Bind(ui, "descText", descText);
        Bind(ui, "typeTag", typeTag);
        Bind(ui, "cardBg", bg);

        return SaveTemplateAsPrefab(go, $"{PrefabDir}/Card.prefab");
    }

    private static GameObject BuildEnemyPanelPrefab()
    {
        GameObject go = NewGO("EnemyEntry");
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 220; le.preferredHeight = 180;

        go.AddComponent<Image>().color = new Color(0.18f, 0.12f, 0.14f);
        go.AddComponent<Button>();

        var outline = Img(go.transform, "SelectionOutline", new Color(1f, 0.85f, 0.2f, 0.45f));
        outline.raycastTarget = false;

        var nameText = Text(go.transform, "Name", "적", 20, TextAlignmentOptions.Center, Color.white);
        Anchor(nameText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -32), new Vector2(-8, -6));

        var hpSlider = Bar(go.transform, "HpBar", new Color(0.8f, 0.25f, 0.25f));
        Anchor(hpSlider.GetComponent<RectTransform>(), new Vector2(0.1f, 1f), new Vector2(0.9f, 1f), new Vector2(0, -52), new Vector2(0, -38));

        var hpText = Text(go.transform, "HpText", "0/0", 15, TextAlignmentOptions.Center, Color.white);
        Anchor(hpText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -70), new Vector2(-8, -54));

        var blockText = Text(go.transform, "BlockText", "", 15, TextAlignmentOptions.Center, new Color(0.5f, 0.75f, 1f));
        Anchor(blockText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -90), new Vector2(-8, -72));

        var statusText = Text(go.transform, "StatusText", "", 14, TextAlignmentOptions.Center, new Color(0.6f, 0.9f, 0.6f));
        Anchor(statusText.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(8, 34), new Vector2(-8, 54));

        var intentText = Text(go.transform, "IntentText", "", 17, TextAlignmentOptions.Center, new Color(1f, 0.7f, 0.3f));
        Anchor(intentText.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(8, 6), new Vector2(-8, 34));

        var ui = go.AddComponent<EnemyPanelUI>();
        Bind(ui, "nameText", nameText);
        Bind(ui, "hpSlider", hpSlider);
        Bind(ui, "hpText", hpText);
        Bind(ui, "blockText", blockText);
        Bind(ui, "intentText", intentText);
        Bind(ui, "statusText", statusText);
        Bind(ui, "selectionOutline", outline);

        return SaveTemplateAsPrefab(go, $"{PrefabDir}/EnemyPanel.prefab");
    }

    private static GameObject BuildShopItemPrefab()
    {
        GameObject go = NewGO("ShopEntry");
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 260; le.preferredHeight = 140;

        go.AddComponent<Image>().color = EntryBg;
        go.AddComponent<Button>();

        var tagText = Text(go.transform, "Tag", "카드", 13, TextAlignmentOptions.TopLeft, TextDim);
        Anchor(tagText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -24), new Vector2(-10, -6));

        var nameText = Text(go.transform, "Name", "아이템", 20, TextAlignmentOptions.TopLeft, Color.white);
        Anchor(nameText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -52), new Vector2(-10, -26));

        var descText = Text(go.transform, "Desc", "설명", 15, TextAlignmentOptions.TopLeft, TextDim);
        Anchor(descText.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(10, 30), new Vector2(-10, -54));
        descText.textWrappingMode = TextWrappingModes.Normal;

        var priceText = Text(go.transform, "Price", "0 G", 18, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
        Anchor(priceText.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(10, 6), new Vector2(-10, 28));

        var soldOverlay = Img(go.transform, "SoldOverlay", new Color(0, 0, 0, 0.6f));
        soldOverlay.raycastTarget = false;
        soldOverlay.gameObject.SetActive(false);

        var ui = go.AddComponent<ShopItemUI>();
        Bind(ui, "itemNameText", nameText);
        Bind(ui, "itemDescText", descText);
        Bind(ui, "priceText", priceText);
        Bind(ui, "tagText", tagText);
        Bind(ui, "soldOverlay", soldOverlay);

        return SaveTemplateAsPrefab(go, $"{PrefabDir}/ShopItem.prefab");
    }

    private static GameObject BuildFoodItemPrefab()
    {
        GameObject go = NewGO("FoodEntry");
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 240; le.preferredHeight = 90;

        go.AddComponent<Image>().color = EntryBg;
        go.AddComponent<Button>();

        var nameText = Text(go.transform, "Name", "식료품", 19, TextAlignmentOptions.TopLeft, Color.white);
        Anchor(nameText.rectTransform, new Vector2(0, 1), new Vector2(0.7f, 1), new Vector2(10, -30), new Vector2(-4, -6));

        var countText = Text(go.transform, "Count", "x1", 19, TextAlignmentOptions.TopRight, new Color(1f, 0.85f, 0.3f));
        Anchor(countText.rectTransform, new Vector2(0.7f, 1), new Vector2(1, 1), new Vector2(4, -30), new Vector2(-10, -6));

        var descText = Text(go.transform, "Desc", "설명", 14, TextAlignmentOptions.TopLeft, TextDim);
        Anchor(descText.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(10, 6), new Vector2(-10, -32));

        var ui = go.AddComponent<FoodItemUI>();
        Bind(ui, "nameText", nameText);
        Bind(ui, "descText", descText);
        Bind(ui, "countText", countText);

        return SaveTemplateAsPrefab(go, $"{PrefabDir}/FoodItem.prefab");
    }

    // ─────────────────────────────────────────────────────────
    // 패널 빌더
    // ─────────────────────────────────────────────────────────
    private static GameObject BuildCharacterSelectPanel(Transform canvas)
    {
        GameObject panel = FullPanel("CharacterSelectPanel", canvas, PanelBg);

        var title = Text(panel.transform, "Title", "직업을 선택하세요", 40, TextAlignmentOptions.Center, TextWhite);
        Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -140), new Vector2(0, -60));

        GameObject btnRow = NewGO("ClassButtons", panel.transform);
        Anchor(btnRow.GetComponent<RectTransform>(), new Vector2(0.15f, 0.74f), new Vector2(0.85f, 0.86f), Vector2.zero, Vector2.zero);
        var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 24;
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

        var warriorBtn = Btn(btnRow.transform, "WarriorBtn", "전사", BtnBlue, out _);
        var rogueBtn   = Btn(btnRow.transform, "RogueBtn", "도적", BtnBlue, out _);
        var mageBtn    = Btn(btnRow.transform, "MageBtn", "마법사", BtnBlue, out _);
        var paladinBtn = Btn(btnRow.transform, "PaladinBtn", "성기사", BtnBlue, out _);

        GameObject infoBox = NewGO("InfoBox", panel.transform);
        Anchor(infoBox.GetComponent<RectTransform>(), new Vector2(0.25f, 0.28f), new Vector2(0.75f, 0.7f), Vector2.zero, Vector2.zero);
        var vlg = infoBox.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

        var classNameText    = TextLine(infoBox.transform, "ClassName", "전사", 28, TextWhite, 44);
        var hpText            = TextLine(infoBox.transform, "HpText", "HP 80", 20, TextWhite, 32);
        var manaText           = TextLine(infoBox.transform, "ManaText", "마나 3", 20, TextWhite, 32);
        var handSizeText       = TextLine(infoBox.transform, "HandSizeText", "시작 패 5장", 20, TextWhite, 32);
        var attackBonusText    = TextLine(infoBox.transform, "AttackBonusText", "공격 보너스 +2", 20, TextWhite, 32);
        var starterCardsText   = TextLine(infoBox.transform, "StarterCardsText", "스타터 덱: ...", 16, TextDim, 60);
        starterCardsText.textWrappingMode = TextWrappingModes.Normal;

        var confirmBtn = Btn(panel.transform, "ConfirmBtn", "선택", BtnGreen, out var confirmBtnText);
        Anchor(confirmBtn.GetComponent<RectTransform>(), new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.18f), Vector2.zero, Vector2.zero);

        var ui = panel.AddComponent<CharacterSelectUI>();
        Bind(ui, "warriorBtn", warriorBtn);
        Bind(ui, "rogueBtn", rogueBtn);
        Bind(ui, "mageBtn", mageBtn);
        Bind(ui, "paladinBtn", paladinBtn);
        Bind(ui, "classNameText", classNameText);
        Bind(ui, "hpText", hpText);
        Bind(ui, "manaText", manaText);
        Bind(ui, "handSizeText", handSizeText);
        Bind(ui, "attackBonusText", attackBonusText);
        Bind(ui, "starterCardsText", starterCardsText);
        Bind(ui, "confirmBtn", confirmBtn);
        Bind(ui, "confirmBtnText", confirmBtnText);

        return panel;
    }

    private static GameObject BuildDungeonMapPanel(Transform canvas, GameObject tilePrefab, GameObject foodItemPrefab)
    {
        GameObject panel = FullPanel("DungeonMapPanel", canvas, PanelBg);

        GameObject topBar = NewGO("TopBar", panel.transform);
        Anchor(topBar.GetComponent<RectTransform>(), new Vector2(0, 0.92f), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        topBar.AddComponent<Image>().color = HeaderBg;
        var hlg = topBar.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 10, 10); hlg.spacing = 24;
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;

        var layerText       = TextLine(topBar.transform, "LayerText", "1계층", 22, TextWhite, 32, 120);
        var playerHpText     = TextLine(topBar.transform, "PlayerHp", "HP 80/80", 20, TextWhite, 32, 150);
        var playerHungerText = TextLine(topBar.transform, "PlayerHunger", "배고픔 100/100", 20, new Color(0.8f, 0.9f, 0.4f), 32, 190);
        var playerGoldText   = TextLine(topBar.transform, "PlayerGold", "골드 0", 20, new Color(1f, 0.85f, 0.3f), 32, 130);
        var playerRelicsText = TextLine(topBar.transform, "PlayerRelics", "유물 없음", 16, TextDim, 32, 380);

        var inventoryBtn = Btn(topBar.transform, "InventoryBtn", "인벤토리", BtnYellow, out _);
        inventoryBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 150;

        GameObject gridArea = NewGO("GridArea", panel.transform);
        Anchor(gridArea.GetComponent<RectTransform>(), new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.9f), Vector2.zero, Vector2.zero);
        var grid = gridArea.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(80, 80);
        grid.spacing = new Vector2(6, 6);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = DungeonMap.WIDTH;
        grid.childAlignment = TextAnchor.MiddleCenter;

        var ui = panel.AddComponent<DungeonMapUI>();
        Bind(ui, "tilePrefab", tilePrefab);
        Bind(ui, "gridParent", gridArea.transform);
        Bind(ui, "layerText", layerText);
        Bind(ui, "playerHpText", playerHpText);
        Bind(ui, "playerGoldText", playerGoldText);
        Bind(ui, "playerRelicsText", playerRelicsText);
        Bind(ui, "playerHungerText", playerHungerText);
        Bind(ui, "inventoryBtn", inventoryBtn);

        GameObject invPanel = BuildInventorySubPanel(panel.transform, foodItemPrefab);
        Bind(ui, "inventoryUI", invPanel.GetComponent<InventoryUI>());

        return panel;
    }

    private static GameObject BuildInventorySubPanel(Transform mapPanel, GameObject foodItemPrefab)
    {
        GameObject root = NewGO("InventoryOverlay", mapPanel);
        StretchFull(root.GetComponent<RectTransform>());
        root.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);

        GameObject box = NewGO("Box", root.transform);
        Anchor(box.GetComponent<RectTransform>(), new Vector2(0.25f, 0.15f), new Vector2(0.75f, 0.85f), Vector2.zero, Vector2.zero);
        box.AddComponent<Image>().color = PanelBg;

        var title = Text(box.transform, "Title", "인벤토리", 28, TextAlignmentOptions.Center, TextWhite);
        Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -60), new Vector2(0, -14));

        var hungerBg = Img(box.transform, "HungerBarBg", new Color(0.15f, 0.15f, 0.15f));
        Anchor(hungerBg.rectTransform, new Vector2(0.08f, 1), new Vector2(0.92f, 1), new Vector2(0, -92), new Vector2(0, -68));
        var hungerFill = Img(hungerBg.transform, "HungerBarFill", new Color(0.8f, 0.75f, 0.25f));
        StretchFull(hungerFill.rectTransform);
        hungerFill.type = Image.Type.Filled;
        hungerFill.fillMethod = Image.FillMethod.Horizontal;
        hungerFill.fillAmount = 1f;

        var hungerText = Text(box.transform, "HungerText", "배고픔 100/100", 18, TextAlignmentOptions.Center, TextWhite);
        Anchor(hungerText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -118), new Vector2(0, -94));

        GameObject listArea = NewGO("ItemList", box.transform);
        Anchor(listArea.GetComponent<RectTransform>(), new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.78f), Vector2.zero, Vector2.zero);
        var grid = listArea.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(240, 90);
        grid.spacing = new Vector2(10, 10);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        var closeBtn = Btn(box.transform, "CloseBtn", "닫기", BtnRed, out _);
        Anchor(closeBtn.GetComponent<RectTransform>(), new Vector2(0.35f, 0.02f), new Vector2(0.65f, 0.1f), Vector2.zero, Vector2.zero);

        var ui = root.AddComponent<InventoryUI>();
        Bind(ui, "hungerText", hungerText);
        Bind(ui, "hungerBarFill", hungerFill);
        Bind(ui, "itemParent", listArea.transform);
        Bind(ui, "itemPrefab", foodItemPrefab);
        Bind(ui, "panelRoot", root);
        Bind(ui, "closeBtn", closeBtn);

        root.SetActive(false);
        return root;
    }

    private static GameObject BuildBattlePanel(Transform canvas, GameObject cardPrefab, GameObject enemyPanelPrefab)
    {
        GameObject panel = FullPanel("BattlePanel", canvas, new Color(0.05f, 0.05f, 0.08f));

        GameObject top = NewGO("PlayerStatus", panel.transform);
        Anchor(top.GetComponent<RectTransform>(), new Vector2(0, 0.86f), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        top.AddComponent<Image>().color = HeaderBg;
        var hlg = top.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 10, 10); hlg.spacing = 20;
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandHeight = true;

        var playerNameText = TextLine(top.transform, "PlayerName", "플레이어", 22, TextWhite, 32, 150);

        GameObject hpBox = NewGO("HpBox", top.transform);
        hpBox.AddComponent<LayoutElement>().preferredWidth = 220;
        var playerHpSlider = Bar(hpBox.transform, "HpSlider", new Color(0.8f, 0.25f, 0.25f));
        StretchFull(playerHpSlider.GetComponent<RectTransform>());
        var playerHpText = Text(hpBox.transform, "HpText", "80/80", 16, TextAlignmentOptions.Center, Color.white);
        StretchFull(playerHpText.rectTransform);

        var playerBlockText  = TextLine(top.transform, "BlockText", "", 18, new Color(0.5f, 0.75f, 1f), 32, 90);
        var playerManaText   = TextLine(top.transform, "ManaText", "마나 ◆◆◆", 18, new Color(0.6f, 0.8f, 1f), 32, 200);
        var playerStatusText = TextLine(top.transform, "StatusText", "", 16, new Color(0.6f, 0.9f, 0.6f), 32, 240);
        var turnText          = TextLine(top.transform, "TurnText", "당신의 턴", 20, new Color(1f, 0.85f, 0.3f), 32, 150);

        GameObject enemyArea = NewGO("EnemyArea", panel.transform);
        Anchor(enemyArea.GetComponent<RectTransform>(), new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.85f), Vector2.zero, Vector2.zero);
        var enemyGrid = enemyArea.AddComponent<GridLayoutGroup>();
        enemyGrid.cellSize = new Vector2(220, 180);
        enemyGrid.spacing = new Vector2(20, 10);
        enemyGrid.childAlignment = TextAnchor.MiddleCenter;

        GameObject handArea = NewGO("HandArea", panel.transform);
        Anchor(handArea.GetComponent<RectTransform>(), new Vector2(0.02f, 0.14f), new Vector2(0.98f, 0.5f), Vector2.zero, Vector2.zero);
        var handGrid = handArea.AddComponent<GridLayoutGroup>();
        handGrid.cellSize = new Vector2(180, 240);
        handGrid.spacing = new Vector2(12, 12);
        handGrid.childAlignment = TextAnchor.MiddleCenter;

        GameObject bottom = NewGO("BottomBar", panel.transform);
        Anchor(bottom.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0.14f), Vector2.zero, Vector2.zero);
        bottom.AddComponent<Image>().color = HeaderBg;
        var bhlg = bottom.AddComponent<HorizontalLayoutGroup>();
        bhlg.padding = new RectOffset(20, 20, 10, 10); bhlg.spacing = 20;
        bhlg.childControlWidth = true; bhlg.childControlHeight = true; bhlg.childForceExpandHeight = true;

        var deckCountText    = TextLine(bottom.transform, "DeckCount", "덱 0", 18, TextDim, 32, 100);
        var discardCountText = TextLine(bottom.transform, "DiscardCount", "묘지 0", 18, TextDim, 32, 100);

        GameObject spacer = NewGO("Spacer", bottom.transform);
        spacer.AddComponent<LayoutElement>().flexibleWidth = 1;

        var endTurnBtn = Btn(bottom.transform, "EndTurnBtn", "턴 종료", BtnGreen, out _);
        endTurnBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 180;

        var ui = panel.AddComponent<BattleUI>();
        Bind(ui, "playerNameText", playerNameText);
        Bind(ui, "playerHpSlider", playerHpSlider);
        Bind(ui, "playerHpText", playerHpText);
        Bind(ui, "playerBlockText", playerBlockText);
        Bind(ui, "playerManaText", playerManaText);
        Bind(ui, "playerStatusText", playerStatusText);
        Bind(ui, "enemyParent", enemyArea.transform);
        Bind(ui, "enemyPanelPrefab", enemyPanelPrefab);
        Bind(ui, "handParent", handArea.transform);
        Bind(ui, "cardPrefab", cardPrefab);
        Bind(ui, "deckCountText", deckCountText);
        Bind(ui, "discardCountText", discardCountText);
        Bind(ui, "endTurnBtn", endTurnBtn);
        Bind(ui, "turnText", turnText);

        return panel;
    }

    private static GameObject BuildRewardPanel(Transform canvas, GameObject cardPrefab)
    {
        GameObject panel = FullPanel("RewardPanel", canvas, PanelBg);

        var titleText = Text(panel.transform, "Title", "카드를 선택하세요", 32, TextAlignmentOptions.Center, TextWhite);
        Anchor(titleText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -100), new Vector2(0, -40));

        var goldRewardText = Text(panel.transform, "GoldReward", "골드 +0", 22, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
        Anchor(goldRewardText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -140), new Vector2(0, -104));

        var foodRewardText = Text(panel.transform, "FoodReward", "", 20, TextAlignmentOptions.Center, new Color(0.7f, 0.9f, 0.5f));
        Anchor(foodRewardText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -176), new Vector2(0, -142));

        var relicRewardText = Text(panel.transform, "RelicReward", "", 20, TextAlignmentOptions.Center, new Color(0.8f, 0.7f, 1f));
        Anchor(relicRewardText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -212), new Vector2(0, -178));

        GameObject cardArea = NewGO("CardChoiceArea", panel.transform);
        Anchor(cardArea.GetComponent<RectTransform>(), new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.74f), Vector2.zero, Vector2.zero);
        var grid = cardArea.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(200, 280);
        grid.spacing = new Vector2(20, 20);
        grid.childAlignment = TextAnchor.MiddleCenter;

        var skipBtn = Btn(panel.transform, "SkipBtn", "건너뛰기", BtnRed, out _);
        Anchor(skipBtn.GetComponent<RectTransform>(), new Vector2(0.4f, 0.06f), new Vector2(0.6f, 0.15f), Vector2.zero, Vector2.zero);

        var ui = panel.AddComponent<RewardUI>();
        Bind(ui, "goldRewardText", goldRewardText);
        Bind(ui, "titleText", titleText);
        Bind(ui, "foodRewardText", foodRewardText);
        Bind(ui, "relicRewardText", relicRewardText);
        Bind(ui, "cardChoiceParent", cardArea.transform);
        Bind(ui, "cardChoicePrefab", cardPrefab);
        Bind(ui, "skipBtn", skipBtn);

        return panel;
    }

    private static GameObject BuildRestPanel(Transform canvas)
    {
        GameObject panel = FullPanel("RestPanel", canvas, PanelBg);

        var title = Text(panel.transform, "Title", "모닥불에서 휴식", 32, TextAlignmentOptions.Center, TextWhite);
        Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -140), new Vector2(0, -60));

        var playerHpText = Text(panel.transform, "PlayerHp", "현재 HP 80/80", 24, TextAlignmentOptions.Center, TextWhite);
        Anchor(playerHpText.rectTransform, new Vector2(0, 0.55f), new Vector2(1, 0.55f), new Vector2(0, -20), new Vector2(0, 20));

        var healPreviewText = Text(panel.transform, "HealPreview", "휴식하면 HP +0", 22, TextAlignmentOptions.Center, new Color(0.5f, 0.9f, 0.5f));
        Anchor(healPreviewText.rectTransform, new Vector2(0, 0.46f), new Vector2(1, 0.46f), new Vector2(0, -20), new Vector2(0, 20));

        var hungerCostText = Text(panel.transform, "HungerCost", "", 18, TextAlignmentOptions.Center, new Color(0.9f, 0.7f, 0.3f));
        Anchor(hungerCostText.rectTransform, new Vector2(0, 0.38f), new Vector2(1, 0.38f), new Vector2(0, -20), new Vector2(0, 20));

        var restBtn = Btn(panel.transform, "RestBtn", "휴식하기", BtnGreen, out _);
        Anchor(restBtn.GetComponent<RectTransform>(), new Vector2(0.35f, 0.2f), new Vector2(0.65f, 0.3f), Vector2.zero, Vector2.zero);

        var leaveBtn = Btn(panel.transform, "LeaveBtn", "그냥 지나가기", BtnRed, out _);
        Anchor(leaveBtn.GetComponent<RectTransform>(), new Vector2(0.35f, 0.06f), new Vector2(0.65f, 0.16f), Vector2.zero, Vector2.zero);

        var ui = panel.AddComponent<RestUI>();
        Bind(ui, "playerHpText", playerHpText);
        Bind(ui, "healPreviewText", healPreviewText);
        Bind(ui, "hungerCostText", hungerCostText);
        Bind(ui, "restBtn", restBtn);
        Bind(ui, "leaveBtn", leaveBtn);

        return panel;
    }

    private static GameObject BuildShopPanel(Transform canvas, GameObject shopItemPrefab, GameObject cardPrefab)
    {
        GameObject panel = FullPanel("ShopPanel", canvas, PanelBg);

        var shopTitleText = Text(panel.transform, "Title", "상점", 32, TextAlignmentOptions.Center, TextWhite);
        Anchor(shopTitleText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -90), new Vector2(0, -40));

        var goldText = Text(panel.transform, "GoldText", "보유 골드 0", 22, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
        Anchor(goldText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -124), new Vector2(0, -92));

        GameObject itemArea = NewGO("ItemArea", panel.transform);
        Anchor(itemArea.GetComponent<RectTransform>(), new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.8f), Vector2.zero, Vector2.zero);
        var grid = itemArea.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(260, 140);
        grid.spacing = new Vector2(16, 16);

        var leaveBtn = Btn(panel.transform, "LeaveBtn", "나가기", BtnRed, out _);
        Anchor(leaveBtn.GetComponent<RectTransform>(), new Vector2(0.4f, 0.03f), new Vector2(0.6f, 0.12f), Vector2.zero, Vector2.zero);

        GameObject removeCardPanel = NewGO("RemoveCardPanel", panel.transform);
        StretchFull(removeCardPanel.GetComponent<RectTransform>());
        removeCardPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);

        var removeCardHintText = Text(removeCardPanel.transform, "Hint", "제거할 카드를 선택하세요", 24, TextAlignmentOptions.Center, TextWhite);
        Anchor(removeCardHintText.rectTransform, new Vector2(0, 0.85f), new Vector2(1, 0.95f), Vector2.zero, Vector2.zero);

        GameObject removeCardListArea = NewGO("RemoveCardList", removeCardPanel.transform);
        Anchor(removeCardListArea.GetComponent<RectTransform>(), new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.8f), Vector2.zero, Vector2.zero);
        var removeGrid = removeCardListArea.AddComponent<GridLayoutGroup>();
        removeGrid.cellSize = new Vector2(180, 240);
        removeGrid.spacing = new Vector2(14, 14);

        removeCardPanel.SetActive(false);

        var ui = panel.AddComponent<ShopUI>();
        Bind(ui, "goldText", goldText);
        Bind(ui, "shopTitleText", shopTitleText);
        Bind(ui, "itemParent", itemArea.transform);
        Bind(ui, "shopItemPrefab", shopItemPrefab);
        Bind(ui, "removeCardPanel", removeCardPanel);
        Bind(ui, "removeCardList", removeCardListArea.transform);
        Bind(ui, "removeCardEntryPrefab", cardPrefab);
        Bind(ui, "removeCardHintText", removeCardHintText);
        Bind(ui, "leaveBtn", leaveBtn);

        return panel;
    }

    private static GameObject BuildShrinePanel(Transform canvas, GameObject cardPrefab)
    {
        GameObject panel = FullPanel("ShrinePanel", canvas, PanelBg);

        var titleText = Text(panel.transform, "Title", "성소 접촉 — 카드 제작", 30, TextAlignmentOptions.Center, TextWhite);
        Anchor(titleText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -110), new Vector2(0, -50));

        Transform attackSlot  = BuildShrineBranchColumn(panel.transform, "AttackColumn",  "공격", new Vector2(0.04f, 0.15f), new Vector2(0.32f, 0.78f));
        Transform utilitySlot = BuildShrineBranchColumn(panel.transform, "UtilityColumn", "유틸", new Vector2(0.36f, 0.15f), new Vector2(0.64f, 0.78f));
        Transform defenseSlot = BuildShrineBranchColumn(panel.transform, "DefenseColumn", "방어", new Vector2(0.68f, 0.15f), new Vector2(0.96f, 0.78f));

        var ui = panel.AddComponent<ShrineUI>();
        Bind(ui, "titleText", titleText);
        Bind(ui, "attackSlot", attackSlot);
        Bind(ui, "utilitySlot", utilitySlot);
        Bind(ui, "defenseSlot", defenseSlot);
        Bind(ui, "cardChoicePrefab", cardPrefab);

        return panel;
    }

    // 성소 패널의 분기 1개 열(라벨 + 카드 슬롯)을 만든다
    private static Transform BuildShrineBranchColumn(Transform parent, string name, string label, Vector2 min, Vector2 max)
    {
        GameObject column = NewGO(name, parent);
        Anchor(column.GetComponent<RectTransform>(), min, max, Vector2.zero, Vector2.zero);

        var labelText = Text(column.transform, "Label", label, 26, TextAlignmentOptions.Center, new Color(0.85f, 0.8f, 1f));
        Anchor(labelText.rectTransform, new Vector2(0, 0.88f), new Vector2(1, 1f), Vector2.zero, Vector2.zero);

        GameObject slot = NewGO("Slot", column.transform);
        Anchor(slot.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0.86f), Vector2.zero, Vector2.zero);
        var grid = slot.AddComponent<GridLayoutGroup>();
        grid.cellSize        = new Vector2(200, 280);
        grid.childAlignment  = TextAnchor.MiddleCenter;

        return slot.transform;
    }

    private static GameObject BuildGameOverPanel(Transform canvas)
    {
        GameObject panel = FullPanel("GameOverPanel", canvas, new Color(0.12f, 0.04f, 0.04f));

        var title = Text(panel.transform, "Title", "패배...", 42, TextAlignmentOptions.Center, new Color(0.9f, 0.3f, 0.3f));
        Anchor(title.rectTransform, new Vector2(0, 0.65f), new Vector2(1, 0.8f), Vector2.zero, Vector2.zero);

        var summaryText = Text(panel.transform, "Summary", "", 22, TextAlignmentOptions.Center, TextWhite);
        Anchor(summaryText.rectTransform, new Vector2(0, 0.45f), new Vector2(1, 0.6f), Vector2.zero, Vector2.zero);

        var retryBtn = Btn(panel.transform, "RetryBtn", "다시 시작", BtnGreen, out _);
        Anchor(retryBtn.GetComponent<RectTransform>(), new Vector2(0.35f, 0.28f), new Vector2(0.65f, 0.38f), Vector2.zero, Vector2.zero);

        var titleBtn = Btn(panel.transform, "TitleBtn", "타이틀로", BtnBlue, out _);
        Anchor(titleBtn.GetComponent<RectTransform>(), new Vector2(0.35f, 0.14f), new Vector2(0.65f, 0.24f), Vector2.zero, Vector2.zero);

        var ui = panel.AddComponent<GameOverUI>();
        Bind(ui, "summaryText", summaryText);
        Bind(ui, "retryBtn", retryBtn);
        Bind(ui, "titleBtn", titleBtn);

        return panel;
    }

    private static GameObject BuildVictoryPanel(Transform canvas)
    {
        GameObject panel = FullPanel("VictoryPanel", canvas, new Color(0.06f, 0.1f, 0.06f));

        var title = Text(panel.transform, "Title", "승리!", 42, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
        Anchor(title.rectTransform, new Vector2(0, 0.65f), new Vector2(1, 0.8f), Vector2.zero, Vector2.zero);

        var summaryText = Text(panel.transform, "Summary", "", 22, TextAlignmentOptions.Center, TextWhite);
        Anchor(summaryText.rectTransform, new Vector2(0, 0.45f), new Vector2(1, 0.6f), Vector2.zero, Vector2.zero);

        var retryBtn = Btn(panel.transform, "RetryBtn", "다시 시작", BtnGreen, out _);
        Anchor(retryBtn.GetComponent<RectTransform>(), new Vector2(0.35f, 0.28f), new Vector2(0.65f, 0.38f), Vector2.zero, Vector2.zero);

        var titleBtn = Btn(panel.transform, "TitleBtn", "타이틀로", BtnBlue, out _);
        Anchor(titleBtn.GetComponent<RectTransform>(), new Vector2(0.35f, 0.14f), new Vector2(0.65f, 0.24f), Vector2.zero, Vector2.zero);

        var ui = panel.AddComponent<VictoryUI>();
        Bind(ui, "summaryText", summaryText);
        Bind(ui, "retryBtn", retryBtn);
        Bind(ui, "titleBtn", titleBtn);

        return panel;
    }

    // ─────────────────────────────────────────────────────────
    // 기초 UI 헬퍼
    // ─────────────────────────────────────────────────────────
    private static GameObject NewGO(string name, Transform parent = null)
    {
        var go = new GameObject(name, typeof(RectTransform));
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject FullPanel(string name, Transform parent, Color bgColor)
    {
        GameObject go = NewGO(name, parent);
        StretchFull(go.GetComponent<RectTransform>());
        go.AddComponent<Image>().color = bgColor;
        go.AddComponent<CanvasGroup>();
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    private static TextMeshProUGUI Text(Transform parent, string name, string text, float size, TextAlignmentOptions align, Color color)
    {
        GameObject go = NewGO(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        StretchFull(go.GetComponent<RectTransform>());
        return tmp;
    }

    // LayoutGroup 안에 들어가는 한 줄짜리 텍스트 (preferredWidth <= 0 이면 flexibleWidth 사용)
    private static TextMeshProUGUI TextLine(Transform parent, string name, string text, float size, Color color, float preferredHeight, float preferredWidth = -1)
    {
        GameObject go = NewGO(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = preferredHeight;
        if (preferredWidth > 0) le.preferredWidth = preferredWidth;
        else le.flexibleWidth = 1;
        return tmp;
    }

    private static Image Img(Transform parent, string name, Color color)
    {
        GameObject go = NewGO(name, parent);
        var img = go.AddComponent<Image>();
        img.color = color;
        StretchFull(go.GetComponent<RectTransform>());
        return img;
    }

    private static Button Btn(Transform parent, string name, string label, Color color, out TextMeshProUGUI labelText)
    {
        GameObject go = NewGO(name, parent);
        go.AddComponent<Image>().color = color;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
        colors.pressedColor     = Color.Lerp(color, Color.black, 0.2f);
        colors.disabledColor    = new Color(color.r, color.g, color.b, 0.35f);
        btn.colors = colors;

        labelText = Text(go.transform, "Label", label, 22, TextAlignmentOptions.Center, Color.white);
        return btn;
    }

    private static Slider Bar(Transform parent, string name, Color fillColor)
    {
        GameObject go = NewGO(name, parent);
        var slider = go.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;

        var bg = Img(go.transform, "Background", new Color(0.15f, 0.15f, 0.18f));
        bg.raycastTarget = false;

        GameObject fillArea = NewGO("Fill Area", go.transform);
        StretchFull(fillArea.GetComponent<RectTransform>());

        var fill = Img(fillArea.transform, "Fill", fillColor);
        fill.raycastTarget = false;
        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(1, 1);

        slider.fillRect = fillRt;
        slider.targetGraphic = fill;
        slider.minValue = 0; slider.maxValue = 1; slider.value = 1;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private static GameObject SaveTemplateAsPrefab(GameObject instance, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return prefab;
    }

    // private [SerializeField] 필드에 값을 연결 (Inspector에서 드래그하는 것과 동일한 효과)
    private static void Bind(Object component, string fieldName, Object value)
    {
        var so = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogError($"[SceneSetup] '{component.GetType().Name}' 에서 필드 '{fieldName}' 를 찾을 수 없습니다.");
            return;
        }
        prop.objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }
}
