public enum TileType
{
    Empty,
    NormalEnemy,   // 일반 조우 (적 1마리)
    GroupEnemy,    // 집단 조우 (적 2~3마리)
    EliteEnemy,    // 엘리트 조우
    Boss,          // 보스
    Rest,          // 휴식
    Shop,          // 상점
}

public class MapTile
{
    public TileType  type      = TileType.Empty;
    public bool      isCleared = false;
    public string[]  enemyIds  = new string[0];

    public bool HasEnemy =>
        !isCleared && (
            type == TileType.NormalEnemy ||
            type == TileType.GroupEnemy  ||
            type == TileType.EliteEnemy  ||
            type == TileType.Boss
        );

    public bool IsGroup => type == TileType.GroupEnemy;
}
