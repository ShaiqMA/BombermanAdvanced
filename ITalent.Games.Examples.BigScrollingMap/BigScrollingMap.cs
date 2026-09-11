using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ITalent.Games.Tiled;
using TextWriter = ITalent.Games.Tiled.TextWriter;
using OpenTK.Mathematics;

namespace ITalent.Games.Examples.BigScrollingMap;

internal class BigScrollingMap(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private float tileSize = 64f; // width and height of a tile on screen, in pixels
  private const float PANSPEED = 1f; // Cursor keys
  private const float ZOOMSPEED = 0.4f; // PgUp / PgDn, <0, 1>

  private const byte KNIGHT = 25;
  private const byte SAND = 6 * 12;
  private const byte CASTLE = 10 * 12;
  private const byte WALL = 7 * 12 + 4;

  private MapRenderLayer _blockLayer = new(MapTileSettings);
  private MapRenderLayer _textLayer = new(TextTileSettings);
  private TextWriter _textWriter;

  protected override void OnLoad()
  {
    _textWriter = new(_textLayer.Tiles);

    AddLayer(_blockLayer);
    AddLayer(_textLayer);

    base.OnLoad();

    GL.ClearColor(new Color4(234, 165, 108, 255));
    GenerateMap();
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);

    ZoomAndPan();
    UpdateText();
  }

  private void GenerateMap()
  {
    var rnd = new Random(12345);

    // create shadow map with logical information

    var dest = _blockLayer.Tiles;
    var grid = new ByteGrid(dest.Size.X, dest.Size.Y);

    // fill entire map with basic sand
    grid.Fill(SAND);
    dest.Fill(SAND);

    // random castle area's all around, staying clear of the edge
    for (int i = 0; i < 500; ++i)
    {
      int w = rnd.Next(4, 12);
      int h = rnd.Next(4, 12);
      int x = rnd.Next(2, dest.Size.X - w - 4);
      int y = rnd.Next(2, dest.Size.Y - h - 4);
      grid.Fill(x, y, w, h, CASTLE);
    }

    // post process: detect castle edges
    for (int y = 1; y < dest.Size.Y - 2; ++y)
      for (int x = 1; x < dest.Size.X - 2; ++x)
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

        if (mid)
        {
          if (!down && !left) dest.SetTile(x, y, 6 * 12 + 9);
          else if (!down && !right) dest.SetTile(x, y, 6 * 12 + 11);
          else if (!up && !left) dest.SetTile(x, y, 10 * 12 + 4);
          else if (!up && !right) dest.SetTile(x, y, 10 * 12 + 5);
          else if (!down) dest.SetTile(x, y, WALL);
          else if (!up) dest.SetTile(x, y, 8 * 12 + 2);
          else if (!left) dest.SetTile(x, y, 9 * 12 + 3);
          else if (!right) dest.SetTile(x, y, 9 * 12 + 1);
          else if (!tl) dest.SetTile(x, y, 8 * 12 + 3);
          else if (!tr) dest.SetTile(x, y, 8 * 12 + 1);
          else if (!bl) dest.SetTile(x, y, 6 * 12 + 11);
          else if (!br) dest.SetTile(x, y, 10 * 12 + 1);
          else dest.SetTile(x, y, CASTLE);
        }
        else
        {
          if (up) dest.SetTile(x, y, 6 * 12 + 2);
        }
      }

    // post-process: randomize tile variations
    int[] sandOptions = [90, 73, 73, SAND, SAND, SAND, SAND, SAND, SAND, SAND, SAND, SAND, SAND, SAND];
    int[] castleOptions = [9 * 12, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE, CASTLE];
    for (int y = 0; y < dest.Size.Y; ++y)
      for (int x = 0; x < dest.Size.X; ++x)
      {
        if (dest.GetTile(x, y) == SAND)
          dest.SetTile(x, y, sandOptions[rnd.Next() % sandOptions.Length]);
        if (dest.GetTile(x, y) == CASTLE)
          dest.SetTile(x, y, castleOptions[rnd.Next() % castleOptions.Length]);
      }
  }

  private void ZoomAndPan()
  {
    // process input
    var input = KeyboardState;

    // zoom map from input
    if (input.IsKeyDown(Keys.PageUp))
    {
      //e.Time;
      tileSize *= 1f + ZOOMSPEED * Time.DeltaF;
    }
    if (input.IsKeyDown(Keys.PageDown))
    {
      //e.Time;
      tileSize *= 1f - ZOOMSPEED * Time.DeltaF;
    }
    //_blockLayer.MapScale.X = ClientSize.X / tileSize;
    //_blockLayer.MapScale.Y = ClientSize.Y / tileSize;
    _blockLayer.ScaleToTilesize(tileSize, ClientSize);
    // mapscale: how many tiles fit on screen?

    // pan map from input
    float panStep = Time.DeltaF * PANSPEED * tileSize;
    if (input.IsKeyDown(Keys.Left))
    {
      _blockLayer.MapOffset.X -= panStep;
    }
    if (input.IsKeyDown(Keys.Right))
    {
      _blockLayer.MapOffset.X += panStep;
    }
    if (input.IsKeyDown(Keys.Up))
    {
      _blockLayer.MapOffset.Y += panStep;
    }
    if (input.IsKeyDown(Keys.Down))
    {
      _blockLayer.MapOffset.Y -= panStep;
    }

    float maxX = _blockLayer.Tiles.Size.X - ClientSize.X / tileSize;
    float maxY = _blockLayer.Tiles.Size.Y - ClientSize.Y / tileSize;
    _blockLayer.MapOffset.X = Clamp(_blockLayer.MapOffset.X, 0f, maxX);
    _blockLayer.MapOffset.Y = Clamp(_blockLayer.MapOffset.Y, 0f, maxY);
  }

  private static float Clamp(float x, float min, float max) => x < min ? min : x > max ? max : x;

  private void UpdateText()
  {
    _textWriter.Clear();
    _textWriter.CursorPos = new(1, 1);
    _textWriter.Write($"FPS: {Time.Fps}, tileSize: {tileSize:0.}");
    _textWriter.CursorPos = new(1, 3);
    _textWriter.Write("MAP:  [Arrows] pan   [Home] center   [PgUp/PgDn] zoom");
  }

  static MapRenderLayerSettings MapTileSettings => new()
  {
    TilesetPath = "Resources/tilemap_packed.png",
    TilesetSize = new(12, 11),
    TilemapSize = new(256, 256)
  };

  static MapRenderLayerSettings TextTileSettings => new()
  {
    TilesetPath = "Resources/oldschool_9x16.png",
    TilesetSize = new(32, 8),
    TilemapSize = new(60, 25),
    Align = AlignmentMode.Stretch,
    IsGreyscaleAlpha = true,
    IsText = true
  };
}
