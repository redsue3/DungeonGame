using System.Collections.Generic;
using UnityEngine;

// Assets/Resources/Art/ 아래에 정해진 이름으로 그림 파일을 넣으면 게임이 자동으로 불러다 쓴다.
// 파일이 없으면 null을 돌려주고, 호출하는 쪽은 지금까지처럼 색 박스/글자로 그린다.
// 이름 규칙은 Assets/Resources/Art/README.md 참고.
public static class ArtLibrary
{
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite Get(string path)
    {
        if (cache.TryGetValue(path, out Sprite cached)) return cached;

        Sprite sprite = Resources.Load<Sprite>("Art/" + path);
        if (sprite == null)
        {
            // 임포트 설정이 Sprite가 아닌 채로 들어온 그림(Texture)도 쓸 수 있게 한다
            Texture2D tex = Resources.Load<Texture2D>("Art/" + path);
            if (tex != null)
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        cache[path] = sprite;
        return sprite;
    }

    // 계층별 그림(map_2 등)이 있으면 그걸, 없으면 공용 그림(map)을 쓴다
    public static Sprite Background(string screen, int layer)
    {
        Sprite perLayer = Get($"Backgrounds/{screen}_{layer}");
        return perLayer != null ? perLayer : Get($"Backgrounds/{screen}");
    }

    public static Sprite Character(CharacterClass cls) => Get($"Characters/{cls.ToString().ToLowerInvariant()}");
    public static Sprite Enemy(string enemyId)         => string.IsNullOrEmpty(enemyId) ? null : Get($"Enemies/{enemyId}");
    public static Sprite Card(string cardId)           => Get($"Cards/{cardId}");
}
