using ITalent.Games.Tiled;
using TextWriter = ITalent.Games.Tiled.TextWriter;

namespace ITalent.Games.TopDownGame;

internal class TextLayer() : MapRenderLayer(Settings)
{
  private TextWriter _textWriter;

  public void Init()
  {
    _textWriter = new(Tiles);
  }

  public void Update(float tileSize)
  {
    UpdateText(tileSize);
  }

  private void UpdateText(float tileSize)
  {
    _textWriter.Clear();
    _textWriter.CursorPos = new(1, 1);
    _textWriter.Write($"FPS: {Time.Fps}, tileSize: {tileSize:0.}");
    _textWriter.CursorPos = new(1, 3);
    _textWriter.Write("  [WASD] move     [PgUp/PgDn] zoom");
    _textWriter.CursorPos = new(1, 4);
    _textWriter.Write("  [Esc] quit      [F11] fullscreen");
  }

  static MapRenderLayerSettings Settings => new()
  {
    TilesetPath = "Resources/oldschool_9x16.png",
    TilesetSize = new(32, 8),
    TilemapSize = new(60, 25),
    Align = AlignmentMode.Stretch,
    IsGreyscaleAlpha = true,
    IsText = true
  };
}
