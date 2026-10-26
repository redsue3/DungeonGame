using UnityEditor;

// Assets/Resources/Art/ 에 넣은 그림은 자동으로 Sprite로 임포트한다 - 파일만 넣으면 바로 게임에 쓰이게.
public class ArtImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Replace('\\', '/').StartsWith("Assets/Resources/Art/")) return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType      = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled    = false;
    }
}
