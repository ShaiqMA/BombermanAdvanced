using ITalent.Games.Tiled;
using OpenTK.Core;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ITalent.Games.Examples.MapScaling;

internal class MapScaling(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private MapRenderLayer _blockLayer = new(Settings);

  protected override void OnLoad()
  {
    AddLayer(_blockLayer);
    base.OnLoad();

    drawTestMap();
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);
    var tiles = _blockLayer.Tiles;
    if (KeyboardState.IsKeyPressed(Keys.A))
    {
      Resize(tiles.Size.X - 1, tiles.Size.Y);
    }
    if (KeyboardState.IsKeyPressed(Keys.D))
    {
      Resize(tiles.Size.X + 1, tiles.Size.Y);
    }
    if (KeyboardState.IsKeyPressed(Keys.W))
    {
      Resize(tiles.Size.X, tiles.Size.Y + 1);
    }
    if (KeyboardState.IsKeyPressed(Keys.S))
    {
      Resize(tiles.Size.X, tiles.Size.Y - 1);
    }
  }

  private new void Resize(int newW, int newH)
  {
    _blockLayer.Resize(newW, newH);
    drawTestMap();
    _blockLayer.Align(ClientSize);
  }

  private void drawTestMap()
  {
    var tiles = _blockLayer.Tiles;
    tiles.Fill(12); // rode blokjes
    for (int y = 0; y < tiles.Size.Y; ++y)
      for (int x = 0; x < tiles.Size.X; ++x)
      {
        tiles.SetTile(x, y, x % 4 + 4 * (y % 4));
      }
    for (int i = 0; i < tiles.Size.X && i < tiles.Size.Y; ++i)
    {
      tiles.SetTile(i, i, 15); // groen blokje
    }
    tiles.SetTile(tiles.Size.X - 1, tiles.Size.Y - 1, 9); // blauw blokje
  }

  static MapRenderLayerSettings Settings => new()
  {
    TilesetPath = "Resources/tetris.png",
    TilesetSize = new(4, 4),
    TilemapSize = new(8, 8),
    Align = AlignmentMode.CenterContain
  };
}

