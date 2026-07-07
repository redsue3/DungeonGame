using System.Collections.Generic;

// 던전 맵의 방 하나. floor(층)와 층 내 가로 위치 x(0~1)로 UI에 배치되고,
// next에 담긴 id들로 다음 층 노드와 연결된다 (분기/합류형 맵).
public class MapNode
{
    public int      id;
    public int      floor;
    public float    x;
    public TileType type      = TileType.Empty;
    public bool     isCleared = false;
    public string[] enemyIds  = new string[0];
    public List<int> next     = new List<int>();

    public bool HasEnemy =>
        !isCleared && (
            type == TileType.NormalEnemy ||
            type == TileType.GroupEnemy  ||
            type == TileType.EliteEnemy  ||
            type == TileType.Boss
        );

    public bool IsGroup => type == TileType.GroupEnemy;
}
