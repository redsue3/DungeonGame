using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;

// NotoSansKR SDF를 Dynamic(런타임에 10MB짜리 원본 폰트 파일을 그대로 들고 있어야 함) 대신
// Static 아틀라스로 다시 구워서, 코드에 실제로 쓰이는 한글 글자만 텍스처에 미리 박아넣고
// 런타임 소스 폰트 참조를 없애 빌드 용량을 줄인다.
//
// 주의: 여기 KnownCharacters에 없는 새 한글 텍스트를 나중에 추가하면 그 글자만 안 보일 수 있음 -
// 그럴 땐 이 메뉴를 다시 실행해서 문자셋을 갱신해야 함 (문자셋은 Assets/Scripts 전체를 긁어서 자동 수집됨).
public static class KoreanFontBaker
{
    // 기존(Dynamic, 10MB 원본 폰트를 런타임까지 들고 있어야 하는) 에셋은 그대로 두고,
    // 새 Static 에셋을 별도 경로에 만들어서 TMP Settings의 폴백 목록만 새 걸로 바꿔치기한다.
    // 기존 에셋 파일을 직접 고치면 GUID/서브에셋 배선이 꼬일 위험이 있어서(TMP는 예전에 이 프로젝트에서
    // 배치모드 임포트 문제로 크래시를 낸 전례가 있음) 더 안전한 쪽을 택함 - 바꿔치기 후 아무도 안 쓰게 된
    // 기존 Dynamic 에셋과 원본 ttf는 빌드에 안 딸려가므로 그대로 둬도 용량 문제는 해결됨.
    private const string OldFontAssetPath = "Assets/Fonts/NotoSansKR SDF.asset";
    private const string NewFontAssetPath = "Assets/Fonts/NotoSansKR SDF Static.asset";
    private const string SourceFontPath   = "Assets/Fonts/NotoSansKR.ttf";
    private const string TmpSettingsPath  = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    [MenuItem("DungeonGame/한글 폰트 정적 베이크")]
    public static void Bake()
    {
        string characters = CollectKoreanCharacters();
        Debug.Log($"[KoreanFontBaker] Assets/Scripts에서 한글 {characters.Length}자 수집");

        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (sourceFont == null)
        {
            Debug.LogError($"[KoreanFontBaker] 소스 폰트를 못 찾음: {SourceFontPath}");
            return;
        }

        // Dynamic으로 새로 만들어서 필요한 글자를 전부 채운 뒤 Static으로 전환한다
        // (TryAddCharacters 자체는 Dynamic 상태여야 새 글리프를 그려넣을 수 있음).
        // 한글 음절은 획이 많아서 samplingPointSize 90/1024 아틀라스로는 한 페이지에 다 안 들어가고
        // 여러 장으로 쪼개져서(장당 ~1MB) 오히려 원본 폰트보다 커지는 문제가 있었음 -
        // 실제 UI에서 쓰는 글자 크기(16~40pt)엔 과한 해상도였던 것도 한몫해서 샘플링을 낮추고
        // 아틀라스를 한 장으로 넉넉하게 키워서 전부 한 페이지에 들어가게 함.
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            samplingPointSize: 48,
            atlasPadding: 5,
            renderMode: GlyphRenderMode.SDFAA,
            atlasWidth: 2048,
            atlasHeight: 2048,
            atlasPopulationMode: AtlasPopulationMode.Dynamic,
            enableMultiAtlasSupport: false);

        if (fontAsset == null)
        {
            Debug.LogError("[KoreanFontBaker] TMP_FontAsset.CreateFontAsset 실패");
            return;
        }

        bool allAdded = fontAsset.TryAddCharacters(characters, out string missing);
        if (!allAdded)
            Debug.LogWarning($"[KoreanFontBaker] 아틀라스에 못 넣은 글자: {missing}");

        // 런타임에 새 글리프를 추가로 그리지 않도록 Static으로 고정 (public 프로퍼티라 바로 설정 가능)
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

        // 먼저 텍스처/머티리얼까지 전부 온전한 상태로 디스크에 저장부터 하고,
        // 그다음에 SerializedObject로 소스 폰트 참조를 끊는다 (저장 전에 끊으면 atlasTextures가
        // 같이 날아가는 문제가 있었음 - 아직 AssetDatabase에 등록 안 된 메모리상 객체를
        // SerializedObject로 건드리면 내부 상태가 꼬이는 듯).
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NewFontAssetPath) != null)
            AssetDatabase.DeleteAsset(NewFontAssetPath);

        AssetDatabase.CreateAsset(fontAsset, NewFontAssetPath);
        foreach (var tex in fontAsset.atlasTextures)
        {
            if (tex == null) continue; // enableMultiAtlasSupport라 실제로 안 쓰인 여분 페이지 슬롯이 null로 남아있을 수 있음
            AssetDatabase.AddObjectToAsset(tex, fontAsset);
        }
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var savedAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NewFontAssetPath);
        var so = new SerializedObject(savedAsset);
        so.FindProperty("m_SourceFontFile").objectReferenceValue = null;
        so.FindProperty("m_ClearDynamicDataOnBuild").boolValue = false;
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();

        if (!SwapFallbackReference(savedAsset))
        {
            Debug.LogError("[KoreanFontBaker] TMP Settings 폴백 목록에서 기존 NotoSansKR SDF를 못 찾음 - 수동으로 교체 필요");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[KoreanFontBaker] 완료 - {NewFontAssetPath} (Static)로 폴백 교체됨. 기존 {OldFontAssetPath}는 더는 안 쓰여서 빌드에서 자동으로 빠짐.");
    }

    private static bool SwapFallbackReference(TMP_FontAsset newAsset)
    {
        var settingsObj = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);
        if (settingsObj == null) return false;

        var settingsSo = new SerializedObject(settingsObj);
        SerializedProperty list = settingsSo.FindProperty("m_fallbackFontAssets");
        if (list == null) return false;

        bool replaced = false;
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty elem = list.GetArrayElementAtIndex(i);
            Object current = elem.objectReferenceValue;
            // 재실행하면 이 메뉴가 기존 Static 에셋을 지웠다 다시 만들기 때문에, 그 사이 이 슬롯은
            // 깨진 참조(null)로 보인다 - 그 슬롯도 우리 것으로 간주하고 새 에셋으로 교체한다.
            if (current == null || current.name == "NotoSansKR SDF" || current.name == "NotoSansKR SDF Static")
            {
                elem.objectReferenceValue = newAsset;
                replaced = true;
            }
        }

        if (replaced) settingsSo.ApplyModifiedProperties();
        return replaced;
    }

    private static string CollectKoreanCharacters()
    {
        var unique = new System.Collections.Generic.HashSet<char>();
        string[] guids = AssetDatabase.FindAssets("t:Script", new[] { "Assets/Scripts" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("/Editor/")) continue; // 에디터 툴 소스는 실제 게임 텍스트가 아님

            string text = System.IO.File.ReadAllText(path);
            foreach (char c in text)
            {
                // 한글 음절(가-힣) + 자모 + 호환 자모 + CJK 문장부호
                if ((c >= '가' && c <= '힣') ||
                    (c >= 'ᄀ' && c <= 'ᇿ') ||
                    (c >= '㄰' && c <= '㆏') ||
                    (c >= '　' && c <= '〿'))
                {
                    unique.Add(c);
                }
            }
        }

        var sb = new System.Text.StringBuilder();
        foreach (char c in unique) sb.Append(c);
        return sb.ToString();
    }
}
