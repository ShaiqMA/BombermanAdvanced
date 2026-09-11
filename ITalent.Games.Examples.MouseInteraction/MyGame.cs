using ITalent.Games.Tiled;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TextWriter = ITalent.Games.Tiled.TextWriter;

namespace ITalent.Games.Examples.MouseInteraction;

internal class MyGame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private MapRenderLayer _blockLayer = new(MyGameTileSettings);
  private MapRenderLayer _textLayer = new(TextTileSettings);

  private const int RED = 12;
  private const int GREEN = 15;
  private const int BLUE = 9;

  protected override void OnLoad()
  {
    AddLayer(_blockLayer);
    AddLayer(_textLayer);
    base.OnLoad();

    _blockLayer.Tiles.Fill(RED);
    for (int i = 0; i < _blockLayer.Tiles.Size.X && i < _blockLayer.Tiles.Size.Y; ++i)
    {
      _blockLayer.Tiles.SetTile(i, i, GREEN);
    }
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);
    var pos = _blockLayer.PixelToTilePosition(MousePosition, ClientSize);

    TextWriter tw = new(_textLayer.Tiles);
    tw.Clear();
    tw.CursorPos = new(1, 1);
    tw.Write($"Draw! LMB = blue, RMB = red ({pos.X} {pos.Y}) fps = {Time.Fps}");

    var posi = (Vector2i)pos;
    if (posi.X >= 0 && posi.Y >= 0
        && posi.X < _blockLayer.Tiles.Size.X
        && posi.Y < _blockLayer.Tiles.Size.Y)
    {
      if (MouseState.IsButtonDown(MouseButton.Button1))
      {
        _blockLayer.Tiles.SetTile(posi, BLUE);
      }
      else if (MouseState.IsButtonDown(MouseButton.Button2))
      {
        _blockLayer.Tiles.SetTile(posi, RED);
      }
    }
  }

  static MapRenderLayerSettings MyGameTileSettings => new()
  {
    TilesetPath = "Resources/tetris.png",
    TilesetSize = new(4, 4),
    TilemapSize = new(40, 40),
    Align = AlignmentMode.CenterContain
  };

  static MapRenderLayerSettings TextTileSettings => new()
  {
    TilesetPath = "Resources/oldschool_9x16.png",
    TilesetSize = new(32, 8),
    TilemapSize = new(75, 25),
    Align = AlignmentMode.Stretch,
    IsGreyscaleAlpha = true,
    IsText = true
  };
}
