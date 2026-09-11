using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ITalent.Games.Tiled;
using TextWriter = ITalent.Games.Tiled.TextWriter;

namespace ITalent.Games.Examples.Demo;

internal class Demo(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private float tileSize = 64f; // width and height of a tile on screen, in pixels
  private const float PANSPEED = 3.5f; // Cursor keys
  private const float ZOOMAMOUNT = 0.9f; // PgUp / PgDn

  private const byte BAT = 0;
  private const byte GHOST = 1;
  private const byte SPIDER = 2;
  private const byte RAT = 4;
  private const byte SLIME = 12;
  private const byte CYCLOPS = 13;
  private const byte CRAB = 14;
  private const byte KNIGHT = 25;
  private const byte WIZARD = 36;

  private MapRenderLayer _blockLayer = new(DemoTileSettings);
  private MapRenderLayer _textLayer = new(TextTileSettings);
  private TextWriter _textWriter;

  protected override void OnLoad()
  {
    _textWriter = new(_textLayer.Tiles);

    AddLayer(_blockLayer);
    AddLayer(_textLayer);

    base.OnLoad();
    _blockLayer.Tiles.RandomMap();
    _blockLayer.AlignCenterContain(ClientSize);
    _textLayer.Tiles.RandomMap();
    _textLayer.Align(ClientSize);
    tileSize = _blockLayer.GetTileSize(ClientSize);

    _textWriter.Clear();
    _textWriter.CursorPos = new(1, 1);
    _textWriter.Write("Hello World!   :)");
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);

    // process input
    var input = KeyboardState;

    // reset map from input
    if (input.IsKeyPressed(Keys.Home))
    {
      _blockLayer.AlignCenterContain(ClientSize);
      tileSize = _blockLayer.GetTileSize(ClientSize);
    }

    // zoom map from input
    if (input.IsKeyDown(Keys.PageUp))
    {
      tileSize *= 1f / ZOOMAMOUNT;
    }
    if (input.IsKeyDown(Keys.PageDown))
    {
      tileSize *= ZOOMAMOUNT;
    }
    _blockLayer.ScaleToTilesize(tileSize, ClientSize);

    // pan map from input
    float zoomStep = PANSPEED / tileSize;
    if (input.IsKeyDown(Keys.Left))
    {
      _blockLayer.MapOffset.X -= zoomStep;
    }
    if (input.IsKeyDown(Keys.Right))
    {
      _blockLayer.MapOffset.X += zoomStep;
    }
    if (input.IsKeyDown(Keys.Up))
    {
      _blockLayer.MapOffset.Y += zoomStep;
    }
    if (input.IsKeyDown(Keys.Down))
    {
      _blockLayer.MapOffset.Y -= zoomStep;
    }

    // random map from input
    var tiles = _blockLayer.Tiles;
    if (input.IsKeyPressed(Keys.R))
    {
      tiles.RandomMap();
    }
    if (input.IsKeyPressed(Keys.D1))
    {
      TestMap1();
    }
    if (input.IsKeyPressed(Keys.D2))
    {
      tiles.Fill(BAT);
      tiles.SetTile(0, 0, GHOST);
    }
    if (input.IsKeyPressed(Keys.D3))
    {
      tiles.Fill(GHOST);
      tiles.SetTile(0, 0, BAT);
      tiles.SetTile(1, 0, SLIME);
      tiles.SetTile(9, 0, SPIDER);
      tiles.SetTile(0, 1, RAT);
      tiles.SetTile(1, 1, WIZARD);
      tiles.SetTile(2, 1, CYCLOPS);
      tiles.SetTile(3, 1, CRAB);
      tiles.SetTile(4, 1, KNIGHT);
    }

    _textWriter.Clear();
    _textWriter.CursorPos = new(1, 1);
    int fps = (int)Math.Round(1.0 / Time.Delta);
    _textWriter.Write($"FPS: {fps}, tileSize: {tileSize:0.}");
    _textWriter.CursorPos = new(1, 3);
    _textWriter.Write("MAP:  [Arrows] pan   [Home] center   [PgUp/PgDn] zoom");
  }

  private void TestMap1()
  {
    // railway:
    // 69 70 71
    // 57 58 59
    // 45 46 47
    var tiles = _blockLayer.Tiles;
    var size = tiles.Size;
    for (int y = 0; y < size.Y; ++y)
      for (int x = 0; x < size.X; ++x)
      {
        // corners
        if (x == 0 && y == 0) tiles.SetTile(x, y, 45);
        else if (x == size.X - 1 && y == 0) tiles.SetTile(x, y, 47);
        else if (x == 0 && y == size.Y - 1) tiles.SetTile(x, y, 69);
        else if (x == size.X - 1 && y == size.Y - 1) tiles.SetTile(x, y, 71);
        // sides
        else if (x == 0) tiles.SetTile(x, y, 57);
        else if (x == size.X - 1) tiles.SetTile(x, y, 59);
        else if (y == 0) tiles.SetTile(x, y, 70);
        else if (y == size.Y - 1) tiles.SetTile(x, y, 46);
        // center
        else tiles.SetTile(x, y, 29); // small centered square
      }
  }

  static MapRenderLayerSettings DemoTileSettings => new()
  {
    TilesetPath = "Resources/tilemap_packed.png",
    TilesetSize = new(12, 11),
    TilemapSize = new(64, 64)
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
