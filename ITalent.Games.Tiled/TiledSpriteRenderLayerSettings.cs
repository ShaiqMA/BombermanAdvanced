using OpenTK.Mathematics;

namespace ITalent.Games.Tiled;

public class TiledSpriteRenderLayerSettings
{
  public string TilesetPath { get; set; } = string.Empty;
  public Vector2i TilesetSize { get; set; } = Vector2i.One;
}
