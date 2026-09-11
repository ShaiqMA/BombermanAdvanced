using ITalent.Games.Tiled;

namespace ITalent.Games.TopDownGame;

internal class MapLayer() : MapRenderLayer(MapTileSettings)
{
  private const byte SAND = 6 * 12;
  private const byte CASTLE = TileID.CASTLE;
  private const byte WALL = TileID.WALL;

  public void GenerateMap()
  {
    var rnd = new Random((int)DateTime.Now.Ticks);

    // create shadow map with logical information

    var grid = new ByteGrid(Tiles.Size.X, Tiles.Size.Y);

    // fill entire map with basic sand
    grid.Fill(SAND);
    Tiles.Fill(SAND);

    // random castle area's all around, staying clear of the edge
    for (int i = 0; i < 500; ++i)
    {
      int w = rnd.Next(4, 12);
      int h = rnd.Next(4, 12);
      int x = rnd.Next(2, Tiles.Size.X - w - 4);
      int y = rnd.Next(2, Tiles.Size.Y - h - 4);
      grid.Fill(x, y, w, h, CASTLE);
    }

    // post process: detect castle edges
    for (int y = 1; y < Tiles.Size.Y - 2; ++y)
      for (int x = 1; x < Tiles.Size.X - 2; ++x)
      {
        var mid = grid[x, y] == CASTLE;
        var up = grid[x, y + 1] == CASTLE;
        var down = grid[x, y - 1] == CASTLE;
        var left = grid[x - 1, y] == CASTLE;
        var right = grid[x + 1, y] == CASTLE;
        var tl = grid[x - 1, y + 1] == CASTLE;
        var tr = grid[x + 1, y + 1] == CASTLE;
        var bl = grid[x - 1, y - 1] == CASTLE;
        var br = grid[x + 1, y - 1] == CASTLE;
        var downdown = y < 2 ? false : grid[x, y - 2] == CASTLE;

        if (mid)
        {
          if (!down && !left) Tiles.SetTile(x, y, 6 * 12 + 9);
          else if (!down && !right) Tiles.SetTile(x, y, 6 * 12 + 11);
          else if (!up && !left) Tiles.SetTile(x, y, 10 * 12 + 4);
          else if (!up && !right) Tiles.SetTile(x, y, 10 * 12 + 5);
          else if (!down) Tiles.SetTile(x, y, WALL);
          else if (!up) Tiles.SetTile(x, y, 8 * 12 + 2);
          else if (!left) Tiles.SetTile(x, y, 9 * 12 + 3);
          else if (!right) Tiles.SetTile(x, y, 9 * 12 + 1);
          else if (!tl) Tiles.SetTile(x, y, 8 * 12 + 3);
          else if (!tr) Tiles.SetTile(x, y, 8 * 12 + 1);
          else if (!bl) Tiles.SetTile(x, y, 9 * 12 + 3);
          else if (!br) Tiles.SetTile(x, y, 9 * 12 + 1);
          else if (down && !downdown) Tiles.SetTile(x, y, 10 * 12 + 2);
          else Tiles.SetTile(x, y, CASTLE);
        }
        else
        {
          if (up) Tiles.SetTile(x, y, 6 * 12 + 2);
        }
      }

    // post-process: randomize tile variations
    int[] wallOptions = [4 + 12 * 8, 5 + 12 * 8, 9 + 12 * 10, WALL, WALL, WALL, WALL, WALL, WALL, WALL, WALL, WALL];
    int[] sandOptions = [90, 73, 73, SAND, SAND, SAND, SAND, SAND, SAND, SAND, SAND, SAND, SAND, SAND];
    int[] castleOptions = [9 * 12, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE];
    for (int y = 0; y < Tiles.Size.Y; ++y)
      for (int x = 0; x < Tiles.Size.X; ++x)
      {
        if (Tiles.GetTile(x, y) == SAND)
          Tiles.SetTile(x, y, sandOptions[rnd.Next() % sandOptions.Length]);
        if (Tiles.GetTile(x, y) == CASTLE)
          Tiles.SetTile(x, y, castleOptions[rnd.Next() % castleOptions.Length]);
        if (Tiles.GetTile(x, y) == WALL)
          Tiles.SetTile(x, y, wallOptions[rnd.Next() % wallOptions.Length]);
      }
  }

  public static MapRenderLayerSettings MapTileSettings => new()
  {
    TilesetPath = "Resources/tilemap_packed.png",
    TilesetSize = new(12, 12),
    TilemapSize = new(256, 256)
  };
}
