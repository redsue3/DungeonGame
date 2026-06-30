public class DungeonMap
{
    public const int WIDTH  = 9;
    public const int HEIGHT = 7;

    private readonly MapTile[,] tiles = new MapTile[WIDTH, HEIGHT];

    public int PlayerX { get; private set; }
    public int PlayerY { get; private set; }
    public int Layer   { get; }

    public DungeonMap(int layer)
    {
        Layer = layer;
        for (int x = 0; x < WIDTH;  x++)
        for (int y = 0; y < HEIGHT; y++)
            tiles[x, y] = new MapTile();
    }

    public MapTile this[int x, int y] => tiles[x, y];

    public MapTile PlayerTile => tiles[PlayerX, PlayerY];

    public bool InBounds(int x, int y) => x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT;

    public void SetTile(int x, int y, MapTile tile) => tiles[x, y] = tile;

    public void SetPlayer(int x, int y) { PlayerX = x; PlayerY = y; }

    // 이동 시도. 이동 불가 시 null 반환
    public MapTile TryMove(int dx, int dy)
    {
        int nx = PlayerX + dx;
        int ny = PlayerY + dy;
        if (!InBounds(nx, ny)) return null;
        PlayerX = nx;
        PlayerY = ny;
        return PlayerTile;
    }
}
