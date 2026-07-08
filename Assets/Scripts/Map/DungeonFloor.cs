using System.Collections.Generic;
using System.Linq;

public enum TileKind { Wall, Floor }
public enum EnemyAiState { Idle, Chasing }

// 방 하나 - 사각형 영역 + 조우 타입(TileType 재사용: Empty=시작방/평범한 통로, NormalEnemy/GroupEnemy/EliteEnemy/Boss/Rest/Shop/Shrine).
public class RoomInfo
{
    public int      id;
    public int      x, y, w, h;
    public TileType roomType  = TileType.Empty;
    public bool     isCleared = false;

    public int CenterX => x + w / 2;
    public int CenterY => y + h / 2;

    public bool Contains(int px, int py) => px >= x && px < x + w && py >= y && py < y + h;
}

// 그리드 위에 실체로 존재하는 적 - roomId가 같으면 한 번에 같이 전투에 들어간다(집단 조우).
public class EnemySpawn
{
    public int           id;
    public int           roomId;
    public string        enemyTemplateId;
    public int           x, y;
    public bool          isDead;
    public EnemyAiState  state = EnemyAiState.Idle;
}

// 실제로 걸어다니는 픽셀던전 스타일 2D 타일 던전 한 층.
// 예전 DungeonMap(노드 그래프)을 대체 - 방/복도로 이루어진 실제 그리드와, 그 위의 적 실체를 담는다.
public class DungeonFloor
{
    public int Layer  { get; }
    public int Width   { get; }
    public int Height  { get; }

    public TileKind[,] Tiles;
    public bool[,]      Visited;
    public bool[,]       Visible;

    public List<RoomInfo>   Rooms   = new List<RoomInfo>();
    public List<EnemySpawn> Enemies = new List<EnemySpawn>();

    public int PlayerX, PlayerY;

    public DungeonFloor(int layer, int width, int height)
    {
        Layer   = layer;
        Width   = width;
        Height  = height;
        Tiles   = new TileKind[width, height];
        Visited = new bool[width, height];
        Visible = new bool[width, height];
    }

    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public bool IsWalkable(int x, int y) => InBounds(x, y) && Tiles[x, y] == TileKind.Floor;

    public RoomInfo RoomAt(int x, int y) => Rooms.FirstOrDefault(r => r.Contains(x, y));

    public EnemySpawn EnemyAt(int x, int y) => Enemies.FirstOrDefault(e => !e.isDead && e.x == x && e.y == y);

    public IEnumerable<EnemySpawn> AliveEnemiesInRoom(int roomId) => Enemies.Where(e => !e.isDead && e.roomId == roomId);

    // 플레이어 주변 반경을 시야로 밝히고 방문 처리 (원형 근사, 실제 시야선 계산은 안 함 - 이 규모에선 과함).
    public void RevealAround(int cx, int cy, int radius)
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                Visible[x, y] = false;

        int r2 = radius * radius;
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy > r2) continue;
                int x = cx + dx, y = cy + dy;
                if (!InBounds(x, y)) continue;
                Visible[x, y] = true;
                Visited[x, y] = true;
            }
        }
    }
}
