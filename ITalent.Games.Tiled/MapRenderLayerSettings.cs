using OpenTK.Mathematics;

namespace ITalent.Games.Tiled;

public class MapRenderLayerSettings
{
  public string TilesetPath { get; set; } = string.Empty;
  /// <summary>
  /// Width and height of the sprite map used for the tiles/sprites.
  /// </summary>
  /// <remarks>
  /// Width must be a multiple of 4!
  /// </remarks>
  public Vector2i TilesetSize { get; set; } = Vector2i.One;
  /// <summary>
  /// Width and height of (world) map
  /// </summary>
  public Vector2i TilemapSize { get; set; } = Vector2i.One;
  public AlignmentMode Align { get; set; } = AlignmentMode.None;
  /// <summary>
  /// Expects an 8-bit greyscale texture where black (0) is fully transparent en white (255) is fully opaque
  /// </summary>
  public bool IsGreyscaleAlpha { get; set; } = false;
  public bool SmoothTextures { get; set; } = false;
  /// <summary>
  /// Renders a black dropshadow
  /// </summary>
  public bool IsText { get; set; } = false;
}
