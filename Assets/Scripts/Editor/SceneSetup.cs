using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class SceneSetup
{
    [MenuItem("DungeonGame/씬 자동 세팅")]
    public static void BuildScene()
    {
        // ── 카메라 ──
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.gameObject.AddComponent<AudioListener>();
            cam.gameObject.tag = "MainCamera";
        }
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
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        UIManager uiManager = canvasGO.AddComponent<UIManager>();

        // ── EventSystem ──
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── 패널 생성 ──
        var panels = new (string name, System.Type uiType)[]
        {
            ("CharacterSelectPanel", typeof(CharacterSelectUI)),
            ("DungeonMapPanel",      typeof(DungeonMapUI)),
            ("BattlePanel",          typeof(BattleUI)),
            ("RewardPanel",          typeof(RewardUI)),
            ("RestPanel",            typeof(RestUI)),
            ("ShopPanel",            typeof(ShopUI)),
            ("GameOverPanel",        typeof(GameOverUI)),
            ("VictoryPanel",         typeof(VictoryUI)),
        };

        GameObject[] panelObjects = new GameObject[panels.Length];
        for (int i = 0; i < panels.Length; i++)
        {
            GameObject p = MakeFullPanel(panels[i].name, canvasGO.transform);
            p.AddComponent(panels[i].uiType);
            p.SetActive(i == 0); // CharacterSelect만 켜두기
            panelObjects[i] = p;
        }

        // UIManager에 패널 연결
        var so = new SerializedObject(uiManager);
        so.FindProperty("characterSelectPanel").objectReferenceValue = panelObjects[0];
        so.FindProperty("dungeonMapPanel").objectReferenceValue      = panelObjects[1];
        so.FindProperty("battlePanel").objectReferenceValue          = panelObjects[2];
        so.FindProperty("rewardPanel").objectReferenceValue          = panelObjects[3];
        so.FindProperty("restPanel").objectReferenceValue            = panelObjects[4];
        so.FindProperty("shopPanel").objectReferenceValue            = panelObjects[5];
        so.FindProperty("gameOverPanel").objectReferenceValue        = panelObjects[6];
        so.FindProperty("victoryPanel").objectReferenceValue         = panelObjects[7];
        so.ApplyModifiedProperties();

        // ── 씬 저장 ──
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(),
            "Assets/Scenes/GameScene.unity");

        Debug.Log("✅ 씬 세팅 완료! Inspector에서 각 패널 UI 연결 후 Play 하세요.");
    }

    static GameObject MakeFullPanel(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<CanvasGroup>();
        return go;
    }
}
