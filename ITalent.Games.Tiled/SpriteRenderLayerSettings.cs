using OpenTK.Mathematics;

namespace ITalent.Games.Tiled;

public class SpriteRenderLayerSettings
{
  public string TilesetPath { get; set; } = string.Empty;
  public Vector2i TilesetSize { get; set; } = Vector2i.One;
  public AlignmentMode Align { get; set; } = AlignmentMode.None;
  public bool SmoothTextures { get; set; } = false;
}
